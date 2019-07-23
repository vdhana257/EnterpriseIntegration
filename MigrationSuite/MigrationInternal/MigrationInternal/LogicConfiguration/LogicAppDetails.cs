using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class LogicAppDetails
    {
        private string subscriptionId;

        private string resourceGroup;

        private string workflow;

        public string SubscriptionId
        {
            get
            {
                return this.subscriptionId;
            }
            set
            {
                this.subscriptionId = value;
            }
        }
        public string ResourceGroup
        {
            get
            {
                return this.resourceGroup;
            }
            set
            {
                this.resourceGroup = value;
            }
        }
        public string Workflow
        {
            get
            {
                return this.workflow;
            }
            set
            {
                this.workflow = value;
            }
        }
    }
}
