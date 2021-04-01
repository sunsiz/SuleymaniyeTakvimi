using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using FFImageLoading.Forms;
using Matcha.BackgroundService;
using MvvmHelpers.Commands;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    class CalendarViewModel : MvvmHelpers.BaseViewModel
    {
        public ObservableCollection<Schedule> schedule { get; set; }
        //Schedule selectedSchedule;
        String today;
        //private ICommand vibreationChecked;
        //private ICommand notificationChecked;
        //private ICommand enableReminder;
        //public ICommand VibrationChecked => vibreationChecked ??= new Command((sch) =>
        //{
        //    var theSchedule = (Schedule)sch;
        //    switch (theSchedule.Title)
        //    {
        //        case "Fecri Kazip": Preferences.Set("FKVibration", theSchedule.Vibration); break;
        //        case "Fecri Sadik": Preferences.Set("FSVibration", theSchedule.Vibration); break;
        //        case "Sabah Sonu": Preferences.Set("SSVibration", theSchedule.Vibration); break;
        //        case "Öğle": Preferences.Set("OGVibration", theSchedule.Vibration); break;
        //        case "İkindi": Preferences.Set("IKVibration", theSchedule.Vibration); break;
        //        case "Akşam": Preferences.Set("AKVibration", theSchedule.Vibration); break;
        //        case "Yatsı": Preferences.Set("YAVibration", theSchedule.Vibration); break;
        //        case "Yatsı Sonu": Preferences.Set("YSVibration", theSchedule.Vibration); break;
        //    }
        //});
        //public ICommand NotificationChecked => notificationChecked ??= new Command(notificationCheckedChanged);
        public ICommand VibrationCheckedChanged { get; private set; }
        public ICommand NotificationCheckedChanged { get; private set; }
        public ICommand AlarmCheckedChanged { get; private set; }
        public ICommand ReminderEnabledChanged { get; private set; }
        public ICommand LoadSchedulesCommand { get; }
        //public ICommand EnableReminder => enableReminder ??= new Command(() =>
        //{
        //    var testing = "";
        //});

        //public Schedule SelectedSchedule
        //{
        //    get => selectedSchedule;
        //    set => SetProperty(ref selectedSchedule, value);
        //}
        public string Today
        {
            get => DateTime.Today.ToString("yyyy MMMM dd");
            set => SetProperty(ref today, value);
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
                    //NotificationOn = Preferences.Get("FKNotification",false),
                    //Vibration = Preferences.Get("FKVibration",false),
                    //Notification = Preferences.Get("FKNotification",false)
                    //NotificationType = Preferences.Get("FKNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.FecriSadik),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.FecriKazip) &&
                    //            DateTime.Now < DateTime.Parse(takvim.FecriSadik),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.FecriKazip)
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
                    //NotificationOn = Preferences.Get("FSNotification",false),
                    //Vibration = Preferences.Get("FSVibration",false),
                    //Notification = Preferences.Get("FSNotification",false)
                    //NotificationType = Preferences.Get("FSNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.SabahSonu),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.FecriSadik) &&
                    //            DateTime.Now < DateTime.Parse(takvim.SabahSonu),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.FecriSadik)
                },
                new Schedule
                {
                    Title = "Sabah Sonu",
                    Hour = takvim.SabahSonu,
                    State = CheckState(DateTime.Parse(takvim.Ogle), DateTime.Parse(takvim.SabahSonu)),
                    ReminderOn = Preferences.Get("sabahsonuEtkin", false),
                    Vibration = Preferences.Get("sabahsonuTitreme", false),
                    Notification = Preferences.Get("SabahsonuBildiri", false),
                    Alarm = Preferences.Get("sabahsonuAlarm", false)
                    //NotificationOn = Preferences.Get("SSNotification",false),
                    //Vibration = Preferences.Get("SSVibration",false),
                    //Notification = Preferences.Get("SSNotification",false)
                    //NotificationType = Preferences.Get("SSNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Ogle),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.SabahSonu) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Ogle),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.SabahSonu)
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
                    //NotificationOn = Preferences.Get("OGNotification",false),
                    //Vibration = Preferences.Get("OGVibration",false),
                    //Notification = Preferences.Get("OGNotification",false)
                    //NotificationType = Preferences.Get("OGNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Ikindi),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Ogle) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Ikindi),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Ogle)
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
                    //NotificationOn = Preferences.Get("IKNotification",false),
                    //Vibration = Preferences.Get("IKVibration",false),
                    //Notification = Preferences.Get("IKNotification",false)
                    //NotificationType = Preferences.Get("IKNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Aksam),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Ikindi) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Aksam),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Ikindi)
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
                    //NotificationOn = Preferences.Get("AKNotification",false),
                    //Vibration = Preferences.Get("AKVibration",false),
                    //Notification = Preferences.Get("AKNotification",false)
                    //NotificationType = Preferences.Get("AKNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.Yatsi),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Aksam) &&
                    //            DateTime.Now < DateTime.Parse(takvim.Yatsi),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Aksam)
                },
                new Schedule
                {
                    Title = "Yatsı",
                    Hour = takvim.Yatsi,
                    State = CheckState(DateTime.Parse(takvim.YatsiSonu), DateTime.Parse(takvim.Yatsi)),
                    ReminderOn = Preferences.Get("yatsiEtkin", false),
                    Vibration = Preferences.Get("yatsiTitrme", false),
                    Notification = Preferences.Get("yatsiBildiri", false),
                    Alarm = Preferences.Get("yatsiAlarm", false)
                    //NotificationOn = Preferences.Get("YANotification",false),
                    //Vibration = Preferences.Get("YAVibration",false),
                    //Notification = Preferences.Get("YANotification",false)
                    //NotificationType = Preferences.Get("YANotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.YatsiSonu),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.Yatsi) &&
                    //            DateTime.Now < DateTime.Parse(takvim.YatsiSonu),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.Yatsi)
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
                    //NotificationOn = Preferences.Get("YSNotification",false),
                    //Vibration = Preferences.Get("YSVibration",false),
                    //Notification = Preferences.Get("YSNotification",false)
                    //NotificationType = Preferences.Get("YSNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.FecriKazip),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.YatsiSonu) &&
                    //            DateTime.Now < DateTime.Parse(takvim.FecriKazip),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.YatsiSonu)
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
            //Console.WriteLine(obj.ToString());
            //When page load the obj value awlways be false, so avoiding it.
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

                    if (theSchedule.ReminderOn && BackgroundAggregatorService.Instance == null)
                    {
                        BackgroundAggregatorService.Add(() => new ReminderService(60));
                        BackgroundAggregatorService.StartBackgroundService();
                    }
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
