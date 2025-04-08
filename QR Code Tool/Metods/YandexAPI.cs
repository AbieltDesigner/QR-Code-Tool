using System.IO;
using System.Threading.Tasks;
using QR_Code_Tool.Metods;
using YandexDisk.Client.Clients;
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

        public async Task<Resource> GetListFilesToFolderAsync(string currentPath)
        {           
            string filePath = $@"disk:/" + $"{currentPath}";
            return await diskHttpApi.MetaInfo.GetInfoAsync(new ResourceRequest { Path = filePath });          
        }

        public async Task GetFileInfoAsync(string folderPath, string filePath)
        {          
            string fullPath = $@"disk:/" + $"{folderPath}" + "/" + $"{filePath}";
            await diskHttpApi.Files.GetDownloadLinkAsync(fullPath);
        }

        public async Task<Link> PublishFolderOrFileAsync(string filePath)
        {
            return await diskHttpApi.MetaInfo.PublishFolderAsync(filePath);         
        }

        public async Task<Link> UnPublishFolderOrFileAsync(string filePath)
        {
            return await diskHttpApi.MetaInfo.UnpublishFolderAsync(filePath);
        }

        public async Task DeleteFileAsync(string filePath)
        {
            var deleteFileRequest = new DeleteFileRequest() 
                { Path = (filePath), Permanently = false };
            await diskHttpApi.Commands.DeleteAsync(deleteFileRequest);
        }

        public async Task UpLoadFileAsync(string filePath, Stream file)
        {
            await diskHttpApi.Files.UploadFileAsync(filePath, false, file);
        }
    }
}
