using System.Collections.Generic;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IDataService
    {
        //Task<Takvim> VakitHesabiAsync();
        Task<Takvim> GetPrayerTimesAsync(bool refreshLocation, bool tryFromFileFirst);
        Task<IList<Takvim>> GetMonthlyPrayerTimesAsync(Location location, bool forceRefresh);
        Task SetWeeklyAlarmsAsync();
    }
}
