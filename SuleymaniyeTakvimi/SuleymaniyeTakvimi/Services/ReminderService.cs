using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Matcha.BackgroundService;
using MediaManager;
using Plugin.LocalNotifications;
using Plugin.SimpleAudioPlayer;

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
            //CrossLocalNotifications.Current.Show("Service Running", $"Service running well at {DateTime.Now.ToShortTimeString()}",
            //    1000);
            //ISimpleAudioPlayer player = Plugin.SimpleAudioPlayer.CrossSimpleAudioPlayer.Current;
            //player.Load(GetStreamFromFile("ezan.mp3"));
            //player.Play();
            //await CrossMediaManager.Current.PlayFromAssembly("ezan.mp3").ConfigureAwait(false);
            return true;
        }

        //Stream GetStreamFromFile(string filename)
        //{
        //    var assembly = typeof(App).GetTypeInfo().Assembly;
        //    var stream = assembly.GetManifestResourceStream("SuleymaniyeTakvimi.Assets." + filename);
        //    return stream;
        //}

    }
}
