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
        public TakvimPage()
        {
            InitializeComponent();
            if (VersionTracking.IsFirstLaunchEver)
            {
                Navigation.PushModalAsync(new OnBoardingPage());
            }
        }
    }
}