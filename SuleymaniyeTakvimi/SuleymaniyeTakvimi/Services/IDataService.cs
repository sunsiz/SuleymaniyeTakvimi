using System.Collections.Generic;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IDataService
    {
        Takvim VakitHesabi();
        Task<Takvim> GetPrayerTimesAsync();
        IList<Takvim> GetMonthlyPrayerTimes(Location location);
        void SetWeeklyAlarms();
    }
}
