using System.Collections.Generic;
using System.Threading.Tasks;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    class MonthViewModel: MvvmHelpers.BaseViewModel
    {
        private IList<Takvim> _monthlyTakvim;
        public Command BackCommand { get; }
        public Command RefreshCommand { get; }

        public IList<Takvim> MonthlyTakvim
        {
            get => _monthlyTakvim;
            set => SetProperty(ref _monthlyTakvim, value);
        }
        public MonthViewModel()
        {
            IsBusy = true;
            var data = new DataService();
            var konum = data.takvim;
            var location = new Location()
                {Latitude = konum.Enlem, Longitude = konum.Boylam, Altitude = konum.Yukseklik};
            //Task.Run(async () =>
            //{
            //    await data.GetMonthlyPrayerTimes(location).ConfigureAwait(true);
            //});
            //data.GetMonthlyPrayerTimes(location);
            //MonthlyTakvim = data.MonthlyTakvim;
            MonthlyTakvim = data.GetMonthlyPrayerTimes(location, false);
            if(MonthlyTakvim==null){
                UserDialogs.Instance.Alert(AppResources.TakvimIcinInternet, AppResources.TakvimIcinInternetBaslik);
                return;
            }
            BackCommand = new Command(GoBack);
            RefreshCommand = new Command(async () =>
            {
                using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
                {
                    var data = new DataService();
                    var location = await data.GetCurrentLocationAsync(true).ConfigureAwait(false);
                    if (location != null && location.Enlem != 0)
                        MonthlyTakvim =
                            data.GetMonthlyPrayerTimes(
                                new Location(location.Enlem, location.Boylam, location.Yukseklik),
                                true);
                    if (MonthlyTakvim == null)
                        UserDialogs.Instance.Alert(AppResources.TakvimIcinInternet,
                            AppResources.TakvimIcinInternetBaslik);
                    else UserDialogs.Instance.Toast(AppResources.AylikTakvimYenilendi);
                }
            });
            IsBusy = false;
        }
        
        private void GoBack(object obj)
        {
            Shell.Current.GoToAsync("..");
        }
    }
}
