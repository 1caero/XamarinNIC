using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.Counters;
using xamarinJKH.Main;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Utils;
using Xamarin.Forms.Internals;

namespace xamarinJKH.Counters
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AddMetersPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        private MeterInfo meter = new MeterInfo();
        private List<MeterInfo> meters = new List<MeterInfo>();
        private CountersPage _countersPage;

        public Color CellColor { get; set; } //=> Color.FromHex(xamarinJKH.Utils.Settings.MobileSettings.color);

        decimal PrevValue;
        bool SetPrev;
        public AddMetersPage(MeterInfo meter, List<MeterInfo> meters, CountersPage countersPage, decimal counterThisMonth = 0, decimal counterPrevMonth = 0)
        {            
            InitializeComponent();
            NavigationPage.SetHasNavigationBar(this, false);
            _countersPage = countersPage;
            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    BackgroundColor = Color.White;
                    ImageFon.Margin = new Thickness(0, 0, 0, 0);
                    StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    break;
                case Device.Android:
                    double or = Math.Round(((double)App.ScreenWidth / (double)App.ScreenHeight), 2);
                    if (Math.Abs(or - 0.5) < 0.02)
                    {
                        ScrollViewContainer.Margin = new Thickness(0, 0, 0, -110);
                        BackStackLayout.Margin = new Thickness(-5, 15, 0, 0);
                    }
                    break;
                default:
                    break;
            }
            this.meter = meter;
            this.meters = meters;
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            BackStackLayout.GestureRecognizers.Add(backClick);

            var saveClick = new TapGestureRecognizer();
            saveClick.Tapped += async (s, e) => { ButtonClick(FrameBtnLogin, null); };
            FrameBtnLogin.GestureRecognizers.Add(saveClick);
            FrameBtnLogin.BackgroundColor = Color.FromHex(xamarinJKH.Utils.Settings.MobileSettings.color);

            if (counterPrevMonth > 0)
            {
                SetPrevious(counterPrevMonth);
                   //PrevValue = counterPrevMonth;
                SetPrev = true;
            }

            //(Counter.CounterInput as xamarinJKH.CustomRenderers.CounterEntry).Editing = SetPrev;
            //if (counterThisMonth > 0)
            //{
            //    Counter.CounterInput.Text = counterThisMonth.ToString().Replace('.',',');
            //}
            Device.BeginInvokeOnMainThread(() =>
            {
                CellColor = Color.FromHex(Settings.MobileSettings.color);
            });
            

            BindingContext = this;

            SetTextAndColor();
        }

        void SetPrevious(decimal counterPrevMonth)
        {
            //var d = 5423.456;
            var ddd = counterPrevMonth * 1000;
            var d08t = ddd % 10;
            ddd = (ddd-d08t)/10;
            var d07t = ddd % 10;
            ddd = (ddd-d07t)/10;
            var d06t = ddd % 10;
            ddd = (ddd-d06t)/10;
            var d05t = ddd % 10;
            ddd = (ddd - d05t) / 10;
            var d04t = ddd % 10;
            ddd = (ddd - d04t) / 10;
            var d03t = ddd % 10;
            ddd = (ddd - d03t) / 10;
            var d02t = ddd % 10;
            ddd = (ddd - d02t) / 10;
            var d01t = ddd % 10;


            Device.BeginInvokeOnMainThread(() =>
            {
                d08.Text = Convert.ToString(d08t);
                d07.Text = Convert.ToString(d07t);
                d06.Text = Convert.ToString(d06t);
                d05.Text = Convert.ToString(d05t);
                d04.Text = Convert.ToString(d04t);
                d03.Text = Convert.ToString(d03t);
                d02.Text = Convert.ToString(d02t);
                d01.Text = Convert.ToString(d01t);
            });            

        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            Device.BeginInvokeOnMainThread(() =>
            {
                d1.Unfocus();
                d1.Focus();
                //Counter.CounterInput.Unfocus();
                //Counter.CounterInput.Focus();
            });
        }

        private async void ButtonClick(object sender, EventArgs e)
        {
            try {
                string count ="";
                var p1 = -1;
                var p2 = -1;
                var p3 = -1;
                var p4 = -1;
                var p5 = -1;
                var p6 = -1;
                var p7 = -1;
                var p8 = -1;

                if (int.TryParse(d1.Text,out p1) && int.TryParse(d2.Text, out p2) && int.TryParse(d3.Text, out p3) 
                    && int.TryParse(d4.Text, out p4) && int.TryParse(d5.Text, out p5) && int.TryParse(d6.Text, out p6)
                    && int.TryParse(d7.Text, out p7) && int.TryParse(d8.Text, out p8))
                {
                    count += d1.Text != "0" ? d1.Text : "";
                    count += d2.Text != "0" ? d2.Text : "";
                    count += d3.Text != "0" ? d3.Text : "";
                    count += d4.Text != "0" ? d4.Text : "";
                    count += d5.Text+ "." ;
                    count += d6.Text;
                    count += d7.Text != "0" ? d7.Text : "";
                    count += d8.Text != "0" ? d8.Text : "";
                    
                    //count += d2.Text + d3.Text + d4.Text + d5.Text+ "." + d6.Text + d7.Text + d8.Text;
                    SaveInfoAccount(count);
                }
                else
                {
                    await DisplayAlert("Внимание", "Не все введенные символы являются цифрами. Пожалуйста, проверьте правильность введенных показаний", "OK");
                }
            }
            catch(Exception ex)
            {
                await DisplayAlert("ОШИБКА", "При передаче показаний произошла ошибка", "OK");
            }
            
        }

        void SetTextAndColor()
        {
            if (meter.Resource.ToLower().Contains("холодное"))
            {
                img.Source = ImageSource.FromFile("ic_cold_water");
            }
            else if (meter.Resource.ToLower().Contains("горячее"))
            {
                img.Source = ImageSource.FromFile("ic_heat_water");
            }
            else
            {
                img.Source = ImageSource.FromFile("ic_electr");
            }
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text = "+" + Settings.Person.Phone;
            NameLbl.Text = meter.Resource;
            LabelseparatorFio.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            progress.Color = Color.FromHex(Settings.MobileSettings.color);
            FrameBtnLogin.BackgroundColor = Color.FromHex(Settings.MobileSettings.color);
            FormattedString formattedUniq = new FormattedString();
            formattedUniq.Spans.Add(new Span
            {
                Text = "Заводской №: ",
                TextColor = Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedUniq.Spans.Add(new Span
            {
                Text = meter.FactoryNumber,
                TextColor = Color.White,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            UniqNumLbl.FormattedText = formattedUniq;

            FormattedString formattedCheckup = new FormattedString();
            formattedCheckup.Spans.Add(new Span
            {
                Text = "Последняя поверка: ",
                TextColor = Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedCheckup.Spans.Add(new Span
            {
                Text = meter.LastCheckupDate,
                TextColor = Color.White,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            CheckupLbl.FormattedText = formattedCheckup;

            FormattedString formattedRecheckup = new FormattedString();
            formattedRecheckup.Spans.Add(new Span
            {
                Text = "Межповерочный интервал: ",
                TextColor = Color.LightGray,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            formattedRecheckup.Spans.Add(new Span
            {
                Text = meter.RecheckInterval.ToString() + " лет",
                TextColor = Color.White,
                FontAttributes = FontAttributes.None,
                FontSize = 15
            });
            RecheckLbl.FormattedText = formattedRecheckup;
            if (meter.Values.Count != 0)
            {
                BindingContext = new AddMetersPageViewModel(SetPrev ? PrevValue : meter.Values[0].Value);
                //PredCount.Text = meter.Values[0].Value.ToString(CultureInfo.InvariantCulture);
            }
        }

        public async void SaveInfoAccount(string count)
        {
            if (!string.IsNullOrEmpty(count))
            {
                progress.IsVisible = true;
                FrameBtnLogin.IsVisible = false;
                progress.IsVisible = true;
                CommonResult result = await _server.SaveMeterValue(meter.ID.ToString(), count, "", "");
                if (result.Error == null)
                {
                    Console.WriteLine(result.ToString());
                    Console.WriteLine("Отправлено");
                    await DisplayAlert("", "Показания успешно переданы", "OK");
                    FrameBtnLogin.IsVisible = true;
                    progress.IsVisible = false;
                    await Navigation.PopAsync();
                    _countersPage.RefreshCountersData();
                }
                else
                {
                    Console.WriteLine("---ОШИБКА---");
                    Console.WriteLine(result.ToString());
                    FrameBtnLogin.IsVisible = true;
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
                FrameBtnLogin.IsVisible = true;
            }
            else
            {
                await DisplayAlert("Введите показания", "", "OK");
            }
        }

        private void d1_Completed(object sender, EventArgs e)
        {            
            Device.BeginInvokeOnMainThread(() =>
            {
                if(string.IsNullOrWhiteSpace(d1.Text))
                {
                    return;
                }
                
                d2.Unfocus();
                d2.Focus();                
            });
        }
        private void d2_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d2.Text))
                {
                    return;
                }

                d3.Unfocus();
                d3.Focus();
            });
        }
        private void d3_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d3.Text))
                {
                    return;
                }

                d4.Unfocus();
                d4.Focus();
            });
        }
        private void d4_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d4.Text))
                {
                    return;
                }

                d5.Unfocus();
                d5.Focus();
            });
        }
        private void d5_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d5.Text))
                {
                    return;
                }

                d6.Unfocus();
                d6.Focus();
            });
        }
        private void d6_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d6.Text))
                {
                    return;
                }

                d7.Unfocus();
                d7.Focus();
            });
        }
        private void d7_Completed(object sender, EventArgs e)
        {
            Device.BeginInvokeOnMainThread(() =>
            {
                if (string.IsNullOrWhiteSpace(d7.Text))
                {
                    return;
                }

                d8.Unfocus();
                d8.Focus();
            });
        }
    }
}