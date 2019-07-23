//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Collections.Generic;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class CustomSettingsMigrator
    {
        public CustomSettingsMigrator()
        {
        }

        public void MigratePartnerCustomSettings(Services.TpmContext cloudContext, Server.Partner serverPartner, Services.Partner cloudPartner)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Server.CustomSettings serverCustomSettings = serverPartner.GetCustomSettings();
            foreach (string key in serverCustomSettings.Keys)
            {
                dict[key] = serverCustomSettings[key];
            }

            Services.CustomSetting cloudCustomSetting = new Services.CustomSetting() { Name = cloudPartner.Name };
            cloudCustomSetting.Blob = ByteArrayFormatter<Dictionary<string, string>>.Serialize(dict);
            cloudContext.AddToCustomSettings(cloudCustomSetting);
            cloudContext.RelateEntities(cloudCustomSetting, cloudPartner, "Partner", "CustomSettings", Services.RelationshipCardinality.ManyToOne);
        }

        public void MigrateAgreementCustomSettings(Services.TpmContext cloudContext, Server.Agreement serverAgreement, Services.Agreement cloudAgreement)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Server.CustomSettings serverCustomSettings = serverAgreement.GetCustomSettings();
            foreach (string key in serverCustomSettings.Keys)
            {
                dict[key] = serverCustomSettings[key];
            }

            Services.CustomSetting cloudCustomSetting = new Services.CustomSetting() { Name = cloudAgreement.Name };
            cloudCustomSetting.Blob = ByteArrayFormatter<Dictionary<string, string>>.Serialize(dict);
            cloudContext.AddToCustomSettings(cloudCustomSetting);
            cloudContext.RelateEntities(cloudCustomSetting, cloudAgreement, "Agreement", "CustomSettings", Services.RelationshipCardinality.ManyToOne);
        }

        public void MigrateProfileCustomSettings(Services.TpmContext cloudContext, Server.BusinessProfile serverBusinessProfile, Services.BusinessProfile cloudBusinessProfile)
        {
            Dictionary<string, string> dict = new Dictionary<string, string>();
            Server.CustomSettings serverCustomSettings = serverBusinessProfile.GetCustomSettings();
            foreach (string key in serverCustomSettings.Keys)
            {
                dict[key] = serverCustomSettings[key];
            }

            Services.CustomSetting cloudCustomSetting = new Services.CustomSetting() { Name = cloudBusinessProfile.Name };
            cloudCustomSetting.Blob = ByteArrayFormatter<Dictionary<string, string>>.Serialize(dict);
            cloudContext.AddToCustomSettings(cloudCustomSetting);
            cloudContext.RelateEntities(cloudCustomSetting, cloudBusinessProfile, "BusinessProfile", "CustomSettings", Services.RelationshipCardinality.ManyToOne);
        }
    }
}
