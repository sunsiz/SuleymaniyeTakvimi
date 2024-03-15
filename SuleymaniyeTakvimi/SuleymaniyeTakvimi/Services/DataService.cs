using Acr.UserDialogs;
using Newtonsoft.Json;

//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Services
{
    public class DataService : IDataService
    {
        public Takvim _takvim;
        private IList<Takvim> _monthlyTakvim;
        private bool askedLocationPermission;
        private bool isPrepareMonthlyPrayerTimesCalled;
        public readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ayliktakvim.json");

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

            if (!askedLocationPermission)
            {
                var status = await DependencyService.Get<IPermissionService>().HandlePermissionAsync().ConfigureAwait(false);
                if (status != PermissionStatus.Granted)
                {
                    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                    askedLocationPermission = true;
                    return GetSavedLocation();
                }
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
                    Debug.WriteLine($"Latitude: {location.Latitude}, Longitude: {location.Longitude}, Altitude: {location.Altitude}");
                    Preferences.Set("LastLatitude", location.Latitude);
                    Preferences.Set("LastLongitude", location.Longitude);
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
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)} **** Permission Exception: {pEx.Message}");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, AppResources.KonumHatasi);
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)} **** Unknown Exception: {ex.Message}");
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
        /// If the file does not exist or an exception occurs while reading the file, it calls the PrepareMonthlyPrayerTimes method to prepare the prayer times for the current month.
        /// </remarks>
        private Takvim GetTakvimFromFile()
        {
            if (File.Exists(_fileName))
            {
                try
                {
                    string json = File.ReadAllText(_fileName);
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
                    Debug.WriteLine(ex.Message);
                }
            }

            if (!isPrepareMonthlyPrayerTimesCalled)
            {
                isPrepareMonthlyPrayerTimesCalled = true;
                Task.Run(async () => { await PrepareMonthlyPrayerTimes().ConfigureAwait(false); });
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
        public async Task<Takvim> PrepareMonthlyPrayerTimes()
        {
            var location = await GetCurrentLocationAsync(true).ConfigureAwait(false);
            GetMonthlyPrayerTimes(location, true);
            _takvim = GetTakvimFromFile();
            return _takvim;
        }

        /// <summary>
        /// This asynchronous method retrieves the prayer times for the current day.
        /// </summary>
        /// <param name="refreshLocation">A boolean indicating whether to force a refresh of the location data not using last known location.</param>
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
                var location = await GetCurrentLocationAsync(refreshLocation).ConfigureAwait(false);
                if (location != null && location.Latitude != 0.0 && location.Longitude != 0.0)
                {
                    using var client = new HttpClient();

                    var uri = new Uri($"https://api.suleymaniyetakvimi.com/api/TimeCalculation/TimeCalculate?" +
                                      $"latitude={location.Latitude}" +
                                      $"&longitude={location.Longitude}" +
                                      $"&date={DateTime.Today:yyyy-MM-dd}");

                    var response = await client.GetAsync(uri);
                    var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
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
        /// If the file does not exist, cannot be read, or the data is more than 21 days old, the method makes a GET request to an API to retrieve the prayer times.
        /// The API response is a JSON string that is deserialized into a list of Takvim objects.
        /// The method also writes the retrieved prayer times to a file for future use.
        /// </remarks>
        public IList<Takvim> GetMonthlyPrayerTimes(Location location, bool forceRefresh)
        {
            //Analytics.TrackEvent("GetMonthlyPrayerTimes in the DataService Triggered: " + $" at {DateTime.Now}");
            if (File.Exists(_fileName) && !forceRefresh)
            {
                try
                {
                    string json = File.ReadAllText(_fileName);
                    var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
                    if (takvims != null)
                    {
                        var days = (DateTime.Today - DateTime.Parse(takvims[0].Tarih)).Days;
                        if (days is < 21 and >= 0)
                        {
                            _monthlyTakvim = takvims;
                            return _monthlyTakvim;
                        }
                    }

                    if (!HaveInternet()) return _monthlyTakvim = takvims;
                }
                catch (Exception exception)
                {
                    Debug.WriteLine($"An error occurred while reading or parsing the file, details: {exception.Message}");
                }
            }

            if (!HaveInternet()) return null;
            
            try
            {
                using var client = new HttpClient();
                var monthlyCalendar = new List<Takvim>();
                var jsonContent = string.Empty;
                
                var monthId = DateTime.Today.Month;
                var year = DateTime.Today.Year;
                    var uri = new Uri($"https://api.suleymaniyetakvimi.com/api/TimeCalculation/TimeCalculateByMonth?" +
                                      $"latitude={location.Latitude}" +
                                      $"&longitude={location.Longitude}" +
                                      $"&monthId={monthId}" +
                                      $"&year={year}");

                    var response = client.GetAsync(uri).GetAwaiter().GetResult();
                    var jsonResult = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                    if (!string.IsNullOrEmpty(jsonResult))
                    {
                        jsonContent += jsonResult;
                        var takvims = JsonConvert.DeserializeObject<List<Takvim>>(jsonResult, new TakvimConverter());
                        monthlyCalendar.AddRange(takvims);
                    }
                    else
                    {
                        UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                    }
                
                _monthlyTakvim = monthlyCalendar;
                // Write the updated prayer times to the file.
                using var streamWriter = new StreamWriter(_fileName, false);
                streamWriter.Write(jsonContent);
                return monthlyCalendar;
            }
            catch (Exception exception)
            {
                UserDialogs.Instance.Alert(exception.Message, AppResources.KonumHatasi);
                System.Diagnostics.Debug.WriteLine(
                    $"An error occurred while downloading or parsing the json file, details: {exception.Message}");
            }

            return _monthlyTakvim;
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
                    var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
                    if (location != null && location.Latitude != 0.0 && location.Longitude != 0.0)
                    {
                        _monthlyTakvim = GetMonthlyPrayerTimes(location, false);
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
            string json = File.ReadAllText(_fileName);
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

        private void SetAlarmForPrayerTime(Takvim todayTakvim)
        {
            var prayerTimes = new Dictionary<string, (string, int)>
            {
                { "Fecri Kazip", (todayTakvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", 0)) },
                { "Fecri Sadık", (todayTakvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", 0)) },
                { "Sabah Sonu", (todayTakvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", 0)) },
                { "Öğle", (todayTakvim.Ogle, Preferences.Get("ogleBildirmeVakti", 0)) },
                { "İkindi", (todayTakvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", 0)) },
                { "Akşam", (todayTakvim.Aksam, Preferences.Get("aksamBildirmeVakti", 0)) },
                { "Yatsı", (todayTakvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", 0)) },
                { "Yatsı Sonu", (todayTakvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", 0)) }
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
        }
        
        //private void WriteTakvimFile(string fileContent)
        //{
        //    //using (Stream stream = this.GetType().Assembly.
        //    //    GetManifestResourceStream("SuleymaniyeTakvimi.Assets.ayliktakvim.xml"))
        //    //{
        //    //    using (StreamWriter sr = new StreamWriter(stream))
        //    //    {
        //    //        sr.WriteAsync(fileContent);
        //    //    }
        //    //}
        //    File.WriteAllText(_fileName, fileContent);
        //}

        /// <summary>
        /// This asynchronous method calculates the prayer times (Vakit) for the current day.
        /// </summary>
        /// <returns>
        /// A Task that represents the asynchronous operation. The Task result contains a Takvim object with the calculated prayer times.
        /// </returns>
        /// <remarks>
        /// The method first tries to get the Takvim object from a file. If the file does not exist or cannot be read, a new Takvim object is created.
        /// If there is no internet connection, the method returns null.
        /// If there is an internet connection, the method gets the current location and makes a GET request to an API that calculates the prayer times based on the location and the current date.
        /// The response from the API is a JSON string that is deserialized into a Takvim object.
        /// </remarks>
        //public async Task<Takvim> VakitHesabiAsync()
        //{
        //    _takvim = GetTakvimFromFile();
        //    if (_takvim != null) return _takvim;
        //    _takvim = new Takvim();

        //    if (!HaveInternet()) return null;
        //    var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
        //    if (location != null)
        //    {
        //        var date = DateTime.Today.ToString("yyyy-MM-dd");
        //        var url = $"https://api.suleymaniyetakvimi.com/api/TimeCalculation/TimeCalculate?latitude={location.Latitude}&longitude={location.Longitude}&date={date}";

        //        using (HttpClient client = new HttpClient())
        //        {
        //            var response = await client.GetStringAsync(url).ConfigureAwait(false);
        //            Debug.WriteLine("TimeStamp-VakitHesabiAsync-JsonResult", response);
        //            _takvim = JsonConvert.DeserializeObject<Takvim>(response, new TakvimConverter());
        //        }
        //    }

        //    return _takvim;
        //}

        //private Takvim ParseXml(string xmlResult)
        //{
        //    _takvim = new Takvim();
        //    XDocument doc = XDocument.Parse(xmlResult);
        //    //if(doc.Descendants("Takvim")!=null)
        //    if (doc.Root == null) return _takvim;
        //    foreach (var item in doc.Root.Descendants())
        //    {
        //        switch (item.Name.LocalName)
        //        {
        //            case "Enlem":
        //                _takvim.Enlem = Convert.ToDouble(item.Value);
        //                break;
        //            case "Boylam":
        //                _takvim.Boylam = Convert.ToDouble(item.Value);
        //                break;
        //            case "Yukseklik":
        //                _takvim.Yukseklik = Convert.ToDouble(item.Value);
        //                break;
        //            case "SaatBolgesi":
        //                _takvim.SaatBolgesi = Convert.ToDouble(item.Value);
        //                break;
        //            case "YazKis":
        //                _takvim.YazKis = Convert.ToDouble(item.Value);
        //                break;
        //            case "FecriKazip":
        //                _takvim.FecriKazip = item.Value;
        //                break;
        //            case "FecriSadik":
        //                _takvim.FecriSadik = item.Value;
        //                break;
        //            case "SabahSonu":
        //                _takvim.SabahSonu = item.Value;
        //                break;
        //            case "Ogle":
        //                _takvim.Ogle = item.Value;
        //                break;
        //            case "Ikindi":
        //                _takvim.Ikindi = item.Value;
        //                break;
        //            case "Aksam":
        //                _takvim.Aksam = item.Value;
        //                break;
        //            case "Yatsi":
        //                _takvim.Yatsi = item.Value;
        //                break;
        //            case "YatsiSonu":
        //                _takvim.YatsiSonu = item.Value;
        //                break;
        //        }
        //    }

        //    return _takvim;
        //}
        
        //private IList<Takvim> ParseXmlList(XDocument doc, double enlem=0.0, double boylam=0.0, double yukseklik=0.0)
        //{
        //    IList<Takvim> monthlyTakvim = new ObservableCollection<Takvim>();
        //    //XDocument doc = XDocument.Parse(xmlResult);
        //    //if(doc.Descendants("Takvim")!=null)
        //    if (doc.Root == null) return monthlyTakvim;
        //    foreach (var item in doc.Root.Descendants())
        //    {
        //        if (item.Name.LocalName == "TakvimListesi")
        //        {
        //            var takvimItem = new Takvim();
        //            foreach (var subitem in item.Descendants())
        //            {
        //                switch (subitem.Name.LocalName)
        //                {
        //                    case "Tarih":
        //                        takvimItem.Tarih = subitem.Value;
        //                        break;
        //                    case "Enlem":
        //                        takvimItem.Enlem = Convert.ToDouble(subitem.Value);
        //                        break;
        //                    case "Boylam":
        //                        takvimItem.Boylam = Convert.ToDouble(subitem.Value);
        //                        break;
        //                    case "Yukseklik":
        //                        takvimItem.Yukseklik = Convert.ToDouble(subitem.Value);
        //                        break;
        //                    case "SaatBolgesi":
        //                        takvimItem.SaatBolgesi = Convert.ToDouble(subitem.Value);
        //                        break;
        //                    case "YazKis":
        //                        takvimItem.YazKis = Convert.ToDouble(subitem.Value);
        //                        break;
        //                    case "FecriKazip":
        //                        takvimItem.FecriKazip = subitem.Value;
        //                        break;
        //                    case "FecriSadik":
        //                        takvimItem.FecriSadik = subitem.Value;
        //                        break;
        //                    case "SabahSonu":
        //                        takvimItem.SabahSonu = subitem.Value;
        //                        break;
        //                    case "Ogle":
        //                        takvimItem.Ogle = subitem.Value;
        //                        break;
        //                    case "Ikindi":
        //                        takvimItem.Ikindi = subitem.Value;
        //                        break;
        //                    case "Aksam":
        //                        takvimItem.Aksam = subitem.Value;
        //                        break;
        //                    case "Yatsi":
        //                        takvimItem.Yatsi = subitem.Value;
        //                        break;
        //                    case "YatsiSonu":
        //                        takvimItem.YatsiSonu = subitem.Value;
        //                        break;
        //                }
        //            }

        //            takvimItem.Enlem = takvimItem.Enlem == 0 ? enlem : takvimItem.Enlem;
        //            takvimItem.Boylam = takvimItem.Boylam == 0 ? boylam : takvimItem.Boylam;
        //            takvimItem.Yukseklik = takvimItem.Yukseklik == 0 ? yukseklik : takvimItem.Yukseklik;
        //            monthlyTakvim.Add(takvimItem);
        //        }
        //    }

        //    return monthlyTakvim;
        //}

        /// <summary>
        /// This method is used to set weekly alarms for prayer times.
        /// It first checks if any reminders are enabled. If they are, it tries to read the prayer times from a file.
        /// If the file does not exist or the prayer times in the file are outdated, it fetches the current location and gets the prayer times for that location.
        /// If the location cannot be fetched or there is no internet connection, it shows an alert to the user.
        /// If the prayer times are successfully fetched, it sets alarms for each prayer time for the next 15 days.
        /// If the prayer times cannot be fetched, it shows a toast to the user.
        /// Finally, it sets the date for the next alarm update to 7 days in the future.
        /// </summary>
        //public async void SetWeeklyAlarms()
        //{
        //    Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //    DependencyService.Get<IAlarmService>().CancelAlarm();
        //    //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
        //    //DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), 0, "Sabah Sonu");
        //    if (CheckRemindersEnabledAny())
        //    {
        //        try
        //        {
        //            if (File.Exists(_fileName))
        //            {
        //                string json = File.ReadAllText(_fileName);
        //                var takvims = JsonConvert.DeserializeObject<List<Takvim>>(json, new TakvimConverter());
        //                if (takvims != null && (DateTime.Parse(takvims[takvims.Count-1].Tarih) - DateTime.Today).Days > 3)
        //                {
        //                    _monthlyTakvim = takvims;
        //                }
        //                else
        //                {
        //                    var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
        //                    if (location != null && location.Latitude != 0 && location.Longitude != 0)
        //                    {
        //                        _monthlyTakvim = GetMonthlyPrayerTimes(location, false);
        //                        if (_monthlyTakvim == null)
        //                        {
        //                            await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
        //                                AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
        //                            return;
        //                        }
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
        //                if (location != null && location.Latitude != 0 && location.Longitude != 0)
        //                {
        //                    _monthlyTakvim = GetMonthlyPrayerTimes(location, false);
        //                    if (_monthlyTakvim == null)
        //                    {
        //                        await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
        //                            AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
        //                        return;
        //                    }
        //                }
        //            }

        //            if (_monthlyTakvim!=null)
        //            {
        //                int dayCounter = 0;
        //                foreach (Takvim todayTakvim in _monthlyTakvim)
        //                {
        //                    if(DateTime.Parse(todayTakvim.Tarih)>=DateTime.Today){
        //                        var fecriKazipZaman = TimeSpan.Parse(todayTakvim.FecriKazip);
        //                        var fecriSadikZaman = TimeSpan.Parse(todayTakvim.FecriSadik);
        //                        var sabahSonuZaman = TimeSpan.Parse(todayTakvim.SabahSonu);
        //                        var ogleZaman = TimeSpan.Parse(todayTakvim.Ogle);
        //                        var ikindiZaman = TimeSpan.Parse(todayTakvim.Ikindi);
        //                        var aksamZaman = TimeSpan.Parse(todayTakvim.Aksam);
        //                        var yatsiZaman = TimeSpan.Parse(todayTakvim.Yatsi);
        //                        var yatsiSonuZaman = TimeSpan.Parse(todayTakvim.YatsiSonu);
        //                        var fecriKazip = DateTime.Parse(todayTakvim.Tarih) + fecriKazipZaman - TimeSpan.FromMinutes(Preferences.Get("fecrikazipBildirmeVakti", 0));
        //                        var fecriSadik = DateTime.Parse(todayTakvim.Tarih) + fecriSadikZaman - TimeSpan.FromMinutes(Preferences.Get("fecrisadikBildirmeVakti", 0));
        //                        var sabahSonu = DateTime.Parse(todayTakvim.Tarih) + sabahSonuZaman - TimeSpan.FromMinutes(Preferences.Get("sabahsonuBildirmeVakti", 0));
        //                        var ogle = DateTime.Parse(todayTakvim.Tarih) + ogleZaman - TimeSpan.FromMinutes(Preferences.Get("ogleBildirmeVakti", 0));
        //                        var ikindi = DateTime.Parse(todayTakvim.Tarih) + ikindiZaman - TimeSpan.FromMinutes(Preferences.Get("ikindiBildirmeVakti", 0));
        //                        var aksam = DateTime.Parse(todayTakvim.Tarih) + aksamZaman - TimeSpan.FromMinutes(Preferences.Get("aksamBildirmeVakti", 0));
        //                        var yatsi = DateTime.Parse(todayTakvim.Tarih) + yatsiZaman - TimeSpan.FromMinutes(Preferences.Get("yatsiBildirmeVakti", 0));
        //                        var yatsiSonu = DateTime.Parse(todayTakvim.Tarih) + yatsiSonuZaman - TimeSpan.FromMinutes(Preferences.Get("yatsisonuBildirmeVakti", 0));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-fecrikazip " + fecriKazip.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < fecriKazip));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + fecriSadik.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < fecriSadik));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + sabahSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < sabahSonu));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-ogle " + ogle.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < ogle));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + ikindi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < ikindi));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-aksam " + aksam.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < aksam));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + yatsi.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < yatsi));
        //                        Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + yatsiSonu.ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < yatsiSonu));
        //                        if (DateTime.Now < fecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), fecriKazipZaman, Preferences.Get("fecrikazipBildirmeVakti", 0), "Fecri Kazip");
        //                        if (DateTime.Now < fecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), fecriSadikZaman, Preferences.Get("fecrisadikBildirmeVakti", 0), "Fecri Sadık");
        //                        if (DateTime.Now < sabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), sabahSonuZaman, Preferences.Get("sabahsonuBildirmeVakti", 0), "Sabah Sonu");
        //                        if (DateTime.Now < ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), ogleZaman, Preferences.Get("ogleBildirmeVakti", 0), "Öğle");
        //                        if (DateTime.Now < ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), ikindiZaman, Preferences.Get("ikindiBildirmeVakti", 0), "İkindi");
        //                        if (DateTime.Now < aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), aksamZaman, Preferences.Get("aksamBildirmeVakti", 0), "Akşam");
        //                        if (DateTime.Now < yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), yatsiZaman, Preferences.Get("yatsiBildirmeVakti", 0), "Yatsı");
        //                        if (DateTime.Now < yatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), yatsiSonuZaman, Preferences.Get("yatsisonuBildirmeVakti", 0), "Yatsı Sonu");
        //                        dayCounter++;
        //                        if (dayCounter >= 15) break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                UserDialogs.Instance.Toast(AppResources.AylikTakvimeErisemedi);
        //            }
        //        }
        //        catch (Exception exception)
        //        {
        //            Debug.WriteLine($"**** {this.GetType().Name}.{nameof(GetCurrentLocationAsync)}: {exception.Message}");
        //        }

        //        Preferences.Set("LastAlarmDate", DateTime.Today.AddDays(7).ToShortDateString());
        //    }
        //    //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
        //    Debug.WriteLine("TimeStamp-SetWeeklyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //}


        //public XDocument ReadTakvimFile()
        //{
        //    //using (Stream stream = this.GetType().Assembly.
        //    //    GetManifestResourceStream("ayliktakvim.xml"))
        //    //{
        //    //    using (StreamReader sr = new StreamReader(stream))
        //    //    {
        //    //        result = sr.ReadToEnd();
        //    //    }
        //    //}
        //    var result = File.ReadAllText(FileName);
        //    var doc = XDocument.Parse(result);
        //    return doc;
        //}

        //public void CheckReminders()
        //{
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipBildiri", false)) SetNotification("fecrikazip", takvim.FecriKazip);
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikBildiri", false)) SetNotification("fecrisadik", takvim.FecriSadik);
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuBildiri", false)) SetNotification("sabahsonu", takvim.SabahSonu);
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleBildiri", false)) SetNotification("ogle", takvim.Ogle);
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiBildiri", false)) SetNotification("ikindi", takvim.Ikindi);
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamBildiri", false)) SetNotification("aksam", takvim.Aksam);
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiBildiri", false)) SetNotification("yatsi", takvim.Yatsi);
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuBildiri", false)) SetNotification("yatsisonu", takvim.YatsiSonu);
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipTitreme", false)) CheckVibration(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"));
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikTitreme", false)) CheckVibration(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"));
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuTitreme", false)) CheckVibration(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"));
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleTitreme", false)) CheckVibration(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"));
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiTitreme", false)) CheckVibration(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"));
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamTitreme", false)) CheckVibration(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"));
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiTitreme", false)) CheckVibration(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"));
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuTitreme", false)) CheckVibration(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"));
        //    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipAlarm", false)) CheckAlarm(takvim.FecriKazip, Preferences.Get("fecrikazipBildirmeVakti", "0"), "fecrikazip");
        //    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikAlarm", false)) CheckAlarm(takvim.FecriSadik, Preferences.Get("fecrisadikBildirmeVakti", "0"), "fecrisadik");
        //    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuAlarm", false)) CheckAlarm(takvim.SabahSonu, Preferences.Get("sabahsonuBildirmeVakti", "0"), "sabahsonu");
        //    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleAlarm", false)) CheckAlarm(takvim.Ogle, Preferences.Get("ogleBildirmeVakti", "0"), "ogle");
        //    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiAlarm", false)) CheckAlarm(takvim.Ikindi, Preferences.Get("ikindiBildirmeVakti", "0"), "ikindi");
        //    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamAlarm", false)) CheckAlarm(takvim.Aksam, Preferences.Get("aksamBildirmeVakti", "0"), "aksam");
        //    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiAlarm", false)) CheckAlarm(takvim.Yatsi, Preferences.Get("yatsiBildirmeVakti", "0"), "yatsi");
        //    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuAlarm", false)) CheckAlarm(takvim.YatsiSonu, Preferences.Get("yatsisonuBildirmeVakti", "0"), "yatsisonu");
        //}
        //private async void CheckAlarm(string vakit, string dakikaFarki, string adi)
        //{
        //    var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
        //    kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
        //    if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
        //    {
        //        var alarmSesi = Preferences.Get(adi + "AlarmSesi", "kus");
        //        var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(alarmSesi+".wav").ConfigureAwait(false);
        //        CrossMediaManager.Current.Notification.ShowNavigationControls = false;
        //        CrossMediaManager.Current.Notification.ShowPlayPauseControls = true;
        //        CrossMediaManager.Current.MediaPlayer.ShowPlaybackControls = true;
        //        CrossMediaManager.Current.RepeatMode = RepeatMode.All;
        //        switch (adi)
        //        {
        //            case "fecrikazip":
        //                mediaItem.DisplayTitle = "Fecri Kazip Alarmı";
        //                break;
        //            case "fecrisadik":
        //                mediaItem.DisplayTitle = "Fecri Sadık Alarmı";
        //                break;
        //            case "sabahsonu":
        //                mediaItem.DisplayTitle = "Sabah Sonu Alarmı";
        //                break;
        //            case "ogle":
        //                mediaItem.DisplayTitle = "Öğle Alarmı";
        //                break;
        //            case "ikindi":
        //                mediaItem.DisplayTitle = "İkindi Alarmı";
        //                break;
        //            case "aksam":
        //                mediaItem.DisplayTitle = "Akşam Alarmı";
        //                break;
        //            case "yatsi":
        //                mediaItem.DisplayTitle = "Yatsı Alarmı";
        //                break;
        //            case "yatsisonu":
        //                mediaItem.DisplayTitle = "Yatsı Sonu Alarmı";
        //                break;
        //        }
        //        //ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
        //        //player.Load(GetStreamFromFile("ezan.mp3"));
        //        //player.Play();
        //    }
        //}
        //Stream GetStreamFromFile(string filename)
        //{
        //    var assembly = typeof(App).GetTypeInfo().Assembly;
        //    var stream = assembly.GetManifestResourceStream("Assets." + filename);
        //    return stream;
        //}

        //private void CheckVibration(string vakit, string dakikaFarki)
        //{
        //    var kalan = DateTime.Now - DateTime.Parse(TimeSpan.Parse(vakit).ToString());
        //    kalan = kalan + TimeSpan.FromMinutes(Convert.ToInt32(dakikaFarki));
        //    if (kalan.Hours == 0 && kalan.Minutes == 0 && kalan.Seconds <= 30)
        //    {
        //        try
        //        {
        //            // Use default vibration length
        //            Vibration.Vibrate();

        //            // Or use specified time
        //            var duration = TimeSpan.FromSeconds(10);
        //            Vibration.Vibrate(duration);
        //        }
        //        catch (FeatureNotSupportedException ex)
        //        {
        //            UserDialogs.Instance.Alert(AppResources.TitremeyiDesteklemiyor + ex.Message, AppResources.CihazDesteklemiyor);
        //        }
        //        catch (Exception ex)
        //        {
        //            UserDialogs.Instance.Alert(ex.Message, AppResources.SorunCikti);
        //        }
        //    }
        //}

        //private void SetNotification(string adi, string vakit)
        //{
        //    var itemAdi = "";
        //    var notificationId = 101;
        //    var bildiriVakti = TimeSpan.Parse(vakit) - TimeSpan.FromMinutes(Convert.ToDouble(Preferences.Get(adi + "BildirmeVakti", 0.0)));
        //    var tamVakit = DateTime.Parse(bildiriVakti.ToString());
        //    if (tamVakit > DateTime.Now)
        //    {
        //        switch (adi)
        //        {
        //            case "fecrikazip":
        //                itemAdi = "Fecri Kazip";
        //                notificationId = 1003;
        //                break;
        //            case "fecrisadik":
        //                itemAdi = "Fecri Sadık";
        //                notificationId = 1004;
        //                break;
        //            case "sabahsonu":
        //                itemAdi = "Sabah Sonu";
        //                notificationId = 1005;
        //                break;
        //            case "ogle":
        //                itemAdi = "Öğle";
        //                notificationId = 1006;
        //                break;
        //            case "ikindi":
        //                itemAdi = "İkindi";
        //                notificationId = 1007;
        //                break;
        //            case "aksam":
        //                itemAdi = "Akşam";
        //                notificationId = 1008;
        //                break;
        //            case "yatsi":
        //                itemAdi = "Yatsı";
        //                notificationId = 1009;
        //                break;
        //            case "yatsisonu":
        //                itemAdi = "Yatsı Sonu";
        //                notificationId = 1010;
        //                break;
        //        }

        //        //var notification = new NotificationRequest
        //        //{
        //        //    NotificationId = 100,
        //        //    Title = itemAdi + " Vakti Bildirimi",
        //        //    Description = itemAdi + " Vakti: " + Vakit,
        //        //    ReturningData = itemAdi + " Bildirimi", // Returning data when tapped on notification.
        //        //    NotifyTime = DateTime.Parse(bildiriVakti.ToString())//DateTime.Now.AddSeconds(10) // Used for Scheduling local notification, if not specified notification will show immediately.
        //        //};
        //        //NotificationCenter.Current.Show(notification);
        //        //var notification = new Notification
        //        //{
        //        //    Id = new Random().Next(101,999),
        //        //    Title = $"{itemAdi} Vakti Bildirimi",
        //        //    Message = $"{itemAdi} Vakti: {bildiriVakti}",
        //        //    ScheduleDate = DateTime.Parse(bildiriVakti.ToString()),
        //        //};
        //        //ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
        //        CrossLocalNotifications.Current.Show($"{itemAdi} {AppResources.VaktiHatirlatmasi}",
        //            $"{itemAdi} {AppResources.Vakti} {bildiriVakti}",
        //            notificationId, tamVakit);
        //    }
        //}

        //public void SetAlarms()
        //{
        //    Analytics.TrackEvent("SetAlarms in the DataService");
        //    Log.Warning("TimeStamp-SetAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //    DependencyService.Get<IAlarmService>().CancelAlarm();
        //    if (CheckRemindersEnabledAny())
        //    {
        //        //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
        //        //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");
        //        Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + DateTime.Parse(takvim.FecriKazip).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriKazip)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + DateTime.Parse(takvim.FecriSadik).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.FecriSadik)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + DateTime.Parse(takvim.SabahSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.SabahSonu)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-ogle " + DateTime.Parse(takvim.Ogle).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ogle)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + DateTime.Parse(takvim.Ikindi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Ikindi)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-aksam " + DateTime.Parse(takvim.Aksam).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Aksam)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + DateTime.Parse(takvim.Yatsi).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.Yatsi)));
        //        Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + DateTime.Parse(takvim.YatsiSonu).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < DateTime.Parse(takvim.YatsiSonu)));
        //        if (DateTime.Now < DateTime.Parse(takvim.FecriKazip) && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriKazip), 0, "Fecri Kazip");
        //        if (DateTime.Now < DateTime.Parse(takvim.FecriSadik) && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.FecriSadik), 0, "Fecri Sadık");
        //        if (DateTime.Now < DateTime.Parse(takvim.SabahSonu) && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.SabahSonu), 0, "Sabah Sonu");
        //        if (DateTime.Now < DateTime.Parse(takvim.Ogle) && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ogle), 0, "Öğle");
        //        if (DateTime.Now < DateTime.Parse(takvim.Ikindi) && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Ikindi), 0, "İkindi");
        //        if (DateTime.Now < DateTime.Parse(takvim.Aksam) && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Aksam), 0, "Akşam");
        //        if (DateTime.Now < DateTime.Parse(takvim.Yatsi) && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.Yatsi), 0, "Yatsı");
        //        if (DateTime.Now < DateTime.Parse(takvim.YatsiSonu) && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(takvim.YatsiSonu), 0, "Yatsı Sonu");
        //    }
        //    //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
        //    Log.Warning("TimeStamp-SetAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        //}
        /*
                public async void SetMonthlyAlarms()
                {
                    Log.Warning("TimeStamp-SetMonthlyAlarms-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                    DependencyService.Get<IAlarmService>().CancelAlarm();
                    //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                    //DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Today, TimeSpan.Parse(testTimeSpan), "test");
                    if (CheckRemindersEnabledAny())
                    {
                        //konum = GetCurrentLocation().Result;
                        var location = await GetCurrentLocationAsync(false).ConfigureAwait(false);
                        if (location != null && location.Latitude != 0 && location.Longitude != 0)
                        {
                            MonthlyTakvim = GetMonthlyPrayerTimes(location, false);
                            if (MonthlyTakvim == null)
                            {
                                await UserDialogs.Instance.AlertAsync(AppResources.TakvimIcinInternet,
                                    AppResources.TakvimIcinInternetBaslik).ConfigureAwait(true);
                                return;
                            }
                            foreach (Takvim todayTakvim in MonthlyTakvim)
                            {
                                var fecriKazip = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriKazip);
                                var fecriSadik = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik);
                                var sabahSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu);
                                var ogle = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle);
                                var ikindi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi);
                                var aksam = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam);
                                var yatsi = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi);
                                var yatsiSonu = DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu);
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrikazipb " + (DateTime.Parse(todayTakvim.Tarih)+TimeSpan.Parse(todayTakvim.FecriKazip)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrikazipEtkin", false) + " --->>> " + (DateTime.Now < fecriKazip));
                                Debug.WriteLine("TimeStamp-SetAlarms-fecrisadik " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.FecriSadik)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("fecrisadikEtkin", false) + " --->>> " + (DateTime.Now < fecriSadik));
                                Debug.WriteLine("TimeStamp-SetAlarms-sabahsonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.SabahSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("sabahsonuEtkin", false) + " --->>> " + (DateTime.Now < sabahSonu));
                                Debug.WriteLine("TimeStamp-SetAlarms-ogle " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ogle)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ogleEtkin", false) + " --->>> " + (DateTime.Now < ogle));
                                Debug.WriteLine("TimeStamp-SetAlarms-ikindi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Ikindi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("ikindiEtkin", false) + " --->>> " + (DateTime.Now < ikindi));
                                Debug.WriteLine("TimeStamp-SetAlarms-aksam " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Aksam)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("aksamEtkin", false) + " --->>> " + (DateTime.Now < aksam));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsi " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.Yatsi)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsiEtkin", false) + " --->>> " + (DateTime.Now < yatsi));
                                Debug.WriteLine("TimeStamp-SetAlarms-yatsisonu " + (DateTime.Parse(todayTakvim.Tarih) + TimeSpan.Parse(todayTakvim.YatsiSonu)).ToString("MM/dd/yyyy hh:mm:ss.fff tt") + " --->>>> " + Preferences.Get("yatsisonuEtkin", false) + " --->>> " + (DateTime.Now < yatsiSonu));
                                if (DateTime.Now < fecriKazip && Preferences.Get("fecrikazipEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriKazip), 0, "Fecri Kazip");
                                if (DateTime.Now < fecriSadik && Preferences.Get("fecrisadikEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.FecriSadik), 0, "Fecri Sadık");
                                if (DateTime.Now < sabahSonu && Preferences.Get("sabahsonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.SabahSonu), 0, "Sabah Sonu");
                                if (DateTime.Now < ogle && Preferences.Get("ogleEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ogle), 0, "Öğle");
                                if (DateTime.Now < ikindi && Preferences.Get("ikindiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Ikindi), 0, "İkindi");
                                if (DateTime.Now < aksam && Preferences.Get("aksamEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Aksam), 0, "Akşam");
                                if (DateTime.Now < yatsi && Preferences.Get("yatsiEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.Yatsi), 0, "Yatsı");
                                if (DateTime.Now < yatsiSonu && Preferences.Get("yatsisonuEtkin", false)) DependencyService.Get<IAlarmService>().SetAlarm(DateTime.Parse(todayTakvim.Tarih), TimeSpan.Parse(todayTakvim.YatsiSonu), 0, "Yatsı Sonu");
                            }
                        }
                        else
                        {
                            Log.Warning("Get monthly prayer times failed in the SetMonthlyAlarm method",DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                            UserDialogs.Instance.Alert("Uygulamaya konuma erişme izni verildiğini ve konum hizmetinin açık olduğunu kontrol edin!",
                                "Konuma Erişmeye Çalışırken Hata Oluştu");
                        }
                        //var testTimeSpan = DateTime.Now.AddMinutes(1).ToString("HH:mm");
                        //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(testTimeSpan), "test");

                    }
                    //DependencyService.Get<IAlarmService>().SetAlarm(TimeSpan.Parse(DateTime.Now.AddMinutes(2).ToShortTimeString()), "test");
                    Log.Warning("TimeStamp-SetMonthlyAlarms-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
                }
        */
    }
}