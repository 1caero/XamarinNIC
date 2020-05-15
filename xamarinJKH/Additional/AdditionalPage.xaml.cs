﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Additional
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AdditionalPage : ContentPage
    {
        public List<AdditionalService> Additional { get; set; }
        public AdditionalPage()
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
            SetText();
            SetAdditional();
            this.BindingContext = this;
        }
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "+" + Settings.Person.Phone;
            
        }

        void SetAdditional()
        {
            Additional = new List<AdditionalService>();
            foreach (var each in Settings.EventBlockData.AdditionalServices)
            {
                if (each.HasLogo)
                {
                    Additional.Add(each);
                }
            }
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            AdditionalService select = e.Item as AdditionalService;
            await Navigation.PushAsync(new AdditionalOnePage(select));
        }
    }
}