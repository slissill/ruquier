using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Plugin.Maui.Audio;

namespace Ruquier;

public partial class MainPage : ContentPage, INotifyPropertyChanged
{

  private bool _loading = false;
  private readonly PreferencesManager _prefsManager = new PreferencesManager();
  public Preferences Prefs { get; private set; }
  private string _root = "/storage/0000-0000/E1";

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

  public MainPage(IAudioManager audioManager)
  {

    InitializeComponent();
    _audioManager = audioManager;
    this.SetTimer();    
    Permissions.RequestStoragePermission();

    _allPodcasts = PodcastService.LoadPodcasts(_root).ToList();

    // Liste années
    List<int> annees = _allPodcasts.Select(p => p.Annee).Distinct().OrderBy(y => y).ToList();
    annees.ForEach(y => Annees.Add(y));

    
    Prefs = _prefsManager.Load();

    _loading = true;  
    if (Prefs.LastPodcast != null)
    {
      AnneeSelectionnee = Prefs.LastPodcastAnnee;
      UpdateMois();
      MoisSelectionnee = Mois.First(m => m.NumMois == Prefs.LastPodcastMois);
    }
    else
    {
      AnneeSelectionnee = Annees.First();
      UpdateMois();
      MoisSelectionnee = Mois.First();
      
    }
    _loading = false;

    
    UpdateListePodcasts();

    BindingContext = this;
    if (Prefs.LastPodcast != null)
    {
      Podcast? lastPodcast = Podcasts.FirstOrDefault(p => p.FilePath == Prefs.LastPodcast);
      lstPodcasts.SelectedItem = lastPodcast;
      PlayPause();
      lstPodcasts.ScrollTo(lastPodcast, position: ScrollToPosition.Center, animate: false);
    }

  }

  protected override void OnAppearing()
  {
    base.OnAppearing();
    SeekSlider.DragStarted += (s, e) => _isDraggingSlider = true;
    SeekSlider.DragCompleted += (s, e) =>
    {
      _isDraggingSlider = false;
      _player?.Seek(SeekSlider.Value);
    };
  }

  private void SetTimer()
  {
    // Mise à jour de la position toutes les 500 ms
    Dispatcher.StartTimer(TimeSpan.FromMilliseconds(500), () =>
    {
      if (_player != null && _player.IsPlaying && !_isDraggingSlider)
      {
        SeekSlider.Maximum = _player.Duration;
        SeekSlider.Value = _player.CurrentPosition;
        UpdatePositionLabel();
      }
      return true; // continue le timer
    });


  }

  private void UpdateListePodcasts()
  {
    if(_loading) return;  

    Podcasts.Clear();
    List<Podcast> lst = _allPodcasts.Where(p => p.Annee == AnneeSelectionnee && p.Mois == MoisSelectionnee?.NumMois).OrderBy(p => p.Jour).ToList();
    lst.ForEach(p => Podcasts.Add(p));
  }

  private bool _IgnoreMoisChange = false; 
  private void cboAnnee_SelectedIndexChanged(object sender, EventArgs e)
  {
    _IgnoreMoisChange = true;
    AnneeSelectionnee = (int)cboAnnees.SelectedItem;
    UpdateMois();
    MoisSelectionnee = Mois.First();
    _IgnoreMoisChange = false;
    UpdateListePodcasts();
  }

  private void cboMois_SelectedIndexChanged(object sender, EventArgs e)
  {
    if (_IgnoreMoisChange) return;

    MoisSelectionnee = (Mois)cboMois.SelectedItem;
    UpdateListePodcasts();
  }

  private void UpdateMois()
  {
    Mois.Clear();
    List<int> numMois = _allPodcasts.Where(p=>p.Annee == AnneeSelectionnee).Select(p => p.Mois).Distinct().OrderBy(m => m).ToList();    
    numMois.ForEach(n => Mois.Add(new Mois(n)));
    
  }

  private async void CollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
  {

    string? path = null;
    try
    {
      if (e.CurrentSelection == null || e.CurrentSelection.Count == 0)
        return;

      Podcast podcast = (Podcast)e.CurrentSelection[0];
      _LastPodcastPath = podcast.FilePath;
      path = podcast.FilePath;
      Console.WriteLine($"Lecture podcast : {path}");

      // Stop ancien player si existant
      _player?.Stop();
      _player?.Dispose();
      _player = null;

      // Ouvre le fichier depuis la SD
      using var fileStream = File.OpenRead(path);
      _player = _audioManager.CreatePlayer(fileStream);

      // Lance lecture
      _player.Play();
      UpdatePlayPauseButtonText();
    }
    catch (Exception ex)
    {
      await ErrorManager.ShowError(this, ex, info: path);
    }
  }

  private void OnPlayPauseClicked(object sender, EventArgs e)
  {
    PlayPause();
  }

  private void PlayPause()
  {
    if (_player == null)
      return;

    if (_player.IsPlaying)
    {
      _player.Pause();
    }
    else
    {
      _player.Play();
    }

    UpdatePlayPauseButtonText();
  }

  private void UpdatePlayPauseButtonText()
  {
    if (_player == null)
    {
      PlayPauseButton.Text = "Play";
    }
    else
    {
      PlayPauseButton.Text = _player.IsPlaying ? "Pause" : "Play";
    }
  }
  
  private void OnSeekSliderChanged(object sender, ValueChangedEventArgs e)
  {
    if (_player == null) return;

    if (_isDraggingSlider)
    {
      _player.Seek(e.NewValue);
      UpdatePositionLabel();
    }
  }

  private void OnMove(object sender, EventArgs e)
  {
    if (_player == null) return;

    double offset = 0;

    if (sender == MinusLarge) offset = -120;
    else if (sender == MinusSmall) offset = -10;
    else if (sender == PlusSmall) offset = 10;
    else if (sender == PlusLarge) offset = 120;

    // Nouvelle position
    double newPos = _player.CurrentPosition + offset;

    // Clamp entre 0 et la durée
    if (newPos < 0) newPos = 0;
    if (newPos > _player.Duration) newPos = _player.Duration;

    _player.Seek(newPos);
    SeekSlider.Value = newPos;
    UpdatePositionLabel();    
  }

  private void UpdatePositionLabel()
  {
    if (_player == null) return;  
    lblPosition.Text = Utils.FormatTime(_player.CurrentPosition, _player.Duration);
    lblDuration.Text = Utils.FormatTime(_player.Duration);    
  }
  

  protected override void OnDisappearing()
  {
    base.OnDisappearing();
    PreferencesSave();
  }

  private void PreferencesSave()
  {   
    //if (lstPodcasts.SelectedItem is Podcast podcast)
    Prefs.LastPodcast = _LastPodcastPath;
    _prefsManager.Save(Prefs);
  }

}





