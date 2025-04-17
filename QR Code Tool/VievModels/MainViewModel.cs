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
using Microsoft.WindowsAPICodePack.Dialogs;
using QR_Code_Tool.API;
using QR_Code_Tool.Metods;
using QR_Code_Tool.SDK;
using QR_Code_Tool.Serializable;
using QR_Code_Tool.Serializable.Entity;
using QR_Code_Tool.UserVariable;
using YandexDisk.Client;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.VievModels
{
    public class MainViewModel : INotifyPropertyChanged, IMainViewModel
    {
        private IYandexAPI yandexClient;
        private LoginWindow loginWindow;
        private ObservableCollection<Resource> folderItems;
        private Visibility isProressVisibility;
        private string currentPath, previousPath;
        private readonly string homePath;
        private readonly ICollection<Resource> selectedItems = new Collection<Resource>();
        private readonly SemaphoreSlim semaphoreSlim = new SemaphoreSlim(1, 3);
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
        private ICommand _clickUpLoadFolder;
        private ICommand _clickDeleteFile;
        private ICommand _clickLogOut;
        private ICommand _clickLogIn;
        private ICommand _clickClose;

        public ICommand ClickBack
        {
            get { return _clickBack ?? (_clickBack = new CommandHandler(
                async() =>
                await BackAsync())); }
        }
        public ICommand ClickGoUp
        {
            get { return _clickGoUp ?? (_clickGoUp = new CommandHandler(
                async () =>
                await UpAsync())); }
        }
        public ICommand ClickRefresh
        {
            get { return _clickRefresh ?? (_clickRefresh = new CommandHandler(
                async () =>
                await RefreshAsync())); }
        }
        public ICommand ClickHome
        {
            get { return _clickHome ?? (_clickHome = new CommandHandler(
                async () =>
                await HomeAsync())); }
        }
        public ICommand ClickPrintQR
        {
            get { return _clickPrintQR ?? (_clickPrintQR = new CommandHandler(
                () => 
                PrintQR())); }
        }
        public ICommand ClickPublish
        {
            get { return _clickPublish ?? (_clickPublish = new CommandHandler(
                async () =>
                await PublishAsync(selectedItems.AsEnumerable<Resource>()))); }
        }
        public ICommand ClickUnPublish
        {
            get { return _clickUnPublish ?? (_clickUnPublish = new CommandHandler(
                async () =>
                await UnPublishAsync(selectedItems.AsEnumerable<Resource>()))); }
        }
        public ICommand ClickCopiLink
        {
            get { return _clickCopiLink ?? (_clickCopiLink = new CommandHandler(
                () =>
                CopiLink())); }
        }
        public ICommand ClickUpLoadFile
        {
            get { return _clickUpLoadFile ?? (_clickUpLoadFile = new CommandHandler(
                async () =>
                await UploadFileAsync())); }
        }
        public ICommand ClickUpLoadFolder
        {
            get { return _clickUpLoadFolder ?? (_clickUpLoadFolder = new CommandHandler(
                async () =>
                await UploadFolderAsync())); }
        }
        public ICommand ClickDeleteFile
        {
            get { return _clickDeleteFile ?? (_clickDeleteFile = new CommandHandler(
                async () =>
                await DeleteFileAsync())); }
        }
        public ICommand ClickLogOut
        {
            get { return _clickLogOut ?? (_clickLogOut = new CommandHandler(
                () =>
                LogOut())); }
        }
        public ICommand ClickLogIn
        {
            get { return _clickLogIn ?? (_clickLogIn = new CommandHandler(
                () =>
                LogIn())); }
        }
        public ICommand ClickClose
        {
            get
            {
                return _clickClose ?? (_clickClose = new CommandHandler(
                () =>
                CloseApp()));
            }
        }

        public MainViewModel(Dispatcher dispatcher)
        {
            this.dispatcher = dispatcher;
            this.jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/appSettings.json");
            this.appSettingsDeserialize = new AppSettingsDeserialize(jsonFilePath);
            this.appSettings = appSettingsDeserialize.GetSettingsModels();
            this.Client_ID = appSettings.ClientSettings.clientId;
            this.Return_URL = string.Concat(@"http://", appSettings.ClientSettings.returnUrl);
            this.homePath = appSettings.FolderSettings.HomeFolder;
            this.ShowLoginWindow(Client_ID, Return_URL);
            _ = InitFolderAsync(homePath);
        }

        private async Task InitFolderAsync(string path)
        {
            if (!string.IsNullOrEmpty(AccessToken))
            {
                this.currentPath = path;
                this.yandexClient = new YandexAPI(AccessToken);
                var resource = await this.yandexClient.GetListFilesToFolderAsync(currentPath);
                await this.dispatcher.BeginInvoke(new Action(() => { this.FolderItems = new ObservableCollection<Resource>(resource.Embedded.Items); }));
                this.OnPropertyChanged("FolderPath");
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
            }
        }

        private async Task BackAsync()
        {       
            var delimeterIndex = this.currentPath.Length > 1 ? this.currentPath.LastIndexOf("/", this.currentPath.Length - 2) : 0;
            if (delimeterIndex > 0)
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                var topPath = this.currentPath.Substring(0, delimeterIndex + 1);
                this.previousPath = this.currentPath;
                await this.InitFolderAsync(topPath);
            }
        }
        private async Task UpAsync()
        {
            this.ChangeVisibilityOfProgressBar(Visibility.Visible);
            var previous = this.previousPath;
            this.previousPath = this.currentPath;
            await this.InitFolderAsync(previous);
        }
        private async Task HomeAsync()
        {
            this.ChangeVisibilityOfProgressBar(Visibility.Visible);
            this.previousPath = this.currentPath;
            await this.InitFolderAsync(homePath);
        }
        private async Task RefreshAsync()
        {
            this.ChangeVisibilityOfProgressBar(Visibility.Visible);
            await this.InitFolderAsync(this.currentPath);
        }
        private void PrintQR()
        {
            var printZPL = new PrintZPL(selectedItems.AsEnumerable(), appSettings.PrintSettings);
            printZPL.Print();
        }

        private async Task PublishAsync(IEnumerable<Resource> collectionRows)
        {
            var currentPath = this.currentPath;
            try
            {
                var localCollectionRows = collectionRows.ToList<Resource>();
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                var tasks = new List<Task>();
                             
                foreach (var rowDataDisk in localCollectionRows)
                {
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl == null)
                    {
                        await semaphoreSlim.WaitAsync();

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await this.yandexClient.PublishFolderOrFileAsync(string.Concat(currentPath, rowDataDisk.Name));
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }
                        }));
                    }                                                                                  
                }
                await Task.WhenAll(tasks);              
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }
            finally
            {
                await this.InitFolderAsync(this.currentPath);
            }
        }
        private async Task UnPublishAsync(IEnumerable<Resource> collectionRows)
        {
            var currentPath = this.currentPath;
            try
            {
                var localCollectionRows = collectionRows.ToList<Resource>();                
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                var tasks = new List<Task>();

                foreach (var rowDataDisk in localCollectionRows)
                {
                    if (rowDataDisk.Name != null && rowDataDisk.PublicUrl != null)
                    {
                        await semaphoreSlim.WaitAsync();

                        tasks.Add(Task.Run(async () =>
                        {
                            try
                            {
                                await this.yandexClient.UnPublishFolderOrFileAsync(string.Concat(currentPath, rowDataDisk.Name));
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }
                        }));
                    }                    
                }
                await Task.WhenAll(tasks);
            }
            catch (ArgumentOutOfRangeException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
            }
            finally
            {
                await this.InitFolderAsync(this.currentPath);
            }
        }

        private void CopiLink()
        {
            try
            {
                var rowDataDisk = selectedItems.FirstOrDefault();
                if (rowDataDisk == null)
                    return;
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

            catch (NullReferenceException ex)
            {
                Debug.WriteLine($"Произошло исключение {ex.Message}");
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

        //Review OK
        private async Task UploadFileAsync()
        {
            var openDialog = new OpenFileDialog
            {
                Multiselect = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openDialog.ShowDialog() != true)
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                return;
            }

            this.ChangeVisibilityOfProgressBar(Visibility.Visible);
            var fileNames = openDialog.FileNames;
            var tasks = new List<Task>();

            try
            {
                foreach (var fileName in fileNames)
                {
                    tasks.Add(Task.Run(async () =>
                    {
                        await semaphoreSlim.WaitAsync();
                        try
                        {
                            using (var fileStream = new FileStream(
                                fileName,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read,
                                bufferSize: 4096,
                                useAsync: true))
                            {
                                var targetPath = Path.Combine(this.currentPath, Path.GetFileName(fileName))
                                    .Replace('\\', '/');

                                await this.yandexClient.UpLoadFileAsync(targetPath, fileStream)
                                    .ConfigureAwait(false);
                            }
                        }
                        catch (YandexApiException ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Ошибка загрузки файла {Path.GetFileName(fileName)}: {ex.Message}",
                                    "Ошибка загрузки",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            });
                        }
                        catch (Exception ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                MessageBox.Show($"Ошибка обработки файла {Path.GetFileName(fileName)}: {ex.Message}",
                                    "Ошибка",
                                    MessageBoxButton.OK,
                                    MessageBoxImage.Error);
                            });
                        }
                        finally
                        {
                            semaphoreSlim.Release();
                        }
                    }));
                }

                await Task.WhenAll(tasks);
            }
            finally
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                await this.InitFolderAsync(this.currentPath);
            }
        }

        //Review OK
        private async Task UploadFolderAsync() 
        {
            CommonOpenFileDialog openDialog = new CommonOpenFileDialog();
            openDialog.IsFolderPicker = true;

            if (openDialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                try
                {
                    string selectedFolder = openDialog.FileName;
                    var listFilesItem = TreeScan(selectedFolder);

                    // Create all folders first
                    var folderTasks = new List<Task>();
                    foreach (var folder in FolderUserData.uniqueFolderName)
                    {
                        folderTasks.Add(Task.Run(async () =>
                        {
                            await semaphoreSlim.WaitAsync();
                            try
                            {
                                string folderName = folder.Remove(0, folder.IndexOf(
                                    Path.GetFileName(selectedFolder),
                                    StringComparison.InvariantCulture))
                                    .Replace('\\', '/');

                                string targetPath = Path.Combine(this.currentPath, folderName)
                                    .Replace('\\', '/');

                                await this.yandexClient.CreateFolderAsync(targetPath);
                            }
                            catch (YandexApiException)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show("Ошибка создания папки. Возможно, она уже существует.",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(folderTasks);
                    FolderUserData.uniqueFolderName.Clear();

                    // Upload all files
                    var fileTasks = new List<Task>();
                    foreach (var fileItem in listFilesItem)
                    {
                        fileTasks.Add(Task.Run(async () =>
                        {
                            await semaphoreSlim.WaitAsync();
                            try
                            {
                                using (var fileStream = new FileStream(
                                    fileItem.FileName, FileMode.Open, FileAccess.Read))
                                {
                                    string targetPath = Path.Combine(this.currentPath,
                                        fileItem.FileName.Remove(0, fileItem.FileName.IndexOf(
                                            Path.GetFileName(selectedFolder),
                                            StringComparison.InvariantCulture))
                                        .Replace('\\', '/'));

                                    await this.yandexClient.UpLoadFileAsync(targetPath, fileStream);
                                }
                            }
                            catch (YandexApiException)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show("Ошибка загрузки файла. Возможно, он уже существует.",
                                        "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                                });
                            }
                            finally
                            {
                                semaphoreSlim.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(fileTasks);
                }
                finally
                {
                    this.ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                }

                foldersItem.Clear();
                await this.InitFolderAsync(this.currentPath);
            }
        }

        List<FolderUserData> foldersItem = new List<FolderUserData>();

        private List<FolderUserData> TreeScan(string sDir)
        {

            foreach (string file in Directory.GetFiles(sDir))
            {
                foldersItem.Add(new FolderUserData(sDir, file));
            }
            foreach (string d in Directory.GetDirectories(sDir))
            {
                TreeScan(d);
            }
            return foldersItem;
        }

        private async Task DeleteFileAsync()
        {
            MessageBoxResult result = MessageBox.Show(
                "Удалить выбранные файлы?",
                "Сообщение",
                MessageBoxButton.YesNo,
                MessageBoxImage.Information);
            if (result == MessageBoxResult.Yes)
            {
                var currentPath = this.currentPath;
                try
                {
                    var collectionRows = selectedItems.AsEnumerable<Resource>();
                    this.ChangeVisibilityOfProgressBar(Visibility.Visible);
                    var tasks = new List<Task>();

                    foreach (var rowDataDisk in collectionRows)
                    {
                        if (rowDataDisk.Name != null)
                        {
                            await semaphoreSlim.WaitAsync();

                            tasks.Add(Task.Run(async () =>
                            {
                                try
                                {
                                    await this.yandexClient.DeleteFileAsync(currentPath + rowDataDisk.Name);
                                }
                                finally
                                {
                                    semaphoreSlim.Release();
                                }
                            }));
                        }                       
                    }
                    await Task.WhenAll(tasks);
                }
                catch (ArgumentOutOfRangeException ex)
                {
                    Debug.WriteLine($"Произошло исключение {ex.Message}");
                }
                finally
                {
                    _ = this.InitFolderAsync(this.currentPath);
                }
            }
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

        private void CloseApp()
        {
            System.Windows.Application.Current.Shutdown();
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
                currentPath = string.Concat (currentPath, rowDataDisk.Name, "/");
                _ = InitFolderAsync(currentPath);
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
            if (!string.IsNullOrEmpty(AccessToken))
                return;
            if (e.Error == null)
            {
                AccessToken = e.Result;
                this.dispatcher.BeginInvoke(new Action(() =>
                {
                    this.OnPropertyChanged("IsLoggedIn");
                    this.OnPropertyChanged("IsLoggedOut");
                    _ = this.InitFolderAsync(homePath);
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