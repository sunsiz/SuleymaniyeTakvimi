using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class AboutViewModel : MvvmHelpers.BaseViewModel
    {
        public Command LinkButtonClicked => new Command<string>(async (url) => await Launcher.OpenAsync(url));

        public AboutViewModel()
        {
            Title = "Süleymaniye vakfı";
        }
    }
}