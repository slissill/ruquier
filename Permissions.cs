using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Maui.Controls.PlatformConfiguration;
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

  public static void RequestStoragePermission()
  {
#if ANDROID
    var activity = Platform.CurrentActivity;
    if (Build.VERSION.SdkInt >= BuildVersionCodes.R)
    {
      if (!Android.OS.Environment.IsExternalStorageManager)
      {
        var intent = new Android.Content.Intent(Android.Provider.Settings.ActionManageAllFilesAccessPermission);
        activity.StartActivity(intent);
      }
    }
    else
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
