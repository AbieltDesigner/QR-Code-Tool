using System;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Input;
using Gecko;
using QR_Code_Tool.Provider;
using QR_Code_Tool.SDK;
using QR_Code_Tool.SDK.Utils;
using QR_Code_Tool.VievModels;

namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для Loggin.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static string retUrl;
        private GeckoWebBrowser browser;
        private static EventHandler<GenericSdkEventArgs<string>> completeHandler;
        private ICommand _clickClose;
        public ICommand ClickClose
        {
            get
            {
                return _clickClose ?? (_clickClose = new CommandHandler(
                () =>
                CloseWindow()));
            }
        }

        public LoginWindow()
        {
            InitializeComponent();
            Xpcom.Initialize("Firefox64");
            WindowsFormsHost host = new WindowsFormsHost();
            GeckoWebBrowser browser = new GeckoWebBrowser();
            host.Child = browser;
            this.browser = browser;
            GridWeb.Children.Add(host);        
            DataContext = this;
        }

        public LoginWindow(string clientID, string returnURL) : this()
        {
            AuthorizeAsync(new WebBrowserWrapper(browser), clientID, returnURL, this.CompleteCallback);
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

        private void CloseWindow()
        {
            this.Close();
        }

        public event EventHandler<GenericSdkEventArgs<string>> AuthCompleted;
    }
}