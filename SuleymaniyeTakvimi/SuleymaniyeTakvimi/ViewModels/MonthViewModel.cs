using System;
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
    class MonthViewModel: BaseViewModel
    {
        private IList<Takvim> _monthlyTakvim;
        //private readonly DataService data;
        public Command BackCommand { get; }
        public Command RefreshCommand { get; }
        public Command ShareCommand => new Command(async () =>
        {
            var takvim = DataService._takvim;
            var url = $"https://www.suleymaniyetakvimi.com/home/monthlyCalendar?latitude={takvim.Enlem}&longitude={takvim.Boylam}";
            await Share.RequestAsync(new ShareTextRequest
            {
                Uri = url,
                Title = AppResources.Paylas + " - " + AppResources.AylikTakvim + " - " +
                        AppResources.SuleymaniyeVakfiTakvimi
            });
        });

        public IList<Takvim> MonthlyTakvim
        {
            get => _monthlyTakvim;
            set => SetProperty(ref _monthlyTakvim, value);
        }
        public MonthViewModel(DataService dataService):base(dataService)
        {
            IsBusy = true;
            //data = dataService;
            //var data = new DataService();
            var konum = DataService._takvim;
            var location = new Location()
                { Latitude = konum.Enlem, Longitude = konum.Boylam, Altitude = konum.Yukseklik };
            BackCommand = new Command(GoBack);
            RefreshCommand = new Command(async () => await RefreshData(location));
            
            if (!konum.IsTakvimLocationUnValid())
            {
                MonthlyTakvim = DataService.GetMonthlyPrayerTimes(location, false);
                if (MonthlyTakvim == null)
                {
                    UserDialogs.Instance.Alert(AppResources.TakvimIcinInternet, AppResources.TakvimIcinInternetBaslik);
                    return;
                }
            }
            else
                UserDialogs.Instance.Toast(AppResources.KonumIzniIcerik, TimeSpan.FromSeconds(3));
            IsBusy = false;
        }

        private async Task RefreshData(Location location)
        {
            using (UserDialogs.Instance.Loading(AppResources.Yenileniyor))
            {
                location = await DataService.GetCurrentLocationAsync(true);
                if (location != null && location.Latitude != 0 && location.Longitude != 0)
                {
                    MonthlyTakvim = DataService.GetMonthlyPrayerTimes(location, true);
                    if (MonthlyTakvim == null)
                    {
                        UserDialogs.Instance.Alert(AppResources.TakvimIcinInternet, AppResources.TakvimIcinInternetBaslik);
                    }
                    else
                    {
                        UserDialogs.Instance.Toast(AppResources.AylikTakvimYenilendi);
                    }
                }
            }
        }
        
        private void GoBack(object obj)
        {
            Shell.Current.GoToAsync("..");
        }
    }
}
