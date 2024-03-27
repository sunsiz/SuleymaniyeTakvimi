using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using SuleymaniyeTakvimi.Localization;
using Item = SuleymaniyeTakvimi.Models.Item;
using Java.Lang;

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
        private Takvim _takvim;
        private string _city;

        private ObservableCollection<Item> _items;
        private string _remainingTime;
        private bool _permissionRequested;
        private TaskCompletionSource<bool> _tcs;

        public ObservableCollection<Item> Items { get => _items; set => SetProperty<ObservableCollection<Item>>(ref _items, value); }
        public bool IsNecessary => DeviceInfo.Platform == DevicePlatform.iOS;
        
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

        private Takvim Takvim
        {
            get => _takvim;
            set
            {
                if (SetProperty(ref _takvim, value))
                    ExecuteLoadItemsCommand();
            }
        }

        public ItemsViewModel(DataService dataService):base(dataService)
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //data = dataService;

            try
            {
                dataService.OnTakvimChanged += (newTakvim) =>
                {
                    Takvim = newTakvim;
                };
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10)
                {
                    ResetAlarms();
                }

                Title = AppResources.PageTitle;
                Items = new ObservableCollection<Item>();
                //data = new DataService();
                Takvim = DataService._takvim;

                LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
                ItemTapped = new Command<Item>(OnItemSelected);
                GoToMapCommand = new Command(GoToMap);
                GoToMonthCommand = new Command(GoToMonthPage);
                SettingsCommand = new Command(Settings);
                RefreshLocationCommand = new Command(RefreshLocation);
                //If Location-saved is false, this means that location is never saved before. So, we need to check the location info.
                if (!Preferences.Get("LocationSaved", false))
                {
                    _ = CheckLocationInfoAsync(3000);
                }
                //If AlwaysRenewLocationEnabled is true, this means that the user wants to refresh the location info every time the app starts.
                if (Preferences.Get("AlwaysRenewLocationEnabled", false))
                {
                    RefreshLocationCommand.Execute(null);
                }
            
                CheckLastAlarmDate();
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine(exception);
            }

            Debug.WriteLine("TimeStamp-ItemsViewModel-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        /// <summary>
        /// Checks the last alarm date and sets the weekly alarms if the last alarm date is within 4 days of today.
        /// </summary>
        private void CheckLastAlarmDate()
        {
            var lastAlarmDateStr = Preferences.Get("LastAlarmDate", "Empty");
            if (lastAlarmDateStr != "Empty")
            {
                if ((DateTime.Parse(lastAlarmDateStr) - DateTime.Today).Days < 4)
                    _ = DataService.SetWeeklyAlarmsAsync();
            }
        }

        private async void RefreshLocation()
        {
            using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
            {
                await GetPrayerTimesAsync().ConfigureAwait(false);
                await Task.Run(() =>
                {
                    var location = new Location(Takvim.Enlem, Takvim.Boylam, Takvim.Yukseklik);
                    DataService.GetMonthlyPrayerTimes(location, true);
                    _ = DataService.SetWeeklyAlarmsAsync();
                }).ConfigureAwait(false);
            }
            ExecuteLoadItemsCommand();
        }

        private async void GoToMap()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var location = new Location(Takvim.Enlem, Takvim.Boylam);
                var placeMark = await Geocoding.GetPlacemarksAsync(location).ConfigureAwait(true);
                var options = new MapLaunchOptions { Name = placeMark.FirstOrDefault()?.Thoroughfare ?? placeMark.FirstOrDefault()?.CountryName };
                await Map.OpenAsync(location, options).ConfigureAwait(false);
            }
            catch (System.Exception ex)
            {
                UserDialogs.Instance.Toast(AppResources.HaritaHatasi + ex.Message);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private static void ResetAlarms()
        {
            //Prevent undesired behaviors caused by old settings.
            var alarms = new[] { "fecrikazipAlarm", "fecrisadikAlarm", "sabahsonuAlarm", "ogleAlarm", "ikindiAlarm", "aksamAlarm", "yatsiAlarm", "yatsisonuAlarm" };
            foreach (var alarm in alarms)
            {
                Preferences.Set(alarm, false);
            }
        }

        /// <summary>
        /// Checks the location information and updates the prayer times accordingly.
        /// </summary>
        /// <param name="timeDelay">The delay before the check starts, in milliseconds.</param>
        /// <remarks>
        /// This method first checks if the location service is enabled and if the device is connected to the internet.
        /// If these conditions are met, it waits for the specified delay, then retrieves the current prayer times.
        /// If the location data indicates that the default value (which the user is in Istanbul) or the location data is not set,
        /// it attempts to get the current location and update the prayer times.
        /// Finally, it sets the weekly alarms and updates the items in the view model.
        /// </remarks>

        private async Task CheckLocationInfoAsync(int timeDelay)
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-CheckLocationInfo-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            var service = DependencyService.Get<IPermissionService>();
            var isLocationEnabled = service.IsLocationServiceEnabled();
            if (!isLocationEnabled) return;
            if (!DataService.HaveInternet()) return;
            Takvim = await Task.Run<Takvim>(async () =>
            {
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(CheckLocationInfoAsync)}: Starting at {DateTime.Now}");
                await Task.Delay(timeDelay).ConfigureAwait(false);
                Takvim = DataService._takvim;
                if (Takvim.IsTakvimLocationUnValid()) DataService.InitTakvim();//If the Takvim location info is not valid, initialize the Takvim.
                _tcs = new TaskCompletionSource<bool>();

                await UpdatePrayerTimesIfNeeded().ConfigureAwait(false);

                await DataService.SetWeeklyAlarmsAsync().ConfigureAwait(false);
                return Takvim;
                //ExecuteLoadItemsCommand();
            }).ConfigureAwait(false);
            Debug.WriteLine("TimeStamp-ItemsViewModel-CheckLocationInfo-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private async Task UpdatePrayerTimesIfNeeded()
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-UpdatePrayerTimesIfNeeded-IsDefaultLocation", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //if (Takvim.IsTakvimLocationUnValid())
            //{
            //    Takvim = await DataService.PrepareMonthlyPrayerTimes().ConfigureAwait(false);
            //    _tcs.SetResult(true);
            //    await _tcs.Task;
            //    Debug.WriteLine($"***** IsDefaultLocation-Takvim {nameof(DataService.PrepareMonthlyPrayerTimes)} returned result: {Takvim}");
            //}
            Takvim = DataService._takvim;
            //If the Takvim location info is still not valid, try to get today's prayer times directly from the internet.
            if (Takvim.IsTakvimLocationUnValid())
            {
                Takvim = await DataService.GetPrayerTimesAsync(false, true).ConfigureAwait(false);
                _tcs.SetResult(true);
                await _tcs.Task;
                Debug.WriteLine($"***** IsDefaultLocation-Takvim {nameof(DataService.GetPrayerTimesAsync)} returned result: {Takvim}");
            }
            //If the Takvim location info is still not valid, try to force refresh the location info and get today's and monthly prayer times and also set weekly alarms.
            if (Takvim.IsTakvimLocationUnValid())
            {
                RefreshLocationCommand.Execute(null);
            }
        }

        
        private ObservableCollection<Item> ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Debug.WriteLine("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            //DataService.InitTakvim();
            DataService.InitTakvim();
            try
            {
                //Items = new ObservableCollection<Item>();
                var vakitIDs = new[] { "fecrikazip", "fecrisadik", "sabahsonu", "ogle", "ikindi", "aksam", "yatsi", "yatsisonu" };
                var vakitNames = new[] { "FecriKazip", "FecriSadik", "SabahSonu", "Ogle", "Ikindi", "Aksam", "Yatsi", "YatsiSonu" };
                var vakitValues = new[] { Takvim.FecriKazip, Takvim.FecriSadik, Takvim.SabahSonu, Takvim.Ogle, Takvim.Ikindi, Takvim.Aksam, Takvim.Yatsi, Takvim.YatsiSonu };

                var newItems = new List<Item>(vakitNames.Length);

                for (int i = 0; i < vakitNames.Length; i++)
                {
                    var item = new Item
                    {
                        Id = vakitIDs[i],
                        Adi = AppResources.ResourceManager.GetString(vakitNames[i], CultureInfo.GetCultureInfo(Preferences.Get("SelectedLanguage","en"))),
                        Vakit = vakitValues[i],
                        Etkin = Preferences.Get($"{vakitIDs[i]}Etkin", false),
                        State = CheckState(DateTime.ParseExact(vakitValues[i],"HH:mm",CultureInfo.InvariantCulture ), DateTime.ParseExact(vakitValues[(i + 1) % vakitIDs.Length],"HH:mm",CultureInfo.InvariantCulture))
                    };
                    newItems.Add(item);
                    Preferences.Set(item.Id, item.Vakit);
                }

                Items = new ObservableCollection<Item>(newItems);
            }
            catch (System.Exception ex)
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
            var now = DateTime.Now;

            if (now > next) return "Passed";
            if (now > current && now < next) return "Happening";
            if (now < current) return "Waiting";

            return string.Empty;
        }

        /// <summary>
        /// This method is called when the view appears. It performs several operations:
        /// 1. Checks if the location is saved and if permission has not been requested before, it handles the permission.
        /// 2. Sets the `IsBusy` property to true indicating that the view is busy.
        /// 3. Sets the `SelectedItem` to null.
        /// 4. Calls the `GetCityAsync` method to get the city based on the current location.
        /// 5. Executes the `LoadItemsCommand` to load the items.
        /// 6. Starts a new task that waits for 5 seconds, then asks for notification permission and starts the alarm foreground service if the "ForegroundServiceEnabled" preference is set to true.
        /// 7. Sets the `Title` property to the page title from the resources.
        /// 8. Sets the `IsBusy` property to false indicating that the view is no longer busy.
        /// 9. Starts a timer that updates the `RemainingTime` property every second.
        /// </summary>
        public async void OnAppearing()
        {
            Debug.WriteLine("TimeStamp-OnAppearing-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            if (!Preferences.Get("LocationSaved", false) && !_permissionRequested)
            {
                var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync();
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnAppearing)}: {result}");
                _permissionRequested = true;
            }

            IsBusy = true;
            Takvim = DataService._takvim;
            SelectedItem = null;
            ExecuteLoadItemsCommand();
            Device.StartTimer(TimeSpan.FromSeconds(1), () =>
            {
                RemainingTime = GetRemainingTime();
                return true; // True = Repeat again, False = Stop the timer
            });
            Task.Run(async () =>
            {
                await Task.Delay(5000).ConfigureAwait(false);
                DependencyService.Get<IPermissionService>().AskNotificationPermission();
                if (Preferences.Get("ForegroundServiceEnabled", true))
                    DependencyService.Get<IAlarmService>().StartAlarmForegroundService();
            });
            await GetCityAsync();
            Title = AppResources.PageTitle;
            IsBusy = false;
            Debug.WriteLine("TimeStamp-OnAppearing-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        /// <summary>
        /// Calculates the remaining time until the next prayer.
        /// </summary>
        /// <returns>
        /// A string representing the remaining time until the next prayer.
        /// If the current time is after the last prayer of the day, it returns the time passed since the last prayer.
        /// If none of the conditions are met, it returns an empty string.
        /// </returns>
        private string GetRemainingTime()
        {
            var currentTime = DateTime.Now.TimeOfDay;
            // Define the names and times of the prayers
            var vakitNames = new[] { "FecriKazip", "FecriSadik", "SabahSonu", "Ogle", "Ikindi", "Aksam", "Yatsi", "YatsiSonu" };
            var vakitValues = new[] { Takvim.FecriKazip, Takvim.FecriSadik, Takvim.SabahSonu, Takvim.Ogle, Takvim.Ikindi, Takvim.Aksam, Takvim.Yatsi, Takvim.YatsiSonu }.Select(TimeSpan.Parse).ToList(); // Parse once
            // Define the resources for the remaining time messages
            var vakitResources = new[] { AppResources.FecriKazibingirmesinekalanvakit, AppResources.FecriSadikakalanvakit, AppResources.SabahSonunakalanvakit, AppResources.Ogleningirmesinekalanvakit, AppResources.Oglenincikmasinakalanvakit, AppResources.Ikindinincikmasinakalanvakit, AppResources.Aksamincikmasnakalanvakit, AppResources.Yatsinincikmasinakalanvakit };
            // If the current time is before the first prayer of the day, return the time left to the first prayer time
            if (currentTime <= vakitValues.First())
            {
                return AppResources.FecriKazibingirmesinekalanvakit + (currentTime - vakitValues.First()).ToString(@"hh\:mm\:ss");
            }
            // Loop through the prayers
            for (int i = 0; i < vakitNames.Length; i++)
            {
                // Parse the current and next prayer times
                var currentVakit = vakitValues[i];
                var nextVakit = vakitValues[(i + 1) % vakitNames.Length];

                // If the current time is between the current and next prayer times, return the remaining time until the next prayer
                if (currentTime >= currentVakit && currentTime <= nextVakit)
                {
                    return vakitResources[i+1] + (nextVakit - currentTime).ToString(@"hh\:mm\:ss");
                }
            }

            // If the current time is after the last prayer of the day, return the time passed since the last prayer
            if (currentTime >= vakitValues.Last())
            {
                return AppResources.Yatsininciktigindangecenvakit + (currentTime - vakitValues.Last()).ToString(@"hh\:mm\:ss");
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

        private static async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}").ConfigureAwait(false);
        }

        private async void GoToMonthPage()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                await Shell.Current.GoToAsync(nameof(MonthPage)).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GetPrayerTimesAsync()
        {
            //IsBusy = true;
            //var data = new DataService();

            Takvim = await DataService.GetPrayerTimesAsync(true).ConfigureAwait(false);
            if (!Takvim.IsTakvimLocationUnValid())
            {
                //Application.Current.Properties["takvim"] = Vakitler;
                Preferences.Set("enlem", Takvim.Enlem);
                Preferences.Set("boylam", Takvim.Boylam);
                Preferences.Set("yukseklik", Takvim.Yukseklik);
                Preferences.Set("saatbolgesi", Takvim.SaatBolgesi);
                Preferences.Set("yazkis", Takvim.YazKis);
                Preferences.Set("fecrikazip", Takvim.FecriKazip);
                Preferences.Set("fecrisadik", Takvim.FecriSadik);
                Preferences.Set("sabahsonu", Takvim.SabahSonu);
                Preferences.Set("ogle", Takvim.Ogle);
                Preferences.Set("ikindi", Takvim.Ikindi);
                Preferences.Set("aksam", Takvim.Aksam);
                Preferences.Set("yatsi", Takvim.Yatsi);
                Preferences.Set("yatsisonu", Takvim.YatsiSonu);
                Preferences.Set("tarih", Takvim.Tarih);
                //await Application.Current.SavePropertiesAsync();
                await GetCityAsync();
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

        private async void Settings()
        {
            if (IsBusy)
            {
                return;
            }
            IsBusy = true;
            // This will push the SettingsPage onto the navigation stack
            try
            {
                await Shell.Current.GoToAsync(nameof(SettingsPage)).ConfigureAwait(false);
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task GetCityAsync()
        {
            try
            {
                await Task.Delay(2000);
                if (DataService.HaveInternet())
                {
                    // Parse latitude and longitude as double values using InvariantCulture to avoid issues with different culture settings
                    // Get the placemarks for the current location
                    var placemarks = await Geocoding.GetPlacemarksAsync(Takvim.Enlem, Takvim.Boylam)
                        .ConfigureAwait(false);

                    // Get the first placemark, if any
                    var placemark = placemarks.FirstOrDefault();

                    if (placemark != null)
                    {
                        // Use the AdminArea, Locality, or CountryName, in that order of preference
                        City = placemark.Locality ?? placemark.AdminArea ?? placemark.CountryName;
                    }
                }
                else
                {
                    UserDialogs.Instance.Toast($"{AppResources.Sehir}: {AppResources.TakvimIcinInternetBaslik}",
                        TimeSpan.FromSeconds(7));
                }
            }
            catch (System.Exception exception)
            {
                Debug.WriteLine($"***** GetCityAsync Exception Details: {exception}");
                UserDialogs.Instance.Toast($"{AppResources.Sehir}: {exception.Message}", TimeSpan.FromSeconds(7));
            }

            // If City is not null or empty, save it in the preferences
            if (!string.IsNullOrEmpty(City))
            {
                Preferences.Set("sehir", City);
            }
            else
            {
                // If City is null or empty, get the city from the preferences or use the default city
                City = Preferences.Get("sehir", AppResources.Sehir);
            }
        }

    }
}