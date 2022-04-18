using System;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using Java.Util;
using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    [Service]
    public class AlarmForegroundService : Service, IAlarmService
    {
        private NotificationManager _notificationManager;
        private readonly int DELAY_BETWEEN_MESSAGES = 30000;
        private readonly int NOTIFICATION_ID = 1993;
        private readonly string NOTIFICATION_CHANNEL_ID = "SuleymaniyeTakvimichannelId";
        private readonly string channelName = "Suleymaniye Takvimi";
        private readonly string channelDescription = "The Suleymaniye Takvimi notification channel.";
        private Notification _notification;
        private bool _isStarted;
        private Handler _handler;
        private Action _runnable;
        
        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedRemainingTime() method.
            return null;
        }

        public void SetAlarm(DateTime date, TimeSpan triggerTimeSpan, int timeOffset, string name)
        {
            using (var alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService))
            using (var calendar = Calendar.Instance)
            {
                var prayerTimeSpan = triggerTimeSpan;
                triggerTimeSpan -= TimeSpan.FromMinutes(timeOffset);
                //Log.Info("SetAlarm", $"Before Alarm set the Calendar time is {calendar.Time} for {name}");
                calendar.Set(date.Year, date.Month-1, date.Day, triggerTimeSpan.Hours, triggerTimeSpan.Minutes, 0);
                var activityIntent = new Intent(Application.Context, typeof(AlarmActivity));
                activityIntent.PutExtra("name", name);
                activityIntent.PutExtra("time", prayerTimeSpan.ToString());
                activityIntent.AddFlags(ActivityFlags.ReceiverForeground);
                //without the different reuestCode there will be only one pending intent and it updates every schedule, so only one alarm will be active at the end.
                var requestCode = name switch
                {
                    "Fecri Kazip" => date.DayOfYear + 1000,
                    "Fecri Sadık" => date.DayOfYear + 2000,
                    "Sabah Sonu" => date.DayOfYear + 3000,
                    "Öğle" => date.DayOfYear + 4000,
                    "İkindi" => date.DayOfYear + 5000,
                    "Akşam" => date.DayOfYear + 6000,
                    "Yatsı" => date.DayOfYear + 7000,
                    "Yatsı Sonu" => date.DayOfYear + 8000,
                    _ => 0
                };
                var pendingActivityIntent = PendingIntent.GetActivity(Application.Context, requestCode, activityIntent,
                    PendingIntentFlags.UpdateCurrent);
                //alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup,calendar.TimeInMillis,pendingActivityIntent);
                //alarmManager.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pendingActivityIntent);
                alarmManager?.SetAlarmClock(new AlarmManager.AlarmClockInfo(calendar.TimeInMillis, pendingActivityIntent), pendingActivityIntent);
                System.Diagnostics.Debug.WriteLine("SetAlarm", $"Alarm set for {calendar.Time} for {name}");
            }
        }

        public void CancelAlarm()
        {
            Analytics.TrackEvent("CancelAlarm in the AlarmForegroundService");
            AlarmManager alarmManager = (AlarmManager)Application.Context.GetSystemService(Context.AlarmService);
            Intent intent = new Intent(Application.Context, typeof(AlarmActivity));
            PendingIntent pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, PendingIntentFlags.UpdateCurrent);
            alarmManager?.Cancel(pendingIntent);
        }

        public override void OnCreate()
        {
            base.OnCreate();
            _handler = new Handler();
            _notificationManager = (NotificationManager)Application.Context.GetSystemService(Context.NotificationService);
            SetNotification();

            this.StartForeground(NOTIFICATION_ID, _notification);

            // This Action will run every 30 second as foreground service running.
            _runnable = new Action(() =>
            {
                _handler.PostDelayed(_runnable, DELAY_BETWEEN_MESSAGES);
                SetNotification();
                _notificationManager.Notify(NOTIFICATION_ID, _notification);
            });
            _handler.PostDelayed(_runnable, DELAY_BETWEEN_MESSAGES);
            _isStarted = true;
            CancelAlarm();
        }

        private void SetNotification()
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(channelName);
                var channel = new NotificationChannel(NOTIFICATION_CHANNEL_ID, channelNameJava, NotificationImportance.Low)
                {
                    Description = channelDescription,
                    LightColor = 1,
                    LockscreenVisibility = NotificationVisibility.Public
                };
                _notificationManager.CreateNotificationChannel(channel);
                _notification = new Notification.Builder(this, NOTIFICATION_CHANNEL_ID)
                    .SetContentTitle(AppResources.SuleymaniyeVakfiTakvimi)
                    .SetContentText(GetFormattedRemainingTime())
                    .SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                    .SetShowWhen(true)
                    .SetOngoing(true)
                    .SetOnlyAlertOnce(true)
                    //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .Build();
            }
            else
            {
                _notification = new Notification.Builder(this)
                    .SetContentTitle(AppResources.SuleymaniyeVakfiTakvimi)
                    .SetContentText(GetFormattedRemainingTime())
                    .SetSmallIcon(Resource.Drawable.app_logo)
                    .SetContentIntent(BuildIntentToShowMainActivity())
                    .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                    .SetShowWhen(true)
                    .SetOngoing(true)
                    .SetOnlyAlertOnce(true)
                    //.SetDefaults(NotificationDefaults.Sound | NotificationDefaults.Vibrate)
                    .Build();
            }
        }

        /// <summary>
        /// Builds a PendingIntent that will display the main activity of the app. This is used when the 
        /// user taps on the notification; it will take them to the main activity of the app.
        /// </summary>
        /// <returns>The content intent.</returns>
        PendingIntent BuildIntentToShowMainActivity()
        {
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            //notificationIntent.PutExtra("has_service_been_started", true);

            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
            return pendingIntent;
        }

        private string GetFormattedRemainingTime()
        {
            var message = "";
            var data = new DataService();
            var takvim = data._takvim;
            var currentTime = DateTime.Now.TimeOfDay;
            if (currentTime < TimeSpan.Parse(takvim.FecriKazip))
                message = AppResources.FecriKazibingirmesinekalanvakit +
                          (TimeSpan.Parse(takvim.FecriKazip) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.FecriKazip) && currentTime <= TimeSpan.Parse(takvim.FecriSadik))
                message = AppResources.FecriSadikakalanvakit +
                          (TimeSpan.Parse(takvim.FecriSadik) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.FecriSadik) && currentTime <= TimeSpan.Parse(takvim.SabahSonu))
                message = AppResources.SabahSonunakalanvakit +
                          (TimeSpan.Parse(takvim.SabahSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.SabahSonu) && currentTime <= TimeSpan.Parse(takvim.Ogle))
                message = AppResources.Ogleningirmesinekalanvakit +
                          (TimeSpan.Parse(takvim.Ogle) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ogle) && currentTime <= TimeSpan.Parse(takvim.Ikindi))
                message = AppResources.Oglenincikmasinakalanvakit +
                          (TimeSpan.Parse(takvim.Ikindi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ikindi) && currentTime <= TimeSpan.Parse(takvim.Aksam))
                message = AppResources.Ikindinincikmasinakalanvakit +
                          (TimeSpan.Parse(takvim.Aksam) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Aksam) && currentTime <= TimeSpan.Parse(takvim.Yatsi))
                message = AppResources.Aksamincikmasnakalanvakit +
                          (TimeSpan.Parse(takvim.Yatsi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Yatsi) && currentTime <= TimeSpan.Parse(takvim.YatsiSonu))
                message = AppResources.Yatsinincikmasinakalanvakit +
                          (TimeSpan.Parse(takvim.YatsiSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.YatsiSonu))
                message = AppResources.Yatsininciktigindangecenvakit +
                          (currentTime - TimeSpan.Parse(takvim.YatsiSonu)).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");

            return message;
        }

        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent == null || intent.Action == null)
            {
                var source = null == intent ? "intent" : "action";
                System.Diagnostics.Debug.WriteLine("OnStartCommand Null Intent Exception: " + source + " was null, flags=" + flags + " bits=" + flags);
                return StartCommandResult.RedeliverIntent;
            }
            Analytics.TrackEvent("OnStartCommand in the AlarmForegroundService");
            if (intent.Action.Equals("SuleymaniyeTakvimi.action.START_SERVICE"))
            {
                if (_isStarted)
                {
                    //Log.Info(TAG, "OnStartCommand: The service is already running.");
                }
                else
                {
                    //Log.Info(TAG, "OnStartCommand: The service is starting.");
                    // Enlist this instance of the service as a foreground service
                    this.StartForeground(NOTIFICATION_ID, _notification);
                    _handler.PostDelayed(_runnable, DELAY_BETWEEN_MESSAGES);
                    _isStarted = true;
                    Task startupWork = new Task(async () =>
                    {
                        await Task.Delay(12000).ConfigureAwait(true);
                        System.Diagnostics.Debug.WriteLine("OnStartCommand: " + $"Starting Set Alarm at {DateTime.Now}");
                        DataService data = new DataService();
                        data.SetWeeklyAlarms();
                    });
                    startupWork.Start();
                    
                }
            }
            else if (intent.Action.Equals("SuleymaniyeTakvimi.action.STOP_SERVICE"))
            {
                //Log.Info(TAG, "OnStartCommand: The service is stopping.");
                StopForeground(true);
                StopSelf(NOTIFICATION_ID);
                _isStarted = false;
            }

            return StartCommandResult.Sticky;
        }
        
        public void StartAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity SetAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            
            var _startServiceIntent = new Intent(Application.Context, typeof(AlarmForegroundService));
            _startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Application.Context?.StartForegroundService(_startServiceIntent);
                }
                else
                {
                    Application.Context?.StartService(_startServiceIntent);
                }
            });
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity SetAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }

        public void StopAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity StopAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            var _stopServiceIntent = new Intent(Application.Context, typeof(AlarmForegroundService));
            _stopServiceIntent.SetAction("SuleymaniyeTakvimi.action.STOP_SERVICE");
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Application.Context?.StartForegroundService(_stopServiceIntent);
                }
                else
                {
                    Application.Context?.StartService(_stopServiceIntent);
                }
            });
            System.Diagnostics.Debug.WriteLine("Main Activity", $"Main Activity StopAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }
    }
}