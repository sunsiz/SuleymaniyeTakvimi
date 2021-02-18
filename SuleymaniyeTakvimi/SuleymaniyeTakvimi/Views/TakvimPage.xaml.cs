using System;
using System.ComponentModel;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    public partial class TakvimPage : ContentPage
    {
        //TakvimViewModel _viewModel;
        public TakvimPage()
        {
            InitializeComponent();
            if (VersionTracking.IsFirstLaunchEver)
            {
                Navigation.PushModalAsync(new OnBoardingPage());
            }
            //BindingContext = new TakvimViewModel();
            //BindingContext = _viewModel = new TakvimViewModel();
        }

        //protected override async void OnAppearing()
        //{
        //    Task.Factory.StartNew(async () =>
        //    {
        //        TakvimData data = new TakvimData();
        //        await data.GetCurrentLocation();
        //        TakvimViewModel vm=new TakvimViewModel();
        //        if(data.konum!=null)
        //            data.VakitHesabi();
        //        vm.Vakitler = data.takvim;
        //    }).Wait();
        //    base.OnAppearing();
        //}
        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    _viewModel.OnAppearing();
        //}
    }
}