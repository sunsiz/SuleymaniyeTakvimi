using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using Acr.UserDialogs;
using MvvmHelpers.Commands;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class CompassViewModel:MvvmHelpers.BaseViewModel
    {
        private double _currentLatitude = 41.0108267;
        private double _currentLongitude = 28.9709183;
        private double _currentAltitude = 4;
        private readonly double _qiblaLatitude = 21.4224779;
        private readonly double _qiblaLongitude = 39.8251832;
        internal readonly SensorSpeed Speed = SensorSpeed.UI;
        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command LocationCommand { get; }
        public Command GoToMapCommand { get; }
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        public ICommand TapCommand => new Xamarin.Forms.Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));
        public CompassViewModel()
        {
            Title = AppResources.KibleGostergesi;
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            LocationCommand = new Command(GetLocation);
            if (!Compass.IsMonitoring) Compass.Start(Speed);
            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            GoToMapCommand = new Command(async () => {
                try
                {
                    var location = new Location(Convert.ToDouble(_currentLatitude, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_currentLongitude, CultureInfo.InvariantCulture.NumberFormat));
                    var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_currentLatitude, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_currentLongitude, CultureInfo.InvariantCulture.NumberFormat)).ConfigureAwait(true);
                    var options = new MapLaunchOptions { Name = placemark.FirstOrDefault()?.Thoroughfare ?? placemark.FirstOrDefault()?.CountryName };

                    await Map.OpenAsync(location, options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Toast(AppResources.HaritaHatasi + ex.Message);
                }
            });
        }

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

        private string _info;
        public string Info
        {
            get => _info;
            set => SetProperty(ref _info, value);
        }

        private string _info1;
        public string Info1
        {
            get => _info1;
            set => SetProperty(ref _info1, value);
        }

        private string _konum;
        public string Konum
        {
            get => _konum;
            set => SetProperty(ref _konum, value);
        }
        private void Start()
        {
            Compass.ReadingChanged += Compass_ReadingChanged;
        }

        private void Stop()
        {
            if (!Compass.IsMonitoring)
                return;

            Compass.ReadingChanged -= Compass_ReadingChanged;
            Compass.Stop();
        }

        internal async void GetLocation()
        {
            IsBusy = true; 
            //UserDialogs.Instance.Toast("Konumu almaya çalışıyor", TimeSpan.FromSeconds(3));
            try
            {
                //var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromMilliseconds(3));
                //var location = await Geolocation.GetLocationAsync(request);
                DataService data = new DataService();
                var takvim = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
                if (takvim != null && takvim.Enlem != 0 && takvim.Boylam != 0)
                {
                    Location location = new Location(takvim.Enlem, takvim.Boylam, takvim.Yukseklik);
                    _currentLatitude = location.Latitude;
                    _currentLongitude = location.Longitude;
                    _currentAltitude = location.Altitude ?? 0.0;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
            finally { IsBusy = false; /*UserDialogs.Instance.Toast("Konum başarıyla yenilendi", TimeSpan.FromSeconds(3));*/ }
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            try
            {
                var qiblaLocation = new Location(_qiblaLatitude, _qiblaLongitude);
                var position = new Location(_currentLatitude, _currentLongitude);
                var res = DistanceCalculator.Bearing(position, qiblaLocation);
                var targetHeading = (360 - res) % 360;

                var currentHeading = 360 - e.Reading.HeadingMagneticNorth;
                Heading = currentHeading - targetHeading;

                //var d = GetDegree() - e.Reading.HeadingMagneticNorth;
                //Heading = d;// e.Reading.HeadingMagneticNorth;
                //Info = HeadingDisplay = $"Heading: {Heading}";
                LatitudeAltitude = $"{AppResources.EnlemFormatsiz}: {_currentLatitude.ToString("F2")}  |  {AppResources.YukseklikFormatsiz}: {_currentAltitude.ToString("N0")}";
                DegreeLongitude = $"{AppResources.BoylamFormatsiz}: {_currentLongitude.ToString("F2")}  |  {AppResources.Aci}: {Heading.ToString("####")}";
                Info = string.Format("Lat: {0} Long: {1} degree:{2}", _currentLatitude, _currentLongitude, Heading);
                Konum = String.Format("{0:F4}, {1:F4}", _currentLatitude, _currentLongitude);
                PointToQibla(e);

            }
            catch (Exception ex) {Debug.WriteLine(ex.Message); }
        }

        internal void PointToQibla(CompassChangedEventArgs e)
        {
            double latitudeFromRadians = _currentLatitude * Math.PI / 180;
            double longitudeFromRadians = _currentLongitude * Math.PI / 180;
            double latitudeToRadians = _qiblaLatitude * Math.PI / 180;
            double longitudeToRadians = _qiblaLongitude * Math.PI / 180;
            double bearing = Math.Atan2(Math.Sin(longitudeToRadians - longitudeFromRadians) * Math.Cos(latitudeToRadians), (Math.Cos(latitudeFromRadians) * Math.Sin(latitudeToRadians)) - (Math.Sin(latitudeFromRadians) * Math.Cos(latitudeToRadians) * Math.Cos(longitudeToRadians - longitudeFromRadians)));
            bearing = Mod(bearing, 2 * Math.PI);
            double bearingDegree = bearing * 180 / Math.PI;
            //pointer1.Value = bearing_degree;
            Info1 = $"Lat: {_currentLatitude} Long: {_currentLongitude} degree:{bearingDegree}";
        }

        private double Mod(double a, double b)
        {
            return a - b * Math.Floor(a / b);
        }
    }

    internal class DistanceCalculator
    {
        private const double KDegreesToRadians = Math.PI / 180.0;
        private const double KRadiansToDegrees = 180.0 / Math.PI;

        public static double Bearing(Location position, Location location)
        {
            double fromLong = position.Longitude * KDegreesToRadians;
            double toLong = location.Longitude * KDegreesToRadians;
            double toLat = location.Latitude * KDegreesToRadians;
            double fromLat = position.Latitude * KDegreesToRadians;

            double dlon = toLong - fromLong;
            double y = Math.Sin(dlon) * Math.Cos(toLat);
            double x = Math.Cos(fromLat) * Math.Sin(toLat) - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(dlon);

            double direction = Math.Atan2(y, x);

            // convert to degrees
            direction *= KRadiansToDegrees;
            // normalize
            double fraction = Modf(direction + 360.0, direction);
            direction += fraction;

            if (direction > 360)
            {
                direction -= 360;
            }

            return direction;
        }
        private static double Modf(double orig, double ipart)
        {
            return orig - (Math.Floor(orig));
        }
    }
}
