using SuleymaniyeTakvimi.ViewModels;
using SuleymaniyeTakvimi.Views;
using System;
using System.Collections.Generic;
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
        }

    }
}
