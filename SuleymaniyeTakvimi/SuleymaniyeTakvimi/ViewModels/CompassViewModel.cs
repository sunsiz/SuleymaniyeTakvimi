using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers.Commands;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class CompassViewModel:MvvmHelpers.BaseViewModel
    {

        double current_latitude = 41.0108267;
        double current_longitude = 28.9709183;
        readonly double QiblaLatitude = 21.4224779;
        readonly double QiblaLongitude = 39.8251832;
        internal readonly SensorSpeed speed = SensorSpeed.UI;
        public Command StartCommand { get; }
        public Command StopCommand { get; }
        public Command LocationCommand { get; }
        public CompassViewModel()
        {
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            LocationCommand = new Command(GetLocation);
            if (!Compass.IsMonitoring) Compass.Start(speed);
        }

        string headingDisplay;
        public string HeadingDisplay
        {
            get => headingDisplay;
            set => SetProperty(ref headingDisplay, value);
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
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromMilliseconds(10));
                var location = await Geolocation.GetLocationAsync(request);
                if (location != null)
                {
                    current_latitude = location.Latitude;
                    current_longitude = location.Longitude;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }
            finally { IsBusy = false; }
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
                Info = string.Format("Lat: {0} Long: {1} degree:{2}", current_latitude, current_longitude, Heading);
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
