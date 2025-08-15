using System.Text.Json;
using System.Text.Json.Serialization;

namespace Ruquier;

public static class PodcastsSerializer
{
  private static string FilePath => Path.Combine(FileSystem.AppDataDirectory, "podcasts.json");

  // Objet simplifié pour la sérialisation
  private class PodcastDto
  {
    public string FilePath { get; set; }
    public double Progress { get; set; }
  }

  // Sauvegarde en JSON
  public static void Save(List<Podcast> podcasts)
  {
    var dtoList = podcasts.Select(p => new PodcastDto
    {
      FilePath = p.FilePath,
      Progress = p.Progress
    }).ToList();

    var options = new JsonSerializerOptions
    {
      WriteIndented = true,
    };

    string json = JsonSerializer.Serialize(dtoList, options);
    File.WriteAllText(FilePath, json);
  }

  // Lecture depuis JSON
  public static List<Podcast> Load()
  {
    if (!File.Exists(FilePath))
      return new List<Podcast>();

    string json = File.ReadAllText(FilePath);

    var dtoList = JsonSerializer.Deserialize<List<PodcastDto>>(json);

    if (dtoList == null)
      return new List<Podcast>();

    return dtoList.Select(dto =>
    {
      var p = new Podcast(dto.FilePath)
      {
        Progress = dto.Progress
      };
      return p;
    }).ToList();
  }

}
