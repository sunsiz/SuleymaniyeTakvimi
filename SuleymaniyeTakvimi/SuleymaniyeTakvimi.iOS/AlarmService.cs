using System;
using Acr.UserDialogs;
using EventKit;
using Foundation;
using SuleymaniyeTakvimi.Services;
using UIKit;
using UserNotifications;

namespace SuleymaniyeTakvimi.iOS
{
    public class AlarmService : IAlarmService
    {
        //public void SetAlarm(TimeSpan triggerTimeSpan, string name)
        //{
        
        //}

        public void SetAlarm(DateTime today, TimeSpan triggerTimeSpan, string name)
        {
            //UILocalNotification notification = new UILocalNotification();
            DateTime triggerDateTime = today;
            triggerDateTime += triggerTimeSpan;
            triggerDateTime = DateTime.SpecifyKind(triggerDateTime, DateTimeKind.Utc);
            //notification.FireDate = (NSDate) triggerDateTime;
            //notification.AlertTitle = $"{name} Hatirlatmasi"; // required for Apple Watch notifications
            ////notification.AlertAction = "View Alert";
            //notification.AlertBody = GetFormattedRemainingTime();
            //// modify the badge
            //notification.ApplicationIconBadgeNumber += 1;
            //// set the sound to be the default sound
            //notification.SoundName = UILocalNotification.DefaultSoundName;
            //UIApplication.SharedApplication.ScheduleLocalNotification(notification);
            var content = new UNMutableNotificationContent();
            content.Title = $"{name} Hatirlatmasi";
            content.Subtitle = "Suleymaniye vakfi takvimi";
            content.Body = GetFormattedRemainingTime();
            content.Sound = UNNotificationSound.GetSound("horoz.wav");
            //content.Badge = 9;
            // New trigger time
            NSDateComponents dateComponents = new NSDateComponents() { Year = triggerDateTime.Year, Month = triggerDateTime.Month, Day = triggerDateTime.Day, Hour = triggerDateTime.Hour, Minute = triggerDateTime.Minute, Second = triggerDateTime.Second };
            var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
            // ID of Notification to be updated
            var requestID = "SuleymaniyeTakvimiRequest";
            var request = UNNotificationRequest.FromIdentifier(requestID, content, trigger);

            // Add to system to modify existing Notification
            UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) => {
                if (err != null)
                {
                    // Do something with error...
                }
            });
            ////RequestAccess();
            ////EKEventStore eventStore = new EKEventStore();
            //EKReminder reminder = EKReminder.Create(AppDelegate.eventStore);

            //reminder.Title = $"{name} vakti Hatırlatması";
            //// an error for the reminders and calendars
            //NSError e = new NSError();
            //// an alarm time
            //EKAlarm timeToRing = new EKAlarm();
            //DateTime triggerDateTime = today;
            //triggerDateTime += triggerTimeSpan;
            ////if (triggerDateTime.Kind == DateTimeKind.Unspecified)
            //    triggerDateTime = DateTime.SpecifyKind(triggerDateTime, DateTimeKind.Utc);
            //timeToRing.AbsoluteDate = (NSDate) triggerDateTime;

            //reminder.AddAlarm(timeToRing);

            //reminder.Notes = GetFormattedRemainingTime();

            ////reminder.Calendar = calendar;
            //reminder.Calendar = AppDelegate.eventStore.DefaultCalendarForNewReminders;

            //AppDelegate.eventStore.SaveReminder(reminder, true, out e);
        }

        public void CancelAlarm()
        {
            ////RequestAccess();
            ////EKEventStore eventStore = new EKEventStore();
            //EKReminder reminder = EKReminder.Create(AppDelegate.eventStore);
            
            //// an error for the reminders and calendars
            //NSError e = new NSError();
            //// an alarm time
            //var alarms = reminder.Alarms;
            //EKCalendarItem myReminder = AppDelegate.eventStore.GetCalendarItem(reminder.CalendarItemIdentifier);

            //if (alarms != null)
            //{
            //    foreach (EKAlarm alarm in alarms)
            //        reminder.RemoveAlarm(alarm);
            //}
            //else //remove reminders in the reminders app
            //{
            //    // create our NSPredicate which we'll use for the query
            //    NSPredicate query = AppDelegate.eventStore.PredicateForReminders(null);

            //    // execute the query
            //    AppDelegate.eventStore.FetchReminders(
            //            query, (EKReminder[] items) =>
            //            {
            //                // do someting with the items
            //                if (items != null) foreach (var item in items)
            //                    {
            //                        AppDelegate.eventStore.RemoveReminder(item, true, out e);

            //                    }
            //            });
            //}
            
            //AppDelegate.eventStore.SaveReminder(reminder, true, out e);
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
            {
                message = "Yatsının çıkması için kalan vakit: " +
                          (TimeSpan.Parse(takvim.YatsiSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
            }
            else if (currentTime >= TimeSpan.Parse(takvim.YatsiSonu))
                message = "Yatsının çıktığından beri geçen vakit: " +
                          (currentTime - TimeSpan.Parse(takvim.YatsiSonu)).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");

            return message;
        }

        //private void RequestAccess()
        //{
        //    //eventStore.RequestAccess(EKEntityType.Event, AccessCompletionHandler);
        //    eventStore.RequestAccess(EKEntityType.Reminder, AccessCompletionHandler);

        //    void AccessCompletionHandler(bool granted, NSError error)
        //    {
        //        if (!granted)
        //        {
        //            UserDialogs.Instance.Alert("User Denied Access to Calendars/Reminders" + error.ToString(), "Access Denied");                    
        //        }
        //    }
        //}
    }
}