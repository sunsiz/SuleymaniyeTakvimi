using System;
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

        // Map JSON properties to Takvim object. Replace "JsonPropertyName" with actual property names from the JSON response.
        takvim.Enlem = (double)jo["latitude"];
        takvim.Boylam = (double)jo["longitude"];
        takvim.Yukseklik = (double)jo["height"];
        takvim.SaatBolgesi = (int)jo["gmt"];
        takvim.YazKis = (bool)jo["isDaylightSaving"] ? 1 : 0;
        takvim.Tarih = ((DateTime)jo["dateTime"]).ToShortDateString();
        takvim.FecriKazip = (string)jo["dawnTime"];
        takvim.FecriSadik = (string)jo["fajrBeginTime"];
        takvim.SabahSonu = (string)jo["fajrEndTime"];
        takvim.Ogle = (string)jo["duhrTime"];
        takvim.Ikindi = (string)jo["asrTime"];
        takvim.Aksam = (string)jo["magrib"];
        takvim.Yatsi = (string)jo["ishaBeginTime"];
        takvim.YatsiSonu = (string)jo["ishaEndTime"];
        // Continue for all properties...

        return takvim;
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }
}
