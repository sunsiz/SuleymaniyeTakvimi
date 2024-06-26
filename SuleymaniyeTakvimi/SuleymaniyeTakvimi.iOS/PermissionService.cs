﻿using System.Diagnostics;
using System.Threading.Tasks;
using Acr.UserDialogs;
using CoreLocation;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.Services;
using UIKit;
using Xamarin.Essentials;

namespace SuleymaniyeTakvimi.iOS
{
    public class PermissionService : IPermissionService
    {
		public void AskNotificationPermission()
		{
			return;
		}

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
                    CancelText = AppResources.Kapat,
                    Message = AppResources.KonumIzniIcerik,
                    OkText = AppResources.GotoSettings,
                    Title = AppResources.KonumIzniBaslik
                }).ConfigureAwait(false);
                if (result) AppInfo.ShowSettingsUI();
                Debug.WriteLine($"Permission Request result: {result}");
            }
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                // Code to run on the main thread
                status = await Permissions.RequestAsync<Permissions.LocationWhenInUse>().ConfigureAwait(false);
            });
            return status;
        }

        public bool IsLocationServiceEnabled()
        {
            return CLLocationManager.Status != CLAuthorizationStatus.Denied;
        }

        public bool IsVoiceOverRunning()
        {
            return UIAccessibility.IsVoiceOverRunning;
        }
    }
}