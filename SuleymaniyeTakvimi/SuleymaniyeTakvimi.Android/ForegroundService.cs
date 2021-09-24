using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Android.Support.V4.App;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Android.Util;
using SuleymaniyeTakvimi.Droid;

namespace SuleymaniyeTakvimi.Droid
{
    [Service]
    public class ForegroundService : Service, IForegroundServiceControlService
    {
        static readonly string TAG = typeof(ForegroundService).FullName;
        private NotificationManager notificationManager;
        private int DELAY_BETWEEN_MESSAGES = 30000;
        private int NOTIFICATION_SERVICE_ID = 1993;
        private string NOTIFICATION_CHANNEL_ID = "SuleymaniyeTakvimichannelId";
        private string channelName = "Suleymaniye Takvimi";
        readonly string channelDescription = "The Suleymaniye Takvimi notification channel.";
        Notification.Action.Builder builder;
        Notification notification;
        private bool isStarted;
        Handler handler;
        Action runnable;
        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedRemainingTime() method.
            return null;
		}

        public override void OnCreate()
        {
            base.OnCreate();
            Log.Info(TAG, "OnCreate: the service is initializing.");
            handler = new Handler();
            notificationManager = (NotificationManager)GetSystemService(NotificationService);

            // This Action is only for demonstration purposes.
            runnable = new Action(() =>
            {
                string msg = GetFormattedRemainingTime();
                Intent i = new Intent("SuleymaniyeTakvimi.Notification.Action");
                i.PutExtra("broadcast_message", msg);
                Android.Support.V4.Content.LocalBroadcastManager.GetInstance(this).SendBroadcast(i);
                handler.PostDelayed(runnable, DELAY_BETWEEN_MESSAGES);
                CreateNotification();
                notificationManager.Notify(NOTIFICATION_SERVICE_ID, notification);
            });
        }

        private void CreateNotification()/* Notification*/
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                //NotificationManager manager =
                //(NotificationManager) AndroidApp.Context.GetSystemService(AndroidApp.NotificationService);
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, channelNameJava, NotificationImportance.Max)
                {
                    Description = channelDescription,
                    LightColor = 1,
                    LockscreenVisibility = NotificationVisibility.Public
                };
                //manager.CreateNotificationChannel(channel);
                notificationManager.CreateNotificationChannel(channel);
                notification = new Notification.Builder(this, NOTIFICATION_CHANNEL_ID)
                    .SetContentTitle("Suleymaniye Vakfi Takvimi")
                    .SetContentText(GetFormattedRemainingTime())
                    .SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetOngoing(true)
                    .SetCategory(NotificationCompat.CategoryService)
                    .SetOnlyAlertOnce(true)
                    //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .Build();
            }
            else
            {
                notification = new Notification.Builder(this)
                    .SetContentTitle("Suleymaniye Vakfi Takvimi")
                    .SetContentText(GetFormattedRemainingTime())
                    .SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetOngoing(true)
                    .SetCategory(NotificationCompat.CategoryService)
                    .SetOnlyAlertOnce(true)
                    //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .Build();
                //return notification;
            }

        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent.Action.Equals("SuleymaniyeTakvimi.action.START_SERVICE"))
            {
                if (isStarted)
                {
                    Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
            //DataService data = new DataService();
            //if (data.CheckRemindersEnabledAny())
            //{
                //notificationManager.Notify(NOTIFICATION_ALARM_ID, notification);
                    Log.Info(TAG, "OnStartCommand: The service is starting.");
                    RegisterForegroundService();
                    handler.PostDelayed(runnable, DELAY_BETWEEN_MESSAGES);
                    isStarted = true;
                //data.CheckReminders();
            //}
            //else
            //{
                //notificationManager.Cancel(NOTIFICATION_SERVICE_ID);
                //StopForeground(true);
                //StopSelf();
                //isStarted = false;
            //}
                }
            }
            else if (intent.Action.Equals("SuleymaniyeTakvimi.action.STOP_SERVICE"))
            {
                Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopForeground(true);
                StopSelf(NOTIFICATION_SERVICE_ID);
                isStarted = false;

            }

            // This tells Android not to restart the service if it is killed to reclaim resources.
            return StartCommandResult.Sticky;
        }

        void RegisterForegroundService()
        {
            CreateNotification();

            // Enlist this instance of the service as a foreground service
            this.StartForeground(NOTIFICATION_SERVICE_ID, notification);
            //var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            //notificationManager.Notify(0, notification);
        }

        /// <summary>
        /// This method will return a formatted timestamp to the client.
        /// </summary>
        /// <returns>A string that details remaining time for the next prayer time.</returns>
        string GetFormattedRemainingTime()
        {
            var message = "";
            var data = new DataService();
            var takvim = data.takvim;
            var currentTime = DateTime.Now.TimeOfDay;
            if (currentTime < TimeSpan.Parse(takvim.FecriKazip))
                message = "Fecri Kazip (Sahur) için kalan vakit: " +
                          (TimeSpan.Parse(takvim.FecriKazip) - currentTime).ToString(@"hh\:mm");
            else if(currentTime >= TimeSpan.Parse(takvim.FecriKazip) && currentTime <= TimeSpan.Parse(takvim.FecriSadik))
                message = "Fecri Sadık (Sahur bitimi) için kalan vakit: " +
                          (TimeSpan.Parse(takvim.FecriSadik) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.FecriSadik) && currentTime <= TimeSpan.Parse(takvim.SabahSonu))
                message = "Sabah Sonu için kalan vakit: " +
                          (TimeSpan.Parse(takvim.SabahSonu) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.SabahSonu) && currentTime <= TimeSpan.Parse(takvim.Ogle))
                message = "Öğlenin girmesi için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Ogle) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ogle) && currentTime <= TimeSpan.Parse(takvim.Ikindi))
                message = "Öğlenin çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Ikindi) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ikindi) && currentTime <= TimeSpan.Parse(takvim.Aksam))
                message = "İkindinin çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Aksam) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Aksam) && currentTime <= TimeSpan.Parse(takvim.Yatsi))
                message = "Akşamın çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Yatsi) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Yatsi) && currentTime <= TimeSpan.Parse(takvim.YatsiSonu))
                message = "Yatsının çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.YatsiSonu) - currentTime).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.YatsiSonu))
                message = "Yatsının çıktığından beri geçen vakit: " +
                          (currentTime - TimeSpan.Parse(takvim.YatsiSonu)).ToString(@"hh\:mm");

            return message;
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction("SuleymaniyeTakvimi.action.MAIN_ACTIVITY");
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            notificationIntent.PutExtra("has_service_been_started", true);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }
        public override void OnDestroy()
        {
            // We need to shut things down.
            Log.Info(TAG, "OnDestroy: The started service is shutting down.");
            // Stop the handler.
            handler.RemoveCallbacks(runnable);
            // Remove the notification from the status bar.
            var notificationManager = (NotificationManager)GetSystemService(NotificationService);
            notificationManager.Cancel(NOTIFICATION_SERVICE_ID);
            isStarted = false;
            base.OnDestroy();
        }

        public void StartService()
        {
            var startServiceIntent = new Intent(Application.Context, typeof(ForegroundService));
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

        public void StopService()
        {
            var stopServiceIntent = new Intent(Application.Context, typeof(ForegroundService));
            stopServiceIntent.SetAction("SuleymaniyeTakvimi.action.STOP_SERVICE");
            if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
            {
                StartForegroundService(stopServiceIntent);
            }
            else
            {
                StartService(stopServiceIntent);
            }
        }
    }
}