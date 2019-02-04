
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

        /// <summary>
        /// Get the information whether the storage contents any files or directories
        /// </summary>
        /// <returns>Boolean value whether the storage is empty</returns>
        bool IsEmpty();
    }
}
