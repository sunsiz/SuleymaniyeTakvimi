using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Acr.UserDialogs;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services;

public class Helper
{
    private readonly DataService _dataService;
    private readonly string _fileName = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "ayliktakvim.json");
    private static readonly HttpClient Client = new HttpClient();

    public Helper(DataService dataService)
    {
        _dataService = dataService;
    }

    internal async Task<string> GetMonthlyPrayerTimesJsonFromApiAsync(string endpoint, Dictionary<string, string> parameters)
    {
        var primaryUri = new Uri($"https://api.suleymaniyetakvimi.com/api/{endpoint}?" +
                                 string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
        Debug.WriteLine($"**** GetMonthlyPrayerTimesJsonFromApiAsync **** Primary Url: {primaryUri}");
        var primaryTask = Client.GetAsync(primaryUri);

        try
        {
            var response = await primaryTask.ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            Debug.WriteLine($"**** GetMonthlyPrayerTimesJsonFromApiAsync **** Primary Response: {response.StatusCode}");
            var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            return jsonResult;

            //var uri = new Uri($"https://api.suleymaniyetakvimi.com/api/{endpoint}?" +
            //                  string.Join("&", parameters.Select(kvp => $"{kvp.Key}={kvp.Value}")));
            //Debug.WriteLine($"**** GetJsonFromApiAsync **** Url: {uri}");
            //// Define a fallback policy
            //var fallbackPolicy = Policy<string>
            //    .Handle<Exception>()
            //    .FallbackAsync(async cancellationToken =>
            //    {
            //        Debug.WriteLine("**** Primary API failed, calling backup API ****");
            //        string backupEndpoint = string.Empty;
            //        switch (endpoint)
            //        {
            //            case "TimeCalculation/TimeCalculate":
            //                backupEndpoint = "VakitHesabi";
            //                break;
            //            case "TimeCalculation/TimeCalculateByMonth":
            //                backupEndpoint = "VakitHesabiListesi";break;
            //        }

            //        return await GetXmlFromBackupApiAsync(backupEndpoint, parameters);
            //    });

            //// Define a retry policy
            //var retryPolicy = Policy<string>
            //    .Handle<Exception>()
            //    .WaitAndRetryAsync(1, retryAttempt => TimeSpan.FromSeconds(2));

            //// Combine the retry and fallback policies
            //var policyWrap = Policy.WrapAsync(fallbackPolicy, retryPolicy);

            //// Execute the policy
            //var jsonResult = await policyWrap.ExecuteAsync(async () =>
            //{
            //    var response = await Client.GetAsync(uri).ConfigureAwait(false);
            //    response.EnsureSuccessStatusCode();
            //    return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //});
            ////var response = await Client.GetAsync(uri).ConfigureAwait(false);
            ////var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //return jsonResult;
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"**** HttpRequestException ****: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine($"**** TaskCanceledException ****: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**** General Exception ****: {ex.Message}");
            throw;
        }
    }

    private async Task<string> GetMonthlyXmlFromBackupApiAsync(string endpoint, Location location)/*Dictionary<string, string> parameters*/
    {
        try
        {
            // Extract necessary parameters for the backup API
            string enlem = location.Latitude.ToString(CultureInfo.InvariantCulture);
            string boylam = location.Longitude.ToString(CultureInfo.InvariantCulture);
            string yukseklik = location.Altitude?.ToString(CultureInfo.InvariantCulture)??"0";
            string saatbolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours.ToString();
            string yazsaati = (TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? 1 : 0).ToString();
            string tarih = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
            
            var uri = new Uri($"https://servis.suleymaniyetakvimi.com/servis.asmx/{endpoint}?" +
                              $"enlem={enlem}&boylam={boylam}&yukseklik={yukseklik}&saatbolgesi={saatbolgesi}&yazsaati={yazsaati}&tarih={tarih}");
            Debug.WriteLine($"**** GetMonthlyXmlFromBackupApiAsync **** Url: {uri}");
            var response = await Client.GetAsync(uri).ConfigureAwait(false);
            var xmlResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            //Debug.WriteLine($"**** GetXmlFromBackupApiAsync **** Xml: {xmlResult}");
            return xmlResult;
            // Convert XML to JSON or your desired format
            //if (endpoint == "VakitHesabi")
            //{
            //    var jsonResult = ConvertXmlToJson(xmlResult);
            //    Debug.WriteLine($"**** GetXmlFromBackupApiAsync VakitHesabi jsonResult ****: {jsonResult}");
            //    return jsonResult;
            //}
            //else
            //{
            //    var jsonResult = ConvertXmlListToJson(xmlResult);
            //    Debug.WriteLine($"**** GetXmlFromBackupApiAsync VakitHesabiList jsonResult ****: {jsonResult}");
            //    return jsonResult;
            //}
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**** GetMonthlyXmlFromBackupApiAsync Exception ****: {ex.Message}");
            throw;
        }
    }

    //private string ConvertXmlListToJson(string xmlList)
    //{
    //    XmlDocument doc = new XmlDocument();
    //    doc.LoadXml(xmlList);

    //    // Remove all namespaces to simplify the JSON conversion
    //    XmlNode root = doc.DocumentElement;
    //    XmlNode sanitizedRoot = RemoveAllNamespaces(root);

    //    string json = JsonConvert.SerializeXmlNode(sanitizedRoot, Newtonsoft.Json.Formatting.Indented, true);
    //    return json;
    //}

    //private static string ConvertXmlToJson(string xml)
    //{
    //    try
    //    {
    //        var doc = new XmlDocument();
    //        doc.LoadXml(xml);
    //        Debug.WriteLine($"**** ConvertXmlToJson **** Loaded XML: {doc.OuterXml}");

    //        // Remove namespaces
    //        var cleanedXml = RemoveAllNamespaces(doc.DocumentElement);
    //        Debug.WriteLine($"**** ConvertXmlToJson **** After removing namespaces: {cleanedXml.OuterXml}");

    //        string json = JsonConvert.SerializeXmlNode(cleanedXml, Newtonsoft.Json.Formatting.None, true);
    //        Debug.WriteLine($"**** ConvertXmlToJson **** JSON: {json}");

    //        // Parse the JSON to remove the XML metadata
    //        var jsonObject = JObject.Parse(json);
    //        Debug.WriteLine($"**** ConvertXmlToJson **** Parsed JSON: {jsonObject}");

    //        return jsonObject.ToString();
    //    }
    //    catch (Exception ex)
    //    {
    //        Debug.WriteLine($"**** ConvertXmlToJson Exception ****: {ex.Message}");
    //        throw;
    //    }
    //}

    //private static XmlNode RemoveAllNamespaces(XmlNode node)
    //{
    //    if (node.NodeType == XmlNodeType.Element)
    //    {
    //        var element = (XmlElement)node;
    //        var newElement = node.OwnerDocument.CreateElement(element.LocalName);

    //        foreach (XmlNode childNode in node.ChildNodes)
    //        {
    //            newElement.AppendChild(RemoveAllNamespaces(childNode));
    //        }

    //        if (element.HasAttributes)
    //        {
    //            foreach (XmlAttribute attribute in element.Attributes)
    //            {
    //                if (!attribute.Name.StartsWith("xmlns"))
    //                {
    //                    newElement.SetAttribute(attribute.Name, attribute.Value);
    //                }
    //            }
    //        }

    //        return newElement;
    //    }
    //    else if (node.NodeType == XmlNodeType.Text)
    //    {
    //        return node.OwnerDocument.CreateTextNode(node.Value);
    //    }

    //    return node;
    //}

    internal async Task<IList<Takvim>> GetMonthlyPrayerTimesFromApiAsync(Location location)
    {
        var monthlyCalendar = new List<Takvim>();

        var parametersForCurrentMonth = new Dictionary<string, string>
        {
            { "latitude", location.Latitude.ToString(CultureInfo.InvariantCulture) },
            { "longitude", location.Longitude.ToString(CultureInfo.InvariantCulture) },
            { "monthId", DateTime.Today.Month.ToString() },
            { "year", DateTime.Today.Year.ToString() }
        };

        var parametersForNextMonth = new Dictionary<string, string>
        {
            { "latitude", location.Latitude.ToString(CultureInfo.InvariantCulture) },
            { "longitude", location.Longitude.ToString(CultureInfo.InvariantCulture) },
            { "monthId", DateTime.Today.AddMonths(1).Month.ToString() },
            { "year", DateTime.Today.AddMonths(1).Year.ToString() }
        };
        var cts = new CancellationTokenSource();
        var backupCts = new CancellationTokenSource();

        var currentMonthTask = GetMonthlyPrayerTimesJsonFromApiAsync("TimeCalculation/TimeCalculateByMonth", parametersForCurrentMonth).ContinueWith(t => t, cts.Token);
        var nextMonthTask = GetMonthlyPrayerTimesJsonFromApiAsync("TimeCalculation/TimeCalculateByMonth", parametersForNextMonth).ContinueWith(t => t, cts.Token);
        var backupTask = GetMonthlyXmlFromBackupApiAsync("VakitHesabiListesi", location).ContinueWith(t => t, backupCts.Token); ;

        var completedTask = await Task.WhenAny(Task.WhenAll(currentMonthTask, nextMonthTask), backupTask);

        if (completedTask == backupTask)
        {
            cts.Cancel(); // Cancel the currentMonthTask and nextMonthTask
            var taskResult = await backupTask;
            // Process the backup result
            var xmlResult = await taskResult;
            if (!string.IsNullOrEmpty(xmlResult))
            {
                var prayerTimes = ParseTakvimListFromXml(xmlResult, location);
                monthlyCalendar.AddRange(prayerTimes);
            }
            else
            {
                UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                return null;
            }
        }
        else
        {
            backupCts.Cancel(); // Cancel the backupTask
            var currentMonthTaskResult = await currentMonthTask.ConfigureAwait(false);
            var nextMonthTaskResult = await nextMonthTask.ConfigureAwait(false);
            // Process the currentMonthJsonResult and nextMonthJsonResult
            var currentMonthJsonResult = await currentMonthTaskResult;
            var nextMonthJsonResult = await nextMonthTaskResult;
            if (!string.IsNullOrEmpty(currentMonthJsonResult) && !string.IsNullOrEmpty(nextMonthJsonResult))
            {
                var jsonContent = currentMonthJsonResult.Replace("}]", "},") + nextMonthJsonResult.Replace("[{", "{");
                var prayerTimes = JsonConvert.DeserializeObject<List<Takvim>>(jsonContent, new TakvimConverter());
                monthlyCalendar.AddRange(prayerTimes);
            }
            else if (!string.IsNullOrEmpty(currentMonthJsonResult) || !string.IsNullOrEmpty(nextMonthJsonResult))
            {
                var prayerTimes = JsonConvert.DeserializeObject<List<Takvim>>(currentMonthJsonResult ?? nextMonthJsonResult, new TakvimConverter());
                monthlyCalendar.AddRange(prayerTimes);
            }
            else
            {
                UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                return null;
            }
        }

        //await ((Task)Task.WhenAll(currentMonthTask, nextMonthTask)).ConfigureAwait(false);

        //var currentMonthJsonResult = await currentMonthTask;
        //var nextMonthJsonResult = await nextMonthTask;

        //if (!string.IsNullOrEmpty(currentMonthJsonResult) && !string.IsNullOrEmpty(nextMonthJsonResult))
        //{
        //    jsonContent = currentMonthJsonResult.Replace("}]", "},") + nextMonthJsonResult.Replace("[{", "{");
        //    var takvims = JsonConvert.DeserializeObject<List<Takvim>>(jsonContent, new TakvimConverter());
        //    monthlyCalendar.AddRange(takvims);
        //}
        //else
        //{
        //    UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
        //    return null;
        //}
        Debug.WriteLine($"**** GetMonthlyPrayerTimesFromApiAsync **** Monthly Calendar: {monthlyCalendar}");
        // Write the updated prayer times to the file.
        try
        {
            await using var fileStream = new FileStream(_fileName, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
            await using var streamWriter = new StreamWriter(fileStream);
            await streamWriter.WriteAsync(JsonConvert.SerializeObject(monthlyCalendar)).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await UserDialogs.Instance.AlertAsync(exception.Message, AppResources.NamazVaktiAlmaHatasi);
            Debug.WriteLine($"**** GetMonthlyPrayerTimesFromApiAsync **** Exception: {exception.Message}");
        }
        return monthlyCalendar;
    }

    private IEnumerable<Takvim> ParseTakvimListFromXml(string xmlResult, Location location)
{
    var monthlyTakvim = new List<Takvim>();
    var doc = XDocument.Parse(xmlResult);

    foreach (var item in doc.Root.Descendants())
    {
        if (item.Name.LocalName == "TakvimListesi")
        {
            var takvimItem = new Takvim();
            foreach (var subitem in item.Elements())
            {
                switch (subitem.Name.LocalName)
                {
                    case "Tarih":
                        takvimItem.Tarih = subitem.Value;
                        break;
                    case "Enlem":
                        takvimItem.Enlem = Convert.ToDouble(subitem.Value, CultureInfo.InvariantCulture);
                        break;
                    case "Boylam":
                        takvimItem.Boylam = Convert.ToDouble(subitem.Value, CultureInfo.InvariantCulture);
                        break;
                    case "Yukseklik":
                        takvimItem.Yukseklik = Convert.ToDouble(subitem.Value, CultureInfo.InvariantCulture);
                        break;
                    case "SaatBolgesi":
                        takvimItem.SaatBolgesi = Convert.ToDouble(subitem.Value, CultureInfo.InvariantCulture);
                        break;
                    case "YazKis":
                        takvimItem.YazKis = Convert.ToDouble(subitem.Value, CultureInfo.InvariantCulture);
                        break;
                    case "FecriKazip":
                        takvimItem.FecriKazip = subitem.Value;
                        break;
                    case "FecriSadik":
                        takvimItem.FecriSadik = subitem.Value;
                        break;
                    case "SabahSonu":
                        takvimItem.SabahSonu = subitem.Value;
                        break;
                    case "Ogle":
                        takvimItem.Ogle = subitem.Value;
                        break;
                    case "Ikindi":
                        takvimItem.Ikindi = subitem.Value;
                        break;
                    case "Aksam":
                        takvimItem.Aksam = subitem.Value;
                        break;
                    case "Yatsi":
                        takvimItem.Yatsi = subitem.Value;
                        break;
                    case "YatsiSonu":
                        takvimItem.YatsiSonu = subitem.Value;
                        break;
                }
            }

            takvimItem.Enlem = takvimItem.Enlem == 0 ? location.Latitude : takvimItem.Enlem;
            takvimItem.Boylam = takvimItem.Boylam == 0 ? location.Longitude : takvimItem.Boylam;
            takvimItem.Yukseklik = takvimItem.Yukseklik == 0 ? location.Altitude ?? 0 : takvimItem.Yukseklik;
            monthlyTakvim.Add(takvimItem);
        }
    }

    return monthlyTakvim;
}


    public static bool HaveInternet()
    {
        var current = Connectivity.NetworkAccess;
        if (current != NetworkAccess.Internet)
        {
            UserDialogs.Instance.Toast(AppResources.TakvimIcinInternet, TimeSpan.FromSeconds(7));
            return false;
        }

        return true;
    }

    public static bool IsValidLocation(Location location)
    {
        return location != null && location.Latitude >= -90 && location.Latitude <= 90 && location.Longitude >= -180 &&
               location.Longitude <= 180 && location.Latitude != 0 && location.Longitude != 0;
    }
    
    public async Task<Takvim> GetPrayerTimeOfTodayFromApiAsync(Location location)
    {
        var primaryUri = new Uri($"https://api.suleymaniyetakvimi.com/api/TimeCalculation/TimeCalculate?latitude={location.Latitude}&longitude={location.Longitude}");
        Debug.WriteLine($"**** GetPrayerTimeOfTodayFromApiAsync **** Primary Url: {primaryUri}");
        var backupUri = PrepareBackupUri(location);
        Debug.WriteLine($"**** GetPrayerTimeOfTodayFromApiAsync **** Backup Url: {backupUri}");
        
        var primaryTask = Client.GetAsync(primaryUri);
        var backupTask = Client.GetAsync(backupUri);

        var completedTask = await Task.WhenAny(primaryTask, backupTask);

        try
        {
            if (completedTask == primaryTask)
            {
                var response = await primaryTask.ConfigureAwait(false);
                response.EnsureSuccessStatusCode();
                Debug.WriteLine($"**** GetPrayerTimeOfTodayFromApiAsync **** Primary Response: {response.StatusCode}");
                var jsonResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return JsonConvert.DeserializeObject<Takvim>(jsonResult, new TakvimConverter());
            }
            else
            {
                Debug.WriteLine("**** Primary API failed, calling backup API ****");
                var response = await backupTask.ConfigureAwait(false);
                var xmlResult = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                Debug.WriteLine($"**** GetPrayerTimeOfTodayFromApiAsync **** Backup Response: {response.StatusCode}");
                if (!string.IsNullOrEmpty(xmlResult) && xmlResult.StartsWith("<?xml"))
                    return ParseTakvimFromXml(xmlResult);
                else
                {
                    UserDialogs.Instance.Toast(AppResources.NamazVaktiAlmaHatasi, TimeSpan.FromSeconds(5));
                    return null;
                }
            }
        }
        catch (HttpRequestException ex)
        {
            Debug.WriteLine($"**** HttpRequestException ****: {ex.Message}");
            throw;
        }
        catch (TaskCanceledException ex)
        {
            Debug.WriteLine($"**** TaskCanceledException ****: {ex.Message}");
            throw;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"**** General Exception ****: {ex.Message}");
            throw;
        }
    }

    private Takvim ParseTakvimFromXml(string xmlResult)
    {
        var doc = XDocument.Parse(xmlResult);
        var prayerTime = new Takvim();

        var elements = doc.Root.Elements().ToDictionary(e => e.Name.LocalName, e => e.Value);

        if (elements.TryGetValue("Enlem", out var enlem))
            prayerTime.Enlem = Convert.ToDouble(enlem, CultureInfo.InvariantCulture);
        if (elements.TryGetValue("Boylam", out var boylam))
            prayerTime.Boylam = Convert.ToDouble(boylam, CultureInfo.InvariantCulture);
        if (elements.TryGetValue("Yukseklik", out var yukseklik))
            prayerTime.Yukseklik = Convert.ToDouble(yukseklik, CultureInfo.InvariantCulture);
        if (elements.TryGetValue("SaatBolgesi", out var saatBolgesi))
            prayerTime.SaatBolgesi = Convert.ToDouble(saatBolgesi, CultureInfo.InvariantCulture);
        if (elements.TryGetValue("YazKis", out var yazKis))
            prayerTime.YazKis = Convert.ToDouble(yazKis, CultureInfo.InvariantCulture);
        if (elements.TryGetValue("FecriKazip", out var fecriKazip))
            prayerTime.FecriKazip = fecriKazip;
        if (elements.TryGetValue("FecriSadik", out var fecriSadik))
            prayerTime.FecriSadik = fecriSadik;
        if (elements.TryGetValue("SabahSonu", out var sabahSonu))
            prayerTime.SabahSonu = sabahSonu;
        if (elements.TryGetValue("Ogle", out var ogle))
            prayerTime.Ogle = ogle;
        if (elements.TryGetValue("Ikindi", out var ikindi))
            prayerTime.Ikindi = ikindi;
        if (elements.TryGetValue("Aksam", out var aksam))
            prayerTime.Aksam = aksam;
        if (elements.TryGetValue("Yatsi", out var yatsi))
            prayerTime.Yatsi = yatsi;
        if (elements.TryGetValue("YatsiSonu", out var yatsiSonu))
            prayerTime.YatsiSonu = yatsiSonu;

        return prayerTime;
    }


    private Uri PrepareBackupUri(Location location)
    {
        var enlem = location.Latitude.ToString(CultureInfo.InvariantCulture);
        var boylam = location.Longitude.ToString(CultureInfo.InvariantCulture);
        var yukseklik = (location.Altitude ?? 0).ToString(CultureInfo.InvariantCulture);
        var saatbolgesi = TimeZoneInfo.Local.BaseUtcOffset.Hours.ToString();
        var yazsaati = TimeZoneInfo.Local.IsDaylightSavingTime(DateTime.Now) ? "1" : "0";
        var tarih = DateTime.Now.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        var uriBuilder = new UriBuilder("https://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi")
        {
            Query = $"enlem={enlem}&boylam={boylam}&yukseklik={yukseklik}&saatbolgesi={saatbolgesi}&yazsaati={yazsaati}&tarih={tarih}"
        };

        return uriBuilder.Uri;
    }

}