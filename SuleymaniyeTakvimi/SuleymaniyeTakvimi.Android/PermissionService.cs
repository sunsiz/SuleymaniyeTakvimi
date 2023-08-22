using System.Threading.Tasks;
using Android.Content;
using Android.Locations;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    public class PermissionService : IPermissionService
    {
		public void AskNotificationPermission()
		{
			MainActivity main = MainActivity.Instance;
			main.HandleNotificationPermissionAsync();
		}

		public async Task<PermissionStatus> HandlePermissionAsync()
        {
            MainActivity main = MainActivity.Instance;
            var status = await main.HandleLocationPermissionAsync().ConfigureAwait(false);
            return status;
        }

        public bool IsLocationServiceEnabled()
        {
            LocationManager locationManager = (LocationManager)Android.App.Application.Context.GetSystemService(Context.LocationService);
            return locationManager != null && locationManager.IsProviderEnabled(LocationManager.GpsProvider);
        }

        public bool IsVoiceOverRunning()
        {
            return false;
        }
    }
}