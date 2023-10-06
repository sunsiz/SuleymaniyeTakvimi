using System.Globalization;
using SuleymaniyeTakvimi.Services;
//using Microsoft.AppCenter;
//using Microsoft.AppCenter.Analytics;
//using Microsoft.AppCenter.Crashes;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Models;
using Xamarin.CommunityToolkit.Helpers;
//using Matcha.BackgroundService;
//using Plugin.LocalNotification;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Collections.Generic;

namespace SuleymaniyeTakvimi
{
	public partial class App : Application
    {
        //private bool reminderEnabled = false;
        const string LogTag = "SuleymaniyeTakvimi";
        
        //static App()
        //{
        //    Crashes.SendingErrorReport += SendingErrorReportHandler;
        //    Crashes.SentErrorReport += SentErrorReportHandler;
        //    Crashes.FailedToSendErrorReport += FailedToSendErrorReportHandler;
        //}
        public App()
        {
            LocalizationResourceManager.Current.PropertyChanged += (sender, e) => AppResources.Culture = LocalizationResourceManager.Current.CurrentCulture;
            LocalizationResourceManager.Current.Init(AppResources.ResourceManager);
            var language = Preferences.Get("SelectedLanguage", "zz");
            if(language=="zz"){
                switch (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName)
                {
                    case "ar":
                        language = "ar";
                        break;
                    case "az":
                        language = "az";
                        break;
                    case "de":
                        language = "de";
                        break;
                    case "en":
                        language = "en";
                        break;
                    case "fa":
                        language = "fa";
                        break;
                    case "fr":
                        language = "fr";
                        break;
                    case "ru":
                        language = "ru";
                        break;
                    case "tr":
                        language = "tr";
                        break;
                    case "ug":
                        language = "ug";
                        break;
                    case "zh":
                        language = "zh";
                        break;
                    default:
                        language = "en";
                        break;
                }
            }
            LocalizationResourceManager.Current.CurrentCulture = new CultureInfo(language);
            InitializeComponent();
            Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
            
            //Sharpnado.Shades.Initializer.Initialize(loggerEnable: false);
            DependencyService.Register<DataService>();
            Xamarin.Forms.Device.SetFlags(new List<string> { "Accessibility_Experimental" });
            MainPage = new AppShell();
        }

        protected override void OnStart()
        {
            //AppCenter.LogLevel = Microsoft.AppCenter.LogLevel.Verbose;
            //Crashes.ShouldProcessErrorReport = ShouldProcess;
            //Crashes.ShouldAwaitUserConfirmation = ConfirmationHandler;
            //AppCenter.Start("android=a40bd6f0-5ad7-4b36-9a89-740333948b82;" +
            //                "ios=f757b6ef-a959-4aac-9404-98dbbd2fb1bb;",
            //    typeof(Analytics), typeof(Crashes));
            //AppCenter.GetInstallIdAsync().ContinueWith(installId =>
            //{
            //    AppCenterLog.Info(LogTag, "AppCenter.InstallId=" + installId.Result);
            //});
            //Crashes.HasCrashedInLastSessionAsync().ContinueWith(hasCrashed =>
            //{
            //    AppCenterLog.Info(LogTag, "Crashes.HasCrashedInLastSession=" + hasCrashed.Result);
            //});
            //Crashes.GetLastSessionCrashReportAsync().ContinueWith(report =>
            //{
            //    AppCenterLog.Info(LogTag, "Crashes.LastSessionCrashReport.Exception=" + report.Result?.StackTrace);
            //});
            //SetReminderEnabled();
            VersionTracking.Track();
            OnResume();
        }

        protected override void OnSleep()
        {
            SetTheme();
            RequestedThemeChanged -= App_RequestedThemeChanged;
        }

        protected override void OnResume()
        {
            SetTheme();
            RequestedThemeChanged += App_RequestedThemeChanged;
            Current.Resources["DefaultFontSize"] = Preferences.Get("FontSize", 14);
        }

        private void App_RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
        {
            MainThread.BeginInvokeOnMainThread(SetTheme);
        }

        private void SetTheme()
        {
            Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
        }
        

        //static void SendingErrorReportHandler(object sender, SendingErrorReportEventArgs e)
        //{
        //    AppCenterLog.Info(LogTag, "Sending error report");

        //    var args = e as SendingErrorReportEventArgs;
        //    ErrorReport report = args.Report;

        //    //test some values
        //    if (report.StackTrace != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.StackTrace.ToString());
        //    }
        //    else if (report.AndroidDetails != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName);
        //    }
        //}

        //static void SentErrorReportHandler(object sender, SentErrorReportEventArgs e)
        //{
        //    AppCenterLog.Info(LogTag, "Sent error report");

        //    var args = e as SentErrorReportEventArgs;
        //    ErrorReport report = args.Report;

        //    //test some values
        //    if (report.StackTrace != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.StackTrace.ToString());
        //    }
        //    else
        //    {
        //        AppCenterLog.Info(LogTag, "No system exception was found");
        //    }

        //    if (report.AndroidDetails != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName);
        //    }
        //}

        //static void FailedToSendErrorReportHandler(object sender, FailedToSendErrorReportEventArgs e)
        //{
        //    AppCenterLog.Info(LogTag, "Failed to send error report");

        //    var args = e as FailedToSendErrorReportEventArgs;
        //    ErrorReport report = args.Report;

        //    //test some values
        //    if (report.StackTrace != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.StackTrace.ToString());
        //    }
        //    else if (report.AndroidDetails != null)
        //    {
        //        AppCenterLog.Info(LogTag, report.AndroidDetails.ThreadName);
        //    }

        //    if (e.Exception != null)
        //    {
        //        AppCenterLog.Info(LogTag, "There is an exception associated with the failure");
        //    }
        //}

        //bool ShouldProcess(ErrorReport report)
        //{
        //    AppCenterLog.Info(LogTag, "Determining whether to process error report");
        //    return true;
        //}

        //bool ConfirmationHandler()
        //{
        //    Xamarin.Forms.Device.BeginInvokeOnMainThread(() =>
        //    {
        //        Current.MainPage.DisplayActionSheet("Kilitlenme algılandı. Anonim kilitlenme raporu gönderilsin mi?", null, null, "Gönder", "Her zaman gönder", "Gönderme").ContinueWith((arg) =>
        //        {
        //            var answer = arg.Result;
        //            UserConfirmation userConfirmationSelection;
        //            if (answer == "Gönder")
        //            {
        //                userConfirmationSelection = UserConfirmation.Send;
        //            }
        //            else if (answer == "Her zaman gönder")
        //            {
        //                userConfirmationSelection = UserConfirmation.AlwaysSend;
        //            }
        //            else
        //            {
        //                userConfirmationSelection = UserConfirmation.DontSend;
        //            }
        //            AppCenterLog.Debug(LogTag, "User selected confirmation option: \"" + answer + "\"");
        //            Crashes.NotifyUserConfirmation(userConfirmationSelection);
        //        });
        //    });

        //    return true;
        //}
    }
}
