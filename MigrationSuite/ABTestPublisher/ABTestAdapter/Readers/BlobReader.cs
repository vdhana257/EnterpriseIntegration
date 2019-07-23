using ABTestAdapter.Contracts;
using ABTestAdapter.Helpers;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Readers
{
    public class BlobReader : IReader
    {

        private Uri sasUri;

        public Uri SasUri { get; set; }

        public BlobReader(Uri blobSasUri)
        {
            SasUri = blobSasUri;
        }

        public List<JObject> GetJsonBlobs()
        {
            List<JObject> blobJsons = new List<JObject>();
            CloudBlobContainer testSuitContainer = new CloudBlobContainer(SasUri);
            var blobs = testSuitContainer.ListBlobs();

            foreach (IListBlobItem item in blobs)
            {
                // reading each test suit
                CloudBlockBlob blob = (CloudBlockBlob)item;
                Stream s = blob.OpenReadAsync().Result;
                string jsonString = Utility.StreamToString(s);

                // parse testsuit into JSON object
                blobJsons.Add(JObject.Parse(jsonString));

            }

            return blobJsons;
        }

        public List<Payload> GetBlobPayloads()
        {
            List<Payload> blobPayloads = new List<Payload>();
            CloudBlobContainer testSuitContainer = new CloudBlobContainer(SasUri);
            var blobs = testSuitContainer.ListBlobs();

            foreach (IListBlobItem item in blobs)
            {
                // reading each test suit
                CloudBlockBlob blob = (CloudBlockBlob)item; 
                var payload = new Payload();
                payload.Stream = blob.OpenReadAsync().Result;
                payload.FileName = blob.Name;
                blobPayloads.Add(payload);
            }
            return blobPayloads;
        }

    }
}
