using MvvmHelpers;

namespace SuleymaniyeTakvimi.Models
{
    class Schedule:ObservableObject
    {
        private bool _reminderOn;
        public string Title { get; set; }
        public string Hour { get; set; }
        public string State { get; set; }
        public bool ReminderOn
        {
            get => _reminderOn;
            set => SetProperty(ref _reminderOn, value);
        }

        public bool Vibration { get; set; }
        public bool Notification { get; set; }
        public bool Alarm { get; set; }
    }
}
