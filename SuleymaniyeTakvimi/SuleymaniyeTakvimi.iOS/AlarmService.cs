using System;
using System.Diagnostics;
using Acr.UserDialogs;
using Foundation;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using UserNotifications;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.iOS
{
    public class AlarmService : IAlarmService
    {
        public void SetAlarm(DateTime today, TimeSpan triggerTimeSpan, int timeOffset, string name)
        {
            //UILocalNotification notification = new UILocalNotification();
            DateTime triggerDateTime = today;
            triggerDateTime += triggerTimeSpan - TimeSpan.FromMinutes(timeOffset);
            triggerDateTime = DateTime.SpecifyKind(triggerDateTime, DateTimeKind.Utc);
            var alarmSesi = Preferences.Get(name + "AlarmSesi", "kus") + ".wav";
            try
            {
                var content = new UNMutableNotificationContent
                {
                    Title = $"{name} {AppResources.VaktiHatirlatmasi}",
                    Subtitle = AppResources.SuleymaniyeVakfiTakvimi,
                    Body = $"{name} {AppResources.Vakti} {triggerTimeSpan}",//GetFormattedRemainingTime(),
                    Sound = UNNotificationSound.GetSound(alarmSesi)
                };
                //content.Badge = 9;
                // New trigger time
                NSDateComponents dateComponents = new NSDateComponents()
                {
                    Calendar = NSCalendar.CurrentCalendar,
                    Year = triggerDateTime.Year,
                    Month = triggerDateTime.Month,
                    Day = triggerDateTime.Day,
                    Hour = triggerDateTime.Hour,
                    Minute = triggerDateTime.Minute,
                    Second = triggerDateTime.Second
                };
                var trigger = UNCalendarNotificationTrigger.CreateTrigger(dateComponents, false);
                // ID of Notification to be updated
                var requestId = "SuleymaniyeTakvimiRequest" + triggerDateTime.ToString("yyyyMMddhhmmss");
                var request = UNNotificationRequest.FromIdentifier(requestId, content, trigger);

                // Add to system to modify existing Notification
                UNUserNotificationCenter.Current.AddNotificationRequest(request, (err) =>
                {
                    if (err != null)
                    {
                        // Do something with error...
                        Debug.WriteLine("Error: {0}", err);
                        UserDialogs.Instance.Alert($"{AppResources.Hatadetaylari} {err}", AppResources.Alarmkurarkenhataolustu, AppResources.Tamam);
                    }
                    else Debug.WriteLine("Notification Scheduled: {0} \n {1}", request, content);
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Notification Scheduling Error: {0}", ex.Message);
            }
        }

        public void CancelAlarm()
        {
            UNUserNotificationCenter.Current.RemoveAllDeliveredNotifications();
            UNUserNotificationCenter.Current.RemoveAllPendingNotificationRequests();
        }

        public void StartAlarmForegroundService()
        {
            
        }

        public void StopAlarmForegroundService()
        {
            
        }

        //private string GetFormattedRemainingTime()
        //{
        //    var message = "";
        //    var data = new DataService();
        //    var takvim = data.takvim;
        //    var currentTime = DateTime.Now.TimeOfDay;
        //    if (currentTime < TimeSpan.Parse(takvim.FecriKazip))
        //        message = AppResources.FecriKazibingirmesinekalanvakit +
        //                  (TimeSpan.Parse(takvim.FecriKazip) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.FecriKazip) && currentTime <= TimeSpan.Parse(takvim.FecriSadik))
        //        message = AppResources.FecriSadikakalanvakit +
        //                  (TimeSpan.Parse(takvim.FecriSadik) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.FecriSadik) && currentTime <= TimeSpan.Parse(takvim.SabahSonu))
        //        message = AppResources.SabahSonunakalanvakit +
        //                  (TimeSpan.Parse(takvim.SabahSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.SabahSonu) && currentTime <= TimeSpan.Parse(takvim.Ogle))
        //        message = AppResources.Ogleningirmesinekalanvakit +
        //                  (TimeSpan.Parse(takvim.Ogle) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.Ogle) && currentTime <= TimeSpan.Parse(takvim.Ikindi))
        //        message = AppResources.Oglenincikmasinakalanvakit +
        //                  (TimeSpan.Parse(takvim.Ikindi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.Ikindi) && currentTime <= TimeSpan.Parse(takvim.Aksam))
        //        message = AppResources.Ikindinincikmasinakalanvakit +
        //                  (TimeSpan.Parse(takvim.Aksam) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.Aksam) && currentTime <= TimeSpan.Parse(takvim.Yatsi))
        //        message = AppResources.Aksamincikmasnakalanvakit +
        //                  (TimeSpan.Parse(takvim.Yatsi) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    else if (currentTime >= TimeSpan.Parse(takvim.Yatsi) && currentTime <= TimeSpan.Parse(takvim.YatsiSonu))
        //    {
        //        message = AppResources.Yatsinincikmasinakalanvakit +
        //                  (TimeSpan.Parse(takvim.YatsiSonu) - currentTime).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");
        //    }
        //    else if (currentTime >= TimeSpan.Parse(takvim.YatsiSonu))
        //        message = AppResources.Yatsininciktigindangecenvakit +
        //                  (currentTime - TimeSpan.Parse(takvim.YatsiSonu)).Add(TimeSpan.FromMinutes(1)).ToString(@"hh\:mm");

        //    return message;
        //}
    }
}