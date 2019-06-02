using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CommonModels
{
    public class IntegrationAccountDetails
    {
        private string aadInstance;

        private string resource;

        private string clientId;

        private string clientSecret;

        private string subscriptionId;

        private string resourceGroupName;

        private string integrationAccountName;

        public string AadInstance
        {
            get
            {
                return aadInstance;
            }

            set
            {
                aadInstance = value;
            }
        }

        public string Resource
        {
            get
            {
                return resource;
            }

            set
            {
                resource = value;
            }
        }

        public string ClientId
        {
            get
            {
                return clientId;
            }

            set
            {
                clientId = value;
            }
        }

        public string ClientSecret
        {
            get
            {
                return clientSecret;
            }

            set
            {
                clientSecret = value;
            }
        }

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

        public IntegrationAccountDetails()
        {
            AadInstance = ConfigurationManager.AppSettings["AADInstance"];
            Resource = ConfigurationManager.AppSettings["Resource"];
            ClientId = ConfigurationManager.AppSettings["ClientID"];
            ClientSecret = ConfigurationManager.AppSettings["ClientPassword"];
            SubscriptionId = ConfigurationManager.AppSettings["SubscriptionId"];
            ResourceGroupName = ConfigurationManager.AppSettings["ResourceGroupName"];
            IntegrationAccountName = ConfigurationManager.AppSettings["IntegrationAccountName"];
        }
    }
}
