using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.Win32;
using QR_Code_Tool_App.Metods;
using QR_Code_Tool_App.SDK;
using QR_Code_Tool_App.Serializable;
using QR_Code_Tool_App.Serializable.Entity;
using QR_Code_Tool_App.Service;
using QR_Code_Tool_App.UserVariable;
using YandexDisk.Client;
using YandexDisk.Client.Protocol;


namespace QR_Code_Tool_App.VievModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private IYandexAPI? yandexClient;
        private LoginWindow? loginWindow;
        private ObservableCollection<Resource>? folderItems;
        private Visibility isProressVisibility;
        private string? currentPath, previousPath;
        private readonly string homePath;
        public ObservableCollection<Resource> SelectedItemsInVM { get; } = [];
        private readonly SemaphoreSlim semaphoreSlim = new(1, 3);
        private readonly string jsonFilePath;
        private readonly AppSettingsDeserialize appSettingsDeserialize;
        private readonly AppSettings appSettings;
        private readonly IWindowService _windowService;

        public static string? AccessToken { get; set; }
        private readonly string Client_ID;
        private readonly string Return_URL;
        private readonly Dispatcher dispatcher;
        private ICommand? _clickBack;
        private ICommand? _clickGoUp;
        private ICommand? _clickRefresh;
        private ICommand? _clickHome;
        private ICommand? _clickPrintQR;
        private ICommand? _clickPublish;
        private ICommand? _clickUnPublish;
        private ICommand? _clickCopiLink;
        private ICommand? _clickUpLoadFile;
        private ICommand? _clickUpLoadFolder;
        private ICommand? _clickDeleteFile;
        private ICommand? _clickLogOut;
        private ICommand? _clickLogIn;
        private ICommand? _clickClose;   
        
        public ICommand CloseCommand { get; }
        public ICommand PreviewKeyDownCommand { get; }
        public ICommand RowDoubleClickCommand { get; }


        public ICommand ClickBack
        {
            get
            {
                return _clickBack ??= new CommandHandler(
                async () =>
                await BackAsync());
            }
        }
        public ICommand ClickGoUp
        {
            get
            {
                return _clickGoUp ??= new CommandHandler(
                async () =>
                await UpAsync());
            }
        }
        public ICommand ClickRefresh
        {
            get
            {
                return _clickRefresh ??= new CommandHandler(
                async () =>
                await RefreshAsync());
            }
        }
        public ICommand ClickHome
        {
            get
            {
                return _clickHome ??= new CommandHandler(
                async () =>
                await HomeAsync());
            }
        }
        public ICommand ClickPrintQR
        {
            get
            {
                return _clickPrintQR ??= new CommandHandler(
                () =>
                PrintQR(SelectedItemsInVM.AsEnumerable()));
            }
        }
        public ICommand ClickPublish
        {
            get
            {
                return _clickPublish ??= new CommandHandler(
                async () =>
                await PublishAsync(SelectedItemsInVM.AsEnumerable()));
            }
        }
        public ICommand ClickUnPublish
        {
            get
            {
                return _clickUnPublish ??= new CommandHandler(
                async () =>
                await UnPublishAsync(SelectedItemsInVM.AsEnumerable()));
            }
        }
        public ICommand ClickCopiLink
        {
            get
            {
                return _clickCopiLink ??= new CommandHandler(
                () =>
                CopiLink(SelectedItemsInVM.AsEnumerable()));
            }
        }
        public ICommand ClickUpLoadFile
        {
            get
            {
                return _clickUpLoadFile ??= new CommandHandler(
                async () =>
                await UploadFileAsync());
            }
        }
        public ICommand ClickUpLoadFolder
        {
            get
            {
                return _clickUpLoadFolder ??= new CommandHandler(
                async () =>
                await UploadFolderAsync());
            }
        }
        public ICommand ClickDeleteFile
        {
            get
            {
                return _clickDeleteFile ??= new CommandHandler(
                async () =>
                await DeleteFileAsync(SelectedItemsInVM.AsEnumerable()));
            }
        }
        public ICommand ClickLogOut
        {
            get
            {
                return _clickLogOut ??= new CommandHandler(
                () =>
                LogOut());
            }
        }
        public ICommand ClickLogIn
        {
            get
            {
                return _clickLogIn ??= new CommandHandler(
                () =>
                LogIn());
            }
        }
        public ICommand ClickClose
        {
            get
            {
                return _clickClose ??= new CommandHandler(
                () =>
                CloseApp());
            }
        }           
               
        public MainViewModel(Dispatcher dispatcher, IWindowService windowService)
        {
            this.dispatcher = dispatcher;
            _windowService = windowService;
            CloseCommand = new CommandHandler(() => _windowService.Close());
            PreviewKeyDownCommand = new CommandHandler(() => Row_OpenFolder(SelectedItemsInVM.AsEnumerable()));
            RowDoubleClickCommand = new CommandHandler(() => Row_OpenFolder(SelectedItemsInVM.AsEnumerable()));

            jsonFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Config/appSettings.json");
            appSettingsDeserialize = new AppSettingsDeserialize(jsonFilePath);
            appSettings = appSettingsDeserialize.GetSettingsModels();
            Client_ID = appSettings.ClientSettings.clientId;
            Return_URL = string.Concat(@"http://", appSettings.ClientSettings.returnUrl);
            homePath = appSettings.FolderSettings.HomeFolder;
            ShowLoginWindow(Client_ID, Return_URL);
            _ = InitFolderAsync(homePath);
        }

        private async Task InitFolderAsync(string path)
        {
            ChangeVisibilityOfProgressBar(Visibility.Visible);
            if (!string.IsNullOrEmpty(AccessToken))
            {
                currentPath = path;
                yandexClient = new YandexAPI(AccessToken);
                var resource = await yandexClient.GetListFilesToFolderAsync(currentPath);
                await dispatcher.BeginInvoke(new Action(() => { FolderItems = [.. resource.Embedded.Items]; }));
                OnPropertyChanged(nameof(FolderPath));
                ChangeVisibilityOfProgressBar(Visibility.Collapsed);
            }
        }

        private async Task BackAsync()
        {
            var delimeterIndex = currentPath!.Length > 1 ? currentPath.LastIndexOf("/", currentPath.Length - 2) : 0;
            if (delimeterIndex > 0)
            {
                var topPath = currentPath[..(delimeterIndex + 1)];
                previousPath = currentPath;
                await InitFolderAsync(topPath);
            }
        }
        private async Task UpAsync()
        {
            var previous = previousPath;
            previousPath = currentPath;
            await InitFolderAsync(previous!);
        }
        private async Task HomeAsync()
        {
            previousPath = currentPath;
            await InitFolderAsync(homePath);
        }
        private async Task RefreshAsync()
        {
            await InitFolderAsync(currentPath!);
        }
        private void PrintQR(IEnumerable<Resource> collectionRows)
        {
            var printZPL = new PrintZPL(collectionRows, appSettings.PrintSettings);
            printZPL.Print();
        }

        private async Task PublishAsync(IEnumerable<Resource> collectionRows)
        {
            var currentPath = this.currentPath;
            try
            {
                var localCollectionRows = collectionRows.ToList();
                ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                                if (yandexClient is not null)
                                    await yandexClient.PublishFolderOrFileAsync(string.Concat(currentPath, rowDataDisk.Name));
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
                Logger.Instance.Log(ex);
            }
            finally
            {
                await InitFolderAsync(this.currentPath!);
            }
        }
        private async Task UnPublishAsync(IEnumerable<Resource> collectionRows)
        {
            var currentPath = this.currentPath;
            try
            {
                var localCollectionRows = collectionRows.ToList();
                ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                                if (yandexClient is not null)
                                    await yandexClient.UnPublishFolderOrFileAsync(string.Concat(currentPath, rowDataDisk.Name));
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
                Logger.Instance.Log(ex);
            }
            finally
            {
                await InitFolderAsync(this.currentPath!);
            }
        }
        //Review OK
        private void CopiLink(IEnumerable<Resource> collectionRows)
        {
            try
            {
                var rowDataDisk = collectionRows.FirstOrDefault();
                if (rowDataDisk == null)
                    return;
                if (rowDataDisk.PublicUrl == null)
                {
                    MessageBox.Show("Публичная ссылка не сформирована, ее необходимо сначала сформировать", "Внимание!", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
                else
                {
                    Clipboard.SetText(rowDataDisk.PublicUrl);
                    MessageBox.Show("Ссылка скопирована в буфер обмена.", "Удачно", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }

            catch (NullReferenceException ex)
            {
                Logger.Instance.Log(ex);
            }

            catch (ArgumentOutOfRangeException ex)
            {
                Logger.Instance.Log(ex);
            }

            catch (ArgumentNullException ex)
            {
                Logger.Instance.Log(ex);
            }
        }

        //Review OK
        private async Task UploadFileAsync()
        {
            OpenFileDialog openDialog = new()
            {
                Multiselect = true,
                CheckFileExists = true,
                CheckPathExists = true
            };

            if (openDialog.ShowDialog() != true)
            {
                ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                return;
            }

            ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                            using var fileStream = new FileStream(
                                fileName,
                                FileMode.Open,
                                FileAccess.Read,
                                FileShare.Read,
                                bufferSize: 4096,
                                useAsync: true);
                            var targetPath = Path.Combine(currentPath!, Path.GetFileName(fileName))
                                .Replace(Path.DirectorySeparatorChar, '/');

                            if (yandexClient is not null)
                                await yandexClient.UpLoadFileAsync(targetPath, fileStream).ConfigureAwait(false);
                        }
                        catch (YandexApiException ex)
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                Logger.Instance.Log(ex);
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
                                Logger.Instance.Log(ex);
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
                ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                await InitFolderAsync(currentPath!);
            }
        }

        //Review OK
        private async Task UploadFolderAsync()
        {
            OpenFolderDialog openDialog = new();
            if (openDialog.ShowDialog() == true)
            {
                ChangeVisibilityOfProgressBar(Visibility.Visible);
                try
                {
                    string selectedFolder = openDialog.FolderName;
                    var listFilesItem = TreeScan(selectedFolder);

                    // Create all folders first
                    var folderTasks = new List<Task>();
                    foreach (var folder in FolderUserData.UniqueFolders)
                    {
                        folderTasks.Add(Task.Run(async () =>
                        {
                            await semaphoreSlim.WaitAsync();
                            try
                            {
                                string folderName = folder.Remove(0, folder.IndexOf(
                                    Path.GetFileName(selectedFolder),
                                    StringComparison.InvariantCulture))
                                    .Replace(Path.DirectorySeparatorChar, '/');

                                string targetPath = Path.Combine(currentPath!, folderName)
                                    .Replace(Path.DirectorySeparatorChar, '/');

                                if (yandexClient is not null)
                                    await yandexClient.CreateFolderAsync(targetPath);
                            }
                            catch (YandexApiException ex)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                            {
                                Logger.Instance.Log(ex);
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
                    FolderUserData.ResetUniqueFolders();

                    // Upload all files
                    var fileTasks = new List<Task>();
                    foreach (var fileItem in listFilesItem)
                    {
                        fileTasks.Add(Task.Run(async () =>
                        {
                            await semaphoreSlim.WaitAsync();
                            try
                            {
                                using var fileStream = new FileStream(
                                    fileItem.FileName, FileMode.Open, FileAccess.Read);
                                string targetPath = Path.Combine(currentPath!,
                                    fileItem.FileName.Remove(0, fileItem.FileName.IndexOf(
                                        Path.GetFileName(selectedFolder),
                                        StringComparison.InvariantCulture))
                                    .Replace(Path.DirectorySeparatorChar, '/'));

                                if (yandexClient is not null)
                                    await yandexClient.UpLoadFileAsync(targetPath, fileStream);
                            }
                            catch (YandexApiException ex)
                            {
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    Logger.Instance.Log(ex);
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
                    ChangeVisibilityOfProgressBar(Visibility.Collapsed);
                }

                await InitFolderAsync(currentPath!);
            }
        }

        private List<FolderUserData> TreeScan(string sDir)
        {
            var result = new List<FolderUserData>();
            ScanDirectory(sDir, result);
            return result;
        }

        private void ScanDirectory(string dir, List<FolderUserData> result)
        {
            foreach (string file in Directory.GetFiles(dir))
            {
                result.Add(new FolderUserData(dir, file));
            }

            foreach (string subDir in Directory.GetDirectories(dir))
            {
                ScanDirectory(subDir, result);
            }
        }

        private async Task DeleteFileAsync(IEnumerable<Resource> collectionRows)
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
                    ChangeVisibilityOfProgressBar(Visibility.Visible);
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
                                    if (yandexClient is not null)
                                        await yandexClient.DeleteFileAsync(currentPath + rowDataDisk.Name);
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
                    Logger.Instance.Log(ex);
                }
                finally
                {
                    _ = InitFolderAsync(this.currentPath!);
                }
            }
        }            

        private void LogOut()
        {

            Action logAction = loginWindow!.DeleteAllCookies;
            logAction();

            FolderItems = null!;
            currentPath = string.Empty;
            AccessToken = string.Empty;

            currentPath = string.Empty;
            AccessToken = string.Empty;
            OnPropertyChanged(nameof(IsLoggedIn));
            OnPropertyChanged(nameof(IsLoggedOut));
        }
        private void LogIn()
        {
            AccessToken = string.Empty;
            FolderItems = null!;
            currentPath = string.Empty;
            OnPropertyChanged(nameof(IsLoggedIn));
            ShowLoginWindow(Client_ID, Return_URL);
        }

        private void CloseApp()
        {
            CloseCommand.Execute(this);
        }       

        public void Row_OpenFolder(IEnumerable<Resource> collectionRows)
        {
            var rowDataDisk = collectionRows.FirstOrDefault();
            if (rowDataDisk?.Type is ResourceType.Dir)
            {
                previousPath = currentPath;
                currentPath = string.Concat(currentPath, rowDataDisk.Name, "/");
                _ = InitFolderAsync(currentPath);
            }
        }

        private void ShowLoginWindow(string client_ID, string return_URL)
        {
            loginWindow = new LoginWindow(client_ID, return_URL);
            loginWindow.AuthCompleted += OnAuthorizeCompleted!;
            loginWindow.ShowDialog();
        }

        private void ChangeVisibilityOfProgressBar(Visibility visibility, bool isIndeterminate = true)
        {
            dispatcher.BeginInvoke(new Action(() => { IsProressVisibility = visibility; }));
        }

        private void OnAuthorizeCompleted(object sender, GenericSdkEventArgs<string> e)
        {
            if (!string.IsNullOrEmpty(AccessToken))
                return;
            if (e.Error == null)
            {
                AccessToken = e.Result;
                dispatcher.BeginInvoke(new Action(() =>
                {
                    OnPropertyChanged(nameof(IsLoggedIn));
                    OnPropertyChanged(nameof(IsLoggedOut));
                    _ = InitFolderAsync(homePath);
                }));
            }
            else
            {
                ProcessError(e.Error);
            }
        }

        public Visibility IsProressVisibility
        {
            get
            {
                return isProressVisibility;
            }
            set
            {
                isProressVisibility = value;
                OnPropertyChanged(nameof(IsProressVisibility));
            }
        }

        public ObservableCollection<Resource> FolderItems
        {
            get { return folderItems!; }
            set
            {
                folderItems = value;
                OnPropertyChanged(nameof(FolderItems));
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
                    return currentPath;
                }
                return string.Empty;
            }
        }

        private void ProcessError(SdkException ex)
        {
            dispatcher.BeginInvoke(new Action(() => MessageBox.Show("SDK error: " + ex.Message)));
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged!?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}