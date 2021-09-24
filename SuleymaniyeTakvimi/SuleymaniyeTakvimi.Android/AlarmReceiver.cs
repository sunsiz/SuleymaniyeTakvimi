using Android.Content;
using Android.OS;

namespace PeriodicBackgroundService.Android
{
	[BroadcastReceiver]
	class AlarmReceiver : BroadcastReceiver
	{
		public override void OnReceive(Context context, Intent intent)
		{
			var backgroundServiceIntent = new Intent(context, typeof(PeriodicBackgroundService));
            //if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            //{
            //    context.StartForegroundService(backgroundServiceIntent);
            //}
            //else
            //{
                context.StartService(backgroundServiceIntent);
            //}
        }
	}
}

