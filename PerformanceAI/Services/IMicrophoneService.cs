using System.Threading.Tasks;

namespace PerformanceAI.Services
{
    // Service requires access to device microphone.
    // can obtain platform-specific implementations of this interface
    // on each platform by using the dependency service
    public interface IMicrophoneService
    {
        Task<bool> GetPermissionAsync();
        void OnRequestPermissionResult(bool isGranted);
    }
}
