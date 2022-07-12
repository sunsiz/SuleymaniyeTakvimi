using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Views
{
    public partial class ItemDetailPage : ContentPage
    {
        private ItemDetailViewModel viewModel;
        public ItemDetailPage()
        {
            InitializeComponent();
            BindingContext = viewModel = new ItemDetailViewModel();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.GoBack(null);
            //DataService data = new DataService();
            //if (data.CheckRemindersEnabledAny())
            //    DependencyService.Get<IForegroundServiceControlService>().StartService();
            //else DependencyService.Get<IForegroundServiceControlService>().StopService();
        }
    }
}