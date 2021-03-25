﻿using System;
using Foundation;
using Shiny;
using Shiny.Jobs;
using UIKit;

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
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            global::Xamarin.Forms.Forms.Init();
            //this.ShinyFinishedLaunching(new Startup(), services => services.UseNotifications());
            iOSShinyHost.Init(platformBuild: services => services.UseNotifications());/*new Startup(),*/
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
        public override void PerformFetch(UIApplication application, Action<UIBackgroundFetchResult> completionHandler)
            => JobManager.OnBackgroundFetch(completionHandler);
    }
}
