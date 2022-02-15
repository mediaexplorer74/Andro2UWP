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

#if !__ANDROID__
            uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");
            //uiAutoRefresh.IsEnabled = false;

#endif
            uiRefresh.IsEnabled = false;
            //uiSettings.IsEnabled = false;
            //uiShowLog.IsEnabled = false;

#if DEBUG 
            p.k.SetSettingsInt("debugLogLevel", 5); // TODO: for now, yes, then you need to cut
#endif 
            p.k.SetSettingsString("appFailData", "");   // no leftovers from the previous run

            // init prog ring...
            p.k.ProgRingInit(true, true);
            
            // we start to write...
            p.k.ProgRingShow(true);

            //p.k.GetAppVers("uiVersion");

            App.gsDeviceName = p.k.GetSettingsString("deviceName");
            App.giCurrentNumber = p.k.GetSettingsInt("currentFileNum") + 1;
            
            await App.AddLogEntry("Andro2UWP:MainPage:Loaded called, values:\n deviceName = " + App.gsDeviceName + ",\n currentFileNum =" + App.giCurrentNumber.ToString(), true);
            // on early days, when it was necessary to complete a task
            // to ease the burden in the table...
            // in general, this is from the previous time

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
                Console.WriteLine("[w] uiPage_Loaded - initODandDict problem: " + ex.Message);
                await App.AddLogEntry("[w] uiPage_Loaded - initODandDict problem: " + ex.Message, true);

                //ignore this deal... still continue :)
            }

            
             //bool Rslt_flag = true;


             //if (!await App.LoadNews(true))
             try
             {
                 //Rslt_flag = 
                 await App.LoadNews(true); // await 
             }
             catch (Exception ex)
             {
                Console.WriteLine("[ex] uiPage_Loaded - LoadNews exception: " + ex.Message);
                await App.AddLogEntry("[ex] uiPage_Loaded - LoadNews exception: " + ex.Message, true);
                //Rslt_flag = false;

                // don't write...
                p.k.ProgRingShow(false);

                uiRefresh.IsEnabled = true;
                //uiSettings.IsEnabled = true;
                //uiShowLog.IsEnabled = true;
#if !__ANDROID__
                //uiAutoRefresh.IsEnabled = true;
                uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");

                // Real tosts / Timer deals
                ControlBackgroundTask();
#endif

                return;
            }


             /*if (Rslt_flag == false)
             {
                 // don't write...
                 p.k.ProgRingShow(false);

                 uiRefresh.IsEnabled = true;
                 uiSettings.IsEnabled = true;
                 uiShowLog.IsEnabled = true;
 #if !__ANDROID__ 
                 uiAutoRefresh.IsEnabled = true;
                 uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");

                 // Real tosts / Timer deals
                 ControlBackgroundTask();
 #endif

                 return;
             }
             */
            

            try
            {
                RefreshListView(false);
            }
            catch (Exception ex)
            {
                Console.WriteLine("[w] uiPage_Loaded - RefreshListView problem: " + ex.Message);
                await App.AddLogEntry("[w] uiPage_Loaded - RefreshListView problem: " + ex.Message, true);

                // Ignore it... Still continue :)
            }

            // don't write...
            p.k.ProgRingShow(false);

            uiRefresh.IsEnabled = true;
                
            //uiSettings.IsEnabled = true;
            //uiShowLog.IsEnabled = true;

#if !__ANDROID__  
            //uiAutoRefresh.IsEnabled = true;
            uiAutoRefresh.IsChecked = p.k.IsTriggersRegistered("Andro2UWP_Timer");
            
            // Real tosts / Timer deals
            ControlBackgroundTask();
#endif

        }//uiPage_Loaded end


#if __ANDROID__
        // Check that Service is available
        private async System.Threading.Tasks.Task<bool> IsServiceAvailable()
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

            await App.AddLogEntry("Unfortunately, service disabled", true);

            return false;

        }//IsServiceAvailable end
#endif

        /*
        // TEMP

        // uiAuth_Click
        private void uiAuth_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(AccountSelectionPage));
        }//uiAuth_Click end
        */



        /// !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        public async void uiTestToast_Click(object sender, RoutedEventArgs e)
        {
#if __ANDROID__
            try
            {
                App.AndroToast.GenerateTestToastEvent();
            }
            catch (Exception ex)
            {
                Console.WriteLine("[ex] Exception: " + ex.Message);
                //p.k.DialogBoxRes("[ex] RefreshList.Click Exception: " + ex.Message);

                return;
            }

            ListRefresh();
#endif
        }//uiTestToast_Click end

        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!





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

        // uiShowLog_Click
        private async void uiAbout_Click(object sender, RoutedEventArgs e)
        {
            await p.k.DialogBox(
             "** About **"
             + "\n" +
             "Andro2UWP 1 (https://github.com/mediaexplorer74/Andro2UWP) is RnD project. AS-IS. " +
             "Distributed under the MIT License." + 
             "\n" 
             + "\n" +
             "* Thanks! *" 
             + "\n" +
            "I wanted to put down some thank you's here for folks/projects/websites " +
            "that were invaluable for helping me get this project into a functional state:" + "\n" +
            "* Piotr Karocki (https://github.com/pkar70/) - Great C#/Xamarin/UNO Platform developer" +"\n" +
            "* Andro2UWP (https://github.com/pkar70/Andro2UWP) - Original Andro2UWP" +"\n");

        }//uiAbout_Click end


        // uiStartStop_Toggled
        private async void uiStartStop_Toggled(object sender, RoutedEventArgs e)
        {
            // this button is Android only
#if __ANDROID__
            //uiStartStop.IsEnabled = false;
#endif

            if (p.k.GetPlatform("uwp"))
            {
                p.k.DialogBox("How did you press this not to be on Android? ;)");
                
                //uiStartStop.IsEnabled = true;
                
                return;
            }

#if __ANDROID__
            if (!uiStartStop.IsOn)
            {
                await App.AddLogEntry("Stopping... @" + DateTime.Now.ToString(), false);
                App.gbCatch = false;

                //TODO
                //App.gOnedrive?.Dispose();
                //App.gOnedrive = null;

                //uiStartStop.IsEnabled = true;
                
                return;
            }

            // 2020.06.29: Move OneDrive before Accessibility, as pkarmode is 
            // from onedrive and change Accessibility

            // initialize OneDrive when it was not yet, and load words 
            // (when they have changed, just off/on and it is already up to date)
            if (!await App.initODandDict(true)) return;  
            

            if (!await IsServiceAvailable())
            {
                bool bIgnore = false;

                if(p.k.GetSettingsBool("pkarMode"))
                {
                    if (await p.k.DialogBoxYNAsync("A service seemingly unrestricted, ignore it?"))
                    {
                        bIgnore = true;
                    }
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
                    //uiStartStop.IsEnabled = true;
                    
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

            await App.AddLogEntry("We have devicename=" + deviceName, true);

            if (!await App.EnsureOneDriveOpen(true))
            {
                await App.AddLogEntry("[e] FAIL cannot open OneDrive", false);
                
                //TODO
                //App.gOnedrive?.Dispose();
                //App.gOnedrive = null;

                // RnD
                //uiStartStop.IsOn = false; // ?

                //uiStartStop.IsEnabled = true;

                p.k.ProgRingShow(false);
                
               
                
                return;
            }

            await App.AddLogEntry("[i] OneDrive handling engine started!", false);

            // wszystko gotowe, mozesz wyłapywać

            App.gbCatch = true;
            
            //uiStartStop.IsEnabled = true;

            p.k.ProgRingShow(false);

#endif
        }//uiStartStop_Toggled end 


        
        // uiClearList_Click
        
        private async void uiClearList_Click(object sender, RoutedEventArgs e)
        {
            // old scheme 
            // TODO: reset all OneDrive toast list (to get rid of older ones)
            //if (p.k.GetPlatform("uwp"))
            //{
            //    // visibility's off, because how? tozsame with delete everything?
            //    await p.k.DialogBoxAsync("I can't do it yet.");
            //}
            //else
            //{
            // on Android: we do not clean OneDrive, only what is on the screen
            //            
            //App.sToasts.Clear();
            //uiList.ItemsSource = App.sToasts.ToList();

            // RnD
#if __ANDROID__
            
            // Show worning dialog box
            if
            (
                // Plan A: mono-language (EN) case
                //!await p.k.DialogBoxYNAsync("Are you sure to delete all toast files (from OneDrive app folder too)?")
                // Plan B: multi-language case
                !await p.k.DialogBoxYNAsync(p.k.GetLangString("msgSureDeleteToastList") + "?")                
            )
            {
                return;
            }

            // from list, oraz from OneDrive
            List<string> filelist = new List<string>();

            foreach (var item in App.sToasts)
            {
                // add toast item to filelist 
                filelist.Add(item.sFileName);                
            }

            // delete files
            await DeleteFiles(filelist, false);

            ListRefresh();
            
#else
            p.k.DialogBox("how come you pressed this button (not from Android)? ;)");
#endif

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

                //uiStartStop.IsEnabled = true;
                
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

//#if !__ANDROID__
            await p.od.DeleteFilesFromOneDrive("Apps/Andro2UWP", filelist);

            for(int iLp = App.sToasts.Count-1; iLp>=0; iLp--)
            {
                var toast = App.sToasts.ElementAt(iLp);
                
                if(filelist.Contains(toast.sFileName))
                {
                    App.sToasts.RemoveAt(iLp);
                }
            }

            // show new ("refreshed") version of filelist
            RefreshListView(false);//(bMsg);
//#endif
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

            foreach(var item in App.sToasts)
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
            foreach (var item in App.sToasts)
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
            foreach (var item in App.sToasts)
            {
                if (item.displaySource == toast.displaySource)
                {
                    filelist.Add(item.sFileName);
                }
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

            //#if __ANDROID__ //!NETFX_CORE
            //            oDlg.Dispose();
            //#endif

            if (oCmd == Windows.UI.Xaml.Controls.ContentDialogResult.Primary)
            {
                return;
            }

            App.glFilters.Add(new App.JedenFiltr(oPackage.Text, oTitle.Text, oText.Text));
            // (...need to save filter)

            p.k.ProgRingShow(true);


            string dictionaryFileContent = "";

            foreach (var entry in App.glFilters)
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

            // --------------TEMP (for Testing ONLY)---------------
            // --- RnD zone - start -------------------------------
            /*
            bool IsCreateToasts = p.k.GetSettingsBool("createToasts");
            if (IsCreateToasts == true)
            {
                // Let make Real Toast :) 
                string sAdd = DateTime.Now.ToString("HH:mm:ss") + " " + "List refreshed!" + "  " + "Test Msg :)" + "  ";

                p.k.MakeToast(sAdd);
            }
            */
            // --- RnD zone - stop --------------------------------

            ListRefresh();

        }//uiRefreshList_Click end


        // ListRefresh
        public async void ListRefresh()
        {
            ProgresywnyRingV2(true);

            // we write...
            //p.k.ProgRingShow(true);
            ProgresywnyRingV2(true);

            // Refresh button - off

            uiRefresh.IsEnabled = false;

            try
            {

                if (!await App.LoadNews(true))
                {
                    // don't write...
                    //p.k.ProgRingShow(false);
                    ProgresywnyRingV2(false);

                    return;
                }
            }
            catch (Exception ex)
            {
                //Console.WriteLine("[ex] Exception: " + ex.Message);
                await p.k.DialogBoxResAsync("Refresh List exception: " + ex.Message);

                // don't write...
                //p.k.ProgRingShow(false);
                ProgresywnyRingV2(false);

                // enable refresh button
                uiRefresh.IsEnabled = false;

                return;
            }

            //RnD
            //first of all-reading the dictionaries anew (so as not to reset the dictionary!)
            //if (!await App.initODandDict()) return;     
            //await App.LoadNew();             


            // Refresh "toasts" list
            RefreshListView(false);//(true);


            // don't write...
            //p.k.ProgRingShow(false);
            ProgresywnyRingV2(false);

            // Refresh button - on
            uiRefresh.IsEnabled = true;


        }//ListRefresh end


        // RefreshListView
        public void RefreshListView(bool bMsg)
        {
            
            // najpierw uzupelnij liste (elementy "display")
            // nie 'foriczem', bo by było Exception ze zmiana w trakcie foreach
            for (int iLoop = 0; iLoop < App.sToasts.Count; iLoop++)
            {
                var oItem = App.sToasts.ElementAt(iLoop);
                
                //RnD
                // comment-out, because not used, a waste of time (especially Android)
                //if (string.IsNullOrEmpty(oItem.displayDevice))
                //{
                //    if (App.gsDeviceName != oItem.sDevice)
                //        oItem.displayDevice = App.gsDeviceName;
                //}

                //oItem.displayDate = oItem.dDate.ToString("dd-MM-yyyy HH:mm");
                // displayDate - takes when loading from a file name

                // replacing source with a word using
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

            // show it at "reverse order"
            if (p.k.GetSettingsBool("sortDescending", true))
            {
                uiList.ItemsSource = 
                    (from item in App.sToasts orderby item.sFileName descending select item).ToList();
            }
            else
            {
                uiList.ItemsSource = //App.sToasts.ToList();
                    (from item in App.sToasts orderby item.sFileName ascending select item).ToList();
            }

 /*
            // and possibly delete the Delete List button
            if (App.sToasts.Count > 0)
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
                if (bMsg)
                {
                    await p.k.DialogBoxResAsync("msgNoData");
                }

                if (p.k.GetPlatform("uwp"))
                {
#if !__ANDROID__
                    //uiClearList.Visibility = Visibility.Collapsed;
#endif
                }
 
            }
 */

        }//RefreshListView end


        // [security reason] uiAutoRefresh does not exist outside UWP
#if !__ANDROID__ //#if NETFX_CORE

        // uiAutoRefresh_Click
        private async void uiAutoRefresh_Click(object sender, RoutedEventArgs e)
        {
            ControlBackgroundTask();
        }//uiAutoRefresh_Click end


        // ControlBackgroundTask
        private void ControlBackgroundTask()
        {
            if (!uiAutoRefresh.IsChecked.HasValue)
            {
                return;
            }

            if (uiAutoRefresh.IsChecked.Value)
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

        }//ControlBackgroundTask end
#endif

        // ProgresywnyRing v2
        private void ProgresywnyRingV2(bool sActive)
        {
            if (sActive)
            {
                double dVal;
                dVal = Math.Min(uiGrid.ActualHeight, uiGrid.ActualWidth) / 2;
                uiProcesuje.Width = dVal;
                uiProcesuje.Height = dVal;
        
                uiProcesuje.Visibility = Visibility.Visible;
                uiProcesuje.IsActive = true;
            }
            else
            {
                uiProcesuje.IsActive = false;
                uiProcesuje.Visibility = Visibility.Collapsed;
            }
        }//ProgresywnyRingV2 end

    }//MainPage class end

}//namespace end


