using System.Threading.Tasks;
using QR_Code_Tool.Metods;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.API
{
    public class YandexAPI : IYandexAPI
    {
        private readonly string accessToken;

        DiskHttpApi diskHttpApi;

        public YandexAPI(string accessToken)
        {
            this.accessToken = accessToken;
            diskHttpApi = new DiskHttpApi(accessToken);
        }

        /// <summary>
        /// Пользовательский токен
        /// </summary>
        /// <value>The access token.</value>
        public string AccessToken
        {
            get { return this.accessToken; }
        }

        public async Task<Resource> GetListFilesToFolder(string currentPath)
        {           
            string filePath = $@"disk:/" + $"{currentPath}";
            return await diskHttpApi.MetaInfo.GetInfoAsync(new ResourceRequest { Path = filePath });          
        }

        public async Task GetFileInfo(string folderPath, string filePath)
        {          
            string fullPath = $@"disk:/" + $"{folderPath}" + "/" + $"{filePath}";
            await diskHttpApi.Files.GetDownloadLinkAsync(fullPath);
        }

        public async Task<Link> PublishFolderOrFile(string folderPath, string filePath = default)
        {
            return await diskHttpApi.MetaInfo.PublishFolderAsync(folderPath + filePath);         
        }

        public async Task<Link> UnPublishFolderOrFile(string folderPath, string filePath = default)
        {
            return await diskHttpApi.MetaInfo.UnpublishFolderAsync(folderPath + filePath);
        }

    }
}
