﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using Plugin.Messaging;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.PancakeView;
using Xamarin.Forms.Xaml;
using xamarinJKH.CustomRenderers;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;
using xamarinJKH.Utils.Compatator;

namespace xamarinJKH.Pays
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SaldosPage : ContentPage
    {
        public List<BillInfo> BillInfos { get; set; }
        private RestClientMP _server = new RestClientMP();
        private bool _isRefreshing = false;
        private RestClientMP server = new RestClientMP();
        private bool isSortDate = true;
        private bool isSortLs = true;
        Color hex = Color.FromHex(Settings.MobileSettings.color);

        public bool IsRefreshing
        {
            get { return _isRefreshing; }
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand RefreshCommand
        {
            get
            {
                return new Command(async () =>
                {
                    IsRefreshing = true;

                    await RefreshData();

                    IsRefreshing = false;
                });
            }
        }

        private async Task RefreshData()
        {
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            ItemsList<AccountAccountingInfo> info = await _server.GetAccountingInfo();
            if (info.Error == null)
            {
                SetBills(info.Data);
                additionalList.ItemsSource = null;
                additionalList.ItemsSource = BillInfos;
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorInfoBills, "OK");
            }
        }

        public SaldosPage(List<AccountAccountingInfo> infos)
        {
            SetBills(infos);
            InitializeComponent();

            switch (Device.RuntimePlatform)
            {
                case Device.iOS:
                    int statusBarHeight = DependencyService.Get<IStatusBar>().GetHeight();
                    if(Xamarin.Essentials.DeviceDisplay.MainDisplayInfo.Width<700)
                    {
                        kvitLabel2.FontSize = 14;
                        LabelDate.FontSize = 12;
                        LabelLs.FontSize = 12;
                    }
                    //BackgroundColor = Color.White;
                    break;
                default:
                    break;
            }

            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await PopupNavigation.Instance.PushAsync(new TechDialog()); };
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall) 
                        phoneDialer.MakePhoneCall(Settings.Person.companyPhone);
                }

            
            };
            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => { _ = await Navigation.PopAsync(); };
            var sortDate = new TapGestureRecognizer();
            sortDate.Tapped += async (s, e) => { SortDate(); };
            StackLayoutSortDate.GestureRecognizers.Add(sortDate);
            var sortLs = new TapGestureRecognizer();
            sortLs.Tapped += async (s, e) => { SortLs(); };
            StackLayoutSortIdent.GestureRecognizers.Add(sortLs);
            SetText();
            this.BindingContext = this;
            additionalList.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
        }

        void SortDate()
        {
            
             List<BillInfo> listTop = new List<BillInfo>();
            List<BillInfo> listBottom = new List<BillInfo>();
            foreach (var each in BillInfos)
            {
                if (each.Period.Split().Length > 1)
                {
                    listTop.Add(each);
                }
                else
                {
                    listBottom.Add(each);
                }
                
            }
            BillinfoComarable comarable = new BillinfoComarable();

            IconViewSortDate.Foreground = hex;
            LabelDate.TextColor = hex;
            Color fromHex = Color.FromHex("#8B8B8B");
            IconViewSortIdent.Foreground = fromHex;
            LabelLs.TextColor = fromHex;
            if (isSortDate)
            {
                IconViewSortDate.Rotation = 0;
                listTop.Sort(comarable);
                var list  = listBottom.OrderBy(u => u.Period);
                listBottom = new List<BillInfo>(list);
                isSortDate = false;
            }
            else
            {
                isSortDate = true;
                IconViewSortDate.Rotation = 180;
                listTop.Sort(comarable);
                listTop.Reverse();
                var list  = listBottom.OrderByDescending(u => u.Ident);
                listBottom = new List<BillInfo>(list);
            }
            BillInfos.Clear();
            BillInfos.AddRange(listTop);
            BillInfos.AddRange(listBottom);
            additionalList.ItemsSource = null;
            additionalList.ItemsSource = BillInfos;
        }

        private void SortLs()
        {
            Color fromHex = Color.FromHex("#8B8B8B");
            IconViewSortDate.Foreground = fromHex;
            LabelDate.TextColor = fromHex;

            IconViewSortIdent.Foreground = hex;
            LabelLs.TextColor = hex;
            if (!isSortLs)
            {
                IconViewSortIdent.Rotation = 0;
                var list  = BillInfos.OrderBy(u => u.Ident);
                BillInfos = new List<BillInfo>(list);
                isSortLs = true;
            }
            else
            {
                isSortLs = false;
                IconViewSortIdent.Rotation = 180;
                var list  = BillInfos.OrderByDescending(u => u.Ident);
                BillInfos = new List<BillInfo>(list);
             
            }

            additionalList.ItemsSource = null;
            additionalList.ItemsSource = BillInfos;
        }

        void SetBills(List<AccountAccountingInfo> infos)
        {
            BillInfos = new List<BillInfo>();
            if (infos == null)
            {
                return;
            }
            foreach (var each in infos)
            {
                if (each.Bills == null)
                {
                    continue;
                }
                foreach (var VARIABLE in each.Bills)
                {
                  
                    BillInfos.Add(VARIABLE);
                }
            }

            var list = BillInfos.OrderByDescending(u => u.Period);

            BillInfos = new List<BillInfo>(list);
        }

        void SortNotFormat()
        {
            
        }
        
        void SetText()
        {
            // IconViewSortIdent.Foreground = Color.FromHex(Settings.MobileSettings.color);

            IconViewSortDate.Foreground = hex;
            LabelDate.TextColor = hex;
            
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            FrameSaldo.SetAppThemeColor(MaterialFrame.BorderColorProperty, hexColor, Color.White);
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            BillInfo select = e.Item as BillInfo;
            if (select != null)
            {
                select.Period = select.Period.ToUpper();
                await Navigation.PushAsync(new PayPdf(new PayPdfViewModel(select)));
            }

            return;
            string filename = @select.Period + ".pdf";
            if (!select.HasFile)
            {
                return;
            }
            
            if (await DependencyService.Get<IFileWorker>().ExistsAsync(filename))
            {
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(filename))
                });
            }
            else
            {
                await Settings.StartProgressBar();
                var stream = await server.DownloadFileAsync(select.ID.ToString());
                if (stream != null)
                {
                    await DependencyService.Get<IFileWorker>().SaveTextAsync(filename, stream);
                    Loading.Instance.Hide();
                    await Launcher.OpenAsync(new OpenFileRequest
                    {
                        File = new ReadOnlyFile(DependencyService.Get<IFileWorker>().GetFilePath(filename))
                    });
                }
                else
                {
                    Loading.Instance.Hide();
                    await DisplayAlert(AppResources.ErrorTitle, "Не удалось скачать файл", "OK");
                }
            }
        }
    }
}