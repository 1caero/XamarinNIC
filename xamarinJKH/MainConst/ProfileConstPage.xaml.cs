﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.PlatformConfiguration;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.MainConst
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfileConstPage : ContentPage
    {
        private LoginResult Person = new LoginResult();
        private RestClientMP _server = new RestClientMP();
        public bool isSave { get; set; }
        public ProfileConstPage()
        {
            InitializeComponent();
            isSave = Preferences.Get("isPass", false);
            NavigationPage.SetHasNavigationBar(this, false);
            var exitClick = new TapGestureRecognizer();
            exitClick.Tapped += async (s, e) =>
            {
                _ = await Navigation.PopModalAsync();
            };
            FrameBtnExit.GestureRecognizers.Add(exitClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await Navigation.PushAsync(new TechSendPage()); };
            LabelTech.GestureRecognizers.Add(techSend);
            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    Pancake.Padding = new Thickness(0, statusBarHeight, 0, 0);
                    break;
                default:
                    break;
            }

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:

                    // //только темная тема в ios
                    themelabel.IsVisible = false;
                    ThemeButtonBlock.IsVisible = false;
                    ThemeButtonBlock.IsEnabled = false;

                    //BackgroundColor = Color.White;
                    // ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    // ScrollViewContainer.Margin = new Thickness(10, -200, 10, 0);
                    break;
                case Device.Android:
                    // double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(10, -150, 10, 0);
                    //     BackStackLayout.Margin = new Thickness(5, 25, 0, 0);
                    // }
                    break;
                default:
                    break;
            }
            SetText();
            SetColor();
            EntryFio.Text = Settings.Person.FIO;
            EntryEmail.Text = Settings.Person.Email;
            BindingContext = this;
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            SaveInfoAccount(EntryFio.Text, EntryEmail.Text);
        }

        public interface ICloseApplication
        {
            void closeApplication();
        }

        public async void SaveInfoAccount(string fio, string email)
        {
            if (fio != "" && email != "")
            {
                progress.IsVisible = true;
                FrameBtnLogin.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.UpdateProfileConst(email, fio);
                if (result.Error == null)
                {
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", AppResources.SuccessProfile, "OK");
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnLogin.IsVisible = true;
            }
            else
            {
                if (fio == "" && email == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFills} {AppResources.FIO} {AppResources.And} E-mail", "OK");
                }
                else if (fio == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFill} {AppResources.FIO}", "OK");
                }
                else if (email == "")
                {
                    await DisplayAlert("", $"{AppResources.ErrorFill} E-mail", "OK");
                }
            }
        }

        void SetText()
        {
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            UkName.Text = Settings.MobileSettings.main_name;
            SetAdminName();
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
            FrameSettings.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        private void SetAdminName()
        {
            FormattedString formattedName = new FormattedString();
            OSAppTheme currentTheme = Application.Current.RequestedTheme;
            //if (Xamarin.Essentials.DeviceInfo.Platform == Xamarin.Essentials.DevicePlatform.iOS)
            //    currentTheme = OSAppTheme.Dark;
            formattedName.Spans.Add(new Span
            {
                Text = Settings.Person.FIO,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.Bold,
                FontSize = 16
            });
            formattedName.Spans.Add(new Span
            {
                Text = AppResources.GoodDay,
                TextColor = currentTheme.Equals(OSAppTheme.Dark) ? Color.White : Color.Black,
                FontAttributes = FontAttributes.None,
                FontSize = 16
            });
            LabelName.FormattedText = formattedName;
        }

        void SetColor()
        {
            Color hexColor = Color.FromHex(Settings.MobileSettings.color);
            UkName.Text = Settings.MobileSettings.main_name;
            IconViewSave.Foreground = Color.White;
            // IconViewNameUk.Foreground = hexColor;
            IconViewFio.Foreground = hexColor;
            IconViewEmail.Foreground = hexColor;
            IconViewExit.Foreground = hexColor;

            FrameBtnExit.BackgroundColor = Color.White;
            FrameBtnExit.BorderColor = hexColor;
            FrameBtnLogin.BackgroundColor = hexColor;
            LabelseparatorEmail.BackgroundColor = hexColor;
            LabelseparatorFio.BackgroundColor = hexColor;
            BtnExit.TextColor = hexColor;
            progress.Color = hexColor;
            
            RadioButtonAuto.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            RadioButtonDark.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            RadioButtonLigth.Effects.Add(Effect.Resolve("MyEffects.RadioButtonEffect"));
            
            int theme = Preferences.Get("Theme", 0);
            switch (theme)
            {
                case 0:
                    RadioButtonAuto.IsChecked = true;
                    break;
                case 1:
                    RadioButtonDark.IsChecked = true;
                    break;
                case 2:
                    RadioButtonLigth.IsChecked = true;
                    break;
            }

        }

        private void SwitchSavePass_OnPropertyChanged(object sender, ToggledEventArgs toggledEventArgs)
        {
            Preferences.Set("isPass", isSave);
        }

        private void RadioButtonAuto_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            //только темная тема в ios
            if (Xamarin.Essentials.DeviceInfo.Platform != DevicePlatform.iOS)
            {
                Application.Current.UserAppTheme = OSAppTheme.Unspecified;
                Preferences.Set("Theme", 0);
                MessagingCenter.Send<Object>(this, "ChangeThemeConst");
                MessagingCenter.Send<Object>(this, "ChangeAdminApp");
                MessagingCenter.Send<Object>(this, "ChangeAdminMonitor");
                SetAdminName();
            }
                
        }

        private void RadioButtonDark_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Application.Current.UserAppTheme = OSAppTheme.Dark;
            Preferences.Set("Theme", 1);
            MessagingCenter.Send<Object>(this, "ChangeThemeConst");
            MessagingCenter.Send<Object>(this, "ChangeAdminApp");
            MessagingCenter.Send<Object>(this, "ChangeAdminMonitor");
            SetAdminName();
        }

        private void RadioButtonLigth_OnCheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            Application.Current.UserAppTheme = OSAppTheme.Light;
            Preferences.Set("Theme", 2);
            MessagingCenter.Send<Object>(this, "ChangeThemeConst");
            MessagingCenter.Send<Object>(this, "ChangeAdminApp");
            MessagingCenter.Send<Object>(this, "ChangeAdminMonitor");
            SetAdminName();
        }
    }
}