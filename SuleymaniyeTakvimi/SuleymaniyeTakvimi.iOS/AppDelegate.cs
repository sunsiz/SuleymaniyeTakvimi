using System;
using Acr.UserDialogs;
using EventKit;
using Foundation;
//using Matcha.BackgroundService.iOS;
using MediaManager;
using SuleymaniyeTakvimi.Services;
using UIKit;
using UserNotifications;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //public static EKEventStore eventStore { get; } = new EKEventStore();
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            //BackgroundAggregator.Init(this);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            //global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            global::Xamarin.Forms.FormsMaterial.Init();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                    (approved, error) => { });

                // Watch for notifications while app is active
                UNUserNotificationCenter.Current.Delegate = new UserNotificationCenterDelegate();
            }
            else if (UIDevice.CurrentDevice.CheckSystemVersion(8, 0))
            {
                // Ask the user for permission to get notifications on iOS 8.0+
                var settings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert | UIUserNotificationType.Badge | UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(settings);
            }
            CrossMediaManager.Current.Init();
            //this.ShinyFinishedLaunching(new Startup(), services => services.UseNotifications());
            //iOSShinyHost.Init(platformBuild: services => services.UseNotifications());/*new Startup(),*/
            // Ask the user for permission to show notifications on iOS 10.0+ at startup.
            // If not asked at startup, user will be asked when showing the first notification.
            //Plugin.LocalNotification.NotificationCenter.AskPermission();
            LoadApplication(new App());
            //eventStore.RequestAccess(EKEntityType.Reminder, (bool granted, NSError e) =>
            //{
            //    if (!granted)
            //    {
            //        UserDialogs.Instance.Alert("User Denied Access to Calendars/Reminders" + e.ToString(), "Access Denied");
            //    }
            //});
            DependencyService.Register<IAlarmService, AlarmService>();
            SetAlarms();
            return base.FinishedLaunching(app, options);
        }

        private static void SetAlarms()
        {
            DataService data = new DataService();
            data.SetWeeklyAlarms();
        }

        public override void WillTerminate(UIApplication uiApplication)
        {
            //var result = ConfirmExitAsync().Result;
            //if (result == true)
            //{

            //UserDialogs.Instance.Alert("Uygulam kapanıyor.", "Uyarı", "Tamam");
            Console.WriteLine("WillTerminate Executing.");
            SetAlarms();
            base.WillTerminate(uiApplication);
            //}
        }
        public override void DidEnterBackground(UIApplication uiApplication)
        {
            //UserDialogs.Instance.Alert("Uygulam arka plana geçiyor ve 30 saniye sonra kapanabilir.", "Uyarı", "Tamam");
            Console.WriteLine("DidEnterBackground Executing.");
            SetAlarms();
            base.DidEnterBackground(uiApplication);
        }
        //private static async System.Threading.Tasks.Task<bool> ConfirmExitAsync()
        //{
        //    return await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig() { Title = "Uyarı!", Message = "Çıkmak istediğinizden eminmisiniz?", OkText = "Evet", CancelText = "Hayır" });
        //}

        //public override void ReceivedLocalNotification(UIApplication application, UILocalNotification notification)
        //{
        //    // show an alert
        //    UIAlertController okayAlertController = UIAlertController.Create(notification.AlertAction, notification.AlertBody, UIAlertControllerStyle.Alert);
        //    okayAlertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));

        //    UIApplication.SharedApplication.KeyWindow.RootViewController.PresentViewController(okayAlertController, true, null);

        //    // reset our badge
        //    UIApplication.SharedApplication.ApplicationIconBadgeNumber = 0;
        //}
        //public override void WillEnterForeground(UIApplication uiApplication)
        //{
        //    Plugin.LocalNotification.NotificationCenter.ResetApplicationIconBadgeNumber(uiApplication);
        //}

        // and add this guy - if you don't use jobs, you won't need it
        //public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        //    => JobManager.OnBackgroundFetch(completionHandler);
    }
}
