using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class ResourceGroups
    {
        public ResourceGroups(string json)
        {
            JObject jObject = JObject.Parse(json);
            name = (string)jObject["name"];
        }

        public string name { get; set; }

        public override string ToString()
        {
            return name;
        }

    }

}
