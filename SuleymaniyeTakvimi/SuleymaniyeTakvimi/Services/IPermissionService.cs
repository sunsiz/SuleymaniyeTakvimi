using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IPermissionService
    {
        Task<PermissionStatus> HandlePermissionAsync();
        bool IsLocationServiceEnabled();
        bool IsVoiceOverRunning();
    }
}
