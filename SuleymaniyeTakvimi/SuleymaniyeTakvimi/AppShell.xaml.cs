﻿using SuleymaniyeTakvimi.Views;
using Xamarin.Forms;

namespace SuleymaniyeTakvimi
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        public AppShell()
        {
            InitializeComponent();
            Routing.RegisterRoute(nameof(ItemsPage), typeof(ItemsPage));
            Routing.RegisterRoute(nameof(ItemDetailPage), typeof(ItemDetailPage));
            Routing.RegisterRoute(nameof(MonthPage), typeof(MonthPage));
            Routing.RegisterRoute(nameof(SettingsPage), typeof(SettingsPage));
            //Routing.RegisterRoute(nameof(CompassPage), typeof(CompassPage));
        }

    }
}
