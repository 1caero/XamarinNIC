﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace xamarinJKH.Main
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AppsPage : ContentPage
    {
        public AppsPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
        }
    }
}