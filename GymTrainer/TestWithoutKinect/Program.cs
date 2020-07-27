using System;
using System.Collections.Generic;

namespace TestWithoutKinect
{
    class Program
    {
        static void Main(string[] args)
        {
            AzureStorageOperation azureStorageOperation = new AzureStorageOperation();
            azureStorageOperation.UploadFile("video", "myFile.txt", @"C:\Users\chhsiao\Documents\myFile.txt");
            using (var fileStream = System.IO.File.OpenWrite(@"C:\Users\chhsiao\Documents\hackathon\myFile.txt"))
            {
                azureStorageOperation.DownloadBlob("video", "myFile.txt", fileStream);
            }
            azureStorageOperation.SetBlobMetadata("video", "myFile.txt", new Dictionary<string, string>{ { "hello", "world" } });
            Dictionary<string, string> dic = azureStorageOperation.GetBlobMetadata("video", "myFile.txt");
            Console.Out.Write(dic);

        }
    }
}
