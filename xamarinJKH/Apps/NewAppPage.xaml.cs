﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Plugin.FilePicker;
using Plugin.FilePicker.Abstractions;
using Plugin.Media;
using Plugin.Media.Abstractions;
using Plugin.Messaging;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server;
using xamarinJKH.Server.RequestModel;
using xamarinJKH.Tech;
using xamarinJKH.Utils;

using Plugin.Permissions;
using Plugin.Permissions.Abstractions;
using Rg.Plugins.Popup.Services;
using Xamarin.Forms.PancakeView;
using xamarinJKH.DialogViews;
using Xamarin.Essentials;
using System.Text.RegularExpressions;

namespace xamarinJKH.Apps
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class NewAppPage : ContentPage
    {
        private RestClientMP _server = new RestClientMP();
        public List<FileData> files { get; set; }
        public List<byte[]> Byteses = new List<byte[]>();
        private AppsPage _appsPage;
        string TAKE_PHOTO = AppResources.AttachmentTakePhoto;
        string TAKE_GALRY = AppResources.AttachmentChoosePhoto;
        string TAKE_FILE = AppResources.AttachmentChooseFile;
        const string CAMERA = "camera";
        const string GALERY = "galery";
        const string FILE = "file";
        public int PikerLsItem = 0;
        public int PikerTypeItem = 0;

        public NewAppPage()
        {
            InitializeComponent();

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
                    //BackgroundColor = Color.White;
                    // ImageTop.Margin = new Thickness(0, 0, 0, 0);
                    // StackLayout.Margin = new Thickness(0, 33, 0, 0);
                    // IconViewNameUk.Margin = new Thickness(0, 33, 0, 0);
                    // if (Application.Current.MainPage.Height > 800)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -180);
                    //     BackStackLayout.Margin = new Thickness(-5, 35, 0, 0);
                    // }
                    break;
                case Device.Android:
                    // ScrollViewContainer.Margin = new Thickness(0, 0, 0, -162);
                    // double or = Math.Round(((double) App.ScreenWidth / (double) App.ScreenHeight), 2);
                    // if (Math.Abs(or - 0.5) < 0.02)
                    // {
                    //     ScrollViewContainer.Margin = new Thickness(0, 0, 0, -115);
                    //     BackStackLayout.Margin = new Thickness(-5, 15, 0, 0);
                    // }

                    break;
                default:
                    break;
            }

            NavigationPage.SetHasNavigationBar(this, false);
            var backClick = new TapGestureRecognizer();
            backClick.Tapped += async (s, e) => {
                try
                {
                    _ = await Navigation.PopAsync();
                }
                catch { }
            };
            BackStackLayout.GestureRecognizers.Add(backClick);
            var techSend = new TapGestureRecognizer();
            techSend.Tapped += async (s, e) => {     await PopupNavigation.Instance.PushAsync(new TechDialog());};
            LabelTech.GestureRecognizers.Add(techSend);
            var call = new TapGestureRecognizer();
            call.Tapped += async (s, e) =>
            {
                if (Settings.Person.Phone != null)
                {
                    IPhoneCallTask phoneDialer;
                    phoneDialer = CrossMessaging.Current.PhoneDialer;
                    if (phoneDialer.CanMakePhoneCall && !string.IsNullOrWhiteSpace(Settings.Person.companyPhone)) 
                        phoneDialer.MakePhoneCall(System.Text.RegularExpressions.Regex.Replace(Settings.Person.companyPhone, "[^+0-9]", ""));
                }

            
            };
            LabelPhone.GestureRecognizers.Add(call);
            var addFile = new TapGestureRecognizer();
            addFile.Tapped += async (s, e) => { AddFile(); };
            StackLayoutAddFile.GestureRecognizers.Add(addFile);

            SetText();
            files = new List<FileData>();
            BindingContext = new AddAppModel()
            {
                AllAcc = Settings.Person.Accounts,
                AllType = Settings.TypeApp,
                hex = Color.FromHex(Settings.MobileSettings.color),
                SelectedAcc = Settings.Person.Accounts[0],
                SelectedType = Settings.TypeApp[0],
                Files = files
            };
            ListViewFiles.Effects.Add(Effect.Resolve("MyEffects.ListViewHighlightEffect"));
        }

        protected async override void OnAppearing()
        {
            base.OnAppearing();
            await Task.Delay(TimeSpan.FromSeconds(1));
           
        }

        private async void AddFile()
        {

            if (Device.RuntimePlatform == "Android")
            {
                try
                {
                    var camera_perm = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Camera);
                    var storage_perm = await Plugin.Permissions.CrossPermissions.Current.CheckPermissionStatusAsync(Permission.Storage);
                    if (camera_perm != PermissionStatus.Granted || storage_perm != PermissionStatus.Granted)
                    {
                        var status = await CrossPermissions.Current.RequestPermissionsAsync(Permission.Camera, Permission.Storage);
                        if (status[Permission.Camera] == PermissionStatus.Denied && status[Permission.Storage] == PermissionStatus.Denied)
                        {
                            return;
                        }
                    }
                }
                catch
                {
                    Device.BeginInvokeOnMainThread(async () =>
                    {
                        var result = await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoPermissions, "OK", AppResources.Cancel);
                        if (result)
                            Plugin.Permissions.CrossPermissions.Current.OpenAppSettings();

                    });
                    return;
                }
            }
            var action = await DisplayActionSheet(AppResources.AttachmentTitle, AppResources.Cancel, null,
                TAKE_PHOTO,
                TAKE_GALRY, TAKE_FILE);
            try
            {
                        if (action == TAKE_PHOTO)
                        {
                            await getCameraFile();
                            return;
                        }
                        if (action == TAKE_GALRY)
                        {
                            await GetGalaryFile();
                            return;
                        }
                        if (action == TAKE_FILE)
                        {
                            await PickAndShowFile(null);
                            return;
                        }
            }
            catch (Exception ex)
            {
                
            }
        }

        private async void PickImage_Clicked(object sender, EventArgs args)
        {
            string[] fileTypes = null;

            if (Device.RuntimePlatform == Device.Android)
            {
                fileTypes = new string[] {"image/png", "image/jpeg"};
            }

            if (Device.RuntimePlatform == Device.iOS)
            {
                fileTypes = new string[] {"public.image"}; // same as iOS constant UTType.Image
            }

            if (Device.RuntimePlatform == Device.UWP)
            {
                fileTypes = new string[] {".jpg", ".png"};
            }

            if (Device.RuntimePlatform == Device.WPF)
            {
                fileTypes = new string[] {"JPEG files (*.jpg)|*.jpg", "PNG files (*.png)|*.png"};
            }

            await PickAndShowFile(fileTypes);
        }

        private async Task PickAndShowFile(string[] fileTypes)
        {
            try
            {
                FileData pickedFile = await CrossFilePicker.Current.PickFile(fileTypes);

                if (pickedFile != null)
                {
                    // UkName.Text = pickedFile.FileName;
                    // LabelPhone.Text = pickedFile.FilePath;
                    if (pickedFile.DataArray.Length > 10000000)
                    {
                        await DisplayAlert(AppResources.ErrorTitle,AppResources.FileTooBig, "OK");
                        return;
                    }

                    files.Add(pickedFile);
                    Byteses.Add(pickedFile.DataArray);
                    ListViewFiles.IsVisible = true;
                    if (ListViewFiles.HeightRequest < 120)
                        ListViewFiles.HeightRequest += 30;
                    setBinding();
                    // if (pickedFile.FileName.EndsWith("jpg", StringComparison.OrdinalIgnoreCase)
                    //     || pickedFile.FileName.EndsWith("png", StringComparison.OrdinalIgnoreCase))
                    // {
                    //     IconViewNameUk.Source = ImageSource.FromStream(() => pickedFile.GetStream());
                    //     IconViewNameUk.IsVisible = true;
                    // }
                    // else
                    // {
                    //     IconViewNameUk.IsVisible = false;
                    // }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert(AppResources.ErrorTitle, ex.ToString(), "OK");
            }
        }

        async Task getCameraFile()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsTakePhotoSupported || !CrossMedia.Current.IsCameraAvailable)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorCameraNotAvailable, "OK");

                return;
            }

            MediaFile file = await CrossMedia.Current.TakePhotoAsync(
                new StoreCameraMediaOptions
                {
                    SaveToAlbum = true,
                    Directory = "Demo"
                });

            if (file == null)
                return;
            FileData fileData = new FileData( file.Path,getFileName(file.Path), () => file.GetStream() );
            Byteses.Add(StreamToByteArray(file.GetStream()));
            files.Add(fileData);
            ListViewFiles.IsVisible = true;
            if (ListViewFiles.HeightRequest < 120)
                ListViewFiles.HeightRequest += 30;
            setBinding();
        }
        
        async Task GetGalaryFile()
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsPickPhotoSupported)
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorGalleryNotAvailable, "OK");

                return;
            }

            var file = await CrossMedia.Current.PickPhotoAsync();
            if (file == null)
                return;
            FileData fileData = new FileData( file.Path,getFileName(file.Path), () => file.GetStream() );
            Byteses.Add(StreamToByteArray(file.GetStream()));
            files.Add(fileData);
            ListViewFiles.IsVisible = true;
            if (ListViewFiles.HeightRequest < 120)
                ListViewFiles.HeightRequest += 30;
            setBinding();
            PickerLs.SelectedIndex = PikerLsItem;
            PickerType.SelectedIndex = PikerTypeItem;
        }
        
        public async Task startLoadFile(string metod)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = Color.FromHex(Settings.MobileSettings.color),
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = AppResources.LoadingFile,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                switch (metod)
                {
                    case CAMERA:
                        // await getCameraFile();
                        break;
                    case GALERY:
                        // await GetGalaryFile();
                        break;
                    case FILE:
                        // await PickAndShowFile(null);
                        break;
                }
            });
        }

        public static byte[] StreamToByteArray(Stream stream)
        {
            if (stream is MemoryStream)
            {
                return ((MemoryStream) stream).ToArray();
            }
            else
            {
                return ReadFully(stream);
            }
        }

        string getFileName(string path)
        {
            try
            {
                string[] fileName = path.Split('/');
                return fileName[fileName.Length - 1];
            }
            catch (Exception ex)
            {
                return "filename";
            }
        }

        public static byte[] ReadFully(Stream input)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                input.CopyTo(ms);
                return ms.ToArray();
            }
        }

        void SetText()
        {
            UkName.Text = Settings.MobileSettings.main_name;
            LabelPhone.Text =  "+" + Settings.Person.companyPhone.Replace("+","");
            Color hexColor = (Color) Application.Current.Resources["MainColor"];
            IconViewLogin.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            IconViewTech.SetAppThemeColor(IconView.ForegroundProperty, hexColor, Color.White);
            Pancake.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);
            PancakeViewIcon.SetAppThemeColor(PancakeView.BorderColorProperty, hexColor, Color.Transparent);if (Device.RuntimePlatform == Device.iOS){ if (AppInfo.PackageName == "rom.best.saburovo" || AppInfo.PackageName == "sys_rom.ru.tsg_saburovo"){PancakeViewIcon.Padding = new Thickness(0);}}
            LabelTech.SetAppThemeColor(Label.TextColorProperty, hexColor, Color.White);
            FrameTop.SetAppThemeColor(Frame.BorderColorProperty, hexColor, Color.White);
        }

        void setBinding()
        {
            PikerLsItem = PickerLs.SelectedIndex;
            PikerTypeItem = PickerType.SelectedIndex;
            try
            {
                BindingContext = null;
                BindingContext = new AddAppModel()
                {
                    AllAcc = Settings.Person.Accounts,
                    AllType = Settings.TypeApp,
                    hex = Color.FromHex(Settings.MobileSettings.color),
                    SelectedAcc = Settings.Person.Accounts[PikerLsItem],
                    SelectedType = Settings.TypeApp[PikerTypeItem],
                    Files = files
                };
            }
            catch (Exception ex)
            {
                // ignored
            }
        }

        private void picker_SelectedIndexChanged(object sender, EventArgs e)
        {
            // try
            // {
            //     var identLength = Settings.Person.Accounts[PickerLs.SelectedIndex].Ident.Length;
            //     if (identLength < 6)
            //     {
            //         PickerLs.WidthRequest = identLength * 9;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     // ignored
            // }
        }

        public class AddAppModel
        {
            public List<AccountInfo> AllAcc { get; set; }
            public List<NamedValue> AllType { get; set; }
            public AccountInfo SelectedAcc { get; set; }
            public NamedValue SelectedType { get; set; }

            public List<FileData> Files { get; set; }
            public Color hex { get; set; }
        }

        private async void addApp(object sender, EventArgs e)
        {
            string text = EntryMess.Text;
            FrameBtnAdd.IsVisible = false;
            progress.IsVisible = true;
            if (!text.Equals(""))
            {
                try
                {
                    if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
                    {
                        Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                        return;
                    }
                    string ident = Settings.Person.Accounts[PickerLs.SelectedIndex].Ident;
                    string typeId = Settings.TypeApp[PickerType.SelectedIndex].ID;
                    IDResult result = await _server.newApp(ident, typeId, text);
                    var update = await _server.GetRequestsUpdates(Settings.UpdateKey, result.ID.ToString());
                    Settings.UpdateKey = update.NewUpdateKey;


                    if (result.Error == null)
                    {
                        sendFiles(result.ID.ToString());
                        await DisplayAlert(AppResources.AlertSuccess, AppResources.AppCreated, "OK");
                        try
                        {
                            _ = await Navigation.PopAsync();
                        }
                        catch { }
                    }
                    else
                    {
                        await DisplayAlert(AppResources.ErrorTitle, result.Error, "OK");
                    }
                    MessagingCenter.Send<Object>(this, "UpdateIdent");
                    MessagingCenter.Send<Object>(this, "UpdateEvents");
                    MessagingCenter.Send<Object>(this, "AutoUpdate");
                }
                catch (Exception ex)
                {
                    // ignored
                }
            }
            else
            {
                await DisplayAlert(AppResources.ErrorTitle, AppResources.AppErrorFill, "OK");
            }

            FrameBtnAdd.IsVisible = true;
            progress.IsVisible = false;
        }

        private async void OnItemTapped(object sender, ItemTappedEventArgs e)
        {
            FileData select = e.Item as FileData;
            bool answer = await DisplayAlert(AppResources.Delete, AppResources.DeleteFile, AppResources.Yes, AppResources.No);
            if (answer)
            {
                int indexOf = files.IndexOf(@select);
                Byteses.RemoveAt(indexOf);
                files.RemoveAt(indexOf);
                ListViewFiles.HeightRequest -= 30;
                if (files.Count == 0)
                {
                    ListViewFiles.IsVisible = false;
                }

                setBinding();
            }
        }


        async void sendFiles(string id)
        {
            int i = 0; 
            if (Xamarin.Essentials.Connectivity.NetworkAccess != Xamarin.Essentials.NetworkAccess.Internet)
            {
                Device.BeginInvokeOnMainThread(async () => await DisplayAlert(AppResources.ErrorTitle, AppResources.ErrorNoInternet, "OK"));
                return;
            }
            foreach (var each in files)
            {
                CommonResult commonResult = await _server.AddFileApps(id, each.FileName, Byteses[i],
                    each.FilePath);
                i++;
            }
        }

        private void pickerType_SelectedIndexChanged(object sender, EventArgs e)
        {
            // try
            // {
            //     var identLength = Settings.TypeApp[PickerType.SelectedIndex].Name.Length;
            //     if (identLength < 6)
            //     {
            //         PickerType.WidthRequest = identLength * 10;
            //     }
            // }
            // catch (Exception ex)
            // {
            //     // ignored
            // }
        }
    }
}