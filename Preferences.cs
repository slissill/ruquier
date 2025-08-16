using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Ruquier;

public class Preferences : INotifyPropertyChanged
{
  public string? Root { get; set; }
  public string? LastPodcast { get; set; }
  public bool AutoPlayNext { get; set; } = false;
  
  public int MoveShort { get; set; } = 10; // secondes  
  public int MoveLarge { get; set; } = 120; // secondes  

  public string MoveShortToString => $"{MoveShort} s";
  public string MoveLargeToString => $"{MoveLarge / 60} m";

  public int LastPodcastAnnee => LastPodcast == null? 0: Utils.ExtractDateFromFile(LastPodcast).Year;
  public int LastPodcastMois => LastPodcast == null ? 0 : Utils.ExtractDateFromFile(LastPodcast).Month;


  private int _speed = 100;
  public int Speed
  {
    get => _speed;
    set
    {
      if (_speed != value)
      {
        _speed = value;
        OnPropertyChanged(); // déclenche l’événement
      }
    }
  }


  private int _timeBeforeAutoPause = 0;

  public int TimeBeforeAutoPause
  {
    get => _timeBeforeAutoPause;
    set
    {
      if (_timeBeforeAutoPause != value)
      {
        _timeBeforeAutoPause = value;
        OnPropertyChanged();
      }
    }
  }


  // === INotifyPropertyChanged ===
  public event PropertyChangedEventHandler PropertyChanged;
  protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
  {
    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
  }

}

public class PreferencesManager
{
  private readonly string _path;

  public PreferencesManager()
  {
    _path = Path.Combine(FileSystem.AppDataDirectory, "preferences.json");
  }

  public void Save(Preferences prefs)
  {
    var json = JsonSerializer.Serialize(prefs);
    File.WriteAllText(_path, json);
  }

  public Preferences Load()
  {
    if (!File.Exists(_path))
      return new Preferences(); // valeurs par défaut

    var json = File.ReadAllText(_path);    
    return JsonSerializer.Deserialize<Preferences>(json) ?? new Preferences();

  }

}
