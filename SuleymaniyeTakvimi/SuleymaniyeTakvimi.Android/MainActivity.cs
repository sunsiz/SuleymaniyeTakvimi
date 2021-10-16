using System;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using FFImageLoading;
//using Matcha.BackgroundService.Droid;
using MediaManager;
using PeriodicBackgroundService.Android;
//using Plugin.LocalNotification;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Services;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "SuleymaniyeTakvimi", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity instance;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            //BackgroundAggregator.Init(this);
            base.OnCreate(savedInstanceState);
            //SetAlarmForBackgroundServices(this);//Use periodic background service
            UserDialogs.Init(this);
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            Xamarin.Forms.Forms.SetFlags(new string[] { "IndicatorView_Experimental" });
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            global::Xamarin.Forms.FormsMaterial.Init(this, savedInstanceState);
            CrossMediaManager.Current.Init(this);
            //this.ShinyOnCreate();
            //AndroidShinyHost.Init(this, platformBuild: services => services.UseNotifications());
            //Shiny.Notifications.AndroidOptions.DefaultSmallIconResourceName = "app_logo.png";
            // Must create a Notification Channel when API >= 26
            // you can created multiple Notification Channels with different names.
            //NotificationCenter.CreateNotificationChannel();
            LoadApplication(new App());
            //NotificationCenter.NotifyNotificationTapped(Intent);
            LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.app_logo;
            //DependencyService.Register<IForegroundServiceControlService, ForegroundService>();
            DependencyService.Register<IAlarmService,AlarmForegroundService>();
            //if (savedInstanceState != null)
            //{
            //    isStarted = savedInstanceState.GetBoolean("has_service_been_started", false);
            //}
            instance = this;
            SetForegroundService();
        }

        internal void SetForegroundService()
        {
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            var startServiceIntent = new Intent(this, typeof(AlarmForegroundService));
            startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
            
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(startServiceIntent);
            }
            else
            {
                StartService(startServiceIntent);
            }
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //AndroidShinyHost.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            //this.ShinyRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            //NotificationCenter.NotifyNotificationTapped(intent);
            base.OnNewIntent(intent);
            CrossMediaManager.Current.Stop();
            if (intent == null)
            {
                return;
            }

            //var bundle = intent.Extras;
            //if (bundle != null)
            //{
            //    if (bundle.ContainsKey("has_service_been_started"))
            //    {
            //        isStarted = true;
            //    }
            //}
        }
        //protected override void OnSaveInstanceState(Bundle outState)
        //{
        //    outState.PutBoolean("has_service_been_started", isStarted);
        //    base.OnSaveInstanceState(outState);
        //}

        //public static void SetAlarmForBackgroundServices(Context context)
        //{
        //    var alarmIntent = new Intent(context.ApplicationContext, typeof(AlarmReceiver));
        //    var broadcast = PendingIntent.GetBroadcast(context.ApplicationContext, 0, alarmIntent, PendingIntentFlags.NoCreate);
        //    if (broadcast == null)
        //    {
        //        var pendingIntent = PendingIntent.GetBroadcast(context.ApplicationContext, 0, alarmIntent, 0);
        //        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService);
        //        var triggerTime = (DateTime.Now - DateTime.Parse(TimeSpan.Parse("17:59").ToString())).Milliseconds;
        //        alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerTime, pendingIntent);
        //        //alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, SystemClock.ElapsedRealtime(), pendingIntent);
        //        if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
        //            alarmManager.SetAlarmClock(new AlarmManager.AlarmClockInfo(triggerTime, pendingIntent), pendingIntent);
        //        else if (Build.VERSION.SdkInt >= BuildVersionCodes.Lollipop)
        //            alarmManager.SetExact(AlarmType.Rtc, triggerTime, pendingIntent);
        //        else
        //            alarmManager.Set(AlarmType.Rtc, triggerTime, pendingIntent);
        //    }
        //}
    }
}