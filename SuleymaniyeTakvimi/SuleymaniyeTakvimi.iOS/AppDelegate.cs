using System;
using Foundation;
using Matcha.BackgroundService.iOS;
using MediaManager;
using UIKit;
using UserNotifications;

namespace SuleymaniyeTakvimi.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            BackgroundAggregator.Init(this);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
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

            return base.FinishedLaunching(app, options);
        }

        //public override void WillEnterForeground(UIApplication uiApplication)
        //{
        //    Plugin.LocalNotification.NotificationCenter.ResetApplicationIconBadgeNumber(uiApplication);
        //}

        // and add this guy - if you don't use jobs, you won't need it
        //public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
        //    => JobManager.OnBackgroundFetch(completionHandler);
    }
}
