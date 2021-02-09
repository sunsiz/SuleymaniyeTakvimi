using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Services
{
    public class TakvimData : ITakvimData
    {
        readonly List<Item> items;
        public Takvim konum;
        public Takvim takvim; 
        //public Command GetLocationCommand { get; }


        //protected override void OnDisappearing()
        //{
        //    if (cts != null && !cts.IsCancellationRequested)
        //        cts.Cancel();
        //    base.OnDisappearing();
        //}
        public TakvimData()
        {
            //GetLocationCommand = new Command(async () => GetCurrentLocation());
            //GetCurrentLocation().ConfigureAwait(true);
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

        public async Task<bool> AddItemAsync(Item item)
        {
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> UpdateItemAsync(Item item)
        {
            var oldItem = items.Where((Item arg) => arg.Id == item.Id).FirstOrDefault();
            items.Remove(oldItem);
            items.Add(item);

            return await Task.FromResult(true);
        }

        public async Task<bool> DeleteItemAsync(string id)
        {
            var oldItem = items.Where((Item arg) => arg.Id == id).FirstOrDefault();
            items.Remove(oldItem);

            return await Task.FromResult(true);
        }

        public async Task<Item> GetItemAsync(string id)
        {
            return await Task.FromResult(items.FirstOrDefault(s => s.Id == id));
        }

        public async Task<IEnumerable<Item>> GetItemsAsync(bool forceRefresh = false)
        {
            return await Task.FromResult(items);
        }


        public async Task<Takvim> GetCurrentLocation()
        {
            try
            {
                var location = await Geolocation.GetLastKnownLocationAsync();

                konum = new Takvim();

                if (location == null)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
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
            }
            catch (PermissionException pEx)
            {
                // Handle permission exception
                Console.WriteLine(pEx.Message);
                await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
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
                url += "&SaatBolgesi=" + TimeZoneInfo.Local.StandardName;
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
    }
}