using QR_Code_Tool.Provider;
using QR_Code_Tool.SDK;
using System;
using System.IO;
using System.Threading.Tasks;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.Metods
{
    public interface IYandexAPI
    {
        string AccessToken { get; }

        Task<Resource> GetListFilesToFolderAsync(string currentPath);

        Task GetFileInfoAsync(string folderPath, string filePath);

        Task<Link> PublishFolderOrFileAsync(string filePath);

        Task<Link> UnPublishFolderOrFileAsync(string filePath);       

        Task DeleteFileAsync (string filePath);

        Task UpLoadFileAsync(string filePath, Stream file);

    }
}
