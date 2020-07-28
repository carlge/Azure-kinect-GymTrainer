using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace TestWithoutKinect
{
    class Program
    {
        static async Task Main(string[] args)
        {
            AzureStorageOperation azureStorageOperation = new AzureStorageOperation();
            await azureStorageOperation.UploadFileAsync("video", "myFile.txt", @"C:\Users\chhsiao\Documents\myFile.txt");
            using (FileStream file = File.Open(@"C:\Users\chhsiao\Desktop\videoTest.mp4", FileMode.Open))
            {

                await azureStorageOperation.UploadStreamAsync("video", "videoTest.mp4", file);
            }

            using (var fileStream = File.OpenWrite(@"C:\Users\chhsiao\Documents\hackathon\myFile.txt"))
            {
               await azureStorageOperation.DownloadBlobAsync("video", "myFile.txt", fileStream);

            }
            await azureStorageOperation.SetBlobMetadataAsync("video", "myFile.txt", new Dictionary<string, string> { { "hello", "world" } });
        }
    }
}
