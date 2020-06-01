using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Server;
using xamarinJKH.Main;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddIdent : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        private PaysPage _paysPage;
        public AddIdent(PaysPage paysPage)
        {
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    BackgroundColor = Color.White;
                    ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    break;
                case Device.Android:
                default:
                    break;
            }
            _paysPage = paysPage;
            SetText();
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);
        }
        
        private async void AddButtonClick(object sender, EventArgs e)
        {
            AddIdentAccount(EntryIdent.Text);
        }
        
        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "+" + Settings.Person.Phone;
            FrameBtnAdd.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            progress.Color = Color.FromHex(Settings.MobileSettings.color);
            Labelseparator.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            IconViewFio.Foreground = Color.FromHex(Settings.MobileSettings.color);
        }
        
        public async void AddIdentAccount(string ident)
        {
            if (ident != "")
            {
                progress.IsVisible = true;
                FrameBtnAdd.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.AddIdent(ident, Settings.Person.Email);
                if (result.Error == null)
                {
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", "Лс/ч " + ident + " успешно подключён", "OK");             
                    FrameBtnAdd.IsVisible = true;
                    progress.IsVisible = false;
                    await Navigation.PopAsync();
                    _paysPage.RefreshPaysData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnAdd.IsVisible = true;
                    progress.IsVisible = false;
                    if (Device.RuntimePlatform == Device.iOS)
                    {
                        await DisplayAlert("ОШИБКА", result.Error, "OK");
                    }
                    else
                    {
                        DependencyService.Get<IMessage>().ShortAlert(result.Error);
                    }
                }

                progress.IsVisible = false;
                FrameBtnAdd.IsVisible = true;
            }
            else
            {
                if (ident == "")
                {
                    await DisplayAlert("", "Заполните номер счета", "OK");
                }
            }
        }
    }
}