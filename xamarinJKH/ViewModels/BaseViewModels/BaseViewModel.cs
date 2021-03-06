﻿using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.DialogViews;
using xamarinJKH.MainConst;
using xamarinJKH.Server;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

namespace xamarinJKH.ViewModels
{
    public class BaseViewModel:INotifyPropertyChanged
    {
        public double Width => App.ScreenWidth2;
        
        bool _isLoading;
        public bool IsLoading
        {
            set
            {
                _isLoading = value;
                if (_isLoading)
                {
                    Loading.Instance.Show(LoadingMessage);
                }
                else
                {
                    Loading.Instance.Hide();
                }
            }
        }
        public string LoadingMessage { get; set; }
        bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                OnPropertyChanged("IsBusy");
            }
        }

        bool isRefreshing;
        public bool IsRefreshing
        {
            get => isRefreshing;
            set
            {
                isRefreshing = value;
                OnPropertyChanged("IsRefreshing");
            }
        }
        private bool _isChangeTheme;

        public bool IsChangeTheme
        {
            get => _isChangeTheme;
            set
            {
                _isChangeTheme = value;
                OnPropertyChanged(nameof(IsChangeTheme));
            }
        }
        string title;
        public string Title
        {
            get => Settings.MobileSettings.main_name;// title;
        }
        string phone;
        public string Phone => Settings.Person.companyPhone;
        public event PropertyChangedEventHandler PropertyChanged;
        public RestClientMP Server => DependencyService.Get<RestClientMP>(DependencyFetchTarget.NewInstance);
        protected void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            var changed = PropertyChanged;
            if (changed == null)
                return;

            changed.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void ShowToast(string title)
        {
            Device.BeginInvokeOnMainThread(async () => Toast.Instance.Show<ToastDialog>(new
            {
                Title = title, Duration = 500, ColorB = Color.Gray,
                ColorT = Color.White
            }));
        }
        
        protected void ShowError(string error, string title = "Ошибка")
        {
            Device.BeginInvokeOnMainThread(async () =>
            {
                await Application.Current.MainPage.DisplayAlert(title, error, "OK");
            });
        }
        protected async Task<bool> ShowChoose(string title)
        {
            return await Application.Current.MainPage.DisplayAlert("",title, AppResources.Yes, AppResources.No );
        }
        protected async Task<bool> ShowDialog<T>(BaseViewModel model) where T: DialogView
        {
            return await Dialog.Instance.ShowAsync<T>(model);
        }
        

        
        
        
    }
}
