using System;
using System.Threading.Tasks;
using PerformanceAI.Helpers;
using Xamarin.Essentials;
using Xamarin.Forms;

[assembly: Dependency(typeof(PerformanceAI.Droid.LocationPermission))]
namespace PerformanceAI.Droid
{
    public class LocationPermission : ILocationPermission
    {

        // straight from code example in the docs
        // docs.microsoft.com/en-us/xamarin/xamarin-forms/app-fundamentals/dependency-service/introduction#create-an-interface

        public async Task<PermissionStatus> CheckAndRequestLocationPermission()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

            if (status == PermissionStatus.Granted)
                return status;

            if (status == PermissionStatus.Denied && DeviceInfo.Platform == DevicePlatform.iOS)
            {
                // Prompt the user to turn on in settings
                // On iOS once a permission has been denied it may not be requested again from the application
                // todo: show pop-up dialog and direct user to settings.
                return status;
            }

            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            {
                // Prompt the user with additional information as to why the permission is needed
                // todo: display alert with reason for permission, e.g. Location permissions required for Bluetooth. 
            }

            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();

            return status;
        }


    }
}
