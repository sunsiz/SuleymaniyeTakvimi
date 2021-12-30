using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AboutPage : ContentPage
    {
        public AboutPage()
        {
            InitializeComponent();
            IsBusy = true;
        }

        //protected override void OnAppearing()
        //{
        //    base.OnAppearing();
        //    //ActivityIndicator.IsRunning = true;
        //    //ActivityIndicator.IsVisible = true;
        //    //IsBusy = true;
        //}

        public void WebView_OnNavigating(object sender, WebNavigatingEventArgs e)
        {
            ActivityIndicator.IsRunning = true;
            ActivityIndicator.IsVisible = true;
            IsBusy = true;
        }

        public void WebView_OnNavigated(object sender, WebNavigatedEventArgs e)
        {
            ActivityIndicator.IsRunning = false;
            ActivityIndicator.IsVisible = false;
            IsBusy = false;
        }
    }
}