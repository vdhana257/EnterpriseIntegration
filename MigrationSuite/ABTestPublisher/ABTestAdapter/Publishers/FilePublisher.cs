using ABTestAdapter.Contracts;
using ABTestAdapter.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Publishers
{
    public class FilePublisher : IPublisher
    {
        private string filesharePath = string.Empty;
        private string contentFormat = "STREAM";

        public FilePublisher(string filePath, string contentType)
        {
            FilesharePath = filePath;
            contentFormat = contentType;
        }

        public string FilesharePath
        {
            get
            {
                return filesharePath;
            }

            set
            {
                filesharePath = value;
            }
        }

        public async void Publish(Payload payloadObject)
        {
            string filename = payloadObject.FileName;
            //expectedOutputFileName = string.Format("{0}_{1}.bin", controlNumber, id);
            if (contentFormat.ToUpper() == AppConstants.FormatStream)
            {
                using (FileStream output = new System.IO.FileStream(FilesharePath + @"\" + filename, FileMode.Create))
                {
                    try
                    {
                        await payloadObject.Stream.CopyToAsync(output);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message);
                    }
                }
            }
            else
            {
                string requestContent = Utility.StreamToStringUtf16(payloadObject.Stream);
                try
                {
                    File.WriteAllText(FilesharePath + @"\" + filename, requestContent);
                }
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
            }
            

        }
    }
}
