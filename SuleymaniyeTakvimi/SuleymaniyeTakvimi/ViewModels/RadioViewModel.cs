﻿using System;
using System.Diagnostics;
using System.Windows.Input;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Media;
using MediaManager.Player;
using SuleymaniyeTakvimi.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class RadioViewModel:MvvmHelpers.BaseViewModel
    {
        //HtmlWebViewSource htmlSource;
        public Command PlayCommand { get; }
        // Launcher.OpenAsync is provided by Xamarin.Essentials.
        public ICommand TapCommand => new Command<string>(async (url) => await Launcher.OpenAsync(url).ConfigureAwait(false));

        //private ISimpleAudioPlayer _player;

        private bool _isPlaying;
        public bool IsPlaying
        {
            get => _isPlaying;
            set => SetProperty(ref _isPlaying, value);
        }

        public RadioViewModel()
        {
            IsBusy = true;
            Title = AppResources.IcerikYukleniyor;
            PlayCommand = new Command(Play);
            Title = AppResources.FitratinSesi;
            CheckInternet();
            //_player = CrossSimpleAudioPlayer.Current;
            IsBusy = false;
            if (CrossMediaManager.Current.IsPlaying()) IsPlaying = true;
        }

        private async void Play()
        {
            IsBusy = true;
            
            if (CrossMediaManager.Current.IsPlaying())
            {
                await CrossMediaManager.Current.Stop().ConfigureAwait(true);
                //CrossMediaManager.Current.Notification.Enabled = false;
                //CrossMediaManager.Current.Notification.UpdateNotification();
                IsPlaying = false;
            }
            else
            {
                Title = AppResources.IcerikYukleniyor;
                CheckInternet();
                var mediaItem = await CrossMediaManager.Current.Play("http://shaincast.caster.fm:22344/listen.mp3").ConfigureAwait(true);
                mediaItem.Title = AppResources.FitratinSesi;
                CrossMediaManager.Current.Notification.Enabled = false;
                //CrossMediaManager.Current.Notification.ShowNavigationControls = false;
                //CrossMediaManager.Current.Notification.ShowPlayPauseControls = false;
                mediaItem.MetadataUpdated += OnMediaItemOnMetadataUpdated;
                CrossMediaManager.Current.StateChanged += Current_StateChanged;
            }

            Title = AppResources.FitratinSesi;
            IsBusy = false;
        }
        

        private static void CheckInternet()
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(7));
            }
        }

        private void Current_StateChanged(object sender, MediaManager.Playback.StateChangedEventArgs e)
        {
            if (CrossMediaManager.Current.State == MediaPlayerState.Loading ||
                CrossMediaManager.Current.State == MediaPlayerState.Buffering)
            {
                Debug.WriteLine($"[Radio Player Buffering] {DateTime.Now:HH:m:s.f}");
                Title = AppResources.IcerikYukleniyor;
                IsBusy = true;
                IsPlaying = false;
                return;
            }

            if (CrossMediaManager.Current.IsPlaying())
            {
                Debug.WriteLine($"[Radio Player Playing] {DateTime.Now:HH:m:s.f}");
                IsPlaying = true;
                Title = AppResources.FitratinSesi;
                IsBusy = false;
                return;
            }

            if (CrossMediaManager.Current.State == MediaPlayerState.Stopped ||
                CrossMediaManager.Current.State == MediaPlayerState.Paused)
            {
                IsPlaying = false;
                Title = AppResources.FitratinSesi;
                IsBusy = false;
                return;
            }

            //if (CrossMediaManager.Current.State == MediaPlayerState.Failed)
            //{
            IsPlaying = false;
            Title = AppResources.FitratinSesi;
            UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(7));
                IsBusy = false;
            //}
        }

        private void OnMediaItemOnMetadataUpdated(object sender, MetadataChangedEventArgs args)
        {
            
        }
    }
}
