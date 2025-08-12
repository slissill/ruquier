using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ruquier;

public static class Utils
{
  public static DateTime ExtractDateFromFile(string fullPath)
  {
    return DateTime.ParseExact(
    Path.GetFileNameWithoutExtension(fullPath).Substring(4).Trim(),
    "yyyy-MM-dd",
    CultureInfo.InvariantCulture);
  }

  public static string FormatTime(double seconds, double duration = 0)
  {
    double max = duration == 0 ? seconds : duration;
    var ts = TimeSpan.FromSeconds(seconds);
    if (TimeSpan.FromSeconds(max).TotalHours < 1)
    {
      // minutes:seconds
      return $"{ts.Minutes:D1}:{ts.Seconds:D2}";
    }
    else
    {
      // heures:minutes:secondes
      return $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}:{ts.Seconds:D2}";
    }
  }


}
