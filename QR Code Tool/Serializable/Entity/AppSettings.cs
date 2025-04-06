using System;

namespace QR_Code_Tool.Serializable.Entity
{
    [Serializable]
    public class AppSettings
    {
        public ClientSettings MainResources { get; set; }
        public PrintSettings PrintSettings { get; set; }
       
        public AppSettings(ClientSettings mainResources, PrintSettings printSettings)
        {
            this.MainResources = mainResources;
            this.PrintSettings = printSettings;
        }
    }
}
