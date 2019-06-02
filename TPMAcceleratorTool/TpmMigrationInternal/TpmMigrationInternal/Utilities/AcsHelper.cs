//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Specialized;
    using System.Configuration;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Text;

    static class AcsHelper
    {
        const string PartnerManagementDataServicePath = "default/$PartnerManagement";

        private static readonly string AcsBaseAddress = ConfigurationManager.AppSettings["acsHost"];

        public static string GetAcsToken(string acsNamespace, string issuerName, string issuerKey, string appliesToAddress)
        {
            using (WebClient client = new WebClient())
            {
                client.BaseAddress = string.Format(CultureInfo.InvariantCulture, @"https://{0}.{1}", acsNamespace, AcsBaseAddress);
                NameValueCollection values = new NameValueCollection();
                values.Add("wrap_name", issuerName);
                values.Add("wrap_password", issuerKey);
                values.Add("wrap_scope", appliesToAddress);
                try
                {
                    byte[] responseBytes = client.UploadValues("WRAPv0.9/", "POST", values);
                    string response = Encoding.UTF8.GetString(responseBytes);

                    // Extract the SWT token and return it.
                    return response
                        .Split('&')
                        .Single(value => value.StartsWith("wrap_access_token=", StringComparison.OrdinalIgnoreCase))
                        .Split('=')[1];
                }
                catch (WebException ex)
                {
                    TraceProvider.WriteLine(Resources.ErrorGettingACSToken, ex);
                    throw new TpmMigrationException(Resources.ErrorGettingACSToken, ex);
                }
            }
        }

        public static string GetAcsToken(IntegrationServiceDetails integrationServiceDetails)
        {
            var address = integrationServiceDetails.DeploymentURL;
            UriBuilder integrationDeploymentBaseUrl = new UriBuilder(address);
            integrationDeploymentBaseUrl.Scheme = "http";

            if (integrationDeploymentBaseUrl.Port == 443)
            {
                integrationDeploymentBaseUrl.Port = -1;
            }

            var partnerManagementDataServiceUrl = new Uri(integrationDeploymentBaseUrl.Uri, PartnerManagementDataServicePath);
            var token = AcsHelper.GetAcsToken(integrationServiceDetails.AcsNamespace, integrationServiceDetails.IssuerName, integrationServiceDetails.IssuerKey, partnerManagementDataServiceUrl.ToString());
            return token;
        }
    }
}
