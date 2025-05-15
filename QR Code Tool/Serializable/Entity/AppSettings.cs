using System;

namespace QR_Code_Tool.Serializable.Entity
{
    [Serializable]
    public class AppSettings
    {
        public ClientSettings ClientSettings { get; set; }
        public PrintSettings PrintSettings { get; set; }
        public FolderSettings FolderSettings { get; set; }

        public AppSettings(ClientSettings сlientSettings, PrintSettings printSettings, FolderSettings folderSettings)
        {
            this.ClientSettings = сlientSettings;
            this.PrintSettings = printSettings;
            this.FolderSettings = folderSettings;
        }
    }
}
