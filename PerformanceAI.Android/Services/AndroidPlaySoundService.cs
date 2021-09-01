using System;
using Android.Media;
using Xamarin.Forms;

[assembly: Dependency(typeof(PerformanceAI.Droid.Services.AndroidPlaySoundService))]
namespace PerformanceAI.Droid.Services
{
    public class AndroidPlaySoundService
    {
        public void PlaySystemSound()
        {
            Android.Net.Uri uri = RingtoneManager.GetDefaultUri(RingtoneType.Ringtone);
            Ringtone rt = RingtoneManager.GetRingtone(MainActivity.Instance.ApplicationContext, uri);
            rt.Play();
        }
    }
}
