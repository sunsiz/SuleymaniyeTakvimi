using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Acr.UserDialogs.Infrastructure;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
//using Matcha.BackgroundService.Droid;
using MediaManager;
//using Plugin.LocalNotification;
//using Plugin.LocalNotification.Platform.Droid;
//using PeriodicBackgroundService.Android;
//using Plugin.LocalNotification;
//using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Forms;
using Xamarin.Essentials;
using Log = Android.Util.Log;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance;
        //private Intent _startServiceIntent;
        //private Intent _stopServiceIntent;
        public bool _permissionRequested;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //SetTheme(Resource.Style.MainTheme);
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity OnCreate Started: {DateTime.Now:HH:m:s.fff}");
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
            //Must create a Notification Channel when API >= 26
            // you can created multiple Notification Channels with different names.
            //NotificationCenter.CreateNotificationChannel();
            LoadApplication(new App());
            //NotificationCenter.NotifyNotificationTapped(Intent);
            //LocalNotificationsImplementation.NotificationIconId = Resource.Drawable.app_logo;
            //DependencyService.Register<IForegroundServiceControlService, ForegroundService>();
            DependencyService.Register<IAlarmService, AlarmForegroundService>();
            DependencyService.Register<IPermissionService,PermissionService>();
            //if (savedInstanceState != null)
            //{
            //    isStarted = savedInstanceState.GetBoolean("has_service_been_started", false);
            //}
            Instance = this;
            //_stopServiceIntent = new Intent(this, typeof(AlarmForegroundService));
            //_startServiceIntent = new Intent(this, typeof(AlarmForegroundService));
            //_startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
            //_stopServiceIntent.SetAction("SuleymaniyeTakvimi.action.STOP_SERVICE");
            //var status = await HandleLocationPermissionAsync().ConfigureAwait(false);

            //StartAlarmForegroundService();
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity OnCreate Finished: {DateTime.Now:HH:m:s.fff} || Permission result:");
        }

        
        public async Task<PermissionStatus> HandleLocationPermissionAsync()
        {
            //UserDialogs.Instance.Toast("Running the handle permission task", TimeSpan.FromSeconds(3));
            PermissionStatus status;/* = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>()*/;
            //UserDialogs.Instance.Alert($"Running the handle permission task and result is {status}");
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            // Code to run on the main thread
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            if (status == PermissionStatus.Granted)
                return status;
            //if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            //{
            //    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
            //}
            else if (status == PermissionStatus.Denied)
            {
                if (!_permissionRequested)
                {
                    if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                    {
                        UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                    }

                    _permissionRequested = true;
                    var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                    {
                        AndroidStyleId = 0,
                        CancelText = AppResources.Kapat,
                        Message = AppResources.KonumIzniIcerik,
                        OkText = AppResources.GotoSettings,
                        Title = AppResources.KonumIzniBaslik
                    }).ConfigureAwait(false);
                    if (result) AppInfo.ShowSettingsUI();
                    System.Diagnostics.Debug.WriteLine("Open settings dialog result:", result.ToString());
                }

            }
            else if (status == PermissionStatus.Disabled)
            {

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                {
                    AndroidStyleId = 0,
                    CancelText = AppResources.Kapat,
                    Message = AppResources.KonumIzniIcerik,
                    OkText = AppResources.GotoSettings,
                    Title = AppResources.KonumIzniBaslik
                }).ConfigureAwait(false);
                if (result) OpenDeviceLocationSettingsPage();
                System.Diagnostics.Debug.WriteLine("Permission Request result:", result.ToString());
            }
            //});
            status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            return status;
            //var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            //UserDialogs.Instance.Alert($"Running the handle permission task and result is {status}");
            //MainThread.BeginInvokeOnMainThread(async () =>
            //{
            //    // Code to run on the main thread
            //    status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            //    switch (status)
            //    {
            //        case PermissionStatus.Unknown:
            //            break;
            //        case PermissionStatus.Denied:
            //        {
            //            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            //            {
            //                UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
            //            }

            //            var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
            //            {
            //                AndroidStyleId = 0, CancelText = AppResources.Kapat, Message = AppResources.KonumIzniIcerik,
            //                OkText = AppResources.GotoSettings,
            //                Title = AppResources.KonumIzniBaslik
            //            }).ConfigureAwait(false);
            //            if (result) AppInfo.ShowSettingsUI();
            //            System.Diagnostics.Debug.WriteLine("Open settings dialog result:", result.ToString());
            //            break;
            //        }
            //        case PermissionStatus.Disabled:
            //        {
            //            if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
            //            {
            //                UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
            //            }

            //            var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
            //            {
            //                AndroidStyleId = 0,
            //                CancelText = AppResources.Kapat,
            //                Message = AppResources.KonumIzniIcerik,
            //                OkText = AppResources.GotoSettings,
            //                Title = AppResources.KonumIzniBaslik
            //            }).ConfigureAwait(false);
            //            if (result) OpenDeviceLocationSettingsPage();
            //            System.Diagnostics.Debug.WriteLine("Permission Request result:", result.ToString());
            //            break;
            //        }
            //    }
            //});
            //return status;
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

        private void OpenDeviceLocationSettingsPage()
        {
            var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }

        //private void OpenApplicationSettingsPage()
        //{
        //    var intent = new Intent(Android.Provider.Settings.ActionApplicationDetailsSettings);
        //    intent.AddFlags(ActivityFlags.NewTask);
        //    string packageName = Android.App.Application.Context.PackageName;
        //    var uri = Android.Net.Uri.FromParts("package", packageName, null);
        //    intent.SetData(uri);
        //    Android.App.Application.Context.StartActivity(intent);
        //}

        //internal void StartAlarmForegroundService()
        //{
        //    Log.Info("Main Activity", $"Main Activity SetAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
        //    //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            
        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    {
        //        StartForegroundService(_startServiceIntent);
        //    }
        //    else
        //    {
        //        StartService(_startServiceIntent);
        //    }
        //    System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity SetAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        //}

        //internal void StopAlarmForegroundService()
        //{
        //    Log.Info("Main Activity", $"Main Activity StopAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
        //    //var startServiceIntent = new Intent(this, typeof(ForegroundService));

        //    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
        //    {
        //        StartForegroundService(_stopServiceIntent);
        //    }
        //    else
        //    {
        //        StartService(_stopServiceIntent);
        //    }
        //    System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity StopAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        //}
    }
}