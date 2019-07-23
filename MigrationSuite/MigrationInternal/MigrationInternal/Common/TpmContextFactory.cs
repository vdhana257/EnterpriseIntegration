//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Data.Services.Client;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Net;
    using System.Web;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;
    
    static class TpmContextFactory
    {
        const string PartnerManagementDataServicePath = "default/$PartnerManagement";

        public static T CreateTpmContext<T>(IApplicationContext applicationContext) where T : class
        {
            if (typeof(T) == typeof(Server.TpmContext))
            {
                return GetBizTalkTpmContext(applicationContext) as T;
            }
            else if (typeof(T) == typeof(Services.TpmContext))
            {
                return GetCloudTpmContext(applicationContext) as T;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        private static Services.TpmContext GetCloudTpmContext(IntegrationServiceDetails integrationServiceDetails)
        {
            var partnerManagementDataServiceUrl =
                new Uri(new Uri(integrationServiceDetails.DeploymentURL), PartnerManagementDataServicePath);
            Services.TpmContext context = new Services.TpmContext(partnerManagementDataServiceUrl);
            context.SaveChangesDefaultOptions = SaveChangesOptions.Batch;
            context.SendingRequest += (sender, args) => OnSendingRequest(args, AcsHelper.GetAcsToken(integrationServiceDetails));
            return context;
        }

        private static Services.TpmContext GetCloudTpmContext(IApplicationContext applicationContext)
        {
            var integrationServiceDetails = applicationContext.GetService<IntegrationServiceDetails>();
            Debug.Assert(integrationServiceDetails != null, "Integration service details not found in application context");
            return GetCloudTpmContext(integrationServiceDetails);
        }

        private static void OnSendingRequest(SendingRequestEventArgs e, string acsToken)
        {
            var request = (HttpWebRequest)e.Request;
            request.Headers[HttpRequestHeader.Authorization] = "WRAP access_token=\"" + HttpUtility.UrlDecode(acsToken) + "\"";
            request.Headers["x-ms-version"] = "1.0";
        }

        private static Server.TpmContext GetBizTalkTpmContext(IApplicationContext applicationContext)
        {
            var bizTalkManagementDbDetails = applicationContext.GetService<BizTalkManagementDBDetails>();
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder
                {
                    InitialCatalog = bizTalkManagementDbDetails.DatabaseName,
                    MultipleActiveResultSets = true,
                    DataSource = bizTalkManagementDbDetails.ServerName,
                    IntegratedSecurity = bizTalkManagementDbDetails.IsIntegratedSecurity,
                };
            if (!bizTalkManagementDbDetails.IsIntegratedSecurity)
            {
                builder.UserID = bizTalkManagementDbDetails.UserName;
                builder.Password = bizTalkManagementDbDetails.Password;
            }

            return Server.TpmContext.Create(builder);
        }

    }
}