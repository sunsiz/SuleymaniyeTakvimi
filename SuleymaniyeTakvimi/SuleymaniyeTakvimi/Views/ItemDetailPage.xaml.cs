using SuleymaniyeTakvimi.ViewModels;
using System.ComponentModel;
using Android.Content;
using Android.Content.PM;
using SuleymaniyeTakvimi.Services;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = new ItemDetailViewModel();
        }

        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    DataService data = new DataService();
        //    if (data.CheckRemindersEnabledAny())
        //        DependencyService.Get<IForegroundServiceControlService>().StartService();
        //    else DependencyService.Get<IForegroundServiceControlService>().StopService();
        //}
    }
}