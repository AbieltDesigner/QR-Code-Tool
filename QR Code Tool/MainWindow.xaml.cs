using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using QR_Code_Tool.API;
using QR_Code_Tool.Metods;
using QR_Code_Tool.SDK;
using QR_Code_Tool.Serializable;
using QR_Code_Tool.Serializable.Entity;
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
        private ObservableCollection<Resource> folderItems;      
        private string currentPath, previousPath;
        private readonly string homePath;
        private readonly ICollection<Resource> selectedItems = new Collection<Resource>();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private readonly string jsonFilePath;
        private readonly AppSettings appSettings;
        public static string AccessToken { get; set; }
        private readonly string Client_ID;
        private readonly string Return_URL;

        public MainWindow()
        {
            InitializeComponent();
            this.jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/appSettings.json");
            AppSettingsDeserialize app = new AppSettingsDeserialize(jsonFilePath);
            this.appSettings = app.GetSettingsModels();
            this.Client_ID = appSettings.ClientSettings.clientId;
            this.Return_URL = @"http://" + appSettings.ClientSettings.returnUrl;
            this.homePath = appSettings.FolderSettings.HomeFolder;
            this.DataContext = this;
            this.ShowLoginWindow(Client_ID, Return_URL);
            _ = InitFolder(homePath);
        }

        private async Task InitFolder(string path)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {               
                this.currentPath = path;
                this.yandexClient = new YandexAPI(AccessToken);
                var resource = await this.yandexClient.GetListFilesToFolderAsync(currentPath);                                
                _ = this.Dispatcher.BeginInvoke(new Action(() => { this.FolderItems = new ObservableCollection<Resource>(resource.Embedded.Items);}));
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                this.OnPropertyChanged("FolderPath");
            }                     
        }

        private void home_Click(object sender, RoutedEventArgs e)
        {
            this.previousPath = this.currentPath;
            _ = this.InitFolder(homePath);
        }

        private void goUp_Click(object sender, RoutedEventArgs e)
        {
            var previous = this.previousPath;
            this.previousPath = this.currentPath;
            _ = this.InitFolder(previous);
        }

        private void back_Click(object sender, RoutedEventArgs e)
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
            _ = DeleteFile(this.currentPath);
        }
        private void upLoadFile_Click(object sender, RoutedEventArgs e)
        {
            _ = UpLoadFile();
        }

        private async Task UpLoadFile()
        {
            var openDialog = new OpenFileDialog();
            if (openDialog.ShowDialog() == true)
            {
                this.ShowProgress(false);
                var stream = openDialog.OpenFile();
                var fileName = Path.GetFileName(openDialog.FileName);
                var filePath = this.currentPath + "/" + fileName;
                await this.yandexClient.UpLoadFileAsync(filePath, stream);
                //this.sdk.UploadFileAsync(filePath, stream, new AsyncProgress(this.UpdateProgress), this.SdkOnUploadCompleted);
            }
            else
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
            }
            _ = this.InitFolder(this.currentPath);
        }


        private async Task Publish(string currentPath)
        {           
            try
            {                
                var collectionRows = selectedItems.AsEnumerable<Resource>();
                this.ShowProgress(false);
                foreach (var rowDataDisk in collectionRows)
                {
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl == null)
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {                           
                            await this.yandexClient.PublishFolderOrFileAsync(currentPath + "/" + rowDataDisk.Name);
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
                var collectionRows = selectedItems.AsEnumerable<Resource>();
                this.ShowProgress(false);
                foreach (var rowDataDisk in collectionRows)
                {                   
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl != null)
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            await this.yandexClient.UnPublishFolderOrFileAsync(currentPath + "/" + rowDataDisk.Name);
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

        private async Task DeleteFile(string currentPath)
        {
            try
            {
                var collectionRows = selectedItems.AsEnumerable<Resource>();
                this.ShowProgress(false);
                foreach (var rowDataDisk in collectionRows)
                {
                    if (rowDataDisk.Name != null)
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            await this.yandexClient.DeleteFileAsync(currentPath + "/" + rowDataDisk.Name);
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
                var rowDataDisk = selectedItems.FirstOrDefault();
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
            this.FolderItems = null;
            this.currentPath = string.Empty;
            this.OnPropertyChanged("IsLoggedIn");
            this.ShowLoginWindow(Client_ID, Return_URL);
        }

        private void logOut_Click(object sender, RoutedEventArgs e)
        {                     
            this.ShowLoginWindow(Client_ID, Return_URL, true);
            this.currentPath = string.Empty;
            AccessToken = string.Empty;
            this.OnPropertyChanged("IsLoggedIn");
            this.OnPropertyChanged("IsLoggedOut");
        }

        private void gridItems_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var items = e.AddedItems.Cast<Resource>();
                foreach (var item in items)
                {
                    this.selectedItems.Add(item);
                }
            }

            if (e.RemovedItems.Count > 0)
            {
                var items = e.RemovedItems.Cast<Resource>();
                foreach (var item in items)
                {
                    this.selectedItems.Remove(item);
                }
            }
            //this.NotifyMenuItems();
        }

        private void Row_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var rowDataDisk = selectedItems.FirstOrDefault();
            if (rowDataDisk.Type is ResourceType.Dir)
            {
                previousPath = currentPath;
                currentPath = currentPath + "/" + rowDataDisk.Name;
                _ = InitFolder(currentPath);
            }
        }

        private void printQR_Click(object sender, RoutedEventArgs e)
        {
            var printZPL = new PrintZPL(selectedItems.AsEnumerable(), appSettings.PrintSettings);
            printZPL.Print();   
        }

        private void ShowLoginWindow(string client_ID, string return_URL, bool isLogout = false)
        {
            this.loginWindow = new LoginWindow(client_ID, return_URL);
            this.loginWindow.AuthCompleted += this.OnAuthorizeCompleted;
            if (isLogout)
                this.loginWindow.ClearAll();
            this.loginWindow.ShowDialog();
        }
        private void ShowProgress(bool isIndeterminate = true)
        {
            this.progressBar.Visibility = Visibility.Visible;
            //this.progressBar.IsIndeterminate = isIndeterminate;
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

        public ObservableCollection<Resource> FolderItems
        {
            get { return this.folderItems; }
            set
            {
                this.folderItems = value;
                this.OnPropertyChanged("FolderItems");
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
