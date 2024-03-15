using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        public SettingsPage()
        {
            InitializeComponent();
            var dataService = DependencyService.Get<DataService>(); // Get DataService from DI container
            BindingContext = new SettingsViewModel(dataService);
        }
    }
}