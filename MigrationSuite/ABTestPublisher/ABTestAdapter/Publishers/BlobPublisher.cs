using ABTestAdapter.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using ABTestAdapter.Helpers;

namespace ABTestAdapter.Publishers
{
    public class BlobPublisher : IPublisher
    {

        private CloudBlobContainer blobContainer;
        private string contentFormat = "STREAM";


        public BlobPublisher(Uri blobContainerUri, string contentType)
        {
            blobContainer = new CloudBlobContainer(blobContainerUri);
            contentFormat = contentType;
        }


        public async void Publish(Payload payloadObject)
        {
            try
            {

                CloudBlockBlob blobToUpload = blobContainer.GetBlockBlobReference(payloadObject.FileName);
                if (contentFormat.ToUpper() == AppConstants.FormatStream)
                {
                    await blobToUpload.UploadFromStreamAsync(payloadObject.Stream);
                }
                else
                {
                    string requestContent = Utility.StreamToStringUtf16(payloadObject.Stream);
                    await blobToUpload.UploadTextAsync(requestContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("error while adding blob file to blob storage " + ex.Message + ex.StackTrace);
            }
        }
    }
}
