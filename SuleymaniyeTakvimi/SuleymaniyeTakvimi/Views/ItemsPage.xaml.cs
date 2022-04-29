using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Views
{
    public partial class ItemsPage : ContentPage
    {
        readonly ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();
            //if (VersionTracking.IsFirstLaunchEver)
            //{
            //    Navigation.PushModalAsync(new OnBoardingPage());
            //}
            BindingContext = _viewModel = new ItemsViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
            _viewModel.OnAppearing();
        }
    }
}