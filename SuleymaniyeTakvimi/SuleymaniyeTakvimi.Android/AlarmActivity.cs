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
using Acr.UserDialogs;
using Android.Media;
using Android.Provider;
using Android.Util;
using MediaManager;
using MediaManager.Library;
using Uri = Android.Net.Uri;
using Xamarin.Essentials;
using MediaManager.Playback;
using Microsoft.AppCenter.Analytics;
using Plugin.LocalNotifications;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "AlarmActivity", Theme = "@style/MyTheme.Alarm")]
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
            var name = intent.GetStringExtra("name");
            var time = TimeSpan.Parse(intent.GetStringExtra("time"));
            Log.Info("AlarmActivity", $"Alarm triggered at {DateTime.Now} for {name} and {time}");
            FindViewById<Button>(Resource.Id.stopButton)?.SetOnClickListener(this);
            var label = FindViewById<TextView>(Resource.Id.textView1);
            var timeLabel = FindViewById<TextView>(Resource.Id.textViewTime);
            //Android.Net.Uri uri = (Android.Net.Uri)intent.GetStringExtra("fileName");
            //uri = uri == null || Uri.Empty.Equals(uri) ? Settings.System.DefaultRingtoneUri : uri;
            switch (name)
            {
                case "Fecri Kazip":
                    label.SetText("Fecri Kazip Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Fecri Kazip Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("fecrikazipAlarm", false)) PlayAlarm(name, "Fecri Kazip Alarmı");
                    if (Preferences.Get("fecrikazipTitreme", false)) Vibrate();
                    if (Preferences.Get("fecrikazipBildiri", false)) ShowNotification("Fecri Kazip");
                    break;
                case "Fecri Sadık":
                    label.SetText("Fecri Sadık Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Fecri Sadık Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("fecrisadikAlarm", false)) PlayAlarm(name, "Fecri Sadık Alarmı");
                    if (Preferences.Get("fecrisadikTitreme", false)) Vibrate();
                    if (Preferences.Get("fecrisadikBildiri", false)) ShowNotification("Fecri Sadık");
                    break;
                case "Sabah Sonu":
                    label.SetText("Sabah Sonu Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Sabah Sonu Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("sabahsonuAlarm", false)) PlayAlarm(name, "Sabah Sonu Alarmı");
                    if (Preferences.Get("sabahsonuTitreme", false)) Vibrate();
                    if (Preferences.Get("sabahsonuBildiri", false)) ShowNotification("Sabah Sonu");
                    break;
                case "Öğle":
                    label.SetText("Öğle Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Öğle Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("ogleAlarm", false)) PlayAlarm(name, "Öğle Alarmı");
                    if (Preferences.Get("ogleTitreme", false)) Vibrate();
                    if (Preferences.Get("ogleBildiri", false)) ShowNotification("Öğle");
                    break;
                case "İkindi":
                    label.SetText("İkindi Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"İkindi Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("ikindiAlarm", false)) PlayAlarm(name, "İkindi Alarmı");
                    if (Preferences.Get("ikindiTitreme", false)) Vibrate();
                    if (Preferences.Get("ikindiBildiri", false)) ShowNotification("İkindi");
                    break;
                case "Akşam":
                    label.SetText("Akşam Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Akşam Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("aksamAlarm", false)) PlayAlarm(name, "Akşam Alarmı");
                    if (Preferences.Get("aksamTitreme", false)) Vibrate();
                    if (Preferences.Get("aksamBildiri", false)) ShowNotification("Akşam");
                    break;
                case "Yatsı":
                    label.SetText("Yatsı Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Yatsı Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("yatsiAlarm", false)) PlayAlarm(name, "Yatsı Alarmı");
                    if (Preferences.Get("yatsiTitreme", false)) Vibrate();
                    if (Preferences.Get("yatsiBildiri", false)) ShowNotification("Yatsı");
                    break;
                case "Yatsı Sonu":
                    label.SetText("Yatsı Sonu Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"Yatsı Sonu Vakti: {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("yatsisonuAlarm", false)) PlayAlarm(name, "Yatsı Sonu Alarmı");
                    if (Preferences.Get("yatsisonuTitreme", false)) Vibrate();
                    if (Preferences.Get("yatsisonuBildiri", false)) ShowNotification("Yatsı Sonu");
                    break;
                default:
                    label.SetText("Test Alarmı", TextView.BufferType.Normal);
                    timeLabel.SetText($"şimdiki zaman: {time}", TextView.BufferType.Normal);
                    PlayAlarm(name, "Test Alarmı");
                    break;
            }
        }

        private static void PlayAlarm(string name, string title)
        {
            Analytics.TrackEvent("PlayAlarm in the AlarmActivity");
            //if (name == "test")
            //{
            //    MediaPlayer player = new MediaPlayer();
            //    player.SetDataSource("http://shaincast.caster.fm:22344/listen.mp3");
            //    player.Prepare();
            //    player.Start();
            //}
            //else
            //{
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
                CrossMediaManager.Current.MediaPlayer.Stop();
                IMediaItem mediaItem;
                var alarmSesi = Preferences.Get(key + "AlarmSesi", "kus");
                mediaItem = CrossMediaManager.Current.PlayFromAssembly(alarmSesi + ".wav").Result;
                //mediaItem.DisplayTitle = title;
                //CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                //CrossMediaManager.Current.Notification.ShowPlayPauseControls = true;
                //CrossMediaManager.Current.MediaPlayer.ShowPlaybackControls = false;
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                CrossMediaManager.Current.MediaPlayer.Play(mediaItem);
            }
            catch (Exception exception)
            {
                Log.Error("AlarmActivity-PlayAlarm",
                    $"Alarm çalarken bir sorun oluştu, detaylar:\n{exception.Message}");
                //UserDialogs.Instance.Alert($"Alarm çalarken bir sorun oluştu, detaylar:\n{exception.Message}",
                //    "Ses Oynatma Hatası");
            }
            //}
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
                Log.Error("AlarmActivity-Vibrate", "Cihazınız titretmeyi desteklemiyor. " + ex.Message);
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
            CrossLocalNotifications.Current.Show($"Süleymaniye Vakfı Takvimi",$"{name} Vakti Hatırlatması", 1994);
            CheckRemainingReminders();
        }

        public override void OnAttachedToWindow()
        {
            Window.AddFlags(WindowManagerFlags.ShowWhenLocked |
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
                    pendingIntent.Send();
                }
            }
        }
    }
}