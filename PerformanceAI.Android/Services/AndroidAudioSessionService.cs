using System;
using PerformanceAI.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PerformanceAI.Droid.Services.AndroidAudioSessionService))]
namespace PerformanceAI.Droid.Services
{
    public class AndroidAudioSessionService : IAudioSessionService
    {
        public void ActivateAudioPlaybackSession()
        {
            // do nothing as not required on Android
        }

        public void ActivateAudioRecordingSession()
        {
            // do nothing as not required on Android
        }
    }
}
