using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class SchemaJson
   {
        public Properties properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class ContentHash
    {
        public string algorithm { get; set; }
        public string value { get; set; }
    }

    public class ContentLink
    {
        public string uri { get; set; }
        public string contentVersion { get; set; }
        public int contentSize { get; set; }
        public ContentHash contentHash { get; set; }
    }

    public class Metadata
    {
    }

    public class Properties
    {
        public string schemaType { get; set; }
        public string targetNamespace { get; set; }
        public string documentName { get; set; }
        public ContentLink contentLink { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime changedTime { get; set; }
        public Metadata metadata { get; set; }
    }

}
