using System.IO;

namespace DataOne.BitUp
{
    public class LocalStorage : IStorage
    {
        public string GetName()
        {
            return "local file system";
        }

        public void SaveFile(string filePath)
        {
            try
            {
                Directory.CreateDirectory(@".\DataOne.BitUp.RepositoryBackups");
            }
            finally
            {
                File.Copy(filePath, @".\DataOne.BitUp.RepositoryBackups\" + Path.GetFileName(filePath), true);
            }
        }
    }
}
