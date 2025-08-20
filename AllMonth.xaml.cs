using System.Collections.ObjectModel;
using System.Globalization;

namespace Ruquier;

public partial class AllMonth : ContentPage
{
  public ObservableCollection<GroupeJours> Jours { get; set; }

  public AllMonth()
  {
    InitializeComponent();
    Init();
    BindingContext = this;
  }

  private void Init()
  {
    var periodeDebut = "2012-01";
    var periodeFin = "2014-12";
    Jours = PeriodeHelper.GenererPeriode(periodeDebut, periodeFin);
  }
}

public class Jour
{ public DateTime Date { get; set; }  
  public string Periode { get; set; }
  public int Day => Date.Day;
}

public class GroupeJours : ObservableCollection<Jour>
{
  public string Periode { get; set; }
  public GroupeJours(string periode, IEnumerable<Jour> jours) : base(jours)
  {
    Periode = periode;
  }
}

public static class PeriodeHelper
{
  public static ObservableCollection<GroupeJours> GenererPeriode(string periodeDebut, string periodeFin)
  {
    var jours = new List<Jour>();

    // Parsing des périodes
    DateTime debut = DateTime.ParseExact(periodeDebut + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture);
    DateTime fin = DateTime.ParseExact(periodeFin + "-01", "yyyy-MM-dd", CultureInfo.InvariantCulture)
                    .AddMonths(1).AddDays(-1);

    // Ajuster la date de début pour remonter au lundi de la semaine contenant le 1er
    int delta = (int)debut.DayOfWeek - (int)DayOfWeek.Monday;
    if (delta < 0) delta += 7;
    DateTime start = debut.AddDays(-delta);

    // Générer les dates
    for (DateTime d = start; d <= fin; d = d.AddDays(1))
    {

      // Exclure samedi et dimanche
      if (d.DayOfWeek == DayOfWeek.Saturday || d.DayOfWeek == DayOfWeek.Sunday)
        continue;

      string periode = d.ToString("yyyy-MM");
      if (d < debut) periode = periodeDebut;
      if (periode.CompareTo(periodeFin) > 0) break;

      jours.Add(new Jour
      {
        Date = d,
        Periode = periode
      });
    }

    // Groupement
    var groupes = jours
        .GroupBy(j => j.Periode)
        .Select(g => new GroupeJours(g.Key, g))
        .ToList();

    return new ObservableCollection<GroupeJours>(groupes);
  }
}