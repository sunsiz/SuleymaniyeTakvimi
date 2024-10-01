using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using SuleymaniyeTakvimi.Localization;
using Item = SuleymaniyeTakvimi.Models.Item;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        private Takvim _takvim;
        private string _city;
        private ObservableCollection<Item> _items;
        private string _remainingTime;
        private bool _permissionRequested;
        private TaskCompletionSource<bool> _tcs;
        //private Command LoadItemsCommand { get; }
        public Command GoToMapCommand { get; }
        public Command GoToMonthCommand { get; }
        public Command RefreshLocationCommand { get; }
        public Command SettingsCommand { get; }
        public Command<Item> ItemTapped { get; }


        public ObservableCollection<Item> Items
        {
            get => _items;
            set
            {
                if (_items != value)
                {
                    _items = value;
                    OnPropertyChanged(nameof(Items));
                }
            } /*set => SetProperty<ObservableCollection<Item>>(ref _items, value);*/
        }

        public bool IsNecessary => DeviceInfo.Platform == DevicePlatform.iOS;

        public string RemainingTime
        {
            get => _remainingTime;
            set => SetProperty(ref _remainingTime, value);
        }

        public string City
        {
            get => _city;
            //set
            //{
            //    if (_city != value)
            //    {
            //        _city = value;
            //        OnPropertyChanged(nameof(City));
            //    }
            //}
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

        private Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                //OnItemSelected(value);
            }
        }

        public ItemsViewModel(DataService dataService) : base(dataService)
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //data = dataService;

            try
            {
                // Initialize commands
                GoToMapCommand = new Command(async () => await GoToMap());
                GoToMonthCommand = new Command(async () => await GoToMonthPage());
                RefreshLocationCommand = new Command(async () => await RefreshLocation());
                SettingsCommand = new Command(async () => await Settings());
                ItemTapped = new Command<Item>(async (item) => await OnItemSelected(item));
                //dataService.OnTakvimChanged += (newTakvim) =>
                //{
                //    Takvim = newTakvim;
                //};
                // Subscribe to the language change event
                MessagingCenter.Subscribe<SettingsViewModel>(this, "LanguageChanged", (sender) =>
                {
                    ExecuteLoadItemsCommand();
                });
                MessagingCenter.Subscribe<ItemDetailViewModel>(this, "AlarmSaved", (sender) =>
                {
                    Debug.WriteLine("AlarmSaved Message Received in ItemsViewModel");
                    ExecuteLoadItemsCommand();
                });
                DataService.PropertyChanged += OnDataServicePropertyChanged;
                if (DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10)
                {
                    ResetAlarms();
                }

                Title = AppResources.PageTitle;

                // Initialize properties
                Items = new ObservableCollection<Item>();
                //data = new DataService();
                Takvim = DataService.Takvim;
                City = Preferences.Get("sehir", AppResources.Sehir);
                //LoadItemsCommand = new Command(() => ExecuteLoadItemsCommand());
                //_ = InitializeLocation();
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
        private void OnDataServicePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(DataService.Takvim) || e.PropertyName == nameof(DataService.MonthlyTakvim))
            {
                Takvim = DataService.Takvim;
                //ExecuteLoadItemsCommand();
                Task.Run(async () => await GetCityAsync());
            }
        }
        //private async Task InitializeLocation()
        //{
        //    //If Location-saved is false, this means that location is never saved before. So, we need to check the location info.
        //    if (!Preferences.Get("LocationSaved", false))
        //    {
        //        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();

        //        if (status == PermissionStatus.Granted)
        //        {
        //            await CheckLocationInfoAsync(5000);
        //        }
        //    }
        //}

        /// <summary>
        /// Checks the last alarm date and sets the weekly alarms if the last alarm date is within 4 days of today.
        /// </summary>
        private void CheckLastAlarmDate()
        {
            var lastAlarmDateStr = Preferences.Get("LastAlarmDate", "Empty");
            if (lastAlarmDateStr != "Empty")
            {
                if ((DateTime.Parse(lastAlarmDateStr) - DateTime.Today).Days < 2)
                    _ = DataService.SetWeeklyAlarmsAsync();
            }
        }

        private async Task RefreshLocation()
        {
            Debug.WriteLine("TimeStamp-ItemsViewModel-RefreshLocation-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            IsBusy = true;
            try
            {
                var permissionService = DependencyService.Get<IPermissionService>();
                Device.BeginInvokeOnMainThread(async () =>
                {
                    var locationPermissionStatus = await permissionService.HandlePermissionAsync();

                    if (locationPermissionStatus != PermissionStatus.Granted)
                    {
                        // Handle the case where permission is not granted, e.g., show a message to the user
                        UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                        return;
                    }

                    using (var loading = UserDialogs.Instance.Loading(AppResources.Yenileniyor))
                    {
                        Takvim = await DataService.GetPrayerTimesAsync(true).ConfigureAwait(false);
                        Debug.WriteLine($"***** {this.GetType().Name}.{nameof(RefreshLocation)} Prayer Time: {Takvim.DisplayValues()}");
                        if (Takvim != null && !Takvim.IsTakvimLocationUnValid())
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
                            Task.Run(async () => await GetCityAsync());
                            //LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
                        }
                        else
                        {
                            if ((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 12))
                                UserDialogs.Instance.Toast(AppResources.KonumIzniIcerik, TimeSpan.FromSeconds(5));
                            else
                                UserDialogs.Instance.Toast(AppResources.KonumKapali, TimeSpan.FromSeconds(5));
                        }

                        if (Takvim != null)
                        {
                            var location = new Location(Takvim.Enlem, Takvim.Boylam, Takvim.Yukseklik);
                            loading.Title = AppResources.AylikTakvimYenileniyor;
                            if(Helper.IsValidLocation(location))await DataService.GetMonthlyPrayerTimesAsync(location, true);
                            loading.Title = AppResources.AlarmlarPlanlaniyor;
                            await DataService.SetWeeklyAlarmsAsync();
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Exception in RefreshLocation: {ex.Message}");
            }
            //ExecuteLoadItemsCommand();
            IsBusy = false;
            Debug.WriteLine("TimeStamp-ItemsViewModel-RefreshLocation-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private async Task GoToMap()
        {
            if (IsBusy)
            {
                return;
            }

            IsBusy = true;
            try
            {
                var location = DataService.GetSavedLocation() ?? new Location(Takvim.Enlem, Takvim.Boylam);
                var placeMark = await Geocoding.GetPlacemarksAsync(location);
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

        //private async Task CheckLocationInfoAsync(int timeDelay)
        //{
        //    Debug.WriteLine("TimeStamp-ItemsViewModel-CheckLocationInfo-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //    var service = DependencyService.Get<IPermissionService>();
        //    var isLocationEnabled = service.IsLocationServiceEnabled();
        //    if (!isLocationEnabled) return;
        //    if (!Helper.HaveInternet()) return;
        //    Takvim = await Task.Run(async () =>
        //    {
        //        Debug.WriteLine($"**** {this.GetType().Name}.{nameof(CheckLocationInfoAsync)}: Starting at {DateTime.Now}");
        //        await Task.Delay(timeDelay).ConfigureAwait(false);
        //        Takvim = DataService.Takvim;
        //        //if (Takvim.IsTakvimLocationUnValid()) DataService.InitTakvim();//If the Takvim location info is not valid, initialize the Takvim.
        //        _tcs = new TaskCompletionSource<bool>();

        //        //await UpdatePrayerTimesIfNeeded().ConfigureAwait(false);

        //        await DataService.SetWeeklyAlarmsAsync().ConfigureAwait(false);
        //        return Takvim;
        //        //ExecuteLoadItemsCommand();
        //    }).ConfigureAwait(false);
        //    Debug.WriteLine("TimeStamp-ItemsViewModel-CheckLocationInfo-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //}

        //private async Task UpdatePrayerTimesIfNeeded()
        //{
        //    Debug.WriteLine("TimeStamp-ItemsViewModel-UpdatePrayerTimesIfNeeded-IsDefaultLocation", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //    //If the Takvim location info is still not valid, try to get today's prayer times directly from the internet.
        //    if (Takvim.IsTakvimLocationUnValid())
        //    {
        //        Takvim = await DataService.GetPrayerTimesAsync(false, true).ConfigureAwait(false);
        //        _tcs.SetResult(true);
        //        await _tcs.Task;
        //        Debug.WriteLine($"***** IsDefaultLocation-Takvim {nameof(DataService.GetPrayerTimesAsync)} returned result: {Takvim}");
        //    }
        //    //If the Takvim location info is still not valid, try to force refresh the location info and get today's and monthly prayer times and also set weekly alarms.
        //    if (Takvim.IsTakvimLocationUnValid())
        //    {
        //        RefreshLocationCommand.Execute(null);
        //    }
        //}


        private void ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Debug.WriteLine("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            //DataService.InitTakvim();
            Debug.WriteLine($"TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-InitTakvim => {Takvim.DisplayValues()}", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            try
            {
                //Items = new ObservableCollection<Item>();
                var prayerTimeIDs = new[] { "fecrikazip", "fecrisadik", "sabahsonu", "ogle", "ikindi", "aksam", "yatsi", "yatsisonu" };
                var prayerTimeNames = new[] { "FecriKazip", "FecriSadik", "SabahSonu", "Ogle", "Ikindi", "Aksam", "Yatsi", "YatsiSonu" };
                var prayerTimeValues = new[] { Takvim.FecriKazip, Takvim.FecriSadik, Takvim.SabahSonu, Takvim.Ogle, Takvim.Ikindi, Takvim.Aksam, Takvim.Yatsi, Takvim.YatsiSonu };

                var newItems = new List<Item>(prayerTimeNames.Length);

                for (int i = 0; i < prayerTimeNames.Length; i++)
                {
                    var currentPrayer = DateTime.ParseExact(prayerTimeValues[i], "HH:mm", CultureInfo.InvariantCulture);
                    var nextPrayer = DateTime.ParseExact(prayerTimeValues[(i + 1) % prayerTimeIDs.Length], "HH:mm", CultureInfo.InvariantCulture);
                    var item = new Item
                    {
                        Id = prayerTimeIDs[i],
                        Adi = AppResources.ResourceManager.GetString(prayerTimeNames[i], CultureInfo.GetCultureInfo(Preferences.Get("SelectedLanguage", CultureInfo.CurrentCulture.TwoLetterISOLanguageName))),
                        Vakit = prayerTimeValues[i],
                        Etkin = Preferences.Get($"{prayerTimeIDs[i]}Etkin", false),
                        State = CheckState(currentPrayer, nextPrayer)
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
                try
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        _permissionRequested = true;
                        Debug.WriteLine($"Is main thread: {Device.IsInvokeRequired}");
                        var result = await DependencyService.Get<IPermissionService>().HandlePermissionAsync();
                        Debug.WriteLine($"**** {this.GetType().Name}.{nameof(OnAppearing)}: {result}");
                    });
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Exception in OnAppearing: {ex.Message}");
                }
            }

            IsBusy = true;
            Takvim = DataService.Takvim;
            SelectedItem = null;
            //ExecuteLoadItemsCommand();
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
            Task.Run(async () => await GetCityAsync());
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
            if (Takvim == null || !Helper.IsValidLocation(new Location(Takvim.Enlem,Takvim.Boylam)))
            {
                Debug.WriteLine($"**** Takvim is invalid. **** {Takvim?.DisplayValues()}");
                return string.Empty;
            }
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
                    return vakitResources[i + 1] + (nextVakit - currentTime).ToString(@"hh\:mm\:ss");
                }
            }

            // If the current time is after the last prayer of the day, return the time passed since the last prayer
            if (currentTime >= vakitValues.Last())
            {
                return AppResources.Yatsininciktigindangecenvakit + (currentTime - vakitValues.Last()).ToString(@"hh\:mm\:ss");
            }

            return "";
        }

        private static async Task OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}").ConfigureAwait(false);
        }

        private Task GoToMonthPage()
        {
            if (IsBusy)
            {
                return Task.CompletedTask;
            }

            IsBusy = true;
            try
            {
                return Shell.Current.GoToAsync(nameof(MonthPage));
            }
            finally
            {
                IsBusy = false;
            }
        }

        //private async Task GetPrayerTimesAsync()
        //{
        //    //IsBusy = true;
        //    //var data = new DataService();

        //    Takvim = await DataService.GetPrayerTimesAsync(true).ConfigureAwait(false);
        //    if(Takvim == null) return;
        //    if (!Takvim.IsTakvimLocationUnValid())
        //    {
        //        //Application.Current.Properties["takvim"] = Vakitler;
        //        Preferences.Set("enlem", Takvim.Enlem);
        //        Preferences.Set("boylam", Takvim.Boylam);
        //        Preferences.Set("yukseklik", Takvim.Yukseklik);
        //        Preferences.Set("saatbolgesi", Takvim.SaatBolgesi);
        //        Preferences.Set("yazkis", Takvim.YazKis);
        //        Preferences.Set("fecrikazip", Takvim.FecriKazip);
        //        Preferences.Set("fecrisadik", Takvim.FecriSadik);
        //        Preferences.Set("sabahsonu", Takvim.SabahSonu);
        //        Preferences.Set("ogle", Takvim.Ogle);
        //        Preferences.Set("ikindi", Takvim.Ikindi);
        //        Preferences.Set("aksam", Takvim.Aksam);
        //        Preferences.Set("yatsi", Takvim.Yatsi);
        //        Preferences.Set("yatsisonu", Takvim.YatsiSonu);
        //        Preferences.Set("tarih", Takvim.Tarih);
        //        //await Application.Current.SavePropertiesAsync();
        //        await GetCityAsync();
        //        //LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
        //    }
        //    else
        //    {
        //        if ((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 12))
        //            UserDialogs.Instance.Toast(AppResources.KonumIzniIcerik, TimeSpan.FromSeconds(5));
        //        else
        //            UserDialogs.Instance.Toast(AppResources.KonumKapali, TimeSpan.FromSeconds(5));
        //    }
        //}

        private async Task Settings()
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
                await Task.Delay(1000);
                if (Helper.HaveInternet())
                {
                    // Parse latitude and longitude as double values using InvariantCulture to avoid issues with different culture settings
                    // Get the placemarks for the current location
                    var placemarks = await Geocoding.GetPlacemarksAsync(Takvim.Enlem, Takvim.Boylam)
                        .ConfigureAwait(false);
                    Debug.WriteLine($"***** GetCityAsync Placemarks: {placemarks}");
                    // Get the first placemark, if any
                    var placemark = placemarks.FirstOrDefault();

                    if (placemark != null)
                    {
                        Debug.WriteLine($"***** GetCityAsync Placemark: {placemark}");
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