using Plugin.Maui.Audio;

namespace Ruquier
{
  public partial class App : Application
  {

    private readonly PreferencesManager _prefsManager = new();

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

      var prefs = _prefsManager.Load();
      prefs.Root = null;
      _prefsManager.Save(prefs);

      prefs = _prefsManager.Load();
      Page startPage;

      if (string.IsNullOrEmpty(prefs.Root) || !Directory.Exists(prefs.Root))
        startPage = new PreferencesPage(_prefsManager);
      else
        startPage = new MainPage(new AudioManager());

      return new Window(startPage);
    }
  }
}
