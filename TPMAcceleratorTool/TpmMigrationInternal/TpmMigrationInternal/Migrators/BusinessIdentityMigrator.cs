//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class BusinessIdentityMigrator
    {
        public BusinessIdentityMigrator()
        {
        }

        public bool MigrateBusinessIdentity(Services.TpmContext cloudContext, Server.QualifierIdentity serverBusinessIdentity, Services.BusinessProfile cloudBusinessProfile)
        {
            var cloudBusinessIdentity = new Services.QualifierIdentity
                {
                    Name = serverBusinessIdentity.Name,
                    Qualifier = serverBusinessIdentity.Qualifier,
                    Value = serverBusinessIdentity.Value,
                };

            cloudContext.AddToBusinessIdentities(cloudBusinessIdentity);
            cloudContext.AddLink(cloudBusinessProfile, "BusinessIdentities", cloudBusinessIdentity);
            cloudContext.SetLink(cloudBusinessIdentity, "BusinessProfile", cloudBusinessProfile);
            return true;
        }

        public void MigrateBusinessIdentities(Services.TpmContext cloudContext, Server.BusinessProfile serverBusinessProfile, Services.BusinessProfile cloudBusinessProfile)
        {
            foreach (Server.BusinessIdentity businessIdentity in serverBusinessProfile.GetBusinessIdentities())
            {
                this.MigrateBusinessIdentity(cloudContext, (Server.QualifierIdentity)businessIdentity, cloudBusinessProfile);
            }
        }
    }
}