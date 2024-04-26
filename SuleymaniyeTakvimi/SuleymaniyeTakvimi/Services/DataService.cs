using Acr.UserDialogs;
using Newtonsoft.Json;
//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
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
    public class DataService : IDataService
    {
        private Takvim _takvim;
        private IList<Takvim> _monthlyTakvim;
        private bool _askedLocationPermission;
        private bool _isPrepareMonthlyPrayerTimesCalled;
        private readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ayliktakvim.json");
        public event Action<Takvim> OnTakvimChanged;
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
        //public static readonly Dictionary<string, string> NameToKey = KeyToName.ToDictionary(x => x.Value, x => x.Key);

        //public static readonly string[] TimeNameKeys = { "fecrikazip", "fecrisadik", "sabahsonu", "ogle", "ikindi", "aksam", "yatsi", "yatsisonu" };

        public static readonly string[] SoundNameKeys = { "kus", "horoz", "ezan", "alarm", "alarm2", "beep1", "beep2", "beep3" };
        private static readonly SemaphoreSlim PermissionSemaphore = new SemaphoreSlim(1, 1);

        public Takvim Takvim
        {
            get => _takvim;
            set
            {
                if (_takvim != value)
                {
                    _takvim = value;
                    OnTakvimChanged?.Invoke(_takvim);
                }
            }
        }
        public DataService()
        {
            InitTakvim();
        }

        /// <summary>
        /// This method initializes the _takvim object.
        /// </summary>
        /// <remarks>
        /// The method first tries to get the Takvim object from a file. If the file does not exist or cannot be read, a new Takvim object is created.
        /// The new Takvim object is initialized with values from the application's preferences. If a preference does not exist, a default value is used.
        /// The properties of the Takvim object include geographical information (latitude, longitude, altitude), time zone information, daylight saving time information, and prayer times.
        /// The Tarih property is set to the current date in "yyyy-MM-dd" format if there is no value in the preferences.
        /// </remarks>
        public void InitTakvim()
        {
            _takvim = GetTakvimFromFile() ?? new Takvim();
        }

        /// <summary>
        /// Asynchronously retrieves the current location of the device.
        /// </summary>
        /// <param name="refreshLocation">A boolean indicating whether to force a refresh of the location data.</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The Task result contains a Location object with the current location data.
        /// </returns>
        /// <remarks>
        /// This method first checks if the location has saved before and not require force refresh and not always refresh location settings enabled then return the saved location.
        /// If the permission has been asked before. If not, it requests the location permission.
        /// If the permission is not granted, it alerts the user and returns the default location.
        /// If the permission is granted, it tries to get the last known location if refreshLocation is false.
        /// If the last known location is null or refreshLocation is true, it makes a new request for the current location.
        /// If the location is successfully retrieved and is not a default location, it saves the location data in the application's preferences.
        /// If the location service is not enabled, it alerts the user.
        /// The method handles various exceptions that can occur when requesting the location, such as the feature not being supported on the device, the feature not being enabled, or the permission not being granted.
        /// </remarks>
        public async Task<Location> GetCurrentLocationAsync(bool refreshLocation)
        {
            var savedLocation = Preferences.Get("LocationSaved", false);
            var alwaysRenewLocationEnabled = Preferences.Get("AlwaysRenewLocationEnabled", false);
            if (!refreshLocation && savedLocation && !alwaysRenewLocationEnabled)
            {
                return GetSavedLocation();
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
                        UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                        _askedLocationPermission = true;
                        return GetSavedLocation();
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
                if (!refreshLocation) location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

                if (location == null || refreshLocation)
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(false);
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
                    if (!DependencyService.Get<IPermissionService>().IsLocationServiceEnabled())
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
                UserDialogs.Instance.Alert(AppResources.KonumKapali, AppResources.KonumKapaliBaslik);
                location = GetSavedLocation();
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

            return location ?? GetSavedLocation();
        }

        private Location GetSavedLocation()
        {
            return new Location()
            {
                Latitude = Preferences.Get("LastLatitude", 0.0),
                Longitude = Preferences.Get("LastLongitude", 0.0)
            };
        }

        public async Task<string> GetJsonFromApiAsync(string endpoint, Dictionary<string, string> parameters)
        {
            using var client = new HttpClient();
            var uri = new Uri($"https://api.suleymaniyetakvimi.com/api/{endpoint}?" +
                              string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));

            var response = await client.GetAsync(uri);
            var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return jsonResult;
        }

        /// <summary>
        /// This method retrieves the Takvim object from a file.
        /// </summary>
        /// <returns>
        /// The Takvim object if it exists in the file and the today's date is within the range, otherwise null.
        /// </returns>
        /// <remarks>
        /// The method first checks if the file exists. If it does, it reads the file and deserializes the JSON content into a list of Takvim objects.
        /// It then checks if the date of the first and last Takvim objects in the list is include today's date.
        /// If it is, it iterates over the list to find the Takvim object for the current day and returns it.
        /// If the file does not exist or an exception occurs while reading the file, it calls the GetPrayerTimesAsync method to get today's prayer time and also calls the PrepareMonthlyPrayerTimes method to prepare the prayer times for the current month.
        /// </remarks>
        private Takvim GetTakvimFromFile()
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    string json = ReadFileContentWithFileStream(_fileName);//File.ReadAllText(_fileName);
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
                    Debug.WriteLine($"**** GetTakvimFromFile can not get takvim from file, exception: {ex.Message}");
                }
            }

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

            return _takvim ?? new Takvim();
        }

        /// <summary>
        /// This asynchronous method prepares the prayer times for the current month.
        /// </summary>
        /// <returns>
        /// A Task that represents the asynchronous operation. The Task result contains a Takvim object with the prayer times for the current date.
        /// </returns>
        /// <remarks>
        /// The method first gets the current location of the device by calling the GetCurrentLocationAsync method with true as the parameter to force a refresh of the location data.
        /// It then calls the GetMonthlyPrayerTimes method with the location and true as the parameters to force a refresh of the prayer times.
        /// Finally, it retrieves the Takvim object from a file by calling the GetTakvimFromFile method and returns it.
        /// </remarks>
        private async Task PrepareMonthlyPrayerTimes()
        {
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method triggered at {DateTime.Now.ToLongTimeString()}");
            var location = await GetCurrentLocationAsync(true);
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method get location at {DateTime.Now.ToLongTimeString()}");
            await GetMonthlyPrayerTimesAsync(location, true);
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method get monthly times at {DateTime.Now.ToLongTimeString()}");
            _takvim = GetTakvimFromFile();
            Debug.WriteLine($"**** PrepareMonthlyPrayerTimes method finished at {DateTime.Now.ToLongTimeString()}");
        }

        /// <summary>
        /// This asynchronous method retrieves the prayer times for the current day.
        /// </summary>
        /// <param name="refreshLocation">A boolean indicating whether to force a refresh of the location data not using last known location.</param>
        /// <param name="tryFromFileFirst">A boolean indicating whether to try read the object from the json file first or just get it from the API</param>
        /// <returns>
        /// A Task that represents the asynchronous operation. The Task result contains a Takvim object with the prayer times for the current day.
        /// </returns>
        /// <remarks>
        /// The method first checks if there is an internet connection. If there is no internet connection, it returns null.
        /// It then gets the current location of the device by calling the GetCurrentLocationAsync method with the refreshLocation parameter.
        /// It makes a GET request to an API that calculates the prayer times based on the location and the current date.
        /// The response from the API is a JSON string that is deserialized into a Takvim object.
        /// If the JSON string is null or empty, it alerts the user.
        /// The method handles any exceptions that occur during the process and alerts the user if an exception occurs.
        /// Finally, it returns the Takvim object.
        /// </remarks>
        public async Task<Takvim> GetPrayerTimesAsync(bool refreshLocation, bool tryFromFileFirst = false)
        {
            Debug.WriteLine("TimeStamp-GetPrayerTimes-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            if (!HaveInternet())
            {
                UserDialogs.Instance.Toast(AppResources.TakvimIcinInternetBaslik, TimeSpan.FromSeconds(5));
                return null;
            }

            if (tryFromFileFirst)
            {
                _takvim = GetTakvimFromFile();
                if (_takvim != null) return _takvim;
            }

            try
            {
                Location location;
                if (Preferences.Get("LocationSaved", false) && !Preferences.Get("AlwaysRenewLocationEnabled", false))
                    location = new Location(Preferences.Get("LastLatitude", 41.0), Preferences.Get("LastLongitude", 29.0));
                else location = await GetCurrentLocationAsync(refreshLocation);
                if (location != null && location.Latitude != 0.0 && location.Longitude != 0.0)
                {
                    var parameters = new Dictionary<string, string>
                    {
                        { "latitude", location.Latitude.ToString() },
                        { "longitude", location.Longitude.ToString() },
                        { "date", DateTime.Today.ToString("yyyy-MM-dd") }
                    };
                    var jsonResult = await GetJsonFromApiAsync("TimeCalculation/TimeCalculate", parameters);
                    if (!string.IsNullOrEmpty(jsonResult))
                    {
                        Debug.WriteLine("TimeStamp-GetPrayerTimes-JsonResult", jsonResult);
                        _takvim = JsonConvert.DeserializeObject<Takvim>(jsonResult, new TakvimConverter());
                    }
                    else
                    {
                        UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                    }
                }
            }
            catch (Exception exception)
            {
                UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
            }
            finally
            {
                UserDialogs.Instance.Toast(AppResources.KonumYenilendi, TimeSpan.FromSeconds(3));
            }

            Debug.WriteLine("TimeStamp-GetPrayerTimes-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            return _takvim;
        }

        /// <summary>
        /// Checks if any prayer time reminders are enabled in the application preferences.
        /// </summary>
        /// <returns>
        /// A boolean indicating whether any prayer time reminders are enabled. Returns true if at least one reminder is enabled, false otherwise.
        /// </returns>
        /// <remarks>
        /// The method checks the application preferences for each prayer time (Fecr-i Kazip, Fecr-i Sadik, Sabah Sonu, Ogle, Ikindi, Aksam, Yatsi, Yatsi Sonu).
        /// If the preference for a prayer time is set to true, it means that the reminder for that prayer time is enabled.
        /// If the preference for all prayer times is set to false, it means that no reminders are enabled.
        /// </remarks>
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

        /// <summary>
        /// Retrieves the monthly prayer times (Takvim) for a given location.
        /// </summary>
        /// <param name="location">The location for which to retrieve the prayer times.</param>
        /// <param name="forceRefresh">If true, the method will ignore any cached data and retrieve fresh data from the API.</param>
        /// <returns>
        /// A list of Takvim objects representing the prayer times for each day of the current month. 
        /// If there is no internet connection and no cached data, the method returns null.
        /// </returns>
        /// <remarks>
        /// The method first checks if there is a file with cached prayer times and if the forceRefresh parameter is false. 
        /// If both conditions are met, it tries to read the prayer times from the file.
        /// If the file does not exist, cannot be read, or the data has only last two weeks left (contains two month's data), the method makes a GET request to an API to retrieve the prayer times.
        /// The API response is a JSON string that is deserialized into a list of Takvim objects.
        /// The method also writes the retrieved prayer times to a file for future use.
        /// </remarks>
        public async Task<IList<Takvim>> GetMonthlyPrayerTimesAsync(Location location, bool forceRefresh)
        {
            if (File.Exists(_fileName) && !forceRefresh)
            {
                try
                {
                    string json = ReadFileContentWithFileStream(_fileName);//File.ReadAllText(_fileName);
                    var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
                    if (takvims != null)
                    {
                        var days = (DateTime.Parse(takvims[takvims.Count - 1].Tarih) - DateTime.Today).Days;
                        if (days >= 0)
                        {
                            _monthlyTakvim = takvims;
                            if (days < 14 && HaveInternet())
                            {
                                _monthlyTakvim = await GetMonthlyPrayerTimesFromApiAsync(location);
                            }
                            return _monthlyTakvim;
                        }
                    }
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"An error occurred while reading or parsing the file, details: {exception.Message}");
                }
            }

            if (HaveInternet())
            {
                try
                {
                    _monthlyTakvim = await GetMonthlyPrayerTimesFromApiAsync(location);
                }
                catch (Exception exception)
                {
                    UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
                    Debug.WriteLine($"An error occurred while downloading or parsing the json file, details: {exception.Message}");
                }
            }

            return _monthlyTakvim;
        }

        private async Task<IList<Takvim>> GetMonthlyPrayerTimesFromApiAsync(Location location)
        {
            using var client = new HttpClient();
            var monthlyCalendar = new List<Takvim>();
            var jsonContent = string.Empty;
            var parameters = new Dictionary<string, string>
                    {
                        { "latitude", location.Latitude.ToString() },
                        { "longitude", location.Longitude.ToString() },
                        { "monthId", DateTime.Today.Month.ToString() },
                        { "year", DateTime.Today.Year.ToString() }
                    };
            var jsonResult = await GetJsonFromApiAsync("TimeCalculation/TimeCalculateByMonth", parameters);
            if (!string.IsNullOrEmpty(jsonResult))
            {
                jsonContent += jsonResult;
                parameters = new Dictionary<string, string>
                        {
                            { "latitude", location.Latitude.ToString() },
                            { "longitude", location.Longitude.ToString() },
                            { "monthId", DateTime.Today.AddMonths(1).Month.ToString() },
                            { "year", DateTime.Today.AddMonths(1).Year.ToString() }
                        };
                jsonResult = await GetJsonFromApiAsync("TimeCalculation/TimeCalculateByMonth", parameters);
                if (!string.IsNullOrEmpty(jsonResult)) jsonContent = jsonContent.Replace("}]", "},") + jsonResult.Replace("[{", "{");
                var takvims = JsonConvert.DeserializeObject<List<Takvim>>(jsonContent, new TakvimConverter());
                monthlyCalendar.AddRange(takvims);
            }
            else
            {
                UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
            }

            // Write the updated prayer times to the file.
            using var fileStream = new FileStream(_fileName, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);
            using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(jsonContent);
            return monthlyCalendar;
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

        /// <summary>
        /// Asynchronously sets weekly alarms for prayer times.
        /// This method first cancels any existing alarms, then checks if any reminders are enabled.
        /// If reminders are enabled, it checks if the prayer times file exists and is up-to-date.
        /// If the file doesn't exist or is outdated, it fetches the current location and gets the monthly prayer times for that location.
        /// If the monthly prayer times are available, it sets the alarms for the prayer times.
        /// If the monthly prayer times are not available, it shows a toast message to the user.
        /// If an exception occurs during this process, it logs the exception message and show it to the user.
        /// Finally, it sets the date for the next alarm update to 14 days in the future.
        /// </summary>
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

                    if (location != null && location.Latitude != 0.0 && location.Longitude != 0.0)
                    {
                        _monthlyTakvim = await GetMonthlyPrayerTimesAsync(location, false);
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
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {exception.Message}");
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

        /// <summary>
        /// Sets alarms for the prayer times for the next 15 days.
        /// It iterates over the monthly prayer times and sets an alarm for each day's prayer time.
        /// The loop breaks after setting alarms for 15 days.
        /// </summary>
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

        /// <summary>
        /// Sets an alarm for each prayer time of the given day.
        /// It creates a dictionary of prayer times and their corresponding alarm times.
        /// Then it iterates over the dictionary and sets an alarm for each prayer time.
        /// The alarm is set only if the alarm time is in the future and the corresponding prayer time is enabled in the preferences.
        /// </summary>
        /// <param name="todayTakvim">The prayer times for the day.</param>
        private void SetAlarmForPrayerTime(Takvim todayTakvim)
        {
            Debug.WriteLine("TimeStamp-SetAlarmForPrayerTime-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            var prayerTimes = new Dictionary<string, (string, int)>
            {
                { "fecrikazip", (todayTakvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", 0)) },
                { "fecrisadik", (todayTakvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", 0)) },
                { "sabahsonu", (todayTakvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", 0)) },
                { "ogle", (todayTakvim.Ogle, Preferences.Get("ogleBildirmeVakti", 0)) },
                { "ikindi", (todayTakvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", 0)) },
                { "aksam", (todayTakvim.Aksam, Preferences.Get("aksamBildirmeVakti", 0)) },
                { "yatsi", (todayTakvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", 0)) },
                { "yatsisonu", (todayTakvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", 0)) }
            };

            foreach (var prayerTime in prayerTimes)
            {
                var prayerTimeSpan = TimeSpan.Parse(prayerTime.Value.Item1);
                var alarmTime = DateTime.Parse(todayTakvim.Tarih) + prayerTimeSpan - TimeSpan.FromMinutes(prayerTime.Value.Item2);

                if (DateTime.Now < alarmTime && Preferences.Get($"{prayerTime.Key}Etkin", false))
                {
                    DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), prayerTimeSpan, prayerTime.Value.Item2, prayerTime.Key);
                }
            }
            Debug.WriteLine("TimeStamp-SetAlarmForPrayerTime-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private string ReadFileContentWithFileStream(string fileName)
        {
            try
            {
                using var stream = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                using var reader = new StreamReader(stream);
                return reader.ReadToEnd();
            }
            catch (IOException ex)
            {
                Debug.WriteLine($"An error occurred while reading the file: {ex.Message}");
                return string.Empty;
            }
        }

    }
}