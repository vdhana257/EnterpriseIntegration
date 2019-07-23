//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;
    
    class BusinessProfileMigrator
    {
        private BusinessIdentityMigrator businessIdentityMigrator;
        private CustomSettingsMigrator customSettingsMigrator;

        public BusinessProfileMigrator()
        {
            this.businessIdentityMigrator = new BusinessIdentityMigrator();
            this.customSettingsMigrator = new CustomSettingsMigrator();
        }

        public void MigrateBusinessProfiles(Services.TpmContext cloudContext, Server.Partner serverPartner, Services.Partner cloudPartner)
        {
            foreach (Server.BusinessProfile businessProfile in serverPartner.GetBusinessProfiles())
            {
                this.MigrateBusinessProfile(cloudContext, businessProfile, cloudPartner);
            }
        }

        public void MigrateBusinessProfile(Services.TpmContext cloudContext, Server.BusinessProfile serverBusinessProfile, Services.Partner cloudPartner)
        {
            Services.BusinessProfile cloudBusinessProfile = new Services.BusinessProfile()
                {
                    Name = serverBusinessProfile.Name,
                    Description = serverBusinessProfile.Description,
                    Partner = cloudPartner
                };
            cloudContext.AddToBusinessProfiles(cloudBusinessProfile);
            cloudContext.RelateEntities(cloudBusinessProfile, cloudPartner, "Partner", "BusinessProfiles", Services.RelationshipCardinality.ManyToOne);
            
            // Migrating the CustomSettings and business identities
            this.customSettingsMigrator.MigrateProfileCustomSettings(cloudContext, serverBusinessProfile, cloudBusinessProfile);
            this.businessIdentityMigrator.MigrateBusinessIdentities(cloudContext, serverBusinessProfile, cloudBusinessProfile);
        }
    }
}