using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using SuleymaniyeTakvimi.Localization;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        private Command LoadItemsCommand { get; }
        public Command GoToMapCommand { get; }
        public Command GoToMonthCommand { get; }
        public Command RefreshLocationCommand { get; }
        //public Command DarkLightModeCommand { get; }
        public Command SettingsCommand { get; }
        public Command<Item> ItemTapped { get; }
        private Takvim _takvim, _vakitler;
        private string _city;
        //private bool _dark;
        private DataService data;

        private ObservableCollection<Item> _items;
        private string _remainingTime;

        public ObservableCollection<Item> Items { get => _items; set => SetProperty<ObservableCollection<Item>>(ref _items, value); }

        private Takvim Vakitler
        {
            get
            {
                if (_vakitler == null)
                {
                    _vakitler = data._takvim;
                    //data.CheckNotification();
                }

                return _vakitler;
            }
            set => SetProperty(ref _vakitler, value);
        }
        //public string Today => AppResources.AylikTakvim; /*DateTime.Today.ToString("M");*/

        //public bool Dark
        //{
        //    get => _dark;
        //    set => SetProperty(ref _dark, value);
        //}

        public string RemainingTime
        {
            get => _remainingTime;
            set => SetProperty(ref _remainingTime, value);
        }

        public string City
        {
            get => _city;
            set => SetProperty(ref _city, value);
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
            City ??= Preferences.Get("sehir", AppResources.Sehir);
        }

        public ItemsViewModel()
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            Title = AppResources.PageTitle;
            Items = new ObservableCollection<Item>();
            data = new DataService();
            _takvim = data._takvim;
            LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            GoToMapCommand = new Command(async () =>
            {
                try
                {
                   var location = new Location(Convert.ToDouble(_takvim.Enlem, CultureInfo.InvariantCulture.NumberFormat),Convert.ToDouble(_takvim.Boylam, CultureInfo.InvariantCulture.NumberFormat));
                    var placeMark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_takvim.Enlem,CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_takvim.Boylam,CultureInfo.InvariantCulture.NumberFormat)).ConfigureAwait(true);
                    var options = new MapLaunchOptions { Name = placeMark.FirstOrDefault()?.Thoroughfare ?? placeMark.FirstOrDefault()?.CountryName};
                    await Map.OpenAsync(location, options).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Toast(AppResources.HaritaHatasi + ex.Message);
                }
            });
            //LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            //GetCity();
            GoToMonthCommand = new Command(GoToMonthPage);
            SettingsCommand = new Command(Settings);
            RefreshLocationCommand = new Command(async () =>
            {
                using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
                {
                    await GetPrayerTimesAsync().ConfigureAwait(false);
                    await Task.Run(() =>
                    {
                        var location = new Location(_takvim.Enlem, _takvim.Boylam, _takvim.Yukseklik);
                        data.GetMonthlyPrayerTimes(location, true);
                        data.SetWeeklyAlarms();
                    }).ConfigureAwait(false);
                }
                ExecuteLoadItemsCommand();
            });
            //DarkLightModeCommand = new Command(ChangeTheme);
            //CheckLocationInfo(data, 3000);
            if (!Preferences.Get("LocationSaved", false))
                CheckLocationInfo(3000);
            //CheckLocationInfo(data, 60000);
            //Dark = Theme.Tema != 1;//0 is dark, 1 is light
            //Console.WriteLine("CurrentCulture is {0}.", CultureInfo.CurrentCulture.Name);
            Debug.WriteLine("TimeStamp-ItemsViewModel-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private void CheckLocationInfo(int timeDelay)
        {
            if (!DependencyService.Get<IPermissionService>().IsLocationServiceEnabled()) return;
            Task.Run(async () =>
            {
                if (!data.HaveInternet()) return;
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(CheckLocationInfo)}: Starting at {DateTime.Now}");
                await Task.Delay(timeDelay).ConfigureAwait(false);
                var takvim = data._takvim;
                /*if (!File.Exists(data._fileName)) */takvim = await data.PrepareMonthlyPrayerTimes().ConfigureAwait(false);
                if ((takvim.Yukseklik == 114.0 && takvim.Enlem == 41.0 && takvim.Boylam == 29.0) || (takvim.Yukseklik == 0 && takvim.Enlem == 0 && takvim.Boylam == 0))
                {
                    _takvim = await data.VakitHesabiAsync().ConfigureAwait(false);
                    var location = await data.GetCurrentLocationAsync(false).ConfigureAwait(false);
                    if (location != null && location.Latitude != 0 && location.Longitude != 0)
                        data.GetMonthlyPrayerTimes(location, false);
                    RefreshLocationCommand.Execute(null);
                }

                //if (_takvim == null) UserDialogs.Instance.Toast(AppResources.TakvimIcinInternet);
                //DataService data = new DataService();
                data.SetWeeklyAlarms();
                ExecuteLoadItemsCommand();
                //GetCity();
            }).ConfigureAwait(false);
        }

        //private void ChangeTheme(object obj)
        //{
        //    //int val = Preferences.Get(nameof(Theme.Tema), 1);
        //    //Debug.WriteLine(nameof(Theme.Tema));
        //    Theme.Tema = Theme.Tema == 1 ? 0 : 1;
        //    Dark = Theme.Tema != 1;
        //    Application.Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
        //}
        
        private ObservableCollection<Item> ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Debug.WriteLine("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            
            data.InitTakvim();
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
                Preferences.Set(fecriKazip.Id, _takvim.FecriKazip);
                Preferences.Set(fecriSadik.Id, _takvim.FecriSadik);
                Preferences.Set(sabahSonu.Id, _takvim.SabahSonu);
                Preferences.Set(ogle.Id, _takvim.Ogle);
                Preferences.Set(ikindi.Id, _takvim.Ikindi);
                Preferences.Set(aksam.Id, _takvim.Aksam);
                Preferences.Set(yatsi.Id, _takvim.Yatsi);
                Preferences.Set(yatsiSonu.Id, _takvim.YatsiSonu);
                //if (Preferences.Get(fecriKazip.Id, "") == "") Preferences.Set(fecriKazip.Id, _takvim.FecriKazip);
                //if (Preferences.Get(fecriSadik.Id, "") == "") Preferences.Set(fecriSadik.Id, _takvim.FecriSadik);
                //if (Preferences.Get(sabahSonu.Id, "") == "") Preferences.Set(sabahSonu.Id, _takvim.SabahSonu);
                //if (Preferences.Get(ogle.Id, "") == "") Preferences.Set(ogle.Id, _takvim.Ogle);
                //if (Preferences.Get(ikindi.Id, "") == "") Preferences.Set(ikindi.Id, _takvim.Ikindi);
                //if (Preferences.Get(aksam.Id, "") == "") Preferences.Set(aksam.Id, _takvim.Aksam);
                //if (Preferences.Get(yatsi.Id, "") == "") Preferences.Set(yatsi.Id, _takvim.Yatsi);
                //if (Preferences.Get(yatsiSonu.Id, "") == "") Preferences.Set(yatsiSonu.Id, _takvim.YatsiSonu);
                //GetCity();
                //var today = Today;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Debug.WriteLine("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
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
            Debug.WriteLine("TimeStamp-OnAppearing-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            if (!Preferences.Get("LocationSaved", false))
            {

                var result = DependencyService.Get<IPermissionService>().HandlePermissionAsync()
                    .ConfigureAwait(false);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnAppearing)}: {result}");
                //Task.Run(async () =>
                //{
                //    var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync()
                //        .ConfigureAwait(false);
                //    Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnAppearing)}: {result}");
                //    if (result == PermissionStatus.Denied)
                //        UserDialogs.Instance.Toast(AppResources.KonumIzniBaslik, TimeSpan.FromSeconds(5));
                //});
            }

            IsBusy = true;
            SelectedItem = null;
            GetCity();
            LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            Task.Run(async () =>
            {
                await Task.Delay(5000).ConfigureAwait(false);
                if (Preferences.Get("ForegroundServiceEnabled", true))
                    DependencyService.Get<IAlarmService>().StartAlarmForegroundService();
            });
            Title = AppResources.PageTitle;
            IsBusy = false;
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                // Do something
                RemainingTime = GetRemainingTime();
                return true; // True = Repeat again, False = Stop the timer
            });
            //CheckLocationInfo(3000);
            Debug.WriteLine("TimeStamp-OnAppearing-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private string GetRemainingTime()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            try
            {
                if (currentTime < TimeSpan.Parse(_takvim.FecriKazip))
                    return AppResources.FecriKazibingirmesinekalanvakit +
                              (TimeSpan.Parse(_takvim.FecriKazip) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.FecriKazip) && currentTime <= TimeSpan.Parse(_takvim.FecriSadik))
                    return AppResources.FecriSadikakalanvakit +
                           (TimeSpan.Parse(_takvim.FecriSadik) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.FecriSadik) && currentTime <= TimeSpan.Parse(_takvim.SabahSonu))
                    return AppResources.SabahSonunakalanvakit +
                           (TimeSpan.Parse(_takvim.SabahSonu) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.SabahSonu) && currentTime <= TimeSpan.Parse(_takvim.Ogle))
                    return AppResources.Ogleningirmesinekalanvakit +
                           (TimeSpan.Parse(_takvim.Ogle) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.Ogle) && currentTime <= TimeSpan.Parse(_takvim.Ikindi))
                    return AppResources.Oglenincikmasinakalanvakit +
                           (TimeSpan.Parse(_takvim.Ikindi) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.Ikindi) && currentTime <= TimeSpan.Parse(_takvim.Aksam))
                    return AppResources.Ikindinincikmasinakalanvakit +
                           (TimeSpan.Parse(_takvim.Aksam) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.Aksam) && currentTime <= TimeSpan.Parse(_takvim.Yatsi))
                    return AppResources.Aksamincikmasnakalanvakit +
                           (TimeSpan.Parse(_takvim.Yatsi) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.Yatsi) && currentTime <= TimeSpan.Parse(_takvim.YatsiSonu))
                    return AppResources.Yatsinincikmasinakalanvakit +
                           (TimeSpan.Parse(_takvim.YatsiSonu) - currentTime).ToString(@"hh\:mm\:ss");
                if (currentTime >= TimeSpan.Parse(_takvim.YatsiSonu))
                    return AppResources.Yatsininciktigindangecenvakit +
                           (currentTime - TimeSpan.Parse(_takvim.YatsiSonu)).ToString(@"hh\:mm\:ss");
                //if (currentTime < TimeSpan.Parse(_takvim.FecriKazip))
                //    return (TimeSpan.Parse(_takvim.FecriKazip) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.FecriKazip) &&
                //    currentTime <= TimeSpan.Parse(_takvim.FecriSadik))
                //    return (TimeSpan.Parse(_takvim.FecriSadik) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.FecriSadik) &&
                //    currentTime <= TimeSpan.Parse(_takvim.SabahSonu))
                //    return (TimeSpan.Parse(_takvim.SabahSonu) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\\\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.SabahSonu) &&
                //    currentTime <= TimeSpan.Parse(_takvim.Ogle))
                //    return (TimeSpan.Parse(_takvim.Ogle) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.Ogle) && currentTime <= TimeSpan.Parse(_takvim.Ikindi))
                //    return (TimeSpan.Parse(_takvim.Ikindi) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.Ikindi) && currentTime <= TimeSpan.Parse(_takvim.Aksam))
                //    return (TimeSpan.Parse(_takvim.Aksam) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.Aksam) && currentTime <= TimeSpan.Parse(_takvim.Yatsi))
                //    return (TimeSpan.Parse(_takvim.Yatsi) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.Yatsi) &&
                //    currentTime <= TimeSpan.Parse(_takvim.YatsiSonu))
                //    return (TimeSpan.Parse(_takvim.YatsiSonu) - currentTime).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
                //if (currentTime >= TimeSpan.Parse(_takvim.YatsiSonu))
                //    return (TimeSpan.Parse("23:59")-currentTime + TimeSpan.Parse(_takvim.FecriKazip)).Add(TimeSpan.FromMinutes(1))
                //        .ToString(@"hh\:mm\:ss");
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine(
                    $"GetFormattedRemainingTime exception: {exception.Message}. Location: {_takvim.Enlem}, {_takvim.Boylam}");
            }

            return "";
        }

        private Item SelectedItem
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
            // This will push the MonthPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(MonthPage)}").ConfigureAwait(false);
            IsBusy = false;
        }

        private async Task GetPrayerTimesAsync()
        {
            //IsBusy = true;
            //var data = new DataService();
            
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
                if ((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 12))
                    UserDialogs.Instance.Toast(AppResources.KonumIzniIcerik, TimeSpan.FromSeconds(5));
                else
                    UserDialogs.Instance.Toast(AppResources.KonumKapali, TimeSpan.FromSeconds(5));
            }
        }

        private async void Settings(object obj)
        {
            IsBusy = true;
            // This will push the SettingsPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(SettingsPage)}").ConfigureAwait(false);
            IsBusy = false;
        }
    }
}