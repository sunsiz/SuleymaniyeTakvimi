using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SuleymaniyeTakvimi.Services;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Droid
{
    public class PermissionService:IPermissionService
    {
        public async Task<PermissionStatus> HandlePermissionAsync()
        {
            MainActivity main = MainActivity.Instance;
            var status = await main.HandleLocationPermissionAsync().ConfigureAwait(false);
            return status;
        }
    }
}