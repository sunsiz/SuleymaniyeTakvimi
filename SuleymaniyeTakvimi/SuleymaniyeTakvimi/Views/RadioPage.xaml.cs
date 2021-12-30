using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class RadioPage : ContentPage
	{
		public RadioPage ()
		{
			InitializeComponent ();
		}

		private void WebviewNavigated(object sender, WebNavigatedEventArgs e)
		{
			//labelLoading.IsVisible = false;
			Title = "Radyo Fıtrat";
		}

		private void WebviewNavigating(object sender, WebNavigatingEventArgs e)
		{
			//labelLoading.IsVisible = true;
			Title = "İçerik yükleniyor ...";
		}
	}
}