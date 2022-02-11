﻿#if __ANDROID__
using Android.App;
#endif 
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Xml;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace Andro2UWP
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }


        private async void uiPage_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG
            p.k.SetSettingsInt("debugLogLevel", 5); // TODO na razie tak, potem trzeba wyciąć
#endif
            p.k.SetSettingsString("appFailData", "");   // zadnych pozostalosci po poprzednim run
            p.k.ProgRingInit(true, true);

            //p.k.GetAppVers("uiVersion");
            
            App.gsDeviceName = p.k.GetSettingsString("deviceName");
            App.giCurrentNumber = p.k.GetSettingsInt("currentFileNum") + 1;
            
            await App.AddLogEntry("Andro2UWP:MainPage:Loaded called, wartosci:\n deviceName = " + App.gsDeviceName + ",\n currentFileNum =" + App.giCurrentNumber.ToString(), true);
            // sprobuj wczytac tabelke zamian - bo moze cos sie zmienilo od poprzedniego razu

            // inicjalizacja OneDrive tutaj tylko wtedy, gdy nie pierwsze uruchomienie (bez logowania do OneDrive na pierwszym start app)
            try
            {
                if (p.k.GetSettingsBool("wasInit"))
                {
                    await App.initODandDict(true);
                }
            }
            catch (Exception ex)
            {
                //Debug.WriteLine(" uiPage_Loaded - initODandDict - exception: " + ex.Message);
                Console.WriteLine(" uiPage_Loaded - initODandDict - exception: " + ex.Message);
            }

            // dla UWP nie robimy refresh - dlugo to trwa przeciez
            if (p.k.GetPlatform("android"))
            {
                RefreshListView(false);
            }

#if NETFX_CORE
            // to jest w BottomBar, którego nie ma poza UWP, więc nie można się do niego odwołać :)
            if (!p.k.IsFamilyDesktop())
                uiAutoRefresh.IsEnabled = false;    // poza Deskop nie ma sensu, bo by OS i tak wyłączał na timeout
            else
                uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");
#endif

        }

#if __ANDROID__
        private async System.Threading.Tasks.Task<bool> UslugaWlaczona()
        {
            await App.AddLogEntry("Andro2UWP:MainPage:UslugaWlaczona called", true);

            var am = (Android.Views.Accessibility.AccessibilityManager)
                Android.App.Application.Context.GetSystemService(Android.App.Application.AccessibilityService);
            var enabledServices = 
                am.GetEnabledAccessibilityServiceList(Android.AccessibilityServices.FeedbackFlags.Generic);
            await App.AddLogEntry("wlaczonych jest " + enabledServices.Count, true );
            foreach (var enabledService in enabledServices)
            {
                var enabledServiceInfo = enabledService.ResolveInfo.ServiceInfo;
                await App.AddLogEntry("np. w " + enabledServiceInfo.PackageName, true);
                if (enabledServiceInfo.PackageName == Android.App.Application.Context.PackageName)
                    return true;
            }
            await App.AddLogEntry("niestety, wylaczona", true);

            return false;
        }
#endif

        /*
        private void uiAuth_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountSelectionPage));
        }
        */

        private void uiSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));
        }
        private void uiShowLog_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AppLog));
        }

        private async void uiStartStop_Toggled(object sender, RoutedEventArgs e)
        {
            // ten guzik jest tylko Androidowy

#if !__ANDROID__
            uiStartStop.IsEnabled = false;
#endif

            if (p.k.GetPlatform("uwp"))
            {
                p.k.DialogBox("jakim cudem nacisnales to nie bedac na Androidzie?");

#if !__ANDROID__
                uiStartStop.IsEnabled = true;
#endif
                return;
            }


            bool b = true;

#if !__ANDROID__
            b = uiStartStop.IsOn;
#endif
            if (!b)
            {
                await App.AddLogEntry("Stopping... @" + DateTime.Now.ToString(), false);
                App.gbPrzechwytuj = false;

//#if !__ANDROID__
               // App.gOnedrive?.Dispose();
               // App.gOnedrive = null;               
//#endif

#if !__ANDROID__
                uiStartStop.IsEnabled = true;
#endif
                
                return;
            }

            // 2020.06.29: przestawienie OneDrive przed Accessibility, jako ze pkarmode jest z onedrive a zmienia Accessibility
            if (!await App.initODandDict(true)) return;  // inicjalizcja OneDrive gdy jeszcze nie bylo, oraz wczytanie slownikow (gdy sie zmienily, wystarczy off/on i juz jest aktualne)

#if __ANDROID__
            if (!await UslugaWlaczona())
            {
#else
            if (1 ==1)
            { 
#endif
            bool bIgnore = false;

                if(p.k.GetSettingsBool("pkarMode"))
                {
                    if (await p.k.DialogBoxYNAsync("usluga niby niewlaczona, zignorowac to?"))
                        bIgnore = true;
                }

                if (!bIgnore)
                {
                    await p.k.DialogBoxResAsync("msgAccessibilityFirst");

#if __ANDROID__
                    var intent = new Android.Content.Intent(Android.Provider.Settings.ActionAccessibilitySettings);
                    Uno.UI.BaseActivity.Current.StartActivity(intent);
#endif

#if !__ANDROID__
                    uiStartStop.IsOn = false;
                    uiStartStop.IsEnabled = true;
#endif
                    return;
                }
            }


            p.k.ProgRingShow(true);
            // jak tu jestesmy, to na pewno jest to wlaczanie przechwytywania
            await App.AddLogEntry("Starting... @" + DateTime.Now.ToString(), false);

            // ustalenie koniecznych zmiennych
            string deviceName = "default";
            if (string.IsNullOrEmpty(App.gsDeviceName) || App.gsDeviceName == "default")
            {
                var AndOSBuildModel = "";
#if __ANDROID__
                AndOSBuildModel = Android.OS.Build.Model;
#endif
                deviceName = await p.k.DialogBoxInputDirectAsync
                    (p.k.GetLangString("msgEnterDevName"),
                    AndOSBuildModel, "resDlgSetName"
                    );

                if (deviceName != "")
                    p.k.SetSettingsString("deviceName", deviceName);
                App.gsDeviceName = deviceName;
            }
            await App.AddLogEntry("mamy devicename=" + deviceName, true);

            if (!await App.EnsureOneDriveOpen(true))
            {
                await App.AddLogEntry("FAIL cannot open OneDrive", false);
#if __ANDROID__
                App.gOnedrive?.Dispose();
                App.gOnedrive = null;
#endif

#if !__ANDROID__
                uiStartStop.IsOn = false;
#endif
                
                p.k.ProgRingShow(false);
#if !__ANDROID__
                uiStartStop.IsEnabled = true;
#endif
                
                return;
            }

            await App.AddLogEntry("startujemy!", false);

            // wszystko gotowe, mozesz wyłapywać
            App.gbPrzechwytuj = true;
#if !__ANDROID__
            uiStartStop.IsEnabled = true;
#endif
            p.k.ProgRingShow(false);


        }//..._Toggled end

        private async void uiClearList_Click(object sender, RoutedEventArgs e)
        {
            // ma byc: resetlisty (zeby pozbyc sie starszych)
            //if (p.k.GetPlatform("uwp"))
            //{
            //    // visibility off, bo niby jak? tozsame z delete all?
            //    await p.k.DialogBoxAsync("jeszcze nie umiem tego zrobic");
            //}
            //else
            //{// na Android: czyscimy nie OneDrive, tylko to co na ekranie

                App.gToasty.Clear();
#if !__ANDROID__
                uiList.ItemsSource = App.gToasty.ToList();
#endif
            //}

        }//uiClearList_Click end


        // uiItem_DoubleTapped
        private void uiItem_DoubleTapped(object sender, RoutedEventArgs e)
        {
            Grid oGrid;
            oGrid = sender as Grid;
            if (oGrid is null)  return;
            p.k.DialogBox(((App.JedenToast)oGrid.DataContext).sMessage);
        }

        private App.JedenToast MFIdataContext(object sender)
        {
            var mfi = (MenuFlyoutItem)sender;
            if (mfi is null)
                return null;
            return (App.JedenToast)mfi.DataContext;
        }
        private void uiDetails_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;
            p.k.DialogBox(toast.sMessage);
        }
        private async void uiRenameSource_Click(object sender, RoutedEventArgs e)
        {
            if (!p.k.GetPlatform("uwp"))
            {
                p.k.DialogBox("jakim cudem to nacisnales bedac na Androidzie?");
#if !__ANDROID__
                uiStartStop.IsEnabled = true;
#endif
                return;
            }

            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;
            var newName = await p.k.DialogBoxInputDirectAsync(
                p.k.GetLangString("msgSourceShould1") + "'" + toast.sSource + "' " + 
                p.k.GetLangString("msgSourceShould2") , toast.sSource);
            if (string.IsNullOrEmpty(newName)) return;
            App.gdSenderRenames.Add(toast.sSource, newName);

            // zapisujemy...
            p.k.ProgRingShow(true);

            string dictionaryFile = "";
            foreach (var entry in App.gdSenderRenames)
            {
                dictionaryFile = dictionaryFile + entry.Key + "|" + entry.Value + "\n";
            }

#if ((!__ANDROID__) && (!__WASM__))
            await p.od.ReplaceOneDriveFileContent("Apps/Andro2UWP/sender.renames.txt", dictionaryFile);
#endif
            RefreshListView(false);
            p.k.ProgRingShow(false);
        }

        private async System.Threading.Tasks.Task UsunPliki(List<string> lista, bool bMsg)
        {
            p.k.ProgRingShow(true, false, 0, lista.Count);
#if !__ANDROID__
            await p.od.UsunPlikiOneDrive("Apps/Andro2UWP", lista);

            for(int iLp = App.gToasty.Count-1; iLp>=0; iLp--)
            {
                var toast = App.gToasty.ElementAt(iLp);
                if(lista.Contains(toast.sFileName))
                {
                    App.gToasty.RemoveAt(iLp);
                }
            }
            // i pokaz nową wersję listy
            RefreshListView(bMsg);
#endif
            p.k.ProgRingShow(false);
        }

        private void uiDeleteThis_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;

            // from list, oraz from OneDrive

            var lista = new List<string>();
            lista.Add(toast.sFileName);
            UsunPliki(lista, false);
        }
        private async void uiDeleteOlder_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;

            if (!await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveOlder") + "\n" + toast.displayDate + " ?"))
                return;

            // from list, oraz from OneDrive
            var lista = new List<string>();
            foreach(var item in App.gToasty)
            {
                if (item.displayDate.CompareTo(toast.displayDate) < 0)
                    lista.Add(item.sFileName);
            }
            await UsunPliki(lista,false);
        }

        private async void uiDeleteThisOlder_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);

            if (toast is null) return;

            if (!await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveThisOlder")))
                return;

            // from list, oraz from OneDrive
            var lista = new List<string>();
            foreach (var item in App.gToasty)
            {
                if (item.displayDate.CompareTo(toast.displayDate) <= 0)
                    lista.Add(item.sFileName);
            }
            await UsunPliki(lista,false);
        }
        private void uiCopy_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            p.k.ClipPut(toast.ToString());
        }

        private async void uiDeleteSender_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;
            // from list, oraz from OneDrive

            if (!await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveSender") + "\n" + toast.displaySource + " ?"))
                return;

            // from list, oraz from OneDrive
            var lista = new List<string>();
            foreach (var item in App.gToasty)
            {
                if (item.displaySource == toast.displaySource)
                    lista.Add(item.sFileName);
            }
            await UsunPliki(lista,false);

        }
        private async void uiCreateFilter_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;

            var oStack = new Windows.UI.Xaml.Controls.StackPanel();
            //var oDlgTitle = new Windows.UI.Xaml.Controls.TextBlock();
            //oDlgTitle.Text = "New filter";
            //oDlgTitle.HorizontalAlignment = HorizontalAlignment.Center;
            //oDlgTitle.FontSize = 18;
            //oDlgTitle.Margin = new Thickness(0, 0, 0, 10);
            //oStack.Children.Add(oDlgTitle);

            var oPackage = new Windows.UI.Xaml.Controls.TextBox();
            oPackage.Text = toast.sSource;
            oPackage.Header = "Source package:";
            oStack.Children.Add(oPackage);

            var oTitle = new Windows.UI.Xaml.Controls.TextBox();
            oTitle.Header = "Title:";
            var oText = new Windows.UI.Xaml.Controls.TextBox();
            oText.Header = "Text:";
            foreach(var linia in toast.sMessage.Split('\n'))
            {
                if (linia.StartsWith("Title: "))
                    oTitle.Text = linia.Substring(7);
                if (linia.StartsWith("Text: "))
                    oText.Text = linia.Substring(6);
            }
            oStack.Children.Add(oTitle);
            oStack.Children.Add(oText);

            Windows.UI.Xaml.Controls.ContentDialog oDlg = new Windows.UI.Xaml.Controls.ContentDialog();
            oDlg.Content = oStack;
            oDlg.PrimaryButtonText = p.k.GetLangString("msgCancel");
            oDlg.SecondaryButtonText = p.k.GetLangString("msgAdd"); 
            oDlg.Title = p.k.GetLangString("msgAddFilterTitle");

            var oCmd = await oDlg.ShowAsync();
            //#if !NETFX_CORE
            //            oDlg.Dispose();
            //#endif
            if (oCmd == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
                return;

            App.glFiltry.Add(new App.JedenFiltr(oPackage.Text, oTitle.Text, oText.Text));
            // save filtry

            //ProgresywnyRing(true);
            p.k.ProgRingShow(true);


            string dictionaryFile = "";
            foreach (var entry in App.glFiltry)
            {
                dictionaryFile = dictionaryFile + entry.sPackageName + "|" + entry.sTitle + "|" + entry.sText + "\n";
            }

#if !__ANDROID__
            await p.od.ReplaceOneDriveFileContent("Apps/Andro2UWP/toasts.filters.txt", dictionaryFile);
#endif
            // uiRefreshList_Click(null, null); // <-- to i tak nie filtruje teraz, wiec nie bawimy sie w to
            //ProgresywnyRing(false);
            p.k.ProgRingShow(false);

        }

        private async void uiRefreshList_Click(object sender, RoutedEventArgs e)
        {
#if !__ANDROID__
            uiRefreshList.IsEnabled = false;
#endif

            if (p.k.GetPlatform("uwp"))
            {
                if (!await App.LoadNews(true))
                {
                    return;
                }

                //if (!await App.initODandDict()) return;     // przede wszystkim - odczytanie slownikow na nowo (żeby nie było reset słownika!)
                //await App.WczytajNowe();
            }
#if !__ANDROID__
            uiRefreshList.IsEnabled = true;
#endif

            //if (sender != null)   // nie ma wywołania innego niż z Event guzika, więc zawsze sender <> null?
            //{
            //}

            RefreshListView(true);
        }

        private void RefreshListView(bool bMsg)
        {
            
            // najpierw uzupelnij liste (elementy "display")
            // nie 'foriczem', bo by było Exception ze zmiana w trakcie foreach
            for (int iLoop = 0; iLoop < App.gToasty.Count; iLoop++)
            {
                var oItem = App.gToasty.ElementAt(iLoop);
                // koment-out, bo nie uzywane, szkoda czasu (szczegolnie Android)
                //if (string.IsNullOrEmpty(oItem.displayDevice))
                //{
                //    if (App.gsDeviceName != oItem.sDevice)
                //        oItem.displayDevice = App.gsDeviceName;
                //}

                //oItem.displayDate = oItem.dDate.ToString("dd-MM-yyyy HH:mm");
                // displayDate - bierze podczas wczytywania z nazwy pliku

                // podmiana source ze slownika korzystajac
                oItem.displaySource = oItem.sSource;
                if (!string.IsNullOrEmpty(oItem.sSource))
                {
                    if (App.gdSenderRenames.ContainsKey(oItem.sSource))
                    {
                        string tempek;
                        App.gdSenderRenames.TryGetValue(oItem.sSource, out tempek);
                        oItem.displaySource = tempek;
                    }
                }
            }

            // dopiero pozniej ją pokaż
            if (p.k.GetSettingsBool("sortDescending", true))
            {
#if !__ANDROID__
                uiList.ItemsSource = (from item in App.gToasty orderby item.sFileName descending select item).ToList();
#endif
            }
            else
            {
#if !__ANDROID__
                uiList.ItemsSource = App.gToasty.ToList();
#endif
            }

            // oraz ewentualnie usuń guzik kasowania listy
            if (App.gToasty.Count > 0)
            {

                if (p.k.GetPlatform("uwp"))
                {
#if !__ANDROID__
                    uiClearList.Visibility = Visibility.Visible;
#endif
                }

            }
                else
            {
                if (bMsg) p.k.DialogBoxRes("msgNoData");

                if (p.k.GetPlatform("uwp"))
                {
#if !__ANDROID__
                    uiClearList.Visibility = Visibility.Collapsed;
#endif
                }
            }


        }

#if NETFX_CORE
        // zabezpieczenie - uiAutoRefresh nie istnieje poza UWP
        private async void uiAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            if(!uiAutoRefresh.IsChecked.HasValue) return;
            if(uiAutoRefresh.IsChecked.Value)
            {
                if(!p.k.IsFamilyDesktop())
                {
                    p.k.DialogBox("Sorry, but this would work only on desktop");    // bez .resw, bo i tak to powinno być zablokowane
                    return;
                }
                Windows.ApplicationModel.Background.SystemCondition oCondition =
                    new Windows.ApplicationModel.Background.SystemCondition(Windows.ApplicationModel.Background.SystemConditionType.InternetAvailable);
                p.k.RegisterTimerTrigger("Andro2UWP_Timer", 15, false, oCondition);
            }
            else
            {
                p.k.UnregisterTriggers("Andro2UWP_Timer");
            }
        }
#endif

        //private void ProgresywnyRing(bool sStart)
        //    {
        //        if (sStart)
        //        {
        //            double dVal;
        //            dVal = Math.Min(uiGrid.ActualHeight, uiGrid.ActualWidth) / 2;
        //            uiProcesuje.Width = dVal;
        //            uiProcesuje.Height = dVal;

        //            uiProcesuje.Visibility = Visibility.Visible;
        //            uiProcesuje.IsActive = true;
        //        }
        //        else
        //        {
        //            uiProcesuje.IsActive = false;
        //            uiProcesuje.Visibility = Visibility.Collapsed;
        //        }
        //    }




    }
}


