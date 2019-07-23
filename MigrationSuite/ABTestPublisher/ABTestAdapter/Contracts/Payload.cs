using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Contracts
{
    /// <summary>
    /// Class Payload.
    /// </summary>
    [DataContract]
    public class Payload
    {
        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        [DataMember]
        public Stream Stream { get; set; }

        /// <summary>
        /// Gets or sets the file size in bytes.
        /// </summary>
        /// <value>The file size in bytes.</value>
        [DataMember]
        public long FileSizeInBytes { get; set; }

        /// <summary>
        /// Gets or sets the name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [DataMember]
        public string FileName { get; set; }

        /// <summary>
        /// Gets or sets the type of the content.
        /// </summary>
        /// <value>The type of the content.</value>
        [DataMember]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or sets the content
        /// </summary>
        /// <value>The file content</value>
        [DataMember]
        public string Content { get; set; }
                
    }
}
