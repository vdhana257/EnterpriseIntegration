using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    class UriHelper
    {
        public string getSubscriptionUri()
        {
            string uri = ConfigurationManager.AppSettings["Config_SubscriptionsUri"];
            return uri;

        }

        public string getResourceGroupUri(string subscriptionId)
        {
            string uri = ConfigurationManager.AppSettings["Config_ResourceGroupUri"];
            uri = string.Format(uri, subscriptionId);
            return uri;
        }

        public string getWorkflowUri(string subscriptionId, string resourceGroup)
        {
            string uri = ConfigurationManager.AppSettings["Config_WorkflowUri"];
            uri = string.Format(uri, subscriptionId, resourceGroup);
            return uri;
        }

        public string getWorkflowVersionsUri(string subscriptionId, string resourceGroup, string workflow)
        {
            string uri = ConfigurationManager.AppSettings["Config_VersionUri"];
            uri = string.Format(uri, subscriptionId, resourceGroup, workflow);
            return uri;
        }

    }
}
