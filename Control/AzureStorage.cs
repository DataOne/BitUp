using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.File;
using System;
using System.IO;
using DataOne.BitUp.Properties;

namespace DataOne.BitUp
{
    public class AzureStorage : IStorage
    {
        public void SaveFile(string filePath)
        {
            string fileName = Path.GetFileName(filePath);

            string accountName = Settings.Default.AzureAccountName;
            string accountKey = Settings.Default.AzureAccountKey;
            string storageContainer = Settings.Default.AzureStorageContainer;
            string defaultEndpointsProtocol = Resources.AzureEndpointsProtocol;
            string fileLocation = Resources.AzureFileLocation;
            string fileUri = Resources.AzureFileUri;

            string connectionKey = string.Concat(defaultEndpointsProtocol, "AccountName=", accountName, ";AccountKey=", accountKey, ";");

            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionKey);
            CloudFileClient fileClient = storageAccount.CreateCloudFileClient();

            CloudFileShare share = fileClient.GetShareReference(storageContainer);
            share.CreateIfNotExists();

            CloudFileDirectory rootDir = share.GetRootDirectoryReference();
            rootDir.CreateIfNotExists();

            var cloudFileUrl = string.Concat(fileLocation, accountName, fileUri, storageContainer, "/", fileName);
            var uriToFile = new Uri(cloudFileUrl);

            CloudFile file = new CloudFile(uriToFile, fileClient.Credentials);
            file.UploadFromFile(filePath);
        }

        public string GetName()
        {
            return "Azure File Storage";
        }

    }
}
