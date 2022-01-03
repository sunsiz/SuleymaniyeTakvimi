using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Internals;
using SuleymaniyeTakvimi.Localization;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        public ObservableCollection<Item> _items;
        public Command LoadItemsCommand { get; }
        public Command GoToMapCommand { get; }
        public Command GoToMonthCommand { get; }
        public Command RefreshLocationCommand { get; }
        public Command<Item> ItemTapped { get; }
        Takvim _takvim, _vakitler;
        private string _city;

        public ObservableCollection<Item> Items { get => _items; set => SetProperty<ObservableCollection<Item>>(ref _items, value); }

        public Takvim Vakitler
        {
            get
            {
                if (_vakitler == null)
                {
                    var data = new DataService();
                    _vakitler = data.takvim;
                    //data.CheckNotification();
                }

                return _vakitler;
            }
            set { SetProperty(ref _vakitler, value); }
        }
        public string Today
        {
            get { return DateTime.Today.ToString("M"); }
        }

        public string City
        {
            get { return _city; }
            set { SetProperty(ref _city, value); }
        }

        private async void GetCity()
        {
            try
            {
                //Without the Convert.ToDouble conversion it confuses the ',' and '.' when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
                var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_takvim.Enlem, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_takvim.Boylam,CultureInfo.InvariantCulture.NumberFormat)).ConfigureAwait(false);
            if (placemark != null)
                City = placemark.FirstOrDefault()?.AdminArea ?? placemark.FirstOrDefault()?.CountryName;
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
            }

            if (!string.IsNullOrEmpty(City)) Preferences.Set("sehir", City);
            City = City ?? Preferences.Get("sehir", AppResources.Sehir);
        }
        public ItemsViewModel()
        {
            Log.Warning("TimeStamp-ItemsViewModel-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            Items = new ObservableCollection<Item>();
            var data = new DataService();
            _takvim = data.takvim;
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            GoToMapCommand = new Command(async () =>
            {
                var location = new Location(Convert.ToDouble(_takvim.Enlem, CultureInfo.InvariantCulture.NumberFormat),Convert.ToDouble(_takvim.Boylam, CultureInfo.InvariantCulture.NumberFormat));
                var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_takvim.Enlem,CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_takvim.Boylam,CultureInfo.InvariantCulture.NumberFormat)).ConfigureAwait(true);
                var options = new MapLaunchOptions { Name = placemark.FirstOrDefault()?.Thoroughfare ?? placemark.FirstOrDefault()?.CountryName};

                try
                {
                    await Map.OpenAsync(location, options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Toast(AppResources.HaritaHatasi + ex.Message);
                }
            });
            //LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            //GetCity();
            GoToMonthCommand=new Command(GoToMonthPage);
            RefreshLocationCommand = new Command(async () =>
            {
                await GetPrayerTimesAsync().ConfigureAwait(false);
                ExecuteLoadItemsCommand();
            });
            Task.Run(async () =>
            {
                if (!data.CheckInternet()) return;
                Log.Warning("TimeStamp-ItemsViewModel-SetAlarms", $"Starting Set Alarm at {DateTime.Now}");
                await Task.Delay(20000).ConfigureAwait(true);
                //DataService data = new DataService();
                data.SetWeeklyAlarms();
                _takvim = await data.VakitHesabiAsync().ConfigureAwait(false);
                ExecuteLoadItemsCommand();
                var location = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
                if (location != null && location.Enlem != 0)
                    data.GetMonthlyPrayerTimes(new Location(location.Enlem, location.Boylam, location.Yukseklik));
                //GetCity();
            }).ConfigureAwait(false);
            Log.Warning("TimeStamp-ItemsViewModel-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private ObservableCollection<Item> ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Log.Warning("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            try
            {
                Items = new ObservableCollection<Item>();
                //var FecriKazip = new Item() { Id = "fecrikazip", Adi = "Fecri Kazip", Vakit = _takvim.FecriKazip, Etkin = Preferences.Get("fecrikazipEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriKazip), DateTime.Parse(_takvim.FecriSadik)), Alarm = Preferences.Get("fecrikazipAlarm",false), Bildiri = Preferences.Get("fecrikazipBildiri",false), Titreme = Preferences.Get("fecrikazipTitreme",false), BildirmeVakti = Preferences.Get("fecrikazipBildiriVakti","0.00")};
                //var FecriSadik = new Item() { Id = "fecrisadik", Adi = "Fecri Sadık", Vakit = _takvim.FecriSadik, Etkin = Preferences.Get("fecrisadikEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriSadik), DateTime.Parse(_takvim.SabahSonu)), Alarm = Preferences.Get("fecrisadikAlarm", false), Bildiri = Preferences.Get("fecrisadikBildiri", false), Titreme = Preferences.Get("fecrisadikTitreme", false), BildirmeVakti = Preferences.Get("fecrisadikBildiriVakti", "0.00") };
                //var SabahSonu = new Item() { Id = "sabahsonu", Adi = "Sabah Sonu", Vakit = _takvim.SabahSonu, Etkin = Preferences.Get("sabahsonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.SabahSonu), DateTime.Parse(_takvim.Ogle)), Alarm = Preferences.Get("sabahsonuAlarm", false), Bildiri = Preferences.Get("sabahsonuBildiri", false), Titreme = Preferences.Get("sabahsonuTitreme", false), BildirmeVakti = Preferences.Get("sabahsonuBildiriVakti", "0.00") };
                //var Ogle = new Item() { Id = "ogle", Adi = "Öğle", Vakit = _takvim.Ogle, Etkin = Preferences.Get("ogleEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ogle), DateTime.Parse(_takvim.Ikindi)), Alarm = Preferences.Get("ogleAlarm", false), Bildiri = Preferences.Get("ogleBildiri", false), Titreme = Preferences.Get("ogleTitreme", false), BildirmeVakti = Preferences.Get("ogleBildiriVakti", "0.00") };
                //var Ikindi = new Item() { Id = "ikindi", Adi = "İkindi", Vakit = _takvim.Ikindi, Etkin = Preferences.Get("ikindiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ikindi), DateTime.Parse(_takvim.Aksam)), Alarm = Preferences.Get("ikindiAlarm", false), Bildiri = Preferences.Get("ikindiBildiri", false), Titreme = Preferences.Get("ikindiTitreme", false), BildirmeVakti = Preferences.Get("ikindiBildiriVakti", "0.00") };
                //var Aksam = new Item() { Id = "aksam", Adi = "Akşam", Vakit = _takvim.Aksam, Etkin = Preferences.Get("aksamEtkin", false), State = CheckState(DateTime.Parse(_takvim.Aksam), DateTime.Parse(_takvim.Yatsi)), Alarm = Preferences.Get("aksamAlarm", false), Bildiri = Preferences.Get("aksamBildiri", false), Titreme = Preferences.Get("aksamTitreme", false), BildirmeVakti = Preferences.Get("aksamBildiriVakti", "0.00") };
                //var Yatsi = new Item() { Id = "yatsi", Adi = "Yatsı", Vakit = _takvim.Yatsi, Etkin = Preferences.Get("yatsiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Yatsi), DateTime.Parse(_takvim.YatsiSonu)), Alarm = Preferences.Get("yatsiAlarm", false), Bildiri = Preferences.Get("yatsiBildiri", false), Titreme = Preferences.Get("yatsiTitreme", false), BildirmeVakti = Preferences.Get("yatsiBildiriVakti", "0.00") };
                //var YatsiSonu = new Item() { Id = "yatsisonu", Adi = "Yatsı Sonu", Vakit = _takvim.YatsiSonu, Etkin = Preferences.Get("yatsisonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.YatsiSonu), DateTime.Parse(_takvim.FecriKazip)), Alarm = Preferences.Get("yatsisonuAlarm", false), Bildiri = Preferences.Get("yatsisonuBildiri", false), Titreme = Preferences.Get("yatsisonuTitreme", false), BildirmeVakti = Preferences.Get("yatsisonuBildiriVakti", "0.00") };
                var fecriKazip = new Item() { Id = "fecrikazip", Adi = AppResources.FecriKazip, Vakit = _takvim.FecriKazip, Etkin = Preferences.Get("fecrikazipEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriKazip), DateTime.Parse(_takvim.FecriSadik)) };
                var fecriSadik = new Item() { Id = "fecrisadik", Adi = AppResources.FecriSadik, Vakit = _takvim.FecriSadik, Etkin = Preferences.Get("fecrisadikEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriSadik), DateTime.Parse(_takvim.SabahSonu))};
                var sabahSonu = new Item() { Id = "sabahsonu", Adi = AppResources.SabahSonu, Vakit = _takvim.SabahSonu, Etkin = Preferences.Get("sabahsonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.SabahSonu), DateTime.Parse(_takvim.Ogle))};
                var ogle = new Item() { Id = "ogle", Adi = AppResources.Ogle, Vakit = _takvim.Ogle, Etkin = Preferences.Get("ogleEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ogle), DateTime.Parse(_takvim.Ikindi))};
                var ikindi = new Item() { Id = "ikindi", Adi = AppResources.Ikindi, Vakit = _takvim.Ikindi, Etkin = Preferences.Get("ikindiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ikindi), DateTime.Parse(_takvim.Aksam))};
                var aksam = new Item() { Id = "aksam", Adi = AppResources.Aksam, Vakit = _takvim.Aksam, Etkin = Preferences.Get("aksamEtkin", false), State = CheckState(DateTime.Parse(_takvim.Aksam), DateTime.Parse(_takvim.Yatsi))};
                var yatsi = new Item() { Id = "yatsi", Adi = AppResources.Yatsi, Vakit = _takvim.Yatsi, Etkin = Preferences.Get("yatsiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Yatsi), DateTime.Parse(_takvim.YatsiSonu))};
                var yatsiSonu = new Item() { Id = "yatsisonu", Adi = AppResources.YatsiSonu, Vakit = _takvim.YatsiSonu, Etkin = Preferences.Get("yatsisonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.YatsiSonu), DateTime.Parse(_takvim.FecriKazip))};
                Items.Add(fecriKazip);
                Items.Add(fecriSadik);
                Items.Add(sabahSonu);
                Items.Add(ogle);
                Items.Add(ikindi);
                Items.Add(aksam);
                Items.Add(yatsi);
                Items.Add(yatsiSonu);
                if (Preferences.Get(fecriKazip.Id, "") == "") Preferences.Set(fecriKazip.Id, _takvim.FecriKazip);
                if (Preferences.Get(fecriSadik.Id, "") == "") Preferences.Set(fecriSadik.Id, _takvim.FecriSadik);
                if (Preferences.Get(sabahSonu.Id, "") == "") Preferences.Set(sabahSonu.Id, _takvim.SabahSonu);
                if (Preferences.Get(ogle.Id, "") == "") Preferences.Set(ogle.Id, _takvim.Ogle);
                if (Preferences.Get(ikindi.Id, "") == "") Preferences.Set(ikindi.Id, _takvim.Ikindi);
                if (Preferences.Get(aksam.Id, "") == "") Preferences.Set(aksam.Id, _takvim.Aksam);
                if (Preferences.Get(yatsi.Id, "") == "") Preferences.Set(yatsi.Id, _takvim.Yatsi);
                if (Preferences.Get(yatsiSonu.Id, "") == "") Preferences.Set(yatsiSonu.Id, _takvim.YatsiSonu);
                //GetCity();
                //var today = Today;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Log.Warning("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                IsBusy = false;
            }

            return Items;
        }
        private string CheckState(DateTime current, DateTime next)
        {
            var state = "";
            if (DateTime.Now > next) state = "Passed";
            if (DateTime.Now > current && DateTime.Now < next) state = "Happening";
            if (DateTime.Now < current) state = "Waiting";
            return state;
        }
        public void OnAppearing()
        {
            Log.Warning("TimeStamp-OnAppearing-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            IsBusy = true;
            SelectedItem = null;
            GetCity();
            LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            Log.Warning("TimeStamp-OnAppearing-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}").ConfigureAwait(false);
        }

        async void GoToMonthPage(object obj)
        {
            IsBusy = true;
            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(MonthPage)}").ConfigureAwait(false);
        }

        private async Task GetPrayerTimesAsync()
        {
            IsBusy = true;
            var data = new DataService();
            //if (File.Exists(data._fileName))
            //{
            //    XDocument xmldoc = XDocument.Load(data._fileName);
            //    var takvims = data.ParseXmlList(xmldoc);
            //    if (takvims != null && DateTime.Parse(takvims[0].Tarih) <= DateTime.Today && DateTime.Parse(takvims[takvims.Count - 1].Tarih) >= DateTime.Today)
            //    {
            //        foreach (var item in takvims)
            //        {
            //            if (DateTime.Parse(item.Tarih) == DateTime.Today)
            //            {
            //                _takvim = item;
            //                return;
            //            }
            //        }
            //    }
            //}
            //try
            //{
                //var takvim = await data.GetCurrentLocationAsync().ConfigureAwait(false);
                ////var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                ////CancellationTokenSource cts = new CancellationTokenSource();
                ////var location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
                ////if (location != null)
                //if (takvim != null && takvim.Enlem > 0 && takvim.Boylam > 0)
                //{
                //    Location location = new Location(takvim.Enlem, takvim.Boylam, takvim.Yukseklik);
                //    data.konum = new Takvim
                //    {
                //        Enlem = location.Latitude,
                //        Boylam = location.Longitude,
                //        Yukseklik = location.Altitude ?? 0
                //    };
            _takvim = Vakitler = await data.GetPrayerTimesAsync(true).ConfigureAwait(false);
            if (Vakitler.Enlem != 0)
            {
                //Application.Current.Properties["takvim"] = Vakitler;
                Preferences.Set("enlem", Vakitler.Enlem);
                Preferences.Set("boylam", Vakitler.Boylam);
                Preferences.Set("yukseklik", Vakitler.Yukseklik);
                Preferences.Set("saatbolgesi", Vakitler.SaatBolgesi);
                Preferences.Set("yazkis", Vakitler.YazKis);
                Preferences.Set("fecrikazip", Vakitler.FecriKazip);
                Preferences.Set("fecrisadik", Vakitler.FecriSadik);
                Preferences.Set("sabahsonu", Vakitler.SabahSonu);
                Preferences.Set("ogle", Vakitler.Ogle);
                Preferences.Set("ikindi", Vakitler.Ikindi);
                Preferences.Set("aksam", Vakitler.Aksam);
                Preferences.Set("yatsi", Vakitler.Yatsi);
                Preferences.Set("yatsisonu", Vakitler.YatsiSonu);
                Preferences.Set("tarih", Vakitler.Tarih);
                //await Application.Current.SavePropertiesAsync();
                GetCity();
                LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            }
            else
            {
                UserDialogs.Instance.Toast(AppResources.KonumKapali, TimeSpan.FromSeconds(7));
            }

            IsBusy = false;
            //}
            //catch (Exception exception)
            //{
            //    UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
            //}
            //finally
            //{
            //    UserDialogs.Instance.Toast(AppResources.KonumYenilendi, TimeSpan.FromSeconds(3));
            //    IsBusy = false;
            //}
        }
    }
}