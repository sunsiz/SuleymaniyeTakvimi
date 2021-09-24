using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FFImageLoading.Forms;
//using Matcha.BackgroundService;
using MvvmHelpers.Commands;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    class CalendarViewModel : MvvmHelpers.BaseViewModel
    {
        public ObservableCollection<Schedule> schedule { get; set; }
        public ICommand VibrationCheckedChanged { get; private set; }
        public ICommand NotificationCheckedChanged { get; private set; }
        public ICommand AlarmCheckedChanged { get; private set; }
        public ICommand ReminderEnabledChanged { get; private set; }
        public ICommand LoadSchedulesCommand { get; }
        public string Today
        {
            get => DateTime.Today.ToString("yyyy MMMM dd");
        }


        public CalendarViewModel()
        {
            Title = "Süleymaniye Vakfı Takvimi";
            ReminderEnabledChanged = new Command(ReminderSettingChanged);
            NotificationCheckedChanged = new Command(notificationSettingChanged);
            VibrationCheckedChanged = new Xamarin.Forms.Command(vibrationSettingChanged);
            AlarmCheckedChanged = new Xamarin.Forms.Command(alarmSettingChanged);
            LoadSchedulesCommand = new Command(async () => await ExecuteLoadSchedulesCommandAsync().ConfigureAwait(false));
            LoadSchedulesCommand.Execute(ExecuteLoadSchedulesCommandAsync());
        }

        private async Task ExecuteLoadSchedulesCommandAsync()
        {
            var data = new DataService();
            var takvim = data.takvim;
            schedule = new ObservableCollection<Schedule>
            {
                new Schedule
                {
                    Title = "Fecri Kazip",
                    Hour = takvim.FecriKazip,
                    State = CheckState(DateTime.Parse(takvim.FecriKazip), DateTime.Parse(takvim.FecriSadik)),
                    ReminderOn = Preferences.Get("fecrikazipEtkin", false),
                    Vibration = Preferences.Get("fecrikazipTitreme", false),
                    Notification = Preferences.Get("fecrikazipBildiri", false),
                    Alarm = Preferences.Get("fecrikazipAlarm", false)
                },
                new Schedule
                {
                    Title = "Fecri Sadik",
                    Hour = takvim.FecriSadik,
                    State = CheckState(DateTime.Parse(takvim.SabahSonu), DateTime.Parse(takvim.FecriSadik)),
                    ReminderOn = Preferences.Get("fecrisadikEtkin", false),
                    Vibration = Preferences.Get("fecrisadikTitreme", false),
                    Notification = Preferences.Get("fecrisadikBildiri", false),
                    Alarm = Preferences.Get("fecrisadikAlarm", false)
                },
                new Schedule
                {
                    Title = "Sabah Sonu",
                    Hour = takvim.SabahSonu,
                    State = CheckState(DateTime.Parse(takvim.Ogle), DateTime.Parse(takvim.SabahSonu)),
                    ReminderOn = Preferences.Get("sabahsonuEtkin", false),
                    Vibration = Preferences.Get("sabahsonuTitreme", false),
                    Notification = Preferences.Get("sabahsonuBildiri", false),
                    Alarm = Preferences.Get("sabahsonuAlarm", false)
                },
                new Schedule
                {
                    Title = "Öğle",
                    Hour = takvim.Ogle,
                    State = CheckState(DateTime.Parse(takvim.Ikindi), DateTime.Parse(takvim.Ogle)),
                    ReminderOn = Preferences.Get("ogleEtkin", false),
                    Vibration = Preferences.Get("ogleTitreme", false),
                    Notification = Preferences.Get("ogleBildiri", false),
                    Alarm = Preferences.Get("ogleAlarm", false)
                },
                new Schedule
                {
                    Title = "İkindi",
                    Hour = takvim.Ikindi,
                    State = CheckState(DateTime.Parse(takvim.Aksam), DateTime.Parse(takvim.Ikindi)),
                    ReminderOn = Preferences.Get("ikindiEtkin", false),
                    Vibration = Preferences.Get("ikindiTitreme", false),
                    Notification = Preferences.Get("ikindiBildiri", false),
                    Alarm = Preferences.Get("ikindiAlarm", false)
                },
                new Schedule
                {
                    Title = "Akşam",
                    Hour = takvim.Aksam,
                    State = CheckState(DateTime.Parse(takvim.Yatsi), DateTime.Parse(takvim.Aksam)),
                    ReminderOn = Preferences.Get("aksamEtkin", false),
                    Vibration = Preferences.Get("aksamTitreme", false),
                    Notification = Preferences.Get("aksamBildiri", false),
                    Alarm = Preferences.Get("aksamAlarm", false)
                },
                new Schedule
                {
                    Title = "Yatsı",
                    Hour = takvim.Yatsi,
                    State = CheckState(DateTime.Parse(takvim.YatsiSonu), DateTime.Parse(takvim.Yatsi)),
                    ReminderOn = Preferences.Get("yatsiEtkin", false),
                    Vibration = Preferences.Get("yatsiTitreme", false),
                    Notification = Preferences.Get("yatsiBildiri", false),
                    Alarm = Preferences.Get("yatsiAlarm", false)
                },
                new Schedule
                {
                    Title = "Yatsı Sonu",
                    Hour = takvim.YatsiSonu,
                    State = CheckState(DateTime.Parse(takvim.FecriKazip), DateTime.Parse(takvim.YatsiSonu)),
                    ReminderOn = Preferences.Get("yatsisonuEtkin", false),
                    Vibration = Preferences.Get("yatsisonuTitreme", false),
                    Notification = Preferences.Get("yatsisonuBildiri", false),
                    Alarm = Preferences.Get("yatsisonuAlarm", false)
                }
            };
        }

        private string CheckState(DateTime next, DateTime current)
        {
            var state = "";
            if (DateTime.Now > next) state = "Passed";
            if (DateTime.Now > current && DateTime.Now < next) state = "Happening";
            if (DateTime.Now < current) state = "Waiting";
            return state;
        }

        private void ReminderSettingChanged(object obj)
        {
            if (!IsBusy)
            {
                if (obj.GetType() == typeof(Schedule))
                {
                    var theSchedule = obj as Schedule;
                    switch (theSchedule.Title)
                    {
                        case "Fecri Kazip":
                            Preferences.Set("fecrikazipEtkin", theSchedule.ReminderOn);
                            break;
                        case "Fecri Sadik":
                            Preferences.Set("fecrisadikEtkin", theSchedule.ReminderOn);
                            break;
                        case "Sabah Sonu":
                            Preferences.Set("sabahsonuEtkin", theSchedule.ReminderOn);
                            break;
                        case "Öğle":
                            Preferences.Set("ogleEtkin", theSchedule.ReminderOn);
                            break;
                        case "İkindi":
                            Preferences.Set("ikindiEtkin", theSchedule.ReminderOn);
                            break;
                        case "Akşam":
                            Preferences.Set("aksamEtkin", theSchedule.ReminderOn);
                            break;
                        case "Yatsı":
                            Preferences.Set("yatsiEtkin", theSchedule.ReminderOn);
                            break;
                        case "Yatsı Sonu":
                            Preferences.Set("yatsisonuEtkin", theSchedule.ReminderOn);
                            break;
                    }

                    //if (theSchedule.ReminderOn && BackgroundAggregatorService.Instance == null)
                    //{
                    //    BackgroundAggregatorService.Add(() => new ReminderService(60));
                    //    BackgroundAggregatorService.StartBackgroundService();
                    //}
                }
            }
        }

        private void alarmSettingChanged(object obj)
        {
            if (obj.GetType() == typeof(Schedule))
            {
                var theSchedule = obj as Schedule;
                switch (theSchedule.Title)
                {
                    case "Fecri Kazip":
                        Preferences.Set("fecrikazipAlarm", theSchedule.Alarm);
                        break;
                    case "Fecri Sadik":
                        Preferences.Set("fecrisadikAlarm", theSchedule.Alarm);
                        break;
                    case "Sabah Sonu":
                        Preferences.Set("sabahsonuAlarm", theSchedule.Alarm);
                        break;
                    case "Öğle":
                        Preferences.Set("ogleAlarm", theSchedule.Alarm);
                        break;
                    case "İkindi":
                        Preferences.Set("ikindiAlarm", theSchedule.Alarm);
                        break;
                    case "Akşam":
                        Preferences.Set("aksamAlarm", theSchedule.Alarm);
                        break;
                    case "Yatsı":
                        Preferences.Set("yatsiAlarm", theSchedule.Alarm);
                        break;
                    case "Yatsı Sonu":
                        Preferences.Set("yatsisonuAlarm", theSchedule.Alarm);
                        break;
                }
            }
        }

        public void vibrationSettingChanged(object obj)
        {
            if (obj.GetType() == typeof(Schedule))
            {
                var theSchedule = obj as Schedule;
                switch (theSchedule.Title)
                {
                    case "Fecri Kazip":
                        Preferences.Set("fecrikazipTitreme", theSchedule.Vibration);
                        break;
                    case "Fecri Sadik":
                        Preferences.Set("fecrisadikTitreme", theSchedule.Vibration);
                        break;
                    case "Sabah Sonu":
                        Preferences.Set("sabahsonuTitreme", theSchedule.Vibration);
                        break;
                    case "Öğle":
                        Preferences.Set("ogleTitreme", theSchedule.Vibration);
                        break;
                    case "İkindi":
                        Preferences.Set("ikindiTitreme", theSchedule.Vibration);
                        break;
                    case "Akşam":
                        Preferences.Set("aksamTitreme", theSchedule.Vibration);
                        break;
                    case "Yatsı":
                        Preferences.Set("yatsiTitreme", theSchedule.Vibration);
                        break;
                    case "Yatsı Sonu":
                        Preferences.Set("yatsisonuTitreme", theSchedule.Vibration);
                        break;
                }
            }
        }

        public void notificationSettingChanged(object obj)
        {
            if (obj.GetType() == typeof(Schedule))
            {
                var theSchedule = (Schedule)obj;
                switch (theSchedule.Title)
                {
                    case "Fecri Kazip":
                        Preferences.Set("fecrikazipBildiri", theSchedule.Notification);
                        break;
                    case "Fecri Sadik":
                        Preferences.Set("fecrisadikBildiri", theSchedule.Notification);
                        break;
                    case "Sabah Sonu":
                        Preferences.Set("sabahsonuBildiri", theSchedule.Notification);
                        break;
                    case "Öğle":
                        Preferences.Set("ogleBildiri", theSchedule.Notification);
                        break;
                    case "İkindi":
                        Preferences.Set("ikindiBildiri", theSchedule.Notification);
                        break;
                    case "Akşam":
                        Preferences.Set("aksamBildiri", theSchedule.Notification);
                        break;
                    case "Yatsı":
                        Preferences.Set("yatsiBildiri", theSchedule.Notification);
                        break;
                    case "Yatsı Sonu":
                        Preferences.Set("yatsisonuBildiri", theSchedule.Notification);
                        break;
                }
            }
        }
        public void OnAppearing()
        {
            IsBusy = true;
            LoadSchedulesCommand.Execute(ExecuteLoadSchedulesCommandAsync());
        }
    }
}
