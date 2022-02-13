// MainPage
// Both for UWP and Android
// 2022

#if __ANDROID__
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

// Andro2UWP namespace
namespace Andro2UWP
{
    // MainPage class
    public sealed partial class MainPage : Page
    {

        // MainPage
        public MainPage()
        {
            this.InitializeComponent();

        }//MainPage()


        // uiPage_Loaded
        private async void uiPage_Loaded(object sender, RoutedEventArgs e)
        {
#if DEBUG 
            p.k.SetSettingsInt("debugLogLevel", 5); // TODO: for now, yes, then you need to cut
#endif 
            p.k.SetSettingsString("appFailData", "");   // no leftovers from the previous run
            p.k.ProgRingInit(true, true);

            //p.k.GetAppVers("uiVersion");
            
            App.gsDeviceName = p.k.GetSettingsString("deviceName");
            App.giCurrentNumber = p.k.GetSettingsInt("currentFileNum") + 1;
            
            await App.AddLogEntry("Andro2UWP:MainPage:Loaded called, values:\n deviceName = " + App.gsDeviceName + ",\n currentFileNum =" + App.giCurrentNumber.ToString(), true);
            // on early days, when it was necessary to complete a task to ease the burden in the table... in general, this is from the previous time

            // initialize OneDrive here only when not first start (without logging into OneDrive on first start app)
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
            }

            // for UWP we do not refresh - it takes a long time
            // RnD: UWP refresh when app started (Main page loaded)
            if  (p.k.GetPlatform("android")) // (1 == 0)
            {

                //if (p.k.GetPlatform("uwp"))
                //{
                if (!await App.LoadNews(true))
                {
                    return;
                }

                    //if (!await App.initODandDict())
                    //    return;     // first of all-reading the dictionaries anew (so as not to reset the dictionary!)

                    //await App.WczytajNowe();
                //}


                RefreshListView(false);
            }

#if NETFX_CORE

            //RnD
            // this is in BottomBar, which is not present outside UWP, so you can not refer to it :)
            if (p.k.GetPlatform("android")) //(!p.k.IsFamilyDesktop())
            {
                // besides the desktop does not make sense, because the OS
                // and so turned off on timeout
                uiAutoRefresh.IsEnabled = false;                  
            }
            else
            {
                uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");
            }
#endif 

        }//uiPage_Loaded end


#if __ANDROID__
        // UslugaWlaczona / Service included
        private async System.Threading.Tasks.Task<bool> UslugaWlaczona()
        {
            await App.AddLogEntry("Andro2UWP:MainPage:Service included called", true);

            var am = (Android.Views.Accessibility.AccessibilityManager)
                Android.App.Application.Context.GetSystemService
                (
                    Android.App.Application.AccessibilityService
                );

            var enabledServices = 
                am.GetEnabledAccessibilityServiceList
                (
                    Android.AccessibilityServices.FeedbackFlags.Generic
                );

            await App.AddLogEntry("included is " + enabledServices.Count, true );
            
            foreach (var enabledService in enabledServices)
            {
                var enabledServiceInfo = enabledService.ResolveInfo.ServiceInfo;
                
                await App.AddLogEntry("np. w " + enabledServiceInfo.PackageName, true);
                
                if (enabledServiceInfo.PackageName == Android.App.Application.Context.PackageName)
                {
                    return true;
                }
            }

            await App.AddLogEntry("unfortunately, disabled / wylaczona", true);

            return false;

        }//UslugaWlaczona end
#endif

        /*
        // TEMP

        // uiAuth_Click
        private void uiAuth_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountSelectionPage));
        }//uiAuth_Click end
        */


        // uiSettings_Click
        private void uiSettings_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(Settings));

        }//uiSettings_Click end


        // uiShowLog_Click
        private void uiShowLog_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AppLog));

        }//uiShowLog_Click end


        // uiStartStop_Toggled
        private async void uiStartStop_Toggled(object sender, RoutedEventArgs e)
        {
            // this button is Android only
#if __ANDROID__
            uiStartStop.IsEnabled = false;
#endif

            if (p.k.GetPlatform("uwp"))
            {
                p.k.DialogBox("How did you press this not to be on Android? ;)");
                
                uiStartStop.IsEnabled = true;
                
                return;
            }

#if __ANDROID__
            if (!uiStartStop.IsOn)
            {
                await App.AddLogEntry("Stopping... @" + DateTime.Now.ToString(), false);
                App.gbPrzechwytuj = false;

                //TODO
                //App.gOnedrive?.Dispose();
                //App.gOnedrive = null;

                uiStartStop.IsEnabled = true;
                
                return;
            }

            // 2020.06.29: Move OneDrive before Accessibility, as pkarmode is 
            // from onedrive and change Accessibility

            // initialize OneDrive when it was not yet, and load words 
            // (when they have changed, just off/on and it is already up to date)
            if (!await App.initODandDict(true)) return;  
            

            if (!await UslugaWlaczona())
            {
                bool bIgnore = false;

                if(p.k.GetSettingsBool("pkarMode"))
                {
                    if (await p.k.DialogBoxYNAsync("usluga niby niewlaczona, zignorowac to?"))
                        bIgnore = true;
                }

                if (!bIgnore)
                {
                    await p.k.DialogBoxResAsync("msgAccessibilityFirst");
                    var intent = new Android.Content.Intent
                      (
                        Android.Provider.Settings.ActionAccessibilitySettings
                      );
                    
                    Uno.UI.BaseActivity.Current.StartActivity(intent);

                    uiStartStop.IsOn = false;
                    uiStartStop.IsEnabled = true;
                    
                    return;
                }
            }

            // Bug ?
            //p.k.ProgRingShow(true);

            // if we're here, it's definitely an interception. =)
            await App.AddLogEntry("Starting... @" + DateTime.Now.ToString(), false);

            // determination of necessary variables
            string deviceName = "default";

            if (string.IsNullOrEmpty(App.gsDeviceName) || App.gsDeviceName == "default")
            {
                deviceName = 
                    await p.k.DialogBoxInputDirectAsync
                    (
                        p.k.GetLangString("msgEnterDevName"), 
                        Android.OS.Build.Model, 
                        "resDlgSetName"
                    );
                
                if (deviceName != "")
                {
                    p.k.SetSettingsString("deviceName", deviceName);
                }

                App.gsDeviceName = deviceName;
            }

            await App.AddLogEntry("mamy devicename=" + deviceName, true);

            if (!await App.EnsureOneDriveOpen(true))
            {
                await App.AddLogEntry("FAIL cannot open OneDrive", false);
                
                //TODO
                //App.gOnedrive?.Dispose();
                //App.gOnedrive = null;

                uiStartStop.IsOn = false;
                
                p.k.ProgRingShow(false);
                
                uiStartStop.IsEnabled = true;
                
                return;
            }

            await App.AddLogEntry("startujemy!", false);

            // wszystko gotowe, mozesz wyłapywać
            App.gbPrzechwytuj = true;
            uiStartStop.IsEnabled = true;

            p.k.ProgRingShow(false);

#endif
        }//uiStartStop_Toggled end 


        // uiClearList_Click
        private async void uiClearList_Click(object sender, RoutedEventArgs e)
        {
            // to be: resetlisty (to get rid of older ones)
            //if (p.k.GetPlatform("uwp"))
            //{
            //    // visibility's off, because how? tozsame with delete everything?
            //    await p.k.DialogBoxAsync("I can't do it yet.");
            //}
            //else
            //{// on Android: we do not clean OneDrive, only what is on the screen
            
            App.gToasty.Clear();
            
            uiList.ItemsSource = App.gToasty.ToList();

            //}

        }//uiClearList_Click end 



        // uiItem_DoubleTapped
        private void uiItem_DoubleTapped(object sender, RoutedEventArgs e)
        {
            Grid oGrid;
            oGrid = sender as Grid;
            
            if (oGrid is null)  return;
            
            // Show details 
            p.k.DialogBox(((App.JedenToast)oGrid.DataContext).sMessage);

        }//uiItem_DoubleTapped end


        // MFIdataContext
        private App.JedenToast MFIdataContext(object sender)
        {
            var mfi = (MenuFlyoutItem)sender;
            if (mfi is null)
                return null;
            return (App.JedenToast)mfi.DataContext;
        
        }// MFIdataContext end


        // uiDetails_Click
        private void uiDetails_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);

            if (toast is null) return;
            
            // RnD
            // Show some toast message (?)
            p.k.DialogBox(toast.sMessage);
        }


        // uiRenameSource_Click
        private async void uiRenameSource_Click(object sender, RoutedEventArgs e)
        {
            if (!p.k.GetPlatform("uwp"))
            {
                p.k.DialogBox("how come you pressed it to be on Android? ;)");

                uiStartStop.IsEnabled = true;
                
                return;
            }

            App.JedenToast toast = MFIdataContext(sender);

            if (toast is null) return;
            
            var newName = await p.k.DialogBoxInputDirectAsync
            (
                p.k.GetLangString("msgSourceShould1") + "'" + toast.sSource + "' " + 
                p.k.GetLangString("msgSourceShould2") , toast.sSource
            );
            
            if (string.IsNullOrEmpty(newName)) return;
            
            App.gdSenderRenames.Add(toast.sSource, newName);

            // we write...
            p.k.ProgRingShow(true);

            string dictionaryFileContent = "";
            foreach (var entry in App.gdSenderRenames)
            {
                dictionaryFileContent = dictionaryFileContent + entry.Key + "|" + entry.Value + "\n";
            }

            // "UWP" ONLY (?) TODO: redo it for Android...
#if ((!__ANDROID__) && (!__WASM__))
            await p.od.ReplaceOneDriveFileContent("Apps/Andro2UWP/sender.renames.txt", dictionaryFileContent);
#endif
            RefreshListView(false);

            p.k.ProgRingShow(false);

        }//uiRenameSource_Click end


        // DeleteFiles
        private async System.Threading.Tasks.Task DeleteFiles(List<string> filelist, bool bMsg)
        {
            p.k.ProgRingShow(true, false, 0, filelist.Count);

#if !__ANDROID__
            await p.od.DeleteFilesFromOneDrive("Apps/Andro2UWP", filelist);

            for(int iLp = App.gToasty.Count-1; iLp>=0; iLp--)
            {
                var toast = App.gToasty.ElementAt(iLp);
                
                if(filelist.Contains(toast.sFileName))
                {
                    App.gToasty.RemoveAt(iLp);
                }
            }

            // show new version of filelist
            RefreshListView(bMsg);
#endif 
            p.k.ProgRingShow(false);

        }//DeleteFiles

        // uiDeleteThis_Click
        private void uiDeleteThis_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;

            // from list, oraz from OneDrive

            List<string> filelist = new List<string>();
            
            filelist.Add(toast.sFileName);
            
            // delete files
            DeleteFiles(filelist, false);

        }//uiDeleteThis_Click end


        // uiDeleteOlder_Click
        private async void uiDeleteOlder_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);

            if (toast is null) return;

            // Show worning dialog box
            if 
            (
                !await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveOlder")
                     + "\n" + toast.displayDate + " ?")
            )
            {
                return;
            }

            // from list, oraz from OneDrive
            List<string> filelist = new List<string>();

            foreach(var item in App.gToasty)
            {
                if (item.displayDate.CompareTo(toast.displayDate) < 0)
                {
                    filelist.Add(item.sFileName);
                }
            }
            
            // delete files
            await DeleteFiles(filelist,false);

        }//uiDeleteOlder_Click end


        // uiDeleteThisOlder_Click
        private async void uiDeleteThisOlder_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);

            if (toast is null) return;

            // show dialog box
            if (!await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveThisOlder")))
            {
                return;
            }

            // from list, oraz from OneDrive
            var filelist = new List<string>();
            foreach (var item in App.gToasty)
            {
                if (item.displayDate.CompareTo(toast.displayDate) <= 0)
                    filelist.Add(item.sFileName);
            }
            
            // delete files
            await DeleteFiles(filelist,false);

        }//uiDeleteThisOlder_Click end


        // uiCopy_Click
        private void uiCopy_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);

            // ?
            p.k.ClipPut(toast.ToString());

        }//uiCopy_Click end 


        // uiDeleteSender_Click
        private async void uiDeleteSender_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            
            if (toast is null) return;
            // from list, oraz from OneDrive

            if
            (
                !await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureRemoveSender") +
                "\n" + toast.displaySource + " ?")
            )
            {
                return;
            }

            // from list, oraz from OneDrive
            List<string> filelist = new List<string>();
            foreach (var item in App.gToasty)
            {
                if (item.displaySource == toast.displaySource)
                    filelist.Add(item.sFileName);
            }

            // delete files
            await DeleteFiles(filelist,false);

        }//uiDeleteSender_Click end


        // uiCreateFilter_Click 
        private async void uiCreateFilter_Click(object sender, RoutedEventArgs e)
        {
            App.JedenToast toast = MFIdataContext(sender);
            if (toast is null) return;

            var oStack = new Windows.UI.Xaml.Controls.StackPanel();

            // RnD
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
            {
                return;
            }

            App.glFiltry.Add(new App.JedenFiltr(oPackage.Text, oTitle.Text, oText.Text));
            // save filtry

            //ProgresywnyRing(true);
            p.k.ProgRingShow(true);


            string dictionaryFileContent = "";

            foreach (var entry in App.glFiltry)
            {
                dictionaryFileContent = dictionaryFileContent + entry.sPackageName 
                    + "|" + entry.sTitle + "|" + entry.sText + "\n";
            }

#if !__ANDROID__
            await p.od.ReplaceOneDriveFileContent("Apps/Andro2UWP/toasts.filters.txt", dictionaryFileContent);
#endif

            // uiRefreshList_Click(null, null); // <-- it's not filtering right now anyway, so we're not playing this
            
            //ProgresywnyRing(false);
            p.k.ProgRingShow(false);

        }//uiCreateFilter_Click end


        // uiRefreshList_Click
        private async void uiRefreshList_Click(object sender, RoutedEventArgs e)
        {
            // Refresh button
            //uiRefresh.IsEnabled = false;

            if (p.k.GetPlatform("uwp"))
            {
                if (!await App.LoadNews(true))
                {
                    return;
                }

                //if (!await App.initODandDict())
                //    return;     // first of all-reading the dictionaries anew (so as not to reset the dictionary!)

                //await App.WczytajNowe();
            }

            // Refresh button
            //uiRefresh.IsEnabled = true;

            //if (sender != null)   // there is no call other than from event button, so always sender < > null?
            //{
            //}

            // Refresh "toasts" list
            RefreshListView(true);

        }//uiRefreshList_Click end


        // RefreshListView
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
                uiList.ItemsSource = (from item in App.gToasty orderby item.sFileName descending select item).ToList();
            }
            else
            {
                uiList.ItemsSource = App.gToasty.ToList();
            }

            // and possibly delete the Delete List button
            if (App.gToasty.Count > 0)
            {

                if (p.k.GetPlatform("uwp"))
                {
#if !__ANDROID__
                    // Make ClearList button visible
                    //uiClearList.Visibility = Visibility.Visible;
#endif
                }

            }
            else
            {
                if (bMsg) p.k.DialogBoxRes("msgNoData");

                if (p.k.GetPlatform("uwp"))
                {
#if !__ANDROID__
                    //uiClearList.Visibility = Visibility.Collapsed;
#endif
                }
            }

        }//RefreshListView end


        // [security reason] uiAutoRefresh does not exist outside UWP
#if NETFX_CORE

        // uiAutoRefresh_Click
        private async void uiAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            if(!uiAutoRefresh.IsChecked.HasValue) return;
            if(uiAutoRefresh.IsChecked.Value)
            {
                // RnD
                //if(!p.k.IsFamilyDesktop())
                //{
                //    bez .resw, bo i tak to powinno być zablokowane...
                //    p.k.DialogBox("Sorry, but this would work only on desktop.");    
                //    return;
                //}

                Windows.ApplicationModel.Background.SystemCondition oCondition =
                    new Windows.ApplicationModel.Background.SystemCondition
                    (Windows.ApplicationModel.Background.SystemConditionType.InternetAvailable);

                p.k.RegisterTimerTrigger("Andro2UWP_Timer", 15, false, oCondition); // 15 - 15 minutes
            }
            else
            {
                p.k.UnregisterTriggers("Andro2UWP_Timer");
            }
        }//uiAutoRefresh_Click end
#endif

        // ProgresywnyRing
        //private void ProgresywnyRing(bool sStart)
        //{
        //    if (sStart)
        //    {
        //        double dVal;
        //        dVal = Math.Min(uiGrid.ActualHeight, uiGrid.ActualWidth) / 2;
        //        uiProcesuje.Width = dVal;
        //        uiProcesuje.Height = dVal;
        //
        //        uiProcesuje.Visibility = Visibility.Visible;
        //        uiProcesuje.IsActive = true;
        //    }
        //    else
        //    {
        //        uiProcesuje.IsActive = false;
        //        uiProcesuje.Visibility = Visibility.Collapsed;
        //    }
        //}//ProgresywnyRing end

    }//MainPage class end

}//namespace end


