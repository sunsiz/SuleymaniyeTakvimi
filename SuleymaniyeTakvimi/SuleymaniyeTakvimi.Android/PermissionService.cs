using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    public class PermissionService : IPermissionService
    {
        private readonly MainActivity _mainActivityInstance;
        private readonly LocationManager _locationManager;

        public PermissionService()
        {
            _mainActivityInstance = MainActivity.Instance;
            _locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
        }

        public void AskNotificationPermission()
        {
            _mainActivityInstance.HandleNotificationPermissionAsync();
        }

        public Task<PermissionStatus> HandlePermissionAsync()
        {
            return _mainActivityInstance.HandleLocationPermissionAsync();
        }

        public bool IsLocationServiceEnabled()
        {
            return _locationManager != null && _locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }

        public bool IsVoiceOverRunning()
        {
            return false;
        }
    }
}