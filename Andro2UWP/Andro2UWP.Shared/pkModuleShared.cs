// pkModuleShared
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
using System.Threading.Tasks;
using System.Diagnostics;
using Andro2UWP;


// ! TODO-s: do Strings:
// "errAnyError", resDlgYes, resDlgNo

namespace p
{
    // !!
    public sealed partial class k
    {

        // -- DEBUG ------------------------------------------------

        public static void DebugOut(string sTxt)
        {
            //TODO: redirect (or make dubbing) debug output

            Debug.WriteLine(sTxt);

            //MainPage.SuperWriteLine(sTxt);


            //App.myDebugList.Add(new App.MyDebugBlock(sTxt));


        }


        public static void DebugOut(int v, string sTxt)
        {
            //TODO: redirect (or make dubbing) debug output

            Debug.WriteLine(v.ToString() + " " + sTxt);

            //MainPage.SuperWriteLine(sTxt);
            //what is 2 ? 
            //if (v != 2)
            {
                //App.myDebugList.Add(new App.MyDebugBlock(v.ToString() + " " + sTxt));

            }
        }

        // --------------------------------------------------------------------------

        // ------------------------------------------------------------------------------------------

        // -- OS-DEPENDENT -------------------------------------------------------------------------
        #region "osdepended"

        // Get Win./And. ver. build number
        public static int WinVer()
        {
#if NETFX_CORE
            // Unknown = 0,
            // Threshold1 = 1507,   // 10240
            // Threshold2 = 1511,   // 10586
            // Anniversary = 1607,  // 14393 Redstone 1
            // Creators = 1703,     // 15063 Redstone 2
            // FallCreators = 1709 // 16299 Redstone 3
            // April = 1803		// 17134
            // October = 1809		// 17763
            // ? = 190?		// 18???

            // April  1803, 17134, RS5

            ulong u = ulong.Parse(Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamilyVersion);
            u = (u & 0xFFFF0000L) >> 16;
            return (int)u;
#elif __ANDROID__
            return (int)Android.OS.Build.VERSION.SdkInt;
#endif
            return 0; // TODO
        }



        public static string GetPlatform()
        {
#if NETFX_CORE
            return "uwp";
#elif __ANDROID__
            return "android";
#elif __IOS__
        return "ios";
#elif __WASM__
            return "wasm";
#else
        return "other";
#endif
        }




        public static bool GetPlatform(string sPlatform)
        {
            if (string.IsNullOrEmpty(sPlatform)) return false;
            if (GetPlatform().ToLower() == sPlatform.ToLower()) return true;
            return false;
        }

        // ?

        public static bool IsFamilyDesktop()
        {
            return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Desktop");
        }

        public static bool IsFamilyMobile()
        {

            // Brewiarz: wymuszanie zmiany dark/jasne
            // GrajCyganie: zmiana wielkosci okna
            // pociagi: ile rzadkow ma pokazac (rozmiar ekranu)
            // kamerki: full screen wlacz/wylacz tylko dla niego
            // sympatia...

            // TODO: WASM w zale?no?ci od rozmiaru ekranu?
            return (Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile");
            //return Windows.System.Profile.AnalyticsInfo.DeviceForm.ToLower().Contains("mobile");
        }


        public static string GetAppVers()
        {
            return Windows.ApplicationModel.Package.Current.Id.Version.Major + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Minor + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Build;

        }

        // Get app. version ... ?
        public static string GetAppVers(TextBlock Ver)
        {
            return Windows.ApplicationModel.Package.Current.Id.Version.Major + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Minor + "." +
                Windows.ApplicationModel.Package.Current.Id.Version.Build;
        }
        // ------------------------------------------------------------------
        #endregion

        // -- Timer Triggers ---------------------------------------------------------
        #region "triggers"

        public static bool IsTriggersRegistered(string sNamePrefix)
        {
            sNamePrefix = sNamePrefix.Replace(" ", "").Replace("'", "");

            try
            {
                foreach (var oTask in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
                {
                    if (oTask.Value.Name.ToLower().Contains(sNamePrefix.ToLower())) return true;
                }
            }
            catch
            {
                // np. gdy nie ma permissions, to mo?e by? FAIL
            }

            return false;
        }

        /// <summary>
        /// jakikolwiek z prefixem Package.Current.DisplayName
        /// </summary>
        public static bool IsTriggersRegistered()
        {
            return IsTriggersRegistered(Windows.ApplicationModel.Package.Current.DisplayName);
        }

        /// <summary>
        /// wszystkie z prefixem Package.Current.DisplayName
        /// </summary>
        public static void UnregisterTriggers()
        {
            UnregisterTriggers(Windows.ApplicationModel.Package.Current.DisplayName);
        }

        public static void UnregisterTriggers(string sNamePrefix)
        {
            sNamePrefix = sNamePrefix.Replace(" ", "").Replace("'", "");

            try
            {
                foreach (var oTask in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
                {
                    if (string.IsNullOrEmpty(sNamePrefix) || oTask.Value.Name.ToLower().Contains(sNamePrefix.ToLower()))
                    {
                        oTask.Value.Unregister(true);
                    }
                }
            }
            catch
            {
                // np. gdy nie ma permissions, to mo?e by? FAIL
            }
        }

        public static async System.Threading.Tasks.Task<bool> CanRegisterTriggersAsync()
        {
            Windows.ApplicationModel.Background.BackgroundAccessStatus oBAS;
            oBAS = await Windows.ApplicationModel.Background.BackgroundExecutionManager.RequestAccessAsync();

            if (oBAS == Windows.ApplicationModel.Background.BackgroundAccessStatus.AlwaysAllowed) return true;
            if (oBAS == Windows.ApplicationModel.Background.BackgroundAccessStatus.AllowedSubjectToSystemPolicy) return true;

            return false;
        }

        public static Windows.ApplicationModel.Background.BackgroundTaskRegistration RegisterTimerTrigger(string sName, uint iMinutes, bool bOneShot = false, Windows.ApplicationModel.Background.SystemCondition oCondition = null)
        {
            try
            {
                var builder = new Windows.ApplicationModel.Background.BackgroundTaskBuilder();

                builder.SetTrigger(new Windows.ApplicationModel.Background.TimeTrigger(iMinutes, bOneShot));
                builder.Name = sName;
                if (oCondition is object) builder.AddCondition(oCondition);
                var oRet = builder.Register();
                return oRet;
            }
            catch
            {
                // np. gdy nie ma permissions, to mo?e by? FAIL
            }

            return null;
        }

        private static string GetTriggerNamePrefix()
        {
            string sName = Windows.ApplicationModel.Package.Current.DisplayName;
            sName = sName.Replace(" ", "").Replace("'", "");
            return sName;
        }

        private static string GetTriggerPolnocnyName()
        {
            return GetTriggerNamePrefix() + "_polnocny";
        }

        public static Windows.ApplicationModel.Background.BackgroundTaskRegistration RegisterUserPresentTrigger(string sName = "", bool bOneShot = false)
        {
            try
            {
                Windows.ApplicationModel.Background.BackgroundTaskBuilder builder = new Windows.ApplicationModel.Background.BackgroundTaskBuilder();
                Windows.ApplicationModel.Background.BackgroundTaskRegistration oRet;

                Windows.ApplicationModel.Background.SystemTrigger oTrigger;
                oTrigger = new Windows.ApplicationModel.Background.SystemTrigger(Windows.ApplicationModel.Background.SystemTriggerType.UserPresent, bOneShot);

                builder.SetTrigger(oTrigger);
                builder.Name = sName;

                if (String.IsNullOrEmpty(sName)) builder.Name = GetTriggerNamePrefix() + "_userpresent";


                oRet = builder.Register();

                return oRet;
            }
            catch
            {
                // brak mo?liwo?ci rejestracji (na przyk?ad)
                return null;
            }
        }

        /// <summary>
        /// Tak naprawd? powtarzalny - w OnBackgroundActivated wywo?aj IsThisTriggerPolnocny
        /// </summary>
        public static async System.Threading.Tasks.Task DodajTriggerPolnocny()
        {
            if (!await p.k.CanRegisterTriggersAsync()) return;

            DateTime oDateNew = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 40, 0);
            if (DateTime.Now.Hour > 21)
                oDateNew = oDateNew.AddDays(1);

            uint iMin = (uint)(oDateNew - DateTime.Now).TotalMinutes;

            string sName = GetTriggerPolnocnyName();
            p.k.RegisterTimerTrigger(sName, iMin, false);
        }

        /// <summary>
        /// para z DodajTriggerPolnocny, do wywo?ywania w OnBackgroundActivated
        /// </summary>
        public static bool IsThisTriggerPolnocny(Windows.ApplicationModel.Activation.BackgroundActivatedEventArgs args)
        {
            string sName = GetTriggerPolnocnyName();
            if (args.TaskInstance.Task.Name != sName) return false;

            // no dobrze, jest to trigger pó?nocny, ale czy o pó?nocy...
            string sCurrDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            SetSettingsString("lastPolnocnyTry", sCurrDate);

            bool bRet = false;
            DateTime oDateNew = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 23, 40, 0);

            if (DateTime.Now.Hour == 23 && DateTime.Now.Minute > 20)    // 40 minut, ale system dodaje ±15 minut!
            {
                // tak, to jest pó?nocny o pó?nocy
                bRet = true;
                oDateNew = oDateNew.AddDays(1);
                SetSettingsString("lastPolnocnyOk", sCurrDate);
            }
            else
            {
                // pó?nocny, ale nie o pó?nocy
                bRet = false;
            }
            int iMin = 0;
            iMin = (int)(oDateNew - DateTime.Now).TotalMinutes;

            // Usuwamy istniej?cy, robimy nowy
            UnregisterTriggers(sName);
            RegisterTimerTrigger(sName, (uint)iMin, false);

            return bRet;
        }

#if NETFX_CORE
        public static Windows.ApplicationModel.Background.BackgroundTaskRegistration RegisterToastTrigger(string sName)
        {
            try
            {
                var builder = new Windows.ApplicationModel.Background.BackgroundTaskBuilder();

                builder.SetTrigger(new Windows.ApplicationModel.Background.ToastNotificationActionTrigger());
                builder.Name = sName;
                var oRet = builder.Register();
                return oRet;
            }
            catch
            {
                // np. gdy nie ma permissions, to mo?e by? FAIL
            }

            return null;
        }
#endif

        // ServicingCompleted unimplemted w Uno
#if false
        public static Background.BackgroundTaskRegistration RegisterServicingCompletedTrigger(string sName)
        {
            try
            {
                Background.BackgroundTaskBuilder builder = new Background.BackgroundTaskBuilder();
                Windows.ApplicationModel.Background.BackgroundTaskRegistration oRet;

                builder.SetTrigger(new Background.SystemTrigger(Background.SystemTriggerType.ServicingComplete, true));
                builder.Name = sName;
                oRet = builder.Register();
                return oRet;
            }
            catch (Exception ex)
            {
            }

            return null/* TODO Change to default(_) if this is not a reference type */;
        }
#endif

        #endregion



        #region "RemoteSystem"
        public static bool IsTriggerAppService(Windows.ApplicationModel.Activation.BackgroundActivatedEventArgs args)
        {
#if NETFX_CORE
            Windows.ApplicationModel.AppService.AppServiceTriggerDetails oDetails =
                args.TaskInstance.TriggerDetails as Windows.ApplicationModel.AppService.AppServiceTriggerDetails;
            if (oDetails is null) return false;
            return true;
#else
            return false;
#endif
        }

        public static string AppServiceStdCmd(string sCommand, string sLocalCmds)
        {
            string sTmp = "";

            switch (sCommand.ToLower())
            {
                case "ping":
                    return "pong";

                case "ver":
                    return GetAppVers(); // p.k.GetAppVers
                case "localdir":
                    return Windows.Storage.ApplicationData.Current.LocalFolder.Path;
                case "appdir":
                    return Windows.ApplicationModel.Package.Current.InstalledLocation.Path;
                case "installeddate":
                    return Windows.ApplicationModel.Package.Current.InstalledDate.ToString("yyyy.MM.dd HH:mm:ss");

                case "help":
                    return "App specific commands:\n" + sLocalCmds;
                case "debug vars":
                    return DumpSettings();
                case "debug triggers":
                    return DumpTriggers();
                case "debug toasts":
                    return DumpToasts();

                case "debug memsize":
                    return GetAppMemData();
                case "debug rungc":
                    sTmp = "Memory usage before Global Collector call: " + GetAppMemData() + "\n";
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                    sTmp = sTmp + "After: " + GetAppMemData();
                    return sTmp;

                case "debug crashmsg":
                    sTmp = GetSettingsString("appFailData", "");
                    if (sTmp == "") sTmp = "No saved crash info";
                    return sTmp;
                case "debug crashmsg clear":
                    sTmp = GetSettingsString("appFailData", "");
                    if (sTmp == "") sTmp = "No saved crash info";
                    SetSettingsString("appFailData", "");
                    return sTmp;
                case "lib unregistertriggers":
                    sTmp = DumpTriggers();
                    UnregisterTriggers(""); // ca?kiem wszystkie
                    return sTmp;
                case "lib isfamilymobile":
                    return IsFamilyMobile().ToString();
                case "lib isfamilydesktop":
                    return IsFamilyDesktop().ToString();
                case "lib netisipavailable":
                    return NetIsIPavailable(false).ToString();
                case "lib netiscellinet":
                    return NetIsCellInet().ToString();
                case "lib gethostname":
                    return GetHostName();
                case "lib isthismoje":
                    return IsThisMoje().ToString();
                case "lib istriggersregistered":
                    return IsTriggersRegistered().ToString();

                case "lib pkarmode 1":
                    SetSettingsBool("pkarMode", true);
                    return "DONE";
                case "lib pkarmode 0":
                    SetSettingsBool("pkarMode", false);
                    return "DONE";
                case "lib pkarmode":
                    return GetSettingsBool("pkarMode").ToString();
            }

            return "";  // oznacza: to nie jest standardowa komenda
        }

        private static string GetAppMemData()
        {
#if NETFX_CORE
            return Windows.System.MemoryManager.AppMemoryUsage.ToString() + "/" + Windows.System.MemoryManager.AppMemoryUsageLimit.ToString();
#else
            return "GetAppMemData is not implemented on non-UWP";
#endif
        }

        private static string DumpSettings()
        {
            string sRoam = "";
            try
            {
                foreach (var oVal in Windows.Storage.ApplicationData.Current.RoamingSettings.Values)
                {
                    sRoam += oVal.Key + "\t" + oVal.Value.ToString() + "\n";
                }
            }
            catch { };

            string sLocal = "";
            try
            {
                foreach (var oVal in Windows.Storage.ApplicationData.Current.LocalSettings.Values)
                {
                    sLocal += oVal.Key + "\t" + oVal.Value.ToString() + "\n";
                }
            }
            catch { };

            string sRet = "Dumping Settings\n";
            if (sRoam != "")
                sRet += "\nRoaming:\n" + sRoam;
            else
                sRet += "(no roaming settings)\n";

            if (sLocal != "")
                sRet += "\nLocal:\n" + sLocal;
            else
                sRet += "(no local settings)\n";


            return sRet;
        }

        private static string DumpTriggers()
        {
            string sRet = "";

            try
            {
                foreach (var oTask in Windows.ApplicationModel.Background.BackgroundTaskRegistration.AllTasks)
                {
                    sRet += oTask.Value.Name + "\n";    //GetType niestety nie daje rzeczywistego typu
                }
            }
            catch { };

            if (sRet == "") return "No registered triggers\n";

            return "Dumping Triggers\n\n" + sRet;

        }
        private static string DumpToasts()
        {
#if NETFX_CORE
            string sResult = "";

            foreach (Windows.UI.Notifications.ScheduledToastNotification oToast
                in Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().GetScheduledToastNotifications())
            {
                sResult = sResult + oToast.DeliveryTime.ToString("yyyy-MM-dd HH:mm:ss") + "\n";
            }

            if (sResult == "")
                sResult = "(no toasts scheduled)";
            else
                sResult = "Toasts scheduled for dates: \n" + sResult;
            return sResult;
#else
            return "DumpToasts on non-UWP is not implemented";
#endif
        }

        private static async System.Threading.Tasks.Task<string> DumpSDKvers()
        {
            var file = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFileAsync("AppxManifest.XML");
            using (Stream stream = await file.OpenStreamForReadAsync())
            {
                //var doc = XDocument.Load(stream);
                //<Package
                // <Dependencies>
                //  <TargetDeviceFamily Name="Windows.Universal" MinVersion="10.0.14393.0" MaxVersionTested="10.0.19041.0" />

            }
            return "bla";
        }

        #endregion




        #region "Datalog"
#if NETFX_CORE
        private async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetSDcardFolderAsync()
        {
            // uwaga: musi by? w Manifest RemoteStorage oraz fileext!

            Windows.Storage.StorageFolder oRootDir;

            try
            {
                oRootDir = Windows.Storage.KnownFolders.RemovableDevices;
            }
            catch
            {
                return null;
            }// brak uprawnie?, mo?e by? tak?e THROW

            try
            {
                IReadOnlyList<Windows.Storage.StorageFolder> oCards = await oRootDir.GetFoldersAsync();
                return oCards.FirstOrDefault();
            }
            catch
            {
            }

            return null;
        }

#endif

        private async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetLogFolderRootDatalogsOnSDcardAsync()
        {
#if !NETFX_CORE
            return null;
#else
            Windows.Storage.StorageFolder oSdCard = null;
            Windows.Storage.StorageFolder oFold;

            oSdCard = await GetSDcardFolderAsync();

            if (oSdCard is null) return null;

            oFold = await oSdCard.CreateFolderAsync("DataLogs", Windows.Storage.CreationCollisionOption.OpenIfExists);
            if (oFold == null) return null;

            string sAppName = Windows.ApplicationModel.Package.Current.DisplayName;
            sAppName = sAppName.Replace(" ", "").Replace("'", "");
            oFold = await oFold.CreateFolderAsync(sAppName, Windows.Storage.CreationCollisionOption.OpenIfExists);

            return oFold;
#endif
        }

        private async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetLogFolderRootDatalogsInAppAsync()
        {
            Windows.Storage.StorageFolder oSdCard = null;
            Windows.Storage.StorageFolder oFold;

            oSdCard = Windows.Storage.ApplicationData.Current.LocalFolder;
            oFold = await oSdCard.CreateFolderAsync("DataLogs", Windows.Storage.CreationCollisionOption.OpenIfExists);
            return oFold;
        }

        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetLogFolderRootAsync(bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = null;

            if (IsFamilyMobile())
            { // poza UWP zwróci null
                oFold = await GetLogFolderRootDatalogsOnSDcardAsync();
            }
            if (oFold is object) return oFold;

            // albo w UWP nie ma karty, albo poza UWP
            if (!bUseOwnFolderIfNotSD) return null;
            oFold = await GetLogFolderRootDatalogsInAppAsync();

            return oFold;
        }


        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetLogFolderYearAsync(bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = await GetLogFolderRootAsync(bUseOwnFolderIfNotSD);
            if (oFold == null)
                return null;
            oFold = await oFold.CreateFolderAsync(DateTime.Now.ToString("yyyy"), Windows.Storage.CreationCollisionOption.OpenIfExists);
            return oFold;
        }

        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFolder> GetLogFolderMonthAsync(bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = await GetLogFolderYearAsync(bUseOwnFolderIfNotSD);
            if (oFold == null)
                return null;

            oFold = await oFold.CreateFolderAsync(DateTime.Now.ToString("MM"), Windows.Storage.CreationCollisionOption.OpenIfExists);
            return oFold;
        }

        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFile> GetLogFileMonthlyAsync(string sBaseName, string sExtension, bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = await GetLogFolderYearAsync(bUseOwnFolderIfNotSD);
            if (oFold is null) return null;

            if (string.IsNullOrEmpty(sExtension)) sExtension = ".txt";
            if (!sExtension.StartsWith(".")) sExtension = "." + sExtension;

            string sFile;
            if (string.IsNullOrEmpty(sBaseName))
                sFile = DateTime.Now.ToString("yyyy.MM") + sExtension;
            else
                sFile = sBaseName + " " + DateTime.Now.ToString("yyyy.MM") + sExtension;

            return await oFold.CreateFileAsync(sFile, Windows.Storage.CreationCollisionOption.OpenIfExists);
        }


        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFile> GetLogFileDailyAsync(string sBaseName, string sExtension, bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = await GetLogFolderMonthAsync(bUseOwnFolderIfNotSD);
            if (oFold == null)
                return null;

            if (!sExtension.StartsWith("."))
                sExtension = "." + sExtension;

            string sFile = sBaseName + " " + DateTime.Now.ToString("yyyy.MM.dd") + sExtension;
            return await oFold.CreateFileAsync(sFile, Windows.Storage.CreationCollisionOption.OpenIfExists);
        }

        public async static System.Threading.Tasks.Task<Windows.Storage.StorageFile> GetLogFileDailyAsync(string sFileName, bool bUseOwnFolderIfNotSD = true)
        {
            Windows.Storage.StorageFolder oFold = await GetLogFolderMonthAsync(bUseOwnFolderIfNotSD);
            if (oFold == null)
                return null;

            return await oFold.CreateFileAsync(sFileName, Windows.Storage.CreationCollisionOption.OpenIfExists);
        }

        #endregion

        public static void DebugBTprintArray(byte[] aArr, int iSpaces)
        {
            string sPrefix = "";
            for (int i = 1; i <= iSpaces; i++)
                sPrefix += " ";


            if (aArr.Length > 6)
                System.Diagnostics.Debug.WriteLine(sPrefix + "length: " + aArr.Length);

            string sBytes = "";
            string sAscii = sBytes;

            for (int i = 0; i <= Math.Min(aArr.Length - 1, 32); i++) // bylo oVal
            {
                byte cBajt = aArr.ElementAt(i);

                // hex: tylko 16 bajtow
                if (i < 16)
                {
                    try
                    {
                        sBytes = sBytes + " 0x" + string.Format("{0:X}", cBajt);
                    }
                    catch
                    {
                        sBytes = sBytes + " ??";
                    }
                }

                // ascii: do 32 bajtow
                if (cBajt > 31 & cBajt < 160)
                    sAscii = sAscii + Convert.ToChar(cBajt);
                else
                    sAscii = sAscii + "?";
            }

            if (aArr.Length - 1 > 16)
                sBytes = sBytes + " ...";
            if (aArr.Length - 1 > 32)
                sAscii = sAscii + " ...";

            System.Diagnostics.Debug.WriteLine(sPrefix + "binary: " + sBytes);
            System.Diagnostics.Debug.WriteLine(sPrefix + "ascii:  " + sAscii);
        }

        // du?a seria funkcji Bluetooth - nieprzenoszona, bo i tak nie ma Bluetooth w Uno

        public static ulong MacStringToULong(string sStr)
        {
            if (string.IsNullOrEmpty(sStr)) throw new ArgumentNullException("sStr", "MacStringToULong powinno miec parametr");
            if (!sStr.Contains(":")) throw new ArgumentException("MacStringToULong - nie ma dwukropków w sStr");

            sStr = sStr.Replace(":", "");
            ulong uLng = ulong.Parse(sStr, System.Globalization.NumberStyles.HexNumber);

            return uLng;
        }


        public static Windows.Devices.Geolocation.BasicGeoposition GetDomekGeopos(UInt16 iDecimalDigits = 0)
        {
            Windows.Devices.Geolocation.BasicGeoposition oTestPoint = new Windows.Devices.Geolocation.BasicGeoposition();
            //' 50.01985 
            //' 19.97872

            switch (iDecimalDigits)
            {
                case 1:
                    oTestPoint.Latitude = 50.0;
                    oTestPoint.Longitude = 19.9;
                    break;
                case 2:
                    oTestPoint.Latitude = 50.01;
                    oTestPoint.Longitude = 19.97;
                    break;
                case 3:
                    oTestPoint.Latitude = 50.019;
                    oTestPoint.Longitude = 19.978;
                    break;
                case 4:
                    oTestPoint.Latitude = 50.0198;
                    oTestPoint.Longitude = 19.9787;
                    break;
                case 5:
                    oTestPoint.Latitude = 50.01985;
                    oTestPoint.Longitude = 19.97872;
                    break;
                default:
                    oTestPoint.Latitude = 50.0;
                    oTestPoint.Longitude = 20.0;
                    break;
            }

            return oTestPoint;
        }


        public static async System.Threading.Tasks.Task<bool> IsFullVersion()
        {
#if DEBUG
            return true;
#else
#if !NETFX_CORE
            return false;
#else
            // if(IsThisMoje()) return true;

            if (!Windows.Foundation.Metadata.ApiInformation.IsTypePresent("Windows.Services.Store.StoreContext"))
                return false;

            var oLicencja = await Windows.Services.Store.StoreContext.GetDefault().GetAppLicenseAsync();
            if (!oLicencja.IsActive) return false; // bez licencji? jak?e? to mo?liwe?
            if (oLicencja.IsTrial) return false;
            return true;
#endif
#endif
        }// func end

        // --------------------------------------------------------------------------



        // --- ProgRing -------------------------------------------------------------
        #region "ProgressBar/Ring"

        //  doda?em 25 X 2020

        private static Windows.UI.Xaml.Controls.ProgressRing _mProgRing = null;
        private static Windows.UI.Xaml.Controls.ProgressBar _mProgBar = null;
        private static int _mProgRingShowCnt = 0;


        // Prog Ring Init
        public static void ProgRingInit(bool bRing, bool bBar)
        {
            // 2020.11.24: dodaj? force-off do ProgRing na Init
            _mProgRingShowCnt = 0;   // skoro inicjalizuje, to znaczy ?e na pewno trzeba wy??czy?

            Windows.UI.Xaml.Controls.Frame oFrame = Windows.UI.Xaml.Window.Current.Content as Windows.UI.Xaml.Controls.Frame;
            Windows.UI.Xaml.Controls.Page oPage = oFrame?.Content as Windows.UI.Xaml.Controls.Page;
            Windows.UI.Xaml.Controls.Grid oGrid = oPage?.Content as Windows.UI.Xaml.Controls.Grid;

            if (oGrid is null)
            {
                // skoro to nie Grid, to nie ma jak umiescic koniecznych elementow
                DebugOut("ProgRingInit wymaga Grid jako podstawy Page");
                throw new ArgumentException("ProgRingInit wymaga Grid jako podstawy Page");
            }

            // *TODO* sprawdz czy istnieje juz taki Control?

            int iCols = 0;

            if (oGrid.ColumnDefinitions is object)
                iCols = oGrid.ColumnDefinitions.Count; // moze byc 0

            int iRows = 0;

            if (oGrid.RowDefinitions is object)
                iRows = oGrid.RowDefinitions.Count; // moze byc 0

            if (bRing)
            {
                _mProgRing = new Windows.UI.Xaml.Controls.ProgressRing();
                _mProgRing.Name = "uiPkAutoProgRing";
                _mProgRing.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Center;
                _mProgRing.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Center;
                _mProgRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                Windows.UI.Xaml.Controls.Canvas.SetZIndex(_mProgRing, 10000);

                if (iRows > 1)
                {
                    Windows.UI.Xaml.Controls.Grid.SetRow(_mProgRing, 0);
                    Windows.UI.Xaml.Controls.Grid.SetRowSpan(_mProgRing, iRows);
                }

                if (iCols > 1)
                {
                    Windows.UI.Xaml.Controls.Grid.SetColumn(_mProgRing, 0);
                    Windows.UI.Xaml.Controls.Grid.SetColumnSpan(_mProgRing, iCols);
                }

                oGrid.Children.Add(_mProgRing);
            }

            if (bBar)
            {
                _mProgBar = new Windows.UI.Xaml.Controls.ProgressBar();
                _mProgBar.Name = "uiPkAutoProgBar";
                _mProgBar.VerticalAlignment = Windows.UI.Xaml.VerticalAlignment.Bottom;
                _mProgBar.HorizontalAlignment = Windows.UI.Xaml.HorizontalAlignment.Stretch;
                _mProgBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;

                Windows.UI.Xaml.Controls.Canvas.SetZIndex(_mProgRing, 10000);

                if (iRows > 1)
                {
                    Windows.UI.Xaml.Controls.Grid.SetRow(_mProgBar, iRows - 1);
                }

                if (iCols > 1)
                {
                    Windows.UI.Xaml.Controls.Grid.SetColumn(_mProgBar, 0);
                    Windows.UI.Xaml.Controls.Grid.SetColumnSpan(_mProgBar, iCols);
                }

                oGrid.Children.Add(_mProgBar);
            }
        }//


        // Prog Ring Incrementation
        public static void ProgRingInc()
        {
            if (_mProgBar is null)
            {
                // skoro to nie Grid, to nie ma jak umiescic koniecznych elementow

                //DebugOut("ProgRing(double) wymaga wczesniej ProgRingInit");

                throw new ArgumentException("ProgRing(double) wymaga wczesniej ProgRingInit");
            }

            double dVal = _mProgBar.Value + 1;

            if (dVal > _mProgBar.Maximum)
            {
                //DebugOut("ProgRingInc na wiecej niz Maximum?");

                _mProgBar.Value = _mProgBar.Maximum;
            }
            else
            {
                _mProgBar.Value = dVal;
            }
        }

        public static void ProgRingShow(bool bVisible, bool bForce = false, double dMin = 0d, double dMax = 100d)
        {
            if (_mProgBar is object)
            {
                _mProgBar.Minimum = dMin;
                _mProgBar.Value = dMin;
                _mProgBar.Maximum = dMax;
            }

            if (bForce)
            {
                if (bVisible)
                {
                    _mProgRingShowCnt = 1;
                }
                else
                {
                    _mProgRingShowCnt = 0;
                }
            }
            else if (bVisible)
            {
                _mProgRingShowCnt += 1;
            }
            else
            {
                _mProgRingShowCnt -= 1;
            }

            //DebugOut("ProgRingShow(" + bVisible + ", " + bForce + "...), current ShowCnt=" + _mProgRingShowCnt +")" );

            try
            {
                if (_mProgRingShowCnt > 0)
                {
                    //DebugOut("ProgRingShow - mam pokazac");

                    if (_mProgRing is object)
                    {
                        double dSize;
                        var oGrid = _mProgRing.Parent as Windows.UI.Xaml.Controls.Grid;
                        dSize = Math.Min(oGrid.ActualHeight, oGrid.ActualWidth) / 2;
                        dSize = Math.Max(dSize, 200); // jakby jeszcze nie by?o ustawione (Android!)
                        _mProgRing.Width = dSize;
                        _mProgRing.Height = dSize;
                        _mProgRing.Visibility = Windows.UI.Xaml.Visibility.Visible;
                        _mProgRing.IsActive = true;
                    }

                    if (_mProgBar is object)
                        _mProgBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else
                {
                    //DebugOut("ProgRingShow - mam ukryc");

                    if (_mProgRing is object)
                    {
                        _mProgRing.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                        _mProgRing.IsActive = false;
                    }

                    if (_mProgBar is object)
                        _mProgBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                }
            }
            catch (Exception ex)
            {
                DebugOut("Exception [Ring] :" + ex.Message);
            }

        }


        public static void ProgRingVal(double dValue)
        {
            if (_mProgBar is null)
            {
                // skoro to nie Grid, to nie ma jak umiescic koniecznych elementow
                DebugOut("ProgRing(double) wymaga wczesniej ProgRingInit");
                throw new ArgumentException("ProgRing(double) wymaga wczesniej ProgRingInit");
            }

            _mProgBar.Value = dValue;
        }


        #endregion
        // -----------------------------------------------------------------------





        // -- Network n Mobile ---------------------------------------------------


        public static bool NetIsMobile()
        {
            return Windows.System.Profile.AnalyticsInfo.VersionInfo.DeviceFamily == "Windows.Mobile";
        }

        public static bool NetIsIPavailable(bool bMsg)
        {
            if (GetSettingsBool("offline"))
                return false;
            if (System.Net.NetworkInformation.NetworkInterface.GetIsNetworkAvailable())
                return true;
            if (bMsg)
            {
                /* TODO ERROR: Skipped WarningDirectiveTrivia
                #Disable Warning BC42358
                */
                DialogBox("ERROR: no IP network available");
                /* TODO ERROR: Skipped WarningDirectiveTrivia
                #Enable Warning BC42358
                */
            }

            return false;
        }

        //
        public static bool NetIsCellInet()
        {
            return Windows.Networking.Connectivity.NetworkInformation.GetInternetConnectionProfile().IsWwanConnectionProfile;
        }

        public static string GetHostName()
        {
            IReadOnlyList<Windows.Networking.HostName> hostNames = Windows.Networking.Connectivity.NetworkInformation.GetHostNames();
            foreach (Windows.Networking.HostName oItem in hostNames)
            {
                if (oItem.DisplayName.Contains(".local"))
                {
                    return oItem.DisplayName.Replace(".local", "");
                }
            }

            return "";
        }

        //
        public static bool IsThisMoje()
        {
            string sTmp = GetHostName().ToLower();
            if (sTmp == "home-pkar")
                return true;
            if (sTmp == "lumia_pkar")
                return true;
            if (sTmp == "kuchnia_pk")
                return true;
            if (sTmp == "ppok_pk")
                return true;
            // If sTmp.Contains("pkar") Then Return True
            // If sTmp.EndsWith("_pk") Then Return True
            return false;
        }

        public async static Task<bool> NetWiFiOffOn()
        {

            // https://social.msdn.microsoft.com/Forums/ie/en-US/60c4a813-dc66-4af5-bf43-e632c5f85593/uwpbluetoothhow-to-turn-onoff-wifi-bluetooth-programmatically?forum=wpdevelop
            var result222 = await Windows.Devices.Radios.Radio.RequestAccessAsync();
            IReadOnlyList<Windows.Devices.Radios.Radio> radios = await Windows.Devices.Radios.Radio.GetRadiosAsync();
            foreach (var oRadio in radios)
            {
                if (oRadio.Kind == Windows.Devices.Radios.RadioKind.WiFi)
                {
                    Windows.Devices.Radios.RadioAccessStatus oStat = await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.Off);

                    if (oStat != Windows.Devices.Radios.RadioAccessStatus.Allowed)
                    {
                        return false;
                    }

                    await Task.Delay(3 * 1000);

                    oStat = await oRadio.SetStateAsync(Windows.Devices.Radios.RadioState.On);

                    if (oStat != Windows.Devices.Radios.RadioAccessStatus.Allowed)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        // ------------------------------------------------------------------------------------


        // --- INNE FUNKCJE -------------------------------------------------------------------

        //public static void SetBadgeNo(int iInt)
        //{
        //    // https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges

        //    Windows.Data.Xml.Dom.XmlDocument oXmlBadge;
        //    oXmlBadge = Windows.UI.Notifications.BadgeUpdateManager.GetTemplateContent(Windows.UI.Notifications.BadgeTemplateType.BadgeNumber);

        //    Windows.Data.Xml.Dom.XmlElement oXmlNum;
        //    oXmlNum = (Windows.Data.Xml.Dom.XmlElement)oXmlBadge.SelectSingleNode("/badge");
        //    oXmlNum.SetAttribute("value", iInt.ToString());

        //    Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new Windows.UI.Notifications.BadgeNotification(oXmlBadge));
        //}


        public static string XmlSafeString(string sInput)
        {
            if (sInput is null) return "";
            string sTmp;
            sTmp = sInput.Replace("&", "&amp;");
            sTmp = sTmp.Replace("<", "&lt;");
            sTmp = sTmp.Replace(">", "&gt;");
            return sTmp;
        }

        public static string XmlSafeStringQt(string sInput)
        {
            string sTmp;
            sTmp = XmlSafeString(sInput);
            sTmp = sTmp.Replace("\"", "&quote;");
            return sTmp;
        }

        // ----------------------------------------------------------------------------


        // -- TOASTS ------------------------------------------------------------------

        public static void MakeToast(string sMsg, string sMsg1 = "")
        {
            /*
            string sXml = "<visual><binding template='ToastGeneric'><text>" + XmlSafeString(sMsg);
            if (!string.IsNullOrEmpty(sMsg1))
                sXml = sXml + "</text><text>" + XmlSafeString(sMsg1);
            sXml = sXml + "</text></binding></visual>";
            var oXml = new Windows.Data.Xml.Dom.XmlDocument();
            oXml.LoadXml("<toast>" + sXml + "</toast>");
            var oToast = new Windows.UI.Notifications.ToastNotification(oXml);
            Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(oToast);
            */


#if NETFX_CORE || __ANDROID__
                string sHdr = "";
                string sAttrib = "";

                //if (WinVer() > 15062)
                //{
                //    // jako header
                //    // https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/toast-headers
                //    sHdr = "<header id=\"SmogMeter\" title=\"SmogMeter\" />";
                //}
                //else
                //{
                //    // ElseIf WinVer() > 14392 Then - ale to jest spelnione, bo kompilacja ma minimum 14393
                //    // https://docs.microsoft.com/en-us/windows/uwp/design/shell/tiles-and-notifications/adaptive-interactive-toasts
                //    sAttrib = "<text placement=\"attribution\">SmogMeter</text>";
                //}

                var sXml = "<visual><binding template='ToastGeneric'>" + sAttrib + "<text>" + XmlSafeString(sMsg);
                if (!string.IsNullOrEmpty(sMsg1))
                    sXml = sXml + "</text><text>" + XmlSafeString(sMsg1);
                sXml = sXml + "</text></binding></visual>";
                var oXml = new Windows.Data.Xml.Dom.XmlDocument();
                oXml.LoadXml("<toast>" + sHdr + sXml + "</toast>");
                var oToast = new Windows.UI.Notifications.ToastNotification(oXml);
                Windows.UI.Notifications.ToastNotificationManager.CreateToastNotifier().Show(oToast);

#else
            // Xam.Plugins.Notifier

            string sTitle, sBody;
            if (sMsg1 == "")
            {
                sTitle = "";
                sBody = sMsg;
            }
            else
            {
                sTitle = sMsg;
                sBody = sMsg1;
            }

            //TODO : RnD
            //Plugin.LocalNotifications.CrossLocalNotifications.Current.Show(sTitle, sBody);
#endif
        }//MakeToast



        // -- Future ?? ----------------------------------------------------

        //public async static System.Threading.Tasks.Task<string> HttpPageAsync(string sUrl, string sErrMsg, string sData = "")
        //{
        //    try
        //    {
        //        if (!NetIsIPavailable(true))
        //            return "";
        //        if (string.IsNullOrEmpty(sUrl))
        //            return "";

        //        if ((sUrl.Substring(0, 4) ?? "") != "http")
        //            sUrl = "http://beskid.geo.uj.edu.pl/p/dysk" + sUrl;

        //        if (moHttp == null)
        //        {
        //            moHttp = new Windows.Web.Http.HttpClient();
        //            moHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("GrajCyganie");
        //        }

        //        var sError = "";
        //        Windows.Web.Http.HttpResponseMessage oResp = null;

        //        try
        //        {
        //            if (!string.IsNullOrEmpty(sData))
        //            {
        //                var oHttpCont = new Windows.Web.Http.HttpStringContent(sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
        //                oResp = await moHttp.PostAsync(new Uri(sUrl), oHttpCont);
        //            }
        //            else
        //                oResp = await moHttp.GetAsync(new Uri(sUrl));
        //        }
        //        catch (Exception ex)
        //        {
        //            sError = ex.Message;
        //        }

        //        if (!string.IsNullOrEmpty(sError))
        //        {
        //            await DialogBox("error " + sError + " at " + sErrMsg + " page");
        //            return "";
        //        }

        //        if ((oResp.StatusCode == 303) || (oResp.StatusCode == 302) || (oResp.StatusCode == 301))
        //        {
        //            // redirect
        //            sUrl = oResp.Headers.Location.ToString;
        //            // If sUrl.ToLower.Substring(0, 4) <> "http" Then
        //            // sUrl = "https://sympatia.onet.pl/" & sUrl   ' potrzebne przy szukaniu
        //            // End If

        //            if (!string.IsNullOrEmpty(sData))
        //            {
        //                // Dim oHttpCont = New HttpStringContent(sData, Text.Encoding.UTF8, "application/x-www-form-urlencoded")
        //                var oHttpCont = new Windows.Web.Http.HttpStringContent(sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
        //                oResp = await moHttp.PostAsync(new Uri(sUrl), oHttpCont);
        //            }
        //            else
        //                oResp = await moHttp.GetAsync(new Uri(sUrl));
        //        }

        //        if (oResp.StatusCode > 290)
        //        {
        //            await DialogBox("ERROR " + oResp.StatusCode + " getting " + sErrMsg + " page");
        //            return "";
        //        }

        //        string sResp = "";
        //        try
        //        {
        //            sResp = await oResp.Content.ReadAsStringAsync;
        //        }
        //        catch (Exception ex)
        //        {
        //            sError = ex.Message;
        //        }

        //        if (!string.IsNullOrEmpty(sError))
        //        {
        //            await DialogBox("error " + sError + " at ReadAsStringAsync " + sErrMsg + " page");
        //            return "";
        //        }

        //        return sResp;
        //    }
        //    catch (Exception ex)
        //    {
        //        CrashMessageExit("@HttpPageAsync", ex.Message);
        //    }

        //    return "";
        //}


        // 
        public static void SetBadgeNo(int iInt)
        {
            // https://docs.microsoft.com/en-us/windows/uwp/controls-and-patterns/tiles-and-notifications-badges

            Windows.Data.Xml.Dom.XmlDocument oXmlBadge;

            oXmlBadge = Windows.UI.Notifications.BadgeUpdateManager.GetTemplateContent(Windows.UI.Notifications.BadgeTemplateType.BadgeNumber);

            Windows.Data.Xml.Dom.XmlElement oXmlNum;
            oXmlNum = (Windows.Data.Xml.Dom.XmlElement)oXmlBadge.SelectSingleNode("/badge");
            oXmlNum.SetAttribute("value", iInt.ToString());
            Windows.UI.Notifications.BadgeUpdateManager.CreateBadgeUpdaterForApplication().Update(new Windows.UI.Notifications.BadgeNotification(oXmlBadge));
        }


        //
        public static string ToastAction(string sAType, string sAct, string sGuid, string sContent)
        {
            string sTmp = sContent;
            if (!string.IsNullOrEmpty(sTmp))
                sTmp = GetSettingsString(sTmp, sTmp);
            string sTxt = "<action " + "activationType=\"" + sAType + "\" " + "arguments=\"" + sAct + sGuid + "\" " + "content=\"" + sTmp + "\"/> ";
            return sTxt;
        }


        // ?
        private static Windows.Web.Http.HttpClient moHttp = new Windows.Web.Http.HttpClient();

        // 
        public async static Task<string> HttpPageAsync(string sUrl, string sErrMsg, string sData = "")
        {
            try
            {
                if (!NetIsIPavailable(true))
                    return "";

                if (string.IsNullOrEmpty(sUrl))
                    return "";

                if (sUrl.Substring(0, 4) != "http")
                    sUrl = "http://beskid.geo.uj.edu.pl/p/dysk" + sUrl;
                if (moHttp is null)
                {
                    moHttp = new Windows.Web.Http.HttpClient();
                    moHttp.DefaultRequestHeaders.UserAgent.TryParseAdd("GrajCyganie");
                }

                string sError = "";

                Windows.Web.Http.HttpResponseMessage oResp = default;
                try
                {
                    if (!string.IsNullOrEmpty(sData))
                    {
                        var oHttpCont = new Windows.Web.Http.HttpStringContent
                            (
                            sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded"
                            );

                        oResp = await moHttp.PostAsync(new Uri(sUrl), oHttpCont);
                    }
                    else
                    {
                        oResp = await moHttp.GetAsync(new Uri(sUrl));
                    }
                }
                catch (Exception ex)
                {
                    sError = ex.Message;
                }

                if (!string.IsNullOrEmpty(sError))
                {
                    await DialogBox("error " + sError + " at " + sErrMsg + " page");
                    return "";
                }

                if
                (
                    oResp.StatusCode.ToString() == "303"
                    |
                    oResp.StatusCode.ToString() == "302"
                    |
                    oResp.StatusCode.ToString() == "301"
                )
                {
                    // redirect
                    sUrl = oResp.Headers.Location.ToString();
                    // If sUrl.ToLower.Substring(0, 4) <> "http" Then
                    // sUrl = "https://sympatia.onet.pl/" & sUrl   ' potrzebne przy szukaniu
                    // End If

                    if (!string.IsNullOrEmpty(sData))
                    {
                        // Dim oHttpCont = New HttpStringContent(sData, Text.Encoding.UTF8, "application/x-www-form-urlencoded")
                        var oHttpCont = new Windows.Web.Http.HttpStringContent(sData, Windows.Storage.Streams.UnicodeEncoding.Utf8, "application/x-www-form-urlencoded");
                        oResp = await moHttp.PostAsync(new Uri(sUrl), oHttpCont);
                    }
                    else
                    {
                        oResp = await moHttp.GetAsync(new Uri(sUrl));
                    }
                }

                if ((int)oResp.StatusCode > 290)
                {
                    await DialogBox("ERROR " + oResp.StatusCode + " getting " + sErrMsg + " page");
                    return "";
                }

                string sResp = "";
                try
                {
                    sResp = await oResp.Content.ReadAsStringAsync();
                }
                catch (Exception ex)
                {
                    sError = ex.Message;
                }

                if (!string.IsNullOrEmpty(sError))
                {
                    await DialogBox("error " + sError + " at ReadAsStringAsync " + sErrMsg + " page");
                    return "";
                }

                return sResp;
            }
            catch (Exception ex)
            {
                CrashMessageExit("@HttpPageAsync", ex.Message);
            }

            return "";
        }

        //
        public static string RemoveHtmlTags(string sHtml)
        {
            int iInd0, iInd1;
            iInd0 = sHtml.IndexOf("<script");
            if (iInd0 > 0)
            {
                iInd1 = sHtml.IndexOf("</script>", iInd0);
                if (iInd1 > 0)
                {
                    sHtml = sHtml.Remove(iInd0, iInd1 - iInd0 + 9);
                }
            }

            iInd0 = sHtml.IndexOf("<");
            iInd1 = sHtml.IndexOf(">");

            while (iInd0 > -1)
            {
                if (iInd1 > -1)
                {
                    sHtml = sHtml.Remove(iInd0, iInd1 - iInd0 + 1);
                }
                else
                {
                    sHtml = sHtml.Substring(0, iInd0);
                }

                sHtml = sHtml.Trim();
                iInd0 = sHtml.IndexOf("<");
                iInd1 = sHtml.IndexOf(">");
            }

            sHtml = sHtml.Replace("&nbsp;", " ");
            sHtml = sHtml.Replace(" ", "  ");
            sHtml = sHtml.Replace("  " + " ", " ");
            sHtml = sHtml.Replace(" " + "  ", " ");
            sHtml = sHtml.Replace(" " + " ", " ");
            return sHtml.Trim();
        }

        public static void OpenBrowser(Uri oUri, bool bForceEdge)
        {
            if (bForceEdge)
            {
                var options = new Windows.System.LauncherOptions();

                options.TargetApplicationPackageFamilyName = "Microsoft.MicrosoftEdge_8wekyb3d8bbwe";

                // TODO ERROR: Skipped WarningDirectiveTrivia
                //#Disable Warning BC42358               
                Windows.System.Launcher.LaunchUriAsync(oUri, options);
            }
            else
            {
                Windows.System.Launcher.LaunchUriAsync(oUri);
                //TODO ERROR: Skipped WarningDirectiveTrivia
                //#Enable Warning BC42358
            }
        }

        public static void OpenBrowser(string sUri, bool bForceEdge)
        {
            var oUri = new Uri(sUri);
            OpenBrowser(oUri, bForceEdge);
        }

        public static string FileLen2string(long iBytes)
        {
            if (iBytes == 1L)
                return "1 byte";
            if (iBytes < 10000L)
                return iBytes + " bytes";
            iBytes = iBytes / 1024L;
            if (iBytes == 1L)
                return "1 kibibyte";
            if (iBytes < 2000L)
                return iBytes + " kibibytes";
            iBytes = iBytes / 1024L;
            if (iBytes == 1L)
                return "1 mebibyte";
            if (iBytes < 2000L)
                return iBytes + " mebibytes";
            iBytes = iBytes / 1024L;
            if (iBytes == 1L)
                return "1 gibibyte";
            return iBytes + " gibibytes";
        }

        public static DateTime UnixTimeToTime(long lTime)
        {
            // 1509993360
            var dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
            dtDateTime = dtDateTime.AddSeconds(lTime);   // UTC
                                                         // dtDateTime.Kind = DateTimeKind.Utc
            return dtDateTime.ToLocalTime();
        }

        // ------------------------------------------------------------------------------------------------


        // -- CRASH ------------------------------------------------
        #region CrashMessage


        public static void CrashMessageAdd(string sTxt, string exMsg, bool b)
        {
            string sAdd = DateTime.Now.ToString("HH:mm:ss") + " " + sTxt + "  " + exMsg + "  ";
            /* TODO ERROR: Skipped IfDirectiveTrivia
            #If DEBUG Then
            *//* TODO ERROR: Skipped DisabledTextTrivia
                    MakeToast(sAdd)
                    Debug.WriteLine(sAdd)
            *//* TODO ERROR: Skipped ElseDirectiveTrivia
            #Else
            */

            if (GetSettingsBool("crashShowToast"))
            {
                MakeToast(sAdd);
            }

            /* TODO ERROR: Skipped EndIfDirectiveTrivia
            #End If
            */
            SetSettingsString("appFailData", GetSettingsString("appFailData") + sAdd);
        }

        public static void CrashMessageAdd(string sTxt)
        {
            string sAdd = DateTime.Now.ToString("HH:mm:ss") + " " + sTxt + "  ";

            /* TODO ERROR: Skipped IfDirectiveTrivia
            #If DEBUG Then
            *//* TODO ERROR: Skipped DisabledTextTrivia
                    MakeToast(sAdd)
                    Debug.WriteLine(sAdd)
            *//* TODO ERROR: Skipped ElseDirectiveTrivia
            #Else
            */

            if (GetSettingsBool("crashShowToast"))
            {
                MakeToast(sAdd);
            }

            /* TODO ERROR: Skipped EndIfDirectiveTrivia
            #End If
            */

            SetSettingsString("appFailData", GetSettingsString("appFailData") + sAdd);
        }


        public static void CrashMessageAdd(string sTxt, string exMsg)
        {
            string sAdd = DateTime.Now.ToString("HH:mm:ss") + " " + sTxt + "\n" + exMsg + "\n";

            /* TODO ERROR: Skipped IfDirectiveTrivia *//* TODO ERROR: Skipped DisabledTextTrivia */
            /* TODO ERROR: Skipped ElseDirectiveTrivia */
            if (GetSettingsBool("crashShowToast"))
            {
                MakeToast(sAdd);
            }
            /* TODO ERROR: Skipped EndIfDirectiveTrivia */

            SetSettingsString("appFailData", GetSettingsString("appFailData") + sAdd);
        }

        public static void CrashMessageAdd(string sTxt, Exception ex)
        {
            string sMsg = ex.Message;
            if (!string.IsNullOrEmpty(ex.StackTrace))
                sMsg = sMsg + "\n" + ex.StackTrace;

            CrashMessageAdd(sTxt, sMsg);
        }


        public async static Task CrashMessageShow()
        {
            string sTxt = GetSettingsString("appFailData");
            if (string.IsNullOrEmpty(sTxt))
                return;
            await DialogBox("Fail message:" + "  " + sTxt);
            SetSettingsString("appFailData", "");
        }

        public static void CrashMessageExit(string sTxt, string exMsg)
        {
            CrashMessageAdd(sTxt, exMsg);
#if NETFX_CORE || __ANDROID__
            Windows.UI.Xaml.Application.Current.Exit();
            // Android.OS.Process.KillProcess(Android.OS.Process.MyPid());
#elif __IOS__
            System.Threading.Thread.CurrentThread.Abort();
#endif
        }


        #endregion
        // --------------------------------------------------------------------------------


        // -- CLIPBOARD -------------------------------------------------------------------

        #region ClipBoard



        //
        public static void ClipPut(string sTxt)
        {
            var oClipCont = new Windows.ApplicationModel.DataTransfer.DataPackage();
            oClipCont.RequestedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Copy;
            oClipCont.SetText(sTxt);
            Windows.ApplicationModel.DataTransfer.Clipboard.SetContent(oClipCont);
        }

        //
        public async static System.Threading.Tasks.Task<string> ClipGet()
        {
            Windows.ApplicationModel.DataTransfer.DataPackageView oClipCont
                = Windows.ApplicationModel.DataTransfer.Clipboard.GetContent();

            return await oClipCont.GetTextAsync();
        }

#if false
#if __ANDROID__ || __IOS__
            return await Plugin.Clipboard.CrossClipboard.Current.GetTextAsync().ConfigureAwait(true);
#elif __WASM__
            return "WASM vers";
#elif __MACOS__
            return "macOS vers";
#else
            return "unkn vers";
#endif
#endif


        #endregion

        // -----------------------------------------------------------------



        // -- Get/Set Settings ---------------------------------------------

        #region Get/Set settings

        #region String


        public static string GetSettingsString(TextBlock oTBox, string sName, string sDefault = "")
        {
            if (oTBox is null)
            {
                return "";
            }

            string sTmp = GetSettingsString(sName, sDefault);
            oTBox.Text = sTmp;

            return sTmp;
        }

        public static string GetSettingsString(TextBox oTBox, string sName, string sDefault = "")
        {
            string sTmp = GetSettingsString(sName, sDefault);
            oTBox.Text = sTmp;
            return sTmp;
        }

        public static string GetSettingsString(string sName, string sDefault = "")
        {
            string sTmp;
            sTmp = sDefault;
            {
                var withBlock = Windows.Storage.ApplicationData.Current;

                if (withBlock.RoamingSettings.Values.ContainsKey(sName))
                {
                    sTmp = withBlock.RoamingSettings.Values[sName].ToString();
                }

                if (withBlock.LocalSettings.Values.ContainsKey(sName))
                {
                    sTmp = withBlock.LocalSettings.Values[sName].ToString();
                }
            }

            return sTmp;
        }

        //
        public static void SetSettingsString(string sName, string sValue)
        {
            SetSettingsString(sName, sValue, false);
        }

        //
        public static void SetSettingsString(string sName, string sValue, bool bRoam)
        {
            {
                var withBlock = Windows.Storage.ApplicationData.Current;

                if (bRoam)
                    withBlock.RoamingSettings.Values[sName] = sValue;

                withBlock.LocalSettings.Values[sName] = sValue;
            }
        }

        public static void SetSettingsString(string sName, TextBox sValue, bool bRoam)
        {
            SetSettingsString(sName, sValue.Text, bRoam);
        }

        public static void SetSettingsString(string sName, TextBox sValue)
        {
            SetSettingsString(sName, sValue.Text, false);
        }

        #endregion


        #region Int

        //
        public static int GetSettingsInt(string sName, int iDefault = 0)
        {
            int sTmp;
            sTmp = iDefault;

            try
            {
                var withBlock = Windows.Storage.ApplicationData.Current;
                if (withBlock.RoamingSettings.Values.ContainsKey(sName))
                    sTmp = System.Convert.ToInt32(withBlock.RoamingSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                if (withBlock.LocalSettings.Values.ContainsKey(sName))
                    sTmp = System.Convert.ToInt32(withBlock.LocalSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
            }
            catch (Exception ex)
            {
                DebugOut("GetSettingsInt Exception: " + ex.Message);
            }

            return sTmp;
        }

        //
        public static void SetSettingsInt(string sName, int sValue)
        {
            SetSettingsInt(sName, sValue, false);
        }

        //
        public static void SetSettingsInt(string sName, int sValue, bool bRoam)
        {
            var withBlock = Windows.Storage.ApplicationData.Current;

            if (bRoam)
            {
                withBlock.RoamingSettings.Values[sName]
                    = sValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
            }

            withBlock.LocalSettings.Values[sName] = sValue.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }


        #endregion



        #region Bool


        //
        public static bool GetSettingsBool(string sName, bool iDefault = false)
        {
            bool sTmp;

            sTmp = iDefault;
            {
                var withBlock = Windows.Storage.ApplicationData.Current;
                if (withBlock.RoamingSettings.Values.ContainsKey(sName))
                {
                    sTmp = System.Convert.ToBoolean(withBlock.RoamingSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }

                if (withBlock.LocalSettings.Values.ContainsKey(sName))
                {
                    sTmp = System.Convert.ToBoolean(withBlock.LocalSettings.Values[sName].ToString(), System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return sTmp;
        }

        //
        public static bool GetSettingsBool(Windows.UI.Xaml.Controls.ToggleSwitch oSwitch, string sName, bool iDefault = false)
        {
            if (oSwitch is null) return iDefault;
            bool sTmp;
            sTmp = GetSettingsBool(sName, iDefault);
            oSwitch.IsOn = sTmp;
            return sTmp;
        }

        //
        public static void SetSettingsBool(string sName, bool sValue)
        {
            SetSettingsBool(sName, sValue, false);
        }

        //
        public static void SetSettingsBool(string sName, bool sValue, bool bRoam)
        {
            {
                var withBlock = Windows.Storage.ApplicationData.Current;

                if (bRoam)
                {
                    withBlock.RoamingSettings.Values[sName] = sValue.ToString();
                }

                withBlock.LocalSettings.Values[sName] = sValue.ToString();
            }
        }

        //
        public static void SetSettingsBool(ToggleSwitch sValue, string sName, bool bRoam = false)
        {
            SetSettingsBool(sName, sValue.IsOn, bRoam);
        }

        //
        public static void SetSettingsBool(string sName, ToggleSwitch sValue, bool bRoam)
        {
            SetSettingsBool(sName, sValue.IsOn, bRoam);
        }

        //
        public static void SetSettingsBool(string sName, ToggleSwitch sValue)
        {
            SetSettingsBool(sName, sValue.IsOn, false);
        }


        //
        public static void SetSettingsBool(string sName, bool? sValue, bool bRoam = false)
        {

            if (sValue.HasValue && sValue.Value)
                SetSettingsBool(sName, true, bRoam);
            else
                SetSettingsBool(sName, false, bRoam);
        }


        #endregion

        #endregion



        // -- Testy sieciowe ---------------------------------------------

        #region testy sieciowe

        // ...

        #endregion


        // -- DialogBoxy ---------------------------------------------

        #region DialogBoxy


        public async static Task DialogBox(string sMsg)
        {
            var oMsg = new Windows.UI.Popups.MessageDialog(sMsg);
            await oMsg.ShowAsync();
        }

        public static string GetLangString(string sMsg)
        {
            if (string.IsNullOrEmpty(sMsg))
                return "";
            string sRet = sMsg;
            try
            {
                sRet = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView().GetString(sMsg);
            }
            catch
            {
            }

            return sRet;
        }

        public async static Task DialogBoxRes(string sMsg, string sErrData)
        {
            sMsg = GetLangString(sMsg) + " " + sErrData;
            await DialogBox(sMsg);
        }

        public async static Task DialogBoxResAsync(string sMsg, string sErrData)
        {
            sMsg = GetLangString(sMsg) + " " + sErrData;
            await DialogBox(sMsg);
        }

        public async static Task DialogBoxRes(string sMsg)
        {
            sMsg = GetLangString(sMsg);
            await DialogBox(sMsg);
        }

        public async static Task DialogBoxResAsync(string sMsg)
        {
            sMsg = GetLangString(sMsg);
            await DialogBox(sMsg);
        }

        public async static void DialogBoxError(int iNr, string sMsg)
        {
            string sTxt = GetLangString("errAnyError");
            sTxt = sTxt + " (" + iNr + ")" + "  " + sMsg;
            await DialogBox(sMsg);
        }

        public async static Task<bool> DialogBoxYN(string sMsg, string sYes = "Tak", string sNo = "Nie")
        {
            var oMsg = new Windows.UI.Popups.MessageDialog(sMsg);
            var oYes = new Windows.UI.Popups.UICommand(sYes);
            var oNo = new Windows.UI.Popups.UICommand(sNo);
            oMsg.Commands.Add(oYes);
            oMsg.Commands.Add(oNo);
            oMsg.DefaultCommandIndex = 1;    // default: No
            oMsg.CancelCommandIndex = 1;

            Windows.UI.Popups.IUICommand oCmd = await oMsg.ShowAsync();

            if (oCmd is null)
                return false;
            if (oCmd.Label == sYes)
                return true;
            return false;
        }


        public async static Task<bool> DialogBoxYNAsync(string sMsg, string sYes = "Tak", string sNo = "Nie")
        {
            var oMsg = new Windows.UI.Popups.MessageDialog(sMsg);
            var oYes = new Windows.UI.Popups.UICommand(sYes);
            var oNo = new Windows.UI.Popups.UICommand(sNo);
            oMsg.Commands.Add(oYes);
            oMsg.Commands.Add(oNo);
            oMsg.DefaultCommandIndex = 1;    // default: No
            oMsg.CancelCommandIndex = 1;

            Windows.UI.Popups.IUICommand oCmd = await oMsg.ShowAsync();

            if (oCmd is null)
                return false;
            if (oCmd.Label == sYes)
                return true;
            return false;
        }

        public async static Task<bool> DialogBoxResYN(string sMsgResId, string sYesResId = "resDlgYes", string sNoResId = "resDlgNo")
        {
            string sMsg, sYes, sNo;
            {
                var withBlock = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                sMsg = withBlock.GetString(sMsgResId);
                sYes = withBlock.GetString(sYesResId);
                sNo = withBlock.GetString(sNoResId);
            }

            if (string.IsNullOrEmpty(sMsg))
                sMsg = sMsgResId;  // zabezpieczenie na brak string w resource
            if (string.IsNullOrEmpty(sYes))
                sYes = sYesResId;
            if (string.IsNullOrEmpty(sNo))
                sNo = sNoResId;
            return await DialogBoxYN(sMsg, sYes, sNo);
        }



        public async static Task<bool> DialogBoxResYNAsync(string sMsgResId, string sYesResId = "resDlgYes", string sNoResId = "resDlgNo")
        {
            string sMsg, sYes, sNo;
            {
                var withBlock = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                sMsg = withBlock.GetString(sMsgResId);
                sYes = withBlock.GetString(sYesResId);
                sNo = withBlock.GetString(sNoResId);
            }

            if (string.IsNullOrEmpty(sMsg))
                sMsg = sMsgResId;  // zabezpieczenie na brak string w resource
            if (string.IsNullOrEmpty(sYes))
                sYes = sYesResId;
            if (string.IsNullOrEmpty(sNo))
                sNo = sNoResId;
            return await DialogBoxYN(sMsg, sYes, sNo);
        }


        //
        public async static Task<string> DialogBoxInput(string sMsgResId, string sDefaultResId = "", string sYesResId = "resDlgContinue", string sNoResId = "resDlgCancel")
        {
            string sMsg, sYes, sNo, sDefault;
            
            sDefault = "";
            
            {
                var withBlock = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();
                
                sMsg = withBlock.GetString(sMsgResId);
                
                sYes = withBlock.GetString(sYesResId);
                
                sNo = withBlock.GetString(sNoResId);
                
                if (!string.IsNullOrEmpty(sDefaultResId))
                    sDefault = withBlock.GetString(sDefaultResId);
            }

            if (string.IsNullOrEmpty(sMsg))
                sMsg = sMsgResId;  // zabezpieczenie na brak string w resource

            if (string.IsNullOrEmpty(sYes))
                sYes = sYesResId;
            
            if (string.IsNullOrEmpty(sNo))
                sNo = sNoResId;
            
            if (string.IsNullOrEmpty(sDefault))
                sDefault = sDefaultResId;
            
            var oInputTextBox = new TextBox();
            
            oInputTextBox.AcceptsReturn = false;
            
            oInputTextBox.Text = sDefault;
            
            var oDlg = new ContentDialog();
            
            oDlg.Content = oInputTextBox;
            
            oDlg.PrimaryButtonText = sYes;
            
            oDlg.SecondaryButtonText = sNo;
            
            oDlg.Title = sMsg;

            var oCmd = await oDlg.ShowAsync();

            if (oCmd != ContentDialogResult.Primary)
                return "";
            
            return oInputTextBox.Text;
        }



        public async static Task<string> DialogBoxAsync(string sMsg)
        {
            var oMsg = new Windows.UI.Popups.MessageDialog(sMsg);

            await oMsg.ShowAsync();

            return "";
        }

        //
        public async static Task<string> DialogBoxInputDirectAsync(string sMsgResId, string sDefaultResId = "")
        {
            string sMsg, sYes, sNo, sDefault;
            sDefault = "";
            {
                var withBlock = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                sMsg = withBlock.GetString(sMsgResId);

                if (!string.IsNullOrEmpty(sDefaultResId))
                    sDefault = withBlock.GetString(sDefaultResId);
            }

            if (string.IsNullOrEmpty(sMsg))
            {
                sMsg = sMsgResId;  // zabezpieczenie na brak string w resource
            }


            if (string.IsNullOrEmpty(sDefault))
            {
                sDefault = sDefaultResId;
            }

            var oInputTextBox = new TextBox();

            oInputTextBox.AcceptsReturn = false;
            oInputTextBox.Text = sDefault;

            var oDlg = new ContentDialog();

            oDlg.Content = oInputTextBox;
            oDlg.Title = sMsg;

            var oCmd = await oDlg.ShowAsync();

            if (oCmd != ContentDialogResult.Primary)
            {
                return "";
            }

            return oInputTextBox.Text;
        }

        // for getting DeviceName, for example
        public async static Task<string> DialogBoxInputDirectAsync(string sMsgResId, string Model, string sDefaultResId = "")
        {
            string sMsg, sYes, sNo, sDefault;

            // My!
            sYes = "OK";

            sDefault = "";
            {
                var withBlock = Windows.ApplicationModel.Resources.ResourceLoader.GetForCurrentView();

                sMsg = withBlock.GetString(sMsgResId);

                if (!string.IsNullOrEmpty(sDefaultResId))
                    sDefault = withBlock.GetString(sDefaultResId);
            }

            if (string.IsNullOrEmpty(sMsg))
            {
                sMsg = sMsgResId;  // zabezpieczenie na brak string w resource
            }


            if (string.IsNullOrEmpty(sDefault))
            {
                sDefault = sDefaultResId;
            }

            var oInputTextBox = new TextBox();

            oInputTextBox.AcceptsReturn = false;
            
            oInputTextBox.Text = sDefault;

            var oDlg = new ContentDialog();
            
            oDlg.Content = oInputTextBox;

            // My !
            oDlg.PrimaryButtonText = sYes; 

            oDlg.Title = sMsg;

            var oCmd = await oDlg.ShowAsync();

            if (oCmd != ContentDialogResult.Primary)
            {
                return "";
            }

            return oInputTextBox.Text;
        }

        // ---------------------------------------------------------



        #endregion


    }//class k end

}//namespace p end


