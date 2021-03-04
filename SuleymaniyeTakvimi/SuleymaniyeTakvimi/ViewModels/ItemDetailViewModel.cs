using SuleymaniyeTakvimi.Models;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
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
        }

        private void BildirmeVaktiAyari(object obj)
        {
            var radiobutton = obj as RadioButton;
            Preferences.Set(itemId + "BildirmeVakti", radiobutton.Value.ToString());
            Console.WriteLine("Value Setted for -> " + itemId + "BildirmeVakti: " +
                              Preferences.Get(itemId + "BildirmeVakti", radiobutton.Value.ToString()));
        }

        private void AlarmAyari(object obj)
        {
            var value = (bool)obj;
            Preferences.Set(itemId + "Alarm", value);
            Console.WriteLine("Value Setted for -> " + itemId + "Alarm: " +
                              Preferences.Get(itemId + "Alarm", value));
        }

        private void TitremeAyari(object obj)
        {
            var value = (bool)obj;
            Preferences.Set(itemId + "Titreme", value);
            Console.WriteLine("Value Setted for -> " + itemId + "Titreme: " +
                              Preferences.Get(itemId + "Titreme", value));
        }

        private void BildiriAyari(object obj)
        {
            var value = (bool)obj;
            Preferences.Set(itemId + "Bildiri", value);
            Console.WriteLine("Value Setted for -> " + itemId + "Bildiri: " +
                              Preferences.Get(itemId + "Bildiri", value));
        }

        private void Etkinlestir(object obj)
        {
            var value = (bool) obj;
            Preferences.Set(itemId + "Etkin", value);
            Console.WriteLine("Value Setted for -> " + itemId + "Etkin: " +
                              Preferences.Get(itemId + "Etkin", value));
        }

        public string ItemId
        {
            get => itemId;
            set
            {
                itemId = value;
                LoadItem(value);
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

        public async void LoadItem(string itemId)
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
                Vakit = Preferences.Get(itemId, "");
                Etkin = Preferences.Get(itemId + "Etkin", false);
                Bildiri = Preferences.Get(itemId + "Bildiri", false);
                Titreme = Preferences.Get(itemId + "Titreme", false);
                Alarm = Preferences.Get(itemId + "Alarm", false);
                BildirmeVakti = Preferences.Get(itemId + "BildirmeVakti", "0");
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
