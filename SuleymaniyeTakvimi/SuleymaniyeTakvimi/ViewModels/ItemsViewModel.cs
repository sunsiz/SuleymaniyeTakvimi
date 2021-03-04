using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;

        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command AddItemCommand { get; }
        public Command<Item> ItemTapped { get; }
        Takvim _takvim;
        private string today;
        private string city;
        public string Today
        {
            get { return DateTime.Today.ToString("M"); }
            set { SetProperty(ref today, value); }
        }

        public string City
        {
            get { return "İstanbul"; }
            set { SetProperty(ref city, value); }
        }
        //public Takvim Vakit
        //{
        //    get
        //    {
        //        if (_takvim == null)
        //        {
        //            var data = new TakvimData();
        //            _takvim = data.takvim;
        //        }

        //        return _takvim;
        //    }
        //    set { SetProperty(ref _takvim, value); }
        //}
        public ItemsViewModel()
        {
            Title = "Süleymaniye Vakfı Takvimi";
            Items = new ObservableCollection<Item>();
            var data = new TakvimData();
            _takvim = data.takvim;
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            AddItemCommand = new Command(OnAddItem);
            LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
        }

        async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;

            try
            {
                Items.Clear();
                var FecriKazip = new Item() { Id = "fecrikazip", Adi = "Fecri Kazip", Vakit = _takvim.FecriKazip, Etkin = Preferences.Get("fecrikazipEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriKazip), DateTime.Parse(_takvim.FecriSadik)) };
                var FecriSadik = new Item() { Id = "fecrisadik", Adi = "Fecri Sadık", Vakit = _takvim.FecriSadik, Etkin = Preferences.Get("fecrisadikEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriSadik), DateTime.Parse(_takvim.SabahSonu)) };
                var SabahSonu = new Item() { Id = "sabahsonu", Adi = "Sabah Sonu", Vakit = _takvim.SabahSonu, Etkin = Preferences.Get("sabahsonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.SabahSonu), DateTime.Parse(_takvim.Ogle)) };
                var Ogle = new Item() { Id = "ogle", Adi = "Öğle", Vakit = _takvim.Ogle, Etkin = Preferences.Get("ogleEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ogle), DateTime.Parse(_takvim.Ikindi)) };
                var Ikindi = new Item() { Id = "ikindi", Adi = "İkindi", Vakit = _takvim.Ikindi, Etkin = Preferences.Get("ikindiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ikindi), DateTime.Parse(_takvim.Aksam)) };
                var Aksam = new Item() { Id = "aksam", Adi = "Akşam", Vakit = _takvim.Aksam, Etkin = Preferences.Get("aksamEtkin", false), State = CheckState(DateTime.Parse(_takvim.Aksam), DateTime.Parse(_takvim.Yatsi)) };
                var Yatsi = new Item() { Id = "yatsi", Adi = "Yatsı", Vakit = _takvim.Yatsi, Etkin = Preferences.Get("yatsiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Yatsi), DateTime.Parse(_takvim.YatsiSonu)) };
                var YatsiSonu = new Item() { Id = "yatsisonu", Adi = "Yatsı Sonu", Vakit = _takvim.YatsiSonu, Etkin = Preferences.Get("yatsisonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.YatsiSonu), DateTime.Parse(_takvim.FecriKazip)) };
                Items.Add(FecriKazip);
                Items.Add(FecriSadik);
                Items.Add(SabahSonu);
                Items.Add(Ogle);
                Items.Add(Ikindi);
                Items.Add(Aksam);
                Items.Add(Yatsi);
                Items.Add(YatsiSonu);
                //Preferences.Set(FecriKazip.Id + "Etkin", FecriKazip.Etkin);
                //Preferences.Set(FecriSadik.Id + "Etkin", FecriSadik.Etkin);
                //Preferences.Set(SabahSonu.Id + "Etkin", SabahSonu.Etkin);
                //Preferences.Set(Ogle.Id + "Etkin", Ogle.Etkin);
                //Preferences.Set(Ikindi.Id + "Etkin", Ikindi.Etkin);
                //Preferences.Set(Aksam.Id + "Etkin", Aksam.Etkin);
                //Preferences.Set(Yatsi.Id + "Etkin", Yatsi.Etkin);
                //Preferences.Set(YatsiSonu.Id + "Etkin", YatsiSonu.Etkin);
                //Preferences.Set(FecriKazip.Id + "Bildiri", FecriKazip.Etkin);
                //Preferences.Set(FecriSadik.Id + "Bildiri", FecriSadik.Etkin);
                //Preferences.Set(SabahSonu.Id + "Bildiri", SabahSonu.Etkin);
                //Preferences.Set(Ogle.Id + "Bildiri", Ogle.Etkin);
                //Preferences.Set(Ikindi.Id + "Bildiri", Ikindi.Etkin);
                //Preferences.Set(Aksam.Id + "Bildiri", Aksam.Etkin);
                //Preferences.Set(Yatsi.Id + "Bildiri", Yatsi.Etkin);
                //Preferences.Set(YatsiSonu.Id + "Bildiri", YatsiSonu.Etkin);
                //Preferences.Set(FecriKazip.Id + "Titreme", FecriKazip.Etkin);
                //Preferences.Set(FecriSadik.Id + "Titreme", FecriSadik.Etkin);
                //Preferences.Set(SabahSonu.Id + "Titreme", SabahSonu.Etkin);
                //Preferences.Set(Ogle.Id + "Titreme", Ogle.Etkin);
                //Preferences.Set(Ikindi.Id + "Titreme", Ikindi.Etkin);
                //Preferences.Set(Aksam.Id + "Titreme", Aksam.Etkin);
                //Preferences.Set(Yatsi.Id + "Titreme", Yatsi.Etkin);
                //Preferences.Set(YatsiSonu.Id + "Titreme", YatsiSonu.Etkin);
                //Preferences.Set(FecriKazip.Id + "Alarm", FecriKazip.Etkin);
                //Preferences.Set(FecriSadik.Id + "Alarm", FecriSadik.Etkin);
                //Preferences.Set(SabahSonu.Id + "Alarm", SabahSonu.Etkin);
                //Preferences.Set(Ogle.Id + "Alarm", Ogle.Etkin);
                //Preferences.Set(Ikindi.Id + "Alarm", Ikindi.Etkin);
                //Preferences.Set(Aksam.Id + "Alarm", Aksam.Etkin);
                //Preferences.Set(Yatsi.Id + "Alarm", Yatsi.Etkin);
                //Preferences.Set(YatsiSonu.Id + "Alarm", YatsiSonu.Etkin);
                //Preferences.Set(FecriKazip.Id + "BildirmeVakti", FecriKazip.Etkin);
                //Preferences.Set(FecriSadik.Id + "BildirmeVakti", FecriSadik.Etkin);
                //Preferences.Set(SabahSonu.Id + "BildirmeVakti", SabahSonu.Etkin);
                //Preferences.Set(Ogle.Id + "BildirmeVakti", Ogle.Etkin);
                //Preferences.Set(Ikindi.Id + "BildirmeVakti", Ikindi.Etkin);
                //Preferences.Set(Aksam.Id + "BildirmeVakti", Aksam.Etkin);
                //Preferences.Set(Yatsi.Id + "BildirmeVakti", Yatsi.Etkin);
                //Preferences.Set(YatsiSonu.Id + "BildirmeVakti", YatsiSonu.Etkin);
                //var items = await DataStore.GetItemsAsync(true);
                //foreach (var item in items)
                //{
                //    Items.Add(item);
                //}
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private string CheckState(DateTime current, DateTime next)
        {
            var state = "";
            if (DateTime.Now > next) state = "Passed";
            if (DateTime.Now > current && DateTime.Now < next) state = "Happening";
            if (DateTime.Now < current) state = "Waiting";
            return state;
        }
        public void OnAppearing()
        {
            IsBusy = true;
            SelectedItem = null;
        }

        public Item SelectedItem
        {
            get => _selectedItem;
            set
            {
                SetProperty(ref _selectedItem, value);
                OnItemSelected(value);
            }
        }

        private async void OnAddItem(object obj)
        {
            await Shell.Current.GoToAsync(nameof(NewItemPage));
        }

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }
    }
}