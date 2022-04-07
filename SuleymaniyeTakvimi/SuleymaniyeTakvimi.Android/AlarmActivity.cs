using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using Android.Util;
using MediaManager;
using Xamarin.Essentials;
using MediaManager.Playback;
using Microsoft.AppCenter.Analytics;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Localization;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "Süleymaniye Takvimi", Icon = "@mipmap/icon", Theme = "@style/MyTheme.Alarm", NoHistory = true)]
    public class AlarmActivity : Activity, View.IOnClickListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Analytics.TrackEvent("OnCreate in the AlarmActivity");
            // Create your application here
            SetContentView(Resource.Layout.AlarmLayout);
            //get the current intent
            Intent intent = this.Intent;
            var name = intent?.GetStringExtra("name");
            var time = TimeSpan.Parse(intent?.GetStringExtra("time") ?? string.Empty);
            Log.Info("AlarmActivity", $"Alarm triggered at {DateTime.Now} for {name} and {time}");
            FindViewById<Button>(Resource.Id.stopButton)?.SetOnClickListener(this);
            var label = FindViewById<TextView>(Resource.Id.textView);
            var timeLabel = FindViewById<TextView>(Resource.Id.textViewTime);
            FindViewById<Button>(Resource.Id.stopButton)?.SetText(AppResources.Kapat, TextView.BufferType.Normal);
            var layout = FindViewById<LinearLayout>(Resource.Id.linearLayout);
            var lightColor = (Xamarin.Forms.Color)Xamarin.Forms.Application.Current.Resources["AppBackgroundColor"];
            var darkColor = (Xamarin.Forms.Color) Xamarin.Forms.Application.Current.Resources["CardBackgroundColorDark"];
            layout?.SetBackgroundColor(Models.Theme.Tema == 1 ? Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(lightColor) : Xamarin.Forms.Platform.Android.ColorExtensions.ToAndroid(darkColor));
            //Android.Net.Uri uri = (Android.Net.Uri)intent.GetStringExtra("fileName");
            //uri = uri == null || Uri.Empty.Equals(uri) ? Settings.System.DefaultRingtoneUri : uri;
            switch (name)
            {
                case "Fecri Kazip":
                    label?.SetText(AppResources.FecriKazip + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.FecriKazip} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("fecrikazipAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("fecrikazipTitreme", false)) Vibrate();
                    if (Preferences.Get("fecrikazipBildiri", false)) ShowNotification(AppResources.FecriKazip);
                    break;
                case "Fecri Sadık":
                    label?.SetText(AppResources.FecriSadik + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.FecriSadik} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("fecrisadikAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("fecrisadikTitreme", false)) Vibrate();
                    if (Preferences.Get("fecrisadikBildiri", false)) ShowNotification(AppResources.FecriSadik);
                    break;
                case "Sabah Sonu":
                    label?.SetText(AppResources.SabahSonu + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.SabahSonu} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("sabahsonuAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("sabahsonuTitreme", false)) Vibrate();
                    if (Preferences.Get("sabahsonuBildiri", false)) ShowNotification(AppResources.SabahSonu);
                    break;
                case "Öğle":
                    label?.SetText(AppResources.Ogle + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Ogle} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("ogleAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("ogleTitreme", false)) Vibrate();
                    if (Preferences.Get("ogleBildiri", false)) ShowNotification(AppResources.Ogle);
                    break;
                case "İkindi":
                    label?.SetText(AppResources.Ikindi + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Ikindi} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("ikindiAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("ikindiTitreme", false)) Vibrate();
                    if (Preferences.Get("ikindiBildiri", false)) ShowNotification(AppResources.Ikindi);
                    break;
                case "Akşam":
                    label?.SetText(AppResources.Aksam + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Aksam} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("aksamAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("aksamTitreme", false)) Vibrate();
                    if (Preferences.Get("aksamBildiri", false)) ShowNotification(AppResources.Aksam);
                    break;
                case "Yatsı":
                    label?.SetText(AppResources.Yatsi + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Yatsi} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("yatsiAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("yatsiTitreme", false)) Vibrate();
                    if (Preferences.Get("yatsiBildiri", false)) ShowNotification(AppResources.Yatsi);
                    break;
                case "Yatsı Sonu":
                    label?.SetText(AppResources.YatsiSonu + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.YatsiSonu} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("yatsisonuAlarm", false)) PlayAlarm(name);
                    if (Preferences.Get("yatsisonuTitreme", false)) Vibrate();
                    if (Preferences.Get("yatsisonuBildiri", false)) ShowNotification(AppResources.YatsiSonu);
                    break;
                default:
                    label?.SetText("Test Alarmı", TextView.BufferType.Normal);
                    timeLabel?.SetText($"şimdiki zaman: {time}", TextView.BufferType.Normal);
                    PlayAlarm(name);
                    break;
            }
        }

        private static async void PlayAlarm(string name)
        {
            Analytics.TrackEvent("PlayAlarm in the AlarmActivity");
            
            var key = "";
            switch (name)
            {
                case "Fecri Kazip":
                    key =  "fecrikazip";
                    break;
                case "Fecri Sadık":
                    key = "fecrisadik";
                    break;
                case "Sabah Sonu":
                    key = "sabahsonu";
                    break;
                case "Öğle":
                    key = "ogle";
                    break;
                case "İkindi":
                    key = "ikindi";
                    break;
                case "Akşam":
                    key = "aksam";
                    break;
                case "Yatsı":
                    key = "yatsi";
                    break;
                case "Yatsı Sonu":
                    key = "yatsisonu";
                    break;
            }
            try
            {
                await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
                var alarmSesi = Preferences.Get(key + "AlarmSesi", "kus");
                var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(alarmSesi + ".wav").ConfigureAwait(true);
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                await CrossMediaManager.Current.MediaPlayer.Play(mediaItem).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                Log.Error("AlarmActivity-PlayAlarm",
                    $"{AppResources.AlarmHatasi}:\n{exception.Message}");

            }
        }

        private static void Vibrate()
        {
            Analytics.TrackEvent("Vibrate in the AlarmActivity");
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
                Log.Error("AlarmActivity-Vibrate", AppResources.CihazTitretmeyiDesteklemiyor + ex.Message);
                //UserDialogs.Instance.Alert("Cihazınız titretmeyi desteklemiyor. " + ex.Message,
                //    "Cihaz desteklemiyor");
            }
            catch (Exception ex)
            {
                Log.Error("AlarmActivity-Vibrate", ex.Message);
                //UserDialogs.Instance.Alert(ex.Message, "Bir sorunla karşılaştık");
            }
        }

        private void ShowNotification(string name)
        {
            CrossLocalNotifications.Current.Show($"{AppResources.SuleymaniyeVakfiTakvimi}",$"{name} {AppResources.VaktiHatirlatmasi}", 1994);
            CheckRemainingReminders();
        }

        public override void OnAttachedToWindow()
        {
            Window?.AddFlags(WindowManagerFlags.ShowWhenLocked |
                             WindowManagerFlags.KeepScreenOn |
                             WindowManagerFlags.DismissKeyguard |
                             WindowManagerFlags.TurnScreenOn);
        }

        public void OnClick(View v)
        {
            CrossMediaManager.Current.MediaPlayer.Stop();
            CheckRemainingReminders();
            Finish();
        }

        private void CheckRemainingReminders()
        {
            //Check if less than 2 days schedule remained, open the main window to reschedule weekly laram.
            var lastAlarmDateStr = Preferences.Get("LastAlarmDate", "Empty");
            if (lastAlarmDateStr != "Empty")
            {
                if ((DateTime.Parse(lastAlarmDateStr) - DateTime.Today).Days < 2)
                {
                    var notificationIntent = new Intent(this, typeof(MainActivity));
                    notificationIntent.SetAction("Alarm.action.MAIN_ACTIVITY");
                    notificationIntent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask);
                    var pendingIntent =
                        PendingIntent.GetActivity(this, 0, notificationIntent, PendingIntentFlags.UpdateCurrent);
                    pendingIntent?.Send();
                }
            }
        }
    }
}