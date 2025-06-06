using System.Windows;
using QR_Code_Tool_App.Service;
using QR_Code_Tool_App.VievModels;

namespace QR_Code_Tool_App
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {       
        private readonly MainViewModel mainViewModel;
        public MainWindow()
        {
            InitializeComponent();
            var service = new WindowService(this);
            mainViewModel = new MainViewModel(Dispatcher, service);
            DataContext = mainViewModel;
        }             
    }
}