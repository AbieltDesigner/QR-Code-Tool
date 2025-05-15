namespace QR_Code_Tool_App.Serializable.Entity
{
    public class FolderSettings(string homeFolder) : IFolderSettings
    {
        public string HomeFolder { get; set; } = homeFolder;
    }
}
