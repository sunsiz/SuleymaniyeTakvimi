using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
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
        public IList<Takvim> MonthlyTakvim { get; set; }
        public Command BackCommand { get; }
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
            MonthlyTakvim = data.GetMonthlyPrayerTimes(location);
            if(MonthlyTakvim==null){
                UserDialogs.Instance.Alert(AppResources.TakvimIcinInternet, AppResources.TakvimIcinInternetBaslik);
                return;
            }
            IsBusy = false;
            BackCommand = new Command(GoBack);
            IsBusy = false;
        }

        private void GoBack(object obj)
        {
            Shell.Current.GoToAsync("..");
        }
    }
}
