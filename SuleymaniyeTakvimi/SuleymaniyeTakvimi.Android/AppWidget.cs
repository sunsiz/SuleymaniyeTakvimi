using Android.App;
using Android.Appwidget;
using Android.Content;

namespace SuleymaniyeTakvimi.Droid
{
    [BroadcastReceiver(Label = "Süleymaniye Takvimi", Exported = false)]
    [IntentFilter(new string[] { "android.appwidget.action.APPWIDGET_UPDATE" })]
    // The "Resource" file has to be all in lower caps, "android.appwidget.action.APPWIDGET_UPDATE_OPTIONS"
    [MetaData("android.appwidget.provider", Resource = "@xml/appwidgetprovider")]
    public class AppWidget : AppWidgetProvider
    {
        //private static string LogoClick = "LogoClickTag";
        //private static string ACTION_REFRESH_CLICK = "REFRESH_CLICK";
        //private static string ACTION_UPDATE_CLICK = "action.UPDATE_CLICK";
        //private static string ACTION_RESIZE_MOVE = "android.appwidget.action.APPWIDGET_UPDATE_OPTIONS";
        //private static string ACTION_UPDATE = "android.appwidget.action.APPWIDGET_UPDATE";

        /// <summary>
        /// This method is called when the 'updatePeriodMillis' from the AppwidgetProvider passes,
        /// or the user manually refreshes/resizes.
        /// </summary>
        public override void OnUpdate(Context context, AppWidgetManager appWidgetManager, int[] appWidgetIds)
        {
            // To prevent any ANR timeouts, we perform the update in a service
            context.StartService(new Intent(context, typeof(WidgetService)));
            //var me = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            //appWidgetManager.UpdateAppWidget(me, BuildRemoteViews(context, appWidgetIds));
        }

        public override void OnReceive(Context context, Intent intent)
        {
            base.OnReceive(context, intent);
            // To prevent any ANR timeouts, we perform the update in a service
            context.StartService(new Intent(context, typeof(WidgetService)));
        }

    //private RemoteViews BuildRemoteViews(Context context, int[] appWidgetIds)
        //{
        //    // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
        //    var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);

        //    SetTextViewText(widgetView);
        //    RegisterClicks(context, appWidgetIds, widgetView);

        //    return widgetView;
        //}

        //private void SetTextViewText(RemoteViews widgetView)
        //{
        //    var data = new DataService();
        //    var takvim = data._takvim;
        //    if (takvim == null || takvim.FecriKazip == "") {widgetView.SetTextViewText(Resource.Id.widgetFecriKazipVakit, "Can not get takvim info"); return; }
        //    widgetView.SetTextViewText(Resource.Id.widgetFecriKazipVakit, takvim.FecriKazip);
        //    widgetView.SetTextViewText(Resource.Id.widgetFecriSadikVakit, takvim.FecriSadik);
        //    widgetView.SetTextViewText(Resource.Id.widgetSabahSonuVakit, takvim.SabahSonu);
        //    widgetView.SetTextViewText(Resource.Id.widgetOgleVakti, takvim.Ogle);
        //    widgetView.SetTextViewText(Resource.Id.widgetIkindiVakit, takvim.Ikindi);
        //    widgetView.SetTextViewText(Resource.Id.widgetAksamVakit, takvim.Aksam);
        //    widgetView.SetTextViewText(Resource.Id.widgetYatsiVakit, takvim.Yatsi);
        //    widgetView.SetTextViewText(Resource.Id.widgetYatsiSonuVakit, takvim.YatsiSonu);
        //    widgetView.SetTextViewText(Resource.Id.widgetLastRefreshed, DateTime.Now.ToString("T"));
        //    //widgetView.SetTextViewText(Resource.Id.widgetSmall, string.Format("Last update: {0:H:mm:ss}", DateTime.Now));
        //}

        //private void RegisterClicks(Context context, int[] appWidgetIds, RemoteViews widgetView)
        //{
        //    var intent = new Intent(context, typeof(AppWidget));
        //    intent.SetAction(AppWidgetManager.ActionAppwidgetUpdate);
        //    intent.PutExtra(AppWidgetManager.ExtraAppwidgetIds, appWidgetIds);

        //    // Register click event for the Background
        //    var piBackground = PendingIntent.GetBroadcast(context, 0, intent, PendingIntentFlags.UpdateCurrent);
        //    widgetView.SetOnClickPendingIntent(Resource.Id.widgetBackground, piBackground);

        //    // Register click event for the Logo-icon
        //    widgetView.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, ACTION_REFRESH_CLICK));
        //}

        //private PendingIntent GetPendingSelfIntent(Context context, string action)
        //{
        //    var intent = new Intent(context, typeof(AppWidget));
        //    intent.SetAction(action);
        //    return PendingIntent.GetBroadcast(context, 0, intent, 0);
        //}

        ///// <summary>
        ///// This method is called when clicks are registered.
        ///// </summary>
        //public override void OnReceive(Context context, Intent intent)
        //{
        //    base.OnReceive(context, intent);

        //    // Check if the click is from the "Refresh" button or resizing the widget || ACTION_RESIZE_MOVE.Equals(intent.Action) || ACTION_UPDATE.Equals(intent.Action)
        //    if (ACTION_REFRESH_CLICK.Equals(intent.Action))
        //    {
        //        // Retrieve the widget layout. This is a RemoteViews, so we can't use 'FindViewById'
        //        var widgetView = new RemoteViews(context.PackageName, Resource.Layout.Widget);
        //        SetTextViewText(widgetView);
        //        UserDialogs.Instance.Toast("Namaz vakitleri yenilandi", TimeSpan.FromSeconds(3));
        //        ComponentName componentName = new ComponentName(context, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
        //        var appWidgetManager = AppWidgetManager.GetInstance(context);
        //        if (appWidgetManager != null) appWidgetManager.UpdateAppWidget(componentName, widgetView);
        //        Toast.MakeText(context, "Namaz vakitleri yenilandi", ToastLength.Short).Show();
        //        //widgetView.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, ACTION_UPDATE_CLICK));
        //        //var pm = context.PackageManager;
        //        //try
        //        //{
        //        //    var packageName = "com.suleymaniyetakvimi";
        //        //    var launchIntent = pm.GetLaunchIntentForPackage(packageName);
        //        //    context.StartActivity(launchIntent);
        //        //}
        //        //catch
        //        //{
        //        //    // Something went wrong :)
        //        //}
        //    }
        //}
    }
}