//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Text;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class EnvelopeOverridesMigrator
    {
        private Services.TpmContext cloudContext;

        public EnvelopeOverridesMigrator(Services.TpmContext cloudContext)
        {
            this.cloudContext = cloudContext;
        }

        public void MigrateX12EnvelopOverridese(Server.X12EnvelopeOverrides serverEnvelopOverrides, Services.X12ProtocolSettings cloudX12ProtocolSettings)
        {
            short tempResponsibleAgencyCode;
            string asciiStringValue = string.Empty;
            Services.X12EnvelopeOverride cloudEnvelopOverride = new Services.X12EnvelopeOverride();
            cloudEnvelopOverride.FunctionalIdentifierCode = serverEnvelopOverrides.FunctionalIdentifierCode;
            cloudEnvelopOverride.HeaderVersion = serverEnvelopOverrides.HeaderVersion;
            cloudEnvelopOverride.DateFormat = (short)serverEnvelopOverrides.DateFormat;
            cloudEnvelopOverride.TimeFormat = (short)serverEnvelopOverrides.TimeFormat;
            cloudEnvelopOverride.MessageId = serverEnvelopOverrides.MessageId;
            cloudEnvelopOverride.ProtocolVersion = serverEnvelopOverrides.ProtocolVersion;
            cloudEnvelopOverride.ReceiverApplicationId = serverEnvelopOverrides.ReceiverApplicationId;
            cloudEnvelopOverride.SenderApplicationId = serverEnvelopOverrides.SenderApplicationId;
            cloudEnvelopOverride.TargetNamespace = serverEnvelopOverrides.TargetNamespaceString;

            byte[] ascii = Encoding.ASCII.GetBytes(serverEnvelopOverrides.ResponsibleAgencyCode);
            asciiStringValue = ((int)ascii[0]).ToString(CultureInfo.InvariantCulture);
            short.TryParse(asciiStringValue, out tempResponsibleAgencyCode);
            cloudEnvelopOverride.ResponsibleAgencyCode = tempResponsibleAgencyCode;

            this.cloudContext.AddToX12EnvelopeOverrides(cloudEnvelopOverride);
            cloudX12ProtocolSettings.EnvelopeOverrides.Add(cloudEnvelopOverride);
            this.cloudContext.AddLink(cloudX12ProtocolSettings, "EnvelopeOverrides", cloudEnvelopOverride);
            cloudEnvelopOverride.X12ProtocolSettings = cloudX12ProtocolSettings;
            this.cloudContext.SetLink(cloudEnvelopOverride, "X12ProtocolSettings", cloudX12ProtocolSettings);
        }

        public void MigrateAllX12EnvelopeOverrides(Server.X12ProtocolSettings serverX12ProtocolSettings, Services.X12ProtocolSettings cloudX12ProtocolSettings)
        {
            foreach (Server.X12EnvelopeOverrides serverEnvOverrides in serverX12ProtocolSettings.EnvelopeSettings.GetOverrides())
            {
                this.MigrateX12EnvelopOverridese(serverEnvOverrides, cloudX12ProtocolSettings);
            }
        }

        public void MigrateEdifactEnvelopOverridese(Server.EDIFACTEnvelopeOverrides serverEnvelopOverrides, Services.EDIFACTProtocolSettings cloudEdifactProtocolSettings)
        {
            Services.EDIFACTEnvelopeOverride cloudEnvelopOverride = new Services.EDIFACTEnvelopeOverride();
            cloudEnvelopOverride.MessageId = serverEnvelopOverrides.MessageId;
            cloudEnvelopOverride.MessageVersion = serverEnvelopOverrides.MessageVersion;
            cloudEnvelopOverride.MessageRelease = serverEnvelopOverrides.MessageVersion;
            cloudEnvelopOverride.MessageAssociationAssignedCode = serverEnvelopOverrides.MessageAssociationAssignedCode;
            cloudEnvelopOverride.TargetNamespace = serverEnvelopOverrides.TargetNamespace;   
            cloudEnvelopOverride.FunctionalGroupId = serverEnvelopOverrides.FunctionalGroupId;
            cloudEnvelopOverride.SenderApplicationQualifier = serverEnvelopOverrides.SenderApplicationQualifier;
            cloudEnvelopOverride.SenderApplicationId = serverEnvelopOverrides.SenderApplicationId;
            cloudEnvelopOverride.ReceiverApplicationQualifier = serverEnvelopOverrides.ReceiverApplicationQualifier;
            cloudEnvelopOverride.ReceiverApplicationId = serverEnvelopOverrides.ReceiverApplicationId;
            cloudEnvelopOverride.ControllingAgencyCode = serverEnvelopOverrides.ControllingAgencyCode;
            cloudEnvelopOverride.GroupHeaderMessageVersion = serverEnvelopOverrides.GroupHeaderMessageVersion;
            cloudEnvelopOverride.GroupHeaderMessageRelease = serverEnvelopOverrides.GroupHeaderMessageRelease;
            cloudEnvelopOverride.AssociationAssignedCode = serverEnvelopOverrides.AssociationAssignedCode;            
            cloudEnvelopOverride.ApplicationPassword = serverEnvelopOverrides.ApplicationPassword;
            
            this.cloudContext.AddToEDIFACTEnvelopeOverrides(cloudEnvelopOverride);
            cloudEdifactProtocolSettings.EnvelopeOverrides.Add(cloudEnvelopOverride);
            this.cloudContext.AddLink(cloudEdifactProtocolSettings, "EnvelopeOverrides", cloudEnvelopOverride);
            cloudEnvelopOverride.EDIFACTProtocolSettings = cloudEdifactProtocolSettings;
            this.cloudContext.SetLink(cloudEnvelopOverride, "EDIFACTProtocolSettings", cloudEdifactProtocolSettings);
        }

        public void MigrateAllEdifactEnvelopeOverrides(Server.EDIFACTProtocolSettings serverEdifactProtocolSettings, Services.EDIFACTProtocolSettings cloudEdifactProtocolSettings)
        {
            foreach (Server.EDIFACTEnvelopeOverrides serverEnvOverrides in serverEdifactProtocolSettings.EnvelopeSettings.GetOverrides())
            {
                this.MigrateEdifactEnvelopOverridese(serverEnvOverrides, cloudEdifactProtocolSettings);
            }
        }
    }
}