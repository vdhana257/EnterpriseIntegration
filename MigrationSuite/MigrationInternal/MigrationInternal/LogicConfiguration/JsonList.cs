
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class JsonList
    {
        public JsonList(string json)
        {
            JObject jObject = JObject.Parse(json);
            subString = jObject["value"].ToArray();
        }

        public Array subString { get; set; }
    }
}
