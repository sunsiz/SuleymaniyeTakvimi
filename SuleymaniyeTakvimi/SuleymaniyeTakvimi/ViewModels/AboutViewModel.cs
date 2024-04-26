using System.Threading.Tasks;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Views;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class AboutViewModel : MvvmHelpers.BaseViewModel
    {
        public Command LinkButtonClicked => new Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));

        public Command SettingsCommand { get; }

        private string _versionNumber;

        public string VersionNumber { get => _versionNumber; set => SetProperty(ref _versionNumber, value); }

        public bool ShowButtons => Device.RuntimePlatform == Device.iOS && DependencyService.Get<Services.IPermissionService>().IsVoiceOverRunning();

        public AboutViewModel()
        {
            Title = AppResources.SuleymaniyeVakfi;
            VersionNumber = $" v{AppInfo.VersionString} ";
            SettingsCommand = new Command(async () => await Settings());
        }

        private async Task Settings()
        {
            IsBusy = true;
            // This will push the SettingsPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(SettingsPage)}").ConfigureAwait(false);
            IsBusy = false;
        }
        //public void OnAppearing()
        //{
        //    var check = ShowButtons;
        //}
    }
}