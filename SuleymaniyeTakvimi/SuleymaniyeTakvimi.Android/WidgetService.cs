using System;
using System.Globalization;
using Android.App;
using Android.Appwidget;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Widget;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.Essentials;
using Locale = Java.Util.Locale;

namespace SuleymaniyeTakvimi.Droid
{
    [Service]
    public class WidgetService : IntentService
    {
        public override void OnStart (Intent intent, int startId)
        {
            UpdateWidget();
        }
        public override IBinder OnBind(Intent intent)
        {
            // We don't need to bind to this service
            return null;
        }

        protected override void OnHandleIntent(Intent intent)
        {
            UpdateWidget();
        }

        private void UpdateWidget()
        {
            // Build the widget update for today
            var updateViews = BuildUpdate(this);

            // Push update for this widget to the home screen
            var thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            var manager = AppWidgetManager.GetInstance(this);
            manager?.UpdateAppWidget(thisWidget, updateViews);
        }

        // Build a widget update to show daily prayer times
        private RemoteViews BuildUpdate (Context context)
        {
            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(AppResources.ResourceManager);
            var language = Xamarin.Essentials.Preferences.Get("SelectedLanguage", "tr");
            //get default local for first initialization
            //String defaultLanguage = context.Resources.Configuration.Locale.Language;
            var configuration = new Configuration();
            //check language preference every time onCreate of all activities, if there is no choice set default language
            Locale newLocaleLanguage = new Locale(language);
            //finally set default language/locale according to newLocaleLanguage.
            if (Build.VERSION.SdkInt >= BuildVersionCodes.N)
            {
                Locale.SetDefault(Locale.Category.Display, newLocaleLanguage);
                configuration.SetLocale(newLocaleLanguage);
            }
            else
            {
                Locale.Default = newLocaleLanguage;
                configuration.Locale = newLocaleLanguage;
            }

            context.Resources?.UpdateConfiguration(configuration, context.Resources.DisplayMetrics);
            LocalizationResourceManager.Current.CurrentCulture = new CultureInfo(language);
            var data = new DataService();
            var takvim = data._takvim;
            var city = Preferences.Get("sehir", AppResources.Sehir);
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
            updateViews.SetTextViewText(Resource.Id.widgetSehirAdi, GetString(Resource.String.sehir));
            updateViews.SetTextViewText(Resource.Id.widgetFecriKazipVakit, takvim.FecriKazip);
            updateViews.SetTextViewText(Resource.Id.widgetFecriSadikVakit, takvim.FecriSadik);
            updateViews.SetTextViewText(Resource.Id.widgetSabahSonuVakit, takvim.SabahSonu);
            updateViews.SetTextViewText(Resource.Id.widgetOgleVakti, takvim.Ogle);
            updateViews.SetTextViewText(Resource.Id.widgetIkindiVakit, takvim.Ikindi);
            updateViews.SetTextViewText(Resource.Id.widgetAksamVakit, takvim.Aksam);
            updateViews.SetTextViewText(Resource.Id.widgetYatsiVakit, takvim.Yatsi);
            updateViews.SetTextViewText(Resource.Id.widgetYatsiSonuVakit, takvim.YatsiSonu);
            updateViews.SetTextViewText(Resource.Id.widgetSehir, city);
            updateViews.SetTextViewText(Resource.Id.widgetLastRefreshed, DateTime.Now.ToString("HH:mm:ss"));
            
            updateViews.SetOnClickPendingIntent(Resource.Id.widgetRefreshIcon, GetPendingSelfIntent(context, "android.appwidget.action.APPWIDGET_UPDATE"));
            //if (_clicked) Toast.MakeText(context, Resource.String.refreshed, ToastLength.Short)?.Show();
            return updateViews;
        }
        private PendingIntent GetPendingSelfIntent(Context context, string action)
        {
            var intent = new Intent(context, typeof(AppWidget));
            //intent.PutExtra("clicked", true);
            intent.SetAction(action);
            var pendingIntentFlags = (Build.VERSION.SdkInt > BuildVersionCodes.R)
                ? PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable
                : PendingIntentFlags.UpdateCurrent;
            //Debug.WriteLine($"**** intent extras: {intent.Extras}");
            return PendingIntent.GetBroadcast(context, 0, intent, pendingIntentFlags);
        }
    }
}