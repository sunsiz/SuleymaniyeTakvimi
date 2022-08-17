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
            try
            {
                string title;
                string body;
                string sound;
                switch (name)
                {
                    case "Fecri Kazip": title = $"{AppResources.FecriKazip} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.FecriKazip} {AppResources.Vakti} {triggerTimeSpan}"; sound = "fecrikazip"; break;
                    case "Fecri Sadık": title = $"{ AppResources.FecriSadik} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.FecriSadik} {AppResources.Vakti} {triggerTimeSpan}"; sound = "fecrisadik"; break;
                    case "Sabah Sonu": title = $"{AppResources.SabahSonu} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.SabahSonu} {AppResources.Vakti} {triggerTimeSpan}"; sound = "sabahsonu"; break;
                    case "Öğle": title = $"{AppResources.Ogle} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.Ogle} {AppResources.Vakti} {triggerTimeSpan}"; sound = "ogle"; break;
                    case "İkindi": title = $"{AppResources.Ikindi} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.Ikindi} {AppResources.Vakti} {triggerTimeSpan}"; sound = "ikindi"; break;
                    case "Akşam": title = $"{AppResources.Aksam} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.Aksam} {AppResources.Vakti} {triggerTimeSpan}"; sound = "aksam"; break;
                    case "Yatsı": title = $"{AppResources.Yatsi} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.Yatsi} {AppResources.Vakti} {triggerTimeSpan}"; sound = "yatsi"; break;
                    case "Yatsı Sonu": title = $"{AppResources.YatsiSonu} {AppResources.VaktiHatirlatmasi}"; body = $"{AppResources.YatsiSonu} {AppResources.Vakti} {triggerTimeSpan}"; sound = "yatsisonu"; break;
                    default:return;
                        //default: title = "Test Alarm"; body = $"{AppResources.Vakti} {triggerTimeSpan}"; sound = "sabahsonu"; break;
                }
                var alarmSesi = Preferences.Get(sound + "AlarmSesi", "kus") + ".wav";
                var content = new UNMutableNotificationContent
                {
                    Title = title,
                    Subtitle = AppResources.SuleymaniyeVakfiTakvimi,
                    Body = body,//GetFormattedRemainingTime(),
                    Sound = UNNotificationSound.GetCriticalSound(alarmSesi, 1.0f)//UNNotificationSound.GetSound(alarmSesi)
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