using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using PerformanceAI.Services;
using Xamarin.Forms;
using PerformanceAI.Droid.Services;

namespace PerformanceAI.Droid
{
    [Activity(Label = "PerformanceAI", Icon = "@mipmap/ic_launcher", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {

        IMicrophoneService micService;

        // static reference for service objects.
        public static MainActivity Instance { get; private set; }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Instance = this;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            micService = DependencyService.Resolve<IMicrophoneService>();
        }
        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            // update AndroidMicrophoneService's RecordAudioPermissionCode object
            // when permissions request is updated by user.
            switch (requestCode)
            {
                case AndroidMicrophoneService.RecordAudioPermissionCode:
                    if (grantResults[0] == Permission.Granted)
                    {
                        micService.OnRequestPermissionResult(true);
                    }
                    else
                    {
                        micService.OnRequestPermissionResult(false);
                    }
                    break;
            }
        }
    }
}
