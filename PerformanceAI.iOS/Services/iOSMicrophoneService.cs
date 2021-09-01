using System;
using System.Threading.Tasks;
using AVFoundation;
using PerformanceAI.iOS.Services;
using PerformanceAI.Services;
using Xamarin.Forms;

// register with the DependencyService
[assembly: Dependency(typeof(iOSMicrophoneService))]

namespace PerformanceAI.iOS.Services 
{
    public class iOSMicrophoneService : IMicrophoneService
    {
        TaskCompletionSource<bool> tcsPermissions;

        public Task<bool> GetPermissionAsync()
        {
            tcsPermissions = new TaskCompletionSource<bool>();
            RequestMicPermission();
            return tcsPermissions.Task;
        }

        public void OnRequestPermissionResult(bool isGranted)
        {
            tcsPermissions.TrySetResult(isGranted);
        }

        // request permissions from the device user.
        void RequestMicPermission()
        {
            var session = AVAudioSession.SharedInstance();
            session.RequestRecordPermission((granted) =>
            {
                tcsPermissions.TrySetResult(granted);
            });
        }
    }
}
