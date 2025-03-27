using System;
using System.Windows;
using System.Windows.Forms.Integration;
using Gecko;
using QR_Code_Tool.Metods;
using QR_Code_Tool.Provider;
using QR_Code_Tool.SDK;
using QR_Code_Tool.SDK.Utils;

namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для Loggin.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string CLIENT_ID = "e44807741c564ca9bcbf3cb14ef20be5";
        private const string RETURN_URL = "http://127.0.0.1:130/test";

        private readonly IYandexAPI sdkClient;
        private static string retUrl;
        private static EventHandler<GenericSdkEventArgs<string>> completeHandler;
       
        public LoginWindow()
        {
            InitializeComponent();
            Xpcom.Initialize("Firefox");
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public LoginWindow(IYandexAPI sdkClient) : this()
        {

            WindowsFormsHost host = new WindowsFormsHost();
            GeckoWebBrowser browser = new GeckoWebBrowser();
            host.Child = browser;
            GridWeb.Children.Add(host);     
            this.sdkClient = sdkClient;

            AuthorizeAsync(new WebBrowserWrapper(browser), CLIENT_ID, RETURN_URL, this.CompleteCallback);
        }

        private void CompleteCallback(object sender, GenericSdkEventArgs<string> e)
        {
            if (this.AuthCompleted != null)
            {
                this.AuthCompleted(this, new GenericSdkEventArgs<string>(e.Result));
            }

            this.Close();
        }

        public void AuthorizeAsync(IBrowser browser, string clientId, string returnUrl, EventHandler<GenericSdkEventArgs<string>> completeCallback)
        {             
            retUrl = returnUrl;
            completeHandler = completeCallback;
            var authUrl = string.Format(WebdavResources.AuthBrowserUrlFormat, clientId);
            browser.Navigating += BrowserOnNavigating;
            browser.Navigate(authUrl);
        }

        /// <summary>
        /// Occurs when browser is navigating to the url.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The event argument.</param>
        private void BrowserOnNavigating(object sender, GenericSdkEventArgs<string> e)
        {
            if (e.Result.Contains(retUrl))
            {
                var token = ResponseParser.ParseToken(e.Result);                                                               
                completeHandler.SafeInvoke(sender, new GenericSdkEventArgs<string>(token));
            }
        }

        public event EventHandler<GenericSdkEventArgs<string>> AuthCompleted;
    }
}