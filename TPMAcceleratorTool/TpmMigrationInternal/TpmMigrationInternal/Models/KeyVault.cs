using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.KeyVault
{
    public class RootObject
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string Location { get; set; }
        public Tags Tags { get; set; }
    }
    public class Tags
    {
        public string AppID { get; set; }
        public string OrgID { get; set; }
        public string Env { get; set; }
    }

   
}
