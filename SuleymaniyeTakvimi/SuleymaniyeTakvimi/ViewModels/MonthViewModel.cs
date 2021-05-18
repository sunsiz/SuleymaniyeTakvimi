using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    class MonthViewModel: MvvmHelpers.BaseViewModel
    {
        public ObservableCollection<Takvim> MonthlyTakvim { get; set; }
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
            data.GetMonthlyPrayerTimes(location);
            MonthlyTakvim = data.MonthlyTakvim;
            IsBusy = false;
        }
    }
}
