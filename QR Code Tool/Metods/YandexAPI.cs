using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using YandexDisk.Client.Http;
using YandexDisk.Client.Protocol;

namespace QR_Code_Tool.API
{
    public class YandexAPI
    {
        private static string AccessToken { get; set; }
        DiskHttpApi diskHttpApi;

        public YandexAPI(string Token)
        {
            AccessToken = Token;
            diskHttpApi = new DiskHttpApi(AccessToken);
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
