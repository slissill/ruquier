using System.Text.RegularExpressions;

namespace Ruquier;

public static partial class PodcastService
{
  [GeneratedRegex(@"\d{4}-\d{2}-\d{2}\.mp3$", RegexOptions.IgnoreCase)]
  private static partial Regex GetRegex();

  public static IEnumerable<Podcast> LoadPodcasts(string folderPath)
  {
    var regex = GetRegex();

    if (!Directory.Exists(folderPath))
      yield break;

    foreach (var file in Directory.GetFiles(folderPath, "*.mp3", SearchOption.AllDirectories))
    {
      if (regex.IsMatch(Path.GetFileName(file)))
        yield return new Podcast(file);
    }
  }
}
