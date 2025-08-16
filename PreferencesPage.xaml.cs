using Plugin.Maui.Audio;

namespace Ruquier;

public partial class PreferencesPage : ContentPage
{

  Preferences prefV1;

  public PreferencesPage()
	{
		InitializeComponent();
  }

  private void OnValidateClicked(object sender, EventArgs e)
  {
    string path = RootEntry.Text?.Trim() ?? "";
    if (!Directory.Exists(path))
    {
      DisplayAlert("Erreur", "Le chemin n'est pas valide.", "OK");
      return;
    }

    bool ResetPodcastsJson = false; 
    
    var prefsManager = new PreferencesManager();
    var prefs = prefsManager.Load();
    string rootBefore = prefs.Root ?? ""; 

    prefs.Root = path;
    prefsManager.Save(prefs);

    if (rootBefore != prefs.Root)
    {
      SetPodcastsJson(prefs);
    }
    Application.Current.MainPage = new MainPage(new AudioManager());
  }

  private async void SetPodcastsJson(Preferences pref)
  {

    List<Podcast> allPodcasts = PodcastService.LoadPodcasts(pref.Root).ToList();
    PodcastsSerializer.Save(allPodcasts);
  }

  private void optRoot_CheckedChanged(object sender, CheckedChangedEventArgs e)
  {
    if (sender == optEmulateur && e.Value)      RootEntry.Text = "/storage/0000-0000/PODCASTS";
    else if (sender == optRedmi && e.Value)     RootEntry.Text = "/storage/0141-3140/PODCASTS";
    else if (sender == optPixel && e.Value)     RootEntry.Text = "/storage/????-????/PODCASTS";
  }
}