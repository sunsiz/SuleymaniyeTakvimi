using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Command = MvvmHelpers.Commands.Command;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class CompassViewModel:MvvmHelpers.BaseViewModel
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
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        //public ICommand TapCommand => new Xamarin.Forms.Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));
        public CompassViewModel()
        {
            Title = AppResources.KibleGostergesi;
            StartCommand = new Command(Start);
            StopCommand = new Command(Stop);
            _currentLatitude = Preferences.Get("LastLatitude", 0.0);
            _currentLongitude = Preferences.Get("LastLongitude", 0.0);
            _currentAltitude = Preferences.Get("LastAltitude", 0.0);
            LocationCommand = new Command(async () =>
            {
                IsBusy = true;
                try
                {
                    if (_currentLatitude==0.0 && _currentLongitude==0.0 && _currentAltitude==0.0)
                    {
                        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
                        if (status == PermissionStatus.Granted)
                        {
                            DataService data = new DataService();
                            var location = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
                            if (location != null && location.Latitude != 0 && location.Longitude != 0)
                            {
                                _currentLatitude = location.Latitude;
                                _currentLongitude = location.Longitude;
                                _currentAltitude = location.Altitude ?? 0.0;
                            }
                        }
                        else if(!_askedPermission)
                        {
                            var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync().ConfigureAwait(false);
                            _askedPermission = true;
                            Debug.WriteLine(result);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
                finally { IsBusy = false; }
            });
            try
            {
                if (!Compass.IsMonitoring)
                {
                    Compass.Start(Speed, applyLowPassFilter: true);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
                UserDialogs.Instance.Toast(AppResources.CihazPusulaDesteklemiyor, TimeSpan.FromSeconds(4));
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(Compass_ReadingChanged)}: {fnsEx.Message}");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
                Debug.WriteLine(ex.Message);
            }

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
            RefreshLocationCommand = new Command(async () =>
            {
                using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
                {
                    var data = new DataService();
                    var location = await data.GetCurrentLocationAsync(true).ConfigureAwait(false);
                    if (location != null && location.Latitude != 0 && location.Longitude != 0)
                    {
                        _currentLatitude = location.Latitude;
                        _currentLongitude = location.Longitude;
                        _currentAltitude = location.Altitude ?? 0.0;
                    }
                }
            });
            LatitudeAltitude =
                $"{AppResources.EnlemFormatsiz}: {_currentLatitude:F2}  |  {AppResources.YukseklikFormatsiz}: {_currentAltitude:N0}";
            DegreeLongitude =
                $"{AppResources.BoylamFormatsiz}: {_currentLongitude:F2}  |  {AppResources.Aci}: {Heading:####}";
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

        //private async void GetLocation()
        //{
        //    IsBusy = true; 
        //    //UserDialogs.Instance.Toast("Konumu almaya çalışıyor", TimeSpan.FromSeconds(3));
        //    try
        //    {
        //        //var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromMilliseconds(3));
        //        //var location = await Geolocation.GetLocationAsync(request);
        //        DataService data = new DataService();
        //        var location = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
        //        if (location != null && location.Latitude != 0 && location.Longitude != 0)
        //        {
        //            //Location location = new Location(takvim.Enlem, takvim.Boylam, takvim.Yukseklik);
        //            _currentLatitude = location.Latitude;
        //            _currentLongitude = location.Longitude;
        //            _currentAltitude = location.Altitude ?? 0.0;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine(ex.Message);
        //    }
        //    finally { IsBusy = false; /*UserDialogs.Instance.Toast("Konum başarıyla yenilendi", TimeSpan.FromSeconds(3));*/ }
        //}

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
                LatitudeAltitude =
                    $"{AppResources.EnlemFormatsiz}: {_currentLatitude:F2}  |  {AppResources.YukseklikFormatsiz}: {_currentAltitude:N0}";
                DegreeLongitude =
                    $"{AppResources.BoylamFormatsiz}: {_currentLongitude:F2}  |  {AppResources.Aci}: {Heading:####}";
                //Info = string.Format("Lat: {0} Long: {1} degree:{2}", _currentLatitude, _currentLongitude, Heading);
                //Konum = String.Format("{0:F4}, {1:F4}", _currentLatitude, _currentLongitude);
                //PointToQibla(e);

            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
                UserDialogs.Instance.Alert(AppResources.CihazPusulaDesteklemiyor, AppResources.CihazPusulaDesteklemiyor);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(Compass_ReadingChanged)}: {fnsEx.Message}");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
                Debug.WriteLine(ex.Message);
            }
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
            //double y = Math.Sin(dlon) * Math.Cos(toLat);
            //double x = Math.Cos(fromLat) * Math.Sin(toLat) - Math.Sin(fromLat) * Math.Cos(toLat) * Math.Cos(dlon);
            double y = Math.Sin(dlon);
            double x = Math.Cos(fromLat) * Math.Tan(toLat) - Math.Sin(fromLat) * Math.Cos(dlon);

            double direction = Math.Atan2(y, x);

            // convert to degrees
            direction *= KRadiansToDegrees;
            // normalize
            //double fraction = Modf(direction + 360.0, direction);
            //direction += fraction;

            //if (direction > 360)
            //{
            //    direction -= 360;
            //}

            return direction;
        }
        //private static double Modf(double orig, double ipart)
        //{
        //    return orig - Math.Floor(orig);
        //}
    }
}
