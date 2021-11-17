using System;
using EventKit;
using Foundation;
using SuleymaniyeTakvimi.Services;

namespace SuleymaniyeTakvimi.iOS
{
    public class AlarmService:IAlarmService
    {
        //public void SetAlarm(TimeSpan triggerTimeSpan, string name)
        //{
            
        //}

        public void SetAlarm(DateTime today, TimeSpan triggerTimeSpan, string name)
        {
            EKEventStore eventStore = new EKEventStore();
            EKReminder reminder = EKReminder.Create(eventStore);

            reminder.Title = $"{name} vakti Hatırlatması";
            // an error for the reminders and calendars
            NSError e = new NSError();
            // an alarm time
            EKAlarm timeToRing = new EKAlarm();
            DateTime triggerDateTime = today;
            triggerDateTime.Add(triggerTimeSpan);
            if (triggerDateTime.Kind == DateTimeKind.Unspecified)
                triggerDateTime = DateTime.SpecifyKind(triggerDateTime, DateTimeKind.Local);
            timeToRing.AbsoluteDate = (NSDate) triggerDateTime;

            reminder.AddAlarm(timeToRing);

            reminder.Notes = GetFormattedRemainingTime();

            //reminder.Calendar = calendar;
            reminder.Calendar = eventStore.DefaultCalendarForNewReminders;

            eventStore.SaveReminder(reminder, true, out e);
        }

        public void CancelAlarm()
        {
            EKEventStore eventStore = new EKEventStore();
            EKReminder reminder = EKReminder.Create(eventStore);
            
            // an error for the reminders and calendars
            NSError e = new NSError();
            // an alarm time
            var alarms = reminder.Alarms;

            if (alarms != null)
                foreach (EKAlarm alarm in alarms)
                    reminder.RemoveAlarm(alarm);
            
            eventStore.SaveReminder(reminder, true, out e);
        }

        private string GetFormattedRemainingTime()
        {
            var message = "";
            var data = new DataService();
            var takvim = data.takvim;
            var currentTime = DateTime.Now.TimeOfDay;
            if (currentTime < TimeSpan.Parse(takvim.FecriKazip))
                message = "Fecri Kazipin (Sahurun) girmesi için kalan vakit: " +
                          (TimeSpan.Parse(takvim.FecriKazip) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.FecriKazip) && currentTime <= TimeSpan.Parse(takvim.FecriSadik))
                message = "Fecri Sadık (Sahur bitimi) için kalan vakit: " +
                          (TimeSpan.Parse(takvim.FecriSadik) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.FecriSadik) && currentTime <= TimeSpan.Parse(takvim.SabahSonu))
                message = "Sabah Sonu için kalan vakit: " +
                          (TimeSpan.Parse(takvim.SabahSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.SabahSonu) && currentTime <= TimeSpan.Parse(takvim.Ogle))
                message = "Öğlenin girmesi için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Ogle) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ogle) && currentTime <= TimeSpan.Parse(takvim.Ikindi))
                message = "Öğlenin çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Ikindi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Ikindi) && currentTime <= TimeSpan.Parse(takvim.Aksam))
                message = "İkindinin çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Aksam) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Aksam) && currentTime <= TimeSpan.Parse(takvim.Yatsi))
                message = "Akşamın çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.Yatsi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.Yatsi) && currentTime <= TimeSpan.Parse(takvim.YatsiSonu))
                message = "Yatsının çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.YatsiSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            else if (currentTime >= TimeSpan.Parse(takvim.YatsiSonu))
                message = "Yatsının çıktığından beri geçen vakit: " +
                          (currentTime - TimeSpan.Parse(takvim.YatsiSonu)).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");

            return message;
        }
    }
}