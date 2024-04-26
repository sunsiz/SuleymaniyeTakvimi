using System;
using System.Collections.Generic;
using System.Text;

namespace SuleymaniyeTakvimi.Models
{
    public class CalculatedTime
    {
        // CalculatedTime myDeserializedClass = JsonConvert.DeserializeObject<CalculatedTime>(myJsonResponse);

        public double latitude { get; set; }
        public double longitude { get; set; }
        public int height { get; set; }
        public string timezone { get; set; }
        public object cityName { get; set; }
        public string cityNameFromTimezone { get; set; }
        public int elevation { get; set; }
        public int gmt { get; set; }
        public int daylightSavingTime { get; set; }
        public bool isDaylightSaving { get; set; }
        public bool isDaylightSavingFromSystem { get; set; }
        public DateTime dateTime { get; set; }
        public string dawnTime { get; set; }
        public string fajrBeginTime { get; set; }
        public string fajrEndTime { get; set; }
        public string duhrTime { get; set; }
        public string asrTime { get; set; }
        public string magrib { get; set; }
        public string ishaBeginTime { get; set; }
        public string ishaEndTime { get; set; }
        public int extraTime { get; set; }

    }
}
