using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using QR_Code_Tool.API;
using QR_Code_Tool.Metods;
using QR_Code_Tool.SDK;
using YandexDisk.Client.Protocol;
using MessageBox = System.Windows.MessageBox;


namespace QR_Code_Tool
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : INotifyPropertyChanged
    {
        private IYandexAPI yandexClient;
        private LoginWindow loginWindow;        
        public static string AccessToken { get; set; }
        private string currentPath, previousPath, homePath;     
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);       

        public MainWindow()
        {
            InitializeComponent();               
            homePath = "Информация для заказчиков и объектов";
            DataContext = this;
            _ = InitFolder(homePath);            
        }

        private async Task InitFolder(string path)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {               
                this.currentPath = path;
                var api = new YandexAPI(AccessToken);
                var resource = await api.GetListFilesToFolder(currentPath);
                gridItems.ItemsSource = resource.Embedded.Items;
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                this.OnPropertyChanged("FolderPath");
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

        private void publish_Click(object sender, RoutedEventArgs e)
        {
            _ = Publish(this.currentPath);           
        }

        private void unpublish_Click(object sender, RoutedEventArgs e)
        {
            _ = UnPublish(this.currentPath);            
        }
        private void deleteFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Пока не реализовано");
        }
        private void upLoadFile_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Пока не реализовано");
        }

        private async Task Publish(string currentPath)
        {           
            try
            {
                var api = new YandexAPI(AccessToken);
                var collectionRows = gridItems.SelectedItems;
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                    MessageBox.Show("Ссылка скопирована в буфер обмена.", "Удачно", MessageBoxButton.OK, MessageBoxImage.Information);
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
            AccessToken = string.Empty;
            this.currentPath = string.Empty;
            this.OnPropertyChanged("IsLoggedIn");
            this.ShowLoginWindow();
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
                      
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

        private void printQR_Click(object sender, RoutedEventArgs e)
        {
            System.Windows.Forms.MessageBox.Show("Пока не реализовано");
        }

        private void ShowLoginWindow()
        {
            this.loginWindow = new LoginWindow(this.yandexClient);
            this.loginWindow.AuthCompleted += this.OnAuthorizeCompleted;
            this.loginWindow.ShowDialog();
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

        private void OnAuthorizeCompleted(object sender, GenericSdkEventArgs<string> e)
        {
            if (e.Error == null)
            {
                AccessToken = e.Result;               
                this.Dispatcher.BeginInvoke(new Action(() =>
                {
                    this.OnPropertyChanged("IsLoggedIn");
                    this.OnPropertyChanged("IsLoggedOut");
                    _ = this.InitFolder(homePath);
                }));
            }
            else
            {
                this.ProcessError(e.Error);
            }
        }

        public bool IsLoggedIn
        {
            get { return !string.IsNullOrEmpty(AccessToken); }
        }

        public bool IsLoggedOut
        {
            get
            {
                return !IsLoggedIn;
            }
        }

        public string FolderPath
        {
            get
            {
                if (!string.IsNullOrEmpty(currentPath))
                {
                    return this.currentPath;
                }
                return string.Empty;
            }
        }

        private void ProcessError(SdkException ex)
        {
            Dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        public event PropertyChangedEventHandler PropertyChanged;
                
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}
