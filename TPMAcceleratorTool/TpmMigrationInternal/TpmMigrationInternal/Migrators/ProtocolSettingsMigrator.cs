//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;

    class ProtocolSettingsMigrator
    {
        private EnvelopeOverridesMigrator envelopeOverridesMigrator;
        private ValidationOverridesMigrator validationOverridesMigrator;

        private IApplicationContext applicationContext;

        private string signingThumbprintMissing;

        private bool isMessageEncrypted;

        private string senderProfileName;

        private string currentAgreementName;

        public ProtocolSettingsMigrator(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
            signingThumbprintMissing = string.Empty;
            isMessageEncrypted = false;
            currentAgreementName = string.Empty;
            senderProfileName = string.Empty;
        }

        public void MigrateProtocolSettings(Services.TpmContext cloudContext, Services.OnewayAgreement cloudOnewayAgreement, Server.ProtocolSettings serverProtocolSettings, Services.BusinessProfile senderProfile, string agreementName, out MigrationStatus migrationStatus)
        {
            this.envelopeOverridesMigrator = new EnvelopeOverridesMigrator(cloudContext);
            this.validationOverridesMigrator = new ValidationOverridesMigrator(cloudContext);
            Services.ProtocolSettings cloudProtocolSettings;
            switch (serverProtocolSettings.ProtocolName)
            {
                case AppConstants.X12ProtocolName:
                    cloudProtocolSettings = CreateX12ProtocolSettings((Server.X12ProtocolSettings)serverProtocolSettings);
                    migrationStatus = MigrationStatus.Succeeded;
                    break;
                case AppConstants.AS2ProtocolName:
                    cloudProtocolSettings = CreateAS2ProtocolSettings((Server.AS2ProtocolSettings)serverProtocolSettings);
                    CleanAs2ProtocolSettings((Services.AS2ProtocolSettings)cloudProtocolSettings, senderProfile);
                    this.UpdateStatus(agreementName, out migrationStatus);
                    break;
                case AppConstants.EdifactProtocolName:
                    cloudProtocolSettings = CreateEdifactProtocolSettings((Server.EDIFACTProtocolSettings)serverProtocolSettings);
                    migrationStatus = MigrationStatus.Succeeded;
                    break;
                default:
                    throw new NotSupportedException();
            }

            cloudContext.AddToProtocolSettings(cloudProtocolSettings);
            cloudProtocolSettings.OnewayAgreement = cloudOnewayAgreement;
            cloudContext.SetLink(cloudProtocolSettings, "OnewayAgreement", cloudOnewayAgreement);
            cloudOnewayAgreement.ProtocolSettings = cloudProtocolSettings;
            cloudContext.SetLink(cloudOnewayAgreement, "ProtocolSettings", cloudProtocolSettings);

            var serverX12ProtocolSettings = serverProtocolSettings as Server.X12ProtocolSettings;
            var cloudX12ProtocolSettings = cloudProtocolSettings as Services.X12ProtocolSettings;
            if (serverX12ProtocolSettings != null)
            {
                this.envelopeOverridesMigrator.MigrateAllX12EnvelopeOverrides(serverX12ProtocolSettings, cloudX12ProtocolSettings);
                this.validationOverridesMigrator.MigrateAllX12ValidationOverrides(serverX12ProtocolSettings, cloudX12ProtocolSettings);
            }

            var serverEdifactProtocolSettings = serverProtocolSettings as Server.EDIFACTProtocolSettings;
            var cloudEdifactProtocolSettings = cloudProtocolSettings as Services.EDIFACTProtocolSettings;
            if (serverEdifactProtocolSettings != null)
            {
                this.envelopeOverridesMigrator.MigrateAllEdifactEnvelopeOverrides(serverEdifactProtocolSettings, cloudEdifactProtocolSettings);
                this.validationOverridesMigrator.MigrateAllEdifactValidationOverrides(serverEdifactProtocolSettings, cloudEdifactProtocolSettings);
            }
        }

        public void UpdateStatus(string agreementName, out MigrationStatus migrationStatus)
        {
            migrationStatus = MigrationStatus.Succeeded;
            var statusBarViewModel = this.applicationContext.GetService<StatusBarViewModel>();
            Debug.Assert(statusBarViewModel != null, "StatusBarViewModel has not been initialized in application context");

            string statusBarMessage = string.Empty;

            if (this.isMessageEncrypted == true)
            {
                statusBarMessage += string.Format(CultureInfo.InvariantCulture, "Message Encryption settings are not migrated for sender {0}", senderProfileName) + Environment.NewLine;
                migrationStatus = MigrationStatus.Partial;
            }

            if (this.signingThumbprintMissing != null && this.signingThumbprintMissing.Length > 0)
            {
                statusBarMessage += string.Format(CultureInfo.InvariantCulture, "Certificate with thumbprint {0} missing in the Sender's {1} profile ", this.signingThumbprintMissing, senderProfileName);
                migrationStatus = MigrationStatus.Partial;
            }

            if (statusBarMessage.Length > 0)
            {
                if (agreementName != currentAgreementName)
                {
                    statusBarViewModel.ShowWarning(Environment.NewLine + agreementName + " (Warning) : " + Environment.NewLine + statusBarMessage);
                    currentAgreementName = agreementName;
                    TraceProvider.WriteLine(statusBarMessage);
                }
                else
                {
                    statusBarViewModel.ShowWarning(Environment.NewLine + statusBarMessage);
                    TraceProvider.WriteLine(statusBarMessage);
                }
            }

            this.isMessageEncrypted = false;
            this.signingThumbprintMissing = string.Empty;
        }

        /*
        Function verifies if the AS2 message is signed and the required certificate is present in the Sender's profile in Services world.
        If the certificate is present, it will assign the certificate ID to the Services certificateId field.
        If the certificate is not present, the migration will still succeed showing a warning message displaying the Agreement name and missing thumbprint.
        */
        private void CleanAs2ProtocolSettings(Services.AS2ProtocolSettings cloudProtocolSettings, Services.BusinessProfile senderProfile)
        {
            if (cloudProtocolSettings.MessageSigned == true && cloudProtocolSettings.SigningCertificateId == null)
            {
                var certificateReference = senderProfile.CertificateReferences.Where(cert => cert.Thumbprint == cloudProtocolSettings.SigningCertificateThumbprint).FirstOrDefault();
                if (certificateReference == null)
                {
                    cloudProtocolSettings.OverrideGroupSigningCertificate = false;
                    cloudProtocolSettings.MessageSigned = false;
                    signingThumbprintMissing = cloudProtocolSettings.SigningCertificateThumbprint;
                    cloudProtocolSettings.SigningCertificateThumbprint = null;
                }
                else
                {
                    cloudProtocolSettings.SigningCertificateId = certificateReference.Id;
                }
            }

            senderProfileName = senderProfile.Name;
        }

        private Services.X12ProtocolSettings CreateX12ProtocolSettings(Server.X12ProtocolSettings serverProtocolSettings)
        {
            Services.X12ProtocolSettings cloudProtocolSettings = new Services.X12ProtocolSettings();
            cloudProtocolSettings.AcknowledgementControlNumberLowerBound = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberLowerBound;
            cloudProtocolSettings.AcknowledgementControlNumberPrefix = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberPrefix;
            cloudProtocolSettings.AcknowledgementControlNumberRollover = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberRollover;
            cloudProtocolSettings.AcknowledgementControlNumberSuffix = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberSuffix;
            cloudProtocolSettings.AcknowledgementControlNumberUpperBound = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberUpperBound;

            cloudProtocolSettings.AllowLeadingAndTrailingSpacesAndZeroes = serverProtocolSettings.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes;
            cloudProtocolSettings.AuthorizationQualifier = serverProtocolSettings.SecuritySettings.AuthorizationQualifier;
            cloudProtocolSettings.AuthorizationValue = serverProtocolSettings.SecuritySettings.AuthorizationValue;
            cloudProtocolSettings.SecurityQualifier = serverProtocolSettings.SecuritySettings.SecurityQualifier;
            if (serverProtocolSettings.SecuritySettings.PasswordValue != null)
            {
                cloudProtocolSettings.SecurityValue = serverProtocolSettings.SecuritySettings.PasswordValue;
            }

            cloudProtocolSettings.BatchFunctionalAck = serverProtocolSettings.AcknowledgementSettings.BatchFunctionalAcknowledgements;
            cloudProtocolSettings.BatchTechnicalAck = serverProtocolSettings.AcknowledgementSettings.BatchTechnicalAcknowledgements;

            cloudProtocolSettings.CharacterSet = (short)serverProtocolSettings.FramingSettings.CharacterSet;
            cloudProtocolSettings.CheckDuplicateGroupControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateGroupControlNumber;
            cloudProtocolSettings.CheckDuplicateInterchangeControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateInterchangeControlNumber;
            cloudProtocolSettings.CheckDuplicateTransactionSetControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateTransactionSetControlNumber;
            cloudProtocolSettings.ComponentSeparator = serverProtocolSettings.FramingSettings.ComponentSeparator;
            cloudProtocolSettings.ControlStandardsId = serverProtocolSettings.EnvelopeSettings.ControlStandardsId;
            cloudProtocolSettings.ControlVersionNumber = serverProtocolSettings.EnvelopeSettings.ControlVersionNumber;
            cloudProtocolSettings.ConvertImpliedDecimal = serverProtocolSettings.ProcessingSettings.ConvertImpliedDecimal;
            cloudProtocolSettings.CreateEmptyXmlTagsForTrailingSeparators = serverProtocolSettings.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators;
            cloudProtocolSettings.DataElementSeparator = serverProtocolSettings.FramingSettings.DataElementSeparator;
            cloudProtocolSettings.EnableDefaultGroupHeaders = serverProtocolSettings.EnvelopeSettings.EnableDefaultGroupHeaders;
            cloudProtocolSettings.FunctionalGroupId = serverProtocolSettings.EnvelopeSettings.FunctionalGroupId;
            cloudProtocolSettings.GenerateLoopForValidMessagesInAck = serverProtocolSettings.AcknowledgementSettings.NeedLoopForValidMessages;
            cloudProtocolSettings.GroupControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.GroupControlNumberLowerBound;
            cloudProtocolSettings.GroupControlNumberRollover = serverProtocolSettings.EnvelopeSettings.GroupControlNumberRollover;
            cloudProtocolSettings.GroupControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.GroupControlNumberUpperBound;

            cloudProtocolSettings.GroupHeaderVersion = serverProtocolSettings.EnvelopeSettings.GroupHeaderVersion;

            cloudProtocolSettings.InterchangeControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberLowerBound;
            cloudProtocolSettings.InterchangeControlNumberRollover = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberRollover;
            cloudProtocolSettings.InterchangeControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberUpperBound;

            cloudProtocolSettings.MaskSecurityInfo = serverProtocolSettings.ProcessingSettings.MaskSecurityInfo;
            cloudProtocolSettings.MessageFilterType = (short)serverProtocolSettings.MessageFilter.MessageFilterType;
            cloudProtocolSettings.NeedFunctionalAck = serverProtocolSettings.AcknowledgementSettings.NeedFunctionalAcknowledgement;
            cloudProtocolSettings.NeedTechnicalAck = serverProtocolSettings.AcknowledgementSettings.NeedTechnicalAcknowledgement;
            cloudProtocolSettings.PreserveInterchange = serverProtocolSettings.ProcessingSettings.PreserveInterchange;
            cloudProtocolSettings.ProtocolName = serverProtocolSettings.ProtocolName;
            cloudProtocolSettings.ReceiverApplicationId = serverProtocolSettings.EnvelopeSettings.ReceiverApplicationId;
            cloudProtocolSettings.ReplaceChar = serverProtocolSettings.FramingSettings.ReplaceChar;
            cloudProtocolSettings.ReplaceSeparatorsInPayload = serverProtocolSettings.FramingSettings.ReplaceSeparatorsInPayload;

            cloudProtocolSettings.SecurityQualifier = serverProtocolSettings.SecuritySettings.SecurityQualifier;

            cloudProtocolSettings.SegmentTerminator = serverProtocolSettings.FramingSettings.SegmentTerminator;
            cloudProtocolSettings.SegmentTerminatorSuffix = (short)serverProtocolSettings.FramingSettings.SegmentTerminatorSuffix;
            cloudProtocolSettings.SenderApplicationId = serverProtocolSettings.EnvelopeSettings.SenderApplicationId;
            cloudProtocolSettings.Name = serverProtocolSettings.SettingsName;

            cloudProtocolSettings.SuspendInterchangeOnError = serverProtocolSettings.ProcessingSettings.SuspendInterchangeOnError;
            cloudProtocolSettings.TrailingSeparatorPolicy = (short)serverProtocolSettings.ValidationSettings.TrailingSeparatorPolicy;

            cloudProtocolSettings.TSControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberLowerBound;
            cloudProtocolSettings.TSControlNumberPrefix = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberPrefix;
            cloudProtocolSettings.TSControlNumberRollover = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberRollover;
            cloudProtocolSettings.TSControlNumberSuffix = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberSuffix;
            cloudProtocolSettings.TSControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberUpperBound;
            cloudProtocolSettings.UsageIndicator = (short)serverProtocolSettings.EnvelopeSettings.UsageIndicator;
            cloudProtocolSettings.UseControlStandardsIdAsRepSep = serverProtocolSettings.EnvelopeSettings.UseControlStandardsIdAsRepetitionCharacter;
            cloudProtocolSettings.UseDotAsDecimalSeparator = serverProtocolSettings.ProcessingSettings.UseDotAsDecimalSeparator;
            cloudProtocolSettings.ValidateCharacterSet = serverProtocolSettings.ValidationSettings.ValidateCharacterSet;
            cloudProtocolSettings.ValidateEDITypes = serverProtocolSettings.ValidationSettings.ValidateEDITypes;
            cloudProtocolSettings.ValidateExtended = serverProtocolSettings.ValidationSettings.ValidateXSDTypes;

            cloudProtocolSettings.NeedFunctionalAck = serverProtocolSettings.AcknowledgementSettings.NeedFunctionalAcknowledgement;
            if (cloudProtocolSettings.NeedFunctionalAck == true)
            {
                cloudProtocolSettings.FunctionalAckVersion = "00401";
            }
            else
            {
                cloudProtocolSettings.FunctionalAckVersion = null;
            }

            cloudProtocolSettings.FunctionalAckVersion = null;
            cloudProtocolSettings.NeedTechnicalAck = serverProtocolSettings.AcknowledgementSettings.NeedTechnicalAcknowledgement;
            cloudProtocolSettings.ImplementationAckVersion = null;
            cloudProtocolSettings.NeedImplementationAck = false;
            cloudProtocolSettings.BatchImplementationAck = false;

            return cloudProtocolSettings;
        }

        private Services.EDIFACTProtocolSettings CreateEdifactProtocolSettings(Server.EDIFACTProtocolSettings serverProtocolSettings)
        {
            Services.EDIFACTProtocolSettings cloudProtocolSettings = new Services.EDIFACTProtocolSettings();

            cloudProtocolSettings.AcknowledgementControlNumberLowerBound = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberLowerBound;
            cloudProtocolSettings.AcknowledgementControlNumberPrefix = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberPrefix;
            cloudProtocolSettings.AcknowledgementControlNumberRollover = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberRollover;
            cloudProtocolSettings.AcknowledgementControlNumberSuffix = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberSuffix;
            cloudProtocolSettings.AcknowledgementControlNumberUpperBound = serverProtocolSettings.AcknowledgementSettings.AcknowledgementControlNumberUpperBound;

            cloudProtocolSettings.AllowLeadingAndTrailingSpacesAndZeroes = serverProtocolSettings.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes;
            cloudProtocolSettings.ApplicationReferenceId = serverProtocolSettings.EnvelopeSettings.ApplicationReferenceId;
            cloudProtocolSettings.ApplyDelimiterStringAdvice = serverProtocolSettings.EnvelopeSettings.ApplyDelimiterStringAdvice;
            cloudProtocolSettings.BatchFunctionalAck = serverProtocolSettings.AcknowledgementSettings.BatchFunctionalAcknowledgements;
            cloudProtocolSettings.BatchTechnicalAck = serverProtocolSettings.AcknowledgementSettings.BatchTechnicalAcknowledgements;
            cloudProtocolSettings.CharacterSet = (short)serverProtocolSettings.FramingSettings.CharacterSet;

            cloudProtocolSettings.CheckDuplicateGroupControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateGroupControlNumber;
            cloudProtocolSettings.CheckDuplicateInterchangeControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateInterchangeControlNumber;
            cloudProtocolSettings.CheckDuplicateTransactionSetControlNumber = serverProtocolSettings.ValidationSettings.CheckDuplicateTransactionSetControlNumber;

            cloudProtocolSettings.CommunicationAgreementId = serverProtocolSettings.EnvelopeSettings.CommunicationAgreementId;
            cloudProtocolSettings.ComponentSeparator = serverProtocolSettings.FramingSettings.ComponentSeparator;
            cloudProtocolSettings.CreateEmptyXmlTagsForTrailingSeparators = serverProtocolSettings.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators;
            cloudProtocolSettings.CreateGroupingSegments = serverProtocolSettings.EnvelopeSettings.CreateGroupingSegments;
            cloudProtocolSettings.DataElementSeparator = serverProtocolSettings.FramingSettings.DataElementSeparator;
            cloudProtocolSettings.DecimalPointIndicator = (short)serverProtocolSettings.FramingSettings.DecimalPointIndicator;
            cloudProtocolSettings.EnableDefaultGroupHeaders = serverProtocolSettings.EnvelopeSettings.EnableDefaultGroupHeaders;
            cloudProtocolSettings.FunctionalGroupId = serverProtocolSettings.EnvelopeSettings.FunctionalGroupId;
            cloudProtocolSettings.GenerateLoopForValidMessagesInAck = serverProtocolSettings.AcknowledgementSettings.NeedLoopForValidMessages;

            cloudProtocolSettings.GroupApplicationReceiverId = serverProtocolSettings.EnvelopeSettings.GroupApplicationReceiverId;
            cloudProtocolSettings.GroupApplicationReceiverQualifier = serverProtocolSettings.EnvelopeSettings.GroupApplicationReceiverQualifier;
            cloudProtocolSettings.GroupApplicationSenderId = serverProtocolSettings.EnvelopeSettings.GroupApplicationSenderId;
            cloudProtocolSettings.GroupApplicationSenderQualifier = serverProtocolSettings.EnvelopeSettings.GroupApplicationSenderQualifier;
            cloudProtocolSettings.GroupAssociationAssignedCode = serverProtocolSettings.EnvelopeSettings.GroupAssociationAssignedCode;
            cloudProtocolSettings.GroupControllingAgencyCode = serverProtocolSettings.EnvelopeSettings.GroupControllingAgencyCode;
            cloudProtocolSettings.GroupControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.GroupControlNumberLowerBound;
            cloudProtocolSettings.GroupControlNumberPrefix = serverProtocolSettings.EnvelopeSettings.GroupControlNumberPrefix;
            cloudProtocolSettings.GroupControlNumberRollover = serverProtocolSettings.EnvelopeSettings.GroupControlNumberRollover;
            cloudProtocolSettings.GroupControlNumberSuffix = serverProtocolSettings.EnvelopeSettings.GroupControlNumberSuffix;
            cloudProtocolSettings.GroupControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.GroupControlNumberUpperBound;
            cloudProtocolSettings.GroupMessageRelease = serverProtocolSettings.EnvelopeSettings.GroupMessageRelease;
            cloudProtocolSettings.GroupMessageVersion = serverProtocolSettings.EnvelopeSettings.GroupMessageVersion;

            cloudProtocolSettings.InterchangeControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberLowerBound;
            cloudProtocolSettings.InterchangeControlNumberPrefix = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberPrefix;
            cloudProtocolSettings.InterchangeControlNumberRollover = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberRollover;
            cloudProtocolSettings.InterchangeControlNumberSuffix = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberSuffix;
            cloudProtocolSettings.InterchangeControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.InterchangeControlNumberUpperBound;

            cloudProtocolSettings.IsTestInterchange = serverProtocolSettings.IsTestInterchange;
            cloudProtocolSettings.MaskSecurityInfo = serverProtocolSettings.ProcessingSettings.MaskSecurityInfo;
            cloudProtocolSettings.MessageFilterType = (short)serverProtocolSettings.MessageFilter.MessageFilterType;
            cloudProtocolSettings.NeedFunctionalAck = serverProtocolSettings.AcknowledgementSettings.NeedFunctionalAcknowledgement;
            cloudProtocolSettings.NeedTechnicalAck = serverProtocolSettings.AcknowledgementSettings.NeedTechnicalAcknowledgement;
            cloudProtocolSettings.PreserveInterchange = serverProtocolSettings.ProcessingSettings.PreserveInterchange;
            cloudProtocolSettings.ProcessingPriorityCode = serverProtocolSettings.EnvelopeSettings.ProcessingPriorityCode;
            cloudProtocolSettings.ProtocolName = serverProtocolSettings.ProtocolName;
            cloudProtocolSettings.ProtocolVersion = serverProtocolSettings.FramingSettings.ProtocolVersion;
            cloudProtocolSettings.ReceiverReverseRoutingAddress = serverProtocolSettings.EnvelopeSettings.ReceiverReverseRoutingAddress;

            cloudProtocolSettings.ReleaseIndicator = serverProtocolSettings.FramingSettings.ReleaseIndicator;
            cloudProtocolSettings.RepetitionSeparator = serverProtocolSettings.FramingSettings.RepetitionSeparator;
            cloudProtocolSettings.RecipientReferencePasswordValue = serverProtocolSettings.EnvelopeSettings.RecipientReferencePasswordValue;
            cloudProtocolSettings.RecipientReferencePasswordQualifier = serverProtocolSettings.EnvelopeSettings.RecipientReferencePasswordQualifier;
            cloudProtocolSettings.SegmentTerminator = serverProtocolSettings.FramingSettings.SegmentTerminator;
            cloudProtocolSettings.SegmentTerminatorSuffix = (short)serverProtocolSettings.FramingSettings.SegmentTerminatorSuffix;
            cloudProtocolSettings.SenderReverseRoutingAddress = serverProtocolSettings.EnvelopeSettings.SenderReverseRoutingAddress;

            cloudProtocolSettings.SuspendInterchangeOnError = serverProtocolSettings.ProcessingSettings.SuspendInterchangeOnError;
            cloudProtocolSettings.TrailingSeparatorPolicy = (short)serverProtocolSettings.ValidationSettings.TrailingSeparatorPolicy;

            cloudProtocolSettings.TSApplyNewId = serverProtocolSettings.EnvelopeSettings.OverwriteExistingTransactionSetControlNumber;
            cloudProtocolSettings.TSControlNumberLowerBound = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberLowerBound;
            cloudProtocolSettings.TSControlNumberPrefix = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberPrefix;
            cloudProtocolSettings.TSControlNumberRollover = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberRollover;
            cloudProtocolSettings.TSControlNumberSuffix = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberSuffix;
            cloudProtocolSettings.TSControlNumberUpperBound = serverProtocolSettings.EnvelopeSettings.TransactionSetControlNumberUpperBound;

            cloudProtocolSettings.UseDotAsDecimalSeparator = serverProtocolSettings.ProcessingSettings.UseDotAsDecimalSeparator;

            cloudProtocolSettings.ValidateEDITypes = serverProtocolSettings.ValidationSettings.ValidateEDITypes;
            cloudProtocolSettings.ValidateXSDTypes = serverProtocolSettings.ValidationSettings.ValidateXSDTypes;

            return cloudProtocolSettings;
        }

        private Services.AS2ProtocolSettings CreateAS2ProtocolSettings(Server.AS2ProtocolSettings serverProtocolSettings)
        {
            Services.AS2ProtocolSettings cloudProtocolSettings = new Services.AS2ProtocolSettings();
            cloudProtocolSettings.AckHttpExpect100Continue = serverProtocolSettings.AcknowledgementConnectionSettings.HttpExpect100ContinueSupported;
            cloudProtocolSettings.AckIgnoreCertificateNameMismatch = serverProtocolSettings.AcknowledgementConnectionSettings.IgnoreCertificateNameMismatch;
            cloudProtocolSettings.AckKeepHttpConnectionAlive = serverProtocolSettings.AcknowledgementConnectionSettings.KeepHttpConnectionAlive;
            cloudProtocolSettings.AckUnfoldHttpHeaders = serverProtocolSettings.AcknowledgementConnectionSettings.UnfoldHttpHeaders;

            cloudProtocolSettings.AutogenerateFileName = serverProtocolSettings.EnvelopeSettings.AutogenerateFileName;

            cloudProtocolSettings.CheckCertificateRevocationListOnReceive = serverProtocolSettings.ValidationSettings.CheckCertificateRevocationListOnReceive;
            cloudProtocolSettings.CheckCertificateRevocationListOnSend = serverProtocolSettings.ValidationSettings.CheckCertificateRevocationListOnSend;
            cloudProtocolSettings.CheckDuplicateMessage = serverProtocolSettings.ValidationSettings.CheckDuplicateMessage;
            cloudProtocolSettings.DispositionNotificationTo = serverProtocolSettings.MDNSettings.DispositionNotificationTo;
            cloudProtocolSettings.EnableNRRForInboundDecodedMessages = serverProtocolSettings.SecuritySettings.EnableNRRForInboundDecodedMessages;
            cloudProtocolSettings.EnableNRRForInboundEncodedMessages = serverProtocolSettings.SecuritySettings.EnableNRRForInboundEncodedMessages;
            cloudProtocolSettings.EnableNRRForInboundMDN = serverProtocolSettings.SecuritySettings.EnableNRRForInboundMDN;
            cloudProtocolSettings.EnableNRRForOutboundDecodedMessages = serverProtocolSettings.SecuritySettings.EnableNRRForOutboundDecodedMessages;
            cloudProtocolSettings.EnableNRRForOutboundEncodedMessages = serverProtocolSettings.SecuritySettings.EnableNRRForOutboundEncodedMessages;
            cloudProtocolSettings.EnableNRRForOutboundMDN = serverProtocolSettings.SecuritySettings.EnableNRRForOutboundMDN;
            cloudProtocolSettings.EncryptionAlgorithm = (short)serverProtocolSettings.ValidationSettings.EncryptionAlgorithm;

            cloudProtocolSettings.FileNameTemplate = serverProtocolSettings.EnvelopeSettings.FileNameTemplate;
            cloudProtocolSettings.HttpExpect100Continue = serverProtocolSettings.MessageConnectionSettings.HttpExpect100ContinueSupported;

            cloudProtocolSettings.IgnoreCertificateNameMismatch = serverProtocolSettings.MessageConnectionSettings.IgnoreCertificateNameMismatch;
            cloudProtocolSettings.InterchangeDuplicatesValidity = serverProtocolSettings.ValidationSettings.InterchangeDuplicatesValidity;
            cloudProtocolSettings.KeepHttpConnectionAlive = serverProtocolSettings.MessageConnectionSettings.KeepHttpConnectionAlive;
            cloudProtocolSettings.MaximumHttpRetryAttempts = serverProtocolSettings.ErrorSettings.MaximumHttpRetryAttempts;
            cloudProtocolSettings.MaxResendAttempts = serverProtocolSettings.ErrorSettings.MaximumResendAttempts;
            cloudProtocolSettings.MDNText = serverProtocolSettings.MDNSettings.MDNText;
            cloudProtocolSettings.MessageCompressed = serverProtocolSettings.ValidationSettings.MessageCompressed;
            cloudProtocolSettings.MessageContentType = serverProtocolSettings.EnvelopeSettings.MessageContentType;
            cloudProtocolSettings.MessageEncrypted = serverProtocolSettings.ValidationSettings.MessageEncrypted;
            if (cloudProtocolSettings.MessageEncrypted == true)
            {
                cloudProtocolSettings.MessageEncrypted = false;
                cloudProtocolSettings.EncryptionCertificateId = null;
                cloudProtocolSettings.EncryptionCertificateThumbprint = null;
                isMessageEncrypted = true;
            }

            cloudProtocolSettings.MessageSigned = serverProtocolSettings.ValidationSettings.MessageSigned;
            if (cloudProtocolSettings.MessageSigned == true)
            {
                cloudProtocolSettings.SigningCertificateId = null;
                cloudProtocolSettings.SigningCertificateThumbprint = serverProtocolSettings.SecuritySettings.SigningCertificateThumbprint;
            }

            cloudProtocolSettings.MicHashingAlgorithm = (short)serverProtocolSettings.MDNSettings.MicHashingAlgorithm;
            cloudProtocolSettings.NeedMDN = serverProtocolSettings.MDNSettings.NeedMDN;

            cloudProtocolSettings.OverrideGroupSigningCertificate = serverProtocolSettings.SecuritySettings.OverrideGroupSigningCertificate;
            cloudProtocolSettings.OverrideMessageProperties = serverProtocolSettings.ValidationSettings.OverrideMessageProperties;
            cloudProtocolSettings.OverrideSendPort = serverProtocolSettings.ErrorSettings.OverrideSendPort;
            cloudProtocolSettings.ProcessMDNtoMsgBox = serverProtocolSettings.MDNSettings.SendInboundMDNToMessageBox;
            cloudProtocolSettings.ProtocolName = serverProtocolSettings.ProtocolName;
            if (serverProtocolSettings.MDNSettings.ReceiptDeliveryUrl != null)
            {
                cloudProtocolSettings.ReceiptDeliveryUrl = serverProtocolSettings.MDNSettings.ReceiptDeliveryUrl.ToString();
            }

            cloudProtocolSettings.ResendIfMDNNotReceived = serverProtocolSettings.ErrorSettings.ResendIfMDNNotReceived;

            cloudProtocolSettings.SendMDNAsynchronously = serverProtocolSettings.MDNSettings.SendMDNAsynchronously;
            cloudProtocolSettings.SignMDN = serverProtocolSettings.MDNSettings.SignMDN;
            cloudProtocolSettings.SignOutboundMDNIfOptional = serverProtocolSettings.MDNSettings.SignOutboundMDNIfOptional;
            cloudProtocolSettings.SuspendDuplicateMessage = serverProtocolSettings.ErrorSettings.SuspendDuplicateMessage;
            cloudProtocolSettings.SuspendMessageOnFileNameGenerationError = serverProtocolSettings.EnvelopeSettings.SuspendMessageOnFileNameGenerationError;
            cloudProtocolSettings.TransmitFileNameInMimeHeader = serverProtocolSettings.EnvelopeSettings.TransmitFileNameInMimeHeader;
            cloudProtocolSettings.UnfoldHttpHeaders = serverProtocolSettings.MessageConnectionSettings.UnfoldHttpHeaders;

            return cloudProtocolSettings;
        }
    }
}