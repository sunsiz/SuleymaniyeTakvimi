using SuleymaniyeTakvimi.Services;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SuleymaniyeTakvimi.Models;
//using Matcha.BackgroundService;
//using Plugin.LocalNotification;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi
{
    public partial class App : Application
    {
        //private bool reminderEnabled = false;
        public App()
        {
            InitializeComponent();

            //Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            DependencyService.Register<DataService>();
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=a40bd6f0-5ad7-4b36-9a89-740333948b82;" +
                            "ios=f757b6ef-a959-4aac-9404-98dbbd2fb1bb;",
                typeof(Analytics), typeof(Crashes));
            //SetReminderEnabled();
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
