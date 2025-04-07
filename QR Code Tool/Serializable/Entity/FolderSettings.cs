namespace QR_Code_Tool.Serializable.Entity
{
    public class FolderSettings : IFolderSettings
    {
        public string HomeFolder { get; set; }

        public FolderSettings(string homeFolder)
        {
            this.HomeFolder = homeFolder;
        }
    }
}
