
namespace Andro2UWP
{
#if __ANDROID__
   using Android.App;
   using Android.Content;   
#endif
    using Microsoft.Graph;
    using Microsoft.OneDrive.Sdk;
    using Microsoft.OneDrive.Sdk.Authentication;
    using Models;
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using Windows.UI.Xaml;
    using Windows.UI.Xaml.Controls;
    
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class AccountSelectionPage : Page
    {
        private enum ClientType
        {
            Business,
            Consumer,
            ConsumerUwp
        }
        
        // Set these values to your app's ID and return URL.
        private readonly string oneDriveForBusinessClientId = "Insert your OneDrive for Business client id";
        private readonly string oneDriveForBusinessReturnUrl = "http://localhost:8080";
        private readonly string oneDriveForBusinessBaseUrl = "https://graph.microsoft.com/";

        private readonly string oneDriveConsumerClientId = "560b76b6-f929-4200-b8b0-70892f08f94a";//"Insert your OneDrive Consumer client id";
        private readonly string oneDriveConsumerReturnUrl = "msal560b76b6-f929-4200-b8b0-70892f08f94a://auth";//"https://login.live.com/oauth20_desktop.srf";
        private readonly string oneDriveConsumerBaseUrl = "https://api.onedrive.com/v1.0";

        private readonly string[] scopes = new string[] 
        {            
            "onedrive.readonly", // ok! //"onedrive.readwrite", //RnD
            "onedrive.appfolder", // ok
            "wl.signin", 
            "offline_access"
        };

        public AccountSelectionPage()
        {

            this.InitializeComponent();

            this.Loaded += AccountSelection_Loaded;
        }

        private async void AccountSelection_Loaded(object sender, RoutedEventArgs e)
        {
            
            App app = ((App) Windows.UI.Xaml.Application.Current);
            

            if (app.uOneDriveClient != null)
            {
                var msaAuthProvider = app.AuthProvider as MsaAuthenticationProvider;

                //var adalAuthProvider = app.AuthProvider as AdalAuthenticationProvider;
                if (msaAuthProvider != null)
                {
                    await msaAuthProvider.SignOutAsync();
                }
                //else if (adalAuthProvider != null)
                //{
                //    await adalAuthProvider.SignOutAsync();
                //}
                
                app.uOneDriveClient = null;
            }

            // Don't show AAD login if the required AAD auth values aren't set
            /*
            if (string.IsNullOrEmpty(this.oneDriveForBusinessClientId) 
                || 
                string.IsNullOrEmpty(this.oneDriveForBusinessReturnUrl))
            {
                this.AadButton.Visibility = Visibility.Collapsed;
            }
            */
        }

        private void AadButton_Click(object sender, RoutedEventArgs e)
        {
            // AAD Button -> Business client's type
            this.InitializeClient(ClientType.Business, e);
        }

        private void MsaButton_Click(object sender, RoutedEventArgs e)
        {
            // MSA Button -> Individual's type (MSA Auth...)
            this.InitializeClient(ClientType.Consumer, e);
        }

        private void OnlineId_Click(object sender, RoutedEventArgs e)
        {
            // OnlineId Button -> Special (Store dev?) client's type
            this.InitializeClient(ClientType.ConsumerUwp, e);
        }

        private async void InitializeClient(ClientType clientType, RoutedEventArgs e)
        {
            
            var app = (App) Windows.UI.Xaml.Application.Current;

            if (app.uOneDriveClient == null)
            {
                Task authTask;

                if (clientType == ClientType.Business)
                {
                    /*
                    var adalAuthProvider = new AdalAuthenticationProvider
                        (
                        this.oneDriveForBusinessClientId,
                        this.oneDriveForBusinessReturnUrl
                        );

                    authTask = adalAuthProvider.AuthenticateUserAsync(this.oneDriveForBusinessBaseUrl);

                    app.uOneDriveClient = new OneDriveClient
                        (
                            this.oneDriveForBusinessBaseUrl + "/_api/v2.0", 
                            adalAuthProvider
                        );

                    app.AuthProvider = adalAuthProvider; // !
                    */
                   
                }
                else if (clientType == ClientType.ConsumerUwp)
                {
                   /*
                    var onlineIdAuthProvider = new OnlineIdAuthenticationProvider
                    (
                        this.scopes
                    );

                    authTask = onlineIdAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();

                    app.uOneDriveClient = new OneDriveClient(this.oneDriveConsumerBaseUrl, onlineIdAuthProvider);
                    
                    app.AuthProvider = onlineIdAuthProvider; // !
                   */
                    
                }
                else
                {
                    // (clientType == ClientType.Consumer case) OK for simple MSA login :)
#if !__ANDROID__
                    var msaAuthProvider = new MsaAuthenticationProvider
                    (
                        this.oneDriveConsumerClientId,
                        this.oneDriveConsumerReturnUrl,
                        this.scopes,
                        new CredentialVault(this.oneDriveConsumerClientId)
                    );
#else
                    Android.Content.Context ctxt = Android.App.Application.Context;
                    
                    var msaAuthProvider = new MsaAuthenticationProvider
                    (
                        ctxt, // RnD
                        this.oneDriveConsumerClientId,
                        this.oneDriveConsumerReturnUrl,
                        this.scopes,
                        new CredentialVault(this.oneDriveConsumerClientId)
                    );
#endif



                    authTask = msaAuthProvider.RestoreMostRecentFromCacheOrAuthenticateUserAsync();

                    app.uOneDriveClient = new OneDriveClient(this.oneDriveConsumerBaseUrl, msaAuthProvider);

                    app.AuthProvider = msaAuthProvider; // !
                }

                try
                {
                    //await authTask;
                    
                    //app.NavigationStack.Add(new ItemModel(new Item()));
                    
                    this.Frame.Navigate(typeof(MainPage), e);
                }
                catch (Exception ex) //catch (ServiceException exception)
                {
                    // Swallow the auth exception but write message for debugging.
                    //Debug.WriteLine(exception.Error.Message);

                    //RnD
                    Console.WriteLine("AccountSelection - InitializeClient - exception: " + ex.Message);

                    // Plan B
                    this.Frame.Navigate(typeof(MainPage), e);
                }
            }
            //else
            //{
                // Go to Main page
                this.Frame.Navigate(typeof(MainPage), e);
            //}
            

            //temp
            //this.Frame.Navigate(typeof(MainPage), e);

        }//InitializeClient
    
    }//class end

}//namespace end
