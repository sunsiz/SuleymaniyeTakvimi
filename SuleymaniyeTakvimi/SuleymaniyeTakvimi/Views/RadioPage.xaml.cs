using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		private void webviewNavigated(object sender, WebNavigatedEventArgs e)
		{
			//labelLoading.IsVisible = false;
			Title = "Radyo Fıtrat";
		}

		private void webviewNavigating(object sender, WebNavigatingEventArgs e)
		{
			//labelLoading.IsVisible = true;
			Title = "İçerik yükleniyor ...";
		}
	}
}