using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Models;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IDataService
    {
        Takvim VakitHesabi();
        Task<Takvim> GetPrayerTimes(Location location);
        void GetMonthlyPrayerTimes(Location location);
    }
}
