using System;
using Android.App;
using Android.Content;
using Android.OS;
//using Microsoft.AppCenter.Analytics;
using Xamarin.Essentials;
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
                if (Preferences.Get("ForegroundServiceEnabled", true))
                {
                    var serviceIntent = new Intent(context, typeof(AlarmForegroundService));
                    serviceIntent.AddFlags(ActivityFlags.NewTask);
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                    {
                        context.StartForegroundService(serviceIntent);
                    }
                    else
                    {
                        context.StartService(serviceIntent);
                    }
                }
            }
            catch (Exception exception)
            {
                Debug.WriteLine($"**** BootBroadcast OnReceive Exception: {exception}");
                //Analytics.TrackEvent("OnReceive in the BootBroadcast Exception: " + exception + "\n Triggered: " + $" at {DateTime.Now}");
            }
        }
    }
}

