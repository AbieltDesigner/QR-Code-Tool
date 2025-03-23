/* Лицензионное соглашение на использование набора средств разработки
 * «SDK Яндекс.Диска» доступно по адресу: http://legal.yandex.ru/sdk_agreement
 */

using System;
using System.Runtime.InteropServices;
using System.Windows.Controls;
using System.Windows.Navigation;
using Gecko;

using Disk.SDK.Utils;
using Gecko.Events;
using System.Diagnostics;
using QR_Code_Tool.SDK;
using QR_Code_Tool.SDK.Utils;

namespace QR_Code_Tool.Provider
{
    /// <summary>
    /// Represents wrapper for platform specific WebBrowser component.
    /// </summary>
    public class WebBrowserWrapper : IBrowser
    {
        private readonly GeckoWebBrowser browser;

        /// <summary>
        /// Initializes a new instance of the <see cref="WebBrowserWrapper"/> class.
        /// </summary>
        /// <param name="browser">The browser.</param>
        public WebBrowserWrapper(GeckoWebBrowser browser)
        {
            this.browser = browser;
            this.browser.Navigating += this.BrowserOnNavigating;
        }

        /// <summary>
        /// Navigates to the specified URL.
        /// </summary>
        /// <param name="url">The URL.</param>
        public void Navigate(string url)
        {
            this.browser.Navigate(url);
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
        private void BrowserOnNavigating(object sender, GeckoNavigatingEventArgs e)
        {
            this.Navigating.SafeInvoke(this, new GenericSdkEventArgs<string>(e.Uri.ToString()));
        }
    }
}