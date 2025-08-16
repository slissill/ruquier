using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;

namespace Ruquier;

public enum enmPlayingStatus
{
  None,
  Playing,
  Paused
} 

public class CalendarDay : INotifyPropertyChanged
{
  public DateTime Date { get; set; }
  public int Day => Date.Day;  
  public bool IsInMonth { get; set; }
  public bool HasFile => !string.IsNullOrEmpty(FilePath);
  public string? FilePath { get; set; }
  public string Title => Date.ToString("ddd dd MMM yyyy", new CultureInfo("fr-FR")).Replace(".", "");
  public string PlayingStatusToString
  { get
    {
      return PlayingStatus switch
      {        
        enmPlayingStatus.Playing => "\ue034",  
        enmPlayingStatus.Paused => "\ue037",
        _ => ""
      };
    }
  } 
  public override string ToString()
  {
    return $"{Date.ToString("yyyy-MM-dd")}   IsInMonth:{IsInMonth}   HasPodcast:{HasFile}";
  }

  private enmPlayingStatus _playingStatus = enmPlayingStatus.None;  
  public enmPlayingStatus PlayingStatus
  {
    get => _playingStatus;
    set
    {
      if (_playingStatus != value)
      {
        _playingStatus = value;
        OnPropertyChanged();
        OnPropertyChanged(nameof(PlayingStatusToString));
      }
    }
  } 

  private double _progress;
  public double Progress
  {
    get => _progress;
    set
    {
      if (_progress != value)
      {
        _progress = value;
        OnPropertyChanged(); // déclenche l’événement
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
