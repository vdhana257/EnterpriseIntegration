using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Subscription
{
    public class RootObject
    {
        [JsonProperty("authorizationSource")]
        public string AuthorizationSource { get; set; }

        [JsonProperty("displayName")]
        public string DisplayName { get; set; }

        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("subscriptionId")]
        public string SubscriptionId { get; set; }

        [JsonProperty("subscriptionPolicies")]
        public SubscriptionPolicies SubscriptionPolicies { get; set; }
    }

    public class SubscriptionPolicies
    {
        [JsonProperty("locationPlacementId")]
        public string LocationPlacementId { get; set; }

        [JsonProperty("quotaId")]
        public string QuotaId { get; set; }

        [JsonProperty("spendingLimit")]
        public string SpendingLimit { get; set; }
    }
}

