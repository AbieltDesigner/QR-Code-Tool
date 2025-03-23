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
    public class Metods
    {
        private static string AccessToken { get; set; }
        DiskHttpApi diskHttpApi;

        public Metods(string Token)
        {
            AccessToken = Token;
            diskHttpApi = new DiskHttpApi(AccessToken);
        }

        public async Task<Resource> GetListFilesToFolder(string currentPath)
        {           
            string filePath = $@"disk:/" + $"{currentPath}";
            return await diskHttpApi.MetaInfo.GetInfoAsync(new ResourceRequest { Path = filePath });          
        }

        public async Task<Link> GetFileInfo(string folderPath, string filePath)
        {          
            string fullPath = $@"disk:/" + $"{folderPath}" + "/" + $"{filePath}";
            return await diskHttpApi.Files.GetDownloadLinkAsync(fullPath);
        }

        public async Task PublishFolderOrFile(string folderPath, string filePath = default)
        {
            await diskHttpApi.MetaInfo.PublishFolderAsync(folderPath + filePath);         
        }
        public async Task UnPublishFolderOrFile(string folderPath, string filePath = default)
        {
            await diskHttpApi.MetaInfo.UnpublishFolderAsync(folderPath + filePath);
        }

    }
}
