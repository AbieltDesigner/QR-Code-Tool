using System.IO;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool_App.Metods
{
    public interface IYandexAPI
    {
        string AccessToken { get; }

        Task<Resource> GetListFilesToFolderAsync(string currentPath);

        Task GetFileInfoAsync(string folderPath, string filePath);

        Task<Link> PublishFolderOrFileAsync(string filePath);

        Task<Link> UnPublishFolderOrFileAsync(string filePath);

        Task DeleteFileAsync(string filePath);

        Task UpLoadFileAsync(string filePath, Stream file);

        Task CreateFolderAsync(string folderPath);

    }
}
