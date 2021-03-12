using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using Plugin.LocalNotification;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            DependencyService.Register<TakvimData>();
            // Local Notification tap event listener
            NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            VersionTracking.Track();
        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }

        private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        {
            // your code goes here
        }
    }
}
