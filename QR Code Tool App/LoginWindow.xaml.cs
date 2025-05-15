using System.Windows;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using QR_Code_Tool_App.Proveder;
using QR_Code_Tool_App.SDK;
using QR_Code_Tool_App.SDK.Utils;
using QR_Code_Tool_App.VievModels;

namespace QR_Code_Tool_App
{
    /// <summary>
    /// Логика взаимодействия для LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        private static string? retUrl;
        private readonly WebView2 browser;
        private static EventHandler<GenericSdkEventArgs<string>>? completeHandler;

        private ICommand? _clickClose;
        public ICommand ClickClose
        {
            get
            {
                return _clickClose ??= new CommandHandler(
                () =>
                CloseWindow());
            }
        }

        public LoginWindow()
        {
            InitializeComponent();
            InitializeWebView2Async();
            DataContext = this;
            browser = webView;
        }

       
        public LoginWindow(string clientID, string returnURL) : this()
        {
            AuthorizeAsync(new WebBrowserWrapper(browser), clientID, returnURL, this.CompleteCallback);
        }

        private void CompleteCallback(object? sender, GenericSdkEventArgs<string> e)
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

        private void BrowserOnNavigating(object? sender, GenericSdkEventArgs<string> e)
        {
            if (e.Result.Contains(retUrl!))
            {
                var token = ResponseParser.ParseToken(e.Result);
                completeHandler?.SafeInvoke(sender!, new GenericSdkEventArgs<string>(token));
            }
        }

        private async void InitializeWebView2Async()
        {
            try
            {
                // Указываем папку для данных (если нужно)
                var env = await CoreWebView2Environment.CreateAsync(
                    userDataFolder: @"C:\Temp\WebView2_Data");

                // Инициализация среды
                await webView.EnsureCoreWebView2Async(env);
                // Настройка параметров
                webView.CoreWebView2.Settings.IsZoomControlEnabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка инициализации WebView2: {ex.Message}");
            }
        }

        internal async void DeleteAllCookies()
        {
            if (webView.CoreWebView2 == null)
            {
                await webView.EnsureCoreWebView2Async();
            }
            var cookieManager = webView.CoreWebView2?.CookieManager;
            cookieManager?.DeleteAllCookies();
        }


        private void CloseWindow()
        {
            this.Close();
        }

        public event EventHandler<GenericSdkEventArgs<string>>? AuthCompleted;

    }
}
