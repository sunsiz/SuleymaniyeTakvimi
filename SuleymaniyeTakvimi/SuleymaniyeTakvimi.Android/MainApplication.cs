using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Shiny;
using Shiny.Notifications;

namespace SuleymaniyeTakvimi.Droid
{
    [Application]
    public class MainApplication : Application
    {
        public MainApplication(IntPtr handle, JniHandleOwnership transfer) : base(handle, transfer)
        {
        }

        public override void OnCreate()
        {
            base.OnCreate();

            AndroidOptions.DefaultSmallIconResourceName = "app_logo";
            //AndroidOptions.DefaultLargeIconResourceName = "app_logo";
            AndroidOptions.DefaultVibrate = true;
            AndroidOptions.DefaultLaunchActivityFlags = AndroidActivityFlags.FromBackground;
            AndroidOptions.DefaultNotificationImportance = AndroidNotificationImportance.High;
            AndroidOptions.DefaultChannel = "TakvimBildirisi";
            AndroidOptions.DefaultChannelId = "TakvimBildirisi";
            AndroidOptions.DefaultChannelDescription = "Süleymaniye Vakfı Takvimi Bildiri Ayarları";
            //AndroidOptions.AutoCancel = false;
            //AndroidShinyHost.Init(this, new Startup(), services => services.UseNotifications());
            AndroidShinyHost.Init(this, platformBuild: services => services.UseNotifications());
        }
    }
}