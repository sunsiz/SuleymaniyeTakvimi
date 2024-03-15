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
        IList<Takvim> GetMonthlyPrayerTimes(Location location, bool forceRefresh);
        Task SetWeeklyAlarmsAsync();
    }
}
