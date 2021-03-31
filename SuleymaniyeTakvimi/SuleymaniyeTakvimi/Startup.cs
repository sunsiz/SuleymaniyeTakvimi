using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Shiny;
using Shiny.Jobs;
using Shiny.Logging;

namespace SuleymaniyeTakvimi
{
    public class Startup:ShinyStartup
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            // custom logging
            Log.UseConsole();
            Log.UseDebug();

            // create your infrastructure
            //services.AddSingleton<SampleSqliteConnection>();

            // register all of the acr stuff you want to use
            //services.UseHttpTransfers<SampleDelegate>();
            //services.UseBeacons<SampleDelegate>();
            //services.UseBleCentral();
            //services.UseBlePeripherals();
            //services.UseGpsBackground<SampleDelegate>();
            //services.UseGeofencing<SampleDelegate>();
            services.UseNotifications(true);
            //services.UseSpeechRecognition();

            //services.UseAccelerometer();
            //services.UseAmbientLightSensor();
            //services.UseBarometer();
            //services.UseCompass();
            //services.UseDeviceOrientationSensor();
            //services.UseMagnetometer();
            //services.UsePedometer();
            //services.UseProximitySensor();

            // to register in your Startup
            services.AddAppState<ShinyDelegate>();
            //var job = new JobInfo(typeof(BackgroundNotificationJob), nameof(BackgroundNotificationJob))
            //{
            //    BatteryNotLow = true,
            //    DeviceCharging = true,
            //    Repeat = true
            //};

            //services.RegisterJob(job);
        }
    }
}
