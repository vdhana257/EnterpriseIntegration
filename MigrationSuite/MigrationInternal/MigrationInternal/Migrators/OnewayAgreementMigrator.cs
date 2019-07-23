//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Linq;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class OnewayAgreementMigrator
    {
        private ProtocolSettingsMigrator protocolSettingsMigrator;

        public OnewayAgreementMigrator(IApplicationContext applicationContext)
        {
            this.protocolSettingsMigrator = new ProtocolSettingsMigrator(applicationContext);
        }

        public void MigrateOnewayAgreements(Services.TpmContext cloudContext, Server.Agreement serverAgreement, string serverAgreementSenderPartnerName, string serverAgreementReceiverPartnername, Services.Agreement cloudAgreement, out MigrationStatus migrationStatus)
        {
            migrationStatus = MigrationStatus.Succeeded;
            Server.OnewayAgreement serverSendOnewayAgreement = serverAgreement.GetOnewayAgreement(serverAgreementSenderPartnerName, serverAgreementReceiverPartnername);
            Server.OnewayAgreement serverReceiveOnewayAgreement = serverAgreement.GetOnewayAgreement(serverAgreementReceiverPartnername, serverAgreementSenderPartnerName);

            var serverSenderBusinessIdentity = serverSendOnewayAgreement.SenderIdentity as Server.QualifierIdentity;
            var serverReceiverBusinessIdentity = serverSendOnewayAgreement.ReceiverIdentity as Server.QualifierIdentity;
            MigrationStatus onewayAgreementAToBMigrationStatus = MigrationStatus.Succeeded;
            this.MigrateOnewayAgreement(cloudContext, serverSendOnewayAgreement, serverSenderBusinessIdentity, serverReceiverBusinessIdentity, cloudAgreement, "OnewayAgreementAToB", out onewayAgreementAToBMigrationStatus);
            MigrationStatus onewayAgreementBToAMigrationStatus = MigrationStatus.Succeeded;
            this.MigrateOnewayAgreement(cloudContext, serverReceiveOnewayAgreement, serverReceiverBusinessIdentity, serverSenderBusinessIdentity, cloudAgreement, "OnewayAgreementBToA", out onewayAgreementBToAMigrationStatus);

            if (onewayAgreementAToBMigrationStatus == MigrationStatus.Partial || onewayAgreementBToAMigrationStatus == MigrationStatus.Partial)
            {
                migrationStatus = MigrationStatus.Partial;
            }
        }

        public void MigrateOnewayAgreement(
            Services.TpmContext cloudContext,
            Server.OnewayAgreement serverOnewayAgreement,
            Server.QualifierIdentity serverSenderBusinessIdentity,
            Server.QualifierIdentity serverReceiverBusinessIdentity,
            Services.Agreement cloudAgreement,
            string onewayAgreementType,
            out MigrationStatus migrationStatus)
        {
            Services.OnewayAgreement cloudOnewayAgreement = new Services.OnewayAgreement();
            cloudContext.AddToOnewayAgreements(cloudOnewayAgreement);
            cloudContext.RelateEntities(
                cloudOnewayAgreement,
                cloudAgreement,
                onewayAgreementType == "OnewayAgreementAToB" ? "AgreementAsAToB" : "AgreementAsBToA",
                onewayAgreementType,
                Services.RelationshipCardinality.OneToOne);

            this.LinkBusinessProfilesToOnewayAgreement(
                cloudContext,
                cloudOnewayAgreement,
                cloudAgreement.BusinessProfileA,
                cloudAgreement.BusinessProfileB,
                serverSenderBusinessIdentity,
                serverReceiverBusinessIdentity,
                onewayAgreementType);

            // Migrate send and receive protocol settings
            Server.ProtocolSettings serverProtocolSettings;
            switch (cloudAgreement.ProtocolName)
            {
                case AppConstants.X12ProtocolName:
                    serverProtocolSettings = serverOnewayAgreement.GetProtocolSettings<Server.X12ProtocolSettings>();
                    break;
                case AppConstants.AS2ProtocolName:
                    serverProtocolSettings = serverOnewayAgreement.GetProtocolSettings<Server.AS2ProtocolSettings>();
                    break;
                case AppConstants.EdifactProtocolName:
                    serverProtocolSettings = serverOnewayAgreement.GetProtocolSettings<Server.EDIFACTProtocolSettings>();
                    break;
                default:
                    throw new NotSupportedException("Migration of  X12, AS2, EDIFACT agreements only is supported");
            }

            this.protocolSettingsMigrator.MigrateProtocolSettings(cloudContext, cloudOnewayAgreement, serverProtocolSettings, onewayAgreementType == "OnewayAgreementAToB" ? cloudAgreement.BusinessProfileA : cloudAgreement.BusinessProfileB, cloudAgreement.Name, out migrationStatus);
        }

        private static bool TryGetCloudBusinessIdentity(
            Services.BusinessProfile cloudBusinessProfile,
            Server.QualifierIdentity serverBusinessIdentity,
            out Services.BusinessIdentity cloudBusinessIdentity)
        {
            TraceProvider.WriteLine("Profile={0}, Identity:({1}, {2})",
                cloudBusinessProfile.Name,
                serverBusinessIdentity.Qualifier,
                serverBusinessIdentity.Value);
            cloudBusinessIdentity = cloudBusinessProfile.BusinessIdentities.SingleOrDefault(id => AreBusinessIdentitiesEquivalent(id, serverBusinessIdentity));
            return cloudBusinessIdentity != null;
        }

        private static bool AreBusinessIdentitiesEquivalent(Services.BusinessIdentity cloudBusinessIdentity, Server.QualifierIdentity serverBusinessIdentity)
        {
            var cloudQualifierIdentity = cloudBusinessIdentity as Services.QualifierIdentity;
            return cloudQualifierIdentity.Value == serverBusinessIdentity.Value
                   && cloudQualifierIdentity.Qualifier == serverBusinessIdentity.Qualifier;
        }

        private void LinkBusinessProfilesToOnewayAgreement(
            Services.TpmContext cloudContext,
            Services.OnewayAgreement cloudOnewayAgreement,
            Services.BusinessProfile agreementBusinessProfileA,
            Services.BusinessProfile agreementBusinessProfileB,
            Server.QualifierIdentity serverAgreementProfileAIdentity,
            Server.QualifierIdentity serverAgreementProfileBIdentity,
            string onewayAgreementType)
        {
            Services.BusinessProfile senderProfile, receiverProfile;
            if (onewayAgreementType == "OnewayAgreementAToB")
            {
                senderProfile = agreementBusinessProfileA;
                receiverProfile = agreementBusinessProfileB;
            }
            else
            {
                senderProfile = agreementBusinessProfileB;
                receiverProfile = agreementBusinessProfileA;
            }

            Services.BusinessIdentity cloudSenderBusinessIdentity, cloudReceiverBusinessIdentity;
            if (!TryGetCloudBusinessIdentity(senderProfile, serverAgreementProfileAIdentity, out cloudSenderBusinessIdentity)
                || !TryGetCloudBusinessIdentity(receiverProfile, serverAgreementProfileBIdentity, out cloudReceiverBusinessIdentity))
            {
                throw new TpmMigrationException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Business identities do not exist: {0}, {1}; {2}, {3}",
                    serverAgreementProfileAIdentity.Qualifier,
                    serverAgreementProfileAIdentity.Value,
                    serverAgreementProfileBIdentity.Qualifier,
                    serverAgreementProfileBIdentity.Value));
            }

            cloudContext.RelateEntities(cloudOnewayAgreement, cloudSenderBusinessIdentity, "SenderBusinessIdentity", "OnewayAgreementSender", Services.RelationshipCardinality.ManyToOne);
            cloudContext.RelateEntities(cloudOnewayAgreement, cloudReceiverBusinessIdentity, "ReceiverBusinessIdentity", "OnewayAgreementReceiver", Services.RelationshipCardinality.ManyToOne);
        }
    }
}