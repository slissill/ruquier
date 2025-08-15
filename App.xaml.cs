using Plugin.Maui.Audio;

namespace Ruquier
{
  public partial class App : Application
  {

    //private readonly PreferencesManager _prefsManager = new();

    public App()
    {
      InitializeComponent();
    }

    //protected override Window CreateWindow(IActivationState? activationState)
    //{
    //  return new Window(new AppShell());
    //}

    protected override Window CreateWindow(IActivationState? activationState)
    {

      var _prefsManager = new PreferencesManager();

      //var prefsReset = _prefsManager.Load();
      //prefsReset.Root = null;
      //_prefsManager.Save(prefsReset);

      var prefs = _prefsManager.Load();
      
      Page startPage;
      if (string.IsNullOrEmpty(prefs.Root) || !Directory.Exists(prefs.Root))
        startPage = new PreferencesPage();
      else
        startPage = new MainPage(new AudioManager());
      return new Window(startPage);
    }
  }
}
