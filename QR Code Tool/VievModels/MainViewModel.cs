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
using System.Windows.Threading;
using Gecko;
using Microsoft.Win32;
using QR_Code_Tool.API;
using QR_Code_Tool.Metods;
using QR_Code_Tool.SDK;
using QR_Code_Tool.Serializable;
using QR_Code_Tool.Serializable.Entity;
using YandexDisk.Client.Protocol;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace QR_Code_Tool.VievModels
{

    public class MainViewModel :INotifyPropertyChanged, IMainViewModel
    {
        private IYandexAPI yandexClient;
        private LoginWindow loginWindow;
        private ObservableCollection<Resource> folderItems;
        private Visibility isProressVisibility; 
        private string currentPath, previousPath;
        private readonly string homePath;
        private readonly ICollection<Resource> selectedItems = new Collection<Resource>();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1);
        private readonly string jsonFilePath;
        private readonly AppSettingsDeserialize appSettingsDeserialize;
        private readonly AppSettings appSettings;
        public static string AccessToken { get; set; }
        private readonly string Client_ID;
        private readonly string Return_URL;
        private Dispatcher dispatcher;
        private ICommand _clickBack;
        private ICommand _clickGoUp;
        private ICommand _clickRefresh;
        private ICommand _clickHome;
        private ICommand _clickPrintQR;
        private ICommand _clickPublish;
        private ICommand _clickUnPublish;
        private ICommand _clickCopiLink;
        private ICommand _clickUpLoadFile;
        private ICommand _clickDeleteFile;
        private ICommand _clickLogOut;
        private ICommand _clickLogIn;

        public ICommand ClickBack
        {
            get { return _clickBack ?? (_clickBack = new CommandHandler(() => Back())); }
        }
        public ICommand ClickGoUp
        {
            get { return _clickGoUp ?? (_clickGoUp = new CommandHandler(() => Up())); }
        }
        public ICommand ClickRefresh
        {
            get { return _clickRefresh ?? (_clickRefresh = new CommandHandler(() => Refresh())); }
        }
        public ICommand ClickHome
        {
            get { return _clickHome ?? (_clickHome = new CommandHandler(() => Home())); }
        }
        public ICommand ClickPrintQR
        {
            get { return _clickPrintQR ?? (_clickPrintQR = new CommandHandler(() => PrintQR())); }
        }
        public ICommand ClickPublish
        {
            get { return _clickPublish ?? (_clickPublish = new CommandHandler(() => GoPublish())); }
        }
        public ICommand ClickUnPublish
        {
            get { return _clickUnPublish ?? (_clickUnPublish = new CommandHandler(() => GoUnPublish())); }
        }
        public ICommand ClickCopiLink
        {
            get { return _clickCopiLink ?? (_clickCopiLink = new CommandHandler(() => CopiLink())); }
        }
        public ICommand ClickUpLoadFile
        {
            get { return _clickUpLoadFile ?? (_clickUpLoadFile = new CommandHandler(() => GoUpLoadFile())); }
        }
        public ICommand ClickDeleteFile
        {
            get { return _clickDeleteFile ?? (_clickDeleteFile = new CommandHandler(() => GoDeleteFile())); }
        }
        public ICommand ClickLogOut
        {
            get { return _clickLogOut ?? (_clickLogOut = new CommandHandler(() => LogOut())); }
        }
        public ICommand ClickLogIn
        {
            get { return _clickLogIn ?? (_clickLogIn = new CommandHandler(() => LogIn())); }
        }

        public MainViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/appSettings.json");
            this.appSettingsDeserialize = new AppSettingsDeserialize(jsonFilePath);
            this.appSettings = appSettingsDeserialize.GetSettingsModels();
            this.Client_ID = appSettings.ClientSettings.clientId;
            this.Return_URL = @"http://" + appSettings.ClientSettings.returnUrl;
            this.homePath = appSettings.FolderSettings.HomeFolder;
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
                _ = this.dispatcher.BeginInvoke(new Action(() => { this.FolderItems = new ObservableCollection<Resource>(resource.Embedded.Items); }));              
                this.OnPropertyChanged("FolderPath");
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
            }
        }    
        
        private void Back()
        {
            var delimeterIndex = this.currentPath.Length > 1 ? this.currentPath.LastIndexOf("/", this.currentPath.Length - 2) : 0;
            if (delimeterIndex > 0)
            {
                var topPath = this.currentPath.Substring(0, delimeterIndex);
                this.previousPath = this.currentPath;
                _ = this.InitFolder(topPath);
            }
        }
        private void Up()
        {
            var previous = this.previousPath;
            this.previousPath = this.currentPath;
            _ = this.InitFolder(previous);
        }
        private void Home()
        {
            this.previousPath = this.currentPath;
            _ = this.InitFolder(homePath);
        }
        private void Refresh()
        {
            _ = this.InitFolder(this.currentPath);
        }
        private void PrintQR()
        {
            var printZPL = new PrintZPL(selectedItems.AsEnumerable(), appSettings.PrintSettings);
            printZPL.Print();
        }
        private void GoPublish()
        {
            _ = Publish(this.currentPath);
        }
        private void GoUnPublish()
        {
            _ = UnPublish(this.currentPath);
        }
        private void CopiLink()
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
        private void GoUpLoadFile()
        {
            _ = UpLoadFile();
        }
        private void GoDeleteFile()
        {
            _ = DeleteFile(this.currentPath);
        }
        private void LogOut()
        {
            nsICookieManager CookieMan;
            CookieMan = Xpcom.GetService<nsICookieManager>("@mozilla.org/cookiemanager;1");
            CookieMan = Xpcom.QueryInterface<nsICookieManager>(CookieMan);
            CookieMan.RemoveAll();

            this.FolderItems = null;
            this.currentPath = string.Empty;
            AccessToken = string.Empty;           

            this.currentPath = string.Empty;
            AccessToken = string.Empty;
            this.OnPropertyChanged("IsLoggedIn");
            this.OnPropertyChanged("IsLoggedOut");
        }
        private void LogIn()
        {
            AccessToken = string.Empty;
            this.FolderItems = null;
            this.currentPath = string.Empty;
            this.OnPropertyChanged("IsLoggedIn");
            this.ShowLoginWindow(Client_ID, Return_URL);
        }

        private async Task Publish(string currentPath)
        {
            try
            {
                var collectionRows = selectedItems.AsEnumerable<Resource>();         
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                _ = this.InitFolder(this.currentPath);
            }
        }
        private async Task UnPublish(string currentPath)
        {
            try
            {
                var collectionRows = selectedItems.AsEnumerable<Resource>();
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
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
        private async Task UpLoadFile()
        {
            throw new NotImplementedException();

            var openDialog = new OpenFileDialog();
            openDialog.Multiselect = true;


            if (openDialog.ShowDialog() == true)
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                foreach (string file in openDialog.FileNames)
                {
                    var stream = openDialog.OpenFile();
                    var fileName = Path.GetFileName(openDialog.FileName);
                    var filePath = this.currentPath + "/" + fileName;
                    await this.yandexClient.UpLoadFileAsync(filePath, stream);
                }               
            }
            else
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
            }
            _ = this.InitFolder(this.currentPath);
        }              
        private async Task DeleteFile(string currentPath)
        {
            try
            {
                var collectionRows = selectedItems.AsEnumerable<Resource>();             
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
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

        public void GridItems_SelectionChanged(SelectionChangedEventArgs e)
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
        }

        public void Row_DoubleClick()
        {
            var rowDataDisk = selectedItems.FirstOrDefault();
            if (rowDataDisk.Type is ResourceType.Dir)
            {
                previousPath = currentPath;
                currentPath = currentPath + "/" + rowDataDisk.Name;
                _ = InitFolder(currentPath);
            }
        }

        private void ShowLoginWindow(string client_ID, string return_URL)
        {    
            this.loginWindow = new LoginWindow(client_ID, return_URL);
            this.loginWindow.AuthCompleted += this.OnAuthorizeCompleted;           
            this.loginWindow.ShowDialog();
        }
        
        private void ChangeVisibilityOfProgressBar(Visibility visibility, bool isIndeterminate = true)
        {
            this.dispatcher.BeginInvoke(new Action(() => { this.IsProressVisibility = visibility; }));
        }

        private void OnAuthorizeCompleted(object sender, GenericSdkEventArgs<string> e)
        {
            if (e.Error == null)
            {
                AccessToken = e.Result;
                this.dispatcher.BeginInvoke(new Action(() =>
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

        public Visibility IsProressVisibility
        {
            get
            {
                return this.isProressVisibility;
            }
            set 
            {                        
                this.isProressVisibility = value;
                this.OnPropertyChanged("IsProressVisibility");
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
            dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = this.PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}

