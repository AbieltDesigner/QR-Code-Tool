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
using MessageBox = System.Windows.MessageBox;
using System.Threading;
using System.ComponentModel;
using System.Runtime.CompilerServices;


namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //private IDiskSdkClient sdk;
        //private LoginWindow loginWindow;
        //public static string AccessToken { get; set; } = "y0__xDvjZ2gqveAAhjXpDYggK3YzRI68bMOcTmM_EFbmYxkG9nBI5nVvw"; //Token chepurin@jg-group.ru
        public static string AccessToken { get; set; } = "y0__xDmxJNrGJ6nNiDvsPzOEtX65QtEgHbHV0OEN8ZqY33Ym2t8"; //Token vlad1988.1@yandex.ru (test token)
        private string currentPath, previousPath, homePath;
        public event PropertyChangedEventHandler PropertyChanged;    
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);


        public void OnPropertyChanged([CallerMemberName] string prop = "")
        {
            if (PropertyChanged != null)
                PropertyChanged(this, new PropertyChangedEventArgs(prop));            
            _ = this.InitFolder(this.currentPath);
        }

        public MainWindow()
        {
            InitializeComponent();               
            homePath = "Информация для заказчиков и объектов";         
            _ = InitFolder(homePath);            
        }

        private async Task InitFolder(string path)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                this.currentPath = path;
                var api = new YandexAPI(AccessToken);
                var resource = await api.GetListFilesToFolder(currentPath);
                gridItems.ItemsSource = resource.Embedded.Items;
            }
            LabelDir.Content = currentPath;
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

        private void publish_Click(object sender, RoutedEventArgs e)
        {
            _ = Publish(this.currentPath);           
        }

        private void unpublish_Click(object sender, RoutedEventArgs e)
        {
            _ = UnPublish(this.currentPath);            
        }

        private async Task Publish(string currentPath)
        {
            try
            {
                var api = new YandexAPI(AccessToken);
                var collectionRows = gridItems.SelectedItems;

                for (int i = 0; i < collectionRows.Count; i++)
                {
                    Resource rowDataDisk = (Resource)collectionRows[i];
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl == null)
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            await api.PublishFolderOrFile(currentPath + "/" + rowDataDisk.Name);
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    }                   
                }                   
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }
            finally
            {
                _= this.InitFolder(this.currentPath);
            }
        }
               
        private async Task UnPublish(string currentPath)
        {
            try
            {
                var api = new YandexAPI(AccessToken);
                var collectionRows = gridItems.SelectedItems;

                for (int i = 0; i < collectionRows.Count; i++)
                {
                    Resource rowDataDisk = (Resource)collectionRows[i];
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl != null)
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            await api.UnPublishFolderOrFile(currentPath + "/" + rowDataDisk.Name);
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    }
                }
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }
            finally
            {
                _ = this.InitFolder(this.currentPath);
            }
        }


        private void copiLink_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Resource rowDataDisk = (Resource)gridItems.SelectedItems[0];
                if (rowDataDisk.PublicUrl == null)
                {
                    MessageBox.Show("Публичная ссылка не сформирована, ее необходимо сначала сформировать", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);  
                }
                else
                {
                    System.Windows.Clipboard.SetText(rowDataDisk.PublicUrl);
                }                                  
            }

            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }

            catch (ArgumentNullException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }
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
            //Resource rowDataDisk = (Resource)gridItems.SelectedItems[0];
            //if (rowDataDisk.Type is ResourceType.Dir)
            //{
            //    previousPath = currentPath;
            //    currentPath = currentPath + "/" + rowDataDisk.Name;
            //    _ = InitFolder(currentPath);                
            //}                  
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            //DataGridRow row = sender as DataGridRow;
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
