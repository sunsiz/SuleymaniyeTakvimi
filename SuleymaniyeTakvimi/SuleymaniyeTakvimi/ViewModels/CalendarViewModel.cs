using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using FFImageLoading.Forms;
using MvvmHelpers.Commands;
using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    class CalendarViewModel:MvvmHelpers.BaseViewModel
    {
        public ObservableCollection<Schedule> schedule { get; set; }
        Schedule selectedSchedule;
        String today;
        private ICommand vibreationChecked;
        //private ICommand notificationChecked;
        //private ICommand enableReminder;
        public ICommand VibrationChecked => vibreationChecked ??= new Command(() =>
        {
            var theSchedule = selectedSchedule;
            switch (theSchedule.Title)
            {
                case "Fecri Kazip": Preferences.Set("FKVibration", theSchedule.Vibration); break;
                case "Fecri Sadik": Preferences.Set("FSVibration", theSchedule.Vibration); break;
                case "Saba Sonu": Preferences.Set("SSVibration", theSchedule.Vibration); break;
                case "Öğle": Preferences.Set("OGVibration", theSchedule.Vibration); break;
                case "İkindi": Preferences.Set("IKVibration", theSchedule.Vibration); break;
                case "Akşam": Preferences.Set("AKVibration", theSchedule.Vibration); break;
                case "Yatsı": Preferences.Set("YAVibration", theSchedule.Vibration); break;
                case "Yatsı Sonu": Preferences.Set("YSVibration", theSchedule.Vibration); break;
            }
        });
        //public ICommand NotificationChecked => notificationChecked ??= new Command(notificationCheckedChanged);
        public ICommand NotificationCheckedChanged { get; private set; }
        public ICommand ReminderEnabledChanged { get; private set; }
        //public ICommand EnableReminder => enableReminder ??= new Command(() =>
        //{
        //    var testing = "";
        //});

        public Schedule SelectedSchedule
        {
            get => selectedSchedule;
            set => SetProperty(ref selectedSchedule, value);
        }
        public string Today
        {
            get { return DateTime.Today.ToString("yyyy MMMM dd"); }
            set { SetProperty(ref today, value); }
        }

        
        public CalendarViewModel()
        {
            Title = "Süleymaniye Vakfı Takvimi";
            var data=new DataService();
            var takvim = data.takvim;
            schedule = new ObservableCollection<Schedule>
            {
                new Schedule
                {
                    Title = "Fecri Kazip",
                    Hour = takvim.FecriKazip,
                    State = CheckState(DateTime.Parse(takvim.FecriKazip),DateTime.Parse(takvim.FecriSadik)),
                    NotificationOn = Preferences.Get("FKNotification",false),
                    Vibration = Preferences.Get("FKVibration",false),
                    Notification = Preferences.Get("FKNotification",false)
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
                    NotificationOn = Preferences.Get("FSNotification",false),
                    Vibration = Preferences.Get("FSVibration",false),
                    Notification = Preferences.Get("FSNotification",false)
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
                    NotificationOn = Preferences.Get("SSNotification",false),
                    Vibration = Preferences.Get("SSVibration",false),
                    Notification = Preferences.Get("SSNotification",false)
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
                    NotificationOn = Preferences.Get("OGNotification",false),
                    Vibration = Preferences.Get("OGVibration",false),
                    Notification = Preferences.Get("OGNotification",false)
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
                    NotificationOn = Preferences.Get("IKNotification",false),
                    Vibration = Preferences.Get("IKVibration",false),
                    Notification = Preferences.Get("IKNotification",false)
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
                    NotificationOn = Preferences.Get("AKNotification",false),
                    Vibration = Preferences.Get("AKVibration",false),
                    Notification = Preferences.Get("AKNotification",false)
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
                    NotificationOn = Preferences.Get("YANotification",false),
                    Vibration = Preferences.Get("YAVibration",false),
                    Notification = Preferences.Get("YANotification",false)
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
                    NotificationOn = Preferences.Get("YSNotification",false),
                    Vibration = Preferences.Get("YSVibration",false),
                    Notification = Preferences.Get("YSNotification",false)
                    //NotificationType = Preferences.Get("YSNotification",0)
                    //Passed = DateTime.Now > DateTime.Parse(takvim.FecriKazip),
                    //Happening = DateTime.Now > DateTime.Parse(takvim.YatsiSonu) &&
                    //            DateTime.Now < DateTime.Parse(takvim.FecriKazip),
                    //Waiting = DateTime.Now < DateTime.Parse(takvim.YatsiSonu)
                }
            };
            ReminderEnabledChanged = new Command(ReminderSettingChanged);
            NotificationCheckedChanged = new Command(notificationSettingChanged);
        }

        private void ReminderSettingChanged(object obj)
        {
            Console.WriteLine(obj.ToString());
        }

        private string CheckState(DateTime next, DateTime current)
        {
            var state = "";
            if (DateTime.Now > next) state = "Passed";
            if (DateTime.Now > current && DateTime.Now < next) state = "Happening";
            if (DateTime.Now < current) state = "Waiting";
            return state;
        }

        public void vibrationCheckedChanged()
        {
            var theSchedule = selectedSchedule;
            switch (theSchedule.Title)
            {
                case "Fecri Kazip":Preferences.Set("FKVibration",theSchedule.Vibration);break;
                case "Fecri Sadik":Preferences.Set("FSVibration",theSchedule.Vibration);break;
                case "Saba Sonu": Preferences.Set("SSVibration", theSchedule.Vibration); break;
                case "Öğle": Preferences.Set("OGVibration", theSchedule.Vibration); break;
                case "İkindi": Preferences.Set("IKVibration", theSchedule.Vibration); break;
                case "Akşam": Preferences.Set("AKVibration", theSchedule.Vibration); break;
                case "Yatsı": Preferences.Set("YAVibration", theSchedule.Vibration); break;
                case "Yatsı Sonu": Preferences.Set("YSVibration", theSchedule.Vibration); break;
            }
        }

        public void notificationSettingChanged(object obj)
        {
            var theSchedule = selectedSchedule;
            switch (theSchedule.Title)
            {
                case "Fecri Kazip": Preferences.Set("FKNotification", theSchedule.Notification); break;
                case "Fecri Sadik": Preferences.Set("FSNotification", theSchedule.Notification); break;
                case "Saba Sonu": Preferences.Set("SSNotification", theSchedule.Notification); break;
                case "Öğle": Preferences.Set("OGNotification", theSchedule.Notification); break;
                case "İkindi": Preferences.Set("IKNotification", theSchedule.Notification); break;
                case "Akşam": Preferences.Set("AKNotification", theSchedule.Notification); break;
                case "Yatsı": Preferences.Set("YANotification", theSchedule.Notification); break;
                case "Yatsı Sonu": Preferences.Set("YSNotification", theSchedule.Notification); break;
            }
        }
    }
}
