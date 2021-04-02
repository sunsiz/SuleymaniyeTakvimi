using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Matcha.BackgroundService;
using MediaManager;
using Plugin.LocalNotifications;

namespace SuleymaniyeTakvimi.Services
{
    public class ReminderService:IPeriodicTask
    {
        public TimeSpan Interval { get; set; }
        public ReminderService(int seconds) => Interval = TimeSpan.FromSeconds(seconds);

        public async Task<bool> StartJob()
        {
            DataService data = new DataService();
            data.CheckReminders();
            ////Testing Notification - Will display notification every time service running.
            //CrossLocalNotifications.Current.Show("Service Running", $"Service running well at {DateTime.Now.ToShortTimeString()}", 1000);

            ////Testing play audio - will play audio every period of service.
            //await CrossMediaManager.Current.PlayFromAssembly("ezan.mp3").ConfigureAwait(false);
            return true;
        }

    }
}
