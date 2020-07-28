using Microsoft.Azure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace TestWithoutKinect
{
    public class AzureStorageOperation
    {
        // Copy paste the SasToken value in oneNote
        const string sasToken = "";
        const string storageAccountName = "kinectgymstorage";

        /// <summary>
        /// Get Uri String for account/container/blob
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="requireInsecureProtocol"></param>
        /// <param name="emulatorServer"></param>
        /// <returns>
        /// UriString for blob Address
        /// </returns>
        public string BuildUriString(
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

        /// <summary>
        /// Builds a root URI to an account in Azure Storage.
        /// </summary>
        /// <param name="accountName"></param>
        /// <param name="requireInsecureProtocol"></param>
        /// <param name="emulatorServer"></param>
        /// <returns>
        /// A string-URI address to an Azure Storage blob.
        /// </returns>
        public static string BuildAccountRootUriString(
          string accountName,
          bool requireInsecureProtocol = false,
          string emulatorServer = null)
        {

            return string.IsNullOrEmpty(emulatorServer)
                ? $@"{(requireInsecureProtocol ? "http" : "https")}://{accountName}.blob.core.windows.net"
                : $@"http://{emulatorServer}:10000/{accountName}";
        }

        /// <summary>
        /// Get CloudBlockBlob instance given containerName and blobName
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <returns>
        /// CloudBlockBlob instance
        /// </returns>
        public CloudBlockBlob GetCloudBlob(
            string containerName,
            string blobName)
        {
            string uri = BuildUriString(storageAccountName, containerName, blobName);
            var cloudBlockBlob = new CloudBlockBlob(new Uri(uri));
            return cloudBlockBlob;
        }

        /// <summary>
        /// Uploads a local file to the specified blob address.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="localFilePath"></param>
       /// <param name="cancellationToken">
        /// Optionally, a cancellation token to cancel the operation.
        /// </param>
        public async Task UploadFileAsync(
            string containerName,
            string blobName,
            string localFilePath,
            CancellationToken cancellationToken = default)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            await cloudBlockBlob.UploadFromFileAsync(localFilePath, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Uploads a stream of data to the specified blob address.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="sourceStream"></param> 
        /// <param name="cancellationToken">
        /// Optionally, a cancellation token to cancel the operation.
        /// </param>
        public async Task UploadStreamAsync(
            string containerName,
            string blobName,
            Stream sourceStream,
            CancellationToken cancellationToken = default)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            await cloudBlockBlob.UploadFromStreamAsync(sourceStream, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Downloads a blob to a target stream given it's container nameand blob name.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="destinationStream"></param>
        /// <param name="cancellationToken">
        /// Optionally, a cancellation token to cancel the operation.
        /// </param>
        public async Task DownloadBlobAsync(
          string containerName,
          string blobName,
          Stream destinationStream,
          CancellationToken cancellationToken = default)
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
            await cloudBlockBlob.DownloadToStreamAsync(destinationStream, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        ///  Sets an Azure Storage blob's metadata.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="metadata"></param>
        /// <param name="deleteExistingMetadata"></param>
        /// <param name="throwOnNullOrEmptyValues"></param>
        /// <param name="cancellationToken">
        /// Optionally, a cancellation token to cancel the operation.
        /// </param>
        public async Task SetBlobMetadataAsync(
            string containerName,
            string blobName,
            IDictionary<string, string> metadata,
            bool deleteExistingMetadata = false,
            bool throwOnNullOrEmptyValues = true,
            CancellationToken cancellationToken = default)
        {
            if (metadata is null)
            {
                throw new ArgumentNullException(nameof(metadata));
            }

            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            await cloudBlockBlob.FetchAttributesAsync(cancellationToken).ConfigureAwait(false);

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

            await cloudBlockBlob.SetMetadataAsync(cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Gets an Azure Storage blob's metadata.
        /// </summary>
        /// <param name="containerName"></param>
        /// <param name="blobName"></param>
        /// <param name="cancellationToken">
        /// Optionally, a cancellation token to cancel the operation.
        /// </param>
        /// <returns>
        /// Blob's metadata
        /// </returns>
        public async Task<Dictionary<string, string>> GetBlobMetadataAsync(
            string containerName,
            string blobName,
            CancellationToken cancellationToken = default)
        {
            CloudBlockBlob cloudBlockBlob = GetCloudBlob(containerName, blobName);

            await cloudBlockBlob.FetchAttributesAsync(cancellationToken).ConfigureAwait(false);

            return cloudBlockBlob.Metadata.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        }
    }
   

}
