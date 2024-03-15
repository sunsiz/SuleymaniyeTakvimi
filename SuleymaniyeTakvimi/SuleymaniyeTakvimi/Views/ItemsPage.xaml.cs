using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Views
{
    public partial class ItemsPage : ContentPage
    {
        private readonly ItemsViewModel _viewModel;

        public ItemsPage()
        {
            InitializeComponent();
            //if (VersionTracking.IsFirstLaunchEver)
            //{
            //    Navigation.PushModalAsync(new OnBoardingPage());
            //}
            //BindingContext = _viewModel = new ItemsViewModel();
            var dataService = DependencyService.Get<DataService>(); // Get DataService from DI container
            BindingContext = _viewModel = new ItemsViewModel(dataService);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            App.Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
            _viewModel.OnAppearing();
        }
    }
}