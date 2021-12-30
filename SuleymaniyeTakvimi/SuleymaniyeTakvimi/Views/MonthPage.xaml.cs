using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MonthPage : ContentPage
    {
        private MonthViewModel _viewModel;
        public MonthPage()
        {
            InitializeComponent();
            BindingContext = _viewModel = new MonthViewModel();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            BindingContext = _viewModel = new MonthViewModel();
            //_viewModel.OnAppearing();
        }
    }
}