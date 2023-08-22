using Acr.UserDialogs;
//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Services
{
    public class DataService : IDataService
    {
        private Takvim _konum;
        public Takvim _takvim;
        private IList<Takvim> _monthlyTakvim;
        private bool askedLocationPermission;
        public readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ayliktakvim.xml");

        public DataService()
        {
            InitTakvim();
        }

        public void InitTakvim()
        {
            _takvim = GetTakvimFromFile() ?? new Takvim()
            {
                Enlem = Preferences.Get("enlem", 41.0),
                Boylam = Preferences.Get("boylam", 29.0),
                Yukseklik = Preferences.Get("yukseklik", 114.0),
                SaatBolgesi = Preferences.Get("saatbolgesi", 3.0),
                YazKis = Preferences.Get("yazkis", 0.0),
                FecriKazip = Preferences.Get("fecrikazip", "06:28"),
                FecriSadik = Preferences.Get("fecrisadik", "07:16"),
                SabahSonu = Preferences.Get("sabahsonu", "08:00"),
                Ogle = Preferences.Get("ogle", "12:59"),
                Ikindi = Preferences.Get("ikindi", "15:27"),
                Aksam = Preferences.Get("aksam", "17:54"),
                Yatsi = Preferences.Get("yatsi", "18:41"),
                YatsiSonu = Preferences.Get("yatsisonu", "19:31"),
                Tarih = Preferences.Get("tarih", DateTime.Today.ToString("dd/MM/yyyy"))
            };
        }

        public async Task<Location> GetCurrentLocationAsync(bool refreshLocation)
        {
            //Analytics.TrackEvent("GetCurrentLocation in the DataService Triggered: " + $" at {DateTime.Now}");
            var location = new Location(0,0);
            if (!askedLocationPermission)
            {
                //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                var status = await DependencyService.Get<IPermissionService>().HandlePermissionAsync().ConfigureAwait(false);
                if (status != PermissionStatus.Granted)
                {
                    // Notify user permission was denied
                    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                    askedLocationPermission = true;
                    return location;
                }
            }
            try
            {
                //var status = await LocationService.IsLocationPermissionGranted();
                //var location = await LocationService.GetCurrentLocation();

                if (!refreshLocation) location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

                //konum = new Takvim();

                if (location == null || refreshLocation)
                {
                    
                    var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(false);
                }

                if (location != null && location.Latitude != 0 && location.Longitude != 0)
                {
                    Debug.WriteLine(
                        $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    //konum.Enlem = location.Latitude;
                    //konum.Boylam = location.Longitude;
                    //konum.Yukseklik = location.Altitude ?? 0;
                    Preferences.Set("LastLatitude", location.Latitude);
                    Preferences.Set("LastLongitude", location.Longitude);
                    Preferences.Set("LastAltitude", location.Altitude ?? 0);
                    Preferences.Set("LocationSaved", true);
                }
                else
                {
                    if (!DependencyService.Get<IPermissionService>().IsLocationServiceEnabled())
                        UserDialogs.Instance.Toast(AppResources.KonumKapaliBaslik, TimeSpan.FromSeconds(5));
                    //var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync().ConfigureAwait(false);
                    //Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {result}");
                    //if (result == PermissionStatus.Denied)
                    //    UserDialogs.Instance.Toast(AppResources.KonumIzniBaslik, TimeSpan.FromSeconds(5));
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                UserDialogs.Instance.Alert(AppResources.CihazGPSDesteklemiyor, AppResources.CihazGPSDesteklemiyor);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {fnsEx.Message}");
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {fneEx.Message}");
                UserDialogs.Instance.Alert(AppResources.KonumKapali, AppResources.KonumKapaliBaslik);
                //await App.Current.MainPage.DisplayAlert("Konum Servisi Hatası", "Cihazın konum servisi kapalı, Öce konum servisini açmanız lazım!", "Tamam");
                if (Preferences.Get("LastLatitude", 0.0) != 0.0 && Preferences.Get("LastLongitude", 0.0) != 0.0)
                {
                    location ??= new Location();
                    location.Latitude = Preferences.Get("LastLatitude", 0.0);
                    location.Longitude = Preferences.Get("LastLongitude", 0.0);
                    location.Altitude = Preferences.Get("LastAltitude", 0.0);
                }
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {pEx.Message}");
                //await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                //UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                //await App.Current.MainPage.DisplayAlert("Konum Servisi İzni Yok", "Uygulamanın normal çalışması için Konuma erişme yetkisi vermeniz lazım!", "Tamam");
                //var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync().ConfigureAwait(false);
                //Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {result}");
                //if (result == PermissionStatus.Denied)
                //    UserDialogs.Instance.Toast(AppResources.KonumIzniBaslik, TimeSpan.FromSeconds(5));
            }
            catch (Exception ex)
            {
                // Unable to get location
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {ex.Message}");
            }

            return location;
        }

        public async Task<Takvim> VakitHesabiAsync()
        {
            //Analytics.TrackEvent("VakitHesabi in the DataService Triggered: " + $" at {DateTime.Now}");
            _takvim = GetTakvimFromFile();
            if (_takvim != null) return _takvim;
            _takvim = new Takvim();

            if (!HaveInternet()) return null;
            var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
            if (location != null)
            {
                
                var url = "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi?";
                url += "Enlem=" + location.Latitude;
                url += "&Boylam=" + location.Longitude;
                url += "&Yukseklik=" + location.Altitude;
                url = url.Replace(',', '.');
                url += "&SaatBolgesi=" + TimeZoneInfo.Local.BaseUtcOffset.Hours;//.StandardName;
                url += "&yazSaati=" + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0);
                url += "&Tarih=" + DateTime.Today.ToString("dd/MM/yyyy");

                //var url =
                //    "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi?Enlem=41.02&Boylam=28.93&Yukseklik=1&SaatBolgesi=3.00&yazSaati=0&Tarih=05/02/2021";
                //Uri geturi = new Uri(url);
                //HttpClient client = new HttpClient();
                //HttpResponseMessage responseGet = await client.GetAsync(geturi).ConfigureAwait(false);
                //string response = await responseGet.Content.ReadAsStringAsync().ConfigureAwait(false);
                //Xml Parsing  
                XDocument doc = XDocument.Load(url);
                if (doc.Root == null) return null;
                foreach (var item in doc.Root.Descendants())
                {
                    switch (item.Name.LocalName)
                    {
                        //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
                        case "Enlem":
                            _takvim.Enlem = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "Boylam":
                            _takvim.Boylam = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "Yukseklik":
                            _takvim.Yukseklik =
                                Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "SaatBolgesi":
                            _takvim.SaatBolgesi =
                                Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "YazKis":
                            _takvim.YazKis = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "FecriKazip":
                            _takvim.FecriKazip = item.Value;
                            break;
                        case "FecriSadik":
                            _takvim.FecriSadik = item.Value;
                            break;
                        case "SabahSonu":
                            _takvim.SabahSonu = item.Value;
                            break;
                        case "Ogle":
                            _takvim.Ogle = item.Value;
                            break;
                        case "Ikindi":
                            _takvim.Ikindi = item.Value;
                            break;
                        case "Aksam":
                            _takvim.Aksam = item.Value;
                            break;
                        case "Yatsi":
                            _takvim.Yatsi = item.Value;
                            break;
                        case "YatsiSonu":
                            _takvim.YatsiSonu = item.Value;
                            break;
                    }
                }
            }

            return _takvim;
        }

        private Takvim GetTakvimFromFile()
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    XDocument xmldoc = XDocument.Load(_fileName);
                    var takvims = ParseXmlList(xmldoc);
                    if (takvims != null && DateTime.Parse(takvims[0].Tarih) <= DateTime.Today &&
                        DateTime.Parse(takvims[takvims.Count - 1].Tarih) >= DateTime.Today)
                    {
                        foreach (var item in takvims)
                        {
                            if (DateTime.Parse(item.Tarih) == DateTime.Today)
                            {
                                _takvim = item;
                                _takvim.Enlem = Preferences.Get("LastLatitude", 0.0);
                                _takvim.Boylam = Preferences.Get("LastLongitude", 0.0);
                                _takvim.Yukseklik = Preferences.Get("LastAltitude", 0.0);
                                return _takvim;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex.Message);
                }
            }

            Task.Run(async () => { await PrepareMonthlyPrayerTimes().ConfigureAwait(false); });

            return _takvim;
        }

        public async Task<Takvim> PrepareMonthlyPrayerTimes()
        {
            var location = await GetCurrentLocationAsync(true).ConfigureAwait(false);
            GetMonthlyPrayerTimes(location, true);
            _takvim = GetTakvimFromFile();
            return _takvim;
        }

        //When refreshLocation true, force refresh location not using last known location.
        public async Task<Takvim> GetPrayerTimesAsync(bool refreshLocation)
        {
            //Analytics.TrackEvent("GetPrayerTimes in the DataService Triggered: " + $" at {DateTime.Now}");
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //if (File.Exists(FileName))
            //{
            //    XDocument xmldoc = XDocument.Load(FileName);
            //    var takvims = ParseXmlList(xmldoc);
            //    if (takvims != null && DateTime.Parse(takvims[0].Tarih) <= DateTime.Today && DateTime.Parse(takvims[takvims.Count - 1].Tarih) >= DateTime.Today)
            //    {
            //        foreach (var item in takvims)
            //        {
            //            if (DateTime.Parse(item.Tarih) == DateTime.Today)
            //            {
            //                takvim = item;
            //                return takvim;
            //            }
            //        }
            //    }
            //}
            try
            {
                if (!HaveInternet()) return null;
                var location = await GetCurrentLocationAsync(refreshLocation).ConfigureAwait(false);
                _konum = new Takvim
                {
                    Enlem = location.Latitude,
                    Boylam = location.Longitude,
                    Yukseklik = location.Altitude ?? 0,
                    SaatBolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours, //.StandardName;
                    YazKis = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0,
                    Tarih = DateTime.Today.ToString("dd/MM/yyyy")
                };
                DateTime now=DateTime.Now;
                DateTime withKind = DateTime.SpecifyKind(now, DateTimeKind.Local);
                Console.WriteLine("{0} - Time is ambiguous? {1}", TimeZoneInfo.Local.DaylightName,TimeZoneInfo.Local.IsAmbiguousTime(withKind));
                Console.WriteLine("{0} - Time is daylight saving time? {1}", TimeZoneInfo.Local.StandardName,TimeZoneInfo.Local.IsDaylightSavingTime(withKind));
                Console.WriteLine("{0} - Time is support daylight saving time? {1}", TimeZoneInfo.Local.DisplayName,TimeZoneInfo.Local.SupportsDaylightSavingTime);
                if (TimeZoneInfo.Local.IsAmbiguousTime(withKind) || TimeZoneInfo.Local.SupportsDaylightSavingTime ||
                    TimeZoneInfo.Local.IsDaylightSavingTime(withKind))
                    Console.WriteLine("{0} may be daylight saving time in {1}.", 
                        withKind, TimeZoneInfo.Local.DisplayName);
                var client = new HttpClient();
                //string baseUrl = "http://servis.suleymaniyetakvimi.com/servis.asmx/";
                //client.BaseAddress = new Uri(baseUrl);

                //var response =
                //    await client.GetAsync(
                //            $"VakitHesabi?Enlem={konum.Enlem}&Boylam={konum.Boylam}&Yukseklik={konum.Yukseklik}&SaatBolgesi={konum.SaatBolgesi}&yazSaati={konum.YazKis}&Tarih={konum.Tarih}")
                //        .ConfigureAwait(false);
                Thread.CurrentThread.CurrentCulture=CultureInfo.GetCultureInfo("en");
                var uri = new Uri("http://servis.suleymaniyetakvimi.com/servis.asmx/" +
                                  $"VakitHesabi?Enlem={Convert.ToDouble(_konum.Enlem, CultureInfo.InvariantCulture.NumberFormat)}" +
                                  $"&Boylam={Convert.ToDouble(_konum.Boylam, CultureInfo.InvariantCulture.NumberFormat)}" +
                                  $"&Yukseklik={Convert.ToDouble(_konum.Yukseklik, CultureInfo.InvariantCulture.NumberFormat)}" +
                                  $"&SaatBolgesi={_konum.SaatBolgesi}&yazSaati={_konum.YazKis}&Tarih={_konum.Tarih}");
                //var url = "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi?";
                //url += "Enlem=" + konum.Enlem;
                //url += "&Boylam=" + konum.Boylam;
                //url += "&Yukseklik=" + konum.Yukseklik;
                //url = url.Replace(',', '.');
                //url += "&SaatBolgesi=" + TimeZoneInfo.Local.BaseUtcOffset.Hours;//.StandardName;
                //url += "&yazSaati=" + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0);
                //url += "&Tarih=" + DateTime.Today.ToString("dd/MM/yyyy");
                var response = await client.GetAsync(uri).ConfigureAwait(false);
                var xmlResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                if (!string.IsNullOrEmpty(xmlResult) && xmlResult.StartsWith("<?xml"))
                    _takvim = ParseXml(xmlResult);
                else
                    UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));

            }
            catch (Exception exception)
            {
                UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
            }
            finally
            {
                UserDialogs.Instance.Toast(AppResources.KonumYenilendi, TimeSpan.FromSeconds(3));
            }
            //var xmlResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Takvim>(responseResult);
            //var xmlResult = baseUrl
            //    .SetQueryParameters(new
            //    {
            //        Enlem = konum.Enlem, Boylam = konum.Boylam, Yukseklik = konum.Yukseklik,
            //        SaatBolgesi = konum.SaatBolgesi, yazSaati = konum.YazKis, Tarih = konum.Tarih
            //    })
            //    .GetJSonAsync<Takvim>().Result;
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            return _takvim;
        }

        private Takvim ParseXml(string xmlResult)
        {
            _takvim = new Takvim();
            XDocument doc = XDocument.Parse(xmlResult);
            //if(doc.Descendants("Takvim")!=null)
            if (doc.Root == null) return _takvim;
            foreach (var item in doc.Root.Descendants())
            {
                switch (item.Name.LocalName)
                {
                    case "Enlem":
                        _takvim.Enlem = Convert.ToDouble(item.Value);
                        break;
                    case "Boylam":
                        _takvim.Boylam = Convert.ToDouble(item.Value);
                        break;
                    case "Yukseklik":
                        _takvim.Yukseklik = Convert.ToDouble(item.Value);
                        break;
                    case "SaatBolgesi":
                        _takvim.SaatBolgesi = Convert.ToDouble(item.Value);
                        break;
                    case "YazKis":
                        _takvim.YazKis = Convert.ToDouble(item.Value);
                        break;
                    case "FecriKazip":
                        _takvim.FecriKazip = item.Value;
                        break;
                    case "FecriSadik":
                        _takvim.FecriSadik = item.Value;
                        break;
                    case "SabahSonu":
                        _takvim.SabahSonu = item.Value;
                        break;
                    case "Ogle":
                        _takvim.Ogle = item.Value;
                        break;
                    case "Ikindi":
                        _takvim.Ikindi = item.Value;
                        break;
                    case "Aksam":
                        _takvim.Aksam = item.Value;
                        break;
                    case "Yatsi":
                        _takvim.Yatsi = item.Value;
                        break;
                    case "YatsiSonu":
                        _takvim.YatsiSonu = item.Value;
                        break;
                }
            }

            return _takvim;
        }

        private bool CheckRemindersEnabledAny()
        {
            return Preferences.Get("fecrikazipEtkin", false) || Preferences.Get("fecrisadikEtkin", false) ||
                   Preferences.Get("sabahsonuEtkin", false) || Preferences.Get("ogleEtkin", false) ||
                   Preferences.Get("ikindiEtkin", false) || Preferences.Get("aksamEtkin", false) ||
                   Preferences.Get("yatsiEtkin", false) || Preferences.Get("yatsisonuEtkin", false);
        }


        public IList<Takvim> GetMonthlyPrayerTimes(Location location, bool forceRefresh)
        {
            //Analytics.TrackEvent("GetMonthlyPrayerTimes in the DataService Triggered: " + $" at {DateTime.Now}");
            if (File.Exists(_fileName) && !forceRefresh)
            {
                try
                {
                    XDocument xmldoc = XDocument.Load(_fileName);
                    var takvims = ParseXmlList(xmldoc);
                    xmldoc = null;
                    if (takvims != null)
                    {
                        var days = (DateTime.Today - DateTime.Parse(takvims[0].Tarih)).Days;
                        if (days is < 21 and >= 0)
                        {
                            //xmldoc = ReadTakvimFile();
                            //MonthlyTakvim = ParseXmlList(xmldoc);
                            _monthlyTakvim = takvims;
                            return _monthlyTakvim;
                        }
                    }

                    if (!HaveInternet()) return _monthlyTakvim = takvims;
                }
                catch (Exception exception)
                {
                    System.Diagnostics.Debug.WriteLine($"An error occurred while reading or parsing the file, details: {exception.Message}");
                }
            }

            if (!HaveInternet()) return null;
            _konum = new Takvim
            {
                Enlem = location.Latitude,
                Boylam = location.Longitude,
                Yukseklik = location.Altitude ?? 0,
                SaatBolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours,
                YazKis = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0,
                Tarih = DateTime.Today.ToString("dd/MM/yyyy")
            };
            //.StandardName;

            var url = "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabiListesi?";
            url += "Enlem=" + _konum.Enlem;
            url += "&Boylam=" + _konum.Boylam;
            url += "&Yukseklik=" + _konum.Yukseklik;
            url = url.Replace(',', '.');
            url += "&SaatBolgesi=" + TimeZoneInfo.Local.BaseUtcOffset.Hours; //.StandardName;
            url += "&yazSaati=" + (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0);
            url += "&Tarih=" + DateTime.Today.ToString("dd/MM/yyyy");

            //var client = new HttpClient();
            //string baseUrl = "http://servis.suleymaniyetakvimi.com/servis.asmx/";
            //client.BaseAddress = new Uri(baseUrl);

            //var response =
            //    await client.GetAsync(
            //            $"VakitHesabiListesi?Enlem={konum.Enlem}&Boylam={konum.Boylam}&Yukseklik={konum.Yukseklik}&SaatBolgesi={konum.SaatBolgesi}&yazSaati={konum.YazKis}&Tarih={konum.Tarih}")
            //        .ConfigureAwait(true);

            //var xmlResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            try
            {
                XDocument doc = XDocument.Load(url);
                _monthlyTakvim = ParseXmlList(doc, _konum.Enlem, _konum.Boylam, _konum.Yukseklik);
                WriteTakvimFile(doc.ToString());
                return _monthlyTakvim;
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"An error occurred while downloading or parsing the xml file, details: {exception.Message}");
            }

            return _monthlyTakvim;
            //var stream = GetType().Module.Assembly.GetManifestResourceStream("SuleymaniyeTakvimi.ayliktakvim.xml");
            //stream.
            //if (!string.IsNullOrEmpty(xmlResult) && xmlResult.StartsWith("<?xml"))
            //    MonthlyTakvim = ParseXmlList(xmlResult);
            //else
            //    UserDialogs.Instance.Toast("Namaz vakitlerini internetten alırken bir hata oluştu.",
            //        TimeSpan.FromSeconds(5));
        }

        private IList<Takvim> ParseXmlList(XDocument doc, double enlem=0.0, double boylam=0.0, double yukseklik=0.0)
        {
            IList<Takvim> monthlyTakvim = new ObservableCollection<Takvim>();
            //XDocument doc = XDocument.Parse(xmlResult);
            //if(doc.Descendants("Takvim")!=null)
            if (doc.Root == null) return monthlyTakvim;
            foreach (var item in doc.Root.Descendants())
            {
                if (item.Name.LocalName == "TakvimListesi")
                {
                    var takvimItem = new Takvim();
                    foreach (var subitem in item.Descendants())
                    {
                        switch (subitem.Name.LocalName)
                        {
                            case "Tarih":
                                takvimItem.Tarih = subitem.Value;
                                break;
                            case "Enlem":
                                takvimItem.Enlem = Convert.ToDouble(subitem.Value);
                                break;
                            case "Boylam":
                                takvimItem.Boylam = Convert.ToDouble(subitem.Value);
                                break;
                            case "Yukseklik":
                                takvimItem.Yukseklik = Convert.ToDouble(subitem.Value);
                                break;
                            case "SaatBolgesi":
                                takvimItem.SaatBolgesi = Convert.ToDouble(subitem.Value);
                                break;
                            case "YazKis":
                                takvimItem.YazKis = Convert.ToDouble(subitem.Value);
                                break;
                            case "FecriKazip":
                                takvimItem.FecriKazip = subitem.Value;
                                break;
                            case "FecriSadik":
                                takvimItem.FecriSadik = subitem.Value;
                                break;
                            case "SabahSonu":
                                takvimItem.SabahSonu = subitem.Value;
                                break;
                            case "Ogle":
                                takvimItem.Ogle = subitem.Value;
                                break;
                            case "Ikindi":
                                takvimItem.Ikindi = subitem.Value;
                                break;
                            case "Aksam":
                                takvimItem.Aksam = subitem.Value;
                                break;
                            case "Yatsi":
                                takvimItem.Yatsi = subitem.Value;
                                break;
                            case "YatsiSonu":
                                takvimItem.YatsiSonu = subitem.Value;
                                break;
                        }
                    }

                    takvimItem.Enlem = takvimItem.Enlem == 0 ? enlem : takvimItem.Enlem;
                    takvimItem.Boylam = takvimItem.Boylam == 0 ? boylam : takvimItem.Boylam;
                    takvimItem.Yukseklik = takvimItem.Yukseklik == 0 ? yukseklik : takvimItem.Yukseklik;
                    monthlyTakvim.Add(takvimItem);
                }
            }

            return monthlyTakvim;
        }

        public async void SetWeeklyAlarms()
        {
            Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            DependencyService.Get<IAlarmService>().CancelAlarm();
            //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
            //DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), 0, "Sabah Sonu");
            if (CheckRemindersEnabledAny())
            {
                try
                {
                    if (File.Exists(_fileName))
                    {
                        XDocument xmldoc = XDocument.Load(_fileName);
                        var enlem = _takvim.Enlem == 0 ? Preferences.Get("LastLatitude", 0.0) : _takvim.Enlem;
                        var boylam = _takvim.Boylam == 0 ? Preferences.Get("LastLongitude", 0.0) : _takvim.Boylam;
                        var yukseklik = _takvim.Yukseklik == 0 ? Preferences.Get("LastAltitude", 0.0) : _takvim.Yukseklik;
                        var takvims = ParseXmlList(xmldoc, enlem, boylam, yukseklik);
                        if (takvims != null && (DateTime.Parse(takvims[takvims.Count-1].Tarih) - DateTime.Today).Days > 3 && takvims[0].Enlem!=0)
                        {
                            //xmldoc = ReadTakvimFile();
                            //MonthlyTakvim = ParseXmlList(xmldoc);
                            _monthlyTakvim = takvims;
                        }
                        else
                        {
                            var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
                            if (location != null && location.Latitude != 0 && location.Longitude != 0)
                            {
                                _monthlyTakvim = GetMonthlyPrayerTimes(location, false);
                                if (_monthlyTakvim == null)
                                {
                                    await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
                                        AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
                                    return;
                                }
                            }
                        }
                    }
                    else
                    {
                        var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
                        if (location != null && location.Latitude != 0 && location.Longitude != 0)
                        {
                            _monthlyTakvim = GetMonthlyPrayerTimes(location, false);
                            if (_monthlyTakvim == null)
                            {
                                await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
                                    AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
                                return;
                            }
                        }
                    }

                    if (_monthlyTakvim!=null)
                    {
                        int dayCounter = 0;
                        foreach (Takvim todayTakvim in _monthlyTakvim)
                        {
                            if(DateTime.Parse(todayTakvim.Tarih)>=DateTime.Today){
                                var fecriKazipZaman = TimeSpan.Parse(todayTakvim.FecriKazip);
                                var fecriSadikZaman = TimeSpan.Parse(todayTakvim.FecriSadik);
                                var sabahSonuZaman = TimeSpan.Parse(todayTakvim.SabahSonu);
                                var ogleZaman = TimeSpan.Parse(todayTakvim.Ogle);
                                var ikindiZaman = TimeSpan.Parse(todayTakvim.Ikindi);
                                var aksamZaman = TimeSpan.Parse(todayTakvim.Aksam);
                                var yatsiZaman = TimeSpan.Parse(todayTakvim.Yatsi);
                                var yatsiSonuZaman = TimeSpan.Parse(todayTakvim.YatsiSonu);
                                var fecriKazip = DateTime.Parse(todayTakvim.Tarih) + fecriKazipZaman - TimeSpan.FromMinutes(Preferences.Get("fecrikazipBildirmeVakti", 0));
                                var fecriSadik = DateTime.Parse(todayTakvim.Tarih) + fecriSadikZaman - TimeSpan.FromMinutes(Preferences.Get("fecrisadikBildirmeVakti", 0));
                                var sabahSonu = DateTime.Parse(todayTakvim.Tarih) + sabahSonuZaman - TimeSpan.FromMinutes(Preferences.Get("sabahsonuBildirmeVakti", 0));
                                var ogle = DateTime.Parse(todayTakvim.Tarih) + ogleZaman - TimeSpan.FromMinutes(Preferences.Get("ogleBildirmeVakti", 0));
                                var ikindi = DateTime.Parse(todayTakvim.Tarih) + ikindiZaman - TimeSpan.FromMinutes(Preferences.Get("ikindiBildirmeVakti", 0));
                                var aksam = DateTime.Parse(todayTakvim.Tarih) + aksamZaman - TimeSpan.FromMinutes(Preferences.Get("aksamBildirmeVakti", 0));
                                var yatsi = DateTime.Parse(todayTakvim.Tarih) + yatsiZaman - TimeSpan.FromMinutes(Preferences.Get("yatsiBildirmeVakti", 0));
                                var yatsiSonu = DateTime.Parse(todayTakvim.Tarih) + yatsiSonuZaman - TimeSpan.FromMinutes(Preferences.Get("yatsisonuBildirmeVakti", 0));
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrikazip " + fecriKazip.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < fecriKazip));
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + fecriSadik.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < fecriSadik));
                                Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + sabahSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < sabahSonu));
                                Debug.WriteLine("TimeStamp-SetAlarms-ogle " + ogle.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < ogle));
                                Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + ikindi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < ikindi));
                                Debug.WriteLine("TimeStamp-SetAlarms-aksam " + aksam.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < aksam));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + yatsi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < yatsi));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + yatsiSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < yatsiSonu));
                                if (DateTime.Now < fecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), fecriKazipZaman, Preferences.Get("fecrikazipBildirmeVakti", 0), "Fecri Kazip");
                                if (DateTime.Now < fecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), fecriSadikZaman, Preferences.Get("fecrisadikBildirmeVakti", 0), "Fecri Sadık");
                                if (DateTime.Now < sabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), sabahSonuZaman, Preferences.Get("sabahsonuBildirmeVakti", 0), "Sabah Sonu");
                                if (DateTime.Now < ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), ogleZaman, Preferences.Get("ogleBildirmeVakti", 0), "Öğle");
                                if (DateTime.Now < ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), ikindiZaman, Preferences.Get("ikindiBildirmeVakti", 0), "İkindi");
                                if (DateTime.Now < aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), aksamZaman, Preferences.Get("aksamBildirmeVakti", 0), "Akşam");
                                if (DateTime.Now < yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), yatsiZaman, Preferences.Get("yatsiBildirmeVakti", 0), "Yatsı");
                                if (DateTime.Now < yatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), yatsiSonuZaman, Preferences.Get("yatsisonuBildirmeVakti", 0), "Yatsı Sonu");
                                dayCounter++;
                                if (dayCounter >= 15) break;
                            }
                        }
                    }
                    else
                    {
                        UserDialogs.Instance.Toast(AppResources.AylikTakvimeErisemedi);
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {exception.Message}");
                }

                Preferences.Set("LastAlarmDate", DateTime.Today.AddDays(7).ToShortDateString());
                //else
                //{
                //    Log.Warning("Get monthly prayer times failed in the SetWeeklyAlarm method", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                //}
            }
            //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
            Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }


        public bool HaveInternet()
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                UserDialogs.Instance.Toast(AppResources.TakvimIcinInternet, TimeSpan.FromSeconds(7));
                return false;
            }

            return true;
        }

        private void WriteTakvimFile(string fileContent)
        {
            //using (Stream stream = this.GetType().Assembly.
            //    GetManifestResourceStream("SuleymaniyeTakvimi.Assets.ayliktakvim.xml"))
            //{
            //    using (StreamWriter sr = new StreamWriter(stream))
            //    {
            //        sr.WriteAsync(fileContent);
            //    }
            //}
            File.WriteAllText(_fileName, fileContent);
        }

        //public XDocument ReadTakvimFile()
        //{
        //    //using (Stream stream = this.GetType().Assembly.
        //    //    GetManifestResourceStream("ayliktakvim.xml"))
        //    //{
        //    //    using (StreamReader sr = new StreamReader(stream))
        //    //    {
        //    //        result = sr.ReadToEnd();
        //    //    }
        //    //}
        //    var result = File.ReadAllText(FileName);
        //    var doc = XDocument.Parse(result);
        //    return doc;
        //}

        //public void CheckReminders()
        //{
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipBildiri", false)) SetNotification("fecrikazip", takvim.FecriKazip);
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikBildiri", false)) SetNotification("fecrisadik", takvim.FecriSadik);
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuBildiri", false)) SetNotification("sabahsonu", takvim.SabahSonu);
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleBildiri", false)) SetNotification("ogle", takvim.Ogle);
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiBildiri", false)) SetNotification("ikindi", takvim.Ikindi);
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamBildiri", false)) SetNotification("aksam", takvim.Aksam);
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiBildiri", false)) SetNotification("yatsi", takvim.Yatsi);
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuBildiri", false)) SetNotification("yatsisonu", takvim.YatsiSonu);
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipTitreme", false)) CheckVibration(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"));
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikTitreme", false)) CheckVibration(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"));
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuTitreme", false)) CheckVibration(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"));
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleTitreme", false)) CheckVibration(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"));
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiTitreme", false)) CheckVibration(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"));
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamTitreme", false)) CheckVibration(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"));
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiTitreme", false)) CheckVibration(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"));
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuTitreme", false)) CheckVibration(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"));
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipAlarm", false)) CheckAlarm(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"), "fecrikazip");
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikAlarm", false)) CheckAlarm(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"), "fecrisadik");
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuAlarm", false)) CheckAlarm(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"), "sabahsonu");
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleAlarm", false)) CheckAlarm(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"), "ogle");
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiAlarm", false)) CheckAlarm(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"), "ikindi");
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamAlarm", false)) CheckAlarm(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"), "aksam");
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiAlarm", false)) CheckAlarm(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"), "yatsi");
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuAlarm", false)) CheckAlarm(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"), "yatsisonu");
        //}
        //private async void CheckAlarm(string vakit, string dakikaFarki, string adi)
        //{
        //    var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
        //    kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
        //    if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
        //    {
        //        var alarmSesi = Preferences.Get(adi + "AlarmSesi", "kus");
        //        var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(alarmSesi+".wav").ConfigureAwait(false);
        //        CrossMediaManager.Current.Notification.ShowNavigationControls = false;
        //        CrossMediaManager.Current.Notification.ShowPlayPauseControls = true;
        //        CrossMediaManager.Current.MediaPlayer.ShowPlaybackControls = true;
        //        CrossMediaManager.Current.RepeatMode = RepeatMode.All;
        //        switch (adi)
        //        {
        //            case "fecrikazip":
        //                mediaItem.DisplayTitle = "Fecri Kazip Alarmı";
        //                break;
        //            case "fecrisadik":
        //                mediaItem.DisplayTitle = "Fecri Sadık Alarmı";
        //                break;
        //            case "sabahsonu":
        //                mediaItem.DisplayTitle = "Sabah Sonu Alarmı";
        //                break;
        //            case "ogle":
        //                mediaItem.DisplayTitle = "Öğle Alarmı";
        //                break;
        //            case "ikindi":
        //                mediaItem.DisplayTitle = "İkindi Alarmı";
        //                break;
        //            case "aksam":
        //                mediaItem.DisplayTitle = "Akşam Alarmı";
        //                break;
        //            case "yatsi":
        //                mediaItem.DisplayTitle = "Yatsı Alarmı";
        //                break;
        //            case "yatsisonu":
        //                mediaItem.DisplayTitle = "Yatsı Sonu Alarmı";
        //                break;
        //        }
        //        //ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
        //        //player.Load(GetStreamFromFile("ezan.mp3"));
        //        //player.Play();
        //    }
        //}
        //Stream GetStreamFromFile(string filename)
        //{
        //    var assembly = typeof(App).GetTypeInfo().Assembly;
        //    var stream = assembly.GetManifestResourceStream("Assets." + filename);
        //    return stream;
        //}

        //private void CheckVibration(string vakit, string dakikaFarki)
        //{
        //    var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
        //    kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
        //    if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
        //    {
        //        try
        //        {
        //            // Use default vibration length
        //            Vibration.Vibrate();

        //            // Or use specified time
        //            var duration = TimeSpan.FromSeconds(10);
        //            Vibration.Vibrate(duration);
        //        }
        //        catch (FeatureNotSupportedException ex)
        //        {
        //            UserDialogs.Instance.Alert(AppResources.TitremeyiDesteklemiyor + ex.Message, AppResources.CihazDesteklemiyor);
        //        }
        //        catch (Exception ex)
        //        {
        //            UserDialogs.Instance.Alert(ex.Message, AppResources.SorunCikti);
        //        }
        //    }
        //}

        //private void SetNotification(string adi, string vakit)
        //{
        //    var itemAdi = "";
        //    var notificationId = 101;
        //    var bildiriVakti = TimeSpan.Parse(vakit) - TimeSpan.FromMinutes(Convert.ToDouble(Preferences.Get(adi + "BildirmeVakti", 0.0)));
        //    var tamVakit = DateTime.Parse(bildiriVakti.ToString());
        //    if (tamVakit > DateTime.Now)
        //    {
        //        switch (adi)
        //        {
        //            case "fecrikazip":
        //                itemAdi = "Fecri Kazip";
        //                notificationId = 1003;
        //                break;
        //            case "fecrisadik":
        //                itemAdi = "Fecri Sadık";
        //                notificationId = 1004;
        //                break;
        //            case "sabahsonu":
        //                itemAdi = "Sabah Sonu";
        //                notificationId = 1005;
        //                break;
        //            case "ogle":
        //                itemAdi = "Öğle";
        //                notificationId = 1006;
        //                break;
        //            case "ikindi":
        //                itemAdi = "İkindi";
        //                notificationId = 1007;
        //                break;
        //            case "aksam":
        //                itemAdi = "Akşam";
        //                notificationId = 1008;
        //                break;
        //            case "yatsi":
        //                itemAdi = "Yatsı";
        //                notificationId = 1009;
        //                break;
        //            case "yatsisonu":
        //                itemAdi = "Yatsı Sonu";
        //                notificationId = 1010;
        //                break;
        //        }

        //        //var notification = new NotificationRequest
        //        //{
        //        //    NotificationId = 100,
        //        //    Title = itemAdi + " Vakti Bildirimi",
        //        //    Description = itemAdi + " Vakti: " + Vakit,
        //        //    ReturningData = itemAdi + " Bildirimi", // Returning data when tapped on notification.
        //        //    NotifyTime = DateTime.Parse(bildiriVakti.ToString())//DateTime.Now.AddSeconds(10) // Used for Scheduling local notification, if not specified notification will show immediately.
        //        //};
        //        //NotificationCenter.Current.Show(notification);
        //        //var notification = new Notification
        //        //{
        //        //    Id = new Random().Next(101,999),
        //        //    Title = $"{itemAdi} Vakti Bildirimi",
        //        //    Message = $"{itemAdi} Vakti: {bildiriVakti}",
        //        //    ScheduleDate = DateTime.Parse(bildiriVakti.ToString()),
        //        //};
        //        //ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
        //        CrossLocalNotifications.Current.Show($"{itemAdi} {AppResources.VaktiHatirlatmasi}",
        //            $"{itemAdi} {AppResources.Vakti} {bildiriVakti}",
        //            notificationId, tamVakit);
        //    }
        //}

        //public void SetAlarms()
        //{
        //    Analytics.TrackEvent("SetAlarms in the DataService");
        //    Log.Warning("TimeStamp-SetAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //    DependencyService.Get<IAlarmService>().CancelAlarm();
        //    if (CheckRemindersEnabledAny())
        //    {
        //        //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
        //        //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");
        //        Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + DateTime.Parse(takvim.FecriKazip).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriKazip)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + DateTime.Parse(takvim.FecriSadik).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriSadik)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + DateTime.Parse(takvim.SabahSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.SabahSonu)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-ogle " + DateTime.Parse(takvim.Ogle).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ogle)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + DateTime.Parse(takvim.Ikindi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ikindi)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-aksam " + DateTime.Parse(takvim.Aksam).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Aksam)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + DateTime.Parse(takvim.Yatsi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Yatsi)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + DateTime.Parse(takvim.YatsiSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.YatsiSonu)));
        //        if (DateTime.Now < DateTime.Parse(takvim.FecriKazip) && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriKazip), 0, "Fecri Kazip");
        //        if (DateTime.Now < DateTime.Parse(takvim.FecriSadik) && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriSadik), 0, "Fecri Sadık");
        //        if (DateTime.Now < DateTime.Parse(takvim.SabahSonu) && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.SabahSonu), 0, "Sabah Sonu");
        //        if (DateTime.Now < DateTime.Parse(takvim.Ogle) && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ogle), 0, "Öğle");
        //        if (DateTime.Now < DateTime.Parse(takvim.Ikindi) && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ikindi), 0, "İkindi");
        //        if (DateTime.Now < DateTime.Parse(takvim.Aksam) && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Aksam), 0, "Akşam");
        //        if (DateTime.Now < DateTime.Parse(takvim.Yatsi) && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Yatsi), 0, "Yatsı");
        //        if (DateTime.Now < DateTime.Parse(takvim.YatsiSonu) && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.YatsiSonu), 0, "Yatsı Sonu");
        //    }
        //    //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
        //    Log.Warning("TimeStamp-SetAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //}
        /*
                public async void SetMonthlyAlarms()
                {
                    Log.Warning("TimeStamp-SetMonthlyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                    DependencyService.Get<IAlarmService>().CancelAlarm();
                    //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                    //DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), "test");
                    if (CheckRemindersEnabledAny())
                    {
                        //konum = GetCurrentLocation().Result;
                        var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
                        if (location != null && location.Latitude != 0 && location.Longitude != 0)
                        {
                            MonthlyTakvim = GetMonthlyPrayerTimes(location, false);
                            if (MonthlyTakvim == null)
                            {
                                await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
                                    AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
                                return;
                            }
                            foreach (Takvim todayTakvim in MonthlyTakvim)
                            {
                                var fecriKazip = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriKazip);
                                var fecriSadik = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik);
                                var sabahSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu);
                                var ogle = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle);
                                var ikindi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi);
                                var aksam = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam);
                                var yatsi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi);
                                var yatsiSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu);
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + (DateTime.Parse(todayTakvim.Tarih)+TimeSpan.Parse(todayTakvim.FecriKazip)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < fecriKazip));
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < fecriSadik));
                                Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < sabahSonu));
                                Debug.WriteLine("TimeStamp-SetAlarms-ogle " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < ogle));
                                Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < ikindi));
                                Debug.WriteLine("TimeStamp-SetAlarms-aksam " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < aksam));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < yatsi));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < yatsiSonu));
                                if (DateTime.Now < fecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriKazip), 0, "Fecri Kazip");
                                if (DateTime.Now < fecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriSadik), 0, "Fecri Sadık");
                                if (DateTime.Now < sabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.SabahSonu), 0, "Sabah Sonu");
                                if (DateTime.Now < ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ogle), 0, "Öğle");
                                if (DateTime.Now < ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ikindi), 0, "İkindi");
                                if (DateTime.Now < aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Aksam), 0, "Akşam");
                                if (DateTime.Now < yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Yatsi), 0, "Yatsı");
                                if (DateTime.Now < yatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.YatsiSonu), 0, "Yatsı Sonu");
                            }
                        }
                        else
                        {
                            Log.Warning("Get monthly prayer times failed in the SetMonthlyAlarm method",DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                            UserDialogs.Instance.Alert("Uygulamaya konuma erişme izni verildiğini ve konum hizmetinin açık olduğunu kontrol edin!",
                                "Konuma Erişmeye Çalışırken Hata Oluştu");
                        }
                        //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                        //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");

                    }
                    //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
                    Log.Warning("TimeStamp-SetMonthlyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                }
        */
    }
}