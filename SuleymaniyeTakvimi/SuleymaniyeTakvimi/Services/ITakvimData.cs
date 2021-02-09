using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Models;

namespace SuleymaniyeTakvimi.Services
{
    public interface ITakvimData
    {
        //Task<bool> AddItemAsync(T item);
        //Task<bool> UpdateItemAsync(T item);
        //Task<bool> DeleteItemAsync(string id);
        //Task<T> GetItemAsync(string id);
        //Task<IEnumerable<T>> GetItemsAsync(bool forceRefresh = false);
        Takvim VakitHesabi();
    }
}
