using SuleymaniyeTakvimi.Models;
using System;
using System.Diagnostics;
using System.Windows.Input;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Playback;
//using Matcha.BackgroundService;
//using Plugin.LocalNotification;
//using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
//using Plugin.LocalNotification;

namespace SuleymaniyeTakvimi.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string _itemId;
        private string _vakit;
        private int _bildirmeVakti;
        private bool _titreme;
        private bool _bildiri;
        private bool _alarm;
        private bool _etkin;
        private ObservableCollection<Sound> _availableSounds;
        private Sound _selectedSound;
        private string _testButtonText;
        private bool _isPlaying;
        public ICommand EnableSwitchToggled { get; }
        public ICommand NotificationCheckedChanged { get; }
        public ICommand VibrationCheckedChanged { get; }
        public ICommand AlarmCheckedChanged { get; }
        public ICommand BackCommand { get; }
        public ICommand TestButtonCommand { get; }

        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public string TestButtonText
        {
            get => _testButtonText;
            set => SetProperty(ref _testButtonText, value);
        }
        public ObservableCollection<Sound> AvailableSounds
        {
            get => _availableSounds;
            set => SetProperty(ref _availableSounds, value);
        }

        public Sound SelectedSound
        {
            get => _selectedSound;
            set => SetProperty(ref _selectedSound, value);
        }
        
        public bool IsNecessary => !((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10) || DeviceInfo.Platform == DevicePlatform.iOS);

        public ItemDetailViewModel(DataService dataService):base(dataService)
        {
            Title = AppResources.PageTitle;
            EnableSwitchToggled = new Command(Activate);
            NotificationCheckedChanged = new Command(NotificationSetting);
            VibrationCheckedChanged = new Command(VibrationSetting);
            AlarmCheckedChanged = new Command(AlarmSetting);
            BackCommand = new Command(async (obj) => await GoBack(obj));
            LoadSounds();
            TestButtonCommand = new Command(TestButtonClicked);
            CrossMediaManager.Current.MediaPlayer.Stop();
            IsPlaying = false;
        }

        private void LoadSounds()
        {
            _availableSounds = new ObservableCollection<Sound>()
            {
                new Sound(fileName: "kus", name: AppResources.KusCiviltisi), /*Index = 0, */
                new Sound(fileName: "horoz", name: AppResources.HorozOtusu), /*Index = 1, */
                new Sound(fileName: "alarm", name: AppResources.CalarSaat), /*Index = 2, */
                new Sound(fileName: "ezan", name: AppResources.EzanSesi), /*Index = 3, */
                new Sound(fileName: "alarm2", name: AppResources.CalarSaat + " 1"), /*Index = 4, */
                new Sound(fileName: "beep1", name: AppResources.CalarSaat + " 2"), /*Index = 5, */
                new Sound(fileName: "beep2", name: AppResources.CalarSaat + " 3"), /*Index = 6, */
                new Sound(fileName: "beep3", name: AppResources.CalarSaat + " 4") /*Index = 7, */
            };
            string file = Preferences.Get(_itemId + "AlarmSesi", "alarm");

            SelectedSound = AvailableSounds.FirstOrDefault(n => n.FileName == file);
        }

        private async void TestButtonClicked(object obj)
        {
            if (!IsPlaying)
            {
                var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(SelectedSound.FileName + ".mp3");
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                await CrossMediaManager.Current.MediaPlayer.Play(mediaItem).ConfigureAwait(false);
                IsPlaying = true;
            }
            else
            {
                await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
                IsPlaying = false;
            }
        }

        private void AlarmSetting(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (IsBusy) return;
            var value = (bool) obj;
            Preferences.Set(_itemId + "Alarm", value);
            Debug.WriteLine("Value Set for -> " + _itemId + "Alarm: " +
                            Preferences.Get(_itemId + "Alarm", value));
            Alarm = value;
        }

        private void VibrationSetting(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (IsBusy) return;
            var value = (bool) obj;
            Preferences.Set(_itemId + "Titreme", value);
            Debug.WriteLine("Value Set for -> " + _itemId + "Titreme: " +
                            Preferences.Get(_itemId + "Titreme", value));
            Titreme = value;
            if (!value) return;
            try
            {
                // Use specified  vibration length
                Vibration.Vibrate(TimeSpan.FromSeconds(2));
            }
            catch (FeatureNotSupportedException ex)
            {
                UserDialogs.Instance.Alert(AppResources.TitremeyiDesteklemiyor + ex.Message, AppResources.CihazTitretmeyiDesteklemiyor);
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, AppResources.SorunCikti);
            }
        }

        private void NotificationSetting(object obj)
        {
            //When page load the obj value always be false, so avoiding it.
            if (IsBusy) return;
            var value = (bool) obj;
            Preferences.Set(_itemId + "Bildiri", value);
            Debug.WriteLine("Value Set for -> " + _itemId + "Bildiri: " +
                            Preferences.Get(_itemId + "Bildiri", value));
            Bildiri = value;
        }

        private void Activate(object obj)
        {

            //When page load the obj value awlways be false, so avoiding it.
            if (IsBusy) return;
            var value = (bool)obj;
            Preferences.Set(_itemId + "Etkin", value);
            Debug.WriteLine("Value Set for -> " + _itemId + "Etkin: " +
                            Preferences.Get(_itemId + "Etkin", value));
            Etkin = value;
            if (value)
            {
                Preferences.Set(_itemId + "Bildiri", Preferences.Get(_itemId + "Bildiri", true));
                Preferences.Set(_itemId + "Titreme", Preferences.Get(_itemId + "Titreme", true));
                Preferences.Set(_itemId + "Alarm", Preferences.Get(_itemId + "Alarm", false));
            }
            else
            {
                Preferences.Set(_itemId + "Bildiri", false);
                Preferences.Set(_itemId + "Titreme", false);
                Preferences.Set(_itemId + "Alarm", false);
            }
        }

        public string ItemId
        {
            get => _itemId;
            set
            {
                IsBusy = true;
                _itemId = value;
                LoadItem(value);
                IsBusy = false;
            }
        }

        public string Vakit
        {
            get => _vakit;
            set => SetProperty(ref _vakit, value);
        }

        public int BildirmeVakti
        {
            get => _bildirmeVakti;
            set
            {
                SetProperty(ref _bildirmeVakti, value);
                Preferences.Set(ItemId + "BildirmeVakti", _bildirmeVakti);
            }
        }

        public bool Etkin
        {
            get => _etkin;
            set => SetProperty(ref _etkin, value);
        }

        public bool Titreme
        {
            get => _titreme;
            set => SetProperty(ref _titreme, value);
        }

        public bool Bildiri
        {
            get => _bildiri;
            set => SetProperty(ref _bildiri, value);
        }

        public bool Alarm
        {
            get => _alarm;
            set => SetProperty(ref _alarm, value);
        }

        private void LoadItem(string itemId)
        {
            try
            {
                Title = itemId switch
                {
                    "fecrikazip" => AppResources.FecriKazip,
                    "fecrisadik" => AppResources.FecriSadik,
                    "sabahsonu" => AppResources.SabahSonu,
                    "ogle" => AppResources.Ogle,
                    "ikindi" => AppResources.Ikindi,
                    "aksam" => AppResources.Aksam,
                    "yatsi" => AppResources.Yatsi,
                    "yatsisonu" => AppResources.YatsiSonu,
                    _ => Title
                };
                Vakit = Preferences.Get(itemId, "");
                Etkin = Preferences.Get(itemId + "Etkin", false);
                Bildiri = Preferences.Get(itemId + "Bildiri", true);
                Titreme = Preferences.Get(itemId + "Titreme", true);
                Alarm = Preferences.Get(itemId + "Alarm", false);
                BildirmeVakti = Preferences.Get(itemId + "BildirmeVakti", 0);//when assign "0" for defaultValue, there always throw exception says: java.lang cannot convert boolean to string. So cheating.
                
                string file = _itemId != null ? Preferences.Get(_itemId + "AlarmSesi", "alarm") : "alarm";
                SelectedSound = AvailableSounds.FirstOrDefault(n => n.FileName == file);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to Load Item\t{ex.Message}");
            }
        }

        public async Task GoBack(object obj)
        {
            try
            {
                using (UserDialogs.Instance.Loading(AppResources.AlarmlarPlanlaniyor))
                {
                    await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
                    if (_itemId != null && _selectedSound != null)
                    {
                        Preferences.Set(_itemId + "AlarmSesi", _selectedSound.FileName);
                    }

                    await Task.Delay(1000);
                    await DataService.SetWeeklyAlarmsAsync();
                }
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message, AppResources.AlarmHatasi, AppResources.Tamam);
            }

            await Shell.Current.GoToAsync("..");
        }
    }
}
