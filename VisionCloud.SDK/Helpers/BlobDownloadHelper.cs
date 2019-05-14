using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using VisionCloud.API;
using VisionCloud.API.Models;

namespace VisionCloud.SDK.Helpers
{
    public static class BlobDownloadHelper
    {
        public static List<Dictionary<string, dynamic>> Download(Blob payload)
        {
            var result = new List<dynamic>();
            var dictToReturn = new List<Dictionary<string, dynamic>>();
            try
            {
                CloudBlobContainer container = new CloudBlobContainer(new Uri(payload.SASToken));
                int i = 0;
                int count = payload.FileNames.Count;
                foreach (var fileName in payload.FileNames)
                {
                    var dictToCollection = new Dictionary<string, dynamic>();
                    CloudBlockBlob blob = container.GetBlockBlobReference(fileName);
                    var obj = JsonConvert.DeserializeObject<object>(blob.DownloadText());
                    dictToCollection.Add(fileName, obj);
                    dictToReturn.Add(dictToCollection);
                    WriteProgress(i++, count);
                }
            }
            catch (StorageException ex)
            {
                ConsoleHelper.WriteException(ex);
            }
            return dictToReturn;
        }

        public static void WriteProgress(int i, int total)
        {
            if (i % 250 == 0)
            {
                Console.Write(i);
            }
            if (i % 10 == 0)
            {
                Console.Write(".");
            }
        }
    }
}
