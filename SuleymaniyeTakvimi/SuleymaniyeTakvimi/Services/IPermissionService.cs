using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.Services
{
    public interface IPermissionService
    {
        Task<PermissionStatus> HandlePermissionAsync();
    }
}
