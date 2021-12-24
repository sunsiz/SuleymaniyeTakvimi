using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Animation;
using Android.Util;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "Süleymaniye Vakfı Takvimi", MainLauncher = true, NoHistory = true, Theme = "@style/MyTheme.Splash")]
    public class SplashScreen : Activity, Android.Animation.Animator.IAnimatorListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Log.Info("TimeStamp-SplashScreen-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            SetContentView(Resource.Layout.SplashLayout);

            var animation = FindViewById<Com.Airbnb.Lottie.LottieAnimationView>(Resource.Id.animation_view);

            animation.AddAnimatorListener(this);
        }
#nullable enable
        public void OnAnimationCancel(Animator? animation)
        {
            
        }

        public void OnAnimationEnd(Animator? animation)
        {
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }

        public void OnAnimationRepeat(Animator? animation)
        {
            
        }

        public void OnAnimationStart(Animator? animation)
        {
            //Task startupWork = new Task(() => { AppInitialize(); });
            //startupWork.Start();
            //var LastLatitude = Preferences.Get("LastLatitude", 0.0);
            //var LastLongitude = Preferences.Get("LastLongitude", 0.0);
            //if (LastLatitude != 0.0 && LastLongitude!=0.0)
            //{
            //    Task StartupWork = new Task(() => AppInitialize());
            //    StartupWork.Start();
            //}
        }

        private void AppInitialize()
        {
            //Log.Info("TimeStamp-AppInitialize-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            //DataService data = new DataService();
            ////var _takvim = data.GetCurrentLocation();
            ////data.VakitHesabi();
            ////data.SetAlarms();
            //Location location = new Location(Preferences.Get("LastLatitude", 0.0),
            //    Preferences.Get("LastLongitude", 0.0), Preferences.Get("LastAltitude", 0.0));
            //var takvim = data.GetPrayerTimes(location).Result;
            //Log.Info("TimeStamp-AppInitialize-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }
    }
}