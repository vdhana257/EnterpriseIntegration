using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public static class UrlHelper
    {
        public static string GetAgreementUrl(string agreementname, IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["AgreementUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, FileOperations.GetFileName(agreementname));
            return url;

        }

        public static string GetPartnerUrl(string partnerName, IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["PartnerUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, FileOperations.GetFileName(partnerName)); 
            return url;

        }

        public static string GetCertificateUrl(string certificatename, IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["CertificateUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, FileOperations.GetFileName(certificatename)); 
            return url;

        }
        public static string GetSchemaUrl(string schemaname, IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["SchemaUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, FileOperations.GetFileName(schemaname));
            return url;
        }
        public static string GetIAUrl(IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["IaUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName); 
            return url;

        }

        public static string GetPartnersUrl(IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["PartnersUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName);
            return url;

        }

        public static string GetSchemasUrl(IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["SchemasUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName); 
            return url;

        }

        public static string GeX12AgreementsUrl(IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["AgreementsUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName) + "&" + ConfigurationManager.AppSettings["X12AgreementsFilter"];
            return url;

        }

        public static string GeEdifactAgreementsUrl(IntegrationAccountDetails iaDetails)
        {
            string url = String.Format(ConfigurationManager.AppSettings["AgreementsUrl"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName) + "&" + ConfigurationManager.AppSettings["EdifactAgreementsFilter"];
            return url;

        }


        public static string GetSubscriptionsUrl()
        {
            string url = ConfigurationManager.AppSettings["SubscriptionsUri"];
            return url;

        }

        public static string GetResourceGroupsUrl(string subscriptionId)
        {
            string url = String.Format(ConfigurationManager.AppSettings["ResourceGroupUri"],subscriptionId);
            return url;
        }

        public static string GetIntegrationAccountsUrl(string subscriptionId, string resourceGroupName)
        {
            string url = String.Format(ConfigurationManager.AppSettings["IntegrationAccountUri"], subscriptionId,resourceGroupName);
            return url;
        }

        public static string GetKeyVaultsUrl(string subscriptionId , string resourceGroupName)
        {
            string url = String.Format(ConfigurationManager.AppSettings["KeyVaultUri"], subscriptionId, resourceGroupName) + ConfigurationManager.AppSettings["KeyVaultFilter1"] + "&" + ConfigurationManager.AppSettings["KeyVaultFilter2"];
            return url;
        }

    }
}
