using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class Subscriptions
    {
        public Subscriptions(string json)
        {
            JObject jObject = JObject.Parse(json);
            subscriptionId = (string)jObject["subscriptionId"];
            displayName = (string)jObject["displayName"];

        }

        public string displayName { get; set; }

        public string subscriptionId { get; set; }

        public override string ToString()
        {
            return displayName;
        }
    }
}
