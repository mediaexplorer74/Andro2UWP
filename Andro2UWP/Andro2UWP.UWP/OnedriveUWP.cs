using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using Andro2UWP;
using Windows.UI.Xaml;

namespace p
{
    class od
    {

        //private static Microsoft.OneDrive.Sdk.IOneDriveClient goOneDriveClnt;

        private static bool gInOneDriveCommand = false;

        private static bool gbLimitToAppFolder = true;

        // Private gOneDriveMutex As Threading.Mutex = New Threading.Mutex
        // no Mutex because all in the same thread!

        // GENERAL NOTE:
        // all functions use Mutex, so they can't invoke each other!
        // if they have, they have to be separated into FUNCTION INT and FUNCTION,
        // so that Private FUNKCJAInt has no Mutex verification,
        // while Public FUNCTION - had

        public static bool IsOneDriveOpened()
        {
            var app = (App)Application.Current;

                           //return goOneDriveClnt != null;
                return app.uOneDriveClient != null;
        }

        public async static Task<bool> OpenOneDrive(bool limitToAppFolder, bool bInteractive)
        {
            p.k.DebugOut("OpenOneDrive(limitToAppFolder=" + limitToAppFolder.ToString());
            if (gInOneDriveCommand)
            {
                p.k.DebugOut("OpenOneDrive: gInOneDriveCommand=true");
                return false;
            }
            gInOneDriveCommand = true;

            bool bRet = await OpenOneDriveInt(bInteractive);
            gbLimitToAppFolder = limitToAppFolder;
            gInOneDriveCommand = false;
            return bRet;
        }

        private async static Task<bool> OpenOneDriveInt(bool bInteractive)
        {
            p.k.DebugOut("OpenOneDriveInt");
            // https://github.com/OneDrive/onedrive-sample-photobrowser-uwp/blob/master/OneDrivePhotoBrowser/AccountSelection.xaml.cs
            // dla PC tu bedzie error, wiec zwróci FALSE

            // If gInOneDriveCommand Then Return False
            // gInOneDriveCommand = True


            var app = (App)Application.Current;

            bool bError = false;


            try
            {   // onedrive.appfolder
                string[] sScopes = new[] { "onedrive.readwrite", "offline_access" };
                const string oneDriveConsumerBaseUrl = "https://api.onedrive.com/v1.0";

                // inny sampel:
                // https://msdn.microsoft.com/en-us/magazine/mt632271.aspx
                // client = OneDriveClientExtensions.GetClientUsingOnlineIdAuthenticator(
                // _scopes);
                // var session = Await client.AuthenticateAsync();
                // Debug.WriteLine($"Token: {session.AccessToken}");


               //------------------------------------------------------------------------------------------- 
               /*
                var onlineIdAuthProvider = new Microsoft.OneDrive.Sdk.OnlineIdAuthenticationProvider(sScopes);
                
                p.k.DebugOut("OpenOneDriveInt: got onlineIdAuthProvider");


                // gdy poniższych dwu linii nie ma, i tak działa
                //await onlineIdAuthProvider.AuthenticateUserAsync();
                //p.k.DebugOut("OpenOneDriveInt: after AuthenticateUserAsync");

                Task authTask;

                if (bInteractive)
                {
                    authTask = onlineIdAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();
                }
                else
                {
                    authTask = onlineIdAuthProvider.RestoreMostRecentFromCacheAsync();
                }

                p.k.DebugOut("OpenOneDriveInt: after RestoreMostRecentFromCacheOrAuthenticateUserAsync");
                
                // Await authTask
                //goOneDriveClnt = 
                app.uOneDriveClient = new Microsoft.OneDrive.Sdk.OneDriveClient
                    (
                        oneDriveConsumerBaseUrl, 
                        onlineIdAuthProvider // TODO
                    );

                p.k.DebugOut("OpenOneDriveInt: got goOneDriveClnt");
                await authTask;     // tu jest w samplu - po moOneDriveClnt

                p.k.DebugOut("OpenOneDriveInt: after authTask");
                */

                //------------------------------------------------------------------------------------------- 


            }
            catch (Exception ex)
            {
                p.k.DebugOut("OpenOneDriveInt catch: " + ex.Message );
                p.k.CrashMessageAdd("OpenOneDriveInt", ex.Message, true);

                //goOneDriveClnt = null;
                app.uOneDriveClient = null;

                bError = true;
            }

            // gInOneDriveCommand = False
            return !bError;
        }



        // Public Async Function OpenCreateOneDriveFolder(sParentId As String, sName As String, bCreate As Boolean) As Task(Of String)
        // If Not IsOneDriveOpened() Then Return ""

        // If sName = "" Then Return ""

        // If Not gOneDriveMutex.WaitOne(100) Then Return ""

        // Dim oParent As Microsoft.OneDrive.Sdk.ItemRequest

        // If sParentId = "" Then
        // oParent = goOneDriveClnt.Drive.Root.Request
        // Else
        // oParent = goOneDriveClnt.Drive.Items(sParentId).Request
        // End If

        // Dim oLista As Microsoft.OneDrive.Sdk.Item = Await oParent.Expand("children").GetAsync

        // For Each oItem As Microsoft.OneDrive.Sdk.Item In oLista.Children.CurrentPage
        // If oItem.Name = sName Then
        // gInOneDriveCommand = false
        // Return oItem.Id
        // End If
        // Next

        // If Not bCreate Then Return ""

        // ' proba utworzenia katalogu
        // Dim oNew As Microsoft.OneDrive.Sdk.Item = New Microsoft.OneDrive.Sdk.Item
        // oNew.Name = sName
        // oNew.Folder = New Microsoft.OneDrive.Sdk.Folder

        // Dim oFolder As Microsoft.OneDrive.Sdk.Item
        // oFolder = Await goOneDriveClnt.Drive.Root.Children.Request().AddAsync(oNew)

        // gInOneDriveCommand = false
        // Return oFolder.Id

        // End Function

        public async static Task<bool> ReplaceOneDriveFileContent(string sFilePathname, string sTresc)
        {

            MemoryStream oStream = new MemoryStream();
            var oWrtr = new StreamWriter(oStream);
            oWrtr.WriteLine(sTresc);
            oWrtr.Flush();

            bool bRet = await ReplaceOneDriveFileContent(sFilePathname, oStream);

            oWrtr.Dispose();
            oWrtr = null;

            oStream.Dispose();
            oStream = null;

            return bRet;
        }


        private static Microsoft.OneDrive.Sdk.IItemRequestBuilder RootOrAppRoot()
        {
            var app = (App)Application.Current;

            //if (gbLimitToAppFolder)
            //    return goOneDriveClnt.Drive.Special.AppRoot;
            //else

            //return goOneDriveClnt.Drive.Root;
            return app.uOneDriveClient.Drive.Root;
        }

        public async static Task<bool> ReplaceOneDriveFileContent(string sFilePathname, Stream sTresc)
        {
            if (!IsOneDriveOpened())
                return false;     // gdy nie widac OneDrive
            if (gInOneDriveCommand)
                return false;
            gInOneDriveCommand = true;

            bool bError = false;

            sTresc.Seek(0, SeekOrigin.Begin);

            try
            {
                await RootOrAppRoot().ItemWithPath(sFilePathname).Content.Request().PutAsync<Microsoft.OneDrive.Sdk.Item>(sTresc);
            }
            catch (Exception ex)
            {
                bError = true;
            }

            gInOneDriveCommand = false;

            return !bError;
        }


        public async static Task<string> CopyFileToOneDrive(Windows.Storage.StorageFile oFile, string sFolderPath, bool bCanResetWifi)
        {
            var app = (App)Application.Current;

            // return: link (zeby mozna bylo bez OneDrive sie dostac do ostatniej ramki

            if (!IsOneDriveOpened())
                return "";

            if (gInOneDriveCommand)
                return "";

            gInOneDriveCommand = true;

            // oneDriveClient.Drive.Root.ItemWithPath("Apps/BicycleApp/ALUWP.db").Request().GetAsync();

            try
            {
                Stream oStream = await oFile.OpenStreamForReadAsync();
                if (!oStream.CanRead)
                {
                    p.k.CrashMessageAdd("@CopyFileToOneDrive", "not readable stream?");
                    return "";
                }

                Microsoft.OneDrive.Sdk.Item oItem = null;
                bool bError = false;

                string sOutFileName = sFolderPath + "/" + oFile.Name;

                try
                {
                    oItem = await RootOrAppRoot().ItemWithPath(sOutFileName).Content.Request().PutAsync<Microsoft.OneDrive.Sdk.Item>(oStream);   // (oRdr.BaseStream)
                }
                catch (Exception ex)
                {
                    p.k.CrashMessageAdd("@CopyFileToOneDrive while trying to copy file (try 1)", ex);
                    bError = true;
                }

                if (bError)
                {
                    // czasem nie kopiuje, jakby blokada i potrzeba reconnect?
                    return "";
                    //if (!bCanResetWifi || !await p.k.NetWiFiOffOn())
                    //{
                    //    p.k.CrashMessageAdd("cannot reconnect WiFi", "");
                    //    return "";
                    //}

                    //// If mbInDebug Then Debug.WriteLine("wifi reconnect OK")
                    //await Task.Delay(15 * 1000);     // 10 sekund na przywrócenie WiFi
                    //if (!await OpenOneDriveInt())
                    //{
                    //    p.k.CrashMessageAdd("cannot reconnect OneDrive", "");
                    //    bError = true;
                    //}
                    //else
                    //{
                    //    // tu sie zmieniało (przynajmniej czasem) na nie CanRead - tak błąd z PutAsync był
                    //    if (!oStream.CanSeek || !oStream.CanRead)
                    //    {
                    //        oStream.Dispose();
                    //        oStream = await oFile.OpenStreamForReadAsync();
                    //    }
                    //    else
                    //        oStream.Seek(0, SeekOrigin.Begin);

                    //    oItem = await goOneDriveClnt.Drive.Root.ItemWithPath(sOutFileName).Content.Request().PutAsync<Microsoft.OneDrive.Sdk.Item>(oStream);   // (oRdr.BaseStream)
                    //}
                }

                oStream.Dispose();
                oStream = null;

                string sLink = "";

                if (oItem != null)
                {
                    Microsoft.OneDrive.Sdk.Permission oLink = null;
                    
                    
                    oLink = await 
                        app.uOneDriveClient.Drive.Items[oItem.Id].CreateLink("view").Request().PostAsync();
                    //goOneDriveClnt.Drive.Items[oItem.Id].CreateLink("view").Request().PostAsync();

                    sLink = oLink.Link.ToString();
                    
                    oLink = null;
                }

                oItem = null;
                gInOneDriveCommand = false;

                // ' próba - czy zmniejszy się zuzycie pamięci
                // gInOneDriveCommand = Nothing
                // Await OpenOneDriveInt()

                return sLink;
            }
            catch (Exception ex)
            {
                gInOneDriveCommand = false;
                return "";
            }
        }


        public async static Task<string> SaveFileToOneDrive(Windows.Storage.StorageFile oFile, string sFolderPath, bool bCanResetWifi)
        {
            var app = (App)Application.Current;

            // return: link (zeby mozna bylo bez OneDrive sie dostac do ostatniej ramki

            if (!IsOneDriveOpened())
            {
                oFile = null/* TODO Change to default(_) if this is not a reference type */;
                return "";
            }

            if (gInOneDriveCommand)
            {
                oFile = null/* TODO Change to default(_) if this is not a reference type */;
                return "";
            }

            gInOneDriveCommand = true;

            // oneDriveClient.Drive.Root.ItemWithPath("Apps/BicycleApp/ALUWP.db").Request().GetAsync();

            try
            {
                Stream oStream = await oFile.OpenStreamForReadAsync();
                if (!oStream.CanRead)
                {
                    p.k.CrashMessageAdd("@CopyFileToOneDrive", "not readable stream?");
                    return "";
                }

                Microsoft.OneDrive.Sdk.Item oItem = null/* TODO Change to default(_) if this is not a reference type */;
                bool bError = false;

                string sOutFileName = sFolderPath + "/" + oFile.Name;

                try
                {
                    oItem = await RootOrAppRoot().ItemWithPath(sOutFileName).Content.Request().PutAsync<Microsoft.OneDrive.Sdk.Item>(oStream);   // (oRdr.BaseStream)
                }
                catch (Exception ex)
                {
                    p.k.CrashMessageAdd("@CopyFileToOneDrive while trying to copy file (try 1)", ex);
                    bError = true;
                }

                if (bError)
                {
                    // czasem nie kopiuje, jakby blokada i potrzeba reconnect?
                    return "";
                    //if (!bCanResetWifi || !await p.k.NetWiFiOffOn())
                    //{
                    //    p.k.CrashMessageAdd("cannot reconnect WiFi", "");
                    //    return "";
                    //}

                    //// If mbInDebug Then Debug.WriteLine("wifi reconnect OK")
                    //await Task.Delay(15 * 1000);     // 10 sekund na przywrócenie WiFi
                    //if (!await OpenOneDriveInt())
                    //{
                    //    p.k.CrashMessageAdd("cannot reconnect OneDrive", "");
                    //    bError = true;
                    //}
                    //else
                    //{
                    //    // tu sie zmieniało (przynajmniej czasem) na nie CanRead - tak błąd z PutAsync był
                    //    if (!oStream.CanSeek || !oStream.CanRead)
                    //    {
                    //        oStream.Dispose();
                    //        oStream = await oFile.OpenStreamForReadAsync();
                    //    }
                    //    else
                    //        oStream.Seek(0, SeekOrigin.Begin);

                    //    oItem = await goOneDriveClnt.Drive.Root.ItemWithPath(sOutFileName).Content.Request().PutAsync<Microsoft.OneDrive.Sdk.Item>(oStream);   // (oRdr.BaseStream)
                    //}
                }

                oStream.Dispose();
                oStream = null;

                string sLink = "";
                if (oItem != null)
                {
                    Microsoft.OneDrive.Sdk.Permission oLink = null/* TODO Change to default(_) if this is not a reference type */;

                    oLink = await
                        app.uOneDriveClient.Drive.Items[oItem.Id].CreateLink("view").Request().PostAsync();
                    //goOneDriveClnt.Drive.Items[oItem.Id].CreateLink("view").Request().PostAsync();

                    sLink = oLink.Link.ToString();
                    
                    oLink = null/* TODO Change to default(_) if this is not a reference type */;
                }
                oItem = null/* TODO Change to default(_) if this is not a reference type */;
                gInOneDriveCommand = false;

                // ' próba - czy zmniejszy się zuzycie pamięci
                // gInOneDriveCommand = Nothing
                // Await OpenOneDriveInt()

                return sLink;
            }
            catch (Exception ex)
            {
                gInOneDriveCommand = false;
                return "";
            }
        }

        // ReadOneDriveTextFileId(string sFileId)
        public async static Task<string> ReadOneDriveTextFileId(string sFileId)
        {
            var stream = await GetOneDriveFileIdStream(sFileId);
            if (stream is null) return null;

            var streamRdr = new StreamReader(stream);
            string retVal = streamRdr.ReadToEnd();
            streamRdr.Dispose();
            if (stream != null) stream.Dispose();

            return retVal;
        }


        // ReadOneDriveTextFile(string sPath)
        public async static Task<string> ReadOneDriveTextFile(string sPath)
        {
            var stream = await GetOneDriveFileStream(sPath);

            if (stream is null)
            {
                return null;
            }

            var streamRdr = new StreamReader(stream);

            string retVal = streamRdr.ReadToEnd();
            
            streamRdr.Dispose();

            if (stream != null)
            {
                stream.Dispose();
            }

            return retVal;

        }//ReadOneDriveTextFile 



        // GetOneDriveFileStream(oItemReq))

        private async static Task<Stream> GetOneDriveFileStream(Microsoft.OneDrive.Sdk.IItemRequestBuilder oItemReq)
        {
            p.k.DebugOut("GetOneDriveFileStream(oItemReq");

            try
            {
                /*
                // ??? That generates Exception !!! :(((
                var oFile = await oItemReq.Request().GetAsync(); // TODO

                if(oFile is null)
                {
                    p.k.DebugOut("GetOneDriveFileStream: oFile is null");
                    return null;
                }

                p.k.DebugOut("GetOneDriveFileStream: got oFile");
                */
                
                Stream oStream = await oItemReq.Content.Request().GetAsync();
                
                p.k.DebugOut("GetOneDriveFileStream: got Stream");

                // Dim oRdr As BinaryReader = New BinaryReader(oStream)
                // oRdr.ReadBytes(1000)

                return oStream;
            }
            catch (Exception ex)
            {
                // but the file may not exist - we accept this option ... TODO
                if (ex.Message != "Item does not exist")
                {
                    p.k.CrashMessageAdd("@GetOneDriveFileStream(oItemReq", ex);
                }

                return null;
            }

        }//GetOneDriveFileStream


        public async static Task<Stream> GetOneDriveFileIdStream(string sFileId)
        {
            p.k.DebugOut("GetOneDriveFileIdStream(" + sFileId);
            // https://msdn.microsoft.com/en-us/magazine/mt632271.aspx

            if (!IsOneDriveOpened())
                return null;

            if (gInOneDriveCommand)
                return null;

            
            gInOneDriveCommand = true;

            var app = (App)Application.Current;

            try
            {
                Microsoft.OneDrive.Sdk.IItemRequestBuilder oItemReq;

                oItemReq = app.uOneDriveClient.Drive.Items[sFileId];
                    //goOneDriveClnt.Drive.Items[sFileId];

                if (oItemReq == null)
                    return null;

                return await GetOneDriveFileStream(oItemReq);
            }
            finally
            {
                gInOneDriveCommand = false;
            }

        }

        // GetOneDriveFileStream(string sFilePath)

        public async static Task<Stream> GetOneDriveFileStream(string sFilePath)
        {
            p.k.DebugOut("GetOneDriveFileStream(" + sFilePath);
            // https://msdn.microsoft.com/en-us/magazine/mt632271.aspx
            if (!IsOneDriveOpened())
                return null;

            if (gInOneDriveCommand)
                return null;

            gInOneDriveCommand = true;

            try
            {
                Microsoft.OneDrive.Sdk.IItemRequestBuilder oItemReq;

                sFilePath = sFilePath.Replace(@"\", "/");
                oItemReq = RootOrAppRoot().ItemWithPath(sFilePath);

                if (oItemReq == null)
                    return null;

                return await GetOneDriveFileStream(oItemReq);
            }
            finally
            {
                gInOneDriveCommand = false;
            }

        }

        //  OneDriveGetAllChilds(string sPathname, bool bFolders, bool bFiles)
        public async static Task<List<string>> OneDriveGetAllChilds(string sPathname, bool bFolders, bool bFiles)
        {
            List<string> lNames = new List<string>();

            if (gInOneDriveCommand)
                return null;
            gInOneDriveCommand = true;

            Microsoft.OneDrive.Sdk.Item oPicLista = null;

            try
            {
                oPicLista = await RootOrAppRoot().ItemWithPath(sPathname).Request().Expand("children").GetAsync();
            }
            catch (Exception ex)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds - get first page", ex);
                return lNames;
            }

            // doing Exception somewhere, so more thorough checks / checking
            if (oPicLista is null)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds - oPicLista null (first page)");
                return lNames;
            }

            if (oPicLista.Children is null)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds - oPicLista.Children null (first page)");
                return lNames;
            }

            if (oPicLista.Children.CurrentPage is null)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds - oPicLista.Children.CurrentPage null (first page)");
                return lNames;
            }

            try
            {
                // Dim oPicItem As Microsoft.OneDrive.Sdk.Item
                for (int iInd = 0; iInd <= oPicLista.Children.CurrentPage.Count - 1; iInd++)
                {
                    // For Each oPicItem As Microsoft.OneDrive.Sdk.Item In oPicLista.Children.CurrentPage
                    Microsoft.OneDrive.Sdk.Item oPicItem = oPicLista.Children.CurrentPage.ElementAt(iInd);
                    if (bFolders && oPicItem.Folder != null)
                        lNames.Add(oPicItem.Name);
                    if (bFiles && oPicItem.File != null)
                        lNames.Add(oPicItem.Name);
                    oPicItem = null;
                }
                oPicLista.Children.CurrentPage.Clear();  // to juz pewnie w ogole niepotrzebne
            }
            catch (Exception ex)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds - iterate first page (should never happen)", ex);
                return lNames;
            }

            if (oPicLista == null)
            {
                gInOneDriveCommand = false;
                return lNames;
            }

            // oPicLista.Children na pewno nie jest null
            if (oPicLista.Children.NextPageRequest == null)
            {
                oPicLista = null;
                gInOneDriveCommand = false;
                return lNames;
            }

            try
            {

                Microsoft.OneDrive.Sdk.IItemChildrenCollectionPage oPicNew = null;
                try
                {
                    oPicNew = await oPicLista.Children.NextPageRequest.GetAsync();
                    oPicLista = null; // juz niepotrzebne
                }
                catch (Exception ex)
                {
                    p.k.CrashMessageAdd("@OneDriveGetAllChilds - get second page", ex);
                }

                if (oPicNew == null)
                {
                    gInOneDriveCommand = false;
                    return lNames;
                }

                for (int iGuard = 1; iGuard <= 12000 / (double)200; iGuard++)   // itemow moze byc, przez itemów na stronę
                {
                    // Microsoft.OneDrive.Sdk.Item oPicItem;
                    for (int iFor = 0; iFor <= oPicNew.CurrentPage.Count - 1; iFor++)
                    {
                        // For Each oPicItem In oPicNew.CurrentPage
                        Microsoft.OneDrive.Sdk.Item oPicItem = oPicNew.CurrentPage.ElementAt(iFor);
                        if (bFolders && oPicItem.Folder != null)
                            lNames.Add(oPicItem.Name);
                        if (bFiles && oPicItem.File != null)
                            lNames.Add(oPicItem.Name);
                    }
                    // oPicItem = null;

                    if (oPicNew.NextPageRequest == null)
                    {
                        oPicNew = null;
                        gInOneDriveCommand = false;
                        return lNames;
                    }
                    try
                    {
                        oPicNew = await oPicNew.NextPageRequest.GetAsync();
                    }
                    catch (Exception ex)
                    {
                        p.k.CrashMessageAdd("@OneDriveGetAllChilds - page " + iGuard, ex);
                        break;
                    }
                }
                oPicNew = null;
            }
            catch (Exception ex)
            {
                p.k.CrashMessageAdd("@OneDriveGetAllChilds", ex);
            }

            gInOneDriveCommand = false;
            return lNames;
        }

        public async static Task<Collection<Microsoft.OneDrive.Sdk.Item>> OneDriveGetAllChildsSDK(string sPathname, bool bFolders, bool bFiles)
        {
            try
            {
                Collection<Microsoft.OneDrive.Sdk.Item> oItems = new Collection<Microsoft.OneDrive.Sdk.Item>();

                if (gInOneDriveCommand)
                    return oItems;
                gInOneDriveCommand = true;

                Microsoft.OneDrive.Sdk.Item oPicLista = await RootOrAppRoot().ItemWithPath(sPathname).Request().Expand("children").GetAsync();
                for (int iInd = 0; iInd <= oPicLista.Children.CurrentPage.Count - 1; iInd++)
                {
                    // For Each oPicItem As Microsoft.OneDrive.Sdk.Item In oPicLista.Children.CurrentPage
                    Microsoft.OneDrive.Sdk.Item oPicItem = oPicLista.Children.CurrentPage.ElementAt(iInd);
                    if (bFolders && oPicItem.Folder != null)
                        oItems.Add(oPicItem);
                    if (bFiles && oPicItem.File != null)
                        oItems.Add(oPicItem);
                    oPicItem = null;
                }

                if (oPicLista.Children.NextPageRequest == null)
                {
                    gInOneDriveCommand = false;
                    return oItems;
                }

                Microsoft.OneDrive.Sdk.IItemChildrenCollectionPage oPicNew = await oPicLista.Children.NextPageRequest.GetAsync();
                oPicLista = null; // juz niepotrzebne

                for (int iGuard = 1; iGuard <= 12000 / (double)200; iGuard++)   // itemow moze byc, przez itemów na stronę
                {
                    Microsoft.OneDrive.Sdk.Item oPicItem;
                    for (int iFor = 0; iFor <= oPicNew.CurrentPage.Count - 1; iFor++)
                    {
                        // For Each oPicItem In oPicNew.CurrentPage
                        oPicItem = oPicNew.CurrentPage.ElementAt(iFor);
                        if (bFolders && oPicItem.Folder != null)
                            oItems.Add(oPicItem);
                        if (bFiles && oPicItem.File != null)
                            oItems.Add(oPicItem);
                        oPicItem = null;
                    }
                    oPicItem = null;
                    if (oPicNew.NextPageRequest == null)
                        return oItems;
                    oPicNew = await oPicNew.NextPageRequest.GetAsync();
                }
                oPicNew = null;

                gInOneDriveCommand = false;

                return oItems;
            }
            catch (Exception ex)
            {
                gInOneDriveCommand = false;
                p.k.CrashMessageExit("@OneDriveGetAllChildsSDK", ex.Message);
                return null;
            }

        }//OneDriveGetAllChildsSDK end


        /// <summary>
        /// Usuwa z podanego katalogu listę plików (filenames)
        /// Ret: -1 = error (nie ma OneDrive, lub InUse)
        /// 0: wszystkie usunął
        /// >0: ile plików się nie udało usunąć
        ///      </summary>
        public async static Task<int> UsunPlikiOneDrive(string sFolderPathname, List<string> lFilesToDel)
        {
            if (!IsOneDriveOpened())
                return -1;

            // If mbInUsunPlikiOneDrive Then Return   ' nie potrzebuje osobnego - nie wejdzie, bo jest w OneDrive w ogole
            if (gInOneDriveCommand)
                return -1;
            gInOneDriveCommand = true;

            // mbInUsunPlikiOneDrive = True

            int iCnt = 0;
            foreach (string sFileName in lFilesToDel)
            {
                // gdy nie ma sieci, przerwij - na wypadek jakby trwało Del, a zaczął robić fotkę i był error powodujący reset WiFi
                if (!p.k.NetIsIPavailable(false))
                    break;
                try
                { // usuwać mogę z jednego UWP, ale z drugiego też - i wtedy już nie ma plików  :)
                    await RootOrAppRoot().ItemWithPath(sFolderPathname + "/" + sFileName).Request().DeleteAsync();
                    iCnt += 1;
                }
                catch (Exception)
                {
                }

                p.k.ProgRingInc();
            }

            // mbInUsunPlikiOneDrive = False
            gInOneDriveCommand = false;

            return lFilesToDel.Count - iCnt;

        }//UsunPlikiOneDrive end

    }//class od end

}//namespace p end
