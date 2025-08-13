using System.Text.Json;
using Microsoft.Maui.Storage;

namespace Ruquier;

public class Preferences
{
  public string? Root { get; set; }
  public string? LastPodcast { get; set; }
  public int Speed { get; set; }

  public int LastPodcastAnnee => LastPodcast == null? 0: Utils.ExtractDateFromFile(LastPodcast).Year;
  public int LastPodcastMois => LastPodcast == null ? 0 : Utils.ExtractDateFromFile(LastPodcast).Month;
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
