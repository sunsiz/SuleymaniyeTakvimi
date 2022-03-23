using System;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using Debug = System.Diagnostics.Debug;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new[] { Intent.ActionBootCompleted, Intent.ActionLockedBootCompleted, Intent.ActionTimeChanged, Intent.ActionTimezoneChanged }, Priority = (int)IntentFilterPriority.HighPriority)]
    public class BootBroadcast : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Analytics.TrackEvent("OnReceive in the BootBroadcast");
            try
            {
                Toast.MakeText(context, "Süleymaniye Vakfı Takvimi Başlatılıyor! " + intent.Action, ToastLength.Short)?.Show();
                //PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
                //PowerManager.WakeLock wakeLock = pm?.NewWakeLock(WakeLockFlags.Partial, "BootBroadcast");
                //wakeLock?.Acquire();

                // Run your code here
                //MainActivity.SetAlarmForBackgroundServices(context);
                MainActivity main = MainActivity.Instance;
                main.StopAlarmForegroundService();
                main.SetAlarmForegroundService();

                //Toast.MakeText(context, "Süleymaniye Vakfı Takvimi Başladı! " + intent.Action, ToastLength.Long)?.Show();
                //wakeLock?.Release();
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                Analytics.TrackEvent("OnReceive in the BootBroadcast Exception: " + exception);
            }
        }
    }
}

