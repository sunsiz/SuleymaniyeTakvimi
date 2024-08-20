using System;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Models
{
    public class Takvim
    {
        public double Enlem { get; set; }

        public double Boylam { get; set; }

        public double Yukseklik { get; set; }

        public double SaatBolgesi { get; set; }

        public double YazKis { get; set; }

        public string FecriKazip { get; set; }

        public string FecriSadik { get; set; }

        public string SabahSonu { get; set; }

        public string Ogle { get; set; }

        public string Ikindi { get; set; }

        public string Aksam { get; set; }

        public string Yatsi { get; set; }

        public string YatsiSonu { get; set; }
        public string Tarih { get; set; }

        /// <summary>
        /// This method initializes the _takvim object.
        /// </summary>
        /// <remarks>
        /// The new Takvim object is initialized with values from the application's preferences. If a preference does not exist, a default value is used.
        /// The properties of the Takvim object include geographical information (latitude, longitude, altitude), time zone information, daylight saving time information, and prayer times.
        /// The Tarih property is set to the current date in "yyyy-MM-dd" format if there is no value in the preferences.
        /// </remarks>
        public Takvim()
        {
            Enlem = Preferences.Get("enlem", 41.0);
            Boylam = Preferences.Get("boylam", 29.0);
            Yukseklik = Preferences.Get("yukseklik", 114.0);
            SaatBolgesi = Preferences.Get("saatbolgesi", 3.0);
            YazKis = Preferences.Get("yazkis", 0.0);
            FecriKazip = Preferences.Get("fecrikazip", "06:28");
            FecriSadik = Preferences.Get("fecrisadik", "07:16");
            SabahSonu = Preferences.Get("sabahsonu", "08:00");
            Ogle = Preferences.Get("ogle", "12:59");
            Ikindi = Preferences.Get("ikindi", "15:27");
            Aksam = Preferences.Get("aksam", "17:54");
            Yatsi = Preferences.Get("yatsi", "18:41");
            YatsiSonu = Preferences.Get("yatsisonu", "19:31");
            Tarih = Preferences.Get("tarih", DateTime.Today.ToString("yyyy-MM-dd"));
        }
        /// <summary>
        /// This method checks if the Takvim's location info is not the default value or 0.
        /// </summary>
        /// <returns></returns>
        public bool IsTakvimLocationUnValid()
        {
            return (Yukseklik == 114.0 && Enlem == 41.0 && Boylam == 29.0) || (Yukseklik == 0 && Enlem == 0 && Boylam == 0);
        }

        public string DisplayValues()
        {
            return
                $"Enlem: {Enlem}, Boylam: {Boylam}, Yukseklik: {Yukseklik}, SaatBolgesi: {SaatBolgesi}, YazKis: {YazKis},FecriKazip: {FecriKazip}, FecriSadik: {FecriSadik}, SabahSonu: {SabahSonu}, Ogle: {Ogle}, Ikindi: {Ikindi}, Aksam: {Aksam}, Yatsi: {Yatsi}, YatsiSonu: {YatsiSonu}, Tarih: {Tarih}";
        }
    }
}