namespace QR_Code_Tool.UserVariable
{
    public class FolderItems
    {
        public string FolderName { get; set; }
        public string FileName { get; set; }

        public FolderItems(string folderName, string fileName)
        {
            this.FolderName = folderName;
            this.FileName = fileName;

        }
    }
}
