using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;
using Acr.UserDialogs;
using MediaManager;
using MediaManager.Library;
using MediaManager.Media;
using MediaManager.Player;
using SuleymaniyeTakvimi.Localization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi.ViewModels
{
    public class RadioViewModel : MvvmHelpers.BaseViewModel
    {
        //HtmlWebViewSource htmlSource;
        public ICommand PlayCommand { get; }
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
            CrossMediaManager.Current.MediaPlayer.Stop();
            Title = AppResources.IcerikYukleniyor;
            PlayCommand = new Command(async () => await Play());
            Title = AppResources.FitratinSesi;
            _ = CheckInternet();
            IsBusy = false;
            IsPlaying = CrossMediaManager.Current.IsPlaying();
        }

        private async Task Play()
        {
            IsBusy = true;

            if (CrossMediaManager.Current.IsPlaying())
            {
                await CrossMediaManager.Current.Stop().ConfigureAwait(false);
                CrossMediaManager.Current.Notification.Enabled = false;
                CrossMediaManager.Current.Notification.UpdateNotification();
                IsPlaying = false;
            }
            else
            {
                Title = AppResources.IcerikYukleniyor;
                if (CheckInternet())
                {
                    try
                    {
                        await CrossMediaManager.Current.Play("https://www.suleymaniyevakfi.org/radio.mp3");
                        CrossMediaManager.Current.Notification.UpdateNotification();
                        CrossMediaManager.Current.StateChanged += Current_StateChanged;
                        IsPlaying = true;
                    }
                    catch (Exception exception)
                    {
                        Debug.WriteLine($"Play exception:{exception.Message}");
                        if (CrossMediaManager.Current != null)
                            await CrossMediaManager.Current.Stop().ConfigureAwait(false);
                    }
                }
            }

            Title = AppResources.FitratinSesi;
            IsBusy = false;
        }


        private static bool CheckInternet()
        {
            var current = Connectivity.NetworkAccess;
            if (current != NetworkAccess.Internet)
            {
                UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(5));
                return false;
            }

            return true;
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

            if (CrossMediaManager.Current.State == MediaPlayerState.Failed)
            {
                IsPlaying = false;
                Title = AppResources.FitratinSesi;
                UserDialogs.Instance.Toast(AppResources.RadyoIcinInternet, TimeSpan.FromSeconds(7));
                IsBusy = false;
            }
        }

        private void OnMediaItemOnMetadataUpdated(object sender, MetadataChangedEventArgs args)
        {
            Debug.WriteLine(args.MediaItem.Extras);
            Debug.WriteLine(args.MediaItem.Album);
        }
    }
}
