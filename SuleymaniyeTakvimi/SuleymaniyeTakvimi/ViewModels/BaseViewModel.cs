using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public ITakvimData TakvimData => DependencyService.Get<ITakvimData>();

        bool isBusy = false;
        public bool IsBusy
        {
            get { return isBusy; }
            set { SetProperty(ref isBusy, value); }
        }

        string title = string.Empty;
        public string Title
        {
            get { return title; }
            set { SetProperty(ref title, value); }
        }
        Takvim vakitler;

        public Takvim Vakitler
        {
            get
            {
                if (vakitler == null)
                {
                    isBusy = true;
                    var data = new TakvimData();
                    vakitler = data.takvim;
                    isBusy = false;
                }

                return vakitler;
            }
            set { SetProperty(ref vakitler, value); }
        }
        //public Takvim Vakitler { get; set; }
        protected bool SetProperty<T>(ref T backingStore, T value,
            [CallerMemberName] string propertyName = "",
            Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
