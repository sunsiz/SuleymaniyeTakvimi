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
        private string _itemAdi;
        private string _vakit;
        private int _bildirmeVakti;
        private bool _titreme;
        private bool _bildiri;
        private bool _alarm;
        private bool _etkin;
        private ObservableCollection<Sound> _availableSounds = new ObservableCollection<Sound>(Enumerable.Empty<Sound>());
        private Sound _selectedSound;
        private string _testButtonText;
        private bool _isPlaying;
        //private int _alarmRepeats;
        public ICommand EnableSwitchToggled { get; }
        public ICommand BildiriCheckedChanged { get; }
        public ICommand TitremeCheckedChanged { get; }
        public ICommand AlarmCheckedChanged { get; }
        //public ICommand RadioButtonCheckedChanged { get; }
        public ICommand BackCommand { get; }
        public ICommand TestButtonCommand { get; }
        //private Command<Sound> SoundSelectedCommand { get; }

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
            //SoundSelected(value);
        }

        //public int AlarmRepeats
        //{
        //    get => _alarmRepeats;
        //    set
        //    {
        //        SetProperty(ref _alarmRepeats, value);
        //        Preferences.Set(ItemId + "AlarmTekrarlari", value);
        //    }
        //}
        public bool IsNecessary => !((DeviceInfo.Platform == DevicePlatform.Android && DeviceInfo.Version.Major >= 10) || DeviceInfo.Platform == DevicePlatform.iOS);

        public ItemDetailViewModel()
        {
            Title = AppResources.PageTitle;
            EnableSwitchToggled = new Command(Etkinlestir);
            BildiriCheckedChanged = new Command(BildiriAyari);
            TitremeCheckedChanged = new Command(TitremeAyari);
            AlarmCheckedChanged = new Command(AlarmAyari);
            //RadioButtonCheckedChanged = new Command(BildirmeVaktiAyari);
            BackCommand = new Command(GoBack);
            LoadSounds();
            //Etkin = Preferences.Get(itemId + "Etkin", false);
            //Titreme = Preferences.Get(itemId + "Titreme", false);
            //Bildiri = Preferences.Get(itemId + "Bildiri", false);
            //Alarm = Preferences.Get(itemId + "Alarm", false);
            //_selectedSound = SetSelectedSound();
            //SoundSelectedCommand = new Command<Sound>(SoundSelected);
            TestButtonCommand = new Command(TestButtonClicked);
            CrossMediaManager.Current.MediaPlayer.Stop();
            //_testButtonText = "&#xe038;"; //AppResources.SesTesti;
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
                //new Sound() { FileName = "alarm2", Name = AppResources.CalarSaat + " 1" }, /*Index = 4, */
                //new Sound() { FileName = "beep1", Name = AppResources.CalarSaat + " 2" }, /*Index = 5, */
                //new Sound() { FileName = "beep2", Name = AppResources.CalarSaat + " 3" }, /*Index = 6, */
                //new Sound() { FileName = "beep3", Name = AppResources.CalarSaat + " 4" } /*Index = 7, */
            };
            //if (Device.RuntimePlatform == Device.Android) { _availableSounds.Add(new Sound(fileName: "ezan", name: AppResources.EzanSesi)); }
            //string name = "Çalar Saat";
            //int index = 2;
            string file = "alarm";
            if (_itemId != null)
            {
                file = Preferences.Get(_itemId + "AlarmSesi", file);
                //switch (file)
                //{
                //    case "kus":
                //        name = AppResources.KusCiviltisi;
                //        //index = 0;
                //        break;
                //    case "alarm":
                //        name = AppResources.CalarSaat;
                //        //index = 2;
                //        break;
                //    case "horoz":
                //        name = AppResources.HorozOtusu;
                //        //index = 1;
                //        break;
                //    case "ezan":
                //        name = AppResources.EzanSesi;
                //        //index = 3;
                //        break;
                //}
            }

            SelectedSound = AvailableSounds.FirstOrDefault(n => n.FileName == file);
            //new Sound() { FileName = file, Name = name };/*Index = index, */
        }

        private async void TestButtonClicked(object obj)
        {
            if (!IsPlaying)//TestButtonText == "&#xe038;"AppResources.SesTesti
            {
                var mediaItem = await CrossMediaManager.Current.PlayFromAssembly(SelectedSound.FileName + ".mp3");
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                await CrossMediaManager.Current.MediaPlayer.Play(mediaItem).ConfigureAwait(false);
                //TestButtonText = "&#xe036;"; //AppResources.TestiDurdur;
                IsPlaying = true;
            }
            else
            {
                await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
                //TestButtonText = "&#xe038;"; //AppResources.SesTesti;
                IsPlaying = false;
            }
        }

        //private void BildirmeVaktiAyari(object obj)
        //{
        //    //When page load the obj value awlways be false, so avoiding it.
        //    //if (!IsBusy)
        //    //{
        //    //    var radiobutton = obj as RadioButton;
        //    //    Preferences.Set(_itemId + "BildirmeVakti", radiobutton?.Value.ToString());
        //    //    Debug.WriteLine("Value Set for -> " + _itemId + "BildirmeVakti: " +
        //    //                      Preferences.Get(_itemId + "BildirmeVakti", radiobutton?.Value.ToString()));
        //    //    _bildirmeVakti = radiobutton?.Value.ToString();
        //    //}
        //}

        private void AlarmAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(_itemId + "Alarm", value);
                Debug.WriteLine("Value Set for -> " + _itemId + "Alarm: " +
                                  Preferences.Get(_itemId + "Alarm", value));
                Alarm = value;
            }
        }

        private void TitremeAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(_itemId + "Titreme", value);
                Debug.WriteLine("Value Set for -> " + _itemId + "Titreme: " +
                                  Preferences.Get(_itemId + "Titreme", value));
                Titreme = value;
                if (value)
                {
                    try
                    {
                        // Use default vibration length
                        Vibration.Vibrate();

                        // Or use specified time
                        var duration = TimeSpan.FromSeconds(1);
                        Vibration.Vibrate(duration);
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
            }
        }

        private void BildiriAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(_itemId + "Bildiri", value);
                Debug.WriteLine("Value Set for -> " + _itemId + "Bildiri: " +
                                  Preferences.Get(_itemId + "Bildiri", value));
                Bildiri = value;
                //var bildiriVakti = TimeSpan.Parse(Vakit) + TimeSpan.FromMinutes(Convert.ToDouble(BildirmeVakti));
                //if (value)
                //{
                //    var notification = new NotificationRequest
                //    {
                //        NotificationId = 1001,
                //        Title = AppResources.BildiriEtkinBaslik,
                //        Description = $"{_itemAdi} --> {bildiriVakti} {AppResources.BildiriEtkinIcerik}"
                //    };
                //    NotificationCenter.Current.Show(notification);
                //    NotificationCenter.Current.Cancel(1002);
                //    //CrossLocalNotifications.Current.Show(AppResources.BildiriEtkinBaslik,
                //    //    $"{_itemAdi} --> {bildiriVakti} {AppResources.BildiriEtkinIcerik}", 1001);
                //    //CrossLocalNotifications.Current.Cancel(1002);
                //    //var notification = new NotificationRequest
                //    //{
                //    //    NotificationId = 100,
                //    //    Title = itemAdi + " Vakti Bildirimi",
                //    //    Description = itemAdi + " Vakti: " + Vakit,
                //    //    ReturningData = itemAdi + " Bildirimi", // Returning data when tapped on notification.
                //    //    NotifyTime =  DateTime.Parse(bildiriVakti.ToString())//DateTime.Now.AddSeconds(10) // Used for Scheduling local notification, if not specified notification will show immediately.
                //    //};
                //    //NotificationCenter.Current.Show(notification);
                //    //var notification = new Notification
                //    //{
                //    //    Id = new Random().Next(101,999),
                //    //    Title = "Bildiri Ayarı Etkinleşti",
                //    //    Message = $"{itemAdi} --> {bildiriVakti} için bildiri etkinleştirildi.",
                //    //    //ScheduleDate = DateTimeOffset.Parse(bildiriVakti.ToString()),
                //    //    //ScheduleDate = DateTime.Now.AddSeconds(2)
                //    //};
                //    //ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
                //    //Task.Run(async () =>
                //    //{
                //    //    var notificationManager = ShinyHost.Resolve<INotificationManager>();
                //    //    var state= await notificationManager.RequestAccess();
                //    //    await notificationManager.Send(notification).ConfigureAwait(false);
                //    //    //Console.WriteLine("Notification Message ID: " + msgId);
                //    //});
                //}
                //else
                //{
                //    //CrossLocalNotifications.Current.Show(AppResources.BildiriDevreDisiBaslik,
                //    //    $"{_itemAdi} --> {bildiriVakti} {AppResources.BildiriDevreDisiIcerik}", 1002);
                //    //CrossLocalNotifications.Current.Cancel(1001);
                //    var notification = new NotificationRequest
                //    {
                //        NotificationId = 1002,
                //        Title = AppResources.BildiriDevreDisiBaslik,
                //        Description = $"{_itemAdi} --> {bildiriVakti} {AppResources.BildiriDevreDisiIcerik}"
                //    };
                //    NotificationCenter.Current.Show(notification);
                //    NotificationCenter.Current.Cancel(1001);
                //}
            }
        }

        private void Etkinlestir(object obj)
        {

            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
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
            
                //DataService data = new DataService();
                //data.SetWeeklyAlarms();
                //if (Device.RuntimePlatform == Device.Android)
                //    data.SetMonthlyAlarms();
                //else data.SetWeeklyAlarms();
                //if(value && BackgroundAggregatorService.Instance==null)
                //{
                //    BackgroundAggregatorService.Add(() => new ReminderService(60));
                //    BackgroundAggregatorService.StartBackgroundService();
                //}
                //if (value)
                //{
                //    startServiceIntent = new Intent(,typeof(ForegroundService));
                //    startServiceIntent.SetAction("SuleymaniyeTakvimi.action.START_SERVICE");
                //    StartService(startServiceIntent);
                //}
                //if (value && !Preferences.Get(_itemId + "Bildiri", false) && !Preferences.Get(_itemId + "Titreme", false) && !Preferences.Get(_itemId + "Alarm", false))
                //{
                //    SetProperty(ref _alarm, true);
                //    Preferences.Set(_itemId + "Alarm", true);
                //}
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
                //SelectedSound = SetSelectedSound();
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

        //public string ItemAdi
        //{
        //    get => _itemAdi;
        //    set => SetProperty(ref _itemAdi, value);
        //}

        private void LoadItem(string itemId)
        {
            try
            {
                switch (itemId)
                {
                    case "fecrikazip":
                        Title = _itemAdi = AppResources.FecriKazip;
                        break;
                    case "fecrisadik":
                        Title = _itemAdi = AppResources.FecriSadik;
                        break;
                    case "sabahsonu":
                        Title = _itemAdi = AppResources.SabahSonu;
                        break;
                    case "ogle":
                        Title = _itemAdi = AppResources.Ogle;
                        break;
                    case "ikindi":
                        Title = _itemAdi = AppResources.Ikindi;
                        break;
                    case "aksam":
                        Title = _itemAdi = AppResources.Aksam;
                        break;
                    case "yatsi":
                        Title = _itemAdi = AppResources.Yatsi;
                        break;
                    case "yatsisonu":
                        Title = _itemAdi = AppResources.YatsiSonu;
                        break;
                }
                Vakit = Preferences.Get(itemId, "");
                Etkin = Preferences.Get(itemId + "Etkin", false);
                Bildiri = Preferences.Get(itemId + "Bildiri", true);
                Titreme = Preferences.Get(itemId + "Titreme", true);
                Alarm = Preferences.Get(itemId + "Alarm", false);
                BildirmeVakti = Preferences.Get(itemId + "BildirmeVakti", 0);//when assign "0" for defaultValue, there always throw exception says: java.lang cannot convert boolean to string. So cheating.
                //AlarmRepeats = Preferences.Get(itemId + "AlarmTekrarlari", 1);
                //string name = "Çalar Saat";
                ////int index = 2;
                //string file = "alarm";
                //if (_itemId != null)
                //{
                //    file = Preferences.Get(_itemId + "AlarmSesi", file);
                //    switch (file)
                //    {
                //        case "kus":
                //            name = AppResources.KusCiviltisi;
                //            //index = 0;
                //            break;
                //        case "alarm":
                //            name = AppResources.CalarSaat;
                //            //index = 2;
                //            break;
                //        case "horoz":
                //            name = AppResources.HorozOtusu;
                //            //index = 1;
                //            break;
                //        case "ezan":
                //            name = AppResources.EzanSesi;
                //            //index = 3;
                //            break;
                //    }
                //}

                //SelectedSound = new Sound() { FileName = file, Name = name };/*Index = index, */
                string file = "alarm";
                if (_itemId != null)
                {
                    file = Preferences.Get(_itemId + "AlarmSesi", file);
                    SelectedSound = AvailableSounds.FirstOrDefault(n => n.FileName == file);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Load Item\t"+ex.Message);
            }
        }

        public void GoBack(object obj)
        {
	        Device.BeginInvokeOnMainThread(async () =>
	        {
		        try
		        {
			        using (UserDialogs.Instance.Loading(AppResources.AlarmlarPlanlaniyor))
			        {
				        await CrossMediaManager.Current.MediaPlayer.Stop().ConfigureAwait(false);
				        if (_itemId != null && _selectedSound != null)
					        Preferences.Set(_itemId + "AlarmSesi", _selectedSound.FileName);
				        await Task.Delay(1000);
				        var data = new DataService();
				        data.SetWeeklyAlarms();
			        }
		        }
		        catch (Exception ex)
		        {
			        var val = ex.Message;
			        UserDialogs.Instance.Alert("Test", val.ToString(), "Ok");
		        }
	        });	
						Shell.Current.GoToAsync("..");
            //using (UserDialogs.Instance.Loading(AppResources.AlarmlarPlanlaniyor,null,null,true,MaskType.Gradient))
            //{
                //DataService data = new DataService();
                //data.SetWeeklyAlarms();
                //if (Device.RuntimePlatform == Device.Android)
                //    data.SetMonthlyAlarms();
                //else data.SetWeeklyAlarms();
            //}
        }

        //private Sound SetSelectedSound()
        //{
        //    string name = "Çalar Saat";
        //    int index = 2;
        //    string file = "alarm";
        //    if (_itemId != null)
        //    {
        //        file = Preferences.Get(_itemId + "AlarmSesi", file);
        //        switch (file)
        //        {
        //            case "kus":
        //                name = AppResources.KusCiviltisi;
        //                index = 0;
        //                break;
        //            case "alarm":
        //                name = AppResources.CalarSaat;
        //                index = 2;
        //                break;
        //            case "horoz":
        //                name = AppResources.HorozOtusu;
        //                index = 1;
        //                break;
        //            case "ezan":
        //                name = AppResources.EzanSesi;
        //                index = 3;
        //                break;
        //        }
        //    }

        //    return new Sound() {Index = index, FileName = file, Name = name};
        ////}

        //private void SoundSelected(Sound sound)
        //{
        //    if (_itemId != null) Preferences.Set(_itemId + "AlarmSesi", sound.FileName);
        //}
    }
}
