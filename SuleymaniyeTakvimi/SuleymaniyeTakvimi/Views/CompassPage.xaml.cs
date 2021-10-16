using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            BindingContext = _viewModel = new CompassViewModel();
            Compass.ReadingChanged += Compass_ReadingChanged;
            if (!Compass.IsMonitoring) Compass.Start(_viewModel.speed);
        }

        void Compass_ReadingChanged(object sender, CompassChangedEventArgs e)
        {
            compassImage.RotateTo(360 - e.Reading.HeadingMagneticNorth);
            _viewModel.PointToQibla(e);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.GetLocation();
            if (!DesignMode.IsDesignModeEnabled)
            {
                ((CompassViewModel) BindingContext).LocationCommand.Execute(null);
                ((CompassViewModel) BindingContext).StartCommand.Execute(null);
            }

            IsBusy = false;
        }
    }
}