using System.Collections.Generic;

namespace QR_Code_Tool.UserVariable
{
    public class FolderUserData
    {
        public string FolderName { get; set; }
        public string FileName { get; set; }

        public static HashSet<string> uniqueFolderName = new HashSet<string>();

        public FolderUserData(string folderName, string fileName)
        {
            this.FolderName = folderName;
            this.FileName = fileName;
            uniqueFolderName.Add(folderName);
        }
    }
}
