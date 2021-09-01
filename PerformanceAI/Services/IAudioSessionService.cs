using System;

// https://stackoverflow.com/questions/18419249/ios-audiosessionsetactive-blocking-main-thread
// https://social.msdn.microsoft.com/Forums/en-US/e2a927ab-4633-48db-bab6-8a3e3640e198/avaudiosessionnotificationsobserveinterruption-does-not-work-with-google-maps?forum=xamarinforms
namespace PerformanceAI.Services
{
    public interface IAudioSessionService
    {
        void ActivateAudioPlaybackSession();
        void ActivateAudioRecordingSession();
    }
}
