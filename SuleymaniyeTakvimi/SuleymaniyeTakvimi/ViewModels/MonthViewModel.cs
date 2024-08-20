using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    class MonthViewModel : BaseViewModel
    {
        private IList<Takvim> _monthlyTakvim;
        //private readonly DataService data;
        public Command BackCommand { get; }
        public Command RefreshCommand { get; }
        public Command ShareCommand { get; }

        private async Task ShareCalendar()
        {
            var takvim = DataService.Takvim;
            var url =
                $"https://www.suleymaniyetakvimi.com/home/monthlyCalendar?latitude={takvim.Enlem}&longitude={takvim.Boylam}";
            await Share.RequestAsync(new ShareTextRequest
            {
                Uri = url,
                Title = AppResources.Paylas + " - " + AppResources.AylikTakvim + " - " +
                        AppResources.SuleymaniyeVakfiTakvimi
            });
        }

        public IList<Takvim> MonthlyTakvim
        {
            get => _monthlyTakvim;
            set => SetProperty(ref _monthlyTakvim, value);
        }
        public MonthViewModel(DataService dataService) : base(dataService)
        {
            IsBusy = true;
            //data = dataService;
            //var data = new DataService();
            //var konum = DataService.Takvim;
            //var location = new Location() { Latitude = konum.Enlem, Longitude = konum.Boylam, Altitude = konum.Yukseklik };
            var location=DataService.GetSavedLocation();
            MonthlyTakvim = DataService.MonthlyTakvim;
            Debug.WriteLine("MonthlyTakvim: " + MonthlyTakvim);
            if(MonthlyTakvim == null) _ = InitializeAsync(location);
            BackCommand = new Command(GoBack);
            RefreshCommand = new Command(async () => await RefreshData(location));
            ShareCommand = new Command(async () => await ShareCalendar());
            //_ = InitializeAsync(location);
            //dataService.OnMonthlyTakvimChanged += (newMonthlyTakvim) =>
            //{
            //    MonthlyTakvim = newMonthlyTakvim;
            //};
            IsBusy = false;
        }

        private async Task InitializeAsync(Location location)
        {
            Debug.WriteLine("TimeStamp-InitializeAsync-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            Debug.WriteLine("Location: " + location);
            if (Helper.IsValidLocation(location))
            {
                MonthlyTakvim = await DataService.GetMonthlyPrayerTimesAsync(location, false).ConfigureAwait(false);
                if (MonthlyTakvim == null)
                {
                    UserDialogs.Instance.Alert(AppResources.NamazVaktiAlmaHatasi,
                        AppResources.TakvimIcinInternetBaslik);
                }
            }
            else
                UserDialogs.Instance.Toast(AppResources.KonumIzniIcerik, TimeSpan.FromSeconds(3));

            Debug.WriteLine("TimeStamp-InitializeAsync-End", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private async Task RefreshData(Location location)
        {
            using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
            {
                if (!Helper.HaveInternet()){
                    UserDialogs.Instance.Toast(AppResources.TakvimIcinInternetBaslik, TimeSpan.FromSeconds(3));
                return;
                }

                if (DependencyService.Get<IPermissionService>().IsLocationServiceEnabled() == false)
                {
                    UserDialogs.Instance.Toast(AppResources.KonumKapaliBaslik, TimeSpan.FromSeconds(3));
                    return;
                }

                if (!Helper.IsValidLocation(location))
                {
                    location = await DataService.GetCurrentLocationAsync(true);
                }

                if (Helper.IsValidLocation(location))
                {
                    MonthlyTakvim = await DataService.GetMonthlyPrayerTimesAsync(location, true);
                }

                if (MonthlyTakvim == null)
                {
                    UserDialogs.Instance.Alert(AppResources.NamazVaktiAlmaHatasi, AppResources.TakvimIcinInternetBaslik);
                }
                else
                {
                    UserDialogs.Instance.Toast(AppResources.AylikTakvimYenilendi);
                }
            }
        }

        private void GoBack(object obj)
        {
            Shell.Current.GoToAsync("..");
        }
    }
}
