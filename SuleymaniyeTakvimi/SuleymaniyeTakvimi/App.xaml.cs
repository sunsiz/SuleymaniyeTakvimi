using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
//using Matcha.BackgroundService;
using Plugin.LocalNotifications;
//using Plugin.LocalNotification;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using Device = Xamarin.Forms.Device;

namespace SuleymaniyeTakvimi
{
    public partial class App : Application
    {
        private bool reminderEnabled = false;
        public App()
        {
            InitializeComponent();

            //Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            DependencyService.Register<DataService>();
            // Local Notification tap event listener
            //NotificationCenter.Current.NotificationTapped += OnLocalNotificationTapped;
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=a40bd6f0-5ad7-4b36-9a89-740333948b82;" +
                            "ios=f757b6ef-a959-4aac-9404-98dbbd2fb1bb;",
                typeof(Analytics), typeof(Crashes));
            SetReminderEnabled();
            VersionTracking.Track();
            if (reminderEnabled) StartBackgroundService();
        }

        public void StartBackgroundService()
        {
            //BackgroundAggregatorService.Add(() => new ReminderService(60));
            //BackgroundAggregatorService.StartBackgroundService();
            //CrossLocalNotifications.Current.Show("Suleymaniye Calendar Service Running", $"Service started at {DateTime.Now.ToShortTimeString()}", 1000);
        }

        private void SetReminderEnabled()
        {
            var fecrikazip = Preferences.Get("fecrikazipEtkin", false);
            var fecrisadik = Preferences.Get("fecrisadikEtkin", false);
            var sabahsonu = Preferences.Get("sabahsonuEtkin", false);
            var ogle = Preferences.Get("ogleEtkin", false);
            var ikindi = Preferences.Get("ikindiEtkin", false);
            var aksam = Preferences.Get("aksamEtkin", false);
            var yatsi = Preferences.Get("yatsiEtkin", false);
            var yatsisonu = Preferences.Get("yatsisonuEtkin", false);
            reminderEnabled = fecrikazip || fecrisadik || sabahsonu || ogle || ikindi || aksam || yatsi || yatsisonu;
        }

        protected override void OnSleep()
        {
            //if (!reminderEnabled) BackgroundAggregatorService.StopBackgroundService();
        }

        protected override void OnResume()
        {
            if (Device.RuntimePlatform == Device.iOS) StartBackgroundService();
        }

        //private void OnLocalNotificationTapped(NotificationTappedEventArgs e)
        //{
        //    // your code goes here
        //}
    }
}
