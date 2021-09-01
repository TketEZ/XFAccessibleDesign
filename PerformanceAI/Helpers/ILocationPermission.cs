using System;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace PerformanceAI.Helpers
{
    public interface ILocationPermission
    {
        Task<PermissionStatus> CheckAndRequestLocationPermission();
    }
}
