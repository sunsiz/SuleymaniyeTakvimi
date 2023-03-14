using Android.App;
using Android.Content;
using Android.OS;
using System;
using Android.Graphics;
using Android.Media;
using AndroidX.Core.App;
using SuleymaniyeTakvimi.Localization;
using Uri = Android.Net.Uri;
using Xamarin.Essentials;
using String = Java.Lang.String;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class AlarmReceiver : BroadcastReceiver
    {
        private NotificationManager _notificationManager;
        private readonly int _notificationId = 2022;
        private readonly string _alarmBirdChannelId = "SuleymaniyeTakvimialarmbirdchannelId";
        private readonly string _alarmRoosterChannelId = "SuleymaniyeTakvimialarmroosterchannelId";
        private readonly string _alarmAdhanChannelId = "SuleymaniyeTakvimialarmadhanchannelId";
        private readonly string _alarmAlarmChannelId = "SuleymaniyeTakvimialarmalarmchannelId";
        private readonly string _alarm1AlarmChannelId = "SuleymaniyeTakvimialarmalarm1channelId";
        private readonly string _alarm2AlarmChannelId = "SuleymaniyeTakvimialarmalarm2channelId";
        private readonly string _alarm3AlarmChannelId = "SuleymaniyeTakvimialarmalarm3channelId";
        private readonly string _alarm4AlarmChannelId = "SuleymaniyeTakvimialarmalarm4channelId";
        private readonly string _birdsChannelName = "Suleymaniye Takvimi Alarm Birds";
        private readonly string _roosterChannelName = "Suleymaniye Takvimi Alarm Rooster";
        private readonly string _adhanChannelName = "Suleymaniye Takvimi Alarm Adhan";
        private readonly string _alarmChannelName = "Suleymaniye Takvimi Alarm Alarm";
        private readonly string _alarm1ChannelName = "Suleymaniye Takvimi Alarm Alarm 1";
        private readonly string _alarm2ChannelName = "Suleymaniye Takvimi Alarm Alarm 2";
        private readonly string _alarm3ChannelName = "Suleymaniye Takvimi Alarm Alarm 3";
        private readonly string _alarm4ChannelName = "Suleymaniye Takvimi Alarm Alarm 4";
        private readonly string _birdsChannelDescription = "The Suleymaniye Takvimi birds alarm channel.";
        private readonly string _roosterChannelDescription = "The Suleymaniye Takvimi rooster alarm channel.";
        private readonly string _adhanChannelDescription = "The Suleymaniye Takvimi adhan alarm channel.";
        private readonly string _alarmChannelDescription = "The Suleymaniye Takvimi alarm alarm channel.";
        private readonly string _alarm1ChannelDescription = "The Suleymaniye Takvimi alarm alarm 1 channel.";
        private readonly string _alarm2ChannelDescription = "The Suleymaniye Takvimi alarm alarm 2 channel.";
        private readonly string _alarm3ChannelDescription = "The Suleymaniye Takvimi alarm alarm 3 channel.";
        private readonly string _alarm4ChannelDescription = "The Suleymaniye Takvimi alarm alarm 4 channel.";
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                var name = intent?.GetStringExtra("name") ?? string.Empty;
                var timeStr = intent?.GetStringExtra("time") ?? string.Empty;
                //Avoid some intents that not scheduled by this app.
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(timeStr)) return;
                var time = TimeSpan.Parse(timeStr);
                //Toast.MakeText(context, "Received intent! " + name + ": " + time, ToastLength.Short).Show();
                _notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
                var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent;
                PendingIntent fullScreenPendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(AlarmActivity)), pendingIntentFlags);
                //var sound = GetRingtoneFileName(context, name);
                var birdsSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.kus}");
                var roosterSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.horoz}");
                var adhanSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.ezan}");
                var alarmSound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm}");
                var alarm1Sound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm2}");
                var alarm2Sound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep1}");
                var alarm3Sound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep2}");
                var alarm4Sound = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.beep3}");
                var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    .SetUsage(AudioUsageKind.NotificationRingtone).Build();
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    var birdsAlarmChannelNameJava = new String(_birdsChannelName);
                    var roosterAlarmChannelNameJava = new String(_roosterChannelName);
                    var adhanAlarmChannelNameJava = new String(_adhanChannelName);
                    var alarmAlarmChannelNameJava = new String(_alarmChannelName);
                    var alarm1AlarmChannelNameJava = new String(_alarm1ChannelName);
                    var alarm2AlarmChannelNameJava = new String(_alarm2ChannelName);
                    var alarm3AlarmChannelNameJava = new String(_alarm3ChannelName);
                    var alarm4AlarmChannelNameJava = new String(_alarm4ChannelName);
                    var birdsAlarmChannel = new NotificationChannel(_alarmBirdChannelId, birdsAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _birdsChannelDescription
                    };
                    var roosterAlarmChannel = new NotificationChannel(_alarmRoosterChannelId, roosterAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _roosterChannelDescription
                    };
                    var adhanAlarmChannel = new NotificationChannel(_alarmAdhanChannelId, adhanAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _adhanChannelDescription
                    };
                    var alarmAlarmChannel = new NotificationChannel(_alarmAlarmChannelId, alarmAlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _alarmChannelDescription
                    };
                    var alarm1AlarmChannel = new NotificationChannel(_alarm1AlarmChannelId, alarm1AlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _alarm1ChannelDescription
                    };
                    var alarm2AlarmChannel = new NotificationChannel(_alarm2AlarmChannelId, alarm2AlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _alarm2ChannelDescription
                    };
                    var alarm3AlarmChannel = new NotificationChannel(_alarm3AlarmChannelId, alarm3AlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _alarm3ChannelDescription
                    };
                    var alarm4AlarmChannel = new NotificationChannel(_alarm4AlarmChannelId, alarm4AlarmChannelNameJava, NotificationImportance.Max)
                    {
                        Description = _alarm4ChannelDescription
                    };
                    birdsAlarmChannel.SetSound(birdsSound, alarmAttributes);
                    roosterAlarmChannel.SetSound(roosterSound, alarmAttributes);
                    adhanAlarmChannel.SetSound(adhanSound, alarmAttributes);
                    alarmAlarmChannel.SetSound(alarmSound, alarmAttributes);
                    alarm1AlarmChannel.SetSound(alarm1Sound, alarmAttributes);
                    alarm2AlarmChannel.SetSound(alarm2Sound, alarmAttributes);
                    alarm3AlarmChannel.SetSound(alarm3Sound, alarmAttributes);
                    alarm4AlarmChannel.SetSound(alarm4Sound, alarmAttributes);
                    _notificationManager?.CreateNotificationChannel(birdsAlarmChannel);
                    _notificationManager?.CreateNotificationChannel(roosterAlarmChannel);
                    _notificationManager?.CreateNotificationChannel(adhanAlarmChannel);
                    _notificationManager?.CreateNotificationChannel(alarmAlarmChannel);
                    _notificationManager?.CreateNotificationChannel(alarm1AlarmChannel);
                    _notificationManager?.CreateNotificationChannel(alarm2AlarmChannel);
                    _notificationManager?.CreateNotificationChannel(alarm3AlarmChannel);
                    _notificationManager?.CreateNotificationChannel(alarm4AlarmChannel);
                }

                //Uri sound = Uri.Parse("android.resource://" + context.PackageName + "/" + Resource.Raw.horoz);
                var alarmId = GetAlarmId(name);
                NotificationCompat.Builder notificationBuilder = null;
                switch (alarmId)
                {
                    case "kus":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarmBirdChannelId).SetSound(birdsSound);
                        break;
                    case "horoz":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarmRoosterChannelId).SetSound(roosterSound);
                        break;
                    case "ezan":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarmAdhanChannelId).SetSound(adhanSound);
                        break;
                    case "alarm":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarmAlarmChannelId).SetSound(alarmSound);
                        break;
                    case "alarm2":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarm1AlarmChannelId).SetSound(alarm1Sound);
                        break;
                    case "beep1":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarm2AlarmChannelId).SetSound(alarm2Sound);
                        break;
                    case "beep2":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarm3AlarmChannelId).SetSound(alarm3Sound);
                        break;
                    case "beep3":
                        notificationBuilder = new NotificationCompat.Builder(context, _alarm4AlarmChannelId).SetSound(alarm4Sound);
                        break;
                    default:
                        notificationBuilder = new NotificationCompat.Builder(context, _alarmAlarmChannelId).SetSound(alarmSound);
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
                _notificationManager.Notify(_notificationId,alarmlNotification);
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception);
                //throw;
            }
        }

        private string GetAlarmId(string name)
        {
            var fileName = "";
            if (name != null)
                fileName = name switch
                {
                    "Fecri Kazip" => Preferences.Get("fecrikazipAlarmSesi", "alarm"),
                    "Fecri Sadık" => Preferences.Get("fecrisadikAlarmSesi", "alarm"),
                    "Sabah Sonu" => Preferences.Get("sabahsonuAlarmSesi", "alarm"),
                    "Öğle" => Preferences.Get("ogleAlarmSesi", "alarm"),
                    "İkindi" => Preferences.Get("ikindiAlarmSesi", "alarm"),
                    "Akşam" => Preferences.Get("aksamAlarmSesi", "alarm"),
                    "Yatsı" => Preferences.Get("yatsiAlarmSesi", "alarm"),
                    "Yatsı Sonu" => Preferences.Get("yatsisonuAlarmSesi", "alarm"),
                    //_ => "ezan"
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

        private string GetContent(string name, TimeSpan time)
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
                    //_ => $"şimdiki zaman: {time}"
                };

            return "";
        }

        private string GetTitle(string name)
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
                    //_ => "Test Alarmı",
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
            
            var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;
            var pendingIntent = PendingIntent.GetActivity(Application.Context, 0, notificationIntent, pendingIntentFlags);
            return pendingIntent;
        }
    }
}