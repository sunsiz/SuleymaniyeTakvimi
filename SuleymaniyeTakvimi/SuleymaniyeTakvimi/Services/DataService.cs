using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Acr.UserDialogs;
using MediaManager;
using Plugin.LocalNotification;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Services
{

    public class DataService : IDataService
    {
        //readonly List<Item> items;
        public Takvim konum;
        public Takvim takvim;
        //public Command GetLocationCommand { get; }


        //protected override void OnDisappearing()
        //{
        //    if (cts != null && !cts.IsCancellationRequested)
        //        cts.Cancel();
        //    base.OnDisappearing();
        //}
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
            //var latitude = Application.Current.Properties.ContainsKey("latitude")?Application.Current.Properties["latitude"].ToString():"";
            //var longitude = Preferences.ContainsKey("longitude") ? Preferences.Get("longitude","") : "";
            //takvim = Application.Current.Properties.ContainsKey("takvim")
            //    ? Application.Current.Properties["takvim"] as Takvim
            //    : new Takvim()
            //    {
            //        Enlem = 50.87,
            //        Boylam = 4.33,
            //        Yukseklik = 4,
            //        SaatBolgesi = 1,
            //        YazKis = 0,
            //        FecriKazip = "06:28",
            //        FecriSadik = "07:16",
            //        SabahSonu = "08:00",
            //        Ogle = "12:59",
            //        Ikindi = "15:27",
            //        Aksam = "17:54",
            //        Yatsi = "18:41",
            //        YatsiSonu = "19:31",
            //        Tarih = DateTime.Today.ToString("dd/MM/yyyy")
            //        //Enlem = 41, Boylam = 28.9, Yukseklik = 4, SaatBolgesi = 3, YazKis = 0, FecriKazip = "06:32",
            //        //FecriSadik = "07:19", SabahSonu = "08:03", Ogle = "13:21", Ikindi = "16:09", Aksam = "", Yatsi = "",
            //        //YatsiSonu = ""
            //    };
            //GetLocationCommand = new Command(async () => GetCurrentLocation());
            //GetCurrentLocation().Wait();
            //takvim = VakitHesabi();
            //items = new List<Item>()
            //{
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "First item", Description="This is an item description." },
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "Second item", Description="This is an item description." },
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "Third item", Description="This is an item description." },
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "Fourth item", Description="This is an item description." },
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "Fifth item", Description="This is an item description." },
            //    new Item { Id = Guid.NewGuid().ToString(), Text = "Sixth item", Description="This is an item description." }
            //};
        }

        //public async Task<bool> AddItemAsync(Item item)
        //{
        //    items.Add(item);

        //    return await Task.FromResult(true);
        //}

        //public async Task<bool> UpdateItemAsync(Item item)
        //{
        //    //var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
        //    //items.Remove(oldItem);
        //    items.Add(item);

        //    return await Task.FromResult(true);
        //}

        //public async Task<bool> DeleteItemAsync(string id)
        //{
        //    //var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
        //    //items.Remove(oldItem);

        //    return await Task.FromResult(true);
        //}

        //public async Task<Item> GetItemAsync(string adi)
        //{
        //    return await Task.FromResult(items.FirstOrDefault(s => s.Adi == adi));
        //}

        //public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        //{
        //    return await Task.FromResult(items);
        //}


        public async Task<Takvim> GetCurrentLocation()
        {
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
                //if(doc.Descendants("Takvim")!=null)
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
            //else
            //{
            //    var location = await Geolocation.GetLastKnownLocationAsync();
            //    if (location != null)
            //    {
            //        Console.WriteLine(
            //            $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
            //        konum = new Takvim();
            //        konum.Enlem = location.Latitude;
            //        konum.Boylam = location.Longitude;
            //        konum.Yukseklik = location.Altitude ?? 0;
            //    }
            //}

            return takvim;/*await Task.FromResult()*/
        }

        public async Task<Takvim> GetPrayerTimes(Location location)
        {
            konum=new Takvim();
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
            if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipTitreme", false)) CheckVibration(takvim.FecriKazip);
            if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikTitreme", false)) CheckVibration(takvim.FecriSadik);
            if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuTitreme", false)) CheckVibration(takvim.SabahSonu);
            if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleTitreme", false)) CheckVibration(takvim.Ogle);
            if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiTitreme", false)) CheckVibration(takvim.Ikindi);
            if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamTitreme", false)) CheckVibration(takvim.Aksam);
            if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiTitreme", false)) CheckVibration(takvim.Yatsi);
            if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuTitreme", false)) CheckVibration(takvim.YatsiSonu);
            if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipAlarm", false)) CheckAlarm(takvim.FecriKazip);
            if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikAlarm", false)) CheckAlarm(takvim.FecriSadik);
            if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuAlarm", false)) CheckAlarm(takvim.SabahSonu);
            if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleAlarm", false)) CheckAlarm(takvim.Ogle);
            if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiAlarm", false)) CheckAlarm(takvim.Ikindi);
            if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamAlarm", false)) CheckAlarm(takvim.Aksam);
            if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiAlarm", false)) CheckAlarm(takvim.Yatsi);
            if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuAlarm", false)) CheckAlarm(takvim.YatsiSonu);
        }

        private async void CheckAlarm(string vakit)
        {
            if ((DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString())) == TimeSpan.FromMinutes(0))
            {
                await CrossMediaManager.Current.PlayFromAssembly("ezan.mp3").ConfigureAwait(false);
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

        private void CheckVibration(string vakit)
        {
            if ((DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString()))==TimeSpan.FromMinutes(0))
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
                UserDialogs.Instance.Alert("Cihazınız titretmeyi desteklemiyor. " + ex.Message, "Cihaz desteklemiyor");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, "Bir sorunla karşılaştık");
            }
        }

        private void SetNotification(string Adi, string vakit)
        {
            var itemAdi = "";
            var NotificationId = 101;
            var bildiriVakti = TimeSpan.Parse(vakit) + TimeSpan.FromMinutes(Convert.ToDouble(Preferences.Get(Adi + "BildirmeVakti", 0.0)));
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
                        itemAdi = "Fecri Sadik";
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
    }
}