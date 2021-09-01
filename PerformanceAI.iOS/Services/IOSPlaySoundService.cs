using System;
using AudioToolbox;
using Xamarin.Forms;

[assembly: Dependency(typeof(PerformanceAI.iOS.Services.IOSPlaySoundService))]
namespace PerformanceAI.iOS.Services
{
    public class IOSPlaySoundService
    {
        public void PlaySystemSound()
        {
            var sound = new SystemSound(1000);
            sound.PlaySystemSound();
        }
    }
}
