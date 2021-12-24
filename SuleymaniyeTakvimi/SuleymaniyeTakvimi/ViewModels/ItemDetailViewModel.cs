using SuleymaniyeTakvimi.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Library;
using MediaManager.Playback;
//using Matcha.BackgroundService;
//using Plugin.LocalNotification;
using Plugin.LocalNotifications;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    [QueryProperty(nameof(ItemId), nameof(ItemId))]
    public class ItemDetailViewModel : BaseViewModel
    {
        private string itemId;
        private string itemAdi;
        private string vakit;
        private string bildirmeVakti;
        private bool titreme;
        private bool bildiri;
        private bool alarm;
        private bool etkin;
        private Sound[] sounds;
        private Sound _selectedSound;
        private string testButtonText;
        public ICommand EnableSwitchToggled { get; }
        public ICommand BildiriCheckedChanged { get; }
        public ICommand TitremeCheckedChanged { get; }
        public ICommand AlarmCheckedChanged { get; }
        public ICommand RadioButtonCheckedChanged { get; }
        public ICommand BackCommand { get; }
        public Command<Sound> SoundSelectedCommand { get; }
        public ICommand TestButtonCommand { get; }

        public string TestButtonText
        {
            get => testButtonText;
            set => SetProperty(ref testButtonText, value);
        }
        public Sound[] Sounds
        {
            get => sounds ??= new[]
            {
                new Sound() {Index = 0, fileName = "kus", Name = AppResources.KusCiviltisi},
                new Sound() {Index = 1, fileName = "horoz", Name = AppResources.HorozOtusu},
                new Sound() {Index = 2, fileName = "alarm", Name = AppResources.CalarSaat},
                new Sound() {Index = 3, fileName = "ezan", Name = AppResources.EzanSesi}
            };
            set => SetProperty(ref sounds, value);
        }

        public Sound SelectedSound
        {
            get => _selectedSound;
            set
            {
                SetProperty(ref _selectedSound, value);
                SoundSelected(value);
            }
        }

        public ItemDetailViewModel()
        {
            EnableSwitchToggled = new Command(Etkinlestir);
            BildiriCheckedChanged = new Command(BildiriAyari);
            TitremeCheckedChanged = new Command(TitremeAyari);
            AlarmCheckedChanged = new Command(AlarmAyari);
            RadioButtonCheckedChanged = new Command(BildirmeVaktiAyari);
            BackCommand = new Command(GoBack);
            //Etkin = Preferences.Get(itemId + "Etkin", false);
            //Titreme = Preferences.Get(itemId + "Titreme", false);
            //Bildiri = Preferences.Get(itemId + "Bildiri", false);
            //Alarm = Preferences.Get(itemId + "Alarm", false);
            //_selectedSound = SetSelectedSound();
            SoundSelectedCommand = new Command<Sound>(SoundSelected);
            TestButtonCommand = new Command(TestButtonClicked);
            CrossMediaManager.Current.MediaPlayer.Stop();
            testButtonText = AppResources.SesTesti;
        }

        private async void TestButtonClicked(object obj)
        {
            if (TestButtonText == AppResources.SesTesti)
            {
                IMediaItem mediaItem;
                var alarmSesi = 
                mediaItem = await CrossMediaManager.Current.PlayFromAssembly(SelectedSound.fileName + ".wav").ConfigureAwait(true);
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.RepeatMode = RepeatMode.All;
                CrossMediaManager.Current.MediaPlayer.Play(mediaItem);
                TestButtonText = AppResources.TestiDurdur;
            }
            else
            {
                CrossMediaManager.Current.MediaPlayer.Stop();
                TestButtonText = AppResources.SesTesti;
            }
        }

        public Sound SetSelectedSound()
        {
            string name = "Çalar Saat";
            int index = 2;
            string file = "alarm";
            if (itemId != null)
            {
                file = Preferences.Get(itemId + "AlarmSesi", file);
                switch (file)
                {
                    case "kus":
                        name = AppResources.KusCiviltisi;
                        index = 0;
                        break;
                    case "alarm":
                        name = AppResources.CalarSaat;
                        index = 2;
                        break;
                    case "horoz":
                        name = AppResources.HorozOtusu;
                        index = 1;
                        break;
                    case "ezan":
                        name = AppResources.EzanSesi;
                        index = 3;
                        break;
                }
            }

            return new Sound() {Index = index, fileName = file, Name = name};
        }

        private void SoundSelected(Sound sound)
        {
            if (itemId != null) Preferences.Set(itemId + "AlarmSesi", sound.fileName);
        }

        private void BildirmeVaktiAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var radiobutton = obj as RadioButton;
                Preferences.Set(itemId + "BildirmeVakti", radiobutton.Value.ToString());
                Debug.WriteLine("Value Setted for -> " + itemId + "BildirmeVakti: " +
                                  Preferences.Get(itemId + "BildirmeVakti", radiobutton.Value.ToString()));
                bildirmeVakti = radiobutton.Value.ToString();
            }
        }

        private void AlarmAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(itemId + "Alarm", value);
                Debug.WriteLine("Value Setted for -> " + itemId + "Alarm: " +
                                  Preferences.Get(itemId + "Alarm", value));
                Alarm = value;
            }
        }

        private void TitremeAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(itemId + "Titreme", value);
                Debug.WriteLine("Value Setted for -> " + itemId + "Titreme: " +
                                  Preferences.Get(itemId + "Titreme", value));
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
                        UserDialogs.Instance.Alert(AppResources.TitremeyiDesteklemiyor + ex.Message, AppResources.CihazDesteklemiyor);
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
                Preferences.Set(itemId + "Bildiri", value);
                Debug.WriteLine("Value Setted for -> " + itemId + "Bildiri: " +
                                  Preferences.Get(itemId + "Bildiri", value));
                Bildiri = value;
                var bildiriVakti = TimeSpan.Parse(Vakit) + TimeSpan.FromMinutes(Convert.ToDouble(BildirmeVakti));
                if (value)
                {
                    CrossLocalNotifications.Current.Show(AppResources.BildiriEtkinBaslik,
                        $"{itemAdi} --> {bildiriVakti} {AppResources.BildiriEtkinIcerik}", 1001);
                    CrossLocalNotifications.Current.Cancel(1002);
                    //var notification = new NotificationRequest
                    //{
                    //    NotificationId = 100,
                    //    Title = itemAdi + " Vakti Bildirimi",
                    //    Description = itemAdi + " Vakti: " + Vakit,
                    //    ReturningData = itemAdi + " Bildirimi", // Returning data when tapped on notification.
                    //    NotifyTime =  DateTime.Parse(bildiriVakti.ToString())//DateTime.Now.AddSeconds(10) // Used for Scheduling local notification, if not specified notification will show immediately.
                    //};
                    //NotificationCenter.Current.Show(notification);
                    //var notification = new Notification
                    //{
                    //    Id = new Random().Next(101,999),
                    //    Title = "Bildiri Ayarı Etkinleşti",
                    //    Message = $"{itemAdi} --> {bildiriVakti} için bildiri etkinleştirildi.",
                    //    //ScheduleDate = DateTimeOffset.Parse(bildiriVakti.ToString()),
                    //    //ScheduleDate = DateTime.Now.AddSeconds(2)
                    //};
                    //ShinyHost.Resolve<INotificationManager>().RequestAccessAndSend(notification);
                    //Task.Run(async () =>
                    //{
                    //    var notificationManager = ShinyHost.Resolve<INotificationManager>();
                    //    var state= await notificationManager.RequestAccess();
                    //    await notificationManager.Send(notification).ConfigureAwait(false);
                    //    //Console.WriteLine("Notification Message ID: " + msgId);
                    //});
                }
                else
                {
                    CrossLocalNotifications.Current.Show(AppResources.BildiriDevreDisiBaslik,
                        $"{itemAdi} --> {bildiriVakti} {AppResources.BildiriDevreDisiIcerik}", 1002);
                    CrossLocalNotifications.Current.Cancel(1001);
                }
            }
        }

        private void Etkinlestir(object obj)
        {

            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var value = (bool) obj;
                Preferences.Set(itemId + "Etkin", value);
                Debug.WriteLine("Value Setted for -> " + itemId + "Etkin: " +
                                  Preferences.Get(itemId + "Etkin", value));
                Etkin = value;
                DataService data = new DataService();
                data.SetWeeklyAlarms();
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
            }
        }

        public string ItemId
        {
            get => itemId;
            set
            {
                IsBusy = true;
                itemId = value;
                LoadItem(value);
                SelectedSound = SetSelectedSound();
                IsBusy = false;
            }
        }

        public string Vakit
        {
            get => vakit;
            set => SetProperty(ref vakit, value);
        }

        public string BildirmeVakti
        {
            get => bildirmeVakti;
            set => SetProperty(ref bildirmeVakti, value);
        }

        public bool Etkin
        {
            get => etkin;
            set => SetProperty(ref etkin, value);
        }

        public bool Titreme
        {
            get => titreme;
            set => SetProperty(ref titreme, value);
        }

        public bool Bildiri
        {
            get => bildiri;
            set => SetProperty(ref bildiri, value);
        }

        public bool Alarm
        {
            get => alarm;
            set => SetProperty(ref alarm, value);
        }

        public string ItemAdi
        {
            get => itemAdi;
            set => SetProperty(ref itemAdi, value);
        }

        public void LoadItem(string itemId)
        {
            try
            {
                switch (itemId)
                {
                    case "fecrikazip":
                        Title = itemAdi = AppResources.FecriKazip;
                        break;
                    case "fecrisadik":
                        Title = itemAdi = AppResources.FecriSadik;
                        break;
                    case "sabahsonu":
                        Title = itemAdi = AppResources.SabahSonu;
                        break;
                    case "ogle":
                        Title = itemAdi = AppResources.Ogle;
                        break;
                    case "ikindi":
                        Title = itemAdi = AppResources.Ikindi;
                        break;
                    case "aksam":
                        Title = itemAdi = AppResources.Aksam;
                        break;
                    case "yatsi":
                        Title = itemAdi = AppResources.Yatsi;
                        break;
                    case "yatsisonu":
                        Title = itemAdi = AppResources.YatsiSonu;
                        break;
                }
                Vakit = Preferences.Get(itemId, "");
                Etkin = Preferences.Get(itemId + "Etkin", false);
                Bildiri = Preferences.Get(itemId + "Bildiri", false);
                Titreme = Preferences.Get(itemId + "Titreme", false);
                Alarm = Preferences.Get(itemId + "Alarm", false);
                BildirmeVakti = Preferences.Get(itemId + "BildirmeVakti", "0");//when assign "0" for defaultValue, there always throw exception says: java.lang cannot convert boolean to string. So cheating.
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Load Item\t"+ex.Message);
            }
        }

        private void GoBack(object obj)
        {
            CrossMediaManager.Current.MediaPlayer.Stop();
            DataService data = new DataService();
            data.SetWeeklyAlarms();
            //if (Device.RuntimePlatform == Device.Android)
            //    data.SetMonthlyAlarms();
            //else data.SetWeeklyAlarms();
            Shell.Current.GoToAsync("..");
        }
    }
}
