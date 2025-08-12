using System.Runtime.Versioning;
#if ANDROID
using Android;
using Android.Content.PM;
using Android.OS;
using AndroidX.Core.App;
using AndroidX.Core.Content;
#endif

namespace Ruquier;

public static class Permissions
{
#if ANDROID
  [SupportedOSPlatform("android30.0")]
  private static bool IsExternalStorageManager()
  {
    return Android.OS.Environment.IsExternalStorageManager;
  }
#endif

  public static void RequestStoragePermission()
  {
#if ANDROID
    var activity = Platform.CurrentActivity;
    if (activity == null)
      return; // Ou gérer l'absence d'activité

    if (Build.VERSION.SdkInt >= BuildVersionCodes.R) // API 30+
    {
#pragma warning disable CA1416 // Validate platform compatibility
      if (!IsExternalStorageManager())
      {
        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);
        activity.StartActivity(intent);
      }
#pragma warning restore CA1416
    }
    else // API < 30
    {
      if (ContextCompat.CheckSelfPermission(activity, Manifest.Permission.ReadExternalStorage) != Permission.Granted)
      {
        ActivityCompat.RequestPermissions(activity, new string[]
        {
                    Manifest.Permission.ReadExternalStorage,
                    Manifest.Permission.WriteExternalStorage
        }, 1);
      }
    }
#endif
  }
}
