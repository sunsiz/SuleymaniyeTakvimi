using SuleymaniyeTakvimi.Models;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Xamarin.Essentials;
using Xamarin.Forms;
using System.Threading;
using Xamarin.Forms.Internals;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class ItemsViewModel : BaseViewModel
    {
        private Item _selectedItem;
        public ObservableCollection<Item> Items { get; }
        public Command LoadItemsCommand { get; }
        public Command GoToMapCommand { get; }
        public Command GoToMonthCommand { get; }
        public Command RefreshLocationCommand { get; }
        public Command<Item> ItemTapped { get; }
        Takvim _takvim, vakitler;
        private string city;

        public Takvim Vakitler
        {
            get
            {
                if (vakitler == null)
                {
                    var data = new DataService();
                    vakitler = data.takvim;
                    //data.CheckNotification();
                }

                return vakitler;
            }
            set { SetProperty(ref vakitler, value); }
        }
        public string Today
        {
            get { return DateTime.Today.ToString("M"); }
        }

        public string City
        {
            get { return city; }
            set { SetProperty(ref city, value); }
        }

        private async void GetCity()
        {
            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_takvim.Enlem, CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_takvim.Boylam,CultureInfo.InvariantCulture.NumberFormat)).ConfigureAwait(false);
            City = placemark.FirstOrDefault()?.AdminArea ?? placemark.FirstOrDefault()?.CountryName;
        }
        public ItemsViewModel()
        {
            Log.Warning("TimeStamp-ItemsViewModel-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            Items = new ObservableCollection<Item>();
            var data = new DataService();
            _takvim = data.takvim;
            LoadItemsCommand = new Command(async () => await ExecuteLoadItemsCommand());

            ItemTapped = new Command<Item>(OnItemSelected);

            //Without the Convert.ToDouble conversion it confuses the , and . when UI culture changed. like latitude=50.674367348783 become latitude= 50674367348783 then throw exception.
            GoToMapCommand = new Command(async () =>
            {
                var location = new Location(Convert.ToDouble(_takvim.Enlem, CultureInfo.InvariantCulture.NumberFormat),Convert.ToDouble(_takvim.Boylam, CultureInfo.InvariantCulture.NumberFormat));
                var placemark = await Geocoding.GetPlacemarksAsync(Convert.ToDouble(_takvim.Enlem,CultureInfo.InvariantCulture.NumberFormat), Convert.ToDouble(_takvim.Boylam,CultureInfo.InvariantCulture.NumberFormat));
                var options = new MapLaunchOptions { Name = placemark.FirstOrDefault()?.Thoroughfare ?? placemark.FirstOrDefault()?.CountryName};

                try
                {
                    await Map.OpenAsync(location, options);
                }
                catch (Exception ex)
                {
                    UserDialogs.Instance.Toast("Haritayı açarken bir sorun oluştu.\nDetaylar: " + ex.Message);
                }
            });
            //LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            //GetCity();
            GoToMonthCommand=new Command(GoToMonthPage);
            RefreshLocationCommand = new Command(async () =>
            {
                await GetPrayerTimesAsync().ConfigureAwait(false);
                await ExecuteLoadItemsCommand().ConfigureAwait(false);
            });
            Log.Warning("TimeStamp-ItemsViewModel-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
        }

        private async Task ExecuteLoadItemsCommand()
        {
            IsBusy = true;
            Log.Warning("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));

            try
            {
                Items.Clear();
                var FecriKazip = new Item() { Id = "fecrikazip", Adi = "Fecri Kazip", Vakit = _takvim.FecriKazip, Etkin = Preferences.Get("fecrikazipEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriKazip), DateTime.Parse(_takvim.FecriSadik)), Alarm = Preferences.Get("fecrikazipAlarm",false), Bildiri = Preferences.Get("fecrikazipBildiri",false), Titreme = Preferences.Get("fecrikazipTitreme",false), BildirmeVakti = Preferences.Get("fecrikazipBildiriVakti","0.00")};
                var FecriSadik = new Item() { Id = "fecrisadik", Adi = "Fecri Sadık", Vakit = _takvim.FecriSadik, Etkin = Preferences.Get("fecrisadikEtkin", false), State = CheckState(DateTime.Parse(_takvim.FecriSadik), DateTime.Parse(_takvim.SabahSonu)), Alarm = Preferences.Get("fecrisadikAlarm", false), Bildiri = Preferences.Get("fecrisadikBildiri", false), Titreme = Preferences.Get("fecrisadikTitreme", false), BildirmeVakti = Preferences.Get("fecrisadikBildiriVakti", "0.00") };
                var SabahSonu = new Item() { Id = "sabahsonu", Adi = "Sabah Sonu", Vakit = _takvim.SabahSonu, Etkin = Preferences.Get("sabahsonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.SabahSonu), DateTime.Parse(_takvim.Ogle)), Alarm = Preferences.Get("sabahsonuAlarm", false), Bildiri = Preferences.Get("sabahsonuBildiri", false), Titreme = Preferences.Get("sabahsonuTitreme", false), BildirmeVakti = Preferences.Get("sabahsonuBildiriVakti", "0.00") };
                var Ogle = new Item() { Id = "ogle", Adi = "Öğle", Vakit = _takvim.Ogle, Etkin = Preferences.Get("ogleEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ogle), DateTime.Parse(_takvim.Ikindi)), Alarm = Preferences.Get("ogleAlarm", false), Bildiri = Preferences.Get("ogleBildiri", false), Titreme = Preferences.Get("ogleTitreme", false), BildirmeVakti = Preferences.Get("ogleBildiriVakti", "0.00") };
                var Ikindi = new Item() { Id = "ikindi", Adi = "İkindi", Vakit = _takvim.Ikindi, Etkin = Preferences.Get("ikindiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Ikindi), DateTime.Parse(_takvim.Aksam)), Alarm = Preferences.Get("ikindiAlarm", false), Bildiri = Preferences.Get("ikindiBildiri", false), Titreme = Preferences.Get("ikindiTitreme", false), BildirmeVakti = Preferences.Get("ikindiBildiriVakti", "0.00") };
                var Aksam = new Item() { Id = "aksam", Adi = "Akşam", Vakit = _takvim.Aksam, Etkin = Preferences.Get("aksamEtkin", false), State = CheckState(DateTime.Parse(_takvim.Aksam), DateTime.Parse(_takvim.Yatsi)), Alarm = Preferences.Get("aksamAlarm", false), Bildiri = Preferences.Get("aksamBildiri", false), Titreme = Preferences.Get("aksamTitreme", false), BildirmeVakti = Preferences.Get("aksamBildiriVakti", "0.00") };
                var Yatsi = new Item() { Id = "yatsi", Adi = "Yatsı", Vakit = _takvim.Yatsi, Etkin = Preferences.Get("yatsiEtkin", false), State = CheckState(DateTime.Parse(_takvim.Yatsi), DateTime.Parse(_takvim.YatsiSonu)), Alarm = Preferences.Get("yatsiAlarm", false), Bildiri = Preferences.Get("yatsiBildiri", false), Titreme = Preferences.Get("yatsiTitreme", false), BildirmeVakti = Preferences.Get("yatsiBildiriVakti", "0.00") };
                var YatsiSonu = new Item() { Id = "yatsisonu", Adi = "Yatsı Sonu", Vakit = _takvim.YatsiSonu, Etkin = Preferences.Get("yatsisonuEtkin", false), State = CheckState(DateTime.Parse(_takvim.YatsiSonu), DateTime.Parse(_takvim.FecriKazip)), Alarm = Preferences.Get("yatsisonuAlarm", false), Bildiri = Preferences.Get("yatsisonuBildiri", false), Titreme = Preferences.Get("yatsisonuTitreme", false), BildirmeVakti = Preferences.Get("yatsisonuBildiriVakti", "0.00") };
                Items.Add(FecriKazip);
                Items.Add(FecriSadik);
                Items.Add(SabahSonu);
                Items.Add(Ogle);
                Items.Add(Ikindi);
                Items.Add(Aksam);
                Items.Add(Yatsi);
                Items.Add(YatsiSonu);
                if (Preferences.Get(FecriKazip.Id, "") == "") Preferences.Set(FecriKazip.Id, _takvim.FecriKazip);
                if (Preferences.Get(FecriSadik.Id, "") == "") Preferences.Set(FecriSadik.Id, _takvim.FecriSadik);
                if (Preferences.Get(SabahSonu.Id, "") == "") Preferences.Set(SabahSonu.Id, _takvim.SabahSonu);
                if (Preferences.Get(Ogle.Id, "") == "") Preferences.Set(Ogle.Id, _takvim.Ogle);
                if (Preferences.Get(Ikindi.Id, "") == "") Preferences.Set(Ikindi.Id, _takvim.Ikindi);
                if (Preferences.Get(Aksam.Id, "") == "") Preferences.Set(Aksam.Id, _takvim.Aksam);
                if (Preferences.Get(Yatsi.Id, "") == "") Preferences.Set(Yatsi.Id, _takvim.Yatsi);
                if (Preferences.Get(YatsiSonu.Id, "") == "") Preferences.Set(YatsiSonu.Id, _takvim.YatsiSonu);
                //GetCity();
                var today = Today;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
            }
            finally
            {
                Log.Warning("TimeStamp-ItemsViewModel-ExecuteLoadItemsCommand-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
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
            Log.Warning("TimeStamp-OnAppearing-Start", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
            IsBusy = true;
            SelectedItem = null;
            LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
            GetCity();
            Log.Warning("TimeStamp-OnAppearing-Finish", DateTime.Now.ToString("MM/dd/yyyy hh:mm:ss.fff tt"));
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

        async void OnItemSelected(Item item)
        {
            if (item == null)
                return;

            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(ItemDetailPage)}?{nameof(ItemDetailViewModel.ItemId)}={item.Id}");
        }

        async void GoToMonthPage(object obj)
        {
            IsBusy = true;
            // This will push the ItemDetailPage onto the navigation stack
            await Shell.Current.GoToAsync($"{nameof(MonthPage)}");
        }

        async Task GetPrayerTimesAsync()
        {
            IsBusy = true;
            var data = new DataService();
            try
            {
                var takvim = await data.GetCurrentLocation().ConfigureAwait(true);
                //var request = new GeolocationRequest(GeolocationAccuracy.Low, TimeSpan.FromSeconds(10));
                //CancellationTokenSource cts = new CancellationTokenSource();
                //var location = await Geolocation.GetLocationAsync(request, cts.Token).ConfigureAwait(true);
                //if (location != null)
                if (takvim != null)
                {
                    Location location = new Location(takvim.Enlem, takvim.Boylam, takvim.Yukseklik);
                    data.konum = new Takvim();
                    data.konum.Enlem = location.Latitude;
                    data.konum.Boylam = location.Longitude;
                    data.konum.Yukseklik = location.Altitude ?? 0;
                    Vakitler = data.VakitHesabi();
                    if (Vakitler.Enlem != 0)
                    {
                        //Application.Current.Properties["takvim"] = Vakitler;
                        Preferences.Set("enlem", Vakitler.Enlem);
                        Preferences.Set("boylam", Vakitler.Boylam);
                        Preferences.Set("yukseklik", Vakitler.Yukseklik);
                        Preferences.Set("saatbolgesi", Vakitler.SaatBolgesi);
                        Preferences.Set("yazkis", Vakitler.YazKis);
                        Preferences.Set("fecrikazip", Vakitler.FecriKazip);
                        Preferences.Set("fecrisadik", Vakitler.FecriSadik);
                        Preferences.Set("sabahsonu", Vakitler.SabahSonu);
                        Preferences.Set("ogle", Vakitler.Ogle);
                        Preferences.Set("ikindi", Vakitler.Ikindi);
                        Preferences.Set("aksam", Vakitler.Aksam);
                        Preferences.Set("yatsi", Vakitler.Yatsi);
                        Preferences.Set("yatsisonu", Vakitler.YatsiSonu);
                        Preferences.Set("tarih", Vakitler.Tarih);
                        //await Application.Current.SavePropertiesAsync();
                    }

                    LoadItemsCommand.Execute(ExecuteLoadItemsCommand());
                }
                else
                {
                    UserDialogs.Instance.Toast(
                        "Konum alma başarısız, Lütfen konum hizmetlerinin açık olduğunu kontrol edin!",
                        TimeSpan.FromSeconds(7));
                }
            }
            catch (Exception exception)
            {
                UserDialogs.Instance.Alert(exception.Message, "Konuma erişmeye çalışırken bir sorun oluştu.");
            }
            finally
            {
                UserDialogs.Instance.Toast("Konum başarıyla yenilendi", TimeSpan.FromSeconds(3));
                IsBusy = false;
            }
        }
    }
}