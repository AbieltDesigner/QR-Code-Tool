using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms.Integration;
using Gecko;
using QR_Code_Tool.SDK;
using QR_Code_Tool.Provider;
using Disk.SDK.Utils;
using QR_Code_Tool.SDK.Utils;

namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для Loggin.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private const string CLIENT_ID = "3d54c7ced73b498b9cd008ffdf3d43ff";
        private const string RETURN_URL = "http://127.0.0.1:130";

        private readonly IDiskSdkClient sdkClient;
        private static string retUrl;
        private static EventHandler<GenericSdkEventArgs<string>> completeHandler;

        public LoginWindow()
        {
            InitializeComponent();
            Gecko.Xpcom.Initialize("Firefox");         
        }

        public LoginWindow(IDiskSdkClient sdkClient)
         : this()
        {
            this.sdkClient = sdkClient;
            WindowsFormsHost host = new WindowsFormsHost();
            GeckoWebBrowser browser = new GeckoWebBrowser();
            host.Child = browser;
            GridWeb.Children.Add(host);
            AuthorizeAsync(new WebBrowserWrapper(browser), CLIENT_ID, RETURN_URL, this.CompleteCallback);
        }

        /// <summary>
        /// Authorizes the asynchronous.
        /// </summary>
        /// <param name="browser">The browser.</param>
        /// <param name="clientId">The client identifier.</param>
        /// <param name="returnUrl">The return URL.</param>
        /// <param name="completeCallback">The complete callback.</param>
        public void AuthorizeAsync(IBrowser browser, string clientId, string returnUrl, EventHandler<GenericSdkEventArgs<string>> completeCallback)
        {
            retUrl = returnUrl;
            completeHandler = completeCallback;
            var authUrl = string.Format(WebdavResources.AuthBrowserUrlFormat, clientId);
            browser.Navigating += BrowserOnNavigating;
            browser.Navigate(authUrl);
        }

        private static void BrowserOnNavigating(object sender, GenericSdkEventArgs<string> e)
        {
            if (e.Result.Contains(retUrl))
            {
                var token = ResponseParser.ParseToken(e.Result);
                completeHandler.SafeInvoke(sender, new GenericSdkEventArgs<string>(token));
            }
        }

        private void CompleteCallback(object sender, GenericSdkEventArgs<string> e)
        {
            if (this.AuthCompleted != null)
            {
                this.AuthCompleted(this, new GenericSdkEventArgs<string>(e.Result));
            }
            this.Close();
        }

        public event EventHandler<GenericSdkEventArgs<string>> AuthCompleted;

    }
}
