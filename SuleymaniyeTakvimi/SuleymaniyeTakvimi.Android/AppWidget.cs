using System;
using Acr.UserDialogs;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Widget;
using SuleymaniyeTakvimi.Services;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Label = "Süleymaniye Takvimi")]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    // The "Resource" file has to be all in lower caps
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        //private static string LogoClick = "LogoClickTag";
        private static string RefreshClick = "RefreshClickTag";
        /// <summary>
		/// This method is called when the 'updatePeriodMillis' from the AppwidgetProvider passes,
		/// or the user manually refreshes/resizes.
		/// </summary>
		public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        {
            // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
            var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

            SetTextViewText(widgetView);
            RegisterClicks(context, appWidgetIds, widgetView);

            return widgetView;
        }

        private void SetTextViewText(RemoteViews widgetView)
        {
            var data = new DataService();
            var Takvim = data._takvim;
            widgetView.SetTextViewText(Resource.Id.widgetFecriKazipVakit, Takvim.FecriKazip);
            widgetView.SetTextViewText(Resource.Id.widgetFecriSadikVakit, Takvim.FecriSadik);
            widgetView.SetTextViewText(Resource.Id.widgetSabahSonuVakit, Takvim.SabahSonu);
            widgetView.SetTextViewText(Resource.Id.widgetOgleVakti, Takvim.Ogle);
            widgetView.SetTextViewText(Resource.Id.widgetIkindiVakit, Takvim.Ikindi);
            widgetView.SetTextViewText(Resource.Id.widgetAksamVakit, Takvim.Aksam);
            widgetView.SetTextViewText(Resource.Id.widgetYatsiVakit, Takvim.Yatsi);
            widgetView.SetTextViewText(Resource.Id.widgetYatsiSonuVakit, Takvim.YatsiSonu);
            widgetView.SetTextViewText(Resource.Id.widgetLastRefreshed, DateTime.Now.ToString("T"));
            //widgetView.SetTextViewText(Resource.Id.widgetSmall, string.Format("Last update: {0:H:mm:ss}", DateTime.Now));
        }

        private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        {
            //var intent = new Intent(context, typeof(AppWidget));
            //intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
            //intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

            // Register click event for the Background
            //var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
            //widgetView.SetOnClickPendingIntent(Resource.Id.widgetBackground, piBackground);

            // Register click event for the Logo-icon
            widgetView.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, RefreshClick));
        }

        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(AppWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }

        /// <summary>
        /// This method is called when clicks are registered.
        /// </summary>
        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);

            // Check if the click is from the "Announcement" button
            if (RefreshClick.Equals(intent.Action))
            {
                // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
                var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);
                SetTextViewText(widgetView);
                Toast.MakeText(context, "Namaz vakitleri yenilandi", ToastLength.Short);
                UserDialogs.Instance.Toast("Namaz vakitleri yenilandi", TimeSpan.FromSeconds(3));
                ComponentName componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
                AppWidgetManager.GetInstance(context)?.UpdateAppWidget(componentName, widgetView);
                widgetView.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, RefreshClick));
                //var pm = context.PackageManager;
                //try
                //{
                //    var packageName = "com.suleymaniyetakvimi";
                //    var launchIntent = pm.GetLaunchIntentForPackage(packageName);
                //    context.StartActivity(launchIntent);
                //}
                //catch
                //{
                //    // Something went wrong :)
                //}
            }
        }
    }
}