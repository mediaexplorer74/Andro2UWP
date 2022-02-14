// Settings page
// Both for UWP and Android
// 2022

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
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
    // Settings class
    public sealed partial class Settings : Page
    {
        // Settings
        public Settings()
        {
            this.InitializeComponent();
        }//Settings end


        // uiPage_Loaded
        private void uiPage_Loaded(object sender, RoutedEventArgs e)
        {
            // uiVersion.Text = p.k.GetAppVers();
            p.k.GetAppVers(uiVersion);
            uiCounter.Text = App.giCurrentNumber.ToString();
            uiDeviceName.Text = App.gsDeviceName;

#if __ANDROID__
            if (string.IsNullOrEmpty(App.gsDeviceName) || App.gsDeviceName == "default")
                uiDeviceName.Text = Android.OS.Build.Model;
#endif
            p.k.GetSettingsBool(uiCreateToasts,"createToasts");

            p.k.GetSettingsBool(uiSortListMode,"sortDescending", true);
            
            p.k.GetSettingsBool(uiDebugLog, "debugLog");

            // RnD zone : Toast feature
            //uiCreateToasts.IsEnabled = true;//false;   // as yet unattended this
#if !__ANDROID__
            uiHowMany.Text = p.k.GetSettingsInt("howMany", 10).ToString();
#endif

        }//uiPage_Loaded end


        // uiPermissAccess_Click
        private async void uiPermissAccess_Click(object sender, RoutedEventArgs e)
        {
            if (p.k.GetPlatform("uwp"))
            {
                await p.k.DialogBoxAsync("jakim cudem nacisnales to nie bedac na Androidzie?");
                return;
            }
#if __ANDROID__
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionAccessibilitySettings);
            Uno.UI.BaseActivity.Current.StartActivity(intent);
#endif
        }//uiPermissAccess_Click end


        // uiPermissBattery_Click
        private async void uiPermissBattery_Click(object sender, RoutedEventArgs e)
        {
            if (p.k.GetPlatform("uwp"))
            {
                await p.k.DialogBoxAsync("How you press this magic button not on Android?...");
                return;
            }

#if __ANDROID__
            var intent = new Android.Content.Intent(Android.Provider.Settings.ActionIgnoreBatteryOptimizationSettings);
            Uno.UI.BaseActivity.Current.StartActivity(intent);
#endif
            // asking for inclusion from Android M:
            // https://stackoverflow.com/questions/39256501/check-if-battery-optimization-is-enabled-or-not-for-an-app

        }//uiPermissBattery_Click end


        // TODO : realize some Renames set
        //private async void uiShowRenames_Click(object sender, RoutedEventArgs e)
        //{
        //}//uiShowRenames_Click end


        // TODO : realize some Filters set
        //private async void uiShowFilters_Click(object sender, RoutedEventArgs e)
        //{
        //}//uiShowFilters_Click end


        // uiResetList_Click
        private async void uiResetList_Click(object sender, RoutedEventArgs e)
        { // Visibility="Collapsed"
            if (await p.k.DialogBoxYNAsync("Na pewno wyzerowac aktualną listę?"))
            {

            }
        }//uiResetList_Click end


        // uiSave_Click
        private void uiSave_Click(object sender, RoutedEventArgs e)
        {
            App.giCurrentNumber = int.Parse(uiCounter.Text);

            p.k.SetSettingsInt("currentFileNum", App.giCurrentNumber);
            
            App.gsDeviceName = uiDeviceName.Text;
            p.k.SetSettingsString("deviceName", App.gsDeviceName);

            p.k.SetSettingsBool(uiCreateToasts, "createToasts");
            
            p.k.SetSettingsBool(uiSortListMode, "sortDescending");
            
            p.k.SetSettingsBool(uiDebugLog, "debugLog");

#if !__ANDROID__
            p.k.SetSettingsInt("howMany", int.Parse(uiHowMany.Text));
#endif
          

            Frame.GoBack();

        }//uiSave_Click end

    }//Settings class end

}//namespace end