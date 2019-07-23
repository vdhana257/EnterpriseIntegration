using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class IntegrationAccountDetails
    {

        private string subscriptionId;

        private string resourceGroupName;

        private string integrationAccountName;

        private string keyvaultName;

       

        public string SubscriptionId
        {
            get
            {
                return subscriptionId;
            }

            set
            {
                subscriptionId = value;
            }
        }

        public string ResourceGroupName
        {
            get
            {
                return resourceGroupName;
            }

            set
            {
                resourceGroupName = value;
            }
        }

        public string IntegrationAccountName
        {
            get
            {
                return integrationAccountName;
            }

            set
            {
                integrationAccountName = value;
            }
        }

        public string KeyVaultName
        {
            get
            {
                return keyvaultName;
            }

            set
            {
                keyvaultName = value;
            }
        }

        public IntegrationAccountDetails()
        {
        }
    }
}
