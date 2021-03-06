﻿using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using AiForms.Dialogs;
using AiForms.Dialogs.Abstractions;
using Microsoft.AppCenter.Analytics;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;
using xamarinJKH.DialogViews;
using xamarinJKH.InterfacesIntegration;
using xamarinJKH.Main;
using xamarinJKH.Server.RequestModel;

namespace xamarinJKH.Utils
{
    public static class Settings
    {
        public static MobileSettings MobileSettings = new MobileSettings();
        public static LoginResult Person = new LoginResult();
        public static List<RequestType> TypeApp = new List<RequestType>();
        public static List<NamedValue> StatusApp = new List<NamedValue>();
        public static List<NamedValue> PrioritetsApp = new List<NamedValue>();
        public static List<string> BrandCar = new List<string>();
        public static HashSet<Page> AppPAge = new HashSet<Page>();
        public static string UpdateKey = "";
        public static string DateUniq = "";
        public static string AppVersion = "3.2";
        public static bool TimerStart = false;
        public static bool AppIsVisible = true;
        public static bool NotifVisible = true;
        public static bool QuestVisible = true;
        public static bool AddVisible = true;
        public static bool GoodsIsVisible = false;
        public static int TimerTime = 59;
        public static int coll = 0;
        public static long timeLoadReq = 0;
        public static bool IsFirsStart = true;
        public static bool ConstAuth = false;
        public static EventBlockData EventBlockData = new EventBlockData();

        public static string[] months =
        {
            "Январь", "Февраль", "Март", "Апрель", "Май", "Июнь", "Июль", "Август", "Сентябрь", "Октябрь", "Ноябрь",
            "Декабрь"
        };

        public static Color GetPriorityColor(int id)
        {
            switch (id)
            {
                case 1:
                    return Color.FromHex("#FF41B200");
                    break;
                case 2:
                    return Color.FromHex("#FF809500");
                    break;
                case 3:
                    return Color.FromHex("#FFCB5C00");
                    break;
                case 4:
                    return Color.Red;
                    break;
            }

            return Color.Black;
        }

        public static PaysPage paysPage;
        public static Page mainPage;
        public static Page Page;
        public static bool? isSelf = null;

        public static bool OnTimerTick()
        {
            if (TimerStart)
            {
                TimerTime -= 1;
                if (TimerTime < 0)
                {
                    TimerStart = false;
                    TimerTime = 59;
                }
            }

            return TimerStart;
        }

        public static AdditionalService GetAdditionalService(int id)
        {
            foreach (var each in EventBlockData.AdditionalServices)
            {
                if (each.ID == id)
                {
                    return each;
                }
            }

            return null;
        }

        public static PollInfo GetPollInfo(int id)
        {
            foreach (var each in EventBlockData.Polls)
            {
                if (each.ID == id)
                {
                    return each;
                }
            }

            return null;
        }

        public static async Task StartProgressBar(string title = "Загрузка", double opacity = 0.6)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = Color.FromHex(MobileSettings.color),
                OverlayColor = Color.Black,
                Opacity = opacity,
                DefaultMessage = title,
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                // for (var i = 0; i < 100; i++)
                // {
                //     await Task.Delay(70);
                //     // can send progress to the dialog with the IProgress.
                //     // progress.Report((i + 1) * 0.01d);
                // }
            });
        }

        public static async Task StartOverlayBackground(Color hex)
        {
            // Loading settings
            Configurations.LoadingConfig = new LoadingConfig
            {
                IndicatorColor = Color.Transparent,
                OverlayColor = Color.Black,
                Opacity = 0.8,
                DefaultMessage = "",
            };

            await Loading.Instance.StartAsync(async progress =>
            {
                // some heavy process.
                var ret = await Dialog.Instance.ShowAsync<RatingBarView>(new
                {
                    HexColor = hex
                });
            });
        }

        public static string GetStatusIcon(int id)
        {
            switch (id)
            {
                case 6:
                case 5:
                case 8:
                case 10:
                case 11:
                case 12:
                case 7: return "ic_status_done";
                case 1: return "ic_status_new";
                default: return "ic_status_wait";
            }
        }

        public static List<string> ParsingLink(String source)
        {
            List<string> links = new List<string>();
         

            string pattern = @"^(https?:\/\/)? #протокол
([\w-]{1,32}\.[\w-]{1,32}) #домен
[^\s@]* #любой не пробельный символ + @
$ ";
       

            if (String.IsNullOrEmpty(source))
                return links;
            RegexOptions options = RegexOptions.Multiline | RegexOptions.IgnorePatternWhitespace;
            foreach (Match match in Regex.Matches(source.Replace(" ", "\n"), pattern, options))
            {
                var m = match.ToString();
                if(!m.ToLower().EndsWith(".jpg") && !m.ToLower().EndsWith(".png") && !m.ToLower().EndsWith(".pdf") && !m.ToLower().EndsWith(".doc"))
                    links.Add(m);
                Console.WriteLine("Т1: " + match);
            }

            return links;
        }

        public static FormattedString FormatedLink(String source, Color color, int fontSize = 15)
        {
         
            if (String.IsNullOrEmpty(source))
                return source;
            var periodReplacement = "[[[replace:period]]]";
            source = Regex.Replace(source, @"(?<=\d)\.(?=\d)", periodReplacement);
            var linkMatches = ParsingLink(source);
            FormattedString formattedString = new FormattedString();

            foreach (string match in linkMatches)
            {
                var m = match.ToString();
                String s = (m.Contains("://")) ? m : "http://" + m;

                //if (Device.RuntimePlatform == Device.Android)
                source = source.Replace(m, "<u>" + s + "<u>");
            }

            source = source.Replace(periodReplacement, ".");

            var rep = source.Split("<u>");

            foreach (string s in rep)
            {
                if (s.Contains("https://") || s.Contains("http://") || s.Contains("www."))
                {
                    formattedString.Spans.Add(new Span()
                    {
                        Text = s, FontSize = fontSize, TextColor = Color.Blue,
                        TextDecorations = TextDecorations.Underline
                    });
                }
                else
                {
                    formattedString.Spans.Add(new Span() {Text = s, FontSize = fontSize, TextColor = color});
                }
            }

            return formattedString;
        }

        public static async Task OpenLinksMessage(string message, Page p)
        {
            try
            {
                List<string> links = ParsingLink(message);
                var linksWithHttps = new List<string>();
                foreach (var each in links)
                {
                    linksWithHttps.Add(each.Contains("://") ? each: "http://" + each);
                }
                if (links.Count > 0)
                {
                    Analytics.TrackEvent($"Поиск ссылок {links.ToString()}");
                    var action = await p.DisplayActionSheet(AppResources.OpenLink, AppResources.Cancel, null,
                        linksWithHttps.ToArray());
                    Analytics.TrackEvent($"Открытие ссылки {action}");
                    await Launcher.OpenAsync(action);
                }
                else
                {
                    return;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                Analytics.TrackEvent($"Открытие ссылки {e.Message}");
            }
        }

        public static void SetPhoneTech(string phone)
        {
            if (Person == null)
                Person = new LoginResult()
                {
                    Phone = phone
                };
            else
                Person.Phone = phone;
        }

        public static string GetHalfAddress(string address)
        {
            if (!string.IsNullOrWhiteSpace(address))
            {
                int addressLength = address.Length / 2;
                string secondHalf = address.Substring(addressLength);
                int indexSpace = secondHalf.IndexOf(" ", StringComparison.Ordinal);
                return address.Insert(addressLength + indexSpace + 1, "\n");
            }

            return "";
        }

        public static async void ChechEnabledNotification(Page page)
        {
#if DEBUG 
            Preferences.Set("DisplayNotification", true);
#endif

            bool isDisplay = Preferences.Get("DisplayNotification", true);

            if (!DependencyService.Get<ISettingsService>().IsEnabledNotification() && isDisplay)
                await PopupNavigation.Instance.PushAsync(new PushEnableCheck());
        }
    }
}