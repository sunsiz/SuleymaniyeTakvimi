using System;
using System.Globalization;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
using Java.Util;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.CommunityToolkit.Helpers;
//using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    [Service]
    public class WidgetService : IntentService
    {
        public override void OnStart (Intent intent, int startId)
        {
            // Build the widget update for today
            RemoteViews updateViews = buildUpdate (this);

            // Push update for this widget to the home screen
            ComponentName thisWidget = new ComponentName (this, Java.Lang.Class.FromType (typeof (AppWidget)).Name);
            AppWidgetManager manager = AppWidgetManager.GetInstance (this);
            manager.UpdateAppWidget (thisWidget, updateViews);
        }
        public override IBinder OnBind(Intent intent)
        {
            // We don't need to bind to this service
            return null;
        }

        protected override void OnHandleIntent(Intent intent)
        {
            // Build the widget update for today
            RemoteViews updateViews = buildUpdate (this);

            // Push update for this widget to the home screen
            ComponentName thisWidget = new ComponentName (this, Java.Lang.Class.FromType (typeof (AppWidget)).Name);
            AppWidgetManager manager = AppWidgetManager.GetInstance (this);
            manager.UpdateAppWidget (thisWidget, updateViews);
        }

        // Build a widget update to show the current Wiktionary
        // "Word of the day." Will block until the online API returns.
        public RemoteViews buildUpdate (Context context)
        {
            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(Localization.AppResources.ResourceManager);
            var language = Xamarin.Essentials.Preferences.Get("SelectedLanguage", "tr");
            //get default local for first initialization
            //String defaultLanguage = context.Resources.Configuration.Locale.Language;
            Configuration configuration = new Configuration();
            //check language preference everytime onCreate of all activities, if there is no choise set default language
            Locale newLocaleLanguage = new Locale(language);
            //finally setdefault language/locale according to newLocaleLanguage.
            Locale.SetDefault(Locale.Category.Display, newLocaleLanguage);
            configuration.Locale = newLocaleLanguage;
            context.Resources?.UpdateConfiguration(configuration, context.Resources.DisplayMetrics);
            LocalizationResourceManager.Current.CurrentCulture = new CultureInfo(language);
            var data = new DataService();
            var takvim = data._takvim;

            // Build an update that holds the updated widget contents
            var updateViews = new RemoteViews (context.PackageName, Resource.Layout.Widget);
            updateViews.SetTextViewText(Resource.Id.widgetAppName, GetString(Resource.String.app_name));
            updateViews.SetTextViewText(Resource.Id.widgetFecriKazip, GetString(Resource.String.fecri_kazip));
            updateViews.SetTextViewText(Resource.Id.widgetFecriSadik, GetString(Resource.String.fecri_sadik));
            updateViews.SetTextViewText(Resource.Id.widgetSabahSonu, GetString(Resource.String.sabah_sonu));
            updateViews.SetTextViewText(Resource.Id.widgetOgle, GetString(Resource.String.ogle));
            updateViews.SetTextViewText(Resource.Id.widgetIkindi, GetString(Resource.String.ikindi));
            updateViews.SetTextViewText(Resource.Id.widgetAksam, GetString(Resource.String.aksam));
            updateViews.SetTextViewText(Resource.Id.widgetYatsi, GetString(Resource.String.yatsi));
            updateViews.SetTextViewText(Resource.Id.widgetYatsiSonu, GetString(Resource.String.yatsi_sonu));
            updateViews.SetTextViewText(Resource.Id.widgetFecriKazipVakit, takvim.FecriKazip);
            updateViews.SetTextViewText(Resource.Id.widgetFecriSadikVakit, takvim.FecriSadik);
            updateViews.SetTextViewText(Resource.Id.widgetSabahSonuVakit, takvim.SabahSonu);
            updateViews.SetTextViewText(Resource.Id.widgetOgleVakti, takvim.Ogle);
            updateViews.SetTextViewText(Resource.Id.widgetIkindiVakit, takvim.Ikindi);
            updateViews.SetTextViewText(Resource.Id.widgetAksamVakit, takvim.Aksam);
            updateViews.SetTextViewText(Resource.Id.widgetYatsiVakit, takvim.Yatsi);
            updateViews.SetTextViewText(Resource.Id.widgetYatsiSonuVakit, takvim.YatsiSonu);
            updateViews.SetTextViewText(Resource.Id.widgetLastRefreshed, DateTime.Now.ToString("HH:mm:ss"));
            //updateViews.SetTextViewText(Resource.Id.widgetAppName, AppResources.SuleymaniyeVakfiTakvimi);
            //updateViews.SetTextViewText(Resource.Id.widgetFecriKazip, AppResources.FecriKazip);
            //updateViews.SetTextViewText(Resource.Id.widgetFecriSadik, AppResources.FecriSadik);
            //updateViews.SetTextViewText(Resource.Id.widgetSabahSonu, AppResources.SabahSonu);
            //updateViews.SetTextViewText(Resource.Id.widgetOgle, AppResources.Ogle);
            //updateViews.SetTextViewText(Resource.Id.widgetIkindi, AppResources.Ikindi);
            //updateViews.SetTextViewText(Resource.Id.widgetAksam, AppResources.Aksam);
            //updateViews.SetTextViewText(Resource.Id.widgetYatsi, AppResources.Yatsi);
            //updateViews.SetTextViewText(Resource.Id.widgetYatsiSonu, AppResources.YatsiSonu);
            //updateViews.SetTextViewText(Resource.Id.widgetFecriKazipVakit, takvim.FecriKazip);
            //updateViews.SetTextViewText(Resource.Id.widgetFecriSadikVakit, takvim.FecriSadik);
            //updateViews.SetTextViewText(Resource.Id.widgetSabahSonuVakit, takvim.SabahSonu);
            //updateViews.SetTextViewText(Resource.Id.widgetOgleVakti, takvim.Ogle);
            //updateViews.SetTextViewText(Resource.Id.widgetIkindiVakit, takvim.Ikindi);
            //updateViews.SetTextViewText(Resource.Id.widgetAksamVakit, takvim.Aksam);
            //updateViews.SetTextViewText(Resource.Id.widgetYatsiVakit, takvim.Yatsi);
            //updateViews.SetTextViewText(Resource.Id.widgetYatsiSonuVakit, takvim.YatsiSonu);
            //updateViews.SetTextViewText(Resource.Id.widgetLastRefreshed, DateTime.Now.ToString("HH:mm:ss"));

            // When user clicks on widget, launch to Wiktionary definition page
            //if (!string.IsNullOrEmpty (entry.Link)) {
            //    Intent defineIntent = new Intent (Intent.ActionView, Android.Net.Uri.Parse (entry.Link));

            //    PendingIntent pendingIntent = PendingIntent.GetActivity (context, 0, defineIntent, 0);
            //    updateViews.SetOnClickPendingIntent (Resource.Id.widget, pendingIntent);
            //}
            updateViews.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, "android.appwidget.action.APPWIDGET_UPDATE"));
            Toast.MakeText(context, Resource.String.refreshed, ToastLength.Short)?.Show();
            return updateViews;
        }
        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(AppWidget));
            intent.SetAction(action);
            return PendingIntent.GetBroadcast(context, 0, intent, 0);
        }
    }
}