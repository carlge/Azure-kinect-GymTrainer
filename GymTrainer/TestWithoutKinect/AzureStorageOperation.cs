using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace TestWithoutKinect
{
    public class AzureStorageOperation
    {
        const string sasToken = "?sv=2019-12-12&ss=bfqt&srt=sco&sp=rwdlacupx&se=2020-08-02T05:18:50Z&st=2020-07-27T21:18:50Z&spr=https&sig=dYd8En18mj%2ByfBJgAhHIqPuKzsWjLbrS4opFFfiA8Ng%3D";
        const string storageAccountName = "kinectgymstorage";

        public static string BuildUriString(
           string accountName,
           string containerName,
           string blobName,
           bool requireInsecureProtocol = false,
           string emulatorServer = null)
        {
            if (blobName.EndsWith("/"))
            {
                throw new ArgumentException("Blob addresses may not end with a forward slash.", nameof(blobName));
            }

            if (blobName.EndsWith("."))
            {
                throw new ArgumentException("Blob addresses may not end with a period.", nameof(blobName));
            }

            string rootUri = BuildAccountRootUriString(accountName, requireInsecureProtocol, emulatorServer);

            if (string.IsNullOrEmpty(containerName))
            {

                if (string.IsNullOrEmpty(blobName))
                {
                    return rootUri + sasToken;
                }

                return $"{rootUri}/{blobName}" + sasToken;
            }

            if (string.IsNullOrEmpty(blobName))
            {
                return $"{rootUri}/{containerName}" + sasToken;
            }

            return $"{rootUri}/{containerName}/{blobName}" + sasToken;
        }

        public static string BuildAccountRootUriString(
          string accountName,
          bool requireInsecureProtocol = false,
          string emulatorServer = null)
        {

            return string.IsNullOrEmpty(emulatorServer)
                ? $@"{(requireInsecureProtocol ? "http" : "https")}://{accountName}.blob.core.windows.net"
                : $@"http://{emulatorServer}:10000/{accountName}";
        }

        public CloudBlockBlob GetCloudBlob(string containerName, string blobName)
        {
            string uri = BuildUriString(storageAccountName, containerName, blobName);
            var cloudBlockBlob = new CloudBlockBlob(new Uri(uri));
            return cloudBlockBlob;
        }

        public void UploadFile(string containerName, string blobName, string localFilePath)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);
            cloudBlockBlob.UploadFromFile(localFilePath);
        }
        public void UploadStream(string containerName, string blobName, Stream sourceStream)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);
            cloudBlockBlob.UploadFromStream(sourceStream);
        }
        private CloudBlobContainer GetCloudBlobContainer(string azureStorageContainerName)
        {
            
            if (string.IsNullOrEmpty(azureStorageContainerName))
            {
                throw new ArgumentNullException(nameof(azureStorageContainerName));
            }
            string uri = BuildUriString(storageAccountName, azureStorageContainerName, "");
            var cloudBlockBlob = new CloudBlobContainer(new Uri(uri));

            return cloudBlockBlob;
        }
        
        public void DownloadBlob(
          string containerName,
          string blobName,
          Stream destinationStream)
        {

            if (destinationStream is null)
            {
                throw new ArgumentNullException(nameof(destinationStream));
            }

            if (!destinationStream.CanWrite)
            {
                throw new ArgumentException("The destination stream is not writable.", nameof(destinationStream));
            }
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);
            cloudBlockBlob.DownloadToStream(destinationStream);
        }

        public void SetBlobMetadata(
            string containerName,
            string blobName,
            IDictionary<string, string> metadata,
            bool deleteExistingMetadata = false,
            bool throwOnNullOrEmptyValues = true)
        {
            if (metadata is null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            if (deleteExistingMetadata)
            {
                cloudBlockBlob.Metadata.Clear();
            }

            foreach (KeyValuePair<string, string> metadataKeyAndValue in metadata)
            {
                if (string.IsNullOrEmpty(metadataKeyAndValue.Key))
                {
                    throw new ArgumentException("A metadata entry key is null or empty.", nameof(metadata));
                }

                if (string.IsNullOrEmpty(metadataKeyAndValue.Value))
                {
                    if (throwOnNullOrEmptyValues)
                    {
                        throw new ArgumentException($"Metadata entry '{metadataKeyAndValue.Key}' has a null or empty value.", nameof(metadata));
                    }

                    continue;
                }

                cloudBlockBlob.Metadata[metadataKeyAndValue.Key] = metadataKeyAndValue.Value;
            }

            cloudBlockBlob.SetMetadata();
        }

        public Dictionary<string, string> GetBlobMetadata(string containerName, string blobName)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            cloudBlockBlob.FetchAttributes();

            return cloudBlockBlob.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
   

}
