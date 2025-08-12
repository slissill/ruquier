using System.Globalization;
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
  public string JourSemaine => Date.ToString("dddd", new CultureInfo("fr-FR"));
  public override string ToString()
  {
    return $"{Jour00}  {JourSemaine}";
  }

  public Podcast(string file)
  {
    FilePath = file;
    Date = Utils.ExtractDateFromFile(file);
  }

}
