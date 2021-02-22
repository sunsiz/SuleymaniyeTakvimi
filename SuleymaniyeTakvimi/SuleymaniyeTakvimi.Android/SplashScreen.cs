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
using Android.Animation;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "Süleymaniye Vakfı Takvimi", MainLauncher = true, NoHistory = true, Theme = "@style/MyTheme.Splash")]
    public class SplashScreen : Activity, Android.Animation.Animator.IAnimatorListener
    {
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);


            SetContentView(Resource.Layout.SplashLayout);

            var animation = FindViewById<Com.Airbnb.Lottie.LottieAnimationView>(Resource.Id.animation_view);

            animation.AddAnimatorListener(this);
        }

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