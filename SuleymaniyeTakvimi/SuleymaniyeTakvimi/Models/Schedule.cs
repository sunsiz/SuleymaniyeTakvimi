using System;
using System.Collections.Generic;
using System.Text;
using MvvmHelpers;

namespace SuleymaniyeTakvimi.Models
{
    class Schedule:ObservableObject
    {
        private bool reminderOn;
        public string Title { get; set; }
        public string Hour { get; set; }
        public string State { get; set; }
        public bool ReminderOn
        {
            get => reminderOn;
            set => SetProperty(ref reminderOn, value);
        }

        public bool Vibration { get; set; }
        public bool Notification { get; set; }
        public bool Alarm { get; set; }
    }
}
