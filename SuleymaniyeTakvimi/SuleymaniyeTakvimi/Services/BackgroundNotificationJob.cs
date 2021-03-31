using System;
using System.Threading;
using System.Threading.Tasks;
using Shiny.Jobs;
using SuleymaniyeTakvimi.Services;

namespace SuleymaniyeTakvimi
{
    internal class BackgroundNotificationJob:IJob
    {
        private DataService _data;

        public BackgroundNotificationJob()
        {
            this._data = new DataService();
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