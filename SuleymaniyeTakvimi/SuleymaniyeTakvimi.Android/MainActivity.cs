using System;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Util;
//using Matcha.BackgroundService.Droid;
using MediaManager;
//using PeriodicBackgroundService.Android;
//using Plugin.LocalNotification;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Services;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance;
        private Intent _startServiceIntent;
        private Intent _stopServiceIntent;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            //SetTheme(Resource.Style.MainTheme);
            Log.Info("Main Activity", $"Main Activity OnCreate Started: {DateTime.Now:HH:m:s.fff}");
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;
            
            //BackgroundAggregator.Init(this);
            base.OnCreate(savedInstanceState);
            //SetAlarmForBackgroundServices(this);//Use periodic background service
            UserDialogs.Init(this);
            //FFImageLoading.Forms.Platform.CachedImageRenderer.Init(enableFastRenderer: true);
            Forms.SetFlags(new string[] { "IndicatorView_Experimental" });
            Forms.SetFlags("UseLegacyRenderers");
            Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
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
            DependencyService.Register<IAlarmService, AlarmForegroundService>();
            //if (savedInstanceState != null)
            //{
            //    isStarted = savedInstanceState.GetBoolean("has_service_been_started", false);
            //}
            Instance = this;
            _stopServiceIntent = new Intent(this, typeof(AlarmForegroundService));
            _startServiceIntent = new Intent(this, typeof(AlarmForegroundService));
            _startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
            _stopServiceIntent.SetAction("SuleymaniyeTakvimi.action.STOP_SERVICE");
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            if (status != PermissionStatus.Granted)
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            StartAlarmForegroundService();
            Log.Info("Main Activity", $"Main Activity OnCreate Finished: {DateTime.Now:HH:m:s.fff} || Permission result:{status}");
        }

        internal void StartAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity SetAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(_startServiceIntent);
            }
            else
            {
                StartService(_startServiceIntent);
            }
            Log.Info("Main Activity", $"Main Activity SetAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }

        internal void StopAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity StopAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                StartForegroundService(_stopServiceIntent);
            }
            else
            {
                StartService(_stopServiceIntent);
            }
            Log.Info("Main Activity", $"Main Activity StopAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
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
        }
    }
}