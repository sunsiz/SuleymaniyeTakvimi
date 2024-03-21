using System.Globalization;
using SuleymaniyeTakvimi.Services;
//using Microsoft.AppCenter;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using Xamarin.CommunityToolkit.Helpers;
//using Matcha.BackgroundService;
//using Plugin.LocalNotification;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Collections.Generic;

namespace SuleymaniyeTakvimi
{
	public partial class App : Application
    {
        public App()
        {
            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(AppResources.ResourceManager);
            var language = Preferences.Get("SelectedLanguage", "zz");
            if(language=="zz")
            {
                language = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName switch
                {
                    "ar" => "ar",
                    "az" => "az",
                    "de" => "de",
                    "en" => "en",
                    "fa" => "fa",
                    "fr" => "fr",
                    "ru" => "ru",
                    "tr" => "tr",
                    "ug" => "ug",
                    "zh" => "zh",
                    _ => "en"
                };
            }
            LocalizationResourceManager.Current.CurrentCulture = new CultureInfo(language);
            InitializeComponent();
            Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
            
            //Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            DependencyService.Register<DataService>();
            Xamarin.Forms.Device.SetFlags(new List<string> { "Accessibility_Experimental" });
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            VersionTracking.Track();
            OnResume();
        }

        protected override void OnSleep()
        {
            SetTheme();
            RequestedThemeChanged -= App_RequestedThemeChanged;
        }

        protected override void OnResume()
        {
            SetTheme();
            RequestedThemeChanged += App_RequestedThemeChanged;
            Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
        }

        private void App_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(SetTheme);
        }

        private void SetTheme()
        {
            Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
        }
    }
}
