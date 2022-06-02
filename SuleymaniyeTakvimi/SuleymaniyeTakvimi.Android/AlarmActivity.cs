using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using System;
using System.Threading.Tasks;
using Android.Graphics;
using Android.Media;
using Android.Util;
using Xamarin.Essentials;
using Microsoft.AppCenter.Analytics;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Localization;
using Uri = Android.Net.Uri;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Icon = "@mipmap/icon", Theme = "@style/MyTheme.Alarm", NoHistory = true)]
    public class AlarmActivity : Activity, View.IOnClickListener
    {
        private static MediaPlayer _player;
        //private static int loop;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Analytics.TrackEvent("OnCreate in the AlarmActivity");
            // Create your application here
            SetContentView(Resource.Layout.AlarmLayout);
            //get the current intent
            var intent = this.Intent;
            var name = intent?.GetStringExtra("name") ?? string.Empty;
            var time = intent?.GetStringExtra("time") ?? string.Empty;
            //var timeSpan = TimeSpan.Parse(time);
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
            //Android.Net.Uri uri = (Android.Net.Uri)intent.GetStringExtra("fileName");
            //uri = uri == null || Uri.Empty.Equals(uri) ? Settings.System.DefaultRingtoneUri : uri;
            _player ??= new MediaPlayer();
            switch (name)
            {
                case "Fecri Kazip":
                    label?.SetText(AppResources.FecriKazip + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.FecriKazip} {AppResources.Vakti} {time}",
                        TextView.BufferType.Normal);
                    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipAlarm", true))
                        PlayAlarm(name);
                    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipTitreme", true))
                        Vibrate();
                    if (Preferences.Get("fecrikazipEtkin", false) && Preferences.Get("fecrikazipBildiri", false))
                        ShowNotification(AppResources.FecriKazip);
                    break;
                case "Fecri Sadık":
                    label?.SetText(AppResources.FecriSadik + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.FecriSadik} {AppResources.Vakti} {time}",
                        TextView.BufferType.Normal);
                    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikAlarm", true))
                        PlayAlarm(name);
                    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikTitreme", true))
                        Vibrate();
                    if (Preferences.Get("fecrisadikEtkin", false) && Preferences.Get("fecrisadikBildiri", false))
                        ShowNotification(AppResources.FecriSadik);
                    break;
                case "Sabah Sonu":
                    label?.SetText(AppResources.SabahSonu + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.SabahSonu} {AppResources.Vakti} {time}",
                        TextView.BufferType.Normal);
                    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuAlarm", true))
                        PlayAlarm(name);
                    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuTitreme", true))
                        Vibrate();
                    if (Preferences.Get("sabahsonuEtkin", false) && Preferences.Get("sabahsonuBildiri", false))
                        ShowNotification(AppResources.SabahSonu);
                    break;
                case "Öğle":
                    label?.SetText(AppResources.Ogle + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Ogle} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleAlarm", true)) PlayAlarm(name);
                    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleTitreme", true)) Vibrate();
                    if (Preferences.Get("ogleEtkin", false) && Preferences.Get("ogleBildiri", false))
                        ShowNotification(AppResources.Ogle);
                    break;
                case "İkindi":
                    label?.SetText(AppResources.Ikindi + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Ikindi} {AppResources.Vakti} {time}",
                        TextView.BufferType.Normal);
                    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiAlarm", true)) PlayAlarm(name);
                    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiTitreme", true)) Vibrate();
                    if (Preferences.Get("ikindiEtkin", false) && Preferences.Get("ikindiBildiri", false))
                        ShowNotification(AppResources.Ikindi);
                    break;
                case "Akşam":
                    label?.SetText(AppResources.Aksam + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Aksam} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamAlarm", true)) PlayAlarm(name);
                    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamTitreme", true)) Vibrate();
                    if (Preferences.Get("aksamEtkin", false) && Preferences.Get("aksamBildiri", false))
                        ShowNotification(AppResources.Aksam);
                    break;
                case "Yatsı":
                    label?.SetText(AppResources.Yatsi + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.Yatsi} {AppResources.Vakti} {time}", TextView.BufferType.Normal);
                    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiAlarm", true)) PlayAlarm(name);
                    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiTitreme", true)) Vibrate();
                    if (Preferences.Get("yatsiEtkin", false) && Preferences.Get("yatsiBildiri", false))
                        ShowNotification(AppResources.Yatsi);
                    break;
                case "Yatsı Sonu":
                    label?.SetText(AppResources.YatsiSonu + " " + AppResources.Alarmi, TextView.BufferType.Normal);
                    timeLabel?.SetText($"{AppResources.YatsiSonu} {AppResources.Vakti} {time}",
                        TextView.BufferType.Normal);
                    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuAlarm", true))
                        PlayAlarm(name);
                    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuTitreme", true))
                        Vibrate();
                    if (Preferences.Get("yatsisonuEtkin", false) && Preferences.Get("yatsisonuBildiri", false))
                        ShowNotification(AppResources.YatsiSonu);
                    break;
                default:
                    label?.SetText("Test Alarmı", TextView.BufferType.Normal);
                    timeLabel?.SetText($"şimdiki zaman: {time}", TextView.BufferType.Normal);
                    PlayAlarm(name);
                    break;
            }
        }

        private void PlayAlarm(string name)
        {
            Analytics.TrackEvent("PlayAlarm in the AlarmActivity");

            var key = "";
            switch (name)
            {
                case "Fecri Kazip":
                    key = "fecrikazip";
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
                //await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
                //var looper = Preferences.Get(key + "AlarmTekrarlari", 7);
                var alarmSesi = Preferences.Get(key + "AlarmSesi", "ezan");
                //var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(alarmSesi + ".mp3").ConfigureAwait(true);
                //CrossMediaManager.Current.Notification.Enabled = false;
                //CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                //await CrossMediaManager.Current.MediaPlayer.Play(mediaItem).ConfigureAwait(false);
                Uri uri;
                var context = Application.Context;
                switch (alarmSesi)
                {
                    case "kus":
                        uri = Uri.Parse(
                            $"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.kus}");
                        break;
                    case "horoz":
                        uri = Uri.Parse(
                            $"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.horoz}");
                        break;
                    case "ezan":
                        uri = Uri.Parse(
                            $"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.ezan}");
                        break;
                    case "alarm":
                        uri = Uri.Parse(
                            $"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm}");
                        break;
                    default:
                        uri = Uri.Parse(
                            $"{ContentResolver.SchemeAndroidResource}://{context.PackageName}/{Resource.Raw.alarm}");
                        break;
                }

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
                //player.Completion += delegate (object o, EventArgs args) { Player_Completion(o, args, looper); };
            }
            catch (Exception exception)
            {
                Log.Error("AlarmActivity-PlayAlarm", $"{AppResources.AlarmHatasi}:\n{exception.Message}");
            }
        }

        // Counts the loop count and stop when finished.
        //private void Player_Completion(object sender, EventArgs e, int looper)
        //{
        //    if (looper <= 0) return;
        //    if (loop < looper) loop++;
        //    if (loop == looper)
        //    {
        //        loop = 0;
        //        if (player != null && player.IsPlaying)
        //        {
        //            player.Stop();
        //            player.Reset();
        //        }
        //        CheckRemainingReminders();
        //        Finish();
        //    }
        //}

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
            //CrossMediaManager.Current.MediaPlayer.Stop();
            if (_player != null && _player.IsPlaying)
            {
                _player.Stop();
                _player.Reset();
            }
            CheckRemainingReminders();
            Finish();
        }

        private void CheckRemainingReminders()
        {
            //Check if less than 2 days schedule remained, open the main window to reschedule weekly alaram.
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

        protected override void OnResume()
        {
            base.OnResume();
            var minute = Preferences.Get("AlarmDuration", 4);
            Task.Run(async () =>
            {
                await Task.Delay(minute*60000).ConfigureAwait(false);
                //await CrossMediaManager.Current.MediaPlayer.Stop();
                if (_player != null && _player.IsPlaying)
                {
                    _player.Stop();
                    _player.Reset();
                }
                CheckRemainingReminders();
                Finish();
                return false;
            });
            //Xamarin.Forms.Device.StartTimer(TimeSpan.FromMinutes(minute), () =>
            //{
            //    // Do something
            //    CrossMediaManager.Current.MediaPlayer.Stop();
            //    CheckRemainingReminders();
            //    Finish();
            //    return false; // True = Repeat again, False = Stop the timer
            //});
        }
    }
}