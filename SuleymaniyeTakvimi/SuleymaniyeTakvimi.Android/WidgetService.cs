using System;
using System.ComponentModel;
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
        /// <summary>
        /// This method is called when the service is started. It updates the widget.
        /// </summary>
        /// <param name="intent">The Intent that was used to start the service, as given to Context.StartService(Intent).</param>
        /// <param name="startId">A unique integer representing this specific request to start. Use with StopSelfResult(int).</param>
        public override void OnStart(Intent intent, int startId)
        {
            LocalizationResourceManager.Current.PropertyChanged += UpdateCulture;
            LocalizationResourceManager.Current.Init(AppResources.ResourceManager);
            UpdateWidget();
        }

        private void UpdateCulture(object sender, PropertyChangedEventArgs e)
        {
            AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
        }

        /// <summary>
        /// This method is called when another component wants to bind with the service (such as to perform RPC).
        /// In this case, we don't need to bind to this service, so we return null.
        /// </summary>
        /// <param name="intent">The Intent that was used to bind to this service, as given to Context.BindService(Intent, IServiceConnection, Bind).</param>
        /// <returns>Returns the communication channel to the service, or null if clients cannot bind to the service.</returns>
        public override IBinder OnBind(Intent intent)
        {
            // We don't need to bind to this service
            return null;
        }

        /// <summary>
        /// This method is called when the service receives a start request. It is used to perform background operations.
        /// In this case, it updates the widget.
        /// </summary>
        /// <param name="intent">The value passed to Context.StartService(Intent). This may be null if the service is being restarted after its process has gone away.</param>
        protected override void OnHandleIntent(Intent intent)
        {
            UpdateWidget();
        }

        /// <summary>
        /// This method is responsible for updating the widget. It first builds the widget update for the current day
        /// and then pushes the update to the home screen.
        /// </summary>
        private void UpdateWidget()
        {
            // Build the widget update for today
            var updateViews = BuildUpdate(this);

            // Push update for this widget to the home screen
            var thisWidget = new ComponentName(this, Java.Lang.Class.FromType(typeof(AppWidget)).Name);
            var manager = AppWidgetManager.GetInstance(this);
            manager?.UpdateAppWidget(thisWidget, updateViews);
        }

        /// <summary>
        /// This method is responsible for building the widget update. It first sets the locale based on the user's selected language.
        /// Then it retrieves the prayer times data and the selected city from the user's preferences.
        /// After that, it creates a new RemoteViews object and sets the text of various TextViews in the widget layout.
        /// These TextViews display the app name, prayer times, city, and last refreshed time.
        /// Finally, it sets an onClickPendingIntent for the refresh icon in the widget, which triggers the widget to update when clicked.
        /// </summary>
        /// <param name="context">The Context in which this PendingIntent should perform the broadcast.</param>
        /// <returns>A RemoteViews object containing the updated widget layout.</returns>
        // Build a widget update to show daily prayer times
        private RemoteViews BuildUpdate (Context context)
        {
            //get default local for first initialization
            //String defaultLanguage = context.Resources.Configuration.Locale.Language;
            var language = Preferences.Get("SelectedLanguage", "tr");
            var configuration = new Configuration();
            //check language preference every time onCreate of all activities, if there is no choice set default language
            var newLocaleLanguage = new Locale(language);
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
            var takvim = data.Takvim;
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

        /// <summary>
        /// This method creates a PendingIntent that will perform a broadcast, to be used with widgets. 
        /// The broadcast will be an Intent to the AppWidget class with the specified action.
        /// The PendingIntent is flagged to update the current one, and if the Android version is greater than R, it's also flagged as immutable.
        /// </summary>
        /// <param name="context">The Context in which the PendingIntent should start the broadcast.</param>
        /// <param name="action">The action of the Intent.</param>
        /// <returns>A PendingIntent that can be used to perform a broadcast.</returns>
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