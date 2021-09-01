using System;
using AVFoundation;
using Foundation;
using PerformanceAI.Services;
using Xamarin.Forms;

[assembly: Dependency(typeof(PerformanceAI.iOS.Services.IOSAudioSessionService))]
namespace PerformanceAI.iOS.Services
{
    public class IOSAudioSessionService : IAudioSessionService
    {
        public void ActivateAudioPlaybackSession()
        {
            var session = AVAudioSession.SharedInstance();
            session.SetCategory(AVAudioSessionCategory.Playback, AVAudioSessionCategoryOptions.DuckOthers);
            session.SetMode(AVAudioSession.ModeSpokenAudio, out NSError error);
            session.SetActive(true);
        }

        public void ActivateAudioRecordingSession()
        {
            try
            {
                new System.Threading.Thread(new System.Threading.ThreadStart(() =>
                {
                    var session = AVAudioSession.SharedInstance();
                    session.SetCategory(AVAudioSessionCategory.Record);
                    session.SetActive(true);
                })).Start();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        // below is one function which just sets category to AVAudioSessionCategoryPlayAndRecord which both enables STT and TTS.
        // This option would mean PAI is still trying to recognise speech whilst playing out
        // audio announcement. I don't want this. Whilst user is requesting one bit of info, they shouldnt be allowed to ask for another.
        // I don't want to handle queuing data queries, etc. 
        //public void AllowRecordingAndPlayback()
        //{
        //    try
        //    {
        //        new System.Threading.Thread(new System.Threading.ThreadStart(() =>
        //        {
        //            var session = AVAudioSession.SharedInstance();
        //            session.SetCategory(AVAudioSessionCategory.PlayAndRecord);
        //            session.SetActive(true);
        //        })).Start();
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine(ex.Message);
        //    }
        //}

    }
}
