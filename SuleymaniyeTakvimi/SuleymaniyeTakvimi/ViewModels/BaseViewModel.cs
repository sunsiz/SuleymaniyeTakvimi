using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class BaseViewModel : MvvmHelpers.BaseViewModel, INotifyPropertyChanged
    {
        protected readonly DataService DataService;

        protected BaseViewModel(DataService dataService)
        {
            DataService = dataService;
        }

        //private bool _isBusy;

        //public bool IsBusy
        //{
        //    get => _isBusy;
        //    set => SetProperty(ref _isBusy, value);
        //}

        //string _title = string.Empty;

        //public string Title
        //{
        //    get => _title;
        //    set => SetProperty(ref _title, value);
        //}

        private int _fontSize = Preferences.Get("FontSize", 14);
        public int FontSize
        {
            get => _fontSize;
            set
            {
                SetProperty(ref _fontSize, value);
                Preferences.Set("FontSize", value);
            }
        }
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
        //public event PropertyChangedEventHandler PropertyChanged;

        //protected override void OnPropertyChanged([CallerMemberName] string propertyName = "")
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        #endregion
    }
}
