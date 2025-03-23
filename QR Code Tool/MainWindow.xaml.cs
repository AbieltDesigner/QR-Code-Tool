using Gecko;
using QR_Code_Tool.SDK;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using YandexDisk.Client.Clients;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;
using QR_Code_Tool.API;


namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private IDiskSdkClient sdk;

        private LoginWindow loginWindow;
        //public static string AccessToken { get; set; } = "y0__xDvjZ2gqveAAhjXpDYggK3YzRI68bMOcTmM_EFbmYxkG9nBI5nVvw"; //Token chepurin@jg-group.ru
        public static string AccessToken { get; set; } = "y0__xDmxJNrGJ6nNiDvsPzOEtX65QtEgHbHV0OEN8ZqY33Ym2t8"; //Token vlad1988.1@yandex.ru (test token)

        private string currentPath, previousPath, homePath;
       
        public MainWindow()
        {
            InitializeComponent();   
            
            homePath = "Информация для заказчиков и объектов";

            var api = new Metods(AccessToken);
            
            _= api.PublishFolderOrFile(homePath + "/Проекты СТ 2024 год/009466(С) - СТ19 ТБ РЖД 2024 ЮВ-Красноярская", "/" + "Паспорт 00001_B24.pdf");

            _ = InitFolder(homePath);            
        }

        private async Task InitFolder(string path)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                this.currentPath = path;
                var api = new Metods(AccessToken);
                var resource = await api.GetListFilesToFolder(currentPath);
                gridItems.ItemsSource = resource.Embedded.Items;
            }
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            this.previousPath = this.currentPath;
            _ = this.InitFolder(homePath);
        }

        private void back_Click(object sender, RoutedEventArgs e)
        {
            var previous = this.previousPath;
            this.previousPath = this.currentPath;
            _ = this.InitFolder(previous);
        }

        private void goUp_Click(object sender, RoutedEventArgs e)
        {            
            var delimeterIndex = this.currentPath.Length > 1 ? this.currentPath.LastIndexOf("/", this.currentPath.Length - 2) : 0;
            if (delimeterIndex > 0)
            {
                var topPath = this.currentPath.Substring(0, delimeterIndex);
                this.previousPath = this.currentPath;
                _ = this.InitFolder(topPath);
            }       
        }

        private void refresh_Click(object sender, RoutedEventArgs e)
        {
            _ = this.InitFolder(this.currentPath);
        }

        private void login_Click(object sender, RoutedEventArgs e)
        {
            //AccessToken = string.Empty;
            //this.CreateSdkClient();
            //this.FolderItems = null;
            //this.CurrentPath = string.Empty;
            //this.OnPropertyChanged("IsLoggedIn");
            this.ShowLoginWindow();
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Resource rowDataDisk = (Resource)gridItems.SelectedItems[0];
            if (rowDataDisk.Type is ResourceType.Dir)
            {
                previousPath = currentPath;
                currentPath = currentPath + "/" + rowDataDisk.Name;
                _ = InitFolder(currentPath);                
            }                  
        }

        private void generateQR_Click(object sender, RoutedEventArgs e)
        {

        }




        private void ShowLoginWindow()
        {
           
        }

        private void ChangeVisibilityOfProgressBar(Visibility visibility, bool isIndeterminate = true)
        {
            this.Dispatcher.BeginInvoke(new Action(() =>
            {
                this.progressBar.Value = 0;
                this.progressBar.Visibility = visibility;
                this.progressBar.IsIndeterminate = isIndeterminate;
            }));
        }
    }
}
