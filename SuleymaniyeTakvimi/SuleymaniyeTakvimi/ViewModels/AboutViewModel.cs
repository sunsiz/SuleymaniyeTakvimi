using SuleymaniyeTakvimi.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class AboutViewModel : MvvmHelpers.BaseViewModel
    {
        public Command LinkButtonClicked => new Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));
        
        private string _versionNumber;

        public string VersionNumber { get => _versionNumber; set => SetProperty(ref _versionNumber, value); }

        public AboutViewModel()
        {
            Title = AppResources.SuleymaniyeVakfi;
            VersionNumber = " v" + AppInfo.VersionString + " ";
        }
    }
}