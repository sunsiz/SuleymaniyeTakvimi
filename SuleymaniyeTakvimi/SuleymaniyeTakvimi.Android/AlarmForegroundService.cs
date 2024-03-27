using System;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Calendar = Java.Util.Calendar;

namespace SuleymaniyeTakvimi.Droid
{
    [Service]
    public class AlarmForegroundService : Service, IAlarmService
    {
        private NotificationManager _notificationManager;
        private const int DelayBetweenMessages = 30000;
        private const int NotificationId = 1993;
        private const string NotificationChannelId = "SuleymaniyeTakvimichannelId";
        private const string ChannelDescription = "The Suleymaniye Takvimi notification channel.";
        private readonly string _channelName = AppResources.SuleymaniyeVakfiTakvimi;
        private Notification _notification;
        private bool _isStarted;
        private Handler _handler;
        private Action _runnable;
        private int _counter;
        
        public override IBinder OnBind(Intent intent)
        {
            // Return null because this is a pure started service. A hybrid service would return a binder that would
            // allow access to the GetFormattedRemainingTime() method.
            return null;
        }

        /// <summary>
        /// The SetAlarm method is used to set an alarm at a specific time. It first calculates the trigger time by subtracting the time offset from the provided trigger time. It then creates an Intent for both the AlarmActivity and AlarmReceiver with the necessary extras. It also calculates a unique request code based on the name of the alarm to ensure that multiple alarms can be set without overriding each other.
        /// The method then creates a PendingIntent for both the activity and the broadcast receiver. Depending on the Android version, it either sets an exact alarm that will wake up the device even if it's in idle mode or sets an alarm clock.
        /// </summary>
        /// <param name="date">Alarm Date</param>
        /// <param name="triggerTimeSpan">Alarm triggering time span (hour and minute i.g. 17:19)</param>
        /// <param name="timeOffset">Alarm triggering offset, generally 0~60 minutes before the trigger time</param>
        /// <param name="name">Alarm prayer time name</param>
        public void SetAlarm(DateTime date, TimeSpan triggerTimeSpan, int timeOffset, string name)
        {
            System.Diagnostics.Debug.WriteLine($"**** Set Alarm in AlarmForeGround Triggered with {date.ToString(CultureInfo.InvariantCulture)}, {triggerTimeSpan.ToString()}, {timeOffset}, {name}");
                var prayerTimeSpan = triggerTimeSpan;
                triggerTimeSpan -= TimeSpan.FromMinutes(timeOffset);
            using (var alarmManager = (AlarmManager)Application.Context.GetSystemService(AlarmService))
            using (var calendar = Calendar.Instance)
            {
                //Log.Info("SetAlarm", $"Before Alarm set the Calendar time is {calendar.Time} for {name}");
                calendar.Set(date.Year, date.Month-1, date.Day, triggerTimeSpan.Hours, triggerTimeSpan.Minutes, 0);
                var activityIntent = new Intent(Application.Context, typeof(AlarmActivity));
                activityIntent.PutExtra("name", name);
                activityIntent.PutExtra("time", prayerTimeSpan.ToString());
                activityIntent.AddFlags(ActivityFlags.ReceiverForeground);
                var intent = new Intent(Application.Context, typeof(AlarmReceiver));
                intent.PutExtra("name", name);
                intent.PutExtra("time", prayerTimeSpan.ToString());
                intent.AddFlags(ActivityFlags.IncludeStoppedPackages);
                intent.AddFlags(ActivityFlags.ReceiverForeground);
                //without the different reuestCode there will be only one pending intent and it updates every schedule, so only one alarm will be active at the end.
                var requestCode = GetRequestCode(name, date.DayOfYear);
                var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent;
                var pendingActivityIntent = PendingIntent.GetActivity(Application.Context, requestCode, activityIntent, pendingIntentFlags);
                var pendingIntent = PendingIntent.GetBroadcast(Application.Context, requestCode, intent, pendingIntentFlags);
                //alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup,calendar.TimeInMillis,pendingActivityIntent);
                //alarmManager.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pendingActivityIntent);
                if (Build.VERSION.SdkInt <= BuildVersionCodes.P)
                    alarmManager?.SetAlarmClock(new AlarmManager.AlarmClockInfo(calendar.TimeInMillis, pendingActivityIntent), pendingActivityIntent);
                else
                    alarmManager?.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, calendar.TimeInMillis, pendingIntent);
                //else
                //    alarmManager?.SetExact(AlarmType.RtcWakeup, calendar.TimeInMillis, pendingIntent);
                System.Diagnostics.Debug.WriteLine("SetAlarm", $"Alarm set for {calendar.Time} for {name}");
            }
        }

        private int GetRequestCode(string name, int dayOfYear)
        {
            return name switch
            {
                "fecrikazip" => dayOfYear + 1000,
                "fecrisadik" => dayOfYear + 2000,
                "sabahsonu" => dayOfYear + 3000,
                "ogle" => dayOfYear + 4000,
                "ikindi" => dayOfYear + 5000,
                "aksam" => dayOfYear + 6000,
                "yatsi" => dayOfYear + 7000,
                "yatsisonu" => dayOfYear + 8000,
                _ => 0
            };
        }

        public void CancelAlarm()
        {
            var alarmService = Application.Context.GetSystemService(Context.AlarmService);
            if (alarmService is AlarmManager alarmManager)
            {
                var intent = new Intent(Application.Context, typeof(AlarmActivity));
                var pendingIntentFlags = Build.VERSION.SdkInt > BuildVersionCodes.R
                    ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                    : PendingIntentFlags.UpdateCurrent;
                var pendingIntent = PendingIntent.GetBroadcast(Application.Context, 0, intent, pendingIntentFlags);
                alarmManager.Cancel(pendingIntent);
            }
        }

        /// <summary>
        /// Called by the system when the service is first created.
        /// </summary>
        /// <remarks>
        /// This method initializes the handler, notification manager, and notification for the service.
        /// If the preference "ForegroundServiceEnabled" is set, it starts the service in the foreground.
        /// It then posts a delayed runnable to the handler that will update the notification every 30 seconds and refresh the widget every 30 minutes.
        /// The _isStarted flag is set to true and the CancelAlarm method is called.
        /// </remarks>
        public override void OnCreate()
        {
            base.OnCreate();
            _handler = new Handler();
            _notificationManager = Application.Context.GetSystemService(Context.NotificationService) as NotificationManager;
            SetNotification();

            if(Preferences.Get("ForegroundServiceEnabled",true))this.StartForeground(NotificationId, _notification);

            // This Action will run every 30 second as foreground service running.
            _runnable = new Action(() =>
            {
                _handler.PostDelayed(_runnable, DelayBetweenMessages);
                SetNotification();
                _notificationManager.Notify(NotificationId, _notification);
                _counter++;

                if (_counter == 60) //When the 60th time (30 minute) refresh widget manually.
                {
                    StartWidgetService();
                    _counter = 0;
                }
            });
            _handler.PostDelayed(_runnable, DelayBetweenMessages);
            _isStarted = true;
            CancelAlarm();
        }

        private void StartWidgetService()
        {
            var intent = new Intent(ApplicationContext, typeof(WidgetService));

            try
            {
                ApplicationContext.StartService(intent);
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"An exception occurred when starting widget service, details: {exception.Message}");
            }
        }

        /// <summary>
        /// Sets the notification for the foreground service.
        /// </summary>
        /// <remarks>
        /// This method first creates a BigTextStyle for the notification. If the preference "NotificationPrayerTimesEnabled" is set,
        /// it sets the big text and summary text for the notification.
        ///
        /// Then, it creates a Notification.Builder. The builder's parameters depend on the Android version.
        /// The builder sets the content title, style, small icon, content intent, when to show, and other properties for the notification.
        ///
        /// If the Android version is Oreo or higher, it creates a NotificationChannel and sets the description, light color, and lockscreen visibility for the channel.
        /// The channel is then created in the notification manager.
        /// </remarks>
        private void SetNotification()
        {
            // Create a BigTextStyle for the notification
            var textStyle = new Notification.BigTextStyle();

            // If the preference "NotificationPrayerTimesEnabled" is set, set the big text and summary text for the notification
            if (Preferences.Get("NotificationPrayerTimesEnabled", false))
            {
                textStyle.BigText(GetTodaysPrayerTimes());
                textStyle.SetSummaryText(AppResources.BugunkuNamazVakitleri);
            }

            // Create a Notification.Builder. The builder's parameters depend on the Android version
            var notificationBuilder = Build.VERSION.SdkInt >= BuildVersionCodes.O
                ? new Notification.Builder(this, NotificationChannelId)
                : new Notification.Builder(this);

            // Set the content title, style, small icon, content intent, when to show, and other properties for the notification
            _notification = notificationBuilder
                .SetContentTitle(GetFormattedRemainingTime())
                .SetStyle(textStyle)
                .SetSmallIcon(Resource.Drawable.app_logo)
                .SetContentIntent(BuildIntentToShowMainActivity())
                .SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis())
                .SetShowWhen(true)
                .SetOngoing(true)
                .SetOnlyAlertOnce(true)
                .Build();

            // If the Android version is Oreo or higher, create a NotificationChannel and set the description, light color, and lockscreen visibility for the channel
            // The channel is then created in the notification manager
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                var channelNameJava = new Java.Lang.String(_channelName);
                var channel = new NotificationChannel(NotificationChannelId, channelNameJava, NotificationImportance.Default)
                {
                    Description = ChannelDescription,
                    LightColor = 1,
                    LockscreenVisibility = NotificationVisibility.Public
                };
                _notificationManager.CreateNotificationChannel(channel);
            }
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
            var notificationIntent = new Intent(this, typeof(MainActivity));
            notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
            notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
            //notificationIntent.PutExtra("has_service_been_started", true);
            
            var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;
            var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, pendingIntentFlags);
            return pendingIntent;
        }

        /// <summary>
        /// This method calculates the remaining time until the next prayer time and returns a formatted string message.
        /// </summary>
        /// <returns>
        /// A string message that indicates the remaining time until the next prayer time.
        /// </returns>
        /// <remarks>
        /// The method first retrieves the current prayer times from the DataService and the current time of day.
        /// It then checks the current time against each prayer time in order.
        /// If the current time is less than a prayer time, it calculates the remaining time until that prayer time,
        /// If the current time is greater than the last prayer time, calculate the elapsed time since that prayer time,
        /// formats it as a string, and sets the message to a localized string indicating the remaining time until that prayer time.
        /// If an exception occurs (for example, if the prayer times cannot be parsed as TimeSpan values),
        /// it logs the exception and sets the message to a localized string indicating that the location permission is required.
        /// </remarks>
        private string GetFormattedRemainingTime()
        {
            var message = "";
            var data = new DataService();
            var takvim = data._takvim;
            var currentTime = DateTime.Now.TimeOfDay;
            try
            {
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
            }
            catch (Exception exception)
            {
                System.Diagnostics.Debug.WriteLine($"GetFormattedRemainingTime exception: {exception.Message}. Location: {takvim.Enlem}, {takvim.Boylam}");
                Log.Error("GetFormattedRemainingTime",$"GetFormattedRemainingTime exception: {exception.Message}. Location: {takvim.Enlem}, {takvim.Boylam}");
                message = AppResources.KonumIzniIcerik;
            }

            return message;
        }

        private string GetTodaysPrayerTimes()
        {
            var data = new DataService();
            var takvim = data._takvim;

            var message = new StringBuilder();
            message.AppendLine($"{AppResources.FecriKazip}: {takvim.FecriKazip}");
            message.AppendLine($"{AppResources.FecriSadik}: {takvim.FecriSadik}");
            message.AppendLine($"{AppResources.SabahSonu}: {takvim.SabahSonu}");
            message.AppendLine($"{AppResources.Ogle}: {takvim.Ogle}");
            message.AppendLine($"{AppResources.Ikindi}: {takvim.Ikindi}");
            message.AppendLine($"{AppResources.Aksam}: {takvim.Aksam}");
            message.Append($"{AppResources.Yatsi}: {takvim.Yatsi}");
            message.Append($"{AppResources.YatsiSonu}: {takvim.YatsiSonu}");

            return message.ToString();
        }

        /// <summary>
        /// This method is called when the service is started. It handles the actions to be performed based on the intent action.
        /// </summary>
        /// <param name="intent">The Intent supplied to start the service, as given to StartService(Intent).</param>
        /// <param name="flags">Additional data about this start request. Currently either 0, StartCommandFlags.Redelivery, or StartCommandFlags.Retry.</param>
        /// <param name="startId">A unique integer representing this specific request to start. Use with StopSelfResult(int).</param>
        /// <returns>The return value indicates what semantics the system should use for the service's current started state.</returns>
        /// <remarks>
        /// If the action of the intent is "SuleymaniyeTakvimi.action.START_SERVICE" and the service is not already started, 
        /// it starts the service in the foreground, posts a delayed runnable to the handler, sets the _isStarted flag to true, 
        /// and starts a task to delay for 12 seconds and then set the weekly alarms.
        /// 
        /// If the action of the intent is "SuleymaniyeTakvimi.action.STOP_SERVICE", it stops the service from running in the foreground, 
        /// stops the service itself, and sets the _isStarted flag to false.
        /// 
        /// If the intent or its action is null, it logs a debug message and returns StartCommandResult.RedeliverIntent to indicate that 
        /// the system should create a new service object and call OnStartCommand(Intent, StartCommandFlags, int) with the last intent that 
        /// was delivered to the service.
        /// 
        /// If none of the above conditions are met, it returns StartCommandResult.Sticky to indicate that the system should not try to 
        /// recreate the service after it has been killed and does not need to have the service's commands redelivered.
        /// </remarks>
        public override StartCommandResult OnStartCommand(Intent intent, StartCommandFlags flags, int startId)
        {
            if (intent?.Action == null)
            {
                var source = null == intent ? "intent" : "action";
                System.Diagnostics.Debug.WriteLine("OnStartCommand Null Intent Exception: " + source +
                                                   " was null, flags=" + flags + " bits=" + flags);
                return StartCommandResult.RedeliverIntent;
            }

            switch (intent.Action)
            {
                //Analytics.TrackEvent("OnStartCommand in the AlarmForegroundService Triggered: " + $" at {DateTime.Now}");
                case "SuleymaniyeTakvimi.action.START_SERVICE":
                    if (!_isStarted) //else Log.Info(TAG, "OnStartCommand: The service is already running.");
                    {
                        //Log.Info(TAG, "OnStartCommand: The service is starting.");
                        // Enlist this instance of the service as a foreground service
                        this.StartForeground(NotificationId, _notification);
                        _handler.PostDelayed(_runnable, DelayBetweenMessages);
                        _isStarted = true;
                        Task.Run(async () =>
                        {
                            await Task.Delay(12000);
                            System.Diagnostics.Debug.WriteLine($"OnStartCommand: Starting Set Alarm at {DateTime.Now}");
                            var data = new DataService();
                            await data.SetWeeklyAlarmsAsync();
                        });
                    }

                    break;
                case "SuleymaniyeTakvimi.action.STOP_SERVICE":
                    //Log.Info(TAG, "OnStartCommand: The service is stopping.");
                    StopForeground(true);
                    StopSelf(NotificationId);
                    _isStarted = false;
                    break;
            }

            return StartCommandResult.Sticky;
        }

        /// <summary>
        /// Starts the foreground service for the alarm.
        /// </summary>
        /// <remarks>
        /// This method creates an Intent for the AlarmForegroundService and sets the action to "SuleymaniyeTakvimi.action.START_SERVICE".
        /// It then calls the StartTheService method with this intent to start the service.
        /// </remarks>
        public void StartAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity SetAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            
            var startServiceIntent = new Intent(Application.Context, typeof(AlarmForegroundService));
            startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
            StartTheService(startServiceIntent);
            System.Diagnostics.Debug.WriteLine("Main Activity" + $"Main Activity SetAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }

        /// <summary>
        /// Stops the foreground service for the alarm.
        /// </summary>
        /// <remarks>
        /// This method creates an Intent for the AlarmForegroundService and sets the action to "SuleymaniyeTakvimi.action.STOP_SERVICE".
        /// It then calls the StartTheService method with this intent to stop the service.
        /// </remarks>
        public void StopAlarmForegroundService()
        {
            Log.Info("Main Activity", $"Main Activity StopAlarmForegroundService Started: {DateTime.Now:HH:m:s.fff}");
            //var startServiceIntent = new Intent(this, typeof(ForegroundService));
            var stopServiceIntent = new Intent(Application.Context, typeof(AlarmForegroundService));
            stopServiceIntent.SetAction("SuleymaniyeTakvimi.action.STOP_SERVICE");
            StartTheService(stopServiceIntent);
            System.Diagnostics.Debug.WriteLine("Main Activity" + $"Main Activity StopAlarmForegroundService Finished: {DateTime.Now:HH:m:s.fff}");
        }

        private void StartTheService(Intent serviceIntent)
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    Application.Context?.StartForegroundService(serviceIntent);
                }
                else
                {
                    Application.Context?.StartService(serviceIntent);
                }
            });
        }
    }
}