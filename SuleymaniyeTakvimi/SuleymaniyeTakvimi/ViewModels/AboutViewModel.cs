using SuleymaniyeTakvimi.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class AboutViewModel : MvvmHelpers.BaseViewModel
    {
        public Command LinkButtonClicked => new Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));

        public AboutViewModel()
        {
            Title = AppResources.SuleymaniyeVakfi;
        }
    }
}