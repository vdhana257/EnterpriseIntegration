using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class SendPortDetails
    {
        /// <summary>
        /// Thumprint of the certificate attached to the sendport.
        /// </summary>
        public string CertificateThumbprint { get; set; }

        /// <summary>
        /// As2Url for the sendport.
        /// </summary>
        public string As2Url { get; set; }

        /// <summary>
        /// Certificate name of the attached sendport certificate.
        /// </summary>
        public string CertificateName { get; set; }
    }
}
