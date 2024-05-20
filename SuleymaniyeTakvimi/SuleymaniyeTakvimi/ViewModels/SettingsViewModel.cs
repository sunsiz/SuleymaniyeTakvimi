using SuleymaniyeTakvimi.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using SuleymaniyeTakvimi.Services;
using Xamarin.CommunityToolkit.Helpers;
using Xamarin.CommunityToolkit.ObjectModel;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class SettingsViewModel : BaseViewModel
    {
        IList<Language> _supportedLanguages;
        private Language _selectedLanguage;
        public ICommand SaveAndGoBackCommand { get; }
        //public ICommand BackCommand { get; }
        public ICommand GotoSettingsCommand { get; }
        public Command RadioButtonCheckedChanged { get; }
        private bool _dark;
        private bool _foregroundServiceEnabled;
        private bool _notificationPrayerTimesEnabled;
        private bool _alwaysRenewLocationEnabled;
        private int _currentTheme;
        private int _duration;
        
        private Xamarin.Forms.PancakeView.GradientStopCollection _cardGradientStops;
        private Xamarin.Forms.PancakeView.GradientStopCollection _backgroundGradientStops;
        public Xamarin.Forms.PancakeView.GradientStopCollection CardGradientStops
        {
            get => _cardGradientStops;
            set => SetProperty(ref _cardGradientStops, value);
        }
        public Xamarin.Forms.PancakeView.GradientStopCollection BackgroundGradientStops
        {
            get => _backgroundGradientStops;
            set => SetProperty(ref _backgroundGradientStops, value);
        }

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
                new("العربية", "ar"),
                new("Azəri Türkcəsi", "az"),
                new("汉语", "zh"),
                new("Deutsch", "de"),
                new("English", "en"),
                new("فارسی", "fa"),
                new("Français", "fr"),
                new("Русский", "ru"),
                new("Türkçe", "tr"),
                new("ئۇيغۇرچە", "ug"),
                new("O'zbekcha", "uz")
            };
            Debug.WriteLine($"**** Current culture is {LocalizationResourceManager.Current.CurrentCulture.TwoLetterISOLanguageName}");
            SelectedLanguage = SupportedLanguages.FirstOrDefault(lan => lan.CI == LocalizationResourceManager.Current.CurrentCulture.TwoLetterISOLanguageName);
        }

        public bool Dark { get => _dark; private set => SetProperty(ref _dark, value); }

        public bool ForegroundServiceEnabled
        {
            get => _foregroundServiceEnabled;
            set
            {
                if (_foregroundServiceEnabled == value) return;
                SetProperty(ref _foregroundServiceEnabled, value);
                Preferences.Set("ForegroundServiceEnabled", value);
                if (!value) DependencyService.Get<IAlarmService>().StopAlarmForegroundService();
                else DependencyService.Get<IAlarmService>().StartAlarmForegroundService();
            }
        }
        public bool NotificationPrayerTimesEnabled
        {
            get => _notificationPrayerTimesEnabled;
            set
            {
                if (_notificationPrayerTimesEnabled == value) return;
                SetProperty(ref _notificationPrayerTimesEnabled, value);
                Preferences.Set("NotificationPrayerTimesEnabled", value);
            }
        }
        public bool AlwaysRenewLocationEnabled
        {
            get => _alwaysRenewLocationEnabled;
            set
            {
                if (_alwaysRenewLocationEnabled == value) return;
                SetProperty(ref _alwaysRenewLocationEnabled, value);
                Preferences.Set("AlwaysRenewLocationEnabled", value);
            }
        }

        public bool IsNecessary => !((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10) || DeviceInfo.Platform == DevicePlatform.iOS);
        public int CurrentTheme
        {
            get => _currentTheme;
            set => SetProperty(ref _currentTheme, value);
        }

        public int Duration
        {
            get => _duration;
            set
            {
                if (Duration != value)
                {
                    SetProperty(ref _duration, value);
                    Preferences.Set("AlarmDuration", value);
                }
            }
        }

        public SettingsViewModel(DataService dataService) : base(dataService)
        {
            _supportedLanguages = Enumerable.Empty<Language>().ToList();
            try
            {
                IsBusy = true;
                if (Application.Current.RequestedTheme == OSAppTheme.Dark)
                {
                    CardGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["DarkElevation16dp"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["DarkElevation12dp"], Offset = 1 }
                    };
                    BackgroundGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["AppBackgroundColorDark"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["Primary"], Offset = 1 }
                    };
                }
                else
                {
                    CardGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["CardBackgroundLight"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["CardBackgroundLightAccent"], Offset = 1 }
                    };
                    BackgroundGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = Color.AliceBlue, Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = Color.WhiteSmoke, Offset = 1 }
                    };
                }
                RadioButtonCheckedChanged = new Command(PerformRadioButtonCheckedChanged);
                var ci = Preferences.Get("SelectedLanguage", "en");
                var name = CultureInfo.GetCultureInfo(ci).NativeName;
                Debug.WriteLine($"**** Selected language name is {name}, CI is {ci}");
                LoadLanguages();
                Debug.WriteLine($"**** Selected Language is {SelectedLanguage.Name} - {SelectedLanguage.CI}");
                Debug.WriteLine($"**** Saved Selected Language is {Preferences.Get("SelectedLanguage", SelectedLanguage.CI)}");
                SelectedLanguage = SupportedLanguages.FirstOrDefault(l => l.CI == ci);
                CurrentTheme = Application.Current.RequestedTheme == OSAppTheme.Dark ? 0 : 1;
                SaveAndGoBackCommand = CommandFactory.Create(() =>
                {
                    var currentLanguage = Preferences.Get("SelectedLanguage", "en");
                    if (SelectedLanguage.CI != currentLanguage)
                    {
                        LocalizationResourceManager.Current.CurrentCulture = CultureInfo.GetCultureInfo(SelectedLanguage.CI);
                        Preferences.Set("SelectedLanguage", SelectedLanguage.CI);
                        LoadLanguages();
                    }
                    GoBack();
                });
                //BackCommand = new Command(GoBack);
                GotoSettingsCommand = new Command(AppInfo.ShowSettingsUI);
                _duration = Preferences.Get("AlarmDuration", 4);
                _foregroundServiceEnabled = Preferences.Get("ForegroundServiceEnabled", true);
                _notificationPrayerTimesEnabled = Preferences.Get("NotificationPrayerTimesEnabled", false);
                _alwaysRenewLocationEnabled = Preferences.Get("AlwaysRenewLocationEnabled", false);
                Debug.WriteLine($"**** Selected language at finish is {SelectedLanguage.Name}");
                IsBusy = false;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"**** Exception thrown in the SettingsViewModel constructor: {ex.Message}");
            }
        }

        private void PerformRadioButtonCheckedChanged(object obj)
        {
            if (!IsBusy)
            {
                var radiobutton = obj as RadioButton;
                Theme.Tema = Convert.ToInt32(radiobutton?.Value.ToString());
                Dark = radiobutton?.Value.ToString() == "0";
                Application.Current.UserAppTheme = Dark ? OSAppTheme.Dark : OSAppTheme.Light;
                if (Dark)
                {
                    CardGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["DarkElevation16dp"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["DarkElevation12dp"], Offset = 1 }
                    };
                    BackgroundGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["AppBackgroundColorDark"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["Primary"], Offset = 1 }
                    };
                }
                else
                {
                    CardGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["CardBackgroundLight"], Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = (Color)Application.Current.Resources["CardBackgroundLightAccent"], Offset = 1 }
                    };
                    BackgroundGradientStops = new Xamarin.Forms.PancakeView.GradientStopCollection
                    {
                        new Xamarin.Forms.PancakeView.GradientStop { Color = Color.AliceBlue, Offset = 0 },
                        new Xamarin.Forms.PancakeView.GradientStop { Color = Color.WhiteSmoke, Offset = 1 }
                    };
                }
            }
        }

        private void GoBack()
        {
            Application.Current.UserAppTheme = Theme.Tema == 1 ? OSAppTheme.Light : OSAppTheme.Dark;
            Shell.Current.GoToAsync("..");
        }
    }
}