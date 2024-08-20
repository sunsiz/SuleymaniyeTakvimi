using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Media;
using AndroidX.Core.App;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Uri = Android.Net.Uri;
using Xamarin.Essentials;
using Debug = System.Diagnostics.Debug;
using String = Java.Lang.String;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class AlarmReceiver : BroadcastReceiver
    {
        private NotificationManager _notificationManager;
        private const int NotificationId = 2022;
        private const string AlarmBirdChannelId = "SuleymaniyeTakvimialarmbirdchannelId";
        private const string AlarmRoosterChannelId = "SuleymaniyeTakvimialarmroosterchannelId";
        private const string AlarmAdhanChannelId = "SuleymaniyeTakvimialarmadhanchannelId";
        private const string AlarmAlarmChannelId = "SuleymaniyeTakvimialarmalarmchannelId";
        private const string Alarm1AlarmChannelId = "SuleymaniyeTakvimialarmalarm1channelId";
        private const string Alarm2AlarmChannelId = "SuleymaniyeTakvimialarmalarm2channelId";
        private const string Alarm3AlarmChannelId = "SuleymaniyeTakvimialarmalarm3channelId";
        private const string Alarm4AlarmChannelId = "SuleymaniyeTakvimialarmalarm4channelId";
        private const string BirdsChannelName = "Suleymaniye Takvimi Alarm Birds";
        private const string RoosterChannelName = "Suleymaniye Takvimi Alarm Rooster";
        private const string AdhanChannelName = "Suleymaniye Takvimi Alarm Adhan";
        private const string AlarmChannelName = "Suleymaniye Takvimi Alarm Alarm";
        private const string Alarm1ChannelName = "Suleymaniye Takvimi Alarm Alarm 1";
        private const string Alarm2ChannelName = "Suleymaniye Takvimi Alarm Alarm 2";
        private const string Alarm3ChannelName = "Suleymaniye Takvimi Alarm Alarm 3";
        private const string Alarm4ChannelName = "Suleymaniye Takvimi Alarm Alarm 4";
        private const string BirdsChannelDescription = "The Suleymaniye Takvimi birds alarm channel.";
        private const string RoosterChannelDescription = "The Suleymaniye Takvimi rooster alarm channel.";
        private const string AdhanChannelDescription = "The Suleymaniye Takvimi adhan alarm channel.";
        private const string AlarmChannelDescription = "The Suleymaniye Takvimi alarm alarm channel.";
        private const string Alarm1ChannelDescription = "The Suleymaniye Takvimi alarm alarm 1 channel.";
        private const string Alarm2ChannelDescription = "The Suleymaniye Takvimi alarm alarm 2 channel.";
        private const string Alarm3ChannelDescription = "The Suleymaniye Takvimi alarm alarm 3 channel.";
        private const string Alarm4ChannelDescription = "The Suleymaniye Takvimi alarm alarm 4 channel.";

        /// <summary>
        /// This method is called when the BroadcastReceiver is receiving an Intent broadcast.
        /// It processes the received intent, extracts the necessary information, and creates a notification.
        /// </summary>
        /// <param name="context">The Context in which the receiver is running.</param>
        /// <param name="intent">The Intent being received.</param>
        public override void OnReceive(Context context, Intent intent)
        {
            try
            {
                // Extract the name and time from the intent.
                var name = intent?.GetStringExtra("name") ?? string.Empty;
                var timeStr = intent?.GetStringExtra("time") ?? string.Empty;
                
                // If the name or time is missing, this intent was not scheduled by this app, so ignore it.
                if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(timeStr)) return;
                
                // Parse the time string into a TimeSpan.
                //var time = TimeSpan.Parse(timeStr);
                //Toast.MakeText(context, "Received intent! " + name + ": " + time, ToastLength.Short).Show();
                // Get the notification manager service.
                _notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);

                // Create a PendingIntent for the full-screen intent.
                var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent;
                PendingIntent fullScreenPendingIntent = PendingIntent.GetActivity(context, 0, new Intent(context, typeof(AlarmActivity)), pendingIntentFlags);
                
                // Get the alarm ID, sound URI, and channel ID based on the name.
                var alarmId = GetAlarmId(name);
                var soundUri = GetSoundUri(context, alarmId);
                var channelId = GetChannelId(alarmId);
                
                // Create the audio attributes for the notification.
                var alarmAttributes = new AudioAttributes.Builder()
                    .SetContentType(AudioContentType.Sonification)
                    ?.SetUsage(AudioUsageKind.NotificationRingtone)
                    ?.Build();
                
                // If the Android version is Oreo or higher, create the notification channels.
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    // Define the channel IDs, names, and descriptions.
                    var channelIds = new List<string> { AlarmBirdChannelId, AlarmRoosterChannelId, AlarmAdhanChannelId, AlarmAlarmChannelId, Alarm1AlarmChannelId, Alarm2AlarmChannelId, Alarm3AlarmChannelId, Alarm4AlarmChannelId };
                    var channelNames = new List<string> { BirdsChannelName, RoosterChannelName, AdhanChannelName, AlarmChannelName, Alarm1ChannelName, Alarm2ChannelName, Alarm3ChannelName, Alarm4ChannelName };
                    var channelDescriptions = new List<string> { BirdsChannelDescription, RoosterChannelDescription, AdhanChannelDescription, AlarmChannelDescription, Alarm1ChannelDescription, Alarm2ChannelDescription, Alarm3ChannelDescription, Alarm4ChannelDescription };
                    
                    // Create each notification channel.
                    for (int i = 0; i < channelIds.Count; i++)
                    {
                        var channelNameJava = new String(channelNames[i]);
                        var alarmChannel = new NotificationChannel(channelIds[i], channelNameJava, NotificationImportance.Max)
                        {
                            Description = channelDescriptions[i]
                        };
                        var channelSoundUri = GetSoundUri(context, DataService.SoundNameKeys[i]);
                        alarmChannel.SetSound(channelSoundUri, alarmAttributes);
                        _notificationManager?.CreateNotificationChannel(alarmChannel);
                        Debug.WriteLine($"**** Alarm Receiver - {channelIds[i]} - {channelNameJava} - {channelDescriptions[i]} - {channelSoundUri}");
                    }
                }
                
                // Build the notification.
                var notificationBuilder = new NotificationCompat.Builder(context, channelId)
                    .SetSound(soundUri)
                    .SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentTitle(GetTitle(name))
                    .SetContentText(GetContent(name, timeStr))
                    .SetPriority(NotificationCompat.PriorityMax)
                    .SetCategory(NotificationCompat.CategoryAlarm)
                    .SetAutoCancel(true)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetLargeIcon(BitmapFactory.DecodeResource(Application.Context.Resources, Resource.Drawable.app_logo))
                    .SetDefaults(0)
                    .SetFullScreenIntent(fullScreenPendingIntent, true);
                
                // Create the notification and notify the notification manager.
                Notification alarmlNotification = notificationBuilder.Build();
                _notificationManager?.Notify(NotificationId,alarmlNotification);
            }
            catch (Exception exception)
            {
                // Log any exceptions that occur while processing the intent.
                Console.WriteLine($@"**** Alarm reciver OnReceive exception: {exception}");
            }
        }

        private static string GetAlarmId(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return name switch
            {
                "fecrikazip" => Preferences.Get("fecrikazipAlarmSesi", "alarm"),
                "fecrisadik" => Preferences.Get("fecrisadikAlarmSesi", "alarm"),
                "sabahsonu" => Preferences.Get("sabahsonuAlarmSesi", "alarm"),
                "ogle" => Preferences.Get("ogleAlarmSesi", "alarm"),
                "ikindi" => Preferences.Get("ikindiAlarmSesi", "alarm"),
                "aksam" => Preferences.Get("aksamAlarmSesi", "alarm"),
                "yatsi" => Preferences.Get("yatsiAlarmSesi", "alarm"),
                "yatsisonu" => Preferences.Get("yatsisonuAlarmSesi", "alarm"),
                _ => string.Empty
            };
        }

        private Uri GetSoundUri(Context context, string alarmId)
        {
            var soundResource = alarmId switch
            {
                "kus" => Resource.Raw.kus,
                "horoz" => Resource.Raw.horoz,
                "ezan" => Resource.Raw.ezan,
                "alarm" => Resource.Raw.alarm,
                "alarm2" => Resource.Raw.alarm2,
                "beep1" => Resource.Raw.beep1,
                "beep2" => Resource.Raw.beep2,
                "beep3" => Resource.Raw.beep3,
                _ => Resource.Raw.alarm
            };

            return Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{soundResource}");
        }

        private string GetChannelId(string alarmId)
        {
            return alarmId switch
            {
                "kus" => AlarmBirdChannelId,
                "horoz" => AlarmRoosterChannelId,
                "ezan" => AlarmAdhanChannelId,
                "alarm" => AlarmAlarmChannelId,
                "alarm2" => Alarm1AlarmChannelId,
                "beep1" => Alarm2AlarmChannelId,
                "beep2" => Alarm3AlarmChannelId,
                "beep3" => Alarm4AlarmChannelId,
                _ => AlarmAlarmChannelId
            };
        }

        private string GetContent(string name, string time)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return name switch
            {
                "fecrikazip" => $"{AppResources.FecriKazip} {AppResources.Vakti} {time}",
                "fecrisadik" => $"{AppResources.FecriSadik} {AppResources.Vakti} {time}",
                "Sabahsonu" => $"{AppResources.SabahSonu} {AppResources.Vakti} {time}",
                "ogle" => $"{AppResources.Ogle} {AppResources.Vakti} {time}",
                "ikindi" => $"{AppResources.Ikindi} {AppResources.Vakti} {time}",
                "aksam" => $"{AppResources.Aksam} {AppResources.Vakti} {time}",
                "yatsi" => $"{AppResources.Yatsi} {AppResources.Vakti} {time}",
                "yatsisonu" => $"{AppResources.YatsiSonu} {AppResources.Vakti} {time}",
                _ => string.Empty
            };
        }

        private string GetTitle(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                return string.Empty;
            }

            return name switch
            {
                "fecrikazip" => AppResources.FecriKazip + " " + AppResources.Alarmi,
                "fecrisadik" => AppResources.FecriSadik + " " + AppResources.Alarmi,
                "sabahsonu" => AppResources.SabahSonu + " " + AppResources.Alarmi,
                "ogle" => AppResources.Ogle + " " + AppResources.Alarmi,
                "ikindi" => AppResources.Ikindi + " " + AppResources.Alarmi,
                "aksam" => AppResources.Aksam + " " + AppResources.Alarmi,
                "yatsi" => AppResources.Yatsi + " " + AppResources.Alarmi,
                "yatsisonu" => AppResources.YatsiSonu + " " + AppResources.Alarmi,
                _ => string.Empty
            };
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        private PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(Application.Context, typeof(MainActivity));
            notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);

            var pendingIntentFlags = Build.VERSION.SdkInt > BuildVersionCodes.R
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;

            return PendingIntent.GetActivity(Application.Context, 0, notificationIntent, pendingIntentFlags);
        }
    }
}