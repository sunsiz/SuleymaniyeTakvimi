using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Microsoft.AppCenter.Analytics;
using SuleymaniyeTakvimi.Droid;

namespace SuleymaniyeTakvimi.Droid
{
	[BroadcastReceiver]
	[IntentFilter(new[] { Intent.ActionBootCompleted })]
	public class BootBroadcast : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
        {
            Analytics.TrackEvent("OnReceive in the BootBroadcast");
            Toast.MakeText(context, "Süleymaniye Vakfı Takvimi Başlatılıyor! " + intent.Action, ToastLength.Long)?.Show();
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            PowerManager.WakeLock wakeLock = pm?.NewWakeLock(WakeLockFlags.Partial, "BootBroadcast");
            wakeLock?.Acquire();

            // Run your code here
            //MainActivity.SetAlarmForBackgroundServices(context);
            MainActivity main = MainActivity.instance;
            main.SetAlarmForegroundService();

            wakeLock?.Release();
		}
	}
}

