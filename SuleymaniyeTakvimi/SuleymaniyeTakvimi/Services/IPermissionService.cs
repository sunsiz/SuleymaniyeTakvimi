using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IPermissionService
    {
        Task<PermissionStatus> HandlePermissionAsync();
        void AskNotificationPermission();
        bool IsLocationServiceEnabled();
        bool IsVoiceOverRunning();
    }
}
