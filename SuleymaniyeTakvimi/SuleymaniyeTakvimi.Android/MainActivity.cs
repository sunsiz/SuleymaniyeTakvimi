using System;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using MediaManager;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = false, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize, LaunchMode = LaunchMode.SingleTop)]
    public class MainActivity : Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        public static MainActivity Instance;
        private bool _permissionRequested;
        private bool _locationDialogShown;
        private bool _locationPermissionHandled;
        //private const string LocationPermissionHandledKey = "LocationPermissionHandled";
        //private const string PermissionRequestedKey = "PermissionRequested";
        //private const string LocationDialogShownKey = "LocationDialogShown";

        protected override void OnCreate(Bundle savedInstanceState)
        {
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity OnCreate Started: {DateTime.Now:HH:m:s.fff}");
            TabLayoutResource = Resource.Layout.Tabbar;
            ToolbarResource = Resource.Layout.Toolbar;

            base.OnCreate(savedInstanceState);
            UserDialogs.Init(this);
            Forms.SetFlags("IndicatorView_Experimental", "UseLegacyRenderers");
            Platform.Init(this, savedInstanceState);
            Forms.Init(this, savedInstanceState);
            FormsMaterial.Init(this, savedInstanceState);
            CrossMediaManager.Current.Init(this);
            LoadApplication(new App());
            DependencyService.Register<IAlarmService, AlarmForegroundService>();
            DependencyService.Register<IPermissionService, PermissionService>();
            Instance = this;
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity OnCreate Finished: {DateTime.Now:HH:m:s.fff} || Permission result:");
        }

        public void HandleNotificationPermissionAsync()
        {
            const int requestLocationId = 0;
            string[] notificationPermission =
            {
                Android.Manifest.Permission.PostNotifications
            };
            if ((int)Build.VERSION.SdkInt >= 33 && CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
            {
                RequestPermissions(notificationPermission, requestLocationId);
            }
        }

        public async Task<PermissionStatus> HandleLocationPermissionAsync()
        {
            //var locationPermissionHandled = Preferences.Get(LocationPermissionHandledKey, false);
            //var permissionRequested = Preferences.Get(PermissionRequestedKey, false);
            //var locationDialogShown = Preferences.Get(LocationDialogShownKey, false);

            //System.Diagnostics.Debug.WriteLine("Location Permission Handled: " + locationPermissionHandled);
            //System.Diagnostics.Debug.WriteLine("Permission Requested: " + permissionRequested);
            //System.Diagnostics.Debug.WriteLine("Location Dialog Shown: " + locationDialogShown);

            if (_locationPermissionHandled)
            {
                System.Diagnostics.Debug.WriteLine("Location Permission Handled flag is true");
                return PermissionStatus.Granted;
            }
            var status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
            System.Diagnostics.Debug.WriteLine($"Location Permission Requested, the result is {status}");
            if (status == PermissionStatus.Granted)
            {
                //Preferences.Set(LocationPermissionHandledKey, true);
                _locationPermissionHandled = true;
                return status;
            }

            if (status == PermissionStatus.Denied && !_permissionRequested)
            {
                System.Diagnostics.Debug.WriteLine("Permission is denied  and Requested flag is false");
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                }

                if (!_locationDialogShown)
                {
                    //Preferences.Set(PermissionRequestedKey, true);
                    //Preferences.Set(LocationDialogShownKey, true);
                    _permissionRequested = true;
                    _locationDialogShown = true;
                    var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                    {
                        AndroidStyleId = 0,
                        CancelText = AppResources.Kapat,
                        Message = AppResources.KonumIzniIcerik,
                        OkText = AppResources.GotoSettings,
                        Title = AppResources.KonumIzniBaslik
                    }).ConfigureAwait(false);
                    Device.BeginInvokeOnMainThread(() =>
                    {
                        if (result)
                        {
                            AppInfo.ShowSettingsUI();
                        }
                        else
                        {
                            // Handle the case where the user clicks the cancel button
                            System.Diagnostics.Debug.WriteLine("User clicked cancel on the permission dialog");
                        }
                    });
                    //if (result) AppInfo.ShowSettingsUI();
                    //System.Diagnostics.Debug.WriteLine("Permission Requested flag saved:", Preferences.Get(PermissionRequestedKey,false).ToString());
                    //System.Diagnostics.Debug.WriteLine("Location Dialog Shown flag saved:", Preferences.Get(LocationDialogShownKey, false).ToString());
                    System.Diagnostics.Debug.WriteLine("Open settings dialog result:", result.ToString());
                }
            }
            else if (status == PermissionStatus.Disabled && !_locationDialogShown)
            {
                //Preferences.Set(LocationDialogShownKey, true);
                _locationDialogShown = true;
                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                {
                    AndroidStyleId = 0,
                    CancelText = AppResources.Kapat,
                    Message = AppResources.KonumIzniIcerik,
                    OkText = AppResources.GotoSettings,
                    Title = AppResources.KonumIzniBaslik
                }).ConfigureAwait(false);
                Device.BeginInvokeOnMainThread(() =>
                {
                    if (result)
                    {
                        OpenDeviceLocationSettingsPage();
                    }
                    else
                    {
                        // Handle the case where the user clicks the cancel button
                        System.Diagnostics.Debug.WriteLine("User clicked cancel on the permission dialog");
                    }
                });
                //if (result) OpenDeviceLocationSettingsPage();
                //System.Diagnostics.Debug.WriteLine("Location Dialog Shown flag saved:", Preferences.Get(LocationDialogShownKey, false).ToString());
                System.Diagnostics.Debug.WriteLine("Permission Request result:", result.ToString());
            }

            return status; //await Permissions.RequestAsync<Permissions.LocationWhenInUse>();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);
            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnNewIntent(Intent intent)
        {
            base.OnNewIntent(intent);
            CrossMediaManager.Current.Stop();
        }

        private void OpenDeviceLocationSettingsPage()
        {
            var intent = new Intent(Android.Provider.Settings.ActionLocationSourceSettings);
            intent.AddFlags(ActivityFlags.NewTask);
            Android.App.Application.Context.StartActivity(intent);
        }
    }
}