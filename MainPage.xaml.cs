using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Maui.Controls.Shapes;
using Plugin.Maui.Audio;
using System.Linq;

namespace Ruquier;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{
  #region declarations  

  public ObservableCollection<CalendarDay> Dates { get; set; } = new();
  public ObservableCollection<DurationOption> DurationOptions { get; } = new ObservableCollection<DurationOption>(DurationOption.GetOptions());

  private readonly PreferencesManager _prefsManager = new();
  public Preferences Prefs { get; set; }
  //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
  private readonly IAudioManager _audioManager;
  private IAudioPlayer? _player = null;
  private bool _isDraggingSlider;
  //XXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXXX
  private List<Podcast> _allPodcasts;
  public ObservableCollection<Podcast> Podcasts { get; set; } = new();
  public ObservableCollection<int> Annees { get; set; } = new();
  public ObservableCollection<Mois> Mois { get; set; } = new();
  private string? _LastPodcastPath;
  private bool _IgnoreMoisChange = false;


  public DurationOption SelectedDuration
  {
    get => DurationOptions.FirstOrDefault(x => x.Value == Prefs.TimeBeforeAutoPause);
    set
    {
      if (value != null && Prefs.TimeBeforeAutoPause != value.Value)
      {
        Prefs.TimeBeforeAutoPause = value.Value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(SelectedDuration)); // Pour rafraîchir l'affichage
      }
    }
  }


  private CalendarDay _currentPodcast;
  public CalendarDay CurrentPodcast
  {
    get => _currentPodcast;
    set
    {
      if (_currentPodcast != value)
      {
        _currentPodcast = value;

        // Quand on change le "courant", on met à jour les statuts des autres
        Dates.ToList().ForEach(d => d.PlayingStatus = enmPlayingStatus.None); 
        _currentPodcast.PlayingStatus = enmPlayingStatus.Paused; 
        OnPropertyChanged(nameof(CurrentPodcast));
        PlayerSetPodcast();
      }
    }
  }


  private int _anneeSelectionnee;
  public int AnneeSelectionnee
  {
    get => _anneeSelectionnee;
    set
    {
      if (_anneeSelectionnee != value)
      {
        _anneeSelectionnee = value;
        OnPropertyChanged();
      }
    }
  }

  private Mois? _moisSelectionnee;
  public Mois? MoisSelectionnee
  {
    get => _moisSelectionnee;
    set
    {
      if (_moisSelectionnee != value)
      {
        _moisSelectionnee = value;
        OnPropertyChanged();
      }
    }
  }
  #endregion

  #region constructor  
  public MainPage(IAudioManager audioManager)
  {
    InitializeComponent();
    _audioManager = audioManager;
    _allPodcasts = new List<Podcast>();
    Permissions.RequestStoragePermission();
    Init();
  }

  private void Init()
  {

    Prefs = _prefsManager.Load();

    SetTimer();
    InitSeekSliderEvtDrag();
    
    
    _allPodcasts = PodcastsSerializer.Load();

    // Remplir les pickers
    Annees.Clear();
    foreach (var y in _allPodcasts.Select(p => p.Annee).Distinct().OrderBy(y => y))
      Annees.Add(y);

    if (Prefs.LastPodcast != null && File.Exists(Prefs.LastPodcast))
    {
      AnneeSelectionnee = Prefs.LastPodcastAnnee;
      AlimPickerMois();
      MoisSelectionnee = Mois.First(m => m.NumMois == Prefs.LastPodcastMois);
    }
    else
    {
      AnneeSelectionnee = Annees.First();
      AlimPickerMois();
      MoisSelectionnee = Mois.First();
    }
    
    UpdateCalendarPodcasts();

    BindingContext = this;


    // On sélectionne le current podcast
    CurrentPodcast = Dates.FirstOrDefault(d => d.IsInMonth && d.HasFile);
    CurrentPodcast.PlayingStatus = enmPlayingStatus.Paused;




  }

  private void InitSeekSliderEvtDrag()
  {
    SeekSlider.DragStarted += (s, e) => _isDraggingSlider = true;
    SeekSlider.DragCompleted += (s, e) =>
    {
      _isDraggingSlider = false;
      _player?.Seek(SeekSlider.Value);
    };
  }
  #endregion

  #region Evennements  
  private void pickerAnnee_SelectedIndexChanged(object sender, EventArgs e)
  {
    _IgnoreMoisChange = true;
    AnneeSelectionnee = (int)pickerAnnees.SelectedItem;
    AlimPickerMois();
    MoisSelectionnee = Mois.First();
    _IgnoreMoisChange = false;
    UpdateCalendarPodcasts();
  }

  private void pickerMois_SelectedIndexChanged(object sender, EventArgs e)
  {
    if (_IgnoreMoisChange) return;

    MoisSelectionnee = (Mois)pickerMois.SelectedItem;
    UpdateCalendarPodcasts();
  }

  private void OnPlayPauseClicked(object sender, EventArgs e)
  {
    PlayPause();
  }

  private void OnSeekSliderChanged(object sender, ValueChangedEventArgs e)
  {
    if (_player == null) return;

    if (_isDraggingSlider)
    {
      _player.Seek(e.NewValue);
      UpdatePlayerPosition();
    }
  }

  private void OnMove(object sender, EventArgs e)
  {
    if (_player == null) return;

    double offset = 0;
    int valueShort = Prefs.MoveShort;
    int valueLarge= Prefs.MoveLarge;  

    if (sender == MinusLarge) offset = -valueLarge;
    else if (sender == MinusSmall) offset = -valueShort;
    else if (sender == PlusSmall) offset = valueShort;
    else if (sender == PlusLarge) offset = valueLarge;

    // Nouvelle position
    double newPos = _player.CurrentPosition + offset;

    // Clamp entre 0 et la durée
    if (newPos < 0) newPos = 0;
    if (newPos > _player.Duration) newPos = _player.Duration;

    _player.Seek(newPos);
    SeekSlider.Value = newPos;
    UpdatePlayerPosition();
  }

  protected override void OnDisappearing()
  {
    SavePodcasts();
    PreferencesSave();
    base.OnDisappearing();
  }
  #endregion

  #region Methodes  
  private void SetTimer()
  {
    // Mise à jour de la position toutes les 500 ms
    Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
    {
      Timer_Tick();
      return true; // continue le timer
    });

  }

  private void Timer_Tick()
  {

    // Gestion de l'heure
    lblHeure.Text = DateTime.Now.ToString("HHhmm");


    // Gestion du slider
    if (_player != null && _player.IsPlaying && !_isDraggingSlider)
    {
      SeekSlider.Maximum = _player.Duration;
      SeekSlider.Value = _player.CurrentPosition;
      UpdatePlayerPosition();


      //if (lstPodcasts.SelectedItem is Podcast podcast)
      //  podcast.Progress = _player.CurrentPosition / _player.Duration;

    }

  } 

  public static int GetFrenchDayOfWeek(DateTime date)
  {
    int day = (int)date.DayOfWeek;
    return day == 0 ? 7 : day;
  }

  private void UpdateCalendarPodcasts()
  {

    Dates.Clear();

    int annee = AnneeSelectionnee;  
    int mois = MoisSelectionnee?.NumMois ?? 1;  

    var lst = _allPodcasts.Where(p => p.Annee == annee && p.Mois == mois);

    // Premier jour du mois demandé
    DateTime firstOfMonth = new DateTime(annee, mois, 1);

    // Décalage pour trouver le lundi de la première semaine
    int offset = (int)firstOfMonth.DayOfWeek;
    if (offset == 0) offset = 7; // Dimanche = 7
    DateTime firstMonday = firstOfMonth.AddDays(-(offset - 1));

    // Dernier jour du mois demandé
    DateTime endOfMonth = firstOfMonth.AddMonths(1).AddDays(-1);

    // Aller jusqu’au dimanche de la dernière semaine
    int endOffset = (int)endOfMonth.DayOfWeek;
    if (endOffset == 0) endOffset = 7;
    DateTime lastSunday = endOfMonth.AddDays(7 - endOffset);

    // Construire la liste en ignorant les samedis et dimanches
    DateTime current = firstMonday;
    while (current <= lastSunday)
    {
      if (current.DayOfWeek != DayOfWeek.Saturday && current.DayOfWeek != DayOfWeek.Sunday)
      {
        //dates.Add(current);
        CalendarDay calDay = new CalendarDay { Date = current };
        calDay.IsInMonth = current.Month == mois;  
        var podcast = lst.FirstOrDefault(p => p.Date == current); 
        if(podcast != null)
        {
          calDay.FilePath = podcast.FilePath;
        }
        else
        {
          calDay.FilePath = null;
        } 
        Dates.Add(calDay);
      }
      current = current.AddDays(1);
    }

  }

  private void AlimPickerMois()
  {
    Mois.Clear();
    foreach (var n in _allPodcasts
                        .Where(p => p.Annee == AnneeSelectionnee)
                        .Select(p => p.Mois)
                        .Distinct()
                        .OrderBy(m => m))
    {
      Mois.Add(new Mois(n));
    }

  }

  private void PlayPause()
  {
    if (_player == null)
      return;

    if(CurrentPodcast.PlayingStatus == enmPlayingStatus.Playing)
    {
      CurrentPodcast.PlayingStatus = enmPlayingStatus.Paused;
      _player.Pause();
    }
      
    else
    {
      CurrentPodcast.PlayingStatus = enmPlayingStatus.Playing;
      _player.Play();
    }
    
  }

  private void UpdatePlayerPosition()
  {
    if (_player == null) return;  
    lblPosition.Text = Utils.FormatTime(_player.CurrentPosition, _player.Duration);
    lblDuration.Text = Utils.FormatTime(_player.Duration);

    //if (_player.CurrentPosition >= _player.Duration -1 && Prefs.AutoPlayNext)    
    //  PlayNext();      
    
  }

  private void PlayNext()
  {
    // dans ma collectionView 
    // si il y a un element de selectionner : 
    //    si il y a un element apres on le selectionne
    //    sinon on selectionne le 1er element
    // sinon
    //    on selectionne le 1er element
    if (lstPodcasts.SelectedItem is Podcast currentPodcast)
    {
      int index = Podcasts.IndexOf(currentPodcast);
      if (index < Podcasts.Count - 1)
      {
        lstPodcasts.SelectedItem = Podcasts[index + 1];
      }
      else
      {
        lstPodcasts.SelectedItem = Podcasts.FirstOrDefault();
      }
    }
    else
    {
      lstPodcasts.SelectedItem = Podcasts.FirstOrDefault();
    } 


  }

  private void PreferencesSave()
  { 
    Prefs.LastPodcast = _LastPodcastPath;
    _prefsManager.Save(Prefs);
  }

  private void SavePodcasts()
  {
    return;
    PodcastsSerializer.Save(_allPodcasts);
  }

  #endregion

  private void MsgBox(string message)
  {
      DisplayAlert("Title", message, "OK");
  }

  private void Button_Clicked(object sender, EventArgs e)
  {
    SavePodcasts(); 
  }

  private void OnPodcastTapped(object sender, TappedEventArgs e)
  {


    if (sender is Border border && border.BindingContext is CalendarDay podcast && podcast.HasFile)
    {

      //if (podcast.PlayingStatus == enmPlayingStatus.Playing)
      //  podcast.PlayingStatus = enmPlayingStatus.Paused;
      //else
      //  podcast.PlayingStatus = enmPlayingStatus.Playing;

      if (podcast == CurrentPodcast)
      {
        PlayPause();
        
      }
      else
      {
        CurrentPodcast = podcast;
        PlayPause();
      }
    }
  }




  //private CalendarDay CurrentPodcast()
  //{
  //  return Dates.FirstOrDefault(d => d.PlayingStatus != enmPlayingStatus.None);
  //}

  private void PlayerSetPodcast()
  {
    CalendarDay podcast = CurrentPodcast;
    string? path = podcast.FilePath;
    try
    {      

      _player?.Stop();
      _player?.Dispose();
      _player = null;

      using var fileStream = File.OpenRead(path);
      _player = _audioManager.CreatePlayer(fileStream);      
      ApplySpeed();
      _LastPodcastPath = path;

    }
    catch (Exception ex)
    {
      ErrorManager.ShowError(this, ex, info: path);
    }
  }

  private void ApplySpeed()
  {
    if (_player == null) return;    
    double speed = (double)Prefs.Speed;
    _player.Speed = speed / 100.0; // Convertir en pourcentage    
  } 


  private void PlayPodcast(CalendarDay calendarDay)
  {

    string? path = calendarDay.FilePath; 
    try
    {
      
      _LastPodcastPath = path;      
      Console.WriteLine($"Lecture podcast : {path}");

      // Stop ancien player si existant
      _player?.Stop();
      _player?.Dispose();
      _player = null;

      // Ouvre le fichier depuis le storage
      using var fileStream = File.OpenRead(path);
      _player = _audioManager.CreatePlayer(fileStream);

      //// Lance lecture
      //if (podcast.Progress > 0)
      //{
      //  _player.Seek(_player.Duration * podcast.Progress);
      //}
      _player.Play();
    }
    catch (Exception ex)
    {
      ErrorManager.ShowError(this, ex, info: path);
    }
  }


  public class DurationOption
  {
    public int Value { get; set; }      // la valeur numérique
    public string Display { get; set; } // ce qui s'affiche dans le Picker

    public static List<DurationOption> GetOptions()
    {
      return new List<DurationOption>
      {
        new DurationOption { Value = 0, Display = "jamais" },
        new DurationOption { Value = 10, Display = "10s" },
        new DurationOption { Value = 1800, Display = "30mn" },
        new DurationOption { Value = 3600, Display = "1h" },
        new DurationOption { Value = 5400, Display = "1h30" },
        new DurationOption { Value = 7200, Display = "2h" },
        new DurationOption { Value = 10800, Display = "3h" },
        new DurationOption { Value = 14400, Display = "4h" }
      };
    } 
  }

  private void OnPlayingTapped(object sender, TappedEventArgs e)
  {
    PlayPause();  
  }

  private void OnSpeed(object sender, EventArgs e)
  {
    int pas = 1;
    if(sender == btnSpeedMinus)
      Prefs.Speed = Math.Max(50, Prefs.Speed - pas); // Limite à 50%
    else if (sender == btnSpeedPlus)
      Prefs.Speed = Math.Min(150, Prefs.Speed + pas); // Limite à 200% 
    else
      Prefs.Speed = 100; // Reset à 100%  

    ApplySpeed(); 
  }

  private void pickerDuration_SelectedIndexChanged(object sender, EventArgs e)
  {

  }
}

