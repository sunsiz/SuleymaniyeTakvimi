using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using AndroidX.AppCompat.App;

namespace SuleymaniyeTakvimi.Droid
{
    [Activity(Label = "@string/app_name", Theme = "@style/MyTheme.Splash", Icon = "@mipmap/icon", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = nameof(SplashActivity);

        public override void OnCreate(Bundle savedInstanceState, PersistableBundle persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState); 
            //SetContentView(Resource.Layout.SplashLayout);
            Log.Debug(TAG, "SplashActivity.OnCreate");
        }

        // Launches the startup task
        protected override void OnResume()
        {
            base.OnResume();
            StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        }
        public override void OnBackPressed() { }
        
        // Simulates background work that happens behind the splash screen
        //async void SimulateStartup()
        //{
        //    //Log.Debug(TAG, "Performing some startup work that takes a bit of time.");
        //    //await Task.Delay(8000); // Simulate a bit of startup work.
        //    //Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
        //    //StartActivity(new Intent(Application.Context, typeof(MainActivity)));
        //}
    }
}