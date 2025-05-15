/* Лицензионное соглашение на использование набора средств разработки
 * «SDK Яндекс.Диска» доступно по адресу: http://legal.yandex.ru/sdk_agreement
 */

using System.Windows.Navigation;
using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;


//using Gecko;
//using Gecko.Events;
using QR_Code_Tool_App.SDK;
using QR_Code_Tool_App.SDK.Utils;

namespace QR_Code_Tool_App.Proveder
{
    /// <summary>
    /// Represents wrapper for platform specific WebBrowser component.
    /// </summary>
    public class WebBrowserWrapper : IBrowser
    {
        private readonly WebView2 browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowserWrapper"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        public WebBrowserWrapper(WebView2 browser)
        {
            this.browser = browser;
            this.browser.NavigationCompleted += this.BrowserOnNavigating;
        }

        /// <summary>
        /// Navigates to the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Navigate(string url)
        {
            this.browser.Source = new Uri(url);
        }

        /// <summary>
        /// Occurs just before navigation to a document.
        /// </summary>
        public event EventHandler<GenericSdkEventArgs<string>> Navigating;

        /// <summary>
        /// Occurs when browser is navigating to the url.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="NavigatingCancelEventArgs"/> instance containing the event data.</param>
        private void BrowserOnNavigating(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            //if (e.IsSuccess)
            //{
            this.Navigating.SafeInvoke(this, new GenericSdkEventArgs<string>(browser.CoreWebView2.Source));
            //}
        }
    }
}