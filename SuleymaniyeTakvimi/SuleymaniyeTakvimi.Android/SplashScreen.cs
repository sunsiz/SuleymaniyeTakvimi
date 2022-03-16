using Android.App;
using Android.Content;
using Android.OS;
using System;
using Android.Animation;
using Android.Util;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "Süleymaniye Vakfı Takvimi", MainLauncher = true, NoHistory = true, Theme = "@style/MyTheme.Splash")]
    public class SplashScreen : Activity, Animator.IAnimatorListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            Log.Info("TimeStamp-SplashScreen-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            SetContentView(Resource.Layout.SplashLayout);

            var animation = FindViewById<Com.Airbnb.Lottie.LottieAnimationView>(Resource.Id.animation_view);

            animation?.AddAnimatorListener(this);
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
            
        }
    }
}