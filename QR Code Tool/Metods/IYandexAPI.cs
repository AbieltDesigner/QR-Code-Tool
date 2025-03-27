using System.Threading.Tasks;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.Metods
{
    public interface IYandexAPI
    {
        string AccessToken { get; }

        Task<Resource> GetListFilesToFolder(string currentPath);

        Task GetFileInfo(string folderPath, string filePath);

        Task<Link> PublishFolderOrFile(string folderPath, string filePath = default);

        Task<Link> UnPublishFolderOrFile(string folderPath, string filePath = default);
    }
}
