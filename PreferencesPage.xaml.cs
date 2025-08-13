using Plugin.Maui.Audio;

namespace Ruquier;

public partial class PreferencesPage : ContentPage
{
  private PreferencesManager _prefsManager;

  public PreferencesPage(PreferencesManager prefsManager)
	{
		InitializeComponent();
    _prefsManager = prefsManager;
  }

  private async void OnValidateClicked(object sender, EventArgs e)
  {
    string path = RootEntry.Text?.Trim() ?? "";
    if (!Directory.Exists(path))
    {
      await DisplayAlert("Erreur", "Le chemin n'est pas valide.", "OK");
      return;
    }

    var prefs = _prefsManager.Load();
    prefs.Root = path;
    _prefsManager.Save(prefs);

    // Retour à la page principale
    Application.Current.MainPage = new MainPage(new AudioManager());
  }

  private void optRoot_CheckedChanged(object sender, CheckedChangedEventArgs e)
  {
    if (sender == optEmulateur && e.Value)
      RootEntry.Text = "/storage/0000-0000/E1";
    else if (sender == optRedmi && e.Value)
      RootEntry.Text = "/storage/0141-3140/PODCASTS/E1";
    else if (sender == optPixel && e.Value)
      RootEntry.Text = "/storage/????-????/PODCASTS/E1";

  }
}