using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        private readonly ItemDetailViewModel _viewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            var dataService = DependencyService.Get<DataService>(); // Get DataService from DI container
            BindingContext = _viewModel = new ItemDetailViewModel(dataService);
        }

        //protected override void OnDisappearing()
        //{
        //    base.OnDisappearing();
        //    _ = _viewModel.GoBack(null);
        //}
    }
}