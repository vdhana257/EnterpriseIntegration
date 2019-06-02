using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.IntegrationAccount
{
    public class RootObject
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("location")]
        public string Location { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("properties")]
        public Properties Properties { get; set; }

        [JsonProperty("sku")]
        public Sku Sku { get; set; }

        [JsonProperty("tags")]
        public Tags Tags { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }
    }

    public  class Tags
    {
        [JsonProperty("appID")]
        public string AppID { get; set; }

        [JsonProperty("env")]
        public string Env { get; set; }

        [JsonProperty("orgID")]
        public string OrgID { get; set; }
    }

    public class Sku
    {
        [JsonProperty("name")]
        public string Name { get; set; }
    }

    public class Properties
    {
    }
}
