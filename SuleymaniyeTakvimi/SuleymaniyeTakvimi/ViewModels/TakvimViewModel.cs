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
        //private Takvim vakitler;

        //public Takvim Vakitler
        //{
        //    get
        //    {var data = new TakvimData();
        //    vakitler = data.takvim;
        //    return vakitler;
        //    }
        //    set { SetProperty(ref vakitler, value); }
        //}
        //public Command LoadTakvimCommand { get; }
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
            //this.takvimDataStore = takvimDataStore;
            Title = "Takvim";
            //OpenWebCommand = new Command(async () => await Browser.OpenAsync("http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi"));
            //LoadTakvimCommand = new Command(() =>
            //{
            //if (Connectivity.NetworkAccess == NetworkAccess.None)
            //{
            //    //No internet
            //    UserDialogs.Instance.Alert("Cihazda internet hizmetleri etkin değil. İnternetinizin açık Olduğundan emin olun!",
            //        "İnternet Kapalı");
            //}

            //if (Application.Current.Properties.ContainsKey("takvim"))
            //{
            //    var takvim = Application.Current.Properties["takvim"] as Takvim;
            //    Vakitler = takvim;
            //    return;
            //    // do something with id
            //}

            ////This method get location and praying time data with separate thread.
            ////Works fine, but maybe not the main thread the Xamarin Essentials doesn't display message to ask location permission.
            ////TakvimData data = new TakvimData();
            ////var getLocation = Task.Run(async () => { await data.GetCurrentLocation(); });
            ////var Vakit = getLocation.ContinueWith((antecedent) =>
            ////{
            ////    //antecedent.RunSynchronously();
            ////    return data.VakitHesabi();
            ////});
            ////try
            ////{
            ////    Vakit.Wait();
            ////}
            ////finally
            ////{
            ////    Vakitler = Vakit.Result;
            ////}

            ////This method also gets location and praying time data with separate thread.
            ////Works fine, but like above method the Xamarin Essentials doesn't display message to ask location permission too.
            //Task.Run(async () =>
            //{
            //    TakvimData data = new TakvimData();
            //    await data.GetCurrentLocation();
            //    if (data.konum != null)
            //        Vakitler = data.VakitHesabi();
            //}).Wait();

            //var data = new TakvimData();
            //data.GetCurrentLocation().Wait(2000);
            //Vakitler = data.VakitHesabi();

            var data = new DataService();
            Vakitler = data.takvim;

            //Task.Run(async () =>
            //{
            //    try
            //    {
            //        var location = await Geolocation.GetLastKnownLocationAsync().ConfigureAwait(false);

            //        if (location == null)
            //        {
            //            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(5));
            //            CancellationTokenSource cts = new CancellationTokenSource();
            //            location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
            //        }

            //        return location;
            //    }
            //    catch (Exception exception)
            //    {
            //        UserDialogs.Instance.Alert(exception.Message, "Konuma erişmeye çalışırken bir sorun oluştu.");
            //        return null;
            //    }
            //}).ContinueWith(async (location) =>
            //{
            //    if (await location.ConfigureAwait(false) != null)
            //    {
            //        var takvimData = new TakvimData();
            //        Vakitler = await takvimData.GetPrayerTimes(await location.ConfigureAwait(false)).ConfigureAwait(false);
            //        if (Vakitler.Enlem != 0)
            //        {
            //            Application.Current.Properties["takvim"] = Vakitler;
            //            await Application.Current.SavePropertiesAsync().ConfigureAwait(false);
            //        }
            //    }
            //}).ConfigureAwait(false);

            //refreshing last known location to the newest one.
            //Task.Run(()=>GetPrayerTimesAsync(data));

            //});
            //public ICommand OpenWebCommand { get; }

            //RefreshLocationCommand = new Command(OnRefreshLocation);
            RefreshLocationCommand = new Command(async ()=> await GetPrayerTimesAsync().ConfigureAwait(false));
            //RefreshLocationCommand.Execute(() => GetLocation());
            IsBusy = false;
        }
            async Task GetPrayerTimesAsync()/*TakvimData data*/
            {
                var data = new DataService();
                try
                {
                    var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                    CancellationTokenSource cts = new CancellationTokenSource();
                    var location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
                    if (location != null)
                    {
                        data.konum=new Takvim();
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
                        UserDialogs.Instance.Toast("Konum alma başarısız, Lütfen konum hizmetlerinin açık olduğunu kontrol edin!",TimeSpan.FromSeconds(7));
                    }
                }
                catch (Exception exception)
                {
                    UserDialogs.Instance.Alert(exception.Message, "Konuma erişmeye çalışırken bir sorun oluştu.");
                }
            }

        private async void OnRefreshLocation(object obj)
        {
            //refreshing last known location to the newest one.
            var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
            CancellationTokenSource cts = new CancellationTokenSource();
            var location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
            var data = new DataService();
            if (location == null)
                await data.GetCurrentLocation();
            else
            {
                data.konum=new Takvim();
                data.konum.Enlem = location.Latitude;
                data.konum.Boylam = location.Longitude;
                data.konum.Yukseklik = location.Altitude ?? 0;
            }
            Vakitler = data.VakitHesabi();
            //Preferences.Set("takvim",Vakitler);
            Application.Current.Properties["takvim"] = Vakitler;
            await Application.Current.SavePropertiesAsync();
        }

        private void GetLocation()
        {
            IsBusy = true;
            var data = new DataService();
            Vakitler = data.takvim;

            //refreshing last known location to the newest one.
            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            CancellationTokenSource cts = new CancellationTokenSource();
            Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
            IsBusy = false;
        }

        private async Task ExecuteLoadTakvimCommand()
        {
            IsBusy = true;

            try
            {
                var data = new DataService();
                //Vakitler = data.takvim.Result;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }

        public void OnAppearing()
        {
            IsBusy = true;
        }
    }
}