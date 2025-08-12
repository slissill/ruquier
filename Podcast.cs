using System.ComponentModel;
using System.Globalization;
using System.Runtime.CompilerServices;
namespace Ruquier;

public class Podcast : INotifyPropertyChanged
{
  public string FilePath { get; }
  public DateTime Date { get; }

  public int Annee => Date.Year;
  public int Mois => Date.Month;
  public int Jour => Date.Day;
  public string Jour00 => Date.Day.ToString("00");
  public string MoisString => Date.ToString("MMMM", new CultureInfo("fr-FR"));
  public string JourSemaine => Date.ToString("ddd", new CultureInfo("fr-FR")).Replace(".", "");
  public override string ToString()
  {
    return Date.ToString ("ddd dd MMM", new CultureInfo("fr-FR"));
  }

  public Podcast(string file)
  {
    FilePath = file;
    Date = Utils.ExtractDateFromFile(file);
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
