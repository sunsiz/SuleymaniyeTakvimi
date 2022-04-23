using System;
using System.Diagnostics;
using Acr.UserDialogs;
using SuleymaniyeTakvimi.Localization;
using SuleymaniyeTakvimi.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SuleymaniyeTakvimi.Views
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class CompassPage : ContentPage
    {
        public CompassPage()
        {
            CompassViewModel viewModel;
            InitializeComponent();
            BindingContext = viewModel = new CompassViewModel();
            Compass.ReadingChanged += Compass_ReadingChanged;
            try
            {
                if (!Compass.IsMonitoring) Compass.Start(viewModel.Speed);
            }
            catch (FeatureNotSupportedException fnsEx)
            {
                // Feature not supported on device
                //UserDialogs.Instance.Alert(AppResources.CihazDesteklemiyor, AppResources.CihazDesteklemiyor);
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
            //_viewModel.PointToQibla(e);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            //_viewModel.GetLocation();
            if (!DesignMode.IsDesignModeEnabled)
            {
                ((CompassViewModel) BindingContext).LocationCommand.Execute(null);
                ((CompassViewModel) BindingContext).StartCommand.Execute(null);
            }
        }
    }
}