using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.Util;
using Xamarin.Essentials;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Debug = System.Diagnostics.Debug;
using Uri = Android.Net.Uri;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MyTheme.Alarm", NoHistory = true)]
    public class AlarmActivity : Activity, View.IOnClickListener
    {
        private static MediaPlayer _player;
        const string DefaultAlarmSesi = "ezan";
        const int MinuteInMilliseconds = 60000;

        // This method is called when the activity is starting. It initializes the activity and sets up the UI.
        /// <summary>
        /// The OnCreate method is part of the Android Activity lifecycle and is called when the activity is first created.
        /// This method sets up the user interface for the alarm activity.
        /// It retrieves the name and time from the intent that started the activity and uses these to configure the alarm.
        /// The method also sets up the layout and text views, and configures the MediaPlayer instance.
        /// If the name corresponds to a key in DataService.NameToKey, the method sets the alarm label and time label, and checks the user's preferences to determine whether to play the alarm, vibrate the device, or show a notification.
        /// If the name does not correspond to a key in DataService.NameToKey, the activity is finished.
        /// </summary>
        /// <param name="savedInstanceState"></param>
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            SetContentView(Resource.Layout.AlarmLayout);

            //get the current intent
            var intent = this.Intent;
            var name = intent?.GetStringExtra("name") ?? string.Empty;
            var time = intent?.GetStringExtra("time") ?? string.Empty;
            Debug.WriteLine($"Alarm triggered at {DateTime.Now} for {name} and {time}");
            Log.Info("AlarmActivity", $"Alarm triggered at {DateTime.Now} for {name} and {time}");
            FindViewById<Button>(Resource.Id.stopButton)?.SetOnClickListener(this);
            var label = FindViewById<TextView>(Resource.Id.textView);
            var timeLabel = FindViewById<TextView>(Resource.Id.textViewTime);
            FindViewById<Button>(Resource.Id.stopButton)?.SetText(AppResources.Kapat, TextView.BufferType.Normal);
            var layout = FindViewById<LinearLayout>(Resource.Id.linearLayout);
            var lightColor = Color.ParseColor("#EFEBE9");
            var darkColor = Color.ParseColor("#121212");
            layout?.SetBackgroundColor(Models.Theme.Tema == 1 ? lightColor : darkColor);
            label?.SetTextColor(Models.Theme.Tema == 1 ? darkColor : lightColor);
            timeLabel?.SetTextColor(Models.Theme.Tema == 1 ? darkColor : lightColor);
            _player ??= new MediaPlayer();
            if (DataService.KeyToName.TryGetValue(name,out string value))
            {
                var labelName = AppResources.ResourceManager.GetString(value, CultureInfo.CurrentCulture) + " " + AppResources.Alarmi;
                var timeLabelName = $"{AppResources.ResourceManager.GetString(value, CultureInfo.CurrentCulture)} {AppResources.Vakti} {time}";
                label?.SetText(labelName, TextView.BufferType.Normal);
                timeLabel?.SetText(timeLabelName, TextView.BufferType.Normal);

                if (Preferences.Get($"{name}Etkin", false) && Preferences.Get($"{name}Alarm", true))
                    PlayAlarm(name);
                if (Preferences.Get($"{name}Etkin", false) && Preferences.Get($"{name}Titreme", true))
                    Vibrate();
                if (Preferences.Get($"{name}Etkin", false) && Preferences.Get($"{name}Bildiri", false))
                    ShowNotification(name, time);
            }
            else
            {
                Finish();
            }
        }
        
        // This method plays the alarm sound based on the given name.
        /// <summary>
        /// The PlayAlarm method is responsible for playing the alarm sound.
        /// It takes a name parameter, which is used to retrieve a key from the DataService.NameToKey dictionary.
        /// This key is then used to get the user's preferred alarm sound from the application preferences. If the preferred sound is not found, a default sound is used.
        /// The method then maps the sound name to a resource ID, creates a URI for the sound resource, and sets up the MediaPlayer to play the sound.
        /// If the MediaPlayer is already initialized, it is reset before the new sound is set.
        /// The sound is set to loop continuously. If an exception occurs during this process, it is caught and logged.
        /// </summary>
        /// <param name="name"></param>
        private void PlayAlarm(string name)
        {
            try
            {
                var alarmSesi = Preferences.Get(name + "AlarmSesi", DefaultAlarmSesi);
                var soundResources = new Dictionary<string, int>
                {
                    { "kus", Resource.Raw.kus },
                    { "horoz", Resource.Raw.horoz },
                    { "ezan", Resource.Raw.ezan },
                    { "alarm", Resource.Raw.alarm },
                    { "alarm2", Resource.Raw.alarm2 },
                    { "beep1", Resource.Raw.beep1 },
                    { "beep2", Resource.Raw.beep2 },
                    { "beep3", Resource.Raw.beep3 }
                };

                var soundId = soundResources.ContainsKey(alarmSesi) ? soundResources[alarmSesi] : Resource.Raw.alarm;
                var context = Application.Context;
                var uri = Uri.Parse($"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{soundId}");

                if (_player == null)
                {
                    _player = new MediaPlayer();
                }
                else
                {
                    _player.Reset();
                }

                var attr = new AudioAttributes.Builder().SetContentType(AudioContentType.Sonification)
                    ?.SetUsage(AudioUsageKind.Alarm)
                    ?.Build();
                _player.SetDataSource(context, uri);
                _player.Prepare();
                _player.Looping = true;
                _player.SetAudioAttributes(attr);
                _player.Start();
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"{AppResources.AlarmHatasi}:\n{exception.Message}");
                Log.Error("AlarmActivity-PlayAlarm", $"{AppResources.AlarmHatasi}:\n{exception.Message}");
            }
        }

        // This method triggers the device's vibration feature.
        private static void Vibrate()
        {
            try
            {
                // Use default vibration length
                Vibration.Vibrate();

                // Or use specified time
                var duration = TimeSpan.FromSeconds(10);
                Vibration.Vibrate(duration);
            }
            catch (FeatureNotSupportedException ex)
            {
                Debug.WriteLine(AppResources.CihazTitretmeyiDesteklemiyor + ex.Message);
                Log.Error("AlarmActivity-Vibrate", AppResources.CihazTitretmeyiDesteklemiyor + ex.Message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("AlarmActivity-Vibrate" + ex.Message);
                Log.Error("AlarmActivity-Vibrate", ex.Message);
            }
        }

        // This method shows a notification with the given name and time.
        private void ShowNotification(string name, string time)
        {
            AlarmReceiver notificationHelper = new AlarmReceiver();
            var intent = new Intent(Application.Context, typeof(AlarmReceiver));
            intent.PutExtra("name", name);
            intent.PutExtra("time", time);
            intent.AddFlags(ActivityFlags.IncludeStoppedPackages);
            intent.AddFlags(ActivityFlags.ReceiverForeground);
            notificationHelper.OnReceive(ApplicationContext, intent);
            StopAlarm();
        }

        // This method is called when the window has been attached to the window manager. It sets up window flags.
        public override void OnAttachedToWindow()
        {
            Window?.AddFlags(WindowManagerFlags.ShowWhenLocked |
                             WindowManagerFlags.KeepScreenOn |
                             WindowManagerFlags.DismissKeyguard |
                             WindowManagerFlags.TurnScreenOn);
        }

        // This method is called when a view has been clicked. It stops the alarm.
        public void OnClick(View v)
        {
            StopAlarm();
        }

        // This method stops the alarm sound and checks for remaining reminders.
        private void StopAlarm()
        {
            if (_player != null && _player.IsPlaying)
            {
                _player.Stop();
                _player.Reset();
            }

            CheckRemainingReminders();
            Finish();
        }

        // This method checks if there are any remaining reminders. If there are less than 5 scheduled days remaining, it opens the main window to reschedule the weekly alarm.
        private void CheckRemainingReminders()
        {
            //Check if less than 5 scheduled days remained (at day 10 t0 15), open the main window to reschedule weekly alaram.
            var lastAlarmDateStr = Preferences.Get("LastAlarmDate", "Empty");
            if (lastAlarmDateStr != "Empty")
            {
                if ((DateTime.Parse(lastAlarmDateStr) - DateTime.Today).Days > 5)
                {
                    var notificationIntent = new Intent(this, typeof(MainActivity));
                    notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
                    notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
                    var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                        ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                        : PendingIntentFlags.UpdateCurrent;
                    var pendingIntent = PendingIntent.GetActivity(this, 0, notificationIntent, pendingIntentFlags);
                    pendingIntent?.Send();
                }
            }
        }

        // This method is called after OnPause(), when the activity is being resumed. It sets up a task to stop the alarm after a certain duration.
        protected override void OnResume()
        {
            base.OnResume();
            var minute = Preferences.Get("AlarmDuration", 4);
            Task.Run(async () =>
            {
                await Task.Delay(minute * MinuteInMilliseconds).ConfigureAwait(false);
                StopAlarm();
                return false;
            });
        }
    }
}