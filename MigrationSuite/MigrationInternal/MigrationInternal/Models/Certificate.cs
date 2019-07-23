using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class Certificate
    {
        public string certificateName { get; set; }
        public string certificateThumbprint { get; set; }

        public MigrationStatus ImportStatus { get; set; }
        public MigrationStatus ExportStatus { get; set; }
        public string ExportStatusText { get; set; }
    }
}
