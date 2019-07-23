using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Models
{
    public class ApplicationDetails
    {
        public ApplicationDetails()
        {
            isSelected = false;
        }

        public string nID { get; set; }
        public string nvcName { get; set; }
        public bool isDefault { get; set; }
        public bool isSystem { get; set; }
        public string nvcDescription { get; set; }
        public DateTime DateModified { get; set; }
        public bool isSelected { get; set; }
    }
}
