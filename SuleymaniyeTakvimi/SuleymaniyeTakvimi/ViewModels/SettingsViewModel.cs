﻿using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class SettingsViewModel:BaseViewModel
    {
        IList<Language> _supportedLanguages = Enumerable.Empty<Language>().ToList();
        private Language _selectedLanguage = new Language(AppResources.English, "en");
        public ICommand ChangeLanguageCommand { get; }
        public ICommand BackCommand { get; }
        public ICommand GotoSettingsCommand { get; }
        public Command RadioButtonCheckedChanged { get; }
        private bool _dark;
        private bool _foregroundServiceEnabled;
        private bool _notificationPrayerTimesEnabled;
        private bool _alwaysRenewLocationEnabled;
        private int _currentTheme;
        private int _alarmDuration;

        public Language SelectedLanguage
        {
            get => _selectedLanguage;
            set => SetProperty(ref _selectedLanguage, value);
        }

        public IList<Language> SupportedLanguages
        {
            get => _supportedLanguages;
            private set => SetProperty(ref _supportedLanguages, value);
        }

        void LoadLanguages()
        {
            SupportedLanguages = new List<Language>()
            {
                new Language(AppResources.Arabic, "ar"),
                new Language(AppResources.Azerbaijani, "az"),
                new Language(AppResources.Chinese, "zh"),
                new Language(AppResources.Deutsch, "de"),
                new Language(AppResources.English, "en"),
                new Language(AppResources.Farsi, "fa"),
                new Language(AppResources.French, "fr"),
                //{ new Language(AppResources.Kyrgyz, "ky") },
                new Language(AppResources.Russian, "ru"),
                new Language(AppResources.Turkish, "tr"),
                new Language(AppResources.Uyghur, "ug"),
                { new Language(AppResources.Uzbek, "uz") }
            };
            SelectedLanguage = SupportedLanguages.FirstOrDefault(lan => lan.CI == LocalizationResourceManager.Current.CurrentCulture.TwoLetterISOLanguageName);
        }

        public bool Dark { get=>_dark; private set=>SetProperty(ref _dark,value); }

        public bool ForegroundServiceEnabled
        {
            get => _foregroundServiceEnabled;
            set
            {
                if (_foregroundServiceEnabled!=value)
                {
                    SetProperty(ref _foregroundServiceEnabled, value);
                    Preferences.Set("ForegroundServiceEnabled", value);
                    if(!value)DependencyService.Get<IAlarmService>().StopAlarmForegroundService();
                    else DependencyService.Get<IAlarmService>().StartAlarmForegroundService();
                }
            }
        }
        public bool NotificationPrayerTimesEnabled
        {
            get => _notificationPrayerTimesEnabled;
            set
            {
                if (_notificationPrayerTimesEnabled!=value)
                {
                    SetProperty(ref _notificationPrayerTimesEnabled, value);
                    Preferences.Set("NotificationPrayerTimesEnabled", value);
                }
            }
        }
        public bool AlwaysRenewLocationEnabled
        {
            get => _alwaysRenewLocationEnabled;
            set
            {
                if (_alwaysRenewLocationEnabled!=value)
                {
                    SetProperty(ref _alwaysRenewLocationEnabled, value);
                    Preferences.Set("AlwaysRenewLocationEnabled", value);
                }
            }
        }

        public bool IsNecessary => !((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10) || DeviceInfo.Platform==DevicePlatform.iOS);
        public int CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        public int AlarmDuration
        {
            get => _alarmDuration;
            set
            {
                if (AlarmDuration!=value)
                {
                    SetProperty(ref _alarmDuration, value);
                    Preferences.Set("AlarmDuration", value);
                }
            }
        }

        public SettingsViewModel()
        {
            IsBusy = true;
            RadioButtonCheckedChanged = new Command(PerformRadioButtonCheckedChanged);
            LoadLanguages();
            CurrentTheme = Application.Current.RequestedTheme == OSAppTheme.Dark ? 0 : 1;
            ChangeLanguageCommand = CommandFactory.Create(() =>
            {
                LocalizationResourceManager.Current.CurrentCulture = CultureInfo.GetCultureInfo(SelectedLanguage.CI);
                Preferences.Set("SelectedLanguage", SelectedLanguage.CI);
                LoadLanguages();
                GoBack(null);
            });
            BackCommand = new Command(GoBack);
            GotoSettingsCommand = new Command(() => { AppInfo.ShowSettingsUI(); });
            _alarmDuration = Preferences.Get("AlarmDuration", 4);
            _foregroundServiceEnabled = Preferences.Get("ForegroundServiceEnabled", true);
            _notificationPrayerTimesEnabled = Preferences.Get("NotificationPrayerTimesEnabled", false);
            _alwaysRenewLocationEnabled = Preferences.Get("AlwaysRenewLocationEnabled", false);
            IsBusy = false;
        }
        
        private void PerformRadioButtonCheckedChanged(object obj)
        {
            if (!IsBusy)
            {
                var radiobutton = obj as RadioButton;
                Theme.Tema = Convert.ToInt32(radiobutton?.Value.ToString());
                Dark = radiobutton?.Value.ToString() == "0";
            }
        }

        private void GoBack(object obj)
        {
            Application.Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
			Shell.Current.GoToAsync("..");
        }
    }
}