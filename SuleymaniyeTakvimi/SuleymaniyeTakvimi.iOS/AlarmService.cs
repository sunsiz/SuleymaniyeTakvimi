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
        private static readonly UNUserNotificationCenter NotificationCenter = UNUserNotificationCenter.Current;
        public void SetAlarm(DateTime today, TimeSpan triggerTimeSpan, int timeOffset, string name)
        {
            DateTime triggerDateTime = today.Add(triggerTimeSpan).AddMinutes(-timeOffset);
            triggerDateTime = DateTime.SpecifyKind(triggerDateTime, DateTimeKind.Utc);
            try
            {
                string title;
                string body;
                string sound;
                var timeReminder = AppResources.VaktiHatirlatmasi;
                var time = AppResources.Vakti;
                switch (name)
                {
                    case "Fecri Kazip": title = $"{AppResources.FecriKazip} {timeReminder}"; body = $"{AppResources.FecriKazip} {time} {triggerTimeSpan}"; sound = "fecrikazip"; break;
                    case "Fecri Sadık": title = $"{ AppResources.FecriSadik} {timeReminder}"; body = $"{AppResources.FecriSadik} {time} {triggerTimeSpan}"; sound = "fecrisadik"; break;
                    case "Sabah Sonu": title = $"{AppResources.SabahSonu} {timeReminder}"; body = $"{AppResources.SabahSonu} {time} {triggerTimeSpan}"; sound = "sabahsonu"; break;
                    case "Öğle": title = $"{AppResources.Ogle} {timeReminder}"; body = $"{AppResources.Ogle} {time} {triggerTimeSpan}"; sound = "ogle"; break;
                    case "İkindi": title = $"{AppResources.Ikindi} {timeReminder}"; body = $"{AppResources.Ikindi} {time} {triggerTimeSpan}"; sound = "ikindi"; break;
                    case "Akşam": title = $"{AppResources.Aksam} {timeReminder}"; body = $"{AppResources.Aksam} {time} {triggerTimeSpan}"; sound = "aksam"; break;
                    case "Yatsı": title = $"{AppResources.Yatsi} {timeReminder}"; body = $"{AppResources.Yatsi} {time} {triggerTimeSpan}"; sound = "yatsi"; break;
                    case "Yatsı Sonu": title = $"{AppResources.YatsiSonu} {timeReminder}"; body = $"{AppResources.YatsiSonu} {time} {triggerTimeSpan}"; sound = "yatsisonu"; break;
                    default:return;
                }
                var alarmSesi = Preferences.Get(sound + "AlarmSesi", "kus") + ".wav";
                var content = new UNMutableNotificationContent
                {
                    Title = title,
                    Subtitle = AppResources.SuleymaniyeVakfiTakvimi,
                    Body = body,
                    Sound = UNNotificationSound.GetSound(alarmSesi)//UNNotificationSound.GetCriticalSound(alarmSesi, 1.0f)
                };
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
                var requestId = $"SuleymaniyeTakvimiRequest{triggerDateTime:yyyyMMddhhmmss}";
                var request = UNNotificationRequest.FromIdentifier(requestId, content, trigger);

                // Add to system to modify existing Notification
                NotificationCenter.AddNotificationRequest(request, (err) =>
                {
                    if (err != null)
                    {
                        // Do something with error...
                        Debug.WriteLine($"Error: {err}");
                        UserDialogs.Instance.Alert($"{AppResources.Hatadetaylari} {err}", AppResources.Alarmkurarkenhataolustu, AppResources.Tamam);
                    }
                    else Debug.WriteLine($"Notification Scheduled: {request} \n {content}");
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Notification Scheduling Error: {{0}}", ex.Message);
            }
        }

        public void CancelAlarm()
        {
            NotificationCenter.RemoveAllDeliveredNotifications();
            NotificationCenter.RemoveAllPendingNotificationRequests();
        }

        public void StartAlarmForegroundService()
        {
            
        }

        public void StopAlarmForegroundService()
        {
            
        }
    }
}