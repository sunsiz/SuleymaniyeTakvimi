using System.Diagnostics;
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
            //FFImageLoading.Forms.Platform.CachedImageRenderer.Init();
            //global::Xamarin.Forms.Forms.SetFlags("CollectionView_Experimental");
            Forms.Init();
            FormsMaterial.Init();
            if (UIDevice.CurrentDevice.CheckSystemVersion(10, 0))
            {
                // Ask the user for permission to get notifications on iOS 10.0+
                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert | UNAuthorizationOptions.Badge | UNAuthorizationOptions.Sound,
                    (approved, error) => { Debug.WriteLine($"approved {approved}, error:{error}");});

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
            
            DependencyService.Register<IAlarmService, AlarmService>();
            DependencyService.Register<IPermissionService, PermissionService>();
            SetAlarms();
            app.SetStatusBarStyle(UIStatusBarStyle.LightContent, true);
            return base.FinishedLaunching(app, options);
        }

        private static void SetAlarms()
        {
            DataService data = new DataService();
            data.SetWeeklyAlarms();
        }

        public override void WillTerminate(UIApplication uiApplication)
        {
            //UserDialogs.Instance.Alert("Uygulam kapanıyor.", "Uyarı", "Tamam");
            Debug.WriteLine("WillTerminate Executing.");
            SetAlarms();
            base.WillTerminate(uiApplication);
        }
        public override void DidEnterBackground(UIApplication uiApplication)
        {
            //UserDialogs.Instance.Alert("Uygulam arka plana geçiyor ve 30 saniye sonra kapanabilir.", "Uyarı", "Tamam");
            Debug.WriteLine("DidEnterBackground Executing.");
            SetAlarms();
            base.DidEnterBackground(uiApplication);
        }
    }
}
