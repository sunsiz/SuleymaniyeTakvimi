using SuleymaniyeTakvimi.Localization;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
            VersionNumber = AppInfo.Name + " v" + AppInfo.VersionString + " " + AppResources.Version;
        }
        

        private string versionNumber;

        public string VersionNumber { get => versionNumber; set => SetProperty(ref versionNumber, value); }
    }
}