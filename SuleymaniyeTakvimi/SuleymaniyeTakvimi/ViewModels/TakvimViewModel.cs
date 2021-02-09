using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
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

        public TakvimViewModel()
        {
            //this.takvimDataStore = takvimDataStore;
            Title = "Takvim";
            //OpenWebCommand = new Command(async () => await Browser.OpenAsync("http://servis.suleymaniyetakvimi.com/servis.asmx/VakitHesabi"));
            //LoadTakvimCommand = new Command(() =>
            //{
            if (Connectivity.NetworkAccess == NetworkAccess.None)
            {
                //No internet
            }
            //TakvimData data=new TakvimData();
            //var getLocation=Task.Run(async () => { await data.GetCurrentLocation();});
            //var Vakit = getLocation.ContinueWith((antecedent) =>
            //{
            //    //antecedent.RunSynchronously();
            //    return data.VakitHesabi();
            //});
            //try
            //{
            //    Vakit.Wait();
            //}
            //finally
            //{
            //    Vakitler = Vakit.Result;
            //}
            Task.Run(async () =>
            {
                TakvimData data = new TakvimData();
                await data.GetCurrentLocation();
                if (data.konum != null)
                    Vakitler = data.VakitHesabi();
            }).Wait();

            var request = new GeolocationRequest(GeolocationAccuracy.Medium, TimeSpan.FromSeconds(10));
            CancellationTokenSource cts = new CancellationTokenSource();
            Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
            //});
            //var data = new TakvimData();
            //Vakitler = data.takvim.Result;
            //public ICommand OpenWebCommand { get; }
        }

        private async Task ExecuteLoadTakvimCommand()
        {
            IsBusy = true;

            try
            {
                var data = new TakvimData();
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