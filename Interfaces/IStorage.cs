
namespace DataOne.BitUp
{
    interface IStorage
    {
        /// <summary>
        /// Stores a file 
        /// </summary>
        /// <param name="filePath">The full path of the file to save</param>
        void SaveFile(string filePath);

        /// <summary>
        /// Get the name of the location where backups will be stored
        /// </summary>
        /// <returns>The name of the location where files are saved, e.g. Azure, local file system...</returns>
        string GetName();
    }
}
