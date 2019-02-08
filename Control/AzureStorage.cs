using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.IO;
using DataOne.BitUp.Properties;
using System.Linq;

namespace DataOne.BitUp
{
    public class AzureStorage : IStorage
    {
        public void SaveFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            string storageContainer = Settings.Default.AzureStorageContainer;
            string accountName = Settings.Default.AzureAccountName;
            string fileLocation = Resources.AzureFileLocation;
            string fileUri = Resources.AzureFileUri;

            var fileClient = GetFileClient();
            var rootDir = EnsureRootDirectory(fileClient);

            var cloudFileUrl = string.Concat(fileLocation, accountName, fileUri, storageContainer, "/", fileName);
            var uriToFile = new Uri(cloudFileUrl);

            CloudFile file = new CloudFile(uriToFile, fileClient.Credentials);
            file.UploadFromFile(filePath);
        }

        public string GetName()
        {
            return "Azure File Storage";
        }

        public bool IsEmpty()
        {
            var fileClient = GetFileClient();
            var rootDir = EnsureRootDirectory(fileClient);
            var content = rootDir.ListFilesAndDirectories().ToList();
            return content.Count == 0;
        }

        private CloudFileClient GetFileClient()
        {
            string defaultEndpointsProtocol = Resources.AzureEndpointsProtocol;
            string accountName = Settings.Default.AzureAccountName;
            string accountKey = Settings.Default.AzureAccountKey;

            string connectionKey = string.Concat(defaultEndpointsProtocol, "AccountName=", accountName, ";AccountKey=", accountKey, ";");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionKey);
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            return fileClient;
        }

        private CloudFileDirectory EnsureRootDirectory(CloudFileClient fileClient)
        {
            string storageContainer = Settings.Default.AzureStorageContainer;
            CloudFileShare share = fileClient.GetShareReference(storageContainer);
            share.CreateIfNotExists();

            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
            rootDir.CreateIfNotExists();
            return rootDir;
        }
    }
}
