using System;
using System.Globalization;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SuleymaniyeTakvimi.Models;

public class TakvimConverter : JsonConverter
{
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(Takvim));
    }

    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        JObject jo = JObject.Load(reader);
        Takvim takvim = new Takvim();

        // Check if the JSON contains the new format properties
        if (jo["latitude"] != null)
        {
            // New format
            takvim.Enlem = (double)jo["latitude"];
            takvim.Boylam = (double)jo["longitude"];
            takvim.Yukseklik = (double)jo["height"];
            takvim.SaatBolgesi = (int)jo["gmt"];
            takvim.YazKis = (bool)jo["isDaylightSaving"] ? 1 : 0;
            string dateTimeString = (string)jo["dateTime"];
            DateTime dateTime = DateTime.Parse(dateTimeString, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            takvim.Tarih = dateTime.ToShortDateString();
            takvim.FecriKazip = (string)jo["dawnTime"];
            takvim.FecriSadik = (string)jo["fajrBeginTime"];
            takvim.SabahSonu = (string)jo["fajrEndTime"];
            takvim.Ogle = (string)jo["duhrTime"];
            takvim.Ikindi = (string)jo["asrTime"];
            takvim.Aksam = (string)jo["magrib"];
            takvim.Yatsi = (string)jo["ishaBeginTime"];
            takvim.YatsiSonu = (string)jo["ishaEndTime"];
        }
        else
        {
            // Original format
            takvim.Enlem = (double)jo["Enlem"];
            takvim.Boylam = (double)jo["Boylam"];
            takvim.Yukseklik = (double)jo["Yukseklik"];
            takvim.SaatBolgesi = (double)jo["SaatBolgesi"];
            takvim.YazKis = (double)jo["YazKis"];
            takvim.FecriKazip = (string)jo["FecriKazip"];
            takvim.FecriSadik = (string)jo["FecriSadik"];
            takvim.SabahSonu = (string)jo["SabahSonu"];
            takvim.Ogle = (string)jo["Ogle"];
            takvim.Ikindi = (string)jo["Ikindi"];
            takvim.Aksam = (string)jo["Aksam"];
            takvim.Yatsi = (string)jo["Yatsi"];
            takvim.YatsiSonu = (string)jo["YatsiSonu"];
            takvim.Tarih = (string)jo["Tarih"];
        }
        return takvim;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
