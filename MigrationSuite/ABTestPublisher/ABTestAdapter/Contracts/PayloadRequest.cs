using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Contracts
{
    /// <summary>
    /// Class PayloadRequest.
    /// </summary>
    [DataContract]
    public class PayloadRequest
    {
        /// <summary>
        /// Gets or sets the path of file containing payload
        /// </summary>
        /// <value>The path to payload file.</value>
        [DataMember]
        public string PayloadFilePath { get; set; }
    }
}
