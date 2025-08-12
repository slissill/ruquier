using System.Globalization;
using Android.Media.Metrics;
namespace Ruquier;

public class Podcast
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
    //return $"{Jour00}  {JourSemaine}";
    return Date.ToString ("ddd dd MMM", new CultureInfo("fr-FR"));
  }

  public Podcast(string file)
  {
    FilePath = file;
    Date = Utils.ExtractDateFromFile(file);
  }

}
