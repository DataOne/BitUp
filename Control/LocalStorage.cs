using System.IO;

namespace DataOne.BitUp
{
    public class LocalStorage : IStorage
    {
        private const string RepositoryFolderName = @".\DataOne.BitUp.RepositoryBackups";

        public string GetName()
        {
            return "local file system";
        }

        public void SaveFile(string filePath)
        {
            try
            {
                Directory.CreateDirectory(RepositoryFolderName);
            }
            finally
            {
                File.Copy(filePath, RepositoryFolderName + @"\" + Path.GetFileName(filePath), true);
            }
        }

        public bool IsEmpty()
        {
            var subDirs = Directory.GetDirectories(RepositoryFolderName);
            var files = Directory.GetFiles(RepositoryFolderName);
            return subDirs.Length > 0 && files.Length > 0;
        }
    }
}
