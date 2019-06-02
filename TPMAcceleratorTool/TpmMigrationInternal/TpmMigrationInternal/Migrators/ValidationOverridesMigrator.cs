//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class ValidationOverridesMigrator
    {
        private Services.TpmContext cloudContext;

        public ValidationOverridesMigrator(Services.TpmContext cloudContext)
        {
            this.cloudContext = cloudContext;
        }

        public void MigrateX12ValidationOverrides(Server.X12ValidationOverrides serverValidationOverrides, Services.X12ProtocolSettings cloudX12ProtocolSettings)
        {
            Services.X12ValidationOverride cloudValidationOverride = new Services.X12ValidationOverride();
            cloudValidationOverride.AllowLeadingAndTrailingSpacesAndZeroes = serverValidationOverrides.AllowLeadingAndTrailingSpacesAndZeroes;
            cloudValidationOverride.MessageId = serverValidationOverrides.MessageId;
            cloudValidationOverride.SeparatorPolicy = (short)serverValidationOverrides.TrailingSeparatorPolicy;
            cloudValidationOverride.ValidateCharacterSet = serverValidationOverrides.ValidateCharacterSet;
            cloudValidationOverride.ValidateEDITypes = serverValidationOverrides.ValidateEDITypes;
            cloudValidationOverride.ValidateXSDTypes = serverValidationOverrides.ValidateXSDTypes;

            this.cloudContext.AddToX12ValidationOverrides(cloudValidationOverride);
            cloudX12ProtocolSettings.ValidationOverrides.Add(cloudValidationOverride);
            this.cloudContext.AddLink(cloudX12ProtocolSettings, "ValidationOverrides", cloudValidationOverride);
            cloudValidationOverride.X12ProtocolSettings = cloudX12ProtocolSettings;
            this.cloudContext.SetLink(cloudValidationOverride, "X12ProtocolSettings", cloudX12ProtocolSettings);
        }

        public void MigrateEdifactValidationOverrides(Server.EDIFACTValidationOverrides serverValidationOverrides, Services.EDIFACTProtocolSettings cloudEdifactProtocolSettings)
        {
            Services.EDIFACTValidationOverride cloudValidationOverride = new Services.EDIFACTValidationOverride();
            cloudValidationOverride.AllowLeadingAndTrailingSpacesAndZeroes = serverValidationOverrides.AllowLeadingAndTrailingSpacesAndZeroes;
            cloudValidationOverride.EnforceCharacterSet = serverValidationOverrides.EnforceCharacterSet;
            cloudValidationOverride.SeparatorPolicy = (short)serverValidationOverrides.TrailingSeparatorPolicy;
            cloudValidationOverride.MessageId = serverValidationOverrides.MessageId;
            cloudValidationOverride.ValidateEDITypes = serverValidationOverrides.ValidateEDITypes;
            cloudValidationOverride.ValidateXSDTypes = serverValidationOverrides.ValidateXSDTypes;

            this.cloudContext.AddToEDIFACTValidationOverrides(cloudValidationOverride);
            cloudEdifactProtocolSettings.ValidationOverrides.Add(cloudValidationOverride);
            this.cloudContext.AddLink(cloudEdifactProtocolSettings, "ValidationOverrides", cloudValidationOverride);
            cloudValidationOverride.EDIFACTProtocolSettings = cloudEdifactProtocolSettings;
            this.cloudContext.SetLink(cloudValidationOverride, "EDIFACTProtocolSettings", cloudEdifactProtocolSettings);
        }

        internal void MigrateAllX12ValidationOverrides(Server.X12ProtocolSettings serverX12ProtocolSettings, Services.X12ProtocolSettings cloudX12ProtocolSettings)
        {
            foreach (var serverValidationOverrides in serverX12ProtocolSettings.ValidationSettings.GetOverrides())
            {
                this.MigrateX12ValidationOverrides(serverValidationOverrides, cloudX12ProtocolSettings);
            }
        }

        internal void MigrateAllEdifactValidationOverrides(Server.EDIFACTProtocolSettings serverEdifactProtocolSettings, Services.EDIFACTProtocolSettings cloudEdifactProtocolSettings)
        {
            foreach (var serverValidationOverrides in serverEdifactProtocolSettings.ValidationSettings.GetOverrides())
            {
                this.MigrateEdifactValidationOverrides(serverValidationOverrides, cloudEdifactProtocolSettings);
            }
        }
    }
}