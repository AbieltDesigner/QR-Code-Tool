using System.Collections.Concurrent;
using System.IO;

namespace QR_Code_Tool_App.UserVariable
{
    public class FolderUserData
    {
        public string FolderPath { get; set; }
        public string FileName { get; set; }

        private static readonly ConcurrentDictionary<string, byte> uniqueFolders = new ConcurrentDictionary<string, byte>();

        // Метод для сброса списка (при необходимости)
        public static void ResetUniqueFolders() => uniqueFolders.Clear();


        // Статическое свойство для доступа к уникальным папкам
        public static IReadOnlyCollection<string> UniqueFolders => uniqueFolders.Keys.OrderBy(folder => folder.Length).ToList();

        public FolderUserData(string folderPath, string fileName)
        {
            FolderPath = folderPath;
            FileName = fileName;

            if (!string.IsNullOrEmpty(folderPath))
            {
                var folder = Path.GetFullPath(folderPath);
                uniqueFolders.TryAdd(folder, 0);
            }
        }
    }
}
