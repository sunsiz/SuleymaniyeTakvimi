using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonthPage : ContentPage
    {
        public MonthPage()
        {
            InitializeComponent();
            var dataService = DependencyService.Get<DataService>(); // Get DataService from DI container
            BindingContext = new MonthViewModel(dataService);
        }
    }
}