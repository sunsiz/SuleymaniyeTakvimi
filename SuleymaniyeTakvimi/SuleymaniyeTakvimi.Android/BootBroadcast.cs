using System;
using Android.App;
using Android.Content;
using Android.OS;
//using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Debug = System.Diagnostics.Debug;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class BootBroadcast : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            //Analytics.TrackEvent("OnReceive in the BootBroadcast Triggered: " + $" at {DateTime.Now}");
            try
            {
                ////Toast.MakeText(context, "Süleymaniye Vakfı Takvimi Başlatılıyor! " + intent.Action, ToastLength.Short)?.Show();
                ////PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
                ////PowerManager.WakeLock wakeLock = pm?.NewWakeLock(WakeLockFlags.Partial, "BootBroadcast");
                ////wakeLock?.Acquire();

                //// Run your code here
                ////MainActivity.SetAlarmForBackgroundServices(context);
                ////MainActivity main = MainActivity.Instance;
                ////main.StopAlarmForegroundService();
                ////main.StartAlarmForegroundService();
                //DependencyService.Get<IAlarmService>().StopAlarmForegroundService();
                //DependencyService.Get<IAlarmService>().StartAlarmForegroundService();
                ////Toast.MakeText(context, "Süleymaniye Vakfı Takvimi Başladı! " + intent.Action, ToastLength.Long)?.Show();
                ////wakeLock?.Release();
                if (Preferences.Get("ForegroundServiceEnabled", true))
                {
                    Intent i = new Intent(context, typeof(AlarmForegroundService));
                    i.AddFlags(ActivityFlags.NewTask);
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        context.StartForegroundService(i);
                    }
                    else
                    {
                        context.StartService(i);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                //Analytics.TrackEvent("OnReceive in the BootBroadcast Exception: " + exception + "\n Triggered: " + $" at {DateTime.Now}");
            }
        }
    }
}

