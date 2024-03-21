using System;
using System.Diagnostics;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Services;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompassPage : ContentPage
    {
        private CompassViewModel _viewModel;

        public CompassPage()
        {
            InitializeComponent();
            var dataService = DependencyService.Get<DataService>(); // Get DataService from DI container
            BindingContext = _viewModel = new CompassViewModel(dataService);
            Compass.ReadingChanged += Compass_ReadingChanged;
            StartCompassMonitoring();
        }

        private void StartCompassMonitoring()
        {
            try
            {
                if (!Compass.IsMonitoring) Compass.Start(_viewModel.Speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                Debug.WriteLine($"**** {this.GetType().Name}.{nameof(Compass_ReadingChanged)}: {fnsEx.Message}");
            }
            catch (Exception ex)
            {
                UserDialogs.Instance.Alert(ex.Message);
                Debug.WriteLine(ex.Message);
            }
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            CompassImage.RotateTo(360 - e.Reading.HeadingMagneticNorth);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (!DesignMode.IsDesignModeEnabled)
            {
                _viewModel.LocationCommand.Execute(null);
                _viewModel.StartCommand.Execute(null);
            }
        }
    }
}