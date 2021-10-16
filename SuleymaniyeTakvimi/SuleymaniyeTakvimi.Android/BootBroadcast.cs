using Android.App;
using Android.Content;
using Android.OS;
using SuleymaniyeTakvimi.Droid;

namespace PeriodicBackgroundService.Android
{
	[BroadcastReceiver]
	[IntentFilter(new[] { Intent.ActionBootCompleted })]
	public class BootBroadcast : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
            PowerManager pm = (PowerManager)context.GetSystemService(Context.PowerService);
            PowerManager.WakeLock wakeLock = pm.NewWakeLock(WakeLockFlags.Partial, "BootBroadcast");
            wakeLock.Acquire();

            // Run your code here
            //MainActivity.SetAlarmForBackgroundServices(context);
            MainActivity main = MainActivity.instance;
            main.SetForegroundService();

            wakeLock.Release();
		}
	}
}

