//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Configuration;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class IntegrationServiceDetails
    {
        private const string DefaultIssuerName = "owner";

        private string issuerName;

        public string DeploymentURL { get; set; }

        public string AcsNamespace { get; set; }
        
        public string IssuerName
        {
            get
            {
                if (issuerName == null)
                {
                    return DefaultIssuerName;
                }
                else
                {
                    return issuerName;
                }
            }

            set
            {
                issuerName = value;
            }
        }

        public string IssuerKey { get; set; }

        public IntegrationServiceDetails()
        {
            AcsNamespace = ConfigurationManager.AppSettings["AcsNamespace"];
            DeploymentURL = ConfigurationManager.AppSettings["DeploymentURL"];
            IssuerKey = ConfigurationManager.AppSettings["IssuerKey"];
        }
    }
}