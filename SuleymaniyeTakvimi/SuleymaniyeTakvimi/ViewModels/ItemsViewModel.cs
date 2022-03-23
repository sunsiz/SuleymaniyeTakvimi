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
        public Command DarkLightModeCommand { get; }
        public Command<Item> ItemTapped { get; }
        Takvim _takvim, _vakitler;
        private string _city;
        private bool dark;

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
            get { return AppResources.AylikTakvim; /*DateTime.Today.ToString("M");*/ }
        }

        public bool Dark
        {
            get => dark;
            set => SetProperty(ref dark, value);
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
                Debug.WriteLine(exception);
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
                using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
                {
                    await GetPrayerTimesAsync().ConfigureAwait(false);
                    ExecuteLoadItemsCommand();
                }
            });
            DarkLightModeCommand = new Command(ChangeTheme);
            Task.Run(async () =>
            {
                if (!data.CheckInternet()) return;
                Log.Warning("TimeStamp-ItemsViewModel-SetAlarms", $"Starting Set Alarm at {DateTime.Now}");
                await Task.Delay(5000).ConfigureAwait(true);
                //DataService data = new DataService();
                data.SetWeeklyAlarms();
                _takvim = await data.VakitHesabiAsync().ConfigureAwait(false);
                ExecuteLoadItemsCommand();
                var location = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
                if (location != null && location.Enlem != 0)
                    data.GetMonthlyPrayerTimes(new Location(location.Enlem, location.Boylam, location.Yukseklik), false);
                //GetCity();
            }).ConfigureAwait(false);
            Dark = Theme.Tema != 1;//0 is dark, 1 is light
            Log.Warning("TimeStamp-ItemsViewModel-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private void ChangeTheme(object obj)
        {
            //int val = Preferences.Get(nameof(Theme.Tema), 1);
            //Debug.WriteLine(nameof(Theme.Tema));
            Theme.Tema = Theme.Tema == 1 ? 0 : 1;
            Dark = Theme.Tema != 1;
            Application.Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
        }
        
        private ObservableCollection<Item> ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Log.Warning("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            try
            {
                Items = new ObservableCollection<Item>();
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
            //IsBusy = true;
            var data = new DataService();
            
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
            
        }
    }
}