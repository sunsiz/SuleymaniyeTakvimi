using Acr.UserDialogs;
using Newtonsoft.Json;
//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Services
{
    public class DataService : IDataService, INotifyPropertyChanged
    {
        private Takvim _takvim;
        private IList<Takvim> _monthlyTakvim;
        private bool _askedLocationPermission;
        private bool _isPrepareMonthlyPrayerTimesCalled;
        private bool _hasCheckedLocationService;
        private bool _isTakvimInitialized;
        private readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ayliktakvim.json");
        private readonly Helper _helper;
        //public event Action<Takvim> OnTakvimChanged;
        //public event Action<IList<Takvim>> OnMonthlyTakvimChanged;
        private static readonly SemaphoreSlim PermissionSemaphore = new SemaphoreSlim(1, 1);
        private static readonly HttpClient Client = new HttpClient();

        //Avoid multiple concurrent requests for these methods
        private static readonly Dictionary<string, Task<Takvim>> OngoingGetPrayerTimeTasks = new Dictionary<string, Task<Takvim>>();
        private static readonly Dictionary<string, Task<IList<Takvim>>> OngoingGetMonthlyPrayerTimeTasks = new Dictionary<string, Task<IList<Takvim>>>();
        private static readonly Dictionary<string, Task<Location>> OngoingGetCurrentLocationTasks = new Dictionary<string, Task<Location>>();
        private static readonly object PrayerLockObject = new object();
        private static readonly object MonthlyPrayerLockObject = new object();
        private static readonly object LocationLockObject = new object();
        public static readonly Dictionary<string, string> KeyToName = new Dictionary<string, string>
        {
            { "fecrikazip", "FecriKazip" },
            { "fecrisadik" , "FecriSadik" },
            { "sabahsonu" , "SabahSonu" },
            { "ogle" , "Ogle" },
            { "ikindi" , "Ikindi" },
            { "aksam" , "Aksam" },
            { "yatsi" , "Yatsi" },
            { "yatsisonu" , "YatsiSonu" }
        };

        public static readonly string[] SoundNameKeys = { "kus", "horoz", "ezan", "alarm", "alarm2", "beep1", "beep2", "beep3" };
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        public Takvim Takvim
        {
            get => _takvim;
            set
            {
                if (_takvim != value)
                {
                    _takvim = value;
                    OnPropertyChanged(nameof(Takvim));
                }
            }
        }
        public IList<Takvim> MonthlyTakvim
        {
            get => _monthlyTakvim;
            set
            {
                if (_monthlyTakvim != value)
                {
                    _monthlyTakvim = value;
                    OnPropertyChanged(nameof(MonthlyTakvim));
                }
            }
            //set { if(_monthlyTakvim!=value){_monthlyTakvim = value;OnMonthlyTakvimChanged?.Invoke(_monthlyTakvim);} }
        }

        public Helper Helper => _helper;

        public DataService()
        {
            InitTakvim();
            _helper = new Helper(this);
        }
        
        public void InitTakvim()
        {
            if (_isTakvimInitialized)
            {
                // If Takvim is already initialized, just return
                return;
            }
            _takvim = GetPrayerTimesOfTodayFromFile();
            if (_takvim == null)
            {
                _takvim = new Takvim();
                Task.Run(async () => { await GetPrayerTimesAsync(false); });
                if (!_isPrepareMonthlyPrayerTimesCalled)
                {
                    _isPrepareMonthlyPrayerTimesCalled = true;
                    if (Preferences.Get("LocationSaved", false) && !Preferences.Get("AlwaysRenewLocationEnabled", false))
                    {
                        var location = new Location(Preferences.Get("LastLatitude", 41.0), Preferences.Get("LastLongitude", 29.0));
                        _ = GetMonthlyPrayerTimesAsync(location, false);
                    }
                    else Task.Run(async () => { await PrepareMonthlyPrayerTimes(); });
                }
            }
            if(Takvim!=null && !Takvim.IsTakvimLocationUnValid())
                _isTakvimInitialized = true;
            _takvim ??= new Takvim();
        }

        public async Task<Location> GetCurrentLocationAsync(bool refreshLocation)
        {
            var taskKey = $"{refreshLocation}";
            Task<Location> existingTask;
            lock (LocationLockObject)
            {
                existingTask = OngoingGetCurrentLocationTasks.ContainsKey(taskKey)
                    ? OngoingGetCurrentLocationTasks[taskKey]
                    : null;
            }

            if (existingTask != null)
            {
                return await existingTask;
            }

            var task = Task.Run(async () =>
            {
                var savedLocation = Preferences.Get("LocationSaved", false);
                var alwaysRenewLocationEnabled = Preferences.Get("AlwaysRenewLocationEnabled", false);
                if (!refreshLocation && savedLocation && !alwaysRenewLocationEnabled)
                {
                    if (Helper.IsValidLocation(GetSavedLocation()))
                    {
                        return GetSavedLocation();
                    }
                }

                if (!DependencyService.Get<IPermissionService>().IsLocationServiceEnabled())
                {
                    if (_hasCheckedLocationService && Helper.IsValidLocation(GetSavedLocation())) return GetSavedLocation();
                    UserDialogs.Instance.Alert(AppResources.KonumKapali, AppResources.KonumKapaliBaslik);
                    _hasCheckedLocationService = true;

                    if (Helper.IsValidLocation(GetSavedLocation()))
                    {
                        return GetSavedLocation();
                    }
                }

                try
                {
                    await PermissionSemaphore.WaitAsync();
                    if (!_askedLocationPermission)
                    {
                        var status = PermissionStatus.Denied;
                        await Device.InvokeOnMainThreadAsync(async () =>
                        {
                            status = await DependencyService.Get<IPermissionService>().HandlePermissionAsync();
                        });
                        if (status != PermissionStatus.Granted)
                        {
                            _askedLocationPermission = true;
                            UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                            if (Helper.IsValidLocation(GetSavedLocation()))
                            {
                                return GetSavedLocation();
                            }
                        }
                    }
                }
                finally
                {
                    PermissionSemaphore.Release();
                }

                Location location = null;
                try
                {
                    if (!refreshLocation) location = await Geolocation.GetLastKnownLocationAsync();

                    if (location == null || refreshLocation)
                    {
                        var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                        CancellationTokenSource cts = new CancellationTokenSource();
                        location = await Geolocation.GetLocationAsync(request, cts.Token);
                        Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {location}");
                    }

                    if (location != null && location.Latitude != 0.0 && location.Longitude != 0.0)
                    {
                        Debug.WriteLine(
                            $"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                        Preferences.Set("LastLatitude", location.Latitude);
                        Preferences.Set("LastLongitude", location.Longitude);
                        Preferences.Set("LastAltitude", location.Altitude ?? 0);
                        Preferences.Set("LocationSaved", true);
                    }
                    else
                    {
                        if (!DependencyService.Get<IPermissionService>().IsLocationServiceEnabled() &&
                            !_hasCheckedLocationService)
                            UserDialogs.Instance.Toast(AppResources.KonumKapaliBaslik, TimeSpan.FromSeconds(5));
                    }
                }
                catch (FeatureNotSupportedException fnsEx)
                {
                    UserDialogs.Instance.Alert(AppResources.CihazGPSDesteklemiyor, AppResources.CihazGPSDesteklemiyor);
                    Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {fnsEx.Message}");
                }
                catch (FeatureNotEnabledException fneEx)
                {
                    Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {fneEx.Message}");
                    if (!_hasCheckedLocationService)
                        UserDialogs.Instance.Alert(AppResources.KonumKapali, AppResources.KonumKapaliBaslik);
                    location = Helper.IsValidLocation(GetSavedLocation())? GetSavedLocation():null;
                    _hasCheckedLocationService = true;
                }
                catch (PermissionException pEx)
                {
                    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                    Debug.WriteLine(
                        $"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)} **** Permission Exception: {pEx.Message}");
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Alert(ex.Message, AppResources.KonumHatasi);
                    Debug.WriteLine(
                        $"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)} **** Unknown Exception: {ex.Message}");
                }

                return location != null ? location : Helper.IsValidLocation(GetSavedLocation())? GetSavedLocation():null;
            });
            lock (LocationLockObject)
            {
                OngoingGetCurrentLocationTasks[taskKey] = task;
            }

            var result = await task;
            lock (LocationLockObject)
            {
                OngoingGetCurrentLocationTasks.Remove(taskKey);
            }

            return result;
        }

        public static Location GetSavedLocation()
        {
            return new Location()
            {
                Latitude = Preferences.Get("LastLatitude", 0.0),
                Longitude = Preferences.Get("LastLongitude", 0.0),
                Altitude = Preferences.Get("LastAltitude", 0.0)
            };
        }


        private Takvim GetPrayerTimesOfTodayFromFile()
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    string json = ReadFileContentWithFileStream(_fileName);//File.ReadAllText(_fileName);
                    Debug.WriteLine($"**** GetPrayerTimesOfTodayFromFile json: {json}");
                    var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
                    if (takvims != null && DateTime.Parse(takvims[0].Tarih) <= DateTime.Today &&
                        DateTime.Parse(takvims[takvims.Count - 1].Tarih) >= DateTime.Today)
                    {
                        foreach (var item in takvims)
                        {
                            if (DateTime.Parse(item.Tarih).ToShortDateString() == DateTime.Today.ToShortDateString())
                            {
                                _takvim = item;
                                return _takvim;
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"**** GetPrayerTimesOfTodayFromFile can not get takvim from file, exception: {ex.Message}");
                }
            }
            return null;
            //Task.Run(async () => { await GetPrayerTimesAsync(false); });
            //if (!_isPrepareMonthlyPrayerTimesCalled)
            //{
            //    _isPrepareMonthlyPrayerTimesCalled = true;
            //    if (Preferences.Get("LocationSaved", false) && !Preferences.Get("AlwaysRenewLocationEnabled", false))
            //    {
            //        var location = new Location(Preferences.Get("LastLatitude", 41.0), Preferences.Get("LastLongitude", 29.0));
            //        _ = GetMonthlyPrayerTimesAsync(location, false);
            //    }
            //    else Task.Run(async () => { await PrepareMonthlyPrayerTimes(); });
            //}

            //return _takvim ?? new Takvim();
        }

        private async Task PrepareMonthlyPrayerTimes()
        {
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method triggered at {DateTime.Now.ToLongTimeString()}");
            var location = await GetCurrentLocationAsync(true);
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method get location at {DateTime.Now.ToLongTimeString()}");
            if (Helper.IsValidLocation(location))
            {
                await GetMonthlyPrayerTimesAsync(location, true);
                Debug.WriteLine(
                    $"**** PrepareMonthlyPrayerTimes method get monthly times at {DateTime.Now.ToLongTimeString()}");
                Takvim = GetPrayerTimesOfTodayFromFile();
            }

            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method finished at {DateTime.Now.ToLongTimeString()}");
        }
        
        public async Task<Takvim> GetPrayerTimesAsync(bool refreshLocation, bool tryFromFileFirst = false)
        {
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            if (!Services.Helper.HaveInternet())
            {
                //UserDialogs.Instance.Toast(AppResources.TakvimIcinInternetBaslik, TimeSpan.FromSeconds(5));
                return null;
            }
            string taskKey = $"{refreshLocation}-{tryFromFileFirst}";
            
            Task<Takvim> existingTask;

            lock (PrayerLockObject)
            {
                existingTask = OngoingGetPrayerTimeTasks.ContainsKey(taskKey) ? OngoingGetPrayerTimeTasks[taskKey] : null;
            }

            if (existingTask != null)
            {
                return await existingTask;
            }



            var task = Task.Run(async () =>
            {
                if (tryFromFileFirst)
                {
                    Takvim = GetPrayerTimesOfTodayFromFile();
                    if (Takvim != null) return Takvim;
                }

                try
                {
                    Location location;
                    if (Preferences.Get("LocationSaved", false) &&
                        !Preferences.Get("AlwaysRenewLocationEnabled", false))
                        location = new Location(Preferences.Get("LastLatitude", 41.0),
                            Preferences.Get("LastLongitude", 29.0));
                    else location = await GetCurrentLocationAsync(refreshLocation).ConfigureAwait(false);
                    Debug.WriteLine($"**** GetPrayerTimesAsync location: {location}");
                    if (Helper.IsValidLocation(location))
                    {
                        var prayerTime = await Helper.GetPrayerTimeOfTodayFromApiAsync(location).ConfigureAwait(false);
                        Debug.WriteLine($"**** GetPrayerTimesAsync prayerTime: {prayerTime.DisplayValues()}");
                        if (_takvim != null)
                        {
                            Takvim = prayerTime;
                            return prayerTime;
                        }
                        else
                        {
                            UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                        }
                    }
                }
                catch (Exception exception)
                {
                    UserDialogs.Instance.Alert(exception.Message, AppResources.NamazVaktiAlmaHatasi);
                    Debug.WriteLine("TimeStamp-GetPrayerTimes-Exception", exception.Message);
                }
                finally
                {
                    if (Takvim != null && !Takvim.IsTakvimLocationUnValid())
                        UserDialogs.Instance.Toast(AppResources.KonumYenilendi, TimeSpan.FromSeconds(3));
                }

                return Takvim;
            });
            lock (PrayerLockObject)
            {
                OngoingGetPrayerTimeTasks[taskKey] = task;
            }

            var result = await task;

            lock (PrayerLockObject)
            {
                OngoingGetPrayerTimeTasks.Remove(taskKey);
            }

            Debug.WriteLine("TimeStamp-GetPrayerTimes-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            return result;
        }
        
        private bool CheckRemindersEnabledAny()
        {
            string[] reminderKeys = new string[]
            {
                "fecrikazipEtkin",
                "fecrisadikEtkin",
                "sabahsonuEtkin",
                "ogleEtkin",
                "ikindiEtkin",
                "aksamEtkin",
                "yatsiEtkin",
                "yatsisonuEtkin"
            };

            return reminderKeys.Any(key => Preferences.Get(key, false));
        }

        public async Task<IList<Takvim>> GetMonthlyPrayerTimesAsync(Location location, bool forceRefresh)
        {
            Debug.WriteLine("TimeStamp-GetMonthlyPrayerTimes-Start",
                DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            string taskKey = $"{location.Latitude}-{location.Longitude}-{forceRefresh}";
            Task<IList<Takvim>> existingTask;
            lock (MonthlyPrayerLockObject)
            {
                existingTask = OngoingGetMonthlyPrayerTimeTasks.ContainsKey(taskKey)
                    ? OngoingGetMonthlyPrayerTimeTasks[taskKey]
                    : null;
            }

            if (existingTask != null)
            {
                return await existingTask;
            }

            var task = Task.Run(async () =>
            {
                if (!forceRefresh && File.Exists(_fileName))
                {
                    try
                    {
                        string json = ReadFileContentWithFileStream(_fileName); //File.ReadAllText(_fileName);
                        var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
                        if (takvims != null)
                        {
                            var days = (DateTime.Parse(takvims[^1].Tarih) - DateTime.Today).Days;
                            if (days >= 0)
                            {
                                _monthlyTakvim = takvims;
                                if (days < 7 && Services.Helper.HaveInternet())
                                {
                                    _monthlyTakvim = await Helper.GetMonthlyPrayerTimesFromApiAsync(location)
                                        .ConfigureAwait(false);
                                }

                                return _monthlyTakvim;
                            }
                        }
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine(
                            $"An error occurred while reading or parsing the file, details: {exception.Message}");
                    }
                }

                if (!Services.Helper.HaveInternet()) return _monthlyTakvim;
                try
                {
                    _monthlyTakvim = await Helper.GetMonthlyPrayerTimesFromApiAsync(location).ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    //UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
                    Debug.WriteLine(
                        $"An error occurred while downloading or parsing the json file, details: {exception.Message}");
                }

                return _monthlyTakvim;
            });
            lock (MonthlyPrayerLockObject)
            {
                OngoingGetMonthlyPrayerTimeTasks[taskKey] = task;
            }

            var result = await task;
            lock (MonthlyPrayerLockObject)
            {
                OngoingGetMonthlyPrayerTimeTasks.Remove(taskKey);
            }

            Debug.WriteLine("TimeStamp-GetMonthlyPrayerTimes-Finish",
                DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            return result;
        }

        public async Task SetWeeklyAlarmsAsync()
        {
            Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            DependencyService.Get<IAlarmService>().CancelAlarm();

            if (!CheckRemindersEnabledAny())
            {
                return;
            }

            try
            {
                if (!File.Exists(_fileName) || IsFileOutdated())
                {
                    Location location;
                    if (!Preferences.Get("LocationSaved", false) || Preferences.Get("AlwaysRenewLocationEnabled", false))
                        location = await GetCurrentLocationAsync(false);
                    else location = new Location(Preferences.Get("LastLatitude", 41.0), Preferences.Get("LastLongitude", 29.0));

                    if (Helper.IsValidLocation(location))
                    {
                        _monthlyTakvim = await GetMonthlyPrayerTimesAsync(location, false).ConfigureAwait(false);
                        if (_monthlyTakvim == null)
                        {
                            await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
                                AppResources.TakvimIcinInternetBaslik);
                            return;
                        }
                    }
                }

                if (_monthlyTakvim != null)
                {
                    SetAlarms();
                }
                else
                {
                    UserDialogs.Instance.Toast(AppResources.AylikTakvimeErisemedi);
                }
            }
            catch (Exception exception)
            {
                await UserDialogs.Instance.AlertAsync(exception.Message, AppResources.Alarmkurarkenhataolustu);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(SetWeeklyAlarmsAsync)}: {exception.Message}");
            }

            Preferences.Set("LastAlarmDate", DateTime.Today.AddDays(14).ToShortDateString());

            Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private bool IsFileOutdated()
        {
            string json = ReadFileContentWithFileStream(_fileName);
            var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
            if (takvims != null && (DateTime.Parse(takvims[takvims.Count - 1].Tarih) - DateTime.Today).Days > 3)
            {
                _monthlyTakvim = takvims;
                return false;
            }
            return true;
        }
        
        private void SetAlarms()
        {
            int dayCounter = 0;
            foreach (Takvim todayTakvim in _monthlyTakvim)
            {
                if (DateTime.Parse(todayTakvim.Tarih) >= DateTime.Today)
                {
                    SetAlarmForPrayerTime(todayTakvim);
                    dayCounter++;
                    if (dayCounter >= 15) break;
                }
            }
        }
        
        private static void SetAlarmForPrayerTime(Takvim todayPrayerTime)
        {
            if(todayPrayerTime == null) return;
            Debug.WriteLine("TimeStamp-SetAlarmForPrayerTime-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            var prayerTimes = new Dictionary<string, (string, int)>
            {
                { "fecrikazip", (todayPrayerTime.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", 0)) },
                { "fecrisadik", (todayPrayerTime.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", 0)) },
                { "sabahsonu", (todayPrayerTime.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", 0)) },
                { "ogle", (todayPrayerTime.Ogle, Preferences.Get("ogleBildirmeVakti", 0)) },
                { "ikindi", (todayPrayerTime.Ikindi, Preferences.Get("ikindiBildirmeVakti", 0)) },
                { "aksam", (todayPrayerTime.Aksam, Preferences.Get("aksamBildirmeVakti", 0)) },
                { "yatsi", (todayPrayerTime.Yatsi, Preferences.Get("yatsiBildirmeVakti", 0)) },
                { "yatsisonu", (todayPrayerTime.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", 0)) }
            };

            foreach (var prayerTime in prayerTimes)
            {
                var prayerTimeSpan = TimeSpan.Parse(prayerTime.Value.Item1);
                var alarmTime = DateTime.Parse(todayPrayerTime.Tarih) + prayerTimeSpan - TimeSpan.FromMinutes(prayerTime.Value.Item2);

                if (DateTime.Now < alarmTime && Preferences.Get($"{prayerTime.Key}Etkin", false))
                {
                    DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayPrayerTime.Tarih), prayerTimeSpan, prayerTime.Value.Item2, prayerTime.Key);
                }
            }
            Debug.WriteLine("TimeStamp-SetAlarmForPrayerTime-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private static string ReadFileContentWithFileStream(string fileName)
        {
            try
            {
                using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException ex)
            {
                //UserDialogs.Instance.AlertAsync(ex.Message, AppResources.NamazVaktiAlmaHatasi);
                Debug.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return string.Empty;
            }
        }

    }
}