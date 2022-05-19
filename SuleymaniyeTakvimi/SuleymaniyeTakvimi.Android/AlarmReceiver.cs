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
using Android.Graphics;
using Android.Media;
using AndroidX.Core.App;
using Java.Lang;
using SuleymaniyeTakvimi.Localization;
using Uri = Android.Net.Uri;
using Xamarin.Essentials;
using String = Java.Lang.String;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver]
    public class AlarmReceiver : BroadcastReceiver
    {
        private NotificationManager notificationManager;
        private readonly int NOTIFICATION_ID = 2022;
        private readonly string ALARM_BIRD_CHANNEL_ID = "SuleymaniyeTakvimialarmbirdchannelId";
        private readonly string ALARM_ROOSTER_CHANNEL_ID = "SuleymaniyeTakvimialarmroosterchannelId";
        private readonly string ALARM_ADHAN_CHANNEL_ID = "SuleymaniyeTakvimialarmadhanchannelId";
        private readonly string ALARM_ALARM_CHANNEL_ID = "SuleymaniyeTakvimialarmalarmchannelId";
        private readonly string birdsChannelName = "Suleymaniye Takvimi Alarm Birds";
        private readonly string roosterChannelName = "Suleymaniye Takvimi Alarm Rooster";
        private readonly string adhanChannelName = "Suleymaniye Takvimi Alarm Adhan";
        private readonly string alarmChannelName = "Suleymaniye Takvimi Alarm Alarm";
        private readonly string birdsChannelDescription = "The Suleymaniye Takvimi birds alarm channel.";
        private readonly string roosterChannelDescription = "The Suleymaniye Takvimi rooster alarm channel.";
        private readonly string adhanChannelDescription = "The Suleymaniye Takvimi adhan alarm channel.";
        private readonly string alarmChannelDescription = "The Suleymaniye Takvimi alarm alarm channel.";
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                var name = intent?.GetStringExtra("name") ?? string.Empty;
                var time = TimeSpan.Parse(intent?.GetStringExtra("time") ?? string.Empty);
                //Toast.MakeText(context, "Received intent! " + name + ": " + time, ToastLength.Short).Show();
                notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                PendingIntent fullScreenPendingIntent = PendingIntent.GetActivity(context, 0,
                    new Intent(context, typeof(AlarmActivity)), PendingIntentFlags.UpdateCurrent);
                //var sound = GetRingtoneFileName(context, name);
                var birdsSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.kus}");
                var roosterSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.horoz}");
                var adhanSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.ezan}");
                var alarmSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm}");
                var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.Notification).Build();
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var birdsAlarmChannelNameJava = new String(birdsChannelName);
                    var roosterAlarmChannelNameJava = new String(roosterChannelName);
                    var adhanAlarmChannelNameJava = new String(adhanChannelName);
                    var alarmAlarmChannelNameJava = new String(alarmChannelName);
                    var birdsAlarmChannel = new NotificationChannel(ALARM_BIRD_CHANNEL_ID, birdsAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = birdsChannelDescription
                    };
                    var roosterAlarmChannel = new NotificationChannel(ALARM_ROOSTER_CHANNEL_ID, roosterAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = roosterChannelDescription
                    };
                    var adhanAlarmChannel = new NotificationChannel(ALARM_ADHAN_CHANNEL_ID, adhanAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = adhanChannelDescription
                    };
                    var alarmAlarmChannel = new NotificationChannel(ALARM_ALARM_CHANNEL_ID, alarmAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = alarmChannelDescription
                    };
                    birdsAlarmChannel.SetSound(birdsSound, alarmAttributes);
                    roosterAlarmChannel.SetSound(roosterSound, alarmAttributes);
                    adhanAlarmChannel.SetSound(adhanSound, alarmAttributes);
                    alarmAlarmChannel.SetSound(alarmSound, alarmAttributes);
                    notificationManager?.CreateNotificationChannel(birdsAlarmChannel);
                    notificationManager?.CreateNotificationChannel(roosterAlarmChannel);
                    notificationManager?.CreateNotificationChannel(adhanAlarmChannel);
                    notificationManager?.CreateNotificationChannel(alarmAlarmChannel);
                }

                //Uri sound = Uri.Parse("android.resource://" + context.PackageName + "/" + Resource.Raw.horoz);
                var alarmID = GetAlamID(context, name);
                NotificationCompat.Builder notificationBuilder = null;
                switch (alarmID)
                {
                    case "kus":
                        notificationBuilder = new NotificationCompat.Builder(context, ALARM_BIRD_CHANNEL_ID).SetSound(birdsSound);
                        break;
                    case "horoz":
                        notificationBuilder = new NotificationCompat.Builder(context, ALARM_ROOSTER_CHANNEL_ID).SetSound(roosterSound);
                        break;
                    case "ezan":
                        notificationBuilder = new NotificationCompat.Builder(context, ALARM_ADHAN_CHANNEL_ID).SetSound(adhanSound);
                        break;
                    case "alarm":
                        notificationBuilder = new NotificationCompat.Builder(context, ALARM_ALARM_CHANNEL_ID).SetSound(alarmSound);
                        break;
                }
                notificationBuilder.SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentTitle(GetTitle(name))
                    .SetContentText(GetContent(name, time))
                    .SetPriority(NotificationCompat.PriorityMax)
                    .SetCategory(NotificationCompat.CategoryAlarm)
                    .SetAutoCancel(true)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetLargeIcon(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.app_logo))
                    .SetDefaults(0)
                    .SetFullScreenIntent(fullScreenPendingIntent, true);
                //NotificationCompat.Builder notificationBuilder =
                //    new NotificationCompat.Builder(context, ALARM_CHANNEL_ID)
                //        .SetSmallIcon(Resource.Drawable.app_logo)
                //        .SetContentTitle(GetTitle(name))
                //        .SetContentText(GetContent(name, time))
                //        .SetPriority(NotificationCompat.PriorityMax)
                //        .SetCategory(NotificationCompat.CategoryAlarm)
                //        .SetSound(RingtoneManager.GetDefaultUri(RingtoneType.Alarm))
                //        .SetAutoCancel(true)
                //        .SetContentIntent(BuildIntentToShowMainActivity())
                //        .SetLargeIcon(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Mipmap.icon))
                //        .SetDefaults(0)
                //        // Use a full-screen intent only for the highest-priority alerts where you
                //        // have an associated activity that you would like to launch after the user
                //        // interacts with the notification. Also, if your app targets Android 10
                //        // or higher, you need to request the USE_FULL_SCREEN_INTENT permission in
                //        // order for the platform to invoke this notification.
                //        .SetFullScreenIntent(fullScreenPendingIntent, true);
                
                Notification alarmlNotification = notificationBuilder.Build();
                notificationManager.Notify(NOTIFICATION_ID,alarmlNotification);
            }
            catch (System.Exception exception)
            {
                Console.WriteLine(exception);
                throw;
            }
        }

        private string GetAlamID(Context context, string? name)
        {
            var fileName = "";
            if (name != null)
                fileName = name switch
                {
                    "Fecri Kazip" => Preferences.Get("fecrikazipAlarmSesi","alarm"),
                    "Fecri Sadık" => Preferences.Get("fecrisadikAlarmSesi","alarm"),
                    "Sabah Sonu" => Preferences.Get("sabahsonuAlarmSesi","alarm"),
                    "Öğle" => Preferences.Get("ogleAlarmSesi","alarm"),
                    "İkindi" => Preferences.Get("ikindiAlarmSesi","alarm"),
                    "Akşam" => Preferences.Get("aksamAlarmSesi","alarm"),
                    "Yatsı" => Preferences.Get("yatsiAlarmSesi","alarm"),
                    "Yatsı Sonu" => Preferences.Get("yatsisonuAlarmSesi","alarm"),
                    _ => "ezan"
                };
            return fileName;
            //return fileName switch
            //{
            //    "kus" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm3}"),
            //    "horoz" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.horoz}"),
            //    "alarm" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm1}"),
            //    "ezan" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.ezan}"),
            //    "alarm2" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm2}"),
            //    "beep1" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep1}"),
            //    "beep2" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep2}"),
            //    "beep3" => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep3}"),
            //    _ => Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm1}")
            //};
        }

        private string GetContent(string? name, TimeSpan time)
        {
            if (name != null)
                return name switch
                {
                    "Fecri Kazip" => $"{AppResources.FecriKazip} {AppResources.Vakti} {time}",
                    "Fecri Sadık" => $"{AppResources.FecriSadik} {AppResources.Vakti} {time}",
                    "Sabah Sonu" => $"{AppResources.SabahSonu} {AppResources.Vakti} {time}",
                    "Öğle" => $"{AppResources.Ogle} {AppResources.Vakti} {time}",
                    "İkindi" => $"{AppResources.Ikindi} {AppResources.Vakti} {time}",
                    "Akşam" => $"{AppResources.Aksam} {AppResources.Vakti} {time}",
                    "Yatsı" => $"{AppResources.Yatsi} {AppResources.Vakti} {time}",
                    "Yatsı Sonu" => $"{AppResources.YatsiSonu} {AppResources.Vakti} {time}",
                    _ => $"şimdiki zaman: {time}"
                };

            return "";
        }

        private string GetTitle(string? name)
        {
            if (name != null)
                return name switch
                {
                    "Fecri Kazip" => AppResources.FecriKazip + " " + AppResources.Alarmi,
                    "Fecri Sadık" => AppResources.FecriSadik + " " + AppResources.Alarmi,
                    "Sabah Sonu" => AppResources.SabahSonu + " " + AppResources.Alarmi,
                    "Öğle" => AppResources.Ogle + " " + AppResources.Alarmi,
                    "İkindi" => AppResources.Ikindi + " " + AppResources.Alarmi,
                    "Akşam" => AppResources.Aksam + " " + AppResources.Alarmi,
                    "Yatsı" => AppResources.Yatsi + " " + AppResources.Alarmi,
                    "Yatsı Sonu" => AppResources.YatsiSonu + " " + AppResources.Alarmi,
                    _ => "Test Alarmı",
                };
            return "";
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent BuildIntentToShowMainActivity()
        {
            //Intent myIntent = new Intent();
            //myIntent.SetAction(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            ////myIntent.SetData(Android.Net.Uri.FromParts("package", PackageName, null));
            //myIntent.AddFlags(ActivityFlags.NewTask);
            //StartActivity(myIntent);
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            //notificationIntent.PutExtra("has_service_been_started", true);

            var pendingIntent = PendingIntent.GetActivity(Application.Context, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }
    }
}