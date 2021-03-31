using SuleymaniyeTakvimi.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using Matcha.BackgroundService;
using Plugin.LocalNotification;
using Plugin.LocalNotifications;
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
        public ICommand SwitchToggled { get; private set; }
        public ICommand BildiriCheckedChanged { get; private set; }
        public ICommand TitremeCheckedChanged { get; private set; }
        public ICommand AlarmCheckedChanged { get; private set; }
        public ICommand RadioButtonCheckedChanged { get; private set; }

        public ItemDetailViewModel()
        {
            SwitchToggled = new Command(Etkinlestir);
            BildiriCheckedChanged = new Command(BildiriAyari);
            TitremeCheckedChanged = new Command(TitremeAyari);
            AlarmCheckedChanged = new Command(AlarmAyari);
            RadioButtonCheckedChanged = new Command(BildirmeVaktiAyari);
            //Etkin = Preferences.Get(itemId + "Etkin", false);
            //Titreme = Preferences.Get(itemId + "Titreme", false);
            //Bildiri = Preferences.Get(itemId + "Bildiri", false);
            //Alarm = Preferences.Get(itemId + "Alarm", false);
        }

        private void BildirmeVaktiAyari(object obj)
        {
            //When page load the obj value awlways be false, so avoiding it.
            if (!IsBusy)
            {
                var radiobutton = obj as RadioButton;
                Preferences.Set(itemId + "BildirmeVakti", radiobutton.Value.ToString());
                Console.WriteLine("Value Setted for -> " + itemId + "BildirmeVakti: " +
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
                Console.WriteLine("Value Setted for -> " + itemId + "Alarm: " +
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
                Console.WriteLine("Value Setted for -> " + itemId + "Titreme: " +
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
                        UserDialogs.Instance.Alert("Cihazınız titretmeyi desteklemiyor. "+ex.Message, "Cihaz desteklemiyor");
                    }
                    catch (Exception ex)
                    {
                        UserDialogs.Instance.Alert(ex.Message, "Bir sorunla karşılaştık");
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
                Console.WriteLine("Value Setted for -> " + itemId + "Bildiri: " +
                                  Preferences.Get(itemId + "Bildiri", value));
                Bildiri = value;
                var bildiriVakti = TimeSpan.Parse(Vakit) + TimeSpan.FromMinutes(Convert.ToDouble(BildirmeVakti));
                if (value)
                {
                    CrossLocalNotifications.Current.Show("Bildiri Ayarı Etkinleşti",
                        $"{itemAdi} --> {bildiriVakti} için bildiri etkinleştirildi.", 1001);
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
                    //try
                    //{
                    //    // Use default vibration length
                    //    Vibration.Vibrate();

                    //    // Or use specified time
                    //    var duration = TimeSpan.FromSeconds(1);
                    //    Vibration.Vibrate(duration);
                    //}
                    //catch (FeatureNotSupportedException ex)
                    //{
                    //    UserDialogs.Instance.Alert("Cihazınız titretmeyi desteklemiyor.", "Cihaz desteklemiyor");
                    //}
                    //catch (Exception ex)
                    //{
                    //    UserDialogs.Instance.Alert(ex.Message, "Bir sorunla karşılaştık");
                    //}
                }
                else
                {
                    CrossLocalNotifications.Current.Show("Bildiri Ayarı Devre Dışı",
                        $"{itemAdi} --> {bildiriVakti} için bildiri devre dışı bırakıldı.", 1002);
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
                Console.WriteLine("Value Setted for -> " + itemId + "Etkin: " +
                                  Preferences.Get(itemId + "Etkin", value));
                Etkin = value;
                if(value && BackgroundAggregatorService.Instance==null)
                {
                    BackgroundAggregatorService.Add(() => new ReminderService(60));
                    BackgroundAggregatorService.StartBackgroundService();
                }
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
                        Title = itemAdi = "Fecri Kazip";
                        break;
                    case "fecrisadik":
                        Title = itemAdi = "Fecri Sadik";
                        break;
                    case "sabahsonu":
                        Title = itemAdi = "Sabah Sonu";
                        break;
                    case "ogle":
                        Title = itemAdi = "Öğle";
                        break;
                    case "ikindi":
                        Title = itemAdi = "İkindi";
                        break;
                    case "aksam":
                        Title = itemAdi = "Akşam";
                        break;
                    case "yatsi":
                        Title = itemAdi = "Yatsı";
                        break;
                    case "yatsisonu":
                        Title = itemAdi = "Yatsı Sonu";
                        break;
                }

                //string itemIdEtkin = itemId + "Etkin";
                //string itemIdBildiri = itemId + "Bildiri";
                //string itemIdTitreme = itemId + "Titreme";
                //string itemIdAlarm = itemId + "Alarm";
                //string itemIdBildirmeVakti = itemId + "BildirmeVakti";
                Vakit = Preferences.Get(itemId, "");
                Etkin = Preferences.Get(itemId + "Etkin", false);
                Bildiri = Preferences.Get(itemId + "Bildiri", false);
                Titreme = Preferences.Get(itemId + "Titreme", false);
                Alarm = Preferences.Get(itemId + "Alarm", false);
                BildirmeVakti = Preferences.Get(itemId + "BildirmeVakti", "0.00");//when assign "0" for defaultValue, there always thow exception says: java.lang cannot convert boolean to string. So cheating.
                //var item = await DataStore.GetItemAsync(itemId);
                //Id = item.Id;
                //Text = item.Text;
                //Description = item.Description;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Failed to Load Item\t"+ex.Message);
            }
        }
    }
}
