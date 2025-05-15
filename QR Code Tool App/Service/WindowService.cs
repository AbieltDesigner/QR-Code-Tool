using System.Windows;

namespace QR_Code_Tool_App.Service
{
    public class WindowService : IWindowService
    {
        private readonly Window _window;

        public WindowService(Window window)
        {
            _window = window;
        }

        public void Close()
        {
            _window.Close();
        }
    }
}
