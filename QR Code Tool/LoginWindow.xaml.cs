using Gecko;
using QR_Code_Tool.Provider;
using QR_Code_Tool.SDK;
using QR_Code_Tool.SDK.Utils;
using System;
using System.Windows;
using System.Windows.Forms.Integration;

namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для Loggin.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {        
        private string CLIENT_ID
        {
            get
            {
                return WebdavResources.ClientID;
            }
        }
        private string RETURN_URL
        {
            get
            {
                return WebdavResources.ReturnURL;
            }
        }
        
        private static string retUrl;
        private static EventHandler<GenericSdkEventArgs<string>> completeHandler;
   
                       
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        public LoginWindow()
        {
            InitializeComponent();
            Xpcom.Initialize("Firefox");
            WindowsFormsHost host = new WindowsFormsHost();
            GeckoWebBrowser browser = new GeckoWebBrowser();
            host.Child = browser;
            GridWeb.Children.Add(host);
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