using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Shiny;
using Shiny.Jobs;
using Shiny.Notifications;
using SuleymaniyeTakvimi.Services;

namespace SuleymaniyeTakvimi
{
    public class ShinyDelegate:IJob, IAppStateDelegate
    {
        // notice you can inject anything you registered in your application here
        readonly INotificationManager _notifications;
        private readonly DataService _data = new DataService();

        public ShinyDelegate(INotificationManager notifications)
        {
            this._notifications = notifications;
        }

        public void OnBackground()
        {
            _data.CheckNotification();
        }

        public void OnForeground()
        {
            _data.CheckNotification();
        }

        public void OnStart()
        {
            _data.CheckNotification();
        }

        public async Task<bool> Run(JobInfo jobInfo, CancellationToken cancelToken)
        {
            var runJob = false;
            if (jobInfo.LastRunUtc == null) // job has never run
                runJob = true;

            else if (DateTime.UtcNow > jobInfo.LastRunUtc.Value.AddMinutes(5))
                runJob = true;  // its been at least an hour since the last run

            if (runJob)
            {
                _data.CheckNotification();
            }
            return true;
        }
    }
}
