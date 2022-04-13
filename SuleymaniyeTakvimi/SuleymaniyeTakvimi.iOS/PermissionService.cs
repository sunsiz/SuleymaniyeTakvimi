using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using Foundation;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using UIKit;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.iOS
{
    public class PermissionService:IPermissionService
    {
        public async Task<PermissionStatus> HandlePermissionAsync()
        {
            var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            if (status == PermissionStatus.Denied)
            {
                if (Permissions.ShouldShowRationale<Permissions.LocationWhenInUse>())
                {
                    UserDialogs.Instance.Alert(AppResources.KonumIzniIcerik, AppResources.KonumIzniBaslik);
                }

                var result = await UserDialogs.Instance.ConfirmAsync(new ConfirmConfig()
                {
                    AndroidStyleId = 0,
                    CancelText = AppResources.Kapat,
                    Message = AppResources.KonumIzniIcerik,
                    OkText = AppResources.GotoSettings,
                    Title = AppResources.KonumIzniBaslik
                }).ConfigureAwait(false);
                if (result) AppInfo.ShowSettingsUI();
                Debug.WriteLine("Permission Request result:", result.ToString());
            }
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Code to run on the main thread
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            });
            return status;
        }
    }
}