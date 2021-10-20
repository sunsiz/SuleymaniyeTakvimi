using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Input;
using Acr.UserDialogs;
using MvvmHelpers.Commands;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class CompassViewModel:MvvmHelpers.BaseViewModel
    {
        private double current_latitude = 41.0108267;
        private double current_longitude = 28.9709183;
        private double current_altitude = 4;
        private readonly double QiblaLatitude = 21.4224779;
        private readonly double QiblaLongitude = 39.8251832;
        internal readonly SensorSpeed speed = SensorSpeed.UI;
        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command LocationCommand { get; }
        public Command GoToMapCommand { get; }
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        public ICommand TapCommand => new Xamarin.Forms.Command<string>(async (url) => await Launcher.OpenAsync(url));
        public CompassViewModel()
        {
            Title = "Kıble Göstergesi";
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            LocationCommand = new Command(GetLocation);
            if (!Compass.IsMonitoring) Compass.Start(speed);
            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            GoToMapCommand = new Command(async () => {
                var location = new Location(Convert.ToDouble(current_latitude, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(current_longitude, CultureInfo.InvariantCulture.NumberFormat));
                var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(current_latitude, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(current_longitude, CultureInfo.InvariantCulture.NumberFormat));
                var options = new MapLaunchOptions { Name = placemark.FirstOrDefault()?.Thoroughfare ?? placemark.FirstOrDefault()?.CountryName };

                try
                {
                    await Map.OpenAsync(location, options);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Toast("Haritayı açarken bir sorun oluştu.\nDetaylar: " + ex.Message);
                }
            });
        }

        string lalongtitude;
        public string Lalongtitude
        {
            get => lalongtitude;
            set => SetProperty(ref lalongtitude, value);
        }

        string degaltitude;
        public string Degaltitude
        {
            get => degaltitude;
            set => SetProperty(ref degaltitude, value);
        }

        double heading = 0;
        public double Heading
        {
            get => heading;
            set => SetProperty(ref heading, value);
        }
        string info;
        public string Info
        {
            get => info;
            set => SetProperty(ref info, value);
        }
        string info1;
        public string Info1
        {
            get => info1;
            set => SetProperty(ref info1, value);
        }

        private string konum;
        public string Konum
        {
            get => konum;
            set => SetProperty(ref konum, value);
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
                var takvim = data.GetCurrentLocation().Result;
                if (takvim != null)
                {
                    Location location = new Location(takvim.Enlem, takvim.Boylam, takvim.Yukseklik);
                    current_latitude = location.Latitude;
                    current_longitude = location.Longitude;
                    current_altitude = location.Altitude ?? 0.0;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally { IsBusy = false; /*UserDialogs.Instance.Toast("Konum başarıyla yenilendi", TimeSpan.FromSeconds(3));*/ }
        }

        private void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            try
            {
                var qiblaLocation = new Location(QiblaLatitude, QiblaLongitude);
                var position = new Location(current_latitude, current_longitude);
                var res = DistanceCalculator.Bearing(position, qiblaLocation);
                var TargetHeading = (360 - res) % 360;

                var currentHeading = 360 - e.Reading.HeadingMagneticNorth;
                Heading = currentHeading - TargetHeading;

                //var d = GetDegree() - e.Reading.HeadingMagneticNorth;
                //Heading = d;// e.Reading.HeadingMagneticNorth;
                //Info = HeadingDisplay = $"Heading: {Heading}";
                Lalongtitude = $"Enlem: {current_latitude.ToString("F2")}  |  Boylam: {current_longitude.ToString("F2")}";
                Degaltitude = $"Yükseklik: {current_altitude.ToString("N0")}  |  Açı: {Heading.ToString("F2")}";
                Info = string.Format("Lat: {0} Long: {1} degree:{2}", current_latitude, current_longitude, Heading);
                Konum = String.Format("{0:F4}, {1:F4}", current_latitude, current_longitude);
                PointToQibla(e);

            }
            catch (Exception ex) { }
        }

        internal void PointToQibla(CompassChangedEventArgs e)
        {
            double latt_from_radians = current_latitude * Math.PI / 180;
            double long_from_radians = current_longitude * Math.PI / 180;
            double latt_to_radians = QiblaLatitude * Math.PI / 180;
            double lang_to_radians = QiblaLongitude * Math.PI / 180;
            double bearing = Math.Atan2(Math.Sin(lang_to_radians - long_from_radians) * Math.Cos(latt_to_radians), (Math.Cos(latt_from_radians) * Math.Sin(latt_to_radians)) - (Math.Sin(latt_from_radians) * Math.Cos(latt_to_radians) * Math.Cos(lang_to_radians - long_from_radians)));
            bearing = Mod(bearing, 2 * Math.PI);
            double bearing_degree = bearing * 180 / Math.PI;
            //pointer1.Value = bearing_degree;
            Info1 = string.Format("Lat: {0} Long: {1} degree:{2}", current_latitude, current_longitude, bearing_degree.ToString());
        }

        private double Mod(double a, double b)
        {
            return a - b * Math.Floor(a / b);
        }
    }

    internal class DistanceCalculator
    {
        const double kDegreesToRadians = Math.PI / 180.0;
        const double kRadiansToDegrees = 180.0 / Math.PI;

        public static double Bearing(Location position, Location location)
        {
            double fromLong = position.Longitude * kDegreesToRadians;
            double toLong = location.Longitude * kDegreesToRadians;
            double toLat = location.Latitude * kDegreesToRadians;
            double fromLat = position.Latitude * kDegreesToRadians;

            double dlon = toLong - fromLong;
            double y = Math.Sin(dlon) * Math.Cos(toLat);
            double x = Math.Cos(fromLat) * Math.Sin(toLat) - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(dlon);

            double direction = Math.Atan2(y, x);

            // convert to degrees
            direction *= kRadiansToDegrees;
            // normalize
            double fraction = modf(direction + 360.0, direction);
            direction += fraction;

            if (direction > 360)
            {
                direction -= 360;
            }

            return direction;
        }
        private static double modf(double orig, double ipart)
        {
            return orig - (Math.Floor(orig));
        }
    }
}
