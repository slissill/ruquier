using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Ruquier;

public static class ErrorManager
{

  public static async Task ShowError(Page page, Exception ex, string? info = null)
  {
    Console.WriteLine($"ZTL : {info}  {ex}");
    await page.DisplayAlert("Erreur", ex.Message, "OK");
  }
}
