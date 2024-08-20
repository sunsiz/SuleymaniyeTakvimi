using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class CompassViewModel : BaseViewModel
    {
        private double _currentLatitude = 41.0;
        private double _currentLongitude = 29.0;
        private double _currentAltitude = 114;
        private readonly double _qiblaLatitude = 21.4224779;
        private readonly double _qiblaLongitude = 39.8251832;
        internal readonly SensorSpeed Speed = SensorSpeed.UI;
        private bool _askedPermission;
        public Command StartCommand { get; }
        private Command StopCommand { get; }
        public Command LocationCommand { get; }
        public Command RefreshLocationCommand { get; }

        private string _latitudeAltitude;
        public string LatitudeAltitude
        {
            get => _latitudeAltitude;
            set => SetProperty(ref _latitudeAltitude, value);
        }

        private string _degreeLongitude;
        public string DegreeLongitude
        {
            get => _degreeLongitude;
            set => SetProperty(ref _degreeLongitude, value);
        }

        private double _heading;
        public double Heading
        {
            get => _heading;
            set => SetProperty(ref _heading, value);
        }

        public Command GoToMapCommand { get; }

        public CompassViewModel(DataService dataService):base(dataService)
        {
            Title = AppResources.KibleGostergesi;
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            _currentLatitude = Preferences.Get("LastLatitude", 0.0);
            _currentLongitude = Preferences.Get("LastLongitude", 0.0);
            _currentAltitude = Preferences.Get("LastAltitude", 0.0);
            LocationCommand = new Command(async () => await UpdateLocation());
            GoToMapCommand = new Command(async () => await GoToMap());
            RefreshLocationCommand = new Command(async () => await RefreshLocation());

            if (!Compass.IsMonitoring)
            {
                try
                {
                    Compass.Start(Speed, applyLowPassFilter: true);
                }
                catch (FeatureNotSupportedException)
                {
                    UserDialogs.Instance.Toast(AppResources.CihazPusulaDesteklemiyor, TimeSpan.FromSeconds(4));
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Alert(ex.Message);
                }
            }

            UpdateDisplay();
        }

        private void Start()
        {
            _currentLatitude = Preferences.Get("LastLatitude", 0.0);
            _currentLongitude = Preferences.Get("LastLongitude", 0.0);
            _currentAltitude = Preferences.Get("LastAltitude", 0.0);
            Compass.ReadingChanged += Compass_ReadingChanged;
        }

        private void Stop()
        {
            if (!Compass.IsMonitoring)
                return;

            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            var qiblaLocation = new Location(_qiblaLatitude, _qiblaLongitude);
            var position = new Location(_currentLatitude, _currentLongitude);
            var res = DistanceCalculator.Bearing(position, qiblaLocation);
            var targetHeading = (360 - res) % 360;

            var currentHeading = 360 - e.Reading.HeadingMagneticNorth;
            Heading = currentHeading - targetHeading;

            UpdateDisplay();
        }

        private async Task UpdateLocation()
        {
            IsBusy = true;
            try
            {
                if (_currentLatitude == 0.0 && _currentLongitude == 0.0 && _currentAltitude == 0.0)
                {
                    var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                    if (status == PermissionStatus.Granted)
                    {
                        var location = await DataService.GetCurrentLocationAsync(false);
                        if (Helper.IsValidLocation(location))
                        {
                            _currentLatitude = location.Latitude;
                            _currentLongitude = location.Longitude;
                            _currentAltitude = location.Altitude ?? 0.0;
                        }
                    }
                    else if (!_askedPermission)
                    {
                        await Device.InvokeOnMainThreadAsync(async () =>
                        {
                            status = await DependencyService.Get<IPermissionService>().HandlePermissionAsync();
                        });
                        _askedPermission = true;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally { IsBusy = false; }
        }

        private async Task GoToMap()
        {
            try
            {
                var location = new Location(_currentLatitude, _currentLongitude);
                var placeMark = await Geocoding.GetPlacemarksAsync(_currentLatitude, _currentLongitude);
                var options = new MapLaunchOptions { Name = placeMark.FirstOrDefault()?.Thoroughfare ?? placeMark.FirstOrDefault()?.CountryName };

                await Map.OpenAsync(location, options);
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Toast(AppResources.HaritaHatasi + ex.Message);
            }
        }

        private async Task RefreshLocation()
        {
            using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
            {
                var location = await DataService.GetCurrentLocationAsync(true);
                if (Helper.IsValidLocation(location))
                {
                    _currentLatitude = location.Latitude;
                    _currentLongitude = location.Longitude;
                    _currentAltitude = location.Altitude ?? 0.0;
                }
            }
        }

        private void UpdateDisplay()
        {
            LatitudeAltitude = $"{AppResources.EnlemFormatsiz}: {_currentLatitude:F2}  |  {AppResources.YukseklikFormatsiz}: {_currentAltitude:N0}";
            DegreeLongitude = $"{AppResources.BoylamFormatsiz}: {_currentLongitude:F2}  |  {AppResources.Aci}: {Heading:####}";
        }
    }

    /// <summary>
    /// The DistanceCalculator class provides a method for calculating the bearing (direction) from one location to another.
    /// </summary>
    internal static class DistanceCalculator
    {
        // Conversion constants
        private const double DegreesToRadians = Math.PI / 180.0;
        private const double RadiansToDegrees = 180.0 / Math.PI;

        /// <summary>
        /// Calculates the bearing (direction) from the position to the location.
        /// The bearing is calculated using the longitude and latitude of the two locations.
        /// The result is in degrees, with values ranging from -180 to 180.
        /// </summary>
        /// <param name="position">The starting location.</param>
        /// <param name="location">The target location.</param>
        /// <returns>The bearing from the position to the location, in degrees.</returns>
        public static double Bearing(Location position, Location location)
        {
            double fromLong = position.Longitude * DegreesToRadians;
            double toLong = location.Longitude * DegreesToRadians;
            double toLat = location.Latitude * DegreesToRadians;
            double fromLat = position.Latitude * DegreesToRadians;

            double dlon = toLong - fromLong;
            double y = Math.Sin(dlon);
            double x = Math.Cos(fromLat) * Math.Tan(toLat) - Math.Sin(fromLat) * Math.Cos(dlon);

            double direction = Math.Atan2(y, x) * RadiansToDegrees;

            return direction;
        }
    }
}
