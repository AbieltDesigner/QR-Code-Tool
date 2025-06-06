using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QR_Code_Tool.Service;
using QR_Code_Tool.VievModels;
using YandexDisk.Client.Protocol;


namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly ICollection<Resource> selectedItems = new Collection<Resource>();
        private readonly IMainViewModel mainViewModel;

        public MainWindow()
        {
            InitializeComponent();
            var service = new WindowService(this);
            mainViewModel = new MainViewModel(Dispatcher, service);
            DataContext = mainViewModel;
            mainViewModel.RequestClose += (s, e) => this.Close();
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            mainViewModel.GridItems_SelectionChanged(e);
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            mainViewModel.Row_DoubleClick();
        }
    }
}
