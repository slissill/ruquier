using System.Globalization;

namespace Ruquier;

public class Mois
{
  public int NumMois { get; set; }
  public string MoisStr => new DateTime(2000, NumMois, 1).ToString("MMM", new CultureInfo("fr-FR")).ToUpper();

  public Mois(int numMois)
  {
    NumMois = numMois;
  }
}
