using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Playback;
using Microsoft.AppCenter.Analytics;
//using Plugin.LocalNotification;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace SuleymaniyeTakvimi.Services
{

    public class DataService : IDataService
    {
        public Takvim konum;
        public Takvim takvim;
        public IList<Takvim> MonthlyTakvim;
        public DataService()
        {
            takvim = new Takvim()
            {
                Enlem = Preferences.Get("enlem", 41.0056),
                Boylam = Preferences.Get("boylam", 28.9767),
                Yukseklik = Preferences.Get("yukseklik", 4.0),
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

        public async Task<Takvim> GetCurrentLocation()
        {
            Analytics.TrackEvent("GetCurrentLocation in the DataService");
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                konum = new Takvim();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
                }
                if (location != null)
                {
                    Console.WriteLine(
                        $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    konum.Enlem = location.Latitude;
                    konum.Boylam = location.Longitude;
                    konum.Yukseklik = location.Altitude ?? 0;
                    Preferences.Set("LastLatitude", konum.Enlem);
                    Preferences.Set("LastLongitude", konum.Boylam);
                    Preferences.Set("LastAltitude", konum.Yukseklik);
                }
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Handle not supported on device exception
                Console.WriteLine(fnsEx.Message);
            }
            catch (FeatureNotEnabledException fneEx)
            {
                // Handle not enabled on device exception
                Console.WriteLine(fneEx.Message);
                UserDialogs.Instance.Alert("Cihazda konum hizmetleri etkin değil. Öce konum hizmetlerini açmanız lazım!",
                    "Konum Hizmetleri Kapalı");
                //await App.Current.MainPage.DisplayAlert("Konum Servisi Hatası", "Cihazın konum servisi kapalı, Öce konum servisini açmanız lazım!", "Tamam");
                if (Preferences.Get("LastLatitude", 0.0) != 0.0 && Preferences.Get("LastLongitude", 0.0) != 0.0)
                {
                    konum.Enlem = Preferences.Get("LastLatitude", 0.0);
                    konum.Boylam = Preferences.Get("LastLongitude", 0.0);
                    konum.Yukseklik = Preferences.Get("LastAltitude", 0.0);
                }
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine(pEx.Message);
                //await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
                UserDialogs.Instance.Alert("Uygulamanın normal çalışması için Konuma erişme yetkisi vermeniz lazım!",
                    "Konuma Erişme İzni Yok");
                //await App.Current.MainPage.DisplayAlert("Konum Servisi İzni Yok", "Uygulamanın normal çalışması için Konuma erişme yetkisi vermeniz lazım!", "Tamam");
            }
            catch (Exception ex)
            {
                // Unable to get location
                Console.WriteLine(ex.Message);
            }

            return konum;
        }

        public Takvim VakitHesabi()
        {
            Analytics.TrackEvent("VakitHesabi in the DataService");
            CheckInternet();
            if (konum != null)
            {
                var url = "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi?";
                url += "Enlem=" + konum.Enlem;
                url += "&Boylam=" + konum.Boylam;
                url += "&Yukseklik=" + konum.Yukseklik;
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
                takvim = new Takvim();
                XDocument doc = XDocument.Load(url);
                foreach (var item in doc.Root.Descendants())
                {
                    switch (item.Name.LocalName)
                    {
                        //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
                        case "Enlem":
                            takvim.Enlem = Convert.ToDouble(item.Value,CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "Boylam":
                            takvim.Boylam = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "Yukseklik":
                            takvim.Yukseklik = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "SaatBolgesi":
                            takvim.SaatBolgesi = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "YazKis":
                            takvim.YazKis = Convert.ToDouble(item.Value, CultureInfo.InvariantCulture.NumberFormat);
                            break;
                        case "FecriKazip":
                            takvim.FecriKazip = item.Value;
                            break;
                        case "FecriSadik":
                            takvim.FecriSadik = item.Value;
                            break;
                        case "SabahSonu":
                            takvim.SabahSonu = item.Value;
                            break;
                        case "Ogle":
                            takvim.Ogle = item.Value;
                            break;
                        case "Ikindi":
                            takvim.Ikindi = item.Value;
                            break;
                        case "Aksam":
                            takvim.Aksam = item.Value;
                            break;
                        case "Yatsi":
                            takvim.Yatsi = item.Value;
                            break;
                        case "YatsiSonu":
                            takvim.YatsiSonu = item.Value;
                            break;
                    }
                }
            }

            return takvim;
        }

        public async Task<Takvim> GetPrayerTimes(Location location)
        {
            Analytics.TrackEvent("GetPrayerTimes in the DataService");
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            CheckInternet();
            konum =new Takvim();
            konum.Enlem = location.Latitude;
            konum.Boylam = location.Longitude;
            konum.Yukseklik = location.Altitude ?? 0;
            konum.SaatBolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours;//.StandardName;
            konum.YazKis = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0;
            konum.Tarih = DateTime.Today.ToString("dd/MM/yyyy");

            var client = new HttpClient();
            string baseUrl = "http://servis.suleymaniyetakvimi.com/servis.asmx/";
            client.BaseAddress = new Uri(baseUrl);

            var response =
                await client.GetAsync(
                        $"VakitHesabi?Enlem={konum.Enlem}&Boylam={konum.Boylam}&Yukseklik={konum.Yukseklik}&SaatBolgesi={konum.SaatBolgesi}&yazSaati={konum.YazKis}&Tarih={konum.Tarih}")
                    .ConfigureAwait(false);

            var xmlResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!string.IsNullOrEmpty(xmlResult) && xmlResult.StartsWith("<?xml"))
                takvim = ParseXml(xmlResult);
            else
                UserDialogs.Instance.Toast("Namaz vakitlerini internetten alırken bir hata oluştu.",
                    TimeSpan.FromSeconds(5));

            //var xmlResult = Newtonsoft.Json.JsonConvert.DeserializeObject<Takvim>(responseResult);
            //var xmlResult = baseUrl
            //    .SetQueryParameters(new
            //    {
            //        Enlem = konum.Enlem, Boylam = konum.Boylam, Yukseklik = konum.Yukseklik,
            //        SaatBolgesi = konum.SaatBolgesi, yazSaati = konum.YazKis, Tarih = konum.Tarih
            //    })
            //    .GetJSonAsync<Takvim>().Result;
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            return takvim;
        }

        private Takvim ParseXml(string xmlResult)
        {
            takvim = new Takvim();
            XDocument doc = XDocument.Parse(xmlResult);
            //if(doc.Descendants("Takvim")!=null)
            foreach (var item in doc.Root.Descendants())
            {
                switch (item.Name.LocalName)
                {
                    case "Enlem":
                        takvim.Enlem = Convert.ToDouble(item.Value);
                        break;
                    case "Boylam":
                        takvim.Boylam = Convert.ToDouble(item.Value);
                        break;
                    case "Yukseklik":
                        takvim.Yukseklik = Convert.ToDouble(item.Value);
                        break;
                    case "SaatBolgesi":
                        takvim.SaatBolgesi = Convert.ToDouble(item.Value);
                        break;
                    case "YazKis":
                        takvim.YazKis = Convert.ToDouble(item.Value);
                        break;
                    case "FecriKazip":
                        takvim.FecriKazip = item.Value;
                        break;
                    case "FecriSadik":
                        takvim.FecriSadik = item.Value;
                        break;
                    case "SabahSonu":
                        takvim.SabahSonu = item.Value;
                        break;
                    case "Ogle":
                        takvim.Ogle = item.Value;
                        break;
                    case "Ikindi":
                        takvim.Ikindi = item.Value;
                        break;
                    case "Aksam":
                        takvim.Aksam = item.Value;
                        break;
                    case "Yatsi":
                        takvim.Yatsi = item.Value;
                        break;
                    case "YatsiSonu":
                        takvim.YatsiSonu = item.Value;
                        break;
                }
            }

            return takvim;
        }
        public void CheckReminders()
        {
            if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipBildiri", false)) SetNotification("fecrikazip", takvim.FecriKazip);
            if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikBildiri", false)) SetNotification("fecrisadik", takvim.FecriSadik);
            if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuBildiri", false)) SetNotification("sabahsonu", takvim.SabahSonu);
            if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleBildiri", false)) SetNotification("ogle", takvim.Ogle);
            if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiBildiri", false)) SetNotification("ikindi", takvim.Ikindi);
            if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamBildiri", false)) SetNotification("aksam", takvim.Aksam);
            if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiBildiri", false)) SetNotification("yatsi", takvim.Yatsi);
            if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuBildiri", false)) SetNotification("yatsisonu", takvim.YatsiSonu);
            if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipTitreme", false)) CheckVibration(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"));
            if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikTitreme", false)) CheckVibration(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"));
            if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuTitreme", false)) CheckVibration(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"));
            if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleTitreme", false)) CheckVibration(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"));
            if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiTitreme", false)) CheckVibration(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"));
            if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamTitreme", false)) CheckVibration(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"));
            if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiTitreme", false)) CheckVibration(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"));
            if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuTitreme", false)) CheckVibration(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"));
            if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipAlarm", false)) CheckAlarm(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"), "fecrikazip");
            if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikAlarm", false)) CheckAlarm(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"), "fecrisadik");
            if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuAlarm", false)) CheckAlarm(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"), "sabahsonu");
            if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleAlarm", false)) CheckAlarm(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"), "ogle");
            if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiAlarm", false)) CheckAlarm(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"), "ikindi");
            if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamAlarm", false)) CheckAlarm(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"), "aksam");
            if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiAlarm", false)) CheckAlarm(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"), "yatsi");
            if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuAlarm", false)) CheckAlarm(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"), "yatsisonu");
        }

        public bool CheckRemindersEnabledAny()
        {
            return Preferences.Get("fecrikazipEtkin", false) || Preferences.Get("fecrisadikEtkin", false) ||
                   Preferences.Get("sabahsonuEtkin", false) || Preferences.Get("ogleEtkin", false) ||
                   Preferences.Get("ikindiEtkin", false) || Preferences.Get("aksamEtkin", false) ||
                   Preferences.Get("yatsiEtkin", false) || Preferences.Get("yatsisonuEtkin", false);
        }
        private async void CheckAlarm(string vakit, string dakikaFarki, string adi)
        {
            var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
            kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
            if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
            {
                var alarmSesi = Preferences.Get(adi + "AlarmSesi", "kus");
                var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(alarmSesi+".wav").ConfigureAwait(false);
                CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                CrossMediaManager.Current.Notification.ShowPlayPauseControls = true;
                CrossMediaManager.Current.MediaPlayer.ShowPlaybackControls = true;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                switch (adi)
                {
                    case "fecrikazip":
                        mediaItem.DisplayTitle = "Fecri Kazip Alarmı";
                        break;
                    case "fecrisadik":
                        mediaItem.DisplayTitle = "Fecri Sadık Alarmı";
                        break;
                    case "sabahsonu":
                        mediaItem.DisplayTitle = "Sabah Sonu Alarmı";
                        break;
                    case "ogle":
                        mediaItem.DisplayTitle = "Öğle Alarmı";
                        break;
                    case "ikindi":
                        mediaItem.DisplayTitle = "İkindi Alarmı";
                        break;
                    case "aksam":
                        mediaItem.DisplayTitle = "Akşam Alarmı";
                        break;
                    case "yatsi":
                        mediaItem.DisplayTitle = "Yatsı Alarmı";
                        break;
                    case "yatsisonu":
                        mediaItem.DisplayTitle = "Yatsı Sonu Alarmı";
                        break;
                }
                //ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
                //player.Load(GetStreamFromFile("ezan.mp3"));
                //player.Play();
            }
        }
        //Stream GetStreamFromFile(string filename)
        //{
        //    var assembly = typeof(App).GetTypeInfo().Assembly;
        //    var stream = assembly.GetManifestResourceStream("Assets." + filename);
        //    return stream;
        //}

        private void CheckVibration(string vakit, string dakikaFarki)
        {
            var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
            kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
            if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
            {
                try
                {
                    // Use default vibration length
                    Vibration.Vibrate();

                    // Or use specified time
                    var duration = TimeSpan.FromSeconds(10);
                    Vibration.Vibrate(duration);
                }
                catch (FeatureNotSupportedException ex)
                {
                    UserDialogs.Instance.Alert("Cihazınız titretmeyi desteklemiyor. " + ex.Message,
                        "Cihaz desteklemiyor");
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Alert(ex.Message, "Bir sorunla karşılaştık");
                }
            }
        }

        private void SetNotification(string Adi, string vakit)
        {
            var itemAdi = "";
            var NotificationId = 101;
            var bildiriVakti = TimeSpan.Parse(vakit) - TimeSpan.FromMinutes(Convert.ToDouble(Preferences.Get(Adi + "BildirmeVakti", 0.0)));
            var tamVakit = DateTime.Parse(bildiriVakti.ToString());
            if (tamVakit > DateTime.Now)
            {
                switch (Adi)
                {
                    case "fecrikazip":
                        itemAdi = "Fecri Kazip";
                        NotificationId = 1003;
                        break;
                    case "fecrisadik":
                        itemAdi = "Fecri Sadık";
                        NotificationId = 1004;
                        break;
                    case "sabahsonu":
                        itemAdi = "Sabah Sonu";
                        NotificationId = 1005;
                        break;
                    case "ogle":
                        itemAdi = "Öğle";
                        NotificationId = 1006;
                        break;
                    case "ikindi":
                        itemAdi = "İkindi";
                        NotificationId = 1007;
                        break;
                    case "aksam":
                        itemAdi = "Akşam";
                        NotificationId = 1008;
                        break;
                    case "yatsi":
                        itemAdi = "Yatsı";
                        NotificationId = 1009;
                        break;
                    case "yatsisonu":
                        itemAdi = "Yatsı Sonu";
                        NotificationId = 1010;
                        break;
                }

                //var notification = new NotificationRequest
                //{
                //    NotificationId = 100,
                //    Title = itemAdi + " Vakti Bildirimi",
                //    Description = itemAdi + " Vakti: " + Vakit,
                //    ReturningData = itemAdi + " Bildirimi", // Returning data when tapped on notification.
                //    NotifyTime = DateTime.Parse(bildiriVakti.ToString())//DateTime.Now.AddSeconds(10) // Used for Scheduling local notification, if not specified notification will show immediately.
                //};
                //NotificationCenter.Current.Show(notification);
                //var notification = new Notification
                //{
                //    Id = new Random().Next(101,999),
                //    Title = $"{itemAdi} Vakti Bildirimi",
                //    Message = $"{itemAdi} Vakti: {bildiriVakti}",
                //    ScheduleDate = DateTime.Parse(bildiriVakti.ToString()),
                //};
                //ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
                CrossLocalNotifications.Current.Show($"{itemAdi} Vakti Hatırlatması",
                    $"{itemAdi} Vakti: {bildiriVakti}",
                    NotificationId, tamVakit);
            }
        }


        public void GetMonthlyPrayerTimes(Location location)
        {
            Analytics.TrackEvent("GetMonthlyPrayerTimes in the DataService");
            CheckInternet();
            konum = new Takvim();
            konum.Enlem = location.Latitude;
            konum.Boylam = location.Longitude;
            konum.Yukseklik = location.Altitude ?? 0;
            konum.SaatBolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours;//.StandardName;
            konum.YazKis = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0;
            konum.Tarih = DateTime.Today.ToString("dd/MM/yyyy");

            var url = "http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabiListesi?";
            url += "Enlem=" + konum.Enlem;
            url += "&Boylam=" + konum.Boylam;
            url += "&Yukseklik=" + konum.Yukseklik;
            url = url.Replace(',', '.');
            url += "&SaatBolgesi=" + TimeZoneInfo.Local.BaseUtcOffset.Hours;//.StandardName;
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
            XDocument doc = XDocument.Load(url);
            MonthlyTakvim = ParseXmlList(doc);
            //var stream = GetType().Module.Assembly.GetManifestResourceStream("SuleymaniyeTakvimi.ayliktakvim.xml");
            //stream.
            //if (!string.IsNullOrEmpty(xmlResult) && xmlResult.StartsWith("<?xml"))
            //    MonthlyTakvim = ParseXmlList(xmlResult);
            //else
            //    UserDialogs.Instance.Toast("Namaz vakitlerini internetten alırken bir hata oluştu.",
            //        TimeSpan.FromSeconds(5));
        }

        private IList<Takvim> ParseXmlList(XDocument doc)
        {
            Takvim TakvimItem;
            IList<Takvim> monthlyTakvim = new ObservableCollection<Takvim>();
            //XDocument doc = XDocument.Parse(xmlResult);
            //if(doc.Descendants("Takvim")!=null)
            foreach (var item in doc.Root.Descendants())
            {
                if (item.Name.LocalName == "TakvimListesi")
                {
                    TakvimItem=new Takvim();
                    foreach (var subitem in item.Descendants())
                    {
                        switch (subitem.Name.LocalName)
                        {
                            case "Tarih":
                                TakvimItem.Tarih = subitem.Value;
                                break;
                            case "Enlem":
                                TakvimItem.Enlem = Convert.ToDouble(subitem.Value);
                                break;
                            case "Boylam":
                                TakvimItem.Boylam = Convert.ToDouble(subitem.Value);
                                break;
                            case "Yukseklik":
                                TakvimItem.Yukseklik = Convert.ToDouble(subitem.Value);
                                break;
                            case "SaatBolgesi":
                                TakvimItem.SaatBolgesi = Convert.ToDouble(subitem.Value);
                                break;
                            case "YazKis":
                                TakvimItem.YazKis = Convert.ToDouble(subitem.Value);
                                break;
                            case "FecriKazip":
                                TakvimItem.FecriKazip = subitem.Value;
                                break;
                            case "FecriSadik":
                                TakvimItem.FecriSadik = subitem.Value;
                                break;
                            case "SabahSonu":
                                TakvimItem.SabahSonu = subitem.Value;
                                break;
                            case "Ogle":
                                TakvimItem.Ogle = subitem.Value;
                                break;
                            case "Ikindi":
                                TakvimItem.Ikindi = subitem.Value;
                                break;
                            case "Aksam":
                                TakvimItem.Aksam = subitem.Value;
                                break;
                            case "Yatsi":
                                TakvimItem.Yatsi = subitem.Value;
                                break;
                            case "YatsiSonu":
                                TakvimItem.YatsiSonu = subitem.Value;
                                break;
                        }
                    }

                    monthlyTakvim.Add(TakvimItem);
                }

            }

            return monthlyTakvim;
        }

        public void SetAlarms()
        {
            Analytics.TrackEvent("SetAlarms in the DataService");
            Log.Warning("TimeStamp-SetAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            DependencyService.Get<IAlarmService>().CancelAlarm();
            if (CheckRemindersEnabledAny())
            {
                //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");
                Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + DateTime.Parse(takvim.FecriKazip).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriKazip)));
                Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + DateTime.Parse(takvim.FecriSadik).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriSadik)));
                Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + DateTime.Parse(takvim.SabahSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.SabahSonu)));
                Debug.WriteLine("TimeStamp-SetAlarms-ogle " + DateTime.Parse(takvim.Ogle).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ogle)));
                Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + DateTime.Parse(takvim.Ikindi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ikindi)));
                Debug.WriteLine("TimeStamp-SetAlarms-aksam " + DateTime.Parse(takvim.Aksam).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Aksam)));
                Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + DateTime.Parse(takvim.Yatsi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Yatsi)));
                Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + DateTime.Parse(takvim.YatsiSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.YatsiSonu)));
                if (DateTime.Now < DateTime.Parse(takvim.FecriKazip) && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriKazip), "Fecri Kazip");
                if (DateTime.Now < DateTime.Parse(takvim.FecriSadik) && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriSadik), "Fecri Sadık");
                if (DateTime.Now < DateTime.Parse(takvim.SabahSonu) && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.SabahSonu), "Sabah Sonu");
                if (DateTime.Now < DateTime.Parse(takvim.Ogle) && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ogle), "Öğle");
                if (DateTime.Now < DateTime.Parse(takvim.Ikindi) && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ikindi), "İkindi");
                if (DateTime.Now < DateTime.Parse(takvim.Aksam) && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Aksam), "Akşam");
                if (DateTime.Now < DateTime.Parse(takvim.Yatsi) && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Yatsi), "Yatsı");
                if (DateTime.Now < DateTime.Parse(takvim.YatsiSonu) && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.YatsiSonu), "Yatsı Sonu");
            }
            //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
            Log.Warning("TimeStamp-SetAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        public void SetWeeklyAlarms()
        {
            Log.Warning("TimeStamp-SetWeeklyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            DependencyService.Get<IAlarmService>().CancelAlarm();
            var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
            DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), "test");
            if (CheckRemindersEnabledAny())
            {
                konum = GetCurrentLocation().Result;
                if (konum != null && konum.Enlem > 0 && konum.Boylam > 0)
                {
                    GetMonthlyPrayerTimes(new Location(konum.Enlem, konum.Boylam, konum.Yukseklik));
                    int dayCounter = 0;
                    foreach (Takvim todayTakvim in MonthlyTakvim)
                    {
                        var FecriKazip = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriKazip);
                        var FecriSadik = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik);
                        var SabahSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu);
                        var Ogle = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle);
                        var Ikindi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi);
                        var Aksam = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam);
                        var Yatsi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi);
                        var YatsiSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu);
                        Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + FecriKazip.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < FecriKazip));
                        Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + FecriSadik.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < FecriSadik));
                        Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + SabahSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < SabahSonu));
                        Debug.WriteLine("TimeStamp-SetAlarms-ogle " + Ogle.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < Ogle));
                        Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + Ikindi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < Ikindi));
                        Debug.WriteLine("TimeStamp-SetAlarms-aksam " + Aksam.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < Aksam));
                        Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + Yatsi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < Yatsi));
                        Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + YatsiSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < YatsiSonu));
                        if (DateTime.Now < FecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriKazip), "Fecri Kazip");
                        if (DateTime.Now < FecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriSadik), "Fecri Sadık");
                        if (DateTime.Now < SabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.SabahSonu), "Sabah Sonu");
                        if (DateTime.Now < Ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ogle), "Öğle");
                        if (DateTime.Now < Ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ikindi), "İkindi");
                        if (DateTime.Now < Aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Aksam), "Akşam");
                        if (DateTime.Now < Yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Yatsi), "Yatsı");
                        if (DateTime.Now < YatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.YatsiSonu), "Yatsı Sonu");
                        dayCounter++;
                        if (dayCounter >= 7) break;
                    }
                }
                else
                {
                    Log.Warning("Get monthly prayer times failed in the SetWeeklyAlarm method", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                }
            }
            //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
            Log.Warning("TimeStamp-SetWeeklyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        public void SetMonthlyAlarms()
        {
            Log.Warning("TimeStamp-SetMonthlyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            DependencyService.Get<IAlarmService>().CancelAlarm();
            var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
            DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), "test");
            if (CheckRemindersEnabledAny())
            {
                konum = GetCurrentLocation().Result;
                if (konum != null && konum.Enlem > 0 && konum.Boylam > 0)
                {
                    GetMonthlyPrayerTimes(new Location(konum.Enlem, konum.Boylam, konum.Yukseklik));
                    foreach (Takvim todayTakvim in MonthlyTakvim)
                    {
                        var FecriKazip = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriKazip);
                        var FecriSadik = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik);
                        var SabahSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu);
                        var Ogle = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle);
                        var Ikindi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi);
                        var Aksam = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam);
                        var Yatsi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi);
                        var YatsiSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu);
                        Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + (DateTime.Parse(todayTakvim.Tarih)+TimeSpan.Parse(todayTakvim.FecriKazip)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < FecriKazip));
                        Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < FecriSadik));
                        Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < SabahSonu));
                        Debug.WriteLine("TimeStamp-SetAlarms-ogle " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < Ogle));
                        Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < Ikindi));
                        Debug.WriteLine("TimeStamp-SetAlarms-aksam " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < Aksam));
                        Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < Yatsi));
                        Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < YatsiSonu));
                        if (DateTime.Now < FecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriKazip), "Fecri Kazip");
                        if (DateTime.Now < FecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriSadik), "Fecri Sadık");
                        if (DateTime.Now < SabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.SabahSonu), "Sabah Sonu");
                        if (DateTime.Now < Ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ogle), "Öğle");
                        if (DateTime.Now < Ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ikindi), "İkindi");
                        if (DateTime.Now < Aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Aksam), "Akşam");
                        if (DateTime.Now < Yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Yatsi), "Yatsı");
                        if (DateTime.Now < YatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.YatsiSonu), "Yatsı Sonu");
                    }
                }
                else
                {
                    Log.Warning("Get monthly prayer times failed in the SetMonthlyAlarm method",DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                }
                //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");
                
            }
            //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
            Log.Warning("TimeStamp-SetMonthlyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private static void CheckInternet()
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                UserDialogs.Instance.Toast(
                    "Namaz vakitlerini yenilemek için internete bağlı olmanız gerekiyor, WiFi yada Mobil veriyi açın ve 'KONUMU YENİLE' yi tıklayın.",
                    TimeSpan.FromSeconds(7));
            }
        }
    }
}