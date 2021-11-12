using System;

namespace SuleymaniyeTakvimi.Services
{
    public interface IAlarmService
    {
        //void SetAlarm(TimeSpan triggerTimeSpan, string name);
        void SetAlarm(DateTime date, TimeSpan triggerTimeSpan, string name);
        void CancelAlarm();
    }
}