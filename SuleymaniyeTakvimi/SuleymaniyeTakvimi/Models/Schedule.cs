using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;

namespace SuleymaniyeTakvimi.Models
{
    class Schedule:ObservableObject
    {
        private bool notificationOn;
        public string Title { get; set; }
        public string Hour { get; set; }
        public string State { get; set; }
        //public bool Passed { get; set; }
        //public bool Happening { get; set; }
        //public bool Waiting { get; set; }
        public bool NotificationOn
        {
            get => notificationOn;
            set => SetProperty(ref notificationOn, value);
        }

        //public int NotificationType { get; set; }
        public bool Vibration { get; set; }
        public bool Notification { get; set; }
    }
}
