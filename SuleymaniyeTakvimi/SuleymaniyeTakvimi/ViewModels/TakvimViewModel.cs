using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class TakvimViewModel : BaseViewModel
    {
        public Command RefreshLocationCommand { get; }

        Takvim vakitler;

        public Takvim Vakitler
        {
            get
            {
                if (vakitler == null)
                {
                    var data = new DataService();
                    vakitler = data.takvim;
                    //data.CheckNotification();
                }

                return vakitler;
            }
            set { SetProperty(ref vakitler, value); }
        }

        public TakvimViewModel()
        {
            IsBusy = true;
            var data = new DataService();
            Vakitler = data.takvim;
            RefreshLocationCommand = new Command(async () => await GetPrayerTimesAsync().ConfigureAwait(false));
            IsBusy = false;
        }

        async Task GetPrayerTimesAsync()
        {
            var data = new DataService();
            try
            {
                var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                CancellationTokenSource cts = new CancellationTokenSource();
                var location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
                if (location != null)
                {
                    data.konum = new Takvim();
                    data.konum.Enlem = location.Latitude;
                    data.konum.Boylam = location.Longitude;
                    data.konum.Yukseklik = location.Altitude ?? 0;
                    Vakitler = data.VakitHesabi();
                    if (Vakitler.Enlem != 0)
                    {
                        //Application.Current.Properties["takvim"] = Vakitler;
                        Preferences.Set("enlem", Vakitler.Enlem);
                        Preferences.Set("boylam", Vakitler.Boylam);
                        Preferences.Set("yukseklik", Vakitler.Yukseklik);
                        Preferences.Set("saatbolgesi", Vakitler.SaatBolgesi);
                        Preferences.Set("yazkis", Vakitler.YazKis);
                        Preferences.Set("fecrikazip", Vakitler.FecriKazip);
                        Preferences.Set("fecrisadik", Vakitler.FecriSadik);
                        Preferences.Set("sabahsonu", Vakitler.SabahSonu);
                        Preferences.Set("ogle", Vakitler.Ogle);
                        Preferences.Set("ikindi", Vakitler.Ikindi);
                        Preferences.Set("aksam", Vakitler.Aksam);
                        Preferences.Set("yatsi", Vakitler.Yatsi);
                        Preferences.Set("yatsisonu", Vakitler.YatsiSonu);
                        Preferences.Set("tarih", Vakitler.Tarih);
                        //await Application.Current.SavePropertiesAsync();
                    }
                }
                else
                {
                    UserDialogs.Instance.Toast(
                        "Konum alma başarısız, Lütfen konum hizmetlerinin açık olduğunu kontrol edin!",
                        TimeSpan.FromSeconds(7));
                }
            }
            catch (Exception exception)
            {
                UserDialogs.Instance.Alert(exception.Message, "Konuma erişmeye çalışırken bir sorun oluştu.");
            }
        }
    }
}