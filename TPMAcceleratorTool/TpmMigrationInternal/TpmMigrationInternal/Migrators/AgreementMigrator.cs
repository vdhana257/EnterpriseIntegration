//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Data.Services.Client;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Collections.Generic;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;
    using System.Net.Http;
    using Common;
    using System.Text;
    using System.Configuration;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System.Xml;
    using System.IO;
    using System.Threading;
    using System.Windows;
    using BizTalk.ExplorerOM;
    using IdentityModel.Clients.ActiveDirectory;
    using System.Text.RegularExpressions;
    using System.Data.SqlClient;

    class AgreementMigrator : TpmMigrator<AgreementMigrationItemViewModel, Server.Agreement>
    {
        private CustomSettingsMigrator customSettingsMigrator;
        private OnewayAgreementMigrator onewayAgreementMigrator;
        private IApplicationContext thisApplicationContext;
        private IntegrationAccountDetails iaDetails;
        public Dictionary<string, string> schemasInIA = new Dictionary<string, string>();
        public Dictionary<KeyValuePair<string, string>, string> partnerIdentitiesInIA = new Dictionary<KeyValuePair<string, string>, string>();
        public static Dictionary<string, KeyValuePair<string, string>> partnerCertificateMappings = new Dictionary<string, KeyValuePair<string, string>>();
        public static Dictionary<string, string> hashingAlgorithmMappings = new Dictionary<string, string>();


        public AgreementMigrator(IApplicationContext applicationContext)
            : base(applicationContext)
        {
            this.onewayAgreementMigrator = new OnewayAgreementMigrator(applicationContext);
            this.customSettingsMigrator = new CustomSettingsMigrator();
            this.thisApplicationContext = applicationContext;
        }

        static AgreementMigrator()
        {
            try
            {
                partnerCertificateMappings = FileOperations.ReadPartnerCertificateMappingFile();
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error while reading the Partner Certificate Mapping File {0}", ExceptionHelper.GetExceptionMessage(ex)));
                MessageBox.Show(string.Format("Error while reading the Partner Certificate Mapping File {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
            try
            {
                hashingAlgorithmMappings = Mappings.hashingAlgorithmMappings;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error while reading the Hashing Alogrithms Mappings {0}", ExceptionHelper.GetExceptionMessage(ex)));
                MessageBox.Show(string.Format("Error while reading the Hashing Alogrithms Mappings {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }


        public override async Task ImportAsync(AgreementMigrationItemViewModel serverAgreementItem)
        {
            this.iaDetails = thisApplicationContext.GetService<IntegrationAccountDetails>() as IntegrationAccountDetails;
            try
            {
                TraceProvider.WriteLine();
                TraceProvider.WriteLine("Exporting agreement to Json: {0}", serverAgreementItem.Name);

                serverAgreementItem.ImportStatus = MigrationStatus.NotStarted;
                serverAgreementItem.ExportStatus = MigrationStatus.NotStarted;
                serverAgreementItem.ImportStatusText = null;
                serverAgreementItem.ExportStatusText = null;
                var serverAgreement = serverAgreementItem.MigrationEntity;

                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        this.LAAgreementProcess(serverAgreementItem).Wait();
                        serverAgreementItem.ImportStatus = MigrationStatus.Succeeded;
                        StringBuilder successMessageText = new StringBuilder();
                        successMessageText.Append(string.Format(Resources.ImportSuccessMessageText, serverAgreementItem.Name));
                        serverAgreementItem.ImportStatusText = successMessageText.ToString();
                        TraceProvider.WriteLine("Agreement Export to Json Successfull: {0}", serverAgreementItem.Name);
                        TraceProvider.WriteLine();

                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }
                });
                // Persist all the above local changes
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        #region AgreementMigrationTOLA

        public async Task LAAgreementProcess(AgreementMigrationItemViewModel agreement)
        {
            try
            {
                if (Convert.ToBoolean(thisApplicationContext.GetProperty(AppConstants.ContextGenerationEnabled)))
                {
                    DocumentDbClass docDb = new DocumentDbClass();
                    docDb.QueryAllDocuments();
                }
                if (agreement.MigrationEntity.Protocol == AppConstants.X12ProtocolName)
                {
                    if (agreement.IsConsolidated == false && agreement.AgreementsToConsolidate == null)
                    {
                        X12AgreementJson.Rootobject x12Agreement = X12AgreementJsonProcess(agreement).Result;
                    }
                    else
                    {
                        X12AgreementJson.Rootobject x12Agreement = ConsolidateX12Agreements(agreement);
                    }
                }
                else if (agreement.MigrationEntity.Protocol == AppConstants.EdifactProtocolName)
                {
                    if (agreement.IsConsolidated == false && agreement.AgreementsToConsolidate == null)
                    {
                        EDIFACTAgreementJson.Rootobject edifactAgreement = EDIFACTAgreementProcess(agreement).Result;
                    }
                    else
                    {
                        EDIFACTAgreementJson.Rootobject edifactAgreement = ConsolidateEdifactAgreements(agreement);

                    }
                }
                else if (agreement.MigrationEntity.Protocol == AppConstants.AS2ProtocolName)
                {
                    JsonAs2Agreement.Rootobject edifactAgreement = As2AgreementProcess(agreement).Result;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion


        #region AS2AgreementProcess
        public async Task<JsonAs2Agreement.Rootobject> As2AgreementProcess(AgreementMigrationItemViewModel agreementItem)
        {
            var agreement = agreementItem.MigrationEntity;
            string agreementName = FileOperations.GetFileName(agreement.Name);
            string type = Resources.JsonPartnerAgreementType;
            var bizTalkTpmContext = this.thisApplicationContext.GetBizTalkServerTpmContext();

            JsonAs2Agreement.Rootobject as2rootObject = null;
            try
            {
                string senderDetailsPartner = agreement.SenderDetails.Partner;
                string receiverDetailsParnter = agreement.ReceiverDetails.Partner;

                Server.OnewayAgreement onewayAgreementAB = agreement.GetOnewayAgreement(senderDetailsPartner, receiverDetailsParnter);//receive agreement
                Server.AS2ProtocolSettings as2protocolsettingsAB = onewayAgreementAB.GetProtocolSettings<Server.AS2ProtocolSettings>();

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABreceiverQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABsenderQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.SenderIdentity;

                Server.OnewayAgreement onewayAgreementBA = agreement.GetOnewayAgreement(receiverDetailsParnter, senderDetailsPartner);//sender agreement
                Server.AS2ProtocolSettings as2protocolsettingsBA = onewayAgreementBA.GetProtocolSettings<Server.AS2ProtocolSettings>();//sender agreement

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAreceiverQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAsenderQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.SenderIdentity;

                #region Func

                Func<string, string> ReceiptDeliveryUrl = (string url) =>
                {
                    string result = "";
                    if (string.IsNullOrEmpty(url))
                    {
                        result = "http://localhost";
                    }
                    else
                    {
                        result = url;
                    }
                    return result;
                };

                #endregion


                #region As2rootObject
                string hostCertName = GetHostCertificateName(onewayAgreementBA, agreement.ReceiverDetails.Partner, bizTalkTpmContext);
                string guestCertName = GetGuestCertificateName(onewayAgreementAB, onewayAgreementBA, agreement.SenderDetails.Partner, bizTalkTpmContext);
                string guestSignCertName = guestCertName;
                string hostEncryptCertName = hostCertName;
                string hostSignCertName = hostCertName;
                string guestEncryptCertName = guestCertName;

                as2rootObject = new JsonAs2Agreement.Rootobject()
                {
                    properties = new JsonAs2Agreement.Properties()
                    {
                        hostPartner = FileOperations.GetFileName(agreement.ReceiverDetails.Partner)
                       ,
                        guestPartner = FileOperations.GetFileName(agreement.SenderDetails.Partner)
                       ,
                        hostIdentity = new JsonAs2Agreement.Hostidentity { qualifier = onewayAgreementABreceiverQualifierIdentity.Qualifier, value = onewayAgreementABreceiverQualifierIdentity.Value }
                       ,
                        guestIdentity = new JsonAs2Agreement.Guestidentity { qualifier = onewayAgreementABsenderQualifierIdentity.Qualifier, value = onewayAgreementABsenderQualifierIdentity.Value }
                       ,
                        agreementType = agreement.Protocol
                       ,
                        content = new JsonAs2Agreement.Content()
                        {
                            #region Content
                            aS2 = new JsonAs2Agreement.As2()
                            {
                                #region ReceiveAgreement
                                receiveAgreement = new JsonAs2Agreement.Receiveagreement()
                                {
                                    #region ProtocolSettings
                                    protocolSettings = new JsonAs2Agreement.Protocolsettings()
                                    {
                                        messageConnectionSettings = new JsonAs2Agreement.Messageconnectionsettings()
                                        {
                                            ignoreCertificateNameMismatch = as2protocolsettingsAB.MessageConnectionSettings.IgnoreCertificateNameMismatch,
                                            supportHttpStatusCodeContinue = as2protocolsettingsAB.MessageConnectionSettings.HttpExpect100ContinueSupported,
                                            keepHttpConnectionAlive = as2protocolsettingsAB.MessageConnectionSettings.KeepHttpConnectionAlive,
                                            unfoldHttpHeaders = as2protocolsettingsAB.MessageConnectionSettings.UnfoldHttpHeaders
                                        },
                                        acknowledgementConnectionSettings = new JsonAs2Agreement.Acknowledgementconnectionsettings()
                                        {
                                            ignoreCertificateNameMismatch = as2protocolsettingsAB.AcknowledgementConnectionSettings.IgnoreCertificateNameMismatch,
                                            supportHttpStatusCodeContinue = as2protocolsettingsAB.AcknowledgementConnectionSettings.HttpExpect100ContinueSupported,
                                            keepHttpConnectionAlive = as2protocolsettingsAB.AcknowledgementConnectionSettings.KeepHttpConnectionAlive,
                                            unfoldHttpHeaders = as2protocolsettingsAB.AcknowledgementConnectionSettings.UnfoldHttpHeaders
                                        },
                                        mdnSettings = new JsonAs2Agreement.Mdnsettings()
                                        {
                                            needMDN = as2protocolsettingsAB.MDNSettings.NeedMDN,
                                            signMDN = as2protocolsettingsAB.MDNSettings.SignMDN,
                                            sendMDNAsynchronously = as2protocolsettingsAB.MDNSettings.SendMDNAsynchronously,
                                            receiptDeliveryUrl = ReceiptDeliveryUrl(Convert.ToString(as2protocolsettingsAB.MDNSettings.ReceiptDeliveryUrl)),
                                            dispositionNotificationTo = as2protocolsettingsAB.MDNSettings.DispositionNotificationTo,
                                            signOutboundMDNIfOptional = as2protocolsettingsAB.MDNSettings.SignOutboundMDNIfOptional,
                                            sendInboundMDNToMessageBox = as2protocolsettingsAB.MDNSettings.SendInboundMDNToMessageBox,
                                            micHashingAlgorithm = GetHashingAlgorithmName(Convert.ToString(as2protocolsettingsAB.MDNSettings.MicHashingAlgorithm.ToString()))
                                        },
                                        securitySettings = new JsonAs2Agreement.Securitysettings()
                                        {
                                            overrideGroupSigningCertificate = as2protocolsettingsAB.SecuritySettings.OverrideGroupSigningCertificate,
                                            enableNRRForInboundEncodedMessages = as2protocolsettingsAB.SecuritySettings.EnableNRRForInboundEncodedMessages,
                                            enableNRRForInboundDecodedMessages = as2protocolsettingsAB.SecuritySettings.EnableNRRForInboundDecodedMessages,
                                            enableNRRForOutboundMDN = as2protocolsettingsAB.SecuritySettings.EnableNRRForOutboundMDN,
                                            enableNRRForOutboundEncodedMessages = as2protocolsettingsAB.SecuritySettings.EnableNRRForOutboundEncodedMessages,
                                            enableNRRForOutboundDecodedMessages = as2protocolsettingsAB.SecuritySettings.EnableNRRForOutboundDecodedMessages,
                                            enableNRRForInboundMDN = as2protocolsettingsAB.SecuritySettings.EnableNRRForInboundMDN,
                                            signingCertificateName = guestSignCertName,
                                            encryptionCertificateName = hostEncryptCertName
                                        },
                                        validationSettings = new JsonAs2Agreement.Validationsettings()
                                        {
                                            overrideMessageProperties = as2protocolsettingsAB.ValidationSettings.OverrideMessageProperties,
                                            encryptMessage = as2protocolsettingsAB.ValidationSettings.MessageEncrypted,
                                            signMessage = as2protocolsettingsAB.ValidationSettings.MessageSigned,
                                            compressMessage = as2protocolsettingsAB.ValidationSettings.MessageCompressed,
                                            checkDuplicateMessage = as2protocolsettingsAB.ValidationSettings.CheckDuplicateMessage,
                                            interchangeDuplicatesValidityDays = as2protocolsettingsAB.ValidationSettings.InterchangeDuplicatesValidity,
                                            checkCertificateRevocationListOnSend = as2protocolsettingsAB.ValidationSettings.CheckCertificateRevocationListOnSend,
                                            checkCertificateRevocationListOnReceive = as2protocolsettingsAB.ValidationSettings.CheckCertificateRevocationListOnReceive,
                                            encryptionAlgorithm = Convert.ToString(as2protocolsettingsAB.ValidationSettings.EncryptionAlgorithm)
                                        }
                                      ,
                                        envelopeSettings = new JsonAs2Agreement.Envelopesettings()
                                        {
                                            messageContentType = as2protocolsettingsAB.EnvelopeSettings.MessageContentType,
                                            transmitFileNameInMimeHeader = as2protocolsettingsAB.EnvelopeSettings.TransmitFileNameInMimeHeader,
                                            fileNameTemplate = as2protocolsettingsAB.EnvelopeSettings.FileNameTemplate,
                                            suspendMessageOnFileNameGenerationError = as2protocolsettingsAB.EnvelopeSettings.SuspendMessageOnFileNameGenerationError,
                                            autogenerateFileName = as2protocolsettingsAB.EnvelopeSettings.AutogenerateFileName
                                        }
                                       ,
                                        errorSettings = new JsonAs2Agreement.Errorsettings()
                                        {
                                            suspendDuplicateMessage = as2protocolsettingsAB.ErrorSettings.SuspendDuplicateMessage,
                                            resendIfMDNNotReceived = as2protocolsettingsAB.ErrorSettings.ResendIfMDNNotReceived
                                        }
                                    }

                                    #endregion
                            ,
                                    #region SenderBusinessIdentity

                                    senderBusinessIdentity = new JsonAs2Agreement.Senderbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABsenderQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderQualifierIdentity.Value
                                    }
                                    #endregion
                            ,
                                    #region ReceiverBusinessIdentity
                                    receiverBusinessIdentity = new JsonAs2Agreement.Receiverbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABreceiverQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverQualifierIdentity.Value
                                    }
                                    #endregion
                                }
                                #endregion
                        ,
                                #region Sender Agreement

                                sendAgreement = new JsonAs2Agreement.Sendagreement()
                                {

                                    #region ProtocolSettings
                                    protocolSettings = new JsonAs2Agreement.Protocolsettings1()
                                    {
                                        messageConnectionSettings = new JsonAs2Agreement.Messageconnectionsettings1()
                                        {
                                            ignoreCertificateNameMismatch = as2protocolsettingsBA.MessageConnectionSettings.IgnoreCertificateNameMismatch,
                                            supportHttpStatusCodeContinue = as2protocolsettingsBA.MessageConnectionSettings.HttpExpect100ContinueSupported,
                                            keepHttpConnectionAlive = as2protocolsettingsBA.MessageConnectionSettings.KeepHttpConnectionAlive,
                                            unfoldHttpHeaders = as2protocolsettingsBA.MessageConnectionSettings.UnfoldHttpHeaders
                                        },
                                        acknowledgementConnectionSettings = new JsonAs2Agreement.Acknowledgementconnectionsettings1()
                                        {
                                            ignoreCertificateNameMismatch = as2protocolsettingsBA.AcknowledgementConnectionSettings.IgnoreCertificateNameMismatch,
                                            supportHttpStatusCodeContinue = as2protocolsettingsBA.AcknowledgementConnectionSettings.HttpExpect100ContinueSupported,
                                            keepHttpConnectionAlive = as2protocolsettingsBA.AcknowledgementConnectionSettings.KeepHttpConnectionAlive,
                                            unfoldHttpHeaders = as2protocolsettingsBA.AcknowledgementConnectionSettings.UnfoldHttpHeaders
                                        },
                                        mdnSettings = new JsonAs2Agreement.Mdnsettings1()
                                        {
                                            needMDN = as2protocolsettingsBA.MDNSettings.NeedMDN,
                                            signMDN = as2protocolsettingsBA.MDNSettings.SignMDN,
                                            sendMDNAsynchronously = as2protocolsettingsBA.MDNSettings.SendMDNAsynchronously,
                                            receiptDeliveryUrl = ReceiptDeliveryUrl(Convert.ToString(as2protocolsettingsBA.MDNSettings.ReceiptDeliveryUrl)),
                                            dispositionNotificationTo = as2protocolsettingsBA.MDNSettings.DispositionNotificationTo,
                                            signOutboundMDNIfOptional = as2protocolsettingsBA.MDNSettings.SignOutboundMDNIfOptional,
                                            sendInboundMDNToMessageBox = as2protocolsettingsBA.MDNSettings.SendInboundMDNToMessageBox,
                                            micHashingAlgorithm = GetHashingAlgorithmName(Convert.ToString(as2protocolsettingsBA.MDNSettings.MicHashingAlgorithm))
                                        },
                                        securitySettings = new JsonAs2Agreement.Securitysettings1()
                                        {
                                            overrideGroupSigningCertificate = as2protocolsettingsBA.SecuritySettings.OverrideGroupSigningCertificate,
                                            enableNRRForInboundEncodedMessages = as2protocolsettingsBA.SecuritySettings.EnableNRRForInboundEncodedMessages,
                                            enableNRRForInboundDecodedMessages = as2protocolsettingsBA.SecuritySettings.EnableNRRForInboundDecodedMessages,
                                            enableNRRForOutboundMDN = as2protocolsettingsBA.SecuritySettings.EnableNRRForOutboundMDN,
                                            enableNRRForOutboundEncodedMessages = as2protocolsettingsBA.SecuritySettings.EnableNRRForOutboundEncodedMessages,
                                            enableNRRForOutboundDecodedMessages = as2protocolsettingsBA.SecuritySettings.EnableNRRForOutboundDecodedMessages,
                                            enableNRRForInboundMDN = as2protocolsettingsBA.SecuritySettings.EnableNRRForInboundMDN,
                                            signingCertificateName = hostSignCertName,
                                            encryptionCertificateName = guestEncryptCertName
                                        },
                                        validationSettings = new JsonAs2Agreement.Validationsettings1()
                                        {
                                            overrideMessageProperties = as2protocolsettingsBA.ValidationSettings.OverrideMessageProperties,
                                            encryptMessage = as2protocolsettingsBA.ValidationSettings.MessageEncrypted,
                                            signMessage = as2protocolsettingsBA.ValidationSettings.MessageSigned,
                                            compressMessage = as2protocolsettingsBA.ValidationSettings.MessageCompressed,
                                            checkDuplicateMessage = as2protocolsettingsBA.ValidationSettings.CheckDuplicateMessage,
                                            interchangeDuplicatesValidityDays = as2protocolsettingsBA.ValidationSettings.InterchangeDuplicatesValidity,
                                            checkCertificateRevocationListOnSend = as2protocolsettingsBA.ValidationSettings.CheckCertificateRevocationListOnSend,
                                            checkCertificateRevocationListOnReceive = as2protocolsettingsBA.ValidationSettings.CheckCertificateRevocationListOnReceive,
                                            encryptionAlgorithm = Convert.ToString(as2protocolsettingsBA.ValidationSettings.EncryptionAlgorithm)
                                        }
                                      ,
                                        envelopeSettings = new JsonAs2Agreement.Envelopesettings1()
                                        {
                                            messageContentType = as2protocolsettingsBA.EnvelopeSettings.MessageContentType,
                                            transmitFileNameInMimeHeader = as2protocolsettingsBA.EnvelopeSettings.TransmitFileNameInMimeHeader,
                                            fileNameTemplate = as2protocolsettingsBA.EnvelopeSettings.FileNameTemplate,
                                            suspendMessageOnFileNameGenerationError = as2protocolsettingsBA.EnvelopeSettings.SuspendMessageOnFileNameGenerationError,
                                            autogenerateFileName = as2protocolsettingsBA.EnvelopeSettings.AutogenerateFileName
                                        }
                                       ,
                                        errorSettings = new JsonAs2Agreement.Errorsettings1()
                                        {
                                            suspendDuplicateMessage = as2protocolsettingsBA.ErrorSettings.SuspendDuplicateMessage,
                                            resendIfMDNNotReceived = as2protocolsettingsBA.ErrorSettings.ResendIfMDNNotReceived
                                        }
                                    }
                                    #endregion
                            ,
                                    #region SenderBusinessIdentity

                                    senderBusinessIdentity = new JsonAs2Agreement.Senderbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABreceiverQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverQualifierIdentity.Value
                                    }
                                    #endregion
                            ,
                                    #region ReceiverBusinessIdentity
                                    receiverBusinessIdentity = new JsonAs2Agreement.Receiverbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABsenderQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderQualifierIdentity.Value
                                    }
                                    #endregion


                                }

                                #endregion
                            }
                            #endregion
                        }
                       ,
                        createdTime = DateTime.Now
                       ,
                        changedTime = DateTime.Now
                        ,
                        metadata = GenerateMetadata(agreementName)
                    },//end properties
                    name = FileOperations.GetFileName(agreementName),
                    type = Resources.AgreementType
                };
                #endregion

                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agreementName), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(as2rootObject);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                TraceProvider.WriteLine(string.Format("Agreement {0} Exported to Json with Name {1}", agreementName, FileOperations.GetFileName(agreementName)));
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
            return as2rootObject;


        }
        #endregion

        #region EDIFACTAgreementProcess
        public async Task<EDIFACTAgreementJson.Rootobject> EDIFACTAgreementProcess(AgreementMigrationItemViewModel agreementItem)
        {
            EDIFACTAgreementJson.Rootobject EDIFACTAgreementJsonRootobject = null;
            try
            {
                var agreement = agreementItem.MigrationEntity;
                string agreementName = FileOperations.GetFileName(agreementItem.Name);
                string type = Resources.JsonPartnerAgreementType;// "Microsoft.Logic/integrationAccounts/agreements";
                string senderGuestDetailsPartner = (agreement.SenderDetails.Partner);
                string receiverHostDetailsParnter = (agreement.ReceiverDetails.Partner);

                Server.OnewayAgreement onewayAgreementAB = agreement.GetOnewayAgreement(senderGuestDetailsPartner, receiverHostDetailsParnter);//receive agreement
                Server.EDIFACTProtocolSettings protocolSettingsAB = onewayAgreementAB.GetProtocolSettings<Server.EDIFACTProtocolSettings>();

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABreceiverHostQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABsenderGuestQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.SenderIdentity;

                Server.OnewayAgreement onewayAgreementBA = agreement.GetOnewayAgreement(receiverHostDetailsParnter, senderGuestDetailsPartner);//sender agreement
                Server.EDIFACTProtocolSettings protocolSettingsBA = onewayAgreementBA.GetProtocolSettings<Server.EDIFACTProtocolSettings>();//sender agreement

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAreceiverQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAsenderQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.SenderIdentity;

                agreementItem.HostPartnerQualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier;
                agreementItem.HostPartnerId = onewayAgreementABreceiverHostQualifierIdentity.Value;
                agreementItem.GuestPartnerQualifer = onewayAgreementABsenderGuestQualifierIdentity.Qualifier;
                agreementItem.GuestPartnerId = onewayAgreementABsenderGuestQualifierIdentity.Value;
                EDIFACTAgreementJsonRootobject = new EDIFACTAgreementJson.Rootobject()
                {
                    properties = new EDIFACTAgreementJson.Properties()
                    {
                        hostPartner = FileOperations.GetFileName(receiverHostDetailsParnter),
                        guestPartner = FileOperations.GetFileName(senderGuestDetailsPartner),
                        hostIdentity = new EDIFACTAgreementJson.Hostidentity()
                        {
                            qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                            value = onewayAgreementABreceiverHostQualifierIdentity.Value
                        },
                        guestIdentity = new EDIFACTAgreementJson.Guestidentity()
                        {
                            qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                            value = onewayAgreementABsenderGuestQualifierIdentity.Value
                        },
                        agreementType = agreement.Protocol,
                        content = new EDIFACTAgreementJson.Content()
                        {
                            edifact = new EDIFACTAgreementJson.Edifact()
                            {
                                #region receiveAgreement
                                receiveAgreement = new EDIFACTAgreementJson.Receiveagreement()
                                {
                                    protocolSettings = new EDIFACTAgreementJson.Protocolsettings()
                                    {
                                        validationSettings = new EDIFACTAgreementJson.Validationsettings()
                                        {
                                            #region validationSettings
                                            validateCharacterSet = protocolSettingsAB.ValidationSettings.ValidateCharacterSet,
                                            checkDuplicateInterchangeControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateInterchangeControlNumber,
                                            interchangeControlNumberValidityDays = protocolSettingsAB.ValidationSettings.InterchangeControlNumberValidityPeriod,
                                            checkDuplicateGroupControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateGroupControlNumber,
                                            checkDuplicateTransactionSetControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateTransactionSetControlNumber,
                                            validateEDITypes = protocolSettingsAB.ValidationSettings.ValidateEDITypes,
                                            validateXSDTypes = protocolSettingsAB.ValidationSettings.ValidateXSDTypes,
                                            trimLeadingAndTrailingSpacesAndZeroes = protocolSettingsAB.ValidationSettings.TrimLeadingAndTrailingSpacesAndZeroes,
                                            allowLeadingAndTrailingSpacesAndZeroes = protocolSettingsAB.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes,
                                            trailingSeparatorPolicy = Convert.ToString(protocolSettingsAB.ValidationSettings.TrailingSeparatorPolicy)
                                            #endregion
                                        },
                                        framingSettings = new EDIFACTAgreementJson.Framingsettings()
                                        {//done
                                            #region FarmingSettings
                                            ComponentSeparator = protocolSettingsAB.FramingSettings.ComponentSeparator,
                                            CharacterEncoding = protocolSettingsAB.FramingSettings.CharacterEncoding,
                                            protocolVersion = protocolSettingsAB.FramingSettings.ProtocolVersion,
                                            dataElementSeparator = protocolSettingsAB.FramingSettings.DataElementSeparator,
                                            componentSeparator = protocolSettingsAB.FramingSettings.ComponentSeparator,
                                            segmentTerminator = protocolSettingsAB.FramingSettings.SegmentTerminator,
                                            releaseIndicator = protocolSettingsAB.FramingSettings.ReleaseIndicator,
                                            repetitionSeparator = protocolSettingsAB.FramingSettings.RepetitionSeparator,
                                            characterSet = Convert.ToString(protocolSettingsAB.FramingSettings.CharacterSet),
                                            decimalPointIndicator = Convert.ToString(protocolSettingsAB.FramingSettings.DecimalPointIndicator),
                                            segmentTerminatorSuffix = Convert.ToString(protocolSettingsAB.FramingSettings.SegmentTerminatorSuffix)
                                            #endregion
                                        },
                                        envelopeSettings = new EDIFACTAgreementJson.Envelopesettings()
                                        {
                                            #region envelopeSettings
                                            applyDelimiterStringAdvice = protocolSettingsAB.EnvelopeSettings.ApplyDelimiterStringAdvice,
                                            createGroupingSegments = protocolSettingsAB.EnvelopeSettings.CreateGroupingSegments,
                                            enableDefaultGroupHeaders = protocolSettingsAB.EnvelopeSettings.EnableDefaultGroupHeaders,
                                            interchangeControlNumberLowerBound = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberLowerBound,
                                            interchangeControlNumberUpperBound = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberUpperBound,
                                            rolloverInterchangeControlNumber = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberRollover,
                                            groupControlNumberLowerBound = protocolSettingsAB.EnvelopeSettings.GroupControlNumberLowerBound,
                                            groupControlNumberUpperBound = protocolSettingsAB.EnvelopeSettings.GroupControlNumberUpperBound,
                                            rolloverGroupControlNumber = protocolSettingsAB.EnvelopeSettings.GroupControlNumberRollover,
                                            overwriteExistingTransactionSetControlNumber = protocolSettingsAB.EnvelopeSettings.OverwriteExistingTransactionSetControlNumber,
                                            transactionSetControlNumberLowerBound = (protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberLowerBound),
                                            transactionSetControlNumberUpperBound = (protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberUpperBound),
                                            rolloverTransactionSetControlNumber = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberRollover,
                                            isTestInterchange = protocolSettingsAB.EnvelopeSettings.IsTestInterchange,
                                            ApplicationReferenceId = protocolSettingsAB.EnvelopeSettings.ApplicationReferenceId,
                                            CommunicationAgreementId = protocolSettingsAB.EnvelopeSettings.CommunicationAgreementId,
                                            FunctionalGroupId = protocolSettingsAB.EnvelopeSettings.FunctionalGroupId,
                                            GroupApplicationPassword = protocolSettingsAB.EnvelopeSettings.GroupApplicationPassword,
                                            GroupApplicationReceiverId = protocolSettingsAB.EnvelopeSettings.GroupApplicationReceiverId,
                                            GroupApplicationReceiverQualifier = protocolSettingsAB.EnvelopeSettings.GroupApplicationReceiverQualifier,
                                            GroupApplicationSenderId = protocolSettingsAB.EnvelopeSettings.GroupApplicationSenderId,
                                            GroupApplicationSenderQualifier = protocolSettingsAB.EnvelopeSettings.GroupApplicationSenderQualifier,
                                            GroupAssociationAssignedCode = protocolSettingsAB.EnvelopeSettings.GroupAssociationAssignedCode,
                                            GroupControllingAgencyCode = protocolSettingsAB.EnvelopeSettings.GroupControllingAgencyCode,
                                            GroupControlNumberPrefix = protocolSettingsAB.EnvelopeSettings.GroupControlNumberPrefix,
                                            GroupControlNumberSuffix = protocolSettingsAB.EnvelopeSettings.GroupControlNumberSuffix,
                                            GroupMessageRelease = protocolSettingsAB.EnvelopeSettings.GroupMessageRelease,
                                            GroupMessageVersion = protocolSettingsAB.EnvelopeSettings.GroupMessageVersion,
                                            InterchangeControlNumberPrefix = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberPrefix,
                                            InterchangeControlNumberSuffix = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberSuffix,
                                            ProcessingPriorityCode = protocolSettingsAB.EnvelopeSettings.ProcessingPriorityCode,
                                            ReceiverInternalIdentification = protocolSettingsAB.EnvelopeSettings.ReceiverInternalIdentification,
                                            ReceiverInternalSubIdentification = protocolSettingsAB.EnvelopeSettings.ReceiverInternalSubIdentification,
                                            ReceiverReverseRoutingAddress = protocolSettingsAB.EnvelopeSettings.ReceiverReverseRoutingAddress,
                                            RecipientReferencePasswordQualifier = protocolSettingsAB.EnvelopeSettings.RecipientReferencePasswordQualifier,
                                            RecipientReferencePasswordValue = protocolSettingsAB.EnvelopeSettings.RecipientReferencePasswordValue,
                                            SenderInternalIdentification = protocolSettingsAB.EnvelopeSettings.SenderInternalIdentification,
                                            SenderInternalSubIdentification = protocolSettingsAB.EnvelopeSettings.SenderInternalSubIdentification,
                                            SenderReverseRoutingAddress = protocolSettingsAB.EnvelopeSettings.SenderReverseRoutingAddress,
                                            TransactionSetControlNumberPrefix = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberPrefix,
                                            TransactionSetControlNumberSuffix = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberSuffix
                                            #endregion

                                        },
                                        acknowledgementSettings = new EDIFACTAgreementJson.Acknowledgementsettings()
                                        {
                                            #region acknowledgementSettings
                                            needTechnicalAcknowledgement = protocolSettingsAB.AcknowledgementSettings.NeedTechnicalAcknowledgement,
                                            batchTechnicalAcknowledgements = protocolSettingsAB.AcknowledgementSettings.BatchTechnicalAcknowledgements,
                                            needFunctionalAcknowledgement = protocolSettingsAB.AcknowledgementSettings.NeedFunctionalAcknowledgement,
                                            batchFunctionalAcknowledgements = protocolSettingsAB.AcknowledgementSettings.BatchFunctionalAcknowledgements,
                                            needLoopForValidMessages = protocolSettingsAB.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            sendSynchronousAcknowledgement = protocolSettingsAB.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            acknowledgementControlNumberLowerBound = (protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberLowerBound),
                                            acknowledgementControlNumberUpperBound = (protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberUpperBound),
                                            rolloverAcknowledgementControlNumber = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberRollover,
                                            AcknowledgementControlNumberSuffix = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberSuffix,
                                            AcknowledgementControlNumberPrefix = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberPrefix
                                            #endregion
                                        },
                                        messageFilter = new EDIFACTAgreementJson.Messagefilter()
                                        {
                                            messageFilterType = Convert.ToString(protocolSettingsAB.MessageFilter.MessageFilterType)//todo india
                                        },
                                        processingSettings = new EDIFACTAgreementJson.Processingsettings()
                                        {
                                            maskSecurityInfo = protocolSettingsAB.ProcessingSettings.MaskSecurityInfo,
                                            preserveInterchange = protocolSettingsAB.ProcessingSettings.PreserveInterchange,
                                            suspendInterchangeOnError = protocolSettingsAB.ProcessingSettings.SuspendInterchangeOnError,
                                            createEmptyXmlTagsForTrailingSeparators = protocolSettingsAB.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators,
                                            useDotAsDecimalSeparator = protocolSettingsAB.ProcessingSettings.UseDotAsDecimalSeparator
                                        },
                                        edifactDelimiterOverrides = GetEdifactDelimiterOverrides(),
                                        schemaReferences = GetSchemaReference(protocolSettingsAB.SchemaSettings),
                                        envelopeOverrides = GetenvelopeOverrides(protocolSettingsAB),
                                        messageFilterList = GetMessageFilterList(protocolSettingsAB),
                                        validationOverrides = GetvalidationOverrides(protocolSettingsAB.ValidationSettings)
                                    },//protocolSettings
                                    senderBusinessIdentity = new EDIFACTAgreementJson.Senderbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderGuestQualifierIdentity.Value
                                    },
                                    receiverBusinessIdentity = new EDIFACTAgreementJson.Receiverbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverHostQualifierIdentity.Value
                                    }
                                }//receiveAgreement


                                #endregion
                            ,
                                #region SendAgreement
                                sendAgreement = new EDIFACTAgreementJson.Sendagreement()
                                {
                                    protocolSettings = new EDIFACTAgreementJson.Protocolsettings1()
                                    {
                                        validationSettings = new EDIFACTAgreementJson.Validationsettings1()
                                        {
                                            validateCharacterSet = protocolSettingsBA.ValidationSettings.ValidateCharacterSet,
                                            checkDuplicateInterchangeControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateInterchangeControlNumber,
                                            interchangeControlNumberValidityDays = protocolSettingsBA.ValidationSettings.InterchangeControlNumberValidityPeriod,
                                            checkDuplicateGroupControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateGroupControlNumber,
                                            checkDuplicateTransactionSetControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateTransactionSetControlNumber,
                                            validateEDITypes = protocolSettingsBA.ValidationSettings.ValidateEDITypes,
                                            validateXSDTypes = protocolSettingsBA.ValidationSettings.ValidateXSDTypes,
                                            trimLeadingAndTrailingSpacesAndZeroes = protocolSettingsBA.ValidationSettings.TrimLeadingAndTrailingSpacesAndZeroes,
                                            allowLeadingAndTrailingSpacesAndZeroes = protocolSettingsBA.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes,
                                            trailingSeparatorPolicy = Convert.ToString(protocolSettingsBA.ValidationSettings.TrailingSeparatorPolicy)
                                        },
                                        framingSettings = new EDIFACTAgreementJson.Framingsettings1()
                                        {
                                            ComponentSeparator = protocolSettingsBA.FramingSettings.ComponentSeparator,
                                            CharacterEncoding = protocolSettingsBA.FramingSettings.CharacterEncoding,
                                            protocolVersion = protocolSettingsBA.FramingSettings.ProtocolVersion,
                                            dataElementSeparator = protocolSettingsBA.FramingSettings.DataElementSeparator,
                                            componentSeparator = protocolSettingsBA.FramingSettings.ComponentSeparator,
                                            segmentTerminator = protocolSettingsBA.FramingSettings.SegmentTerminator,
                                            releaseIndicator = protocolSettingsBA.FramingSettings.ReleaseIndicator,
                                            repetitionSeparator = protocolSettingsBA.FramingSettings.RepetitionSeparator,
                                            characterSet = Convert.ToString(protocolSettingsBA.FramingSettings.CharacterSet),
                                            decimalPointIndicator = Convert.ToString(protocolSettingsBA.FramingSettings.DecimalPointIndicator),
                                            segmentTerminatorSuffix = Convert.ToString(protocolSettingsBA.FramingSettings.SegmentTerminatorSuffix)
                                        }
                                        ,
                                        envelopeSettings = new EDIFACTAgreementJson.Envelopesettings1()
                                        {
                                            applyDelimiterStringAdvice = protocolSettingsBA.EnvelopeSettings.ApplyDelimiterStringAdvice,
                                            createGroupingSegments = protocolSettingsBA.EnvelopeSettings.CreateGroupingSegments,
                                            enableDefaultGroupHeaders = protocolSettingsBA.EnvelopeSettings.EnableDefaultGroupHeaders,
                                            interchangeControlNumberLowerBound = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberLowerBound,
                                            interchangeControlNumberUpperBound = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberUpperBound,
                                            rolloverInterchangeControlNumber = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberRollover,
                                            groupControlNumberLowerBound = protocolSettingsBA.EnvelopeSettings.GroupControlNumberLowerBound,
                                            groupControlNumberUpperBound = protocolSettingsBA.EnvelopeSettings.GroupControlNumberUpperBound,
                                            rolloverGroupControlNumber = protocolSettingsBA.EnvelopeSettings.GroupControlNumberRollover,
                                            overwriteExistingTransactionSetControlNumber = protocolSettingsBA.EnvelopeSettings.OverwriteExistingTransactionSetControlNumber,
                                            transactionSetControlNumberLowerBound = (protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberLowerBound),
                                            transactionSetControlNumberUpperBound = (protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberUpperBound),
                                            rolloverTransactionSetControlNumber = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberRollover,
                                            isTestInterchange = protocolSettingsBA.EnvelopeSettings.IsTestInterchange,
                                            ApplicationReferenceId = protocolSettingsBA.EnvelopeSettings.ApplicationReferenceId,
                                            CommunicationAgreementId = protocolSettingsBA.EnvelopeSettings.CommunicationAgreementId,
                                            FunctionalGroupId = protocolSettingsBA.EnvelopeSettings.FunctionalGroupId,
                                            GroupApplicationPassword = protocolSettingsBA.EnvelopeSettings.GroupApplicationPassword,
                                            GroupApplicationReceiverId = protocolSettingsBA.EnvelopeSettings.GroupApplicationReceiverId,
                                            GroupApplicationReceiverQualifier = protocolSettingsBA.EnvelopeSettings.GroupApplicationReceiverQualifier,
                                            GroupApplicationSenderId = protocolSettingsBA.EnvelopeSettings.GroupApplicationSenderId,
                                            GroupApplicationSenderQualifier = protocolSettingsBA.EnvelopeSettings.GroupApplicationSenderQualifier,
                                            GroupAssociationAssignedCode = protocolSettingsBA.EnvelopeSettings.GroupAssociationAssignedCode,
                                            GroupControllingAgencyCode = protocolSettingsBA.EnvelopeSettings.GroupControllingAgencyCode,
                                            GroupControlNumberPrefix = protocolSettingsBA.EnvelopeSettings.GroupControlNumberPrefix,
                                            GroupControlNumberSuffix = protocolSettingsBA.EnvelopeSettings.GroupControlNumberSuffix,
                                            GroupMessageRelease = protocolSettingsBA.EnvelopeSettings.GroupMessageRelease,
                                            GroupMessageVersion = protocolSettingsBA.EnvelopeSettings.GroupMessageVersion,
                                            InterchangeControlNumberPrefix = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberPrefix,
                                            InterchangeControlNumberSuffix = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberSuffix,
                                            ProcessingPriorityCode = protocolSettingsBA.EnvelopeSettings.ProcessingPriorityCode,
                                            ReceiverInternalIdentification = protocolSettingsBA.EnvelopeSettings.ReceiverInternalIdentification,
                                            ReceiverInternalSubIdentification = protocolSettingsBA.EnvelopeSettings.ReceiverInternalSubIdentification,
                                            ReceiverReverseRoutingAddress = protocolSettingsBA.EnvelopeSettings.ReceiverReverseRoutingAddress,
                                            RecipientReferencePasswordQualifier = protocolSettingsBA.EnvelopeSettings.RecipientReferencePasswordQualifier,
                                            RecipientReferencePasswordValue = protocolSettingsBA.EnvelopeSettings.RecipientReferencePasswordValue,
                                            SenderInternalIdentification = protocolSettingsBA.EnvelopeSettings.SenderInternalIdentification,
                                            SenderInternalSubIdentification = protocolSettingsBA.EnvelopeSettings.SenderInternalSubIdentification,
                                            SenderReverseRoutingAddress = protocolSettingsBA.EnvelopeSettings.SenderReverseRoutingAddress,
                                            TransactionSetControlNumberPrefix = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberPrefix,
                                            TransactionSetControlNumberSuffix = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberSuffix

                                        },
                                        acknowledgementSettings = new EDIFACTAgreementJson.Acknowledgementsettings1()
                                        {
                                            needTechnicalAcknowledgement = protocolSettingsBA.AcknowledgementSettings.NeedTechnicalAcknowledgement,
                                            batchTechnicalAcknowledgements = protocolSettingsBA.AcknowledgementSettings.BatchTechnicalAcknowledgements,
                                            needFunctionalAcknowledgement = protocolSettingsBA.AcknowledgementSettings.NeedFunctionalAcknowledgement,
                                            batchFunctionalAcknowledgements = protocolSettingsBA.AcknowledgementSettings.BatchFunctionalAcknowledgements,
                                            needLoopForValidMessages = protocolSettingsBA.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            sendSynchronousAcknowledgement = protocolSettingsBA.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            acknowledgementControlNumberLowerBound = (protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberLowerBound),
                                            acknowledgementControlNumberUpperBound = (protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberUpperBound),
                                            rolloverAcknowledgementControlNumber = protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberRollover,

                                        },

                                        messageFilter = new EDIFACTAgreementJson.Messagefilter1()
                                        {
                                            messageFilterType = Convert.ToString(protocolSettingsBA.MessageFilter.MessageFilterType)//todo india
                                        },
                                        processingSettings = new EDIFACTAgreementJson.Processingsettings1()
                                        {
                                            maskSecurityInfo = protocolSettingsBA.ProcessingSettings.MaskSecurityInfo,
                                            preserveInterchange = protocolSettingsBA.ProcessingSettings.PreserveInterchange,
                                            suspendInterchangeOnError = protocolSettingsBA.ProcessingSettings.SuspendInterchangeOnError,
                                            createEmptyXmlTagsForTrailingSeparators = protocolSettingsBA.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators,
                                            useDotAsDecimalSeparator = protocolSettingsBA.ProcessingSettings.UseDotAsDecimalSeparator
                                        }
                                        ,

                                        edifactDelimiterOverrides = GetEdifactDelimiterOverrides1(),
                                        schemaReferences = GetSchemaReference1(protocolSettingsBA.SchemaSettings),
                                        envelopeOverrides = GetenvelopeOverrides1(protocolSettingsBA.EnvelopeSettings),
                                        messageFilterList = GetMessageFilterList1(protocolSettingsBA),//messageFilterList = protocolSettingsBA.MessageFilterList.ToArray(),
                                        validationOverrides = GetvalidationOverrides1(protocolSettingsBA.ValidationSettings)//  protocolSettingsBA.ValidationSettings.GetOverrides() //  protocolSettingsBA.ValidationSettings.GetOverrides().ToArray()


                                    }
                                    ,
                                    senderBusinessIdentity = new EDIFACTAgreementJson.Senderbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverHostQualifierIdentity.Value
                                    },
                                    receiverBusinessIdentity = new EDIFACTAgreementJson.Receiverbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderGuestQualifierIdentity.Value
                                    }

                                }
                                #endregion

                            }//edifact
                        }//content
                        ,
                        createdTime = DateTime.Now,
                        changedTime = DateTime.Now,
                        metadata = GenerateMetadata(agreementName)
                    },
                    name = FileOperations.GetFileName(agreementName),
                    type = Resources.AgreementType,
                };
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(EDIFACTAgreementJsonRootobject.name), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(EDIFACTAgreementJsonRootobject);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                TraceProvider.WriteLine(string.Format("Agreement {0} Exported to Json with Name {1}", EDIFACTAgreementJsonRootobject.name, FileOperations.GetFileName(EDIFACTAgreementJsonRootobject.name)));

                return EDIFACTAgreementJsonRootobject;
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }


        public void CreateIndividualEdifactAgreements(AgreementMigrationItemViewModel agreementItem)
        {
            EDIFACTAgreementJson.Rootobject edifactObj = null;
            foreach (var agreement in agreementItem.AgreementsToConsolidate)
            {
                try
                {

                    edifactObj = EDIFACTAgreementProcess(agreement).Result;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to create individual agreement {0}. {1}", agreement.Name, ExceptionHelper.GetExceptionMessage(ex)));
                }
            }
        }

        public EDIFACTAgreementJson.Rootobject ConsolidateTwoEdifactAgreements(EDIFACTAgreementJson.Rootobject baseAgreementRootObj, EDIFACTAgreementJson.Rootobject agreementJson)
        {
            try
            {
                InitializeBaseEdifactAgreement(baseAgreementRootObj);
                if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences != null && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences.Count() != 0)
                {
                    foreach (var schemaReference in agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences.Concat(new EDIFACTAgreementJson.Schemareference[] { schemaReference }).ToArray();
                        }
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides != null
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Length != 0
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) != 0)
                            {
                                var edifactDelimiterOverride = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Where(x => x.messageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Concat(new EDIFACTAgreementJson.EdifactDelimiterOverrides[] { edifactDelimiterOverride }).ToArray();
                            }
                            else
                            {
                                EDIFACTAgreementJson.EdifactDelimiterOverrides[] receiveDelimiterOverride = new EDIFACTAgreementJson.EdifactDelimiterOverrides[]{new EDIFACTAgreementJson.EdifactDelimiterOverrides()
                                    {
                                        messageId = schemaReference.messageId,
                                        messageRelease = schemaReference.messageRelease,
                                        messageVersion = schemaReference.messageVersion,
                                        dataElementSeparator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.dataElementSeparator,
                                        componentSeparator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.ComponentSeparator,
                                        segmentTerminator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.segmentTerminator,
                                        segmentTerminatorSuffix = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.segmentTerminatorSuffix,
                                        repetitionSeparator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.repetitionSeparator,
                                        decimalPointIndicator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.decimalPointIndicator,
                                        releaseIndicator = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.framingSettings.releaseIndicator,
                                        targetNamespace = schemaReference.schemaName
                                    } };
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Concat(receiveDelimiterOverride).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides != null
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Length != 0
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var validationOverride = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides[] { validationOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides[] {new EDIFACTAgreementJson.ValidationOverrides()
                                        {
                                            MessageId =  schemaReference.messageId,
                                            AllowLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.allowLeadingAndTrailingSpacesAndZeroes,
                                            EnforceCharacterSet = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.validateCharacterSet,
                                            TrailingSeparatorPolicy = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.trailingSeparatorPolicy,
                                            TrimLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.trimLeadingAndTrailingSpacesAndZeroes,
                                            ValidateEDITypes = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.validateEDITypes,
                                            ValidateXSDTypes = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationSettings.validateXSDTypes
                                        }}).ToArray();
                            }
                        }

                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides != null
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Length != 0
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var envelopeOverride = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides[] { envelopeOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides[] { new EDIFACTAgreementJson.EnvelopeOverrides()
                                        {
                                            ApplicationPassword = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupApplicationPassword,
                                            AssociationAssignedCode = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupAssociationAssignedCode,
                                            ControllingAgencyCode = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupControllingAgencyCode,
                                            FunctionalGroupId = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.FunctionalGroupId,
                                            GroupHeaderMessageRelease = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupMessageRelease,
                                            GroupHeaderMessageVersion = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupMessageVersion,
                                            MessageAssociationAssignedCode = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupAssociationAssignedCode,
                                            MessageId =  schemaReference.messageId,
                                            MessageRelease =  schemaReference.messageRelease,
                                            MessageVersion =  schemaReference.messageVersion,
                                            ReceiverApplicationId = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupApplicationReceiverId,
                                            ReceiverApplicationQualifier = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupApplicationReceiverQualifier,
                                            SenderApplicationId = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupApplicationSenderId,
                                            SenderApplicationQualifier = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeSettings.GroupApplicationSenderQualifier,
                                            TargetNamespace = schemaReference.schemaName
                                        }}).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList != null
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Length != 0
                                && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var messageFilter = agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Concat(new EDIFACTAgreementJson.EDIFACTMessageIdentifier[] { messageFilter }).ToArray();
                            }
                            else
                            {
                            }
                        }
                    }
                }

                if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences != null && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences.Count() != 0)
                {
                    foreach (var schemaReference in agreementJson.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences.Concat(new EDIFACTAgreementJson.Schemareference1[] { schemaReference }).ToArray();
                        }
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides != null
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Length != 0
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) != 0)
                            {
                                var edifactDelimiterOverride = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Where(x => x.messageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Concat(new EDIFACTAgreementJson.EdifactDelimiterOverrides1[] { edifactDelimiterOverride }).ToArray();
                            }
                            else
                            {
                                EDIFACTAgreementJson.EdifactDelimiterOverrides1[] sendDelimiterOverride = new EDIFACTAgreementJson.EdifactDelimiterOverrides1[] { new EDIFACTAgreementJson.EdifactDelimiterOverrides1()
                                {
                                    messageId = schemaReference.messageId,
                                    messageRelease =  schemaReference.messageRelease,
                                    messageVersion =  schemaReference.messageVersion,
                                    dataElementSeparator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.dataElementSeparator,
                                    componentSeparator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.ComponentSeparator,
                                    segmentTerminator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.segmentTerminator,
                                    segmentTerminatorSuffix = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.segmentTerminatorSuffix,
                                    repetitionSeparator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.repetitionSeparator,
                                    decimalPointIndicator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.decimalPointIndicator,
                                    releaseIndicator = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.framingSettings.releaseIndicator,
                                    targetNamespace = schemaReference.schemaName
                                } };
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Concat(sendDelimiterOverride).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides != null
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Length != 0
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var validationOverride = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides1[] { validationOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides1[] { new EDIFACTAgreementJson.ValidationOverrides1()
                                    {
                                        MessageId =  schemaReference.messageId,
                                        AllowLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.allowLeadingAndTrailingSpacesAndZeroes,
                                        EnforceCharacterSet = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.validateCharacterSet,
                                        TrailingSeparatorPolicy = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.trailingSeparatorPolicy,
                                        TrimLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.trimLeadingAndTrailingSpacesAndZeroes,
                                        ValidateEDITypes = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.validateEDITypes,
                                        ValidateXSDTypes = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationSettings.validateXSDTypes
                                    } }).ToArray();
                            }
                        }

                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides != null
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Length != 0
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var envelopeOverride = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides1[] { envelopeOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides1[] { new EDIFACTAgreementJson.EnvelopeOverrides1()
                                    {
                                        ApplicationPassword = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupApplicationPassword,
                                        AssociationAssignedCode = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupAssociationAssignedCode,
                                        ControllingAgencyCode = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupControllingAgencyCode,
                                        FunctionalGroupId = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.FunctionalGroupId,
                                        GroupHeaderMessageRelease = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupMessageRelease,
                                        GroupHeaderMessageVersion = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupMessageVersion,
                                        MessageAssociationAssignedCode = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupAssociationAssignedCode,
                                        MessageId = schemaReference.messageId,
                                        MessageRelease = schemaReference.messageRelease,
                                        MessageVersion =  schemaReference.messageVersion,
                                        ReceiverApplicationId = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupApplicationReceiverId,
                                        ReceiverApplicationQualifier = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupApplicationReceiverQualifier,
                                        SenderApplicationId = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupApplicationSenderId,
                                        SenderApplicationQualifier = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeSettings.GroupApplicationSenderQualifier,
                                        TargetNamespace =  schemaReference.schemaName
                                    }}).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList != null
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Length != 0
                                && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var messageFilter = agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Concat(new EDIFACTAgreementJson.EDIFACTMessageIdentifier[] { messageFilter }).ToArray();
                            }
                            else
                            {
                            }
                        }
                    }
                }


                if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides != null && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Length != 0)
                {
                    foreach (var edifactDelimiterOverride in agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == edifactDelimiterOverride.messageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides.Concat(new EDIFACTAgreementJson.EdifactDelimiterOverrides[] { edifactDelimiterOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides != null && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Length != 0)
                {
                    foreach (var edifactDelimiterOverride in agreementJson.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Count(x => x.messageId == edifactDelimiterOverride.messageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides.Concat(new EDIFACTAgreementJson.EdifactDelimiterOverrides1[] { edifactDelimiterOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides != null && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Length != 0)
                {
                    foreach (var validationOverride in agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == validationOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides[] { validationOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides != null && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Length != 0)
                {
                    foreach (var validationOverride in agreementJson.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == validationOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides.Concat(new EDIFACTAgreementJson.ValidationOverrides1[] { validationOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides != null && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Length != 0)
                {
                    foreach (var envelopeOverride in agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == envelopeOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides[] { envelopeOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides != null && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Length != 0)
                {
                    foreach (var envelopeOverride in agreementJson.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == envelopeOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides.Concat(new EDIFACTAgreementJson.EnvelopeOverrides1[] { envelopeOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList != null && agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Length != 0)
                {
                    foreach (var messageFilter in agreementJson.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == messageFilter.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList = baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList.Concat(new EDIFACTAgreementJson.EDIFACTMessageIdentifier[] { messageFilter }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList != null && agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Length != 0)
                {
                    foreach (var messageFilter in agreementJson.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList)
                    {
                        if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Count(x => x.MessageId == messageFilter.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList = baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList.Concat(new EDIFACTAgreementJson.EDIFACTMessageIdentifier[] { messageFilter }).ToArray();
                        }
                    }
                }
                return baseAgreementRootObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public EDIFACTAgreementJson.Rootobject ConsolidateEdifactAgreements(AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                CreateIndividualEdifactAgreements(agreementItem);
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
            try
            {
                EDIFACTAgreementJson.Rootobject baseAgreementRootObj = GetEdifactAgreementJson(agreementItem.BaseAgreementName);
                baseAgreementRootObj.name = agreementItem.Name;
                baseAgreementRootObj.type = Resources.AgreementType;
                baseAgreementRootObj.properties.guestIdentity = new EDIFACTAgreementJson.Guestidentity { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.hostIdentity = new EDIFACTAgreementJson.Hostidentity { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.receiverBusinessIdentity = new EDIFACTAgreementJson.Receiverbusinessidentity { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.senderBusinessIdentity = new EDIFACTAgreementJson.Senderbusinessidentity { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.content.edifact.sendAgreement.receiverBusinessIdentity = new EDIFACTAgreementJson.Receiverbusinessidentity1 { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.content.edifact.sendAgreement.senderBusinessIdentity = new EDIFACTAgreementJson.Senderbusinessidentity1 { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides>().ToArray();
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides1>().ToArray();
                foreach (var agreement in agreementItem.AgreementsToConsolidate)
                {
                    if (agreement.Name != agreementItem.BaseAgreementName)
                    {
                        try
                        {
                            EDIFACTAgreementJson.Rootobject agreementJson = GetEdifactAgreementJson(agreement.Name);
                            baseAgreementRootObj = ConsolidateTwoEdifactAgreements(baseAgreementRootObj, agreementJson);

                        }
                        catch (Exception ex)
                        {
                            TraceProvider.WriteLine(string.Format("Skipping agreement {0} while consolidating in agreement {1}. Reason: {2}", agreement.Name, agreementItem.Name, ExceptionHelper.GetExceptionMessage(ex)));
                            continue;
                        }
                    }

                }
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(baseAgreementRootObj.name), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(baseAgreementRootObj);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                TraceProvider.WriteLine(string.Format("Agreement {0} Exported to Json with Name {1}", baseAgreementRootObj.name, FileOperations.GetFileName(baseAgreementRootObj.name)));
                return baseAgreementRootObj;
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public EDIFACTAgreementJson.Rootobject GetEdifactAgreementJson(string agreementName)
        {
            try
            {
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agreementName), ".json");
                string partnerJsonFileContent = File.ReadAllText(fileName);
                EDIFACTAgreementJson.Rootobject obj = JsonConvert.DeserializeObject<EDIFACTAgreementJson.Rootobject>(partnerJsonFileContent);
                return obj;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error while Consolidating agreements. Cannot Read the agreement {0} from local folder. Reason : {1}", agreementName, ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Error while Consolidating agreements. Cannot Read the agreement {0} from local folder. Reason : {1}", agreementName, ExceptionHelper.GetExceptionMessage(ex)));
            }
        }
        #endregion

        #region X12AgreementJsonProcess

        public async Task<X12AgreementJson.Rootobject> X12AgreementJsonProcess(AgreementMigrationItemViewModel agreementItem)
        {
            X12AgreementJson.Rootobject X12AgreementJsonRootobject = null;
            try
            {
                var agreement = agreementItem.MigrationEntity;
                string agreementName = FileOperations.GetFileName(agreement.Name);
                string type = Resources.JsonPartnerAgreementType;
                string senderGuestDetailsPartner = (agreement.SenderDetails.Partner);
                string receiverHostDetailsParnter = (agreement.ReceiverDetails.Partner);

                Server.OnewayAgreement onewayAgreementAB = agreement.GetOnewayAgreement(senderGuestDetailsPartner, receiverHostDetailsParnter);//receive agreement
                Server.X12ProtocolSettings protocolSettingsAB = onewayAgreementAB.GetProtocolSettings<Server.X12ProtocolSettings>();

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABreceiverHostQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABsenderGuestQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.SenderIdentity;

                Server.OnewayAgreement onewayAgreementBA = agreement.GetOnewayAgreement(receiverHostDetailsParnter, senderGuestDetailsPartner);//sender agreement
                Server.X12ProtocolSettings protocolSettingsBA = onewayAgreementBA.GetProtocolSettings<Server.X12ProtocolSettings>();//sender agreement

                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAreceiverQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.ReceiverIdentity;
                Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementBAsenderQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementBA.SenderIdentity;

                agreementItem.HostPartnerQualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier;
                agreementItem.HostPartnerId = onewayAgreementABreceiverHostQualifierIdentity.Value;
                agreementItem.GuestPartnerQualifer = onewayAgreementABsenderGuestQualifierIdentity.Qualifier;
                agreementItem.GuestPartnerId = onewayAgreementABsenderGuestQualifierIdentity.Value;
                if (Convert.ToBoolean(thisApplicationContext.GetProperty(AppConstants.ContextGenerationEnabled)))
                {
                    GenerateMetadataContext(senderGuestDetailsPartner, agreementItem, protocolSettingsAB, protocolSettingsBA);
                }
                X12AgreementJsonRootobject = new X12AgreementJson.Rootobject()
                {
                    properties = new X12AgreementJson.Properties()
                    {
                        hostPartner = FileOperations.GetFileName(receiverHostDetailsParnter),
                        guestPartner = FileOperations.GetFileName(senderGuestDetailsPartner),
                        hostIdentity = new X12AgreementJson.Hostidentity()
                        {
                            qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                            value = onewayAgreementABreceiverHostQualifierIdentity.Value
                        },
                        guestIdentity = new X12AgreementJson.Guestidentity()
                        {
                            qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                            value = onewayAgreementABsenderGuestQualifierIdentity.Value
                        },
                        agreementType = agreement.Protocol,
                        content = new X12AgreementJson.Content()
                        {
                            x12 = new X12AgreementJson.X12()
                            {
                                receiveAgreement = new X12AgreementJson.Receiveagreement()
                                {
                                    protocolSettings = new X12AgreementJson.Protocolsettings()
                                    {
                                        validationSettings = new X12AgreementJson.Validationsettings()
                                        {
                                            #region validationSettings
                                            validateCharacterSet = protocolSettingsAB.ValidationSettings.ValidateCharacterSet,
                                            checkDuplicateInterchangeControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateInterchangeControlNumber,
                                            interchangeControlNumberValidityDays = protocolSettingsAB.ValidationSettings.InterchangeControlNumberValidityPeriod,
                                            checkDuplicateGroupControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateGroupControlNumber,
                                            checkDuplicateTransactionSetControlNumber = protocolSettingsAB.ValidationSettings.CheckDuplicateTransactionSetControlNumber,
                                            validateEDITypes = protocolSettingsAB.ValidationSettings.ValidateEDITypes,
                                            validateXSDTypes = protocolSettingsAB.ValidationSettings.ValidateXSDTypes,
                                            trimLeadingAndTrailingSpacesAndZeroes = protocolSettingsAB.ValidationSettings.TrimLeadingAndTrailingSpacesAndZeroes,
                                            allowLeadingAndTrailingSpacesAndZeroes = protocolSettingsAB.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes,
                                            trailingSeparatorPolicy = Convert.ToString(protocolSettingsAB.ValidationSettings.TrailingSeparatorPolicy)
                                            #endregion
                                        },
                                        framingSettings = new X12AgreementJson.Framingsettings()
                                        {
                                            #region framingSettings
                                            CharacterSet = protocolSettingsAB.FramingSettings.CharacterSet.ToString(),
                                            ComponentSeparator = protocolSettingsAB.FramingSettings.ComponentSeparator,
                                            DataElementSeparator = protocolSettingsAB.FramingSettings.DataElementSeparator,
                                            replaceCharacter = protocolSettingsAB.FramingSettings.ReplaceChar,
                                            ReplaceSeparatorsInPayload = protocolSettingsAB.FramingSettings.ReplaceSeparatorsInPayload,
                                            SegmentTerminator = protocolSettingsAB.FramingSettings.SegmentTerminator,
                                            SegmentTerminatorSuffix = protocolSettingsAB.FramingSettings.SegmentTerminatorSuffix.ToString()
                                            #endregion
                                        },
                                        envelopeSettings = new X12AgreementJson.Envelopesettings()
                                        {
                                            #region envelopsettings
                                            ControlStandardsId = protocolSettingsAB.EnvelopeSettings.ControlStandardsId,
                                            ControlVersionNumber = protocolSettingsAB.EnvelopeSettings.ControlVersionNumber,
                                            EnableDefaultGroupHeaders = protocolSettingsAB.EnvelopeSettings.EnableDefaultGroupHeaders,
                                            FunctionalGroupId = protocolSettingsAB.EnvelopeSettings.FunctionalGroupId,
                                            GroupControlNumberLowerBound = protocolSettingsAB.EnvelopeSettings.GroupControlNumberLowerBound,
                                            rolloverGroupControlNumber = protocolSettingsAB.EnvelopeSettings.GroupControlNumberRollover,
                                            GroupControlNumberUpperBound = protocolSettingsAB.EnvelopeSettings.GroupControlNumberUpperBound,
                                            GroupHeaderAgencyCode = protocolSettingsAB.EnvelopeSettings.GroupHeaderAgencyCode,
                                            GroupHeaderDateFormat = protocolSettingsAB.EnvelopeSettings.GroupHeaderDateFormat.ToString(),
                                            GroupHeaderTimeFormat = protocolSettingsAB.EnvelopeSettings.GroupHeaderTimeFormat.ToString(),
                                            GroupHeaderVersion = protocolSettingsAB.EnvelopeSettings.GroupHeaderVersion,
                                            InterchangeControlNumberLowerBound = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberLowerBound,
                                            rolloverInterchangeControlNumber = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberRollover,
                                            InterchangeControlNumberUpperBound = protocolSettingsAB.EnvelopeSettings.InterchangeControlNumberUpperBound,
                                            OverwriteExistingTransactionSetControlNumber = protocolSettingsAB.EnvelopeSettings.OverwriteExistingTransactionSetControlNumber,
                                            ReceiverApplicationId = protocolSettingsAB.EnvelopeSettings.ReceiverApplicationId,
                                            SenderApplicationId = protocolSettingsAB.EnvelopeSettings.SenderApplicationId,
                                            TransactionSetControlNumberLowerBound = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberLowerBound,
                                            TransactionSetControlNumberPrefix = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberPrefix,
                                            rolloverTransactionSetControlNumber = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberRollover,
                                            TransactionSetControlNumberSuffix = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberSuffix,
                                            TransactionSetControlNumberUpperBound = protocolSettingsAB.EnvelopeSettings.TransactionSetControlNumberUpperBound,
                                            UsageIndicator = protocolSettingsAB.EnvelopeSettings.UsageIndicator.ToString(),
                                            UseControlStandardsIdAsRepetitionCharacter = protocolSettingsAB.EnvelopeSettings.UseControlStandardsIdAsRepetitionCharacter
                                            #endregion
                                        },
                                        acknowledgementSettings = new X12AgreementJson.Acknowledgementsettings()
                                        {
                                            #region acknowledgementSettings
                                            functionalAcknowledgementVersion = protocolSettingsAB.AcknowledgementSettings.FunctionalAcknowledgementVersion,
                                            needTechnicalAcknowledgement = protocolSettingsAB.AcknowledgementSettings.NeedTechnicalAcknowledgement,
                                            batchTechnicalAcknowledgements = protocolSettingsAB.AcknowledgementSettings.BatchTechnicalAcknowledgements,
                                            needFunctionalAcknowledgement = protocolSettingsAB.AcknowledgementSettings.NeedFunctionalAcknowledgement,
                                            batchFunctionalAcknowledgements = protocolSettingsAB.AcknowledgementSettings.BatchFunctionalAcknowledgements,
                                            needLoopForValidMessages = protocolSettingsAB.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            sendSynchronousAcknowledgement = protocolSettingsAB.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            acknowledgementControlNumberLowerBound = (protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberLowerBound),
                                            acknowledgementControlNumberUpperBound = (protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberUpperBound),
                                            rolloverAcknowledgementControlNumber = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberRollover,
                                            AcknowledgementControlNumberSuffix = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberSuffix,
                                            AcknowledgementControlNumberPrefix = protocolSettingsAB.AcknowledgementSettings.AcknowledgementControlNumberPrefix,
                                            ImplementationAcknowledgementVersion = protocolSettingsAB.AcknowledgementSettings.ImplementationAcknowledgementVersion,
                                            needImplementationAcknowledgement = protocolSettingsAB.AcknowledgementSettings.NeedImplementationAcknowledgement,
                                            batchImplementationAcknowledgements = protocolSettingsAB.AcknowledgementSettings.BatchImplementationAcknowledgements
                                            #endregion
                                        },
                                        messageFilter = new X12AgreementJson.Messagefilter()
                                        {
                                            messageFilterType = Convert.ToString(protocolSettingsAB.MessageFilter.MessageFilterType)//todo india
                                        },
                                        processingSettings = new X12AgreementJson.Processingsettings()
                                        {
                                            #region Processingsettings
                                            maskSecurityInfo = protocolSettingsAB.ProcessingSettings.MaskSecurityInfo,
                                            preserveInterchange = protocolSettingsAB.ProcessingSettings.PreserveInterchange,
                                            suspendInterchangeOnError = protocolSettingsAB.ProcessingSettings.SuspendInterchangeOnError,
                                            createEmptyXmlTagsForTrailingSeparators = protocolSettingsAB.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators,
                                            useDotAsDecimalSeparator = protocolSettingsAB.ProcessingSettings.UseDotAsDecimalSeparator
                                            #endregion
                                        }
                                        ,
                                        securitySettings = new X12AgreementJson.Securitysettings()
                                        {
                                            #region SecuritySettings
                                            AuthorizationQualifier = protocolSettingsAB.SecuritySettings.AuthorizationQualifier,
                                            AuthorizationValue = protocolSettingsAB.SecuritySettings.AuthorizationValue,
                                            PasswordValue = protocolSettingsAB.SecuritySettings.PasswordValue,
                                            SecurityQualifier = protocolSettingsAB.SecuritySettings.SecurityQualifier
                                            #endregion
                                        }
                                        ,
                                        envelopeOverrides = X12GetenvelopeOverrides(protocolSettingsAB),
                                        validationOverrides = X12GetvalidationOverrides(protocolSettingsAB.ValidationSettings),
                                        schemaReferences = X12GetSchemaReference(protocolSettingsAB.SchemaSettings)


                                    },
                                    senderBusinessIdentity = new X12AgreementJson.Senderbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderGuestQualifierIdentity.Value
                                    },
                                    receiverBusinessIdentity = new X12AgreementJson.Receiverbusinessidentity()
                                    {
                                        qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverHostQualifierIdentity.Value
                                    },


                                }
                               ,
                                sendAgreement = new X12AgreementJson.Sendagreement()
                                {
                                    protocolSettings = new X12AgreementJson.Protocolsettings1()
                                    {
                                        validationSettings = new X12AgreementJson.Validationsettings1()
                                        {
                                            #region Validationsettings1
                                            validateCharacterSet = protocolSettingsBA.ValidationSettings.ValidateCharacterSet,
                                            checkDuplicateInterchangeControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateInterchangeControlNumber,
                                            interchangeControlNumberValidityDays = protocolSettingsBA.ValidationSettings.InterchangeControlNumberValidityPeriod,
                                            checkDuplicateGroupControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateGroupControlNumber,
                                            checkDuplicateTransactionSetControlNumber = protocolSettingsBA.ValidationSettings.CheckDuplicateTransactionSetControlNumber,
                                            validateEDITypes = protocolSettingsBA.ValidationSettings.ValidateEDITypes,
                                            validateXSDTypes = protocolSettingsBA.ValidationSettings.ValidateXSDTypes,
                                            trimLeadingAndTrailingSpacesAndZeroes = protocolSettingsBA.ValidationSettings.TrimLeadingAndTrailingSpacesAndZeroes,
                                            allowLeadingAndTrailingSpacesAndZeroes = protocolSettingsBA.ValidationSettings.AllowLeadingAndTrailingSpacesAndZeroes,
                                            trailingSeparatorPolicy = Convert.ToString(protocolSettingsBA.ValidationSettings.TrailingSeparatorPolicy)
                                            #endregion
                                        },
                                        framingSettings = new X12AgreementJson.Framingsettings1()
                                        {
                                            #region Framingsettings1
                                            CharacterSet = protocolSettingsBA.FramingSettings.CharacterSet.ToString(),
                                            ComponentSeparator = protocolSettingsBA.FramingSettings.ComponentSeparator,
                                            DataElementSeparator = protocolSettingsBA.FramingSettings.DataElementSeparator,
                                            replaceCharacter = protocolSettingsBA.FramingSettings.ReplaceChar,
                                            ReplaceSeparatorsInPayload = protocolSettingsBA.FramingSettings.ReplaceSeparatorsInPayload,
                                            SegmentTerminator = protocolSettingsBA.FramingSettings.SegmentTerminator,
                                            SegmentTerminatorSuffix = protocolSettingsBA.FramingSettings.SegmentTerminatorSuffix.ToString()
                                            #endregion
                                        },
                                        envelopeSettings = new X12AgreementJson.Envelopesettings1()
                                        {
                                            #region Envelopesettings1
                                            ControlStandardsId = protocolSettingsBA.EnvelopeSettings.ControlStandardsId,
                                            ControlVersionNumber = protocolSettingsBA.EnvelopeSettings.ControlVersionNumber,
                                            EnableDefaultGroupHeaders = protocolSettingsBA.EnvelopeSettings.EnableDefaultGroupHeaders,
                                            FunctionalGroupId = protocolSettingsBA.EnvelopeSettings.FunctionalGroupId,
                                            GroupControlNumberLowerBound = protocolSettingsBA.EnvelopeSettings.GroupControlNumberLowerBound,
                                            rolloverGroupControlNumber = protocolSettingsBA.EnvelopeSettings.GroupControlNumberRollover,
                                            GroupControlNumberUpperBound = protocolSettingsBA.EnvelopeSettings.GroupControlNumberUpperBound,
                                            GroupHeaderAgencyCode = protocolSettingsBA.EnvelopeSettings.GroupHeaderAgencyCode,
                                            GroupHeaderDateFormat = protocolSettingsBA.EnvelopeSettings.GroupHeaderDateFormat.ToString(),
                                            GroupHeaderTimeFormat = protocolSettingsBA.EnvelopeSettings.GroupHeaderTimeFormat.ToString(),
                                            GroupHeaderVersion = protocolSettingsBA.EnvelopeSettings.GroupHeaderVersion,
                                            InterchangeControlNumberLowerBound = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberLowerBound,
                                            rolloverInterchangeControlNumber = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberRollover,
                                            InterchangeControlNumberUpperBound = protocolSettingsBA.EnvelopeSettings.InterchangeControlNumberUpperBound,
                                            OverwriteExistingTransactionSetControlNumber = protocolSettingsBA.EnvelopeSettings.OverwriteExistingTransactionSetControlNumber,
                                            ReceiverApplicationId = protocolSettingsBA.EnvelopeSettings.ReceiverApplicationId,
                                            SenderApplicationId = protocolSettingsBA.EnvelopeSettings.SenderApplicationId,
                                            TransactionSetControlNumberLowerBound = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberLowerBound,
                                            TransactionSetControlNumberPrefix = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberPrefix,
                                            rolloverTransactionSetControlNumber = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberRollover,
                                            TransactionSetControlNumberSuffix = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberSuffix,
                                            TransactionSetControlNumberUpperBound = protocolSettingsBA.EnvelopeSettings.TransactionSetControlNumberUpperBound,
                                            UsageIndicator = protocolSettingsBA.EnvelopeSettings.UsageIndicator.ToString(),
                                            UseControlStandardsIdAsRepetitionCharacter = protocolSettingsBA.EnvelopeSettings.UseControlStandardsIdAsRepetitionCharacter
                                            #endregion
                                        },
                                        acknowledgementSettings = new X12AgreementJson.Acknowledgementsettings1()
                                        {
                                            #region Acknowledgementsettings1
                                            functionalAcknowledgementVersion = protocolSettingsBA.AcknowledgementSettings.FunctionalAcknowledgementVersion,
                                            needTechnicalAcknowledgement = protocolSettingsBA.AcknowledgementSettings.NeedTechnicalAcknowledgement,
                                            batchTechnicalAcknowledgements = protocolSettingsBA.AcknowledgementSettings.BatchTechnicalAcknowledgements,
                                            needFunctionalAcknowledgement = protocolSettingsBA.AcknowledgementSettings.NeedFunctionalAcknowledgement,
                                            batchFunctionalAcknowledgements = protocolSettingsBA.AcknowledgementSettings.BatchFunctionalAcknowledgements,
                                            needLoopForValidMessages = protocolSettingsBA.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            sendSynchronousAcknowledgement = protocolSettingsBA.AcknowledgementSettings.SendSynchronousAcknowledgement,
                                            acknowledgementControlNumberLowerBound = (protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberLowerBound),
                                            acknowledgementControlNumberUpperBound = (protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberUpperBound),
                                            rolloverAcknowledgementControlNumber = protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberRollover,
                                            AcknowledgementControlNumberSuffix = protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberSuffix,
                                            AcknowledgementControlNumberPrefix = protocolSettingsBA.AcknowledgementSettings.AcknowledgementControlNumberPrefix,
                                            ImplementationAcknowledgementVersion = protocolSettingsBA.AcknowledgementSettings.ImplementationAcknowledgementVersion,
                                            needImplementationAcknowledgement = protocolSettingsBA.AcknowledgementSettings.NeedImplementationAcknowledgement,
                                            batchImplementationAcknowledgements = protocolSettingsBA.AcknowledgementSettings.BatchImplementationAcknowledgements
                                            #endregion
                                        },

                                        messageFilter = new X12AgreementJson.Messagefilter1()
                                        {
                                            messageFilterType = Convert.ToString(protocolSettingsBA.MessageFilter.MessageFilterType)//todo india
                                        },
                                        processingSettings = new X12AgreementJson.Processingsettings1()
                                        {
                                            #region Processingsettings1
                                            maskSecurityInfo = protocolSettingsBA.ProcessingSettings.MaskSecurityInfo,
                                            preserveInterchange = protocolSettingsBA.ProcessingSettings.PreserveInterchange,
                                            suspendInterchangeOnError = protocolSettingsBA.ProcessingSettings.SuspendInterchangeOnError,
                                            createEmptyXmlTagsForTrailingSeparators = protocolSettingsBA.ProcessingSettings.CreateEmptyXmlTagsForTrailingSeparators,
                                            useDotAsDecimalSeparator = protocolSettingsBA.ProcessingSettings.UseDotAsDecimalSeparator
                                            #endregion
                                        }
                                        ,
                                        securitySettings = new X12AgreementJson.Securitysettings1()
                                        {
                                            #region SecuritySettings
                                            AuthorizationQualifier = protocolSettingsBA.SecuritySettings.AuthorizationQualifier,
                                            AuthorizationValue = protocolSettingsBA.SecuritySettings.AuthorizationValue,
                                            PasswordValue = protocolSettingsBA.SecuritySettings.PasswordValue,
                                            SecurityQualifier = protocolSettingsAB.SecuritySettings.SecurityQualifier
                                            #endregion
                                        }
                                        ,
                                        envelopeOverrides = X12GetenvelopeOverrides1(protocolSettingsBA),
                                        validationOverrides = X12GetvalidationOverrides1(protocolSettingsBA.ValidationSettings),
                                        schemaReferences = X12GetSchemaReference(protocolSettingsBA.SchemaSettings)

                                    }
                                   ,
                                    senderBusinessIdentity = new X12AgreementJson.Senderbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier,
                                        value = onewayAgreementABreceiverHostQualifierIdentity.Value
                                    },
                                    receiverBusinessIdentity = new X12AgreementJson.Receiverbusinessidentity1()
                                    {
                                        qualifier = onewayAgreementABsenderGuestQualifierIdentity.Qualifier,
                                        value = onewayAgreementABsenderGuestQualifierIdentity.Value
                                    }

                                }

                            }//edifact
                        }//content
                       ,
                        createdTime = DateTime.Now,
                        changedTime = DateTime.Now,
                        metadata = GenerateMetadata(agreementName)
                    },
                    name = FileOperations.GetFileName(agreementName),
                    type = Resources.AgreementType
                };
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(X12AgreementJsonRootobject.name), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(X12AgreementJsonRootobject);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                TraceProvider.WriteLine(string.Format("Agreement {0} Exported to Json with Name {1}", X12AgreementJsonRootobject.name, FileOperations.GetFileName(X12AgreementJsonRootobject.name)));

                return X12AgreementJsonRootobject;
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public void CreateIndividualX12Agreements(AgreementMigrationItemViewModel agreementItem)
        {
            X12AgreementJson.Rootobject x12Obj = null;
            foreach (var agreement in agreementItem.AgreementsToConsolidate)
            {
                try
                {

                    x12Obj = X12AgreementJsonProcess(agreement).Result;
                }
                catch (Exception ex)
                {
                    throw new Exception(string.Format("Failed to create individual agreement {0}. {1}", agreement.Name, ExceptionHelper.GetExceptionMessage(ex)));
                }
            }
        }

        public void InitializeBaseX12Agreement(X12AgreementJson.Rootobject baseAgreementRootObj)
        {
            if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences == null)
            {
                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences = new List<X12AgreementJson.Schemareference>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences == null)
            {
                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences = new List<X12AgreementJson.Schemareference>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides = new List<X12AgreementJson.X12DelimiterOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides = new List<X12AgreementJson.X12DelimiterOverrides1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides = new List<X12AgreementJson.ValidationOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides = new List<X12AgreementJson.ValidationOverrides1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides = new List<X12AgreementJson.EnvelopeOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides == null)
            {
                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides = new List<X12AgreementJson.EnvelopeOverrides1>().ToArray();
            }
        }

        public void InitializeBaseEdifactAgreement(EDIFACTAgreementJson.Rootobject baseAgreementRootObj)
        {
            if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences == null)
            {
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences = new List<EDIFACTAgreementJson.Schemareference>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences == null)
            {
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences = new List<EDIFACTAgreementJson.Schemareference1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.edifactDelimiterOverrides = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.edifactDelimiterOverrides = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.validationOverrides = new List<EDIFACTAgreementJson.ValidationOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.validationOverrides = new List<EDIFACTAgreementJson.ValidationOverrides1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.envelopeOverrides = new List<EDIFACTAgreementJson.EnvelopeOverrides>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides == null)
            {
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.envelopeOverrides = new List<EDIFACTAgreementJson.EnvelopeOverrides1>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList == null)
            {
                baseAgreementRootObj.properties.content.edifact.receiveAgreement.protocolSettings.messageFilterList = new List<EDIFACTAgreementJson.EDIFACTMessageIdentifier>().ToArray();
            }
            if (baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList == null)
            {
                baseAgreementRootObj.properties.content.edifact.sendAgreement.protocolSettings.messageFilterList = new List<EDIFACTAgreementJson.EDIFACTMessageIdentifier>().ToArray();
            }
        }

        public X12AgreementJson.Rootobject ConsolidateTwoX12Agreements(X12AgreementJson.Rootobject baseAgreementRootObj, X12AgreementJson.Rootobject agreementJson)
        {
            try
            {
                InitializeBaseX12Agreement(baseAgreementRootObj);
                if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences != null && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Count() != 0)
                {
                    foreach (var schemaReference in agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences)
                    {
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Concat(new X12AgreementJson.Schemareference[] { schemaReference }).ToArray();
                        }
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides != null
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Length != 0
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) != 0)
                            {
                                var x12DelimiterOverride = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Where(x => x.messageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Concat(new X12AgreementJson.X12DelimiterOverrides[] { x12DelimiterOverride }).ToArray();
                            }
                            else
                            {
                                X12AgreementJson.X12DelimiterOverrides[] receiveDelimiterOverride = new X12AgreementJson.X12DelimiterOverrides[] { new X12AgreementJson.X12DelimiterOverrides()
                                {
                                    protocolVersion = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().schemaVersion,
                                    messageId = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().messageId,
                                    dataElementSeparator = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.DataElementSeparator,
                                    componentSeparator = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.ComponentSeparator,
                                    segmentTerminator = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.SegmentTerminator,
                                    segmentTerminatorSuffix = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.SegmentTerminatorSuffix,
                                    replaceCharacter = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.replaceCharacter,
                                    replaceSeparatorsInPayload = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.framingSettings.ReplaceSeparatorsInPayload,
                                    targetNamespace = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().schemaName
                                } };

                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Concat(receiveDelimiterOverride).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides != null
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Length != 0
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var validationOverride = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides[] { validationOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides[] { new X12AgreementJson.ValidationOverrides()
                                    {
                                        MessageId = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().messageId,
                                        AllowLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.allowLeadingAndTrailingSpacesAndZeroes,
                                        ValidateCharacterSet = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.validateCharacterSet,
                                        TrailingSeparatorPolicy = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.trailingSeparatorPolicy,
                                        TrimLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.trimLeadingAndTrailingSpacesAndZeroes,
                                        ValidateEDITypes = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.validateEDITypes,
                                        ValidateXSDTypes = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationSettings.validateXSDTypes
                                    } }).ToArray();
                            }
                        }


                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides != null
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Length != 0
                                && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var envelopeOverride = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides[] { envelopeOverride }).ToArray();
                            }
                            else
                            {

                                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides[] { new X12AgreementJson.EnvelopeOverrides()
                                    {
                                        DateFormat = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.GroupHeaderDateFormat,
                                        FunctionalIdentifierCode = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.FunctionalGroupId,
                                        HeaderVersion = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.GroupHeaderVersion,
                                        MessageId = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().messageId,
                                        ProtocolVersion = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().schemaVersion,
                                        ReceiverApplicationId = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.ReceiverApplicationId,
                                        ResponsibleAgencyCode = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.GroupHeaderAgencyCode,
                                        SenderApplicationId = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.SenderApplicationId,
                                        TargetNamespace = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.First().schemaName,
                                        TimeFormat = agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeSettings.GroupHeaderTimeFormat
                                    } }).ToArray();
                            }
                        }
                    }
                }


                if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences != null && agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Count() != 0)
                {
                    foreach (var schemaReference in agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences)
                    {
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Concat(new X12AgreementJson.Schemareference[] { schemaReference }).ToArray();
                        }
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides != null
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Length != 0
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == schemaReference.messageId) != 0)
                            {
                                var x12DelimiterOverride = agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Where(x => x.messageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Concat(new X12AgreementJson.X12DelimiterOverrides1[] { x12DelimiterOverride }).ToArray();
                            }
                            else
                            {
                                X12AgreementJson.X12DelimiterOverrides1[] sendDelimiterOverride = new X12AgreementJson.X12DelimiterOverrides1[] { new X12AgreementJson.X12DelimiterOverrides1()
                                {
                                    protocolVersion = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().schemaVersion,
                                    messageId = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().messageId,
                                    dataElementSeparator = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.DataElementSeparator,
                                    componentSeparator = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.ComponentSeparator,
                                    segmentTerminator = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.SegmentTerminator,
                                    segmentTerminatorSuffix = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.SegmentTerminatorSuffix,
                                    replaceCharacter = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.replaceCharacter,
                                    replaceSeparatorsInPayload = agreementJson.properties.content.x12.sendAgreement.protocolSettings.framingSettings.ReplaceSeparatorsInPayload,
                                    targetNamespace = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().schemaName
                                } };

                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Concat(sendDelimiterOverride).ToArray();
                            }
                        }
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides != null
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Length != 0
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var validationOverride = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides1[] { validationOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides1[] { new X12AgreementJson.ValidationOverrides1()
                                    {
                                        MessageId = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().messageId,
                                        AllowLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.allowLeadingAndTrailingSpacesAndZeroes,
                                        ValidateCharacterSet = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.validateCharacterSet,
                                        TrailingSeparatorPolicy = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.trailingSeparatorPolicy,
                                        TrimLeadingAndTrailingSpacesAndZeroes = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.trimLeadingAndTrailingSpacesAndZeroes,
                                        ValidateEDITypes = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.validateEDITypes,
                                        ValidateXSDTypes = agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationSettings.validateXSDTypes
                                    } }).ToArray();
                            }
                        }

                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) == 0)
                        {
                            if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides != null
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Length != 0
                                && agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == schemaReference.messageId) != 0)
                            {
                                var envelopeOverride = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Where(x => x.MessageId == schemaReference.messageId).First();
                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides1[] { envelopeOverride }).ToArray();
                            }
                            else
                            {
                                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides1[] { new X12AgreementJson.EnvelopeOverrides1()
                                    {
                                        DateFormat = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.GroupHeaderDateFormat,
                                        FunctionalIdentifierCode = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.FunctionalGroupId,
                                        HeaderVersion = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.GroupHeaderVersion,
                                        MessageId = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().messageId,
                                        ProtocolVersion = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().schemaVersion,
                                        ReceiverApplicationId = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.ReceiverApplicationId,
                                        ResponsibleAgencyCode = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.GroupHeaderAgencyCode,
                                        SenderApplicationId = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.SenderApplicationId,
                                        TargetNamespace = agreementJson.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.First().schemaName,
                                        TimeFormat = agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeSettings.GroupHeaderTimeFormat
                                    } }).ToArray();
                            }
                        }

                    }
                }


                if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides != null && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Length != 0)
                {
                    foreach (var x12DelimiterOverride in agreementJson.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == x12DelimiterOverride.messageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides.Concat(new X12AgreementJson.X12DelimiterOverrides[] { x12DelimiterOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides != null && agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Length != 0)
                {
                    foreach (var x12DelimiterOverride in agreementJson.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Count(x => x.messageId == x12DelimiterOverride.messageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides.Concat(new X12AgreementJson.X12DelimiterOverrides1[] { x12DelimiterOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides != null && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Length != 0)
                {
                    foreach (var validationOverride in agreementJson.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == validationOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides[] { validationOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides != null && agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Length != 0)
                {
                    foreach (var validationOverride in agreementJson.properties.content.x12.sendAgreement.protocolSettings.validationOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Count(x => x.MessageId == validationOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.validationOverrides.Concat(new X12AgreementJson.ValidationOverrides1[] { validationOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides != null && agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Length != 0)
                {
                    foreach (var envelopeOverride in agreementJson.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == envelopeOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides[] { envelopeOverride }).ToArray();
                        }
                    }
                }
                if (agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides != null && agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Length != 0)
                {
                    foreach (var envelopeOverride in agreementJson.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides)
                    {
                        if (baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Count(x => x.MessageId == envelopeOverride.MessageId) == 0)
                        {

                            baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides = baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides.Concat(new X12AgreementJson.EnvelopeOverrides1[] { envelopeOverride }).ToArray();
                        }
                    }
                }
                return baseAgreementRootObj;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public X12AgreementJson.Rootobject ConsolidateX12Agreements(AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                CreateIndividualX12Agreements(agreementItem);
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
            try
            {
                X12AgreementJson.Rootobject baseAgreementRootObj = GetX12AgreementJson(agreementItem.BaseAgreementName);
                baseAgreementRootObj.name = agreementItem.Name;
                baseAgreementRootObj.type = Resources.AgreementType;
                baseAgreementRootObj.properties.guestIdentity = new X12AgreementJson.Guestidentity { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.hostIdentity = new X12AgreementJson.Hostidentity { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.x12.receiveAgreement.receiverBusinessIdentity = new X12AgreementJson.Receiverbusinessidentity { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.x12.receiveAgreement.senderBusinessIdentity = new X12AgreementJson.Senderbusinessidentity { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.content.x12.sendAgreement.receiverBusinessIdentity = new X12AgreementJson.Receiverbusinessidentity1 { qualifier = agreementItem.GuestPartnerQualifer, value = agreementItem.GuestPartnerId };
                baseAgreementRootObj.properties.content.x12.sendAgreement.senderBusinessIdentity = new X12AgreementJson.Senderbusinessidentity1 { qualifier = agreementItem.HostPartnerQualifier, value = agreementItem.HostPartnerId };
                baseAgreementRootObj.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides = new List<X12AgreementJson.X12DelimiterOverrides>().ToArray();
                baseAgreementRootObj.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides = new List<X12AgreementJson.X12DelimiterOverrides1>().ToArray();
                foreach (var agreement in agreementItem.AgreementsToConsolidate)
                {
                    if (agreement.Name != agreementItem.BaseAgreementName)
                    {
                        try
                        {
                            X12AgreementJson.Rootobject agreementJson = GetX12AgreementJson(agreement.Name);
                            baseAgreementRootObj = ConsolidateTwoX12Agreements(baseAgreementRootObj, agreementJson);

                        }
                        catch (Exception ex)
                        {
                            TraceProvider.WriteLine(string.Format("Skipping agreement {0} while consolidating in agreement {1}. Reason: {2}", agreement.Name, agreementItem.Name, ExceptionHelper.GetExceptionMessage(ex)));
                            continue;
                        }
                    }

                }
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(baseAgreementRootObj.name), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(baseAgreementRootObj);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                TraceProvider.WriteLine(string.Format("Agreement {0} Exported to Json with Name {1}", baseAgreementRootObj.name, FileOperations.GetFileName(baseAgreementRootObj.name)));
                return baseAgreementRootObj;
            }
            catch (Exception ex)
            {
                agreementItem.ImportStatus = MigrationStatus.Failed;
                agreementItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public X12AgreementJson.Rootobject GetX12AgreementJson(string agreementName)
        {
            try
            {
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agreementName), ".json");
                string partnerJsonFileContent = File.ReadAllText(fileName);
                X12AgreementJson.Rootobject obj = JsonConvert.DeserializeObject<X12AgreementJson.Rootobject>(partnerJsonFileContent);
                return obj;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error while Consolidating agreements. Cannot Read the agreement {0} from local folder. Reason : {1}", agreementName, ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Error while Consolidating agreements. Cannot Read the agreement {0} from local folder. Reason : {1}", agreementName, ExceptionHelper.GetExceptionMessage(ex)));
            }
        }
        #endregion


        #region AS2 Helpers

        public string GetHostCertificateName(Server.OnewayAgreement onewayAgreementBA, string partner, Server.TpmContext bizTalkTpmContext)
        {
            StringBuilder sb = new StringBuilder(string.Empty);
            Server.AS2ProtocolSettings pSettings = onewayAgreementBA.GetProtocolSettings<Server.AS2ProtocolSettings>();
            KeyValuePair<string, string> certFromMappingFile = partnerCertificateMappings.ContainsKey(partner) ? partnerCertificateMappings[partner] : new KeyValuePair<string, string>(null, null);
            Certificate certNameFromMappingFile = new Certificate
            {
                certificateName = certFromMappingFile.Key,
                certificateThumbprint = certFromMappingFile.Value
            };
            Certificate certNameFromAgreement = new Certificate
            {
                certificateName = pSettings.SecuritySettings.SigningCertificateName,
                certificateThumbprint = pSettings.SecuritySettings.SigningCertificateThumbprint
            };

            Certificate certNameFromPartner = new Certificate
            {
                certificateName = bizTalkTpmContext.Partners.Where(p => p.Name == partner).First().CertificateName,
                certificateThumbprint = bizTalkTpmContext.Partners.Where(p => p.Name == partner).First().CertificateThumbprint
            };
            if ((certNameFromMappingFile.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != certNameFromMappingFile.certificateThumbprint) ||
                (certNameFromMappingFile.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != certNameFromMappingFile.certificateThumbprint) ||
                (certNameFromPartner.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != certNameFromAgreement.certificateThumbprint))
            {
                sb.AppendLine("Warning: Multiple Certificates Name gathered from Biztalk.");
                if (certNameFromMappingFile != null)
                {
                    sb.AppendLine("Certificate Name from Mapping File: Name- " + certNameFromMappingFile.certificateName + "  Thumbprint- " + certNameFromMappingFile.certificateThumbprint);
                }
                if (certNameFromAgreement != null)
                {
                    sb.AppendLine("Certificate Name from Agreement:Name- " + certNameFromAgreement.certificateName + "  Thumbprint- " + certNameFromAgreement.certificateThumbprint);
                }
                if (certNameFromPartner != null)
                {
                    sb.AppendLine("Certificate Name from Partner: Name- " + certNameFromPartner.certificateName + "  Thumbprint- " + certNameFromPartner.certificateThumbprint);
                }
            }
            if (sb.ToString() != string.Empty)
            {
                TraceProvider.WriteLine(sb.ToString(), true);
            }
            string certExtracted = string.Empty;
            Certificate hostcert;
            List<Certificate> certs = thisApplicationContext.GetProperty("OtherCertificates") as List<Certificate>;
            if (!string.IsNullOrEmpty(certNameFromMappingFile.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromMappingFile.certificateThumbprint) != 0)
                {
                    hostcert = certs.Where(x => x.certificateThumbprint == certNameFromMappingFile.certificateThumbprint).First();
                }
                else
                {
                    hostcert = certNameFromMappingFile;
                    certExtracted = TryExtractCertificate(certNameFromMappingFile, "Host");
                }
            }
            else if (!string.IsNullOrEmpty(certNameFromPartner.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromPartner.certificateThumbprint) != 0)
                {
                    hostcert = certs.Where(x => x.certificateThumbprint == certNameFromPartner.certificateThumbprint).First();
                }
                else
                {
                    hostcert = certNameFromPartner;
                    certExtracted = TryExtractCertificate(certNameFromPartner, "Host");
                }
            }
            else if (!string.IsNullOrEmpty(certNameFromAgreement.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromAgreement.certificateThumbprint) != 0)
                {
                    hostcert = certs.Where(x => x.certificateThumbprint == certNameFromAgreement.certificateThumbprint).First();
                }
                else
                {
                    hostcert = certNameFromAgreement;
                    certExtracted = TryExtractCertificate(certNameFromAgreement, "Host");
                }
            }
            else
            {
                TraceProvider.WriteLine("Host certificate used in the agreement could not be known", true);
                return string.Empty;
            }
            if (certExtracted == "Success")
            {
                hostcert.ImportStatus = MigrationStatus.Succeeded;

                if (certs == null || certs.Count == 0)
                {
                    certs = new List<Certificate>();
                    certs.Add(hostcert);
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
                else
                {
                    if (certs.Count(x => x.certificateThumbprint == hostcert.certificateThumbprint) == 0)
                    {
                        certs.Add(hostcert);
                    }
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
            }
            else if (certExtracted == "Fail")
            {
                hostcert.ImportStatus = MigrationStatus.Failed;
                if (certs == null || certs.Count == 0)
                {
                    certs = new List<Certificate>();
                    certs.Add(hostcert);
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
                else
                {
                    if (certs.Count(x => x.certificateThumbprint == hostcert.certificateThumbprint) == 0)
                    {
                        certs.Add(hostcert);
                    }
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
            }
            return FileOperations.GetFileName(hostcert.certificateName);
        }

        public string GetGuestCertificateName(Server.OnewayAgreement onewayAgreementAB, Server.OnewayAgreement onewayAgreementBA, string partner, Server.TpmContext bizTalkTpmContext)
        {
            StringBuilder sb = new StringBuilder(string.Empty);
            Server.AS2ProtocolSettings pSettings = onewayAgreementAB.GetProtocolSettings<Server.AS2ProtocolSettings>();
            KeyValuePair<string, string> certFromMappingFile = partnerCertificateMappings.ContainsKey(partner) ? partnerCertificateMappings[partner] : new KeyValuePair<string, string>(null, null);
            Certificate certNameFromMappingFile = new Certificate
            {
                certificateName = certFromMappingFile.Key,
                certificateThumbprint = certFromMappingFile.Value
            };
            SendPortDetails certFromSendPort = GetAgreementSendPortDetails(onewayAgreementBA);
            Certificate certNameFromSendPort = new Certificate
            {
                certificateName = certFromSendPort.CertificateName,
                certificateThumbprint = certFromSendPort.CertificateThumbprint
            };
            Certificate certNameFromAgreement = new Certificate
            {
                certificateName = pSettings.SecuritySettings.SigningCertificateName,
                certificateThumbprint = pSettings.SecuritySettings.SigningCertificateThumbprint
            };

            Certificate certNameFromPartner = new Certificate
            {
                certificateName = bizTalkTpmContext.Partners.Where(p => p.Name == partner).First().CertificateName,
                certificateThumbprint = bizTalkTpmContext.Partners.Where(p => p.Name == partner).First().CertificateThumbprint
            };
            if ((certNameFromMappingFile.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != certNameFromMappingFile.certificateThumbprint) ||
                (certNameFromMappingFile.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != certNameFromMappingFile.certificateThumbprint) ||
                (certNameFromMappingFile.certificateThumbprint != null && certNameFromSendPort.certificateThumbprint != null && certNameFromSendPort.certificateThumbprint != certNameFromMappingFile.certificateThumbprint) ||
                (certNameFromPartner.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != certNameFromAgreement.certificateThumbprint) ||
                (certNameFromPartner.certificateThumbprint != null && certNameFromSendPort.certificateThumbprint != null && certNameFromPartner.certificateThumbprint != certNameFromSendPort.certificateThumbprint) ||
                (certNameFromAgreement.certificateThumbprint != null && certNameFromSendPort.certificateThumbprint != null && certNameFromAgreement.certificateThumbprint != certNameFromSendPort.certificateThumbprint))
            {
                sb.AppendLine("Warning: Multiple Certificates Name gathered from Biztalk.");
                if (certNameFromMappingFile != null)
                {
                    sb.AppendLine("Certificate Name from Mapping File: Name- " + certNameFromMappingFile.certificateName + "  Thumbprint- " + certNameFromMappingFile.certificateThumbprint);
                }
                if (certNameFromSendPort != null)
                {
                    sb.AppendLine("Certificate Name from Sendport: Name- " + certNameFromSendPort.certificateName + "  Thumbprint- " + certNameFromSendPort.certificateThumbprint);
                }
                if (certNameFromAgreement != null)
                {
                    sb.AppendLine("Certificate Name from Agreement: Name- " + certNameFromAgreement.certificateName + "  Thumbprint- " + certNameFromAgreement.certificateThumbprint);
                }
                if (certNameFromPartner != null)
                {
                    sb.AppendLine("Certificate Name from Partner: Name- " + certNameFromPartner.certificateName + "  Thumbprint- " + certNameFromPartner.certificateThumbprint);
                }
            }
            if (sb.ToString() != string.Empty)
            {
                TraceProvider.WriteLine(sb.ToString(), true);
            }
            string certExtracted = string.Empty;
            Certificate guestCert;
            List<Certificate> certs = thisApplicationContext.GetProperty("OtherCertificates") as List<Certificate>;
            if (!string.IsNullOrEmpty(certNameFromMappingFile.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromMappingFile.certificateThumbprint) != 0)
                {
                    guestCert = certs.Where(x => x.certificateThumbprint == certNameFromMappingFile.certificateThumbprint).First();
                }
                else
                {
                    guestCert = certNameFromMappingFile;
                    certExtracted = TryExtractCertificate(certNameFromMappingFile, "Guest");
                }
            }
            else if (!string.IsNullOrEmpty(certNameFromPartner.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromPartner.certificateThumbprint) != 0)
                {
                    guestCert = certs.Where(x => x.certificateThumbprint == certNameFromPartner.certificateThumbprint).First();
                }
                else
                {
                    guestCert = certNameFromPartner;
                    certExtracted = TryExtractCertificate(certNameFromPartner, "Guest");
                }
            }
            else if (!string.IsNullOrEmpty(certNameFromSendPort.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromSendPort.certificateThumbprint) != 0)
                {
                    guestCert = certs.Where(x => x.certificateThumbprint == certNameFromSendPort.certificateThumbprint).First();
                }
                else
                {
                    guestCert = certNameFromSendPort;
                    certExtracted = TryExtractCertificate(certNameFromSendPort, "Guest");
                }
            }
            else if (!string.IsNullOrEmpty(certNameFromAgreement.certificateName))
            {
                if (certs != null && certs.Count(x => x.certificateThumbprint == certNameFromAgreement.certificateThumbprint) != 0)
                {
                    guestCert = certs.Where(x => x.certificateThumbprint == certNameFromAgreement.certificateThumbprint).First();
                }
                else
                {
                    guestCert = certNameFromAgreement;
                    certExtracted = TryExtractCertificate(certNameFromAgreement, "Guest");
                }
            }
            else
            {
                TraceProvider.WriteLine("Guest certificate used in the agreement could not be known", true);
                return string.Empty;
            }
            if (certExtracted == "Success")
            {
                guestCert.ImportStatus = MigrationStatus.Succeeded;
                if (certs == null || certs.Count == 0)
                {
                    certs = new List<Certificate>();
                    certs.Add(guestCert);
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
                else
                {
                    if (certs.Count(x => x.certificateThumbprint == guestCert.certificateThumbprint) == 0)
                    {
                        certs.Add(guestCert);
                    }
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
            }
            else if (certExtracted == "Fail")
            {
                guestCert.ImportStatus = MigrationStatus.Failed;
                if (certs == null || certs.Count == 0)
                {
                    certs = new List<Certificate>();
                    certs.Add(guestCert);
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
                else
                {
                    if (certs.Count(x => x.certificateThumbprint == guestCert.certificateThumbprint) == 0)
                    {
                        certs.Add(guestCert);
                    }
                    thisApplicationContext.SetProperty("OtherCertificates", certs);
                }
            }
            return FileOperations.GetFileName(guestCert.certificateName);
        }

        private string TryExtractCertificate(Certificate certificate, string dir)
        {
            TraceProvider.WriteLine(string.Format("{0} certificate used in the agreement is: {1}", dir, certificate.certificateName), true);
            var selectedPartners = thisApplicationContext.GetProperty(AppConstants.SelectedPartnersContextPropertyName) as IEnumerable<PartnerMigrationItemViewModel>;
            var importedPartners = selectedPartners.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            var extractedCerts = importedPartners.Where(item => item.CertificateImportStatus == MigrationStatus.Succeeded);
            try
            {
                if (extractedCerts.Where(item => item.MigrationEntity.CertificateThumbprint == certificate.certificateThumbprint).Count() != 0)
                {
                    TraceProvider.WriteLine("Certificate has already been extracted");
                    return "Exists";
                }
                else
                {
                    TraceProvider.WriteLine("Trying to extract certificate");
                    CerificateMigrator<Certificate> cmigrator = new CerificateMigrator<Certificate>(thisApplicationContext);
                    cmigrator.CreateCertificates(certificate.certificateName, certificate.certificateThumbprint);
                    return "Success";
                }
            }
            catch (Exception ex)
            {
                return "Fail";
            }
        }
        private SendPortDetails GetAgreementSendPortDetails(Server.OnewayAgreement receiverPartnerOneWayAgreement)
        {
            SendPortDetails sendPortDetails = new SendPortDetails();
            IEnumerable<Server.SendPortReference> agreementSendPorts = receiverPartnerOneWayAgreement.GetSendPortAssociations();

            using (BtsCatalogExplorer catalogExplorer = new BtsCatalogExplorer())
            {
                catalogExplorer.ConnectionString = thisApplicationContext.GetProperty("DatabaseConnectionString") as string;
                foreach (var sendport in agreementSendPorts)
                {
                    BizTalk.ExplorerOM.SendPort agrsendport = catalogExplorer.SendPorts[sendport.Name];
                    if (agrsendport != null)
                    {
                        sendPortDetails.As2Url = agrsendport.PrimaryTransport.Address;
                        sendPortDetails.CertificateThumbprint = agrsendport.EncryptionCert.ThumbPrint;
                        sendPortDetails.CertificateName = agrsendport.EncryptionCert.ShortName;
                        break;
                    }
                }
            }

            return sendPortDetails;
        }

        public string GetHashingAlgorithmName(string hashingAlgorithmName)
        {
            return hashingAlgorithmMappings.ContainsKey(hashingAlgorithmName) ? hashingAlgorithmMappings[hashingAlgorithmName] : hashingAlgorithmName;
        }
        #endregion

        #region Edifact Helpers

        public EDIFACTAgreementJson.EDIFACTMessageIdentifier[] GetMessageFilterList(Server.EDIFACTProtocolSettings ps)
        {
            List<EDIFACTAgreementJson.EDIFACTMessageIdentifier> list = new List<EDIFACTAgreementJson.EDIFACTMessageIdentifier>();

            foreach (var item in ps.MessageFilterList.ToList())
            {
                list.Add(new EDIFACTAgreementJson.EDIFACTMessageIdentifier()
                {
                    MessageId = item.MessageId
                });
            }
            return list.ToArray();
        }

        public EDIFACTAgreementJson.EDIFACTMessageIdentifier[] GetMessageFilterList1(Server.EDIFACTProtocolSettings ps)
        {
            List<EDIFACTAgreementJson.EDIFACTMessageIdentifier> list = new List<EDIFACTAgreementJson.EDIFACTMessageIdentifier>();

            foreach (var item in ps.MessageFilterList.ToList())
            {
                list.Add(new EDIFACTAgreementJson.EDIFACTMessageIdentifier()
                {
                    MessageId = item.MessageId
                });
            }
            return list.ToArray();
        }


        public EDIFACTAgreementJson.ValidationOverrides[] GetvalidationOverrides(Server.EDIFACTValidationSettings input)
        {
            List<Server.EDIFACTValidationOverrides> result = input.GetOverrides().ToList();
            List<EDIFACTAgreementJson.ValidationOverrides> x = new List<EDIFACTAgreementJson.ValidationOverrides>();
            result.Where(delegate (Server.EDIFACTValidationOverrides eo)
            {
                x.Add(new EDIFACTAgreementJson.ValidationOverrides()
                {
                    MessageId = eo.MessageId,
                    AllowLeadingAndTrailingSpacesAndZeroes = eo.AllowLeadingAndTrailingSpacesAndZeroes,
                    EnforceCharacterSet = eo.EnforceCharacterSet,
                    TrailingSeparatorPolicy = eo.TrailingSeparatorPolicy.ToString(),
                    TrimLeadingAndTrailingSpacesAndZeroes = eo.TrimLeadingAndTrailingSpacesAndZeroes,
                    ValidateEDITypes = eo.ValidateEDITypes,
                    ValidateXSDTypes = eo.ValidateXSDTypes
                });
                return true;
            }
            ).ToArray();
            return x.ToArray();
        }

        public EDIFACTAgreementJson.ValidationOverrides1[] GetvalidationOverrides1(Server.EDIFACTValidationSettings input)
        {
            List<Server.EDIFACTValidationOverrides> result = input.GetOverrides().ToList();
            List<EDIFACTAgreementJson.ValidationOverrides1> x = new List<EDIFACTAgreementJson.ValidationOverrides1>();
            result.Where(delegate (Server.EDIFACTValidationOverrides eo)
            {
                x.Add(new EDIFACTAgreementJson.ValidationOverrides1()
                {
                    MessageId = eo.MessageId,
                    AllowLeadingAndTrailingSpacesAndZeroes = eo.AllowLeadingAndTrailingSpacesAndZeroes,
                    EnforceCharacterSet = eo.EnforceCharacterSet,
                    TrailingSeparatorPolicy = eo.TrailingSeparatorPolicy.ToString(),
                    TrimLeadingAndTrailingSpacesAndZeroes = eo.TrimLeadingAndTrailingSpacesAndZeroes,
                    ValidateEDITypes = eo.ValidateEDITypes,
                    ValidateXSDTypes = eo.ValidateXSDTypes
                });
                return true;
            }
            ).ToArray();
            return x.ToArray();
        }

        public EDIFACTAgreementJson.EdifactDelimiterOverrides[] GetEdifactDelimiterOverrides()
        {
            List<EDIFACTAgreementJson.EdifactDelimiterOverrides> result = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides>();
            return result.ToArray();
        }

        public EDIFACTAgreementJson.EdifactDelimiterOverrides1[] GetEdifactDelimiterOverrides1()
        {
            List<EDIFACTAgreementJson.EdifactDelimiterOverrides1> result = new List<EDIFACTAgreementJson.EdifactDelimiterOverrides1>();
            return result.ToArray();
        }

        public EDIFACTAgreementJson.EnvelopeOverrides[] GetenvelopeOverrides(Server.EDIFACTProtocolSettings ps)
        {
            Server.EDIFACTEnvelopeSettings serverEDIFACTEnvelopeSettings = ps.EnvelopeSettings;
            List<Server.EDIFACTEnvelopeOverrides> serverEDIOverRides = serverEDIFACTEnvelopeSettings.GetOverrides().ToList();
            List<EDIFACTAgreementJson.EnvelopeOverrides> x = new List<EDIFACTAgreementJson.EnvelopeOverrides>();
            foreach (var item in serverEDIOverRides)
            {
                x.Add(new EDIFACTAgreementJson.EnvelopeOverrides()
                {
                    ApplicationPassword = item.ApplicationPassword,
                    AssociationAssignedCode = item.AssociationAssignedCode,
                    ControllingAgencyCode = item.ControllingAgencyCode,
                    FunctionalGroupId = item.FunctionalGroupId,
                    GroupHeaderMessageRelease = item.GroupHeaderMessageRelease,
                    GroupHeaderMessageVersion = item.GroupHeaderMessageVersion,
                    MessageAssociationAssignedCode = item.MessageAssociationAssignedCode,
                    MessageId = item.MessageId,
                    MessageRelease = item.MessageRelease,
                    MessageVersion = item.MessageVersion,
                    ReceiverApplicationId = item.ReceiverApplicationId,
                    ReceiverApplicationQualifier = item.ReceiverApplicationQualifier,
                    SenderApplicationId = item.SenderApplicationId,
                    SenderApplicationQualifier = item.SenderApplicationQualifier,
                    TargetNamespace = item.TargetNamespace
                });
            }


            return x.ToArray();

        }

        public Dictionary<string, string> CheckAllEdifactSchemaReferences(EDIFACTAgreementJson.Schemareference[] schemareferences)
        {
            StringBuilder sb1 = new StringBuilder("");
            StringBuilder sb2 = new StringBuilder("");
            Dictionary<string, string> checkResult = new Dictionary<string, string>();
            foreach (var edifactschema in schemareferences)
            {
                if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsValue(edifactschema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} mapped to schema {1} from IA", edifactschema.schemaName, schemasInIA.Where(item => item.Value == edifactschema.schemaName).First().Key));
                }
                else if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsKey(edifactschema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference found for schema {0} in IA", schemasInIA.Where(item => item.Key == edifactschema.schemaName).First().Key));
                }
                else
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} not found in IA", edifactschema.schemaName));
                    sb2.AppendLine(string.Format("Error: Schema Reference with Target Namespace {0} in agreement doesn't exist in IA. Please migrate the schema before migrating the agreement.", edifactschema.schemaName));
                }
            }
            checkResult.Add("Found", sb1.ToString());
            checkResult.Add("NotFound", sb2.ToString());
            return checkResult;
        }

        public Dictionary<string, string> CheckAllEdifactSchemaReferences1(EDIFACTAgreementJson.Schemareference1[] schemareferences)
        {
            StringBuilder sb1 = new StringBuilder("");
            StringBuilder sb2 = new StringBuilder("");
            Dictionary<string, string> checkResult = new Dictionary<string, string>();
            foreach (var edifactschema in schemareferences)
            {
                if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsValue(edifactschema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} mapped to schema {1} from IA", edifactschema.schemaName, schemasInIA.Where(item => item.Value == edifactschema.schemaName).First().Key));
                }
                else if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsKey(edifactschema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference found for schema {0} in IA", schemasInIA.Where(item => item.Key == edifactschema.schemaName).First().Key));
                }
                else
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} not found in IA", edifactschema.schemaName));
                    sb2.AppendLine(string.Format("Error: Schema Reference with Target Namespace {0} in agreement doesn't exist in IA. Please migrate the schema before migrating the agreement.", edifactschema.schemaName));
                }
            }
            checkResult.Add("Found", sb1.ToString());
            checkResult.Add("NotFound", sb2.ToString());
            return checkResult;
        }

        public EDIFACTAgreementJson.Schemareference[] GetSchemaReference(Server.EDIFACTSchemaSettings serverEDISchemaSettings)
        {
            List<Server.EDIFACTSchemaOverrides> serverEDIOverRides = serverEDISchemaSettings.GetOverrides().ToList();
            List<EDIFACTAgreementJson.Schemareference> x = new List<EDIFACTAgreementJson.Schemareference>();



            serverEDIOverRides.Where(delegate (Server.EDIFACTSchemaOverrides eo)
            {
                x.Add(new EDIFACTAgreementJson.Schemareference()
                {
                    messageId = eo.MessageId,
                    messageRelease = eo.MessageRelease,
                    messageVersion = eo.MessageVersion,
                    // schemaName = GeSchemaNameByTargetNameSpace(eo.TargetNamespace)
                    schemaName = eo.TargetNamespace
                });
                return true;
            }
             ).ToArray();
            return x.ToArray();

        }

        public EDIFACTAgreementJson.EnvelopeOverrides1[] GetenvelopeOverrides1(Server.EDIFACTEnvelopeSettings serverEDifactEs)
        {

            List<Server.EDIFACTEnvelopeOverrides> serverEDIOverRides = serverEDifactEs.GetOverrides().ToList();
            List<EDIFACTAgreementJson.EnvelopeOverrides1> x = new List<EDIFACTAgreementJson.EnvelopeOverrides1>();
            serverEDIOverRides.Where(delegate (Server.EDIFACTEnvelopeOverrides eo)
            {
                x.Add(new EDIFACTAgreementJson.EnvelopeOverrides1()
                {
                    ApplicationPassword = eo.ApplicationPassword,
                    AssociationAssignedCode = eo.AssociationAssignedCode,
                    ControllingAgencyCode = eo.ControllingAgencyCode,
                    FunctionalGroupId = eo.FunctionalGroupId,
                    GroupHeaderMessageRelease = eo.GroupHeaderMessageVersion,
                    GroupHeaderMessageVersion = eo.GroupHeaderMessageVersion,
                    MessageId = eo.MessageId,
                    MessageRelease = eo.MessageRelease,
                    MessageVersion = eo.MessageVersion,
                    ReceiverApplicationId = eo.ReceiverApplicationId,
                    ReceiverApplicationQualifier = eo.ReceiverApplicationQualifier,
                    SenderApplicationId = eo.SenderApplicationId,
                    SenderApplicationQualifier = eo.SenderApplicationQualifier,
                    TargetNamespace = eo.TargetNamespace
                });
                return true;
            }).ToArray();
            return x.ToArray();


        }

        public EDIFACTAgreementJson.Schemareference1[] GetSchemaReference1(Server.EDIFACTSchemaSettings serverEDISchemaSettings)
        {
            List<Server.EDIFACTSchemaOverrides> serverEDIOverRides = serverEDISchemaSettings.GetOverrides().ToList();
            List<EDIFACTAgreementJson.Schemareference1> x = new List<EDIFACTAgreementJson.Schemareference1>();


            serverEDIOverRides.Where(delegate (Server.EDIFACTSchemaOverrides eo)
            {
                x.Add(new EDIFACTAgreementJson.Schemareference1()
                {
                    messageId = eo.MessageId,
                    messageRelease = eo.MessageRelease,
                    messageVersion = eo.MessageVersion,
                    // schemaName = GeSchemaNameByTargetNameSpace(eo.TargetNamespace)
                    schemaName = eo.TargetNamespace
                });
                return true;
            }).ToArray();
            return x.ToArray();

        }
        #endregion


        #region X12 Helpers

        public X12AgreementJson.EnvelopeOverrides[] X12GetenvelopeOverrides(Server.X12ProtocolSettings ps)
        {
            Server.X12EnvelopeSettings serverX12EnvelopeSettings = ps.EnvelopeSettings;
            List<Server.X12EnvelopeOverrides> serverEDIOverRides = serverX12EnvelopeSettings.GetOverrides().ToList();
            List<X12AgreementJson.EnvelopeOverrides> x = new List<X12AgreementJson.EnvelopeOverrides>();
            foreach (var item in serverEDIOverRides)
            {
                x.Add(new X12AgreementJson.EnvelopeOverrides()
                {
                    DateFormat = item.DateFormat.ToString(),
                    FunctionalIdentifierCode = item.FunctionalIdentifierCode,
                    HeaderVersion = item.HeaderVersion,
                    MessageId = item.MessageId,
                    ProtocolVersion = item.ProtocolVersion,
                    ReceiverApplicationId = item.ReceiverApplicationId,
                    ResponsibleAgencyCode = item.ResponsibleAgencyCode,
                    SenderApplicationId = item.SenderApplicationId,
                    TargetNamespace = item.TargetNamespace,
                    TimeFormat = item.TimeFormat.ToString()
                });
            }
            return x.ToArray();
        }
        public X12AgreementJson.EnvelopeOverrides1[] X12GetenvelopeOverrides1(Server.X12ProtocolSettings ps)
        {
            Server.X12EnvelopeSettings serverX12EnvelopeSettings = ps.EnvelopeSettings;
            List<Server.X12EnvelopeOverrides> serverEDIOverRides = serverX12EnvelopeSettings.GetOverrides().ToList();
            List<X12AgreementJson.EnvelopeOverrides1> x = new List<X12AgreementJson.EnvelopeOverrides1>();
            foreach (var item in serverEDIOverRides)
            {
                x.Add(new X12AgreementJson.EnvelopeOverrides1()
                {
                    DateFormat = item.DateFormat.ToString(),
                    FunctionalIdentifierCode = item.FunctionalIdentifierCode,
                    HeaderVersion = item.HeaderVersion,
                    MessageId = item.MessageId,
                    ProtocolVersion = item.ProtocolVersion,
                    ReceiverApplicationId = item.ReceiverApplicationId,
                    ResponsibleAgencyCode = item.ResponsibleAgencyCode,
                    SenderApplicationId = item.SenderApplicationId,
                    TargetNamespace = item.TargetNamespace,
                    TimeFormat = item.TimeFormat.ToString()
                });
            }
            return x.ToArray();
        }
        public Dictionary<string, string> CheckAllX12SchemaReferences(X12AgreementJson.Schemareference[] schemaReferences)
        {
            StringBuilder sb1 = new StringBuilder("");
            StringBuilder sb2 = new StringBuilder("");
            Dictionary<string, string> checkResult = new Dictionary<string, string>();
            foreach (var x12schema in schemaReferences)
            {
                if (schemasInIA.Count != 0 && schemasInIA.ContainsValue(x12schema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} mapped to schema {1} from IA", x12schema.schemaName, schemasInIA.Where(item => item.Value == x12schema.schemaName).First().Key));
                }
                else if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsKey(x12schema.schemaName))
                {
                    sb1.AppendLine(string.Format("Schema Reference found for schema {0} in IA", schemasInIA.Where(item => item.Key == x12schema.schemaName).First().Key));
                }
                else
                {
                    sb1.AppendLine(string.Format("Schema Reference {0} not found in IA", x12schema.schemaName));
                    sb2.AppendLine(string.Format("Error: Schema Reference with Target Namespace {0} in agreement doesn't exist in IA. Please migrate the schema before migrating the agreement.", x12schema.schemaName));
                }
            }
            checkResult.Add("Found", sb1.ToString());
            checkResult.Add("NotFound", sb2.ToString());
            return checkResult;
        }


        public X12AgreementJson.Schemareference[] X12GetSchemaReference(Server.X12SchemaSettings serverEDISchemaSettings)
        {
            List<Server.X12SchemaOverrides> serverEDIOverRides = serverEDISchemaSettings.GetOverrides().ToList();
            List<X12AgreementJson.Schemareference> x = new List<X12AgreementJson.Schemareference>();

            serverEDIOverRides.Where(delegate (Server.X12SchemaOverrides eo)
            {
                string version;
                string name = eo.TargetNamespace;
                x.Add(new X12AgreementJson.Schemareference()
                {
                    messageId = eo.MessageId,
                    senderApplicationId = eo.SenderApplicationId,
                    schemaName = name,
                    schemaVersion = name
                });
                return true;
            }
             ).ToArray();
            return x.ToArray();
            //return x.ToArray();
        }

        public X12AgreementJson.ValidationOverrides[] X12GetvalidationOverrides(Server.X12ValidationSettings input)
        {
            List<Server.X12ValidationOverrides> result = input.GetOverrides().ToList();
            List<X12AgreementJson.ValidationOverrides> x = new List<X12AgreementJson.ValidationOverrides>();

            result.Where(delegate (Server.X12ValidationOverrides eo)
            {
                x.Add(new X12AgreementJson.ValidationOverrides()
                {
                    MessageId = eo.MessageId,
                    AllowLeadingAndTrailingSpacesAndZeroes = eo.AllowLeadingAndTrailingSpacesAndZeroes,
                    ValidateCharacterSet = eo.ValidateCharacterSet,
                    TrailingSeparatorPolicy = eo.TrailingSeparatorPolicy.ToString(),
                    TrimLeadingAndTrailingSpacesAndZeroes = eo.TrimLeadingAndTrailingSpacesAndZeroes,
                    ValidateEDITypes = eo.ValidateEDITypes,
                    ValidateXSDTypes = eo.ValidateXSDTypes
                });
                return true;
            }
            ).ToArray();
            return x.ToArray();
        }

        public X12AgreementJson.ValidationOverrides1[] X12GetvalidationOverrides1(Server.X12ValidationSettings input)
        {
            List<Server.X12ValidationOverrides> result = input.GetOverrides().ToList();
            List<X12AgreementJson.ValidationOverrides1> x = new List<X12AgreementJson.ValidationOverrides1>();

            result.Where(delegate (Server.X12ValidationOverrides eo)
            {
                x.Add(new X12AgreementJson.ValidationOverrides1()
                {
                    MessageId = eo.MessageId,
                    AllowLeadingAndTrailingSpacesAndZeroes = eo.AllowLeadingAndTrailingSpacesAndZeroes,
                    ValidateCharacterSet = eo.ValidateCharacterSet,
                    TrailingSeparatorPolicy = eo.TrailingSeparatorPolicy.ToString(),
                    TrimLeadingAndTrailingSpacesAndZeroes = eo.TrimLeadingAndTrailingSpacesAndZeroes,
                    ValidateEDITypes = eo.ValidateEDITypes,
                    ValidateXSDTypes = eo.ValidateXSDTypes
                });
                return true;
            }
            ).ToArray();
            return x.ToArray();
        }
        #endregion

        #region Other Helpers

        public static async Task<Dictionary<string, string>> GetAllSchemasInIA(IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            Dictionary<string, string> schemaInIA = new Dictionary<string, string>();
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                string responseAsString = "";
                JObject responseAsJObject = null;
                string url = UrlHelper.GetSchemasUrl(iaDetails);
                while (true)
                {
                    response = sclient.GetArtifactsFromIA(url, authResult);
                    responseAsString = await response.Content.ReadAsStringAsync();
                    responseAsJObject = JsonConvert.DeserializeObject<JObject>(responseAsString);
                    if (responseAsJObject.GetValue("value") != null)
                    {
                        foreach (var obj in responseAsJObject["value"])
                        {
                            if (obj["properties"] != null && obj["name"] != null && obj["properties"]["targetNamespace"] != null)
                            {
                                if (!schemaInIA.ContainsKey(obj["name"].ToString()))
                                {
                                    schemaInIA.Add(obj["name"].ToString(), obj["properties"]["targetNamespace"].ToString());
                                }
                            }
                        }
                    }
                    if (responseAsJObject["nextLink"] != null)
                    {
                        url = (string)responseAsJObject["nextLink"];
                    }
                    else
                    {
                        break;
                    }
                }
                return schemaInIA;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Failed to get all schemas from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Failed to get all schemas from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }

        public static async Task<List<X12AgreementJson.Rootobject>> GetAllX12AgreementsInIA(IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            List<X12AgreementJson.Rootobject> x12AgreementsInIA = new List<X12AgreementJson.Rootobject>();
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                string responseAsString = "";
                JObject responseAsJObject = null;
                string url = UrlHelper.GeX12AgreementsUrl(iaDetails);
                while (true)
                {
                    response = sclient.GetArtifactsFromIA(url, authResult);
                    responseAsString = await response.Content.ReadAsStringAsync();
                    responseAsJObject = JsonConvert.DeserializeObject<JObject>(responseAsString);
                    if (responseAsJObject.GetValue("value") != null)
                    {
                        x12AgreementsInIA.AddRange(JsonConvert.DeserializeObject<X12AgreementJson.Rootobject[]>(responseAsJObject["value"].ToString()));
                    }
                    if (responseAsJObject["nextLink"] != null)
                    {
                        url = (string)responseAsJObject["nextLink"];
                    }
                    else
                    {
                        break;
                    }
                }
                return x12AgreementsInIA;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Failed to get all x12 agreements from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Failed to get all x12 agreements from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }

        public static async Task<List<EDIFACTAgreementJson.Rootobject>> GetAllEdifactAgreementsInIA(IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            List<EDIFACTAgreementJson.Rootobject> edifactAgreementsInIA = new List<EDIFACTAgreementJson.Rootobject>();
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                string responseAsString = "";
                JObject responseAsJObject = null;
                string url = UrlHelper.GeEdifactAgreementsUrl(iaDetails);
                while (true)
                {
                    response = sclient.GetArtifactsFromIA(url, authResult);
                    responseAsString = await response.Content.ReadAsStringAsync();
                    responseAsJObject = JsonConvert.DeserializeObject<JObject>(responseAsString);
                    if (responseAsJObject.GetValue("value") != null)
                    {
                        edifactAgreementsInIA.AddRange(JsonConvert.DeserializeObject<EDIFACTAgreementJson.Rootobject[]>(responseAsJObject["value"].ToString()));
                    }
                    if (responseAsJObject["nextLink"] != null)
                    {
                        url = (string)responseAsJObject["nextLink"];
                    }
                    else
                    {
                        break;
                    }
                }
                return edifactAgreementsInIA;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Failed to get all x12 agreements from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Failed to get all x12 agreements from IA.Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }
        public static async Task<Dictionary<KeyValuePair<string, string>, string>> GetAllPartnerIdentitiesInIA(IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            Dictionary<KeyValuePair<string, string>, string> partnerIdentities = new Dictionary<KeyValuePair<string, string>, string>();
            try
            {
                HttpResponseMessage response = new HttpResponseMessage();
                string responseAsString = "";
                JObject responseAsJObject = null;
                string url = UrlHelper.GetPartnersUrl(iaDetails);
                while (true)
                {
                    response = sclient.GetArtifactsFromIA(url, authResult);
                    responseAsString = await response.Content.ReadAsStringAsync();
                    responseAsJObject = JsonConvert.DeserializeObject<JObject>(responseAsString);
                    if (responseAsJObject.GetValue("value") != null)
                    {
                        foreach (var partner in responseAsJObject["value"])
                        {
                            if (partner["properties"] != null && partner["properties"]["content"] != null && partner["properties"]["content"]["b2b"] != null && partner["properties"]["content"]["b2b"]["businessIdentities"] != null)
                            {
                                foreach (var identity in partner["properties"]["content"]["b2b"]["businessIdentities"])
                                {
                                    if (identity["qualifier"] != null && identity["value"] != null)
                                    {
                                        KeyValuePair<string, string> kvpair = new KeyValuePair<string, string>(identity["qualifier"].ToString(), identity["value"].ToString());
                                        if (!partnerIdentities.ContainsKey(kvpair))
                                        {
                                            partnerIdentities.Add(kvpair, partner["name"].ToString());
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (responseAsJObject["nextLink"] != null)
                    {
                        url = (string)responseAsJObject["nextLink"];
                    }
                    else
                    {
                        break;
                    }
                }
                return partnerIdentities;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Failed to get all partner identities from IA. Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                throw new Exception(string.Format("Failed to get all partner identities from IA. Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }

        public string CheckIfHostPartnerIdentityExists(KeyValuePair<string, string> kvpair, string partnerName)
        {
            bool exists;
            if (partnerIdentitiesInIA != null && partnerIdentitiesInIA.Count != 0 && partnerIdentitiesInIA.ContainsKey(kvpair))
            {
                exists = true;
                return partnerIdentitiesInIA[kvpair];
            }
            else
            {
                exists = false;
                throw new Exception(string.Format("The Host Partner {0} with identity {1}:{2} doesn't exist in IA. Please migrate this partner identity/Partner before migrating the agreement", partnerName, kvpair.Key, kvpair.Value));
            }
        }

        public bool CheckIfGuestPartnerIdentityExists(KeyValuePair<string, string> kvpair, string partnerName)
        {
            List<KeyValuePair<string, string>> guestPartnerIdentities = ReadPartnerJson(FileOperations.GetFileName(partnerName));
            if (guestPartnerIdentities != null && guestPartnerIdentities.Count != 0 && guestPartnerIdentities.Contains(kvpair))
            {
                return true;
            }
            else
            {
                throw new Exception(string.Format("The Guest Partner {0} with identity {1}:{2} doesn't exist in IA. Please migrate this partner identity/Partner before migrating the agreement", partnerName, kvpair.Key, kvpair.Value));
            }
        }

        public List<KeyValuePair<string, string>> ReadPartnerJson(string partnerName)
        {
            List<KeyValuePair<string, string>> guestPartnerIdentities = new List<KeyValuePair<string, string>>();
            string content = File.ReadAllText(FileOperations.GetPartnerJsonFilePath(partnerName));
            JObject partnerJson = JsonConvert.DeserializeObject<JObject>(content);
            if (partnerJson["properties"] != null && partnerJson["properties"]["content"] != null && partnerJson["properties"]["content"]["b2b"] != null && partnerJson["properties"]["content"]["b2b"]["businessIdentities"] != null)
            {
                foreach (var identity in partnerJson["properties"]["content"]["b2b"]["businessIdentities"])
                {
                    if (identity["qualifier"] != null && identity["value"] != null)
                    {
                        guestPartnerIdentities.Add(new KeyValuePair<string, string>(identity["qualifier"].ToString(), identity["value"].ToString()));
                    }
                }
            }
            return guestPartnerIdentities;
        }
        #endregion


        public void ValidateAgreement(string agreementFilePath, AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                string agreementContent = File.ReadAllText(agreementFilePath);
                JObject agreementObject = JObject.Parse(agreementContent);
                switch (agreementObject["properties"]["agreementType"].ToString().ToLower())
                {
                    case "x12":
                        ValidateX12Agreement(agreementContent, agreementItem);
                        break;
                    case "edifact":
                        ValidateEdifactAgreement(agreementContent, agreementItem);
                        break;
                    case "as2":
                        ValidateAS2Agreement(agreementContent, agreementItem);
                        break;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        private void ValidateEdifactAgreement(string agreementContent, AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                EDIFACTAgreementJson.Rootobject agmt = JsonConvert.DeserializeObject<EDIFACTAgreementJson.Rootobject>(agreementContent);

                var hostPartnerName = CheckIfHostPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.hostIdentity.qualifier, agmt.properties.hostIdentity.value), agmt.properties.hostPartner);
                agmt.properties.hostPartner = hostPartnerName;

                bool guestExists = CheckIfGuestPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.guestIdentity.qualifier, agmt.properties.guestIdentity.value), agmt.properties.guestPartner);

                StringBuilder exception = new StringBuilder("");
                StringBuilder x = new StringBuilder("");

                x.AppendLine(string.Format("Host Partner {0} present in IA with name {1}", agmt.properties.hostPartner, hostPartnerName));
                x.AppendLine(string.Format("Guest Partner {0} will be migrated to IA with name {1}", agmt.properties.guestPartner, FileOperations.GetFileName(agmt.properties.guestPartner)));

                var schemasInAB = CheckAllEdifactSchemaReferences(agmt.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences);
                var schemasInBA = CheckAllEdifactSchemaReferences1(agmt.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences);

                if (schemasInAB["NotFound"] != "" || schemasInBA["NotFound"] != "")
                {
                    if (schemasInAB["NotFound"] != "")
                    {
                        exception.AppendLine("Receive Agreement Properties:");
                        exception.Append(schemasInAB["NotFound"]);
                    }
                    if (schemasInBA["NotFound"] != "")
                    {
                        exception.AppendLine("Send Agreement Properties:");
                        exception.Append(schemasInBA["NotFound"]);
                    }
                }

                if (schemasInAB["Found"] != "" || schemasInBA["Found"] != "")
                {
                    if (schemasInAB["Found"] != "")
                    {
                        x.AppendLine("Receive Agreement Properties:");
                        x.Append(schemasInAB["Found"]);
                    }
                    if (schemasInBA["Found"] != "")
                    {
                        x.AppendLine("Send Agreement Properties:");
                        x.Append(schemasInBA["Found"]);
                    }

                }
                if (x.ToString() != "")
                {
                    TraceProvider.WriteLine("Agreement includes following details:", true);
                    TraceProvider.WriteLine(x.ToString().TrimEnd('\n'), true);
                }

                if (exception.ToString() != "")
                {
                    throw new Exception(exception.ToString().TrimEnd('\n'));
                }

                Func<string, string> GeSchemaNameByTargetNameSpace = delegate (string TargetNamespace)
                {
                    if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsValue(TargetNamespace))
                    {
                        return schemasInIA.Where(item => item.Value == TargetNamespace).First().Key;
                    }
                    else if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsKey(TargetNamespace))
                    {
                        return TargetNamespace;
                    }
                    else
                    {
                        throw new Exception(string.Format("Error: Schema Reference with Target Namespace {0} in agreement doesn't exist in IA. Please migrate the schema before migrating the agreement.", TargetNamespace));

                    }
                };
                foreach (var schema in agmt.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences)
                {
                    schema.schemaName = GeSchemaNameByTargetNameSpace(schema.schemaName);
                }
                foreach (var schema in agmt.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences)
                {
                    schema.schemaName = GeSchemaNameByTargetNameSpace(schema.schemaName);
                }

                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agreementItem.Name), ".json");
                string agmtJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(agmt);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, agmtJsonFileContent);


            }
            catch (Exception ex)
            {
                agreementItem.ExportStatus = MigrationStatus.Failed;
                agreementItem.ExportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        private void ValidateX12Agreement(string agreementContent, AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                X12AgreementJson.Rootobject agmt = JsonConvert.DeserializeObject<X12AgreementJson.Rootobject>(agreementContent);

                var hostPartnerName = CheckIfHostPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.hostIdentity.qualifier, agmt.properties.hostIdentity.value), agmt.properties.hostPartner);
                agmt.properties.hostPartner = hostPartnerName;

                bool guestExists = CheckIfGuestPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.guestIdentity.qualifier, agmt.properties.guestIdentity.value), agmt.properties.guestPartner);

                StringBuilder exception = new StringBuilder("");
                StringBuilder x = new StringBuilder("");

                x.AppendLine(string.Format("Host Partner {0} present in IA with name {1}", agmt.properties.hostPartner, hostPartnerName));
                x.AppendLine(string.Format("Guest Partner {0} will be migrated to IA with name {1}", agmt.properties.guestPartner, FileOperations.GetFileName(agmt.properties.guestPartner)));

                var schemasInAB = CheckAllX12SchemaReferences(agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences);
                var schemasInBA = CheckAllX12SchemaReferences(agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences);

                if (schemasInAB["NotFound"] != "" || schemasInBA["NotFound"] != "")
                {
                    if (schemasInAB["NotFound"] != "")
                    {
                        exception.AppendLine("Receive Agreement Properties:");
                        exception.Append(schemasInAB["NotFound"]);
                    }
                    if (schemasInBA["NotFound"] != "")
                    {
                        exception.AppendLine("Send Agreement Properties:");
                        exception.Append(schemasInBA["NotFound"]);
                    }
                }

                if (schemasInAB["Found"] != "" || schemasInBA["Found"] != "")
                {
                    if (schemasInAB["Found"] != "")
                    {
                        x.AppendLine("Receive Agreement Properties:");
                        x.Append(schemasInAB["Found"]);
                    }
                    if (schemasInBA["Found"] != "")
                    {
                        x.AppendLine("Send Agreement Properties:");
                        x.Append(schemasInBA["Found"]);
                    }

                }


                if (x.ToString() != "")
                {
                    TraceProvider.WriteLine("Agreement includes following details:", true);
                    TraceProvider.WriteLine(x.ToString().TrimEnd('\n'), true);
                }

                if (exception.ToString() != "")
                {
                    throw new Exception(exception.ToString().TrimEnd('\n'));
                }

                var schemaNamespaceVersionMappingDict = thisApplicationContext.GetProperty(AppConstants.SchemaNamespaceVersionList) as Dictionary<string, string>;
                Func<string, string> GeSchemaNameByTargetNameSpace = delegate (string TargetNamespace)
                {
                    if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsValue(TargetNamespace))
                    {
                        return schemasInIA.Where(item => item.Value == TargetNamespace).First().Key;
                    }
                    else if (schemasInIA != null && schemasInIA.Count != 0 && schemasInIA.ContainsKey(TargetNamespace))
                    {
                        return TargetNamespace;
                    }
                    else
                    {
                        throw new Exception(string.Format("Error: Schema Reference with Target Namespace {0} in agreement doesn't exist in IA. Please migrate the schema before migrating the agreement.", TargetNamespace));

                    }
                };
                Func<string, string, int, string> GetVersionFromSchemaName = delegate (string sourceString, string removeString, int index)
                {
                    string version = "";
                    version = (index < 0) ? sourceString : sourceString.Remove(index, removeString.Length);
                    return version;
                };
                Func<string, string, string> GetVersionFromSchema = delegate (string schemaNamespace, string id)
                {
                    string version = "";
                    if (schemaNamespaceVersionMappingDict != null && schemaNamespaceVersionMappingDict.Count != 0 && schemaNamespaceVersionMappingDict.ContainsKey(schemaNamespace))
                    {
                        version = schemaNamespaceVersionMappingDict[schemaNamespace];
                    }
                    else
                    {
                        string name = GeSchemaNameByTargetNameSpace(schemaNamespace);
                        string rootnode = name.Split('.')[name.Split('.').Length - 1];
                        string nameWithoutX12 = GetVersionFromSchemaName(rootnode, "X12", rootnode.IndexOf("X12"));
                        string nameWithoutX12AndId = GetVersionFromSchemaName(nameWithoutX12, id, nameWithoutX12.IndexOf(id));
                        version = nameWithoutX12AndId.Substring(0, 5);
                    }
                    return version;
                };

                Dictionary<string, string> schemaNamespaceVersionDict = this.thisApplicationContext.GetProperty(AppConstants.SchemaNamespaceVersionList) as Dictionary<string, string>;
                foreach (var schema in agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences)
                {
                    if (schema.schemaVersion == schema.schemaName)
                    {
                        schema.schemaVersion = GetVersionFromSchema(schema.schemaName, schema.messageId);
                    }
                    schema.schemaName = GeSchemaNameByTargetNameSpace(schema.schemaName);

                }
                foreach (var schema in agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences)
                {
                    if (schema.schemaVersion == schema.schemaName)
                    {
                        schema.schemaVersion = GetVersionFromSchema(schema.schemaName, schema.messageId);
                    }
                    schema.schemaName = GeSchemaNameByTargetNameSpace(schema.schemaName);

                }
                if (agmt.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides != null)
                {
                    foreach (var delimiterOverride in agmt.properties.content.x12.receiveAgreement.protocolSettings.x12DelimiterOverrides)
                    {
                        if (agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Count(y => y.messageId == delimiterOverride.messageId) != 0)
                        {
                            delimiterOverride.protocolVersion = agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Where(y => y.messageId == delimiterOverride.messageId).First().schemaVersion;

                        }
                        else if (delimiterOverride.protocolVersion == delimiterOverride.targetNamespace)
                        {
                            delimiterOverride.protocolVersion = GetVersionFromSchema(delimiterOverride.targetNamespace, delimiterOverride.messageId);
                        }
                    }
                }
                if (agmt.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides != null)
                {
                    foreach (var delimiterOverride in agmt.properties.content.x12.sendAgreement.protocolSettings.x12DelimiterOverrides)
                    {
                        if (agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Count(y => y.messageId == delimiterOverride.messageId) != 0)
                        {
                            delimiterOverride.protocolVersion = agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Where(y => y.messageId == delimiterOverride.messageId).First().schemaVersion;

                        }
                        else if (delimiterOverride.protocolVersion == delimiterOverride.targetNamespace)
                        {
                            delimiterOverride.protocolVersion = GetVersionFromSchema(delimiterOverride.targetNamespace, delimiterOverride.messageId);
                        }
                    }
                }
                if (agmt.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides != null)
                {

                    foreach (var envelopeOverride in agmt.properties.content.x12.receiveAgreement.protocolSettings.envelopeOverrides)
                    {
                        if (agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Count(y => y.messageId == envelopeOverride.MessageId) != 0)
                        {
                            envelopeOverride.ProtocolVersion = agmt.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.Where(y => y.messageId == envelopeOverride.MessageId).First().schemaVersion;

                        }
                        else if (envelopeOverride.ProtocolVersion == envelopeOverride.TargetNamespace)
                        {
                            envelopeOverride.ProtocolVersion = GetVersionFromSchema(envelopeOverride.TargetNamespace, envelopeOverride.MessageId);
                        }
                    }
                }
                if (agmt.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides != null)
                {
                    foreach (var envelopeOverride in agmt.properties.content.x12.sendAgreement.protocolSettings.envelopeOverrides)
                    {

                        if (agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Count(y => y.messageId == envelopeOverride.MessageId) != 0)
                        {
                            envelopeOverride.ProtocolVersion = agmt.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.Where(y => y.messageId == envelopeOverride.MessageId).First().schemaVersion;

                        }
                        else if (envelopeOverride.ProtocolVersion == envelopeOverride.TargetNamespace)
                        {
                            envelopeOverride.ProtocolVersion = GetVersionFromSchema(envelopeOverride.TargetNamespace, envelopeOverride.MessageId);
                        }
                    }
                }
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agreementItem.Name), ".json");
                string agmtJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(agmt);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, agmtJsonFileContent);
            }
            catch (Exception ex)
            {
                agreementItem.ExportStatus = MigrationStatus.Failed;
                agreementItem.ExportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public void ValidateAS2Agreement(string agreementContent, AgreementMigrationItemViewModel agreementItem)
        {
            try
            {
                JsonAs2Agreement.Rootobject agmt = JsonConvert.DeserializeObject<JsonAs2Agreement.Rootobject>(agreementContent);

                var hostPartnerName = CheckIfHostPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.hostIdentity.qualifier, agmt.properties.hostIdentity.value), agmt.properties.hostPartner);
                agmt.properties.hostPartner = hostPartnerName;

                bool guestExists = CheckIfGuestPartnerIdentityExists(new KeyValuePair<string, string>(agmt.properties.guestIdentity.qualifier, agmt.properties.guestIdentity.value), agmt.properties.guestPartner);

                string guestSignCertName = agmt.properties.content.aS2.receiveAgreement.protocolSettings.securitySettings.signingCertificateName;
                string hostEncryptCertName = agmt.properties.content.aS2.receiveAgreement.protocolSettings.securitySettings.encryptionCertificateName;
                string hostSignCertName = agmt.properties.content.aS2.sendAgreement.protocolSettings.securitySettings.signingCertificateName;
                string guestEncryptCertName = agmt.properties.content.aS2.sendAgreement.protocolSettings.securitySettings.encryptionCertificateName;

                StringBuilder exception = new StringBuilder("");
                StringBuilder x = new StringBuilder("");

                x.AppendLine(string.Format("Host Partner {0} present in IA with name {1}", agmt.properties.hostPartner, hostPartnerName));
                x.AppendLine(string.Format("Guest Partner {0} will be migrated to IA with name {1}", agmt.properties.guestPartner, FileOperations.GetFileName(agmt.properties.guestPartner)));

                if (guestSignCertName == "" && agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.signMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Signing is enabled at Receive side but the Guest Certificate Name could not be known for the Guest {0}. Please ensure the Certificate exists on IA and include the mapping in the Mapping file before migrating the agreement.", agmt.properties.guestPartner));
                }
                if (hostEncryptCertName == "" && agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.encryptMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Encryption is enabled at Receive side but the Host Certificate Name could not be known for the Host {0}. Please ensure the Certificate exists on IA and include the mapping in the Mapping file before migrating the agreement.", agmt.properties.hostPartner));
                }
                if (hostSignCertName == "" && agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.signMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Signing is enabled at Send side but the Host Certificate Name could not be known for the Host {0}. Please ensure the Certificate exists on IA and include the mapping in the Mapping file before migrating the agreement.", agmt.properties.hostPartner));
                }
                if (guestEncryptCertName == "" && agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.encryptMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Encryption is enabled at Send side but the Guest Certificate Name could not be known for the Guest {0}. Please ensure the Certificate exists on IA and include the mapping in the Mapping file before migrating the agreement.", agmt.properties.guestPartner));
                }

                AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                bool hostCertExists = CheckIfArtifactExists(hostSignCertName, "Certificate", this.iaDetails, authresult).Result;
                bool guestCertExists = CheckIfArtifactExists(guestEncryptCertName, "Certificate", this.iaDetails, authresult).Result;

                if (!guestCertExists && agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.signMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Signing is enabled at Receive side but the Guest Certificate {0} could not be found for the Guest {1} in IA. Please migrate the Certificate to IA before migrating the agreement.", guestSignCertName, agmt.properties.guestPartner));
                }
                if (!hostCertExists && agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.encryptMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Encryption is enabled at Receive side but the Host Certificate {0} could not be found for the Host {1} in IA. Please migrate the Certificate to IA before migrating the agreement.", hostEncryptCertName, agmt.properties.hostPartner));
                }
                if (!hostCertExists && agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.signMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Signing is enabled at Send side but the Host Certificate {0} could not be found for the Host {1} in IA. Please migrate the Certificate to IA before migrating the agreement.", hostSignCertName, agmt.properties.hostPartner));
                }
                if (!guestCertExists && agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.encryptMessage == true)
                {
                    exception.AppendLine(string.Format("Error: Encryption is enabled at Send side but the Guest Certificate {0} could not be found for the Guest {1} in IA. Please migrate the Certificate to IA before migrating the agreement.", guestEncryptCertName, agmt.properties.guestPartner));
                }


                if (agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.signMessage == false)
                {
                    guestSignCertName = "";
                    x.AppendLine(string.Format("Receive Side: Signing disabled for Guest partner {0}", agmt.properties.guestPartner));
                }
                else
                {
                    x.AppendLine(string.Format("Receive Side: Signing Certificate used for Guest partner {0} is {1}", agmt.properties.guestPartner, guestSignCertName));
                }

                if (agmt.properties.content.aS2.receiveAgreement.protocolSettings.validationSettings.encryptMessage == false)
                {
                    hostEncryptCertName = "";
                    x.AppendLine(string.Format("Receive Side: Encrypion disabled for Host partner {0}", agmt.properties.hostPartner));
                }
                else
                {
                    x.AppendLine(string.Format("Receive Side: Encryption Certificate used for Host partner {0} is {1}", agmt.properties.hostPartner, hostEncryptCertName));
                }

                if (agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.signMessage == false)
                {
                    hostSignCertName = "";
                    x.AppendLine(string.Format("Send Side: Signing disabled for Host partner {0}", agmt.properties.hostPartner));
                }
                else
                {
                    x.AppendLine(string.Format("Send Side: Signing Certificate used for Host partner {0} is {1}", agmt.properties.hostPartner, hostSignCertName));
                }

                if (agmt.properties.content.aS2.sendAgreement.protocolSettings.validationSettings.encryptMessage == false)
                {
                    guestEncryptCertName = "";
                    x.AppendLine(string.Format("Send Side: Encryption disabled for Guest partner {0}", agmt.properties.guestPartner));
                }
                else
                {
                    x.AppendLine(string.Format("Send Side: Encryption Certificate used for Guest partner {0} is {1}", agmt.properties.guestPartner, guestEncryptCertName));
                }

                if (x.ToString() != "")
                {
                    TraceProvider.WriteLine("Agreement includes following details:", true);
                    TraceProvider.WriteLine(x.ToString().TrimEnd('\n'), true);
                }
                if (exception.ToString() != "")
                {
                    throw new Exception(exception.ToString().TrimEnd('\n'));
                }
                agmt.properties.content.aS2.receiveAgreement.protocolSettings.securitySettings.signingCertificateName = guestSignCertName;
                agmt.properties.content.aS2.receiveAgreement.protocolSettings.securitySettings.encryptionCertificateName = hostEncryptCertName;
                agmt.properties.content.aS2.sendAgreement.protocolSettings.securitySettings.signingCertificateName = hostSignCertName;
                agmt.properties.content.aS2.sendAgreement.protocolSettings.securitySettings.encryptionCertificateName = guestEncryptCertName;
                string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(agmt.name), ".json");
                string agmtJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(agmt);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, agmtJsonFileContent);


            }
            catch (Exception ex)
            {
                agreementItem.ExportStatus = MigrationStatus.Failed;
                agreementItem.ExportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                TraceProvider.WriteLine("Agreement Export to Json Failed. Reason:");
                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex), true);
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public List<string> GetX12SchemasList(X12AgreementJson.Rootobject obj)
        {
            List<string> transactionsInAgreement = new List<string>();
            obj.properties.content.x12.receiveAgreement.protocolSettings.schemaReferences.ToList().ForEach(x => transactionsInAgreement.Add(x.messageId));
            obj.properties.content.x12.sendAgreement.protocolSettings.schemaReferences.ToList().ForEach(x => transactionsInAgreement.Add(x.messageId));
            return transactionsInAgreement;
        }

        public List<string> GetEdifactSchemasList(EDIFACTAgreementJson.Rootobject obj)
        {
            List<string> transactionsInAgreement = new List<string>();
            obj.properties.content.edifact.receiveAgreement.protocolSettings.schemaReferences.ToList().ForEach(x => transactionsInAgreement.Add(x.messageId));
            obj.properties.content.edifact.sendAgreement.protocolSettings.schemaReferences.ToList().ForEach(x => transactionsInAgreement.Add(x.messageId));
            return transactionsInAgreement;
        }


        public void ConsolidateAgreements()
        {

        }
        public void CheckIfAgreementHasToBeConsolidated(AgreementMigrationItemViewModel serverAgreementItem)
        {
            try
            {
                List<X12AgreementJson.Rootobject> x12Agreements = new List<X12AgreementJson.Rootobject>();
                List<EDIFACTAgreementJson.Rootobject> edifactAgreements = new List<EDIFACTAgreementJson.Rootobject>();
                x12Agreements = thisApplicationContext.GetProperty("X12AgreementsInIntegrationAccount") as List<X12AgreementJson.Rootobject>;
                edifactAgreements = thisApplicationContext.GetProperty("EdifactAgreementsInIntegrationAccount") as List<EDIFACTAgreementJson.Rootobject>;

                var idMappings = thisApplicationContext.GetProperty("OriginalAndTpmIdMappings") as Dictionary<string, string>;
                var tpmIds = new List<string>();
                tpmIds = idMappings.Where(x => x.Value == serverAgreementItem.GuestPartnerId).ToDictionary(x => x.Key, y => y.Value).Keys.ToList();
                if (serverAgreementItem.Protocol == "x12")
                {
                    var agreementsInIA = x12Agreements.Where(x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                                                                && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                                                                && x.properties.guestIdentity.qualifier == serverAgreementItem.GuestPartnerQualifer
                                                                && x.properties.guestIdentity.value == serverAgreementItem.GuestPartnerId);

                    if (agreementsInIA != null && agreementsInIA.Count() != 0)
                    {
                        var agreementInLocal = GetX12AgreementJson(serverAgreementItem.Name);
                        if (serverAgreementItem.Transactions == null || serverAgreementItem.Transactions.Count == 0)
                        {
                            serverAgreementItem.Transactions.AddRange(GetX12SchemasList(agreementInLocal));
                        }

                        X12AgreementJson.Rootobject baseAgreementInIA = agreementsInIA.First();
                        int countTransactionsInAgreementInIA = 0;
                        List<string> transactionsInBaseAgreementInIA = new List<string>();
                        foreach (var agreementInIA in agreementsInIA)
                        {
                            List<string> transactionsInAgreementInIA = new List<string>();
                            transactionsInAgreementInIA.AddRange(GetX12SchemasList(agreementInIA));
                            if (countTransactionsInAgreementInIA < transactionsInAgreementInIA.Count)
                            {
                                baseAgreementInIA = agreementInIA;
                                countTransactionsInAgreementInIA = transactionsInAgreementInIA.Count;
                                transactionsInBaseAgreementInIA = transactionsInAgreementInIA;
                            }
                            else
                            {
                                TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with same business Identifiers. Skipping the agreement..", baseAgreementInIA.name));
                            }

                        }
                        if (transactionsInBaseAgreementInIA.Count != 0
                             && serverAgreementItem.Transactions.Count(x => transactionsInBaseAgreementInIA.Contains(x)) != serverAgreementItem.Transactions.Count)
                        {
                            TraceProvider.WriteLine(string.Format("Information: Agreement {0} in Integration Account will be consolidated with {1}", baseAgreementInIA.name, serverAgreementItem.Name));
                            agreementInLocal = ConsolidateTwoX12Agreements(baseAgreementInIA, agreementInLocal);
                            serverAgreementItem.Transactions = GetX12SchemasList(agreementInLocal);
                            serverAgreementItem.IsConsolidated = true;
                            serverAgreementItem.BaseAgreementInIA = baseAgreementInIA.name;
                            serverAgreementItem.MigrationEntity.Name = serverAgreementItem.Name;
                            serverAgreementItem.Name = serverAgreementItem.HostedPartnerName + "_" + serverAgreementItem.GuestPartnerName + "_" + string.Join("_", serverAgreementItem.Transactions);
                            agreementInLocal.name = serverAgreementItem.Name;
                            string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                            string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(serverAgreementItem.Name), ".json");
                            string agmtJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(agreementInLocal);
                            FileOperations.CreateFolder(fileName);
                            System.IO.File.WriteAllText(fileName, agmtJsonFileContent);
                            TraceProvider.WriteLine(string.Format("Information: Agreement {0} in Integration Account consolidated with agreement. The new name of agreement in IA is {1}", baseAgreementInIA.name, serverAgreementItem.Name));
                        }
                        else
                        {
                            TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with same business Identifiers and supported transactions. Skipping the agreement..", baseAgreementInIA.name));
                        }
                    }
                    if (x12Agreements.Count(x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                      && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                      && tpmIds.Contains(x.properties.guestIdentity.value)) != 0)
                    {
                        var otherMatchingAgreementsInIA = x12Agreements.Where((x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                                                                                 && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                                                                                 && tpmIds.Contains(x.properties.guestIdentity.value)));
                        foreach (var matchingAgreement in otherMatchingAgreementsInIA)
                        {
                            TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with dummy business Identifiers matched with the agreement guest identifier.", matchingAgreement.name));
                        }
                    }
                    TraceProvider.WriteLine();
                }

                else if (serverAgreementItem.Protocol == "edifact")
                {
                    var agreementsInIA = edifactAgreements.Where(x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                                                                && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                                                                && x.properties.guestIdentity.qualifier == serverAgreementItem.GuestPartnerQualifer
                                                                && x.properties.guestIdentity.value == serverAgreementItem.GuestPartnerId);

                    if (agreementsInIA != null && agreementsInIA.Count() != 0)
                    {
                        var agreementInLocal = GetEdifactAgreementJson(serverAgreementItem.Name);
                        if (serverAgreementItem.Transactions == null || serverAgreementItem.Transactions.Count == 0)
                        {
                            serverAgreementItem.Transactions.AddRange(GetEdifactSchemasList(agreementInLocal));
                        }

                        EDIFACTAgreementJson.Rootobject baseAgreementInIA = agreementsInIA.First();
                        int countTransactionsInAgreementInIA = 0;
                        List<string> transactionsInBaseAgreementInIA = new List<string>();
                        foreach (var agreementInIA in agreementsInIA)
                        {
                            List<string> transactionsInAgreementInIA = new List<string>();
                            transactionsInAgreementInIA.AddRange(GetEdifactSchemasList(agreementInIA));
                            if (countTransactionsInAgreementInIA < transactionsInAgreementInIA.Count)
                            {
                                baseAgreementInIA = agreementInIA;
                                countTransactionsInAgreementInIA = transactionsInAgreementInIA.Count;
                                transactionsInBaseAgreementInIA = transactionsInAgreementInIA;
                            }
                            else
                            {
                                TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with same business Identifiers. Skipping the agreement..", baseAgreementInIA.name));
                            }

                        }
                        if (transactionsInBaseAgreementInIA.Count != 0
                             && serverAgreementItem.Transactions.Count(x => transactionsInBaseAgreementInIA.Contains(x)) != serverAgreementItem.Transactions.Count)
                        {
                            TraceProvider.WriteLine(string.Format("Information: Agreement {0} in Integration Account will be consolidated with {1}", baseAgreementInIA.name, serverAgreementItem.Name));
                            agreementInLocal = ConsolidateTwoEdifactAgreements(baseAgreementInIA, agreementInLocal);
                            serverAgreementItem.Transactions = GetEdifactSchemasList(agreementInLocal);
                            serverAgreementItem.IsConsolidated = true;
                            serverAgreementItem.BaseAgreementInIA = baseAgreementInIA.name;
                            serverAgreementItem.MigrationEntity.Name = serverAgreementItem.Name;
                            serverAgreementItem.Name = serverAgreementItem.HostedPartnerName + "_" + serverAgreementItem.GuestPartnerName + "_" + string.Join("_", serverAgreementItem.Transactions);
                            agreementInLocal.name = serverAgreementItem.Name;
                            string directroyPathForJsonFiles = Resources.JsonAgreementFilesLocalPath;
                            string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(serverAgreementItem.Name), ".json");
                            string agmtJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(agreementInLocal);
                            FileOperations.CreateFolder(fileName);
                            System.IO.File.WriteAllText(fileName, agmtJsonFileContent);
                            TraceProvider.WriteLine(string.Format("Information: Agreement {0} in Integration Account consolidated with agreement. The new name of agreement in IA is {1}", baseAgreementInIA.name, serverAgreementItem.Name));

                        }
                        else
                        {
                            TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with same business Identifiers and supported transactions. Skipping the agreement..", baseAgreementInIA.name));
                        }
                    }
                    if (edifactAgreements.Count(x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                      && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                      && tpmIds.Contains(x.properties.guestIdentity.value)) != 0)
                    {
                        var otherMatchingAgreementsInIA = edifactAgreements.Where((x => x.properties.hostIdentity.qualifier == serverAgreementItem.HostPartnerQualifier
                                                                                 && x.properties.hostIdentity.value == serverAgreementItem.HostPartnerId
                                                                                 && tpmIds.Contains(x.properties.guestIdentity.value)));
                        foreach (var matchingAgreement in otherMatchingAgreementsInIA)
                        {
                            TraceProvider.WriteLine(string.Format("Warning: Agreement {0} already exists in Integration Account with dummy business Identifiers matched with the agreement guest identifier.", matchingAgreement.name));
                        }
                    }
                    TraceProvider.WriteLine();
                }
                else
                {
                    TraceProvider.WriteLine(string.Format("Information: Agreement {0} is AS2 agreement. No need for consolidation.", serverAgreementItem.Name));
                    TraceProvider.WriteLine();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #region Export Agreement
        public override async Task ExportToIA(AgreementMigrationItemViewModel serverAgreementItem, IntegrationAccountDetails iaDetails)
        {
            try
            {
                this.iaDetails = iaDetails;
                this.schemasInIA = this.thisApplicationContext.GetProperty("SchemasInIntegrationAccount") as Dictionary<string, string>;
                this.partnerIdentitiesInIA = this.thisApplicationContext.GetProperty("PartnerIdentitiesInIntegrationAccount") as Dictionary<KeyValuePair<string, string>, string>;

                TraceProvider.WriteLine();
                TraceProvider.WriteLine("Migrating agreement: {0}", serverAgreementItem.Name);
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        bool overwrite = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
                        if (!overwrite)
                        {
                            bool exists = CheckIfArtifactExists(serverAgreementItem.Name, "Agreement", this.iaDetails, authresult).Result;
                            if (exists)
                            {
                                serverAgreementItem.ExportStatus = MigrationStatus.Partial;
                                serverAgreementItem.ExportStatusText = string.Format("The Agreement {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the agreement was not overwritten.", serverAgreementItem.MigrationEntity.Name, FileOperations.GetFileName(serverAgreementItem.MigrationEntity.Name));
                                TraceProvider.WriteLine(serverAgreementItem.ExportStatusText);
                                TraceProvider.WriteLine();
                            }
                            else
                            {
                                ValidateAgreement(FileOperations.GetAgreementJsonFilePath(serverAgreementItem.Name), serverAgreementItem);
                                MigrateToCloudIAAgreement(FileOperations.GetAgreementJsonFilePath(serverAgreementItem.Name), FileOperations.GetFileName(serverAgreementItem.Name), serverAgreementItem, iaDetails, authresult).Wait();
                                serverAgreementItem.ExportStatus = MigrationStatus.Succeeded;
                                StringBuilder successMessageText = new StringBuilder();
                                successMessageText.Append(string.Format(Resources.ExportSuccessMessageText, serverAgreementItem.Name));
                                serverAgreementItem.ExportStatusText = successMessageText.ToString();
                                try
                                {
                                    if (!string.IsNullOrEmpty(serverAgreementItem.BaseAgreementInIA))
                                    {
                                        TraceProvider.WriteLine(string.Format("Deleting original agreement {0} from IA before adding consolidated agreement {1}", serverAgreementItem.BaseAgreementInIA, serverAgreementItem.Name));
                                        DeleteFromIntegrationAccount(serverAgreementItem, iaDetails, authresult).Wait();
                                        TraceProvider.WriteLine("Succesfully deleted original agreement from IA");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex));
                                }
                                TraceProvider.WriteLine("Agreement Migration Successfull: {0}", serverAgreementItem.Name);
                                TraceProvider.WriteLine();
                            }
                        }
                        else
                        {
                            ValidateAgreement(FileOperations.GetAgreementJsonFilePath(serverAgreementItem.Name), serverAgreementItem);
                            MigrateToCloudIAAgreement(FileOperations.GetAgreementJsonFilePath(serverAgreementItem.Name), FileOperations.GetFileName(serverAgreementItem.Name), serverAgreementItem, iaDetails, authresult).Wait();
                            serverAgreementItem.ExportStatus = MigrationStatus.Succeeded;
                            StringBuilder successMessageText = new StringBuilder();
                            successMessageText.Append(string.Format(Resources.ExportSuccessMessageText, serverAgreementItem.Name));
                            serverAgreementItem.ExportStatusText = successMessageText.ToString();
                            try
                            {
                                if (!string.IsNullOrEmpty(serverAgreementItem.BaseAgreementInIA))
                                {
                                    TraceProvider.WriteLine(string.Format("Deleting original agreement {0} from IA before adding consolidated agreement {1}", serverAgreementItem.BaseAgreementInIA, serverAgreementItem.Name));
                                    DeleteFromIntegrationAccount(serverAgreementItem, iaDetails, authresult).Wait();
                                    TraceProvider.WriteLine("Succesfully deleted original agreement from IA");
                                }
                            }
                            catch (Exception ex)
                            {
                                TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex));
                            }
                            TraceProvider.WriteLine("Agreement Migration Successfull: {0}", serverAgreementItem.Name);
                            TraceProvider.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {

                        //do  nothing
                    }
                });
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        #endregion


        #region Migrate To Cloud IA

        public async Task<HttpResponseMessage> MigrateToCloudIAAgreement(string filePath, string name, AgreementMigrationItemViewModel serverAgreementItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                var x = await sclient.LAIntegrationFromFile(UrlHelper.GetAgreementUrl(name, iaDetails), filePath, authResult);
                return x;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Agreement Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                serverAgreementItem.ExportStatus = MigrationStatus.Failed;
                serverAgreementItem.ExportStatusText = ex.Message;
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> DeleteFromIntegrationAccount(AgreementMigrationItemViewModel serverAgreementItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                var x = await sclient.DeleteFromIA(UrlHelper.GetAgreementUrl(serverAgreementItem.BaseAgreementInIA, iaDetails), authResult);
                return x;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Agreement Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                serverAgreementItem.ExportStatus = MigrationStatus.Failed;
                serverAgreementItem.ExportStatusText = ex.Message;
                throw ex;
            }
        }
        #endregion


        public void GetX12AgreementDetails(AgreementMigrationItemViewModel agreement)
        {
            string senderGuestDetailsPartner = (agreement.MigrationEntity.SenderDetails.Partner);
            string receiverHostDetailsParnter = (agreement.MigrationEntity.ReceiverDetails.Partner);

            Server.OnewayAgreement onewayAgreementAB = agreement.MigrationEntity.GetOnewayAgreement(senderGuestDetailsPartner, receiverHostDetailsParnter);//receive agreement
            Server.X12ProtocolSettings protocolSettingsAB = onewayAgreementAB.GetProtocolSettings<Server.X12ProtocolSettings>();

            Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABreceiverHostQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.ReceiverIdentity;
            Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABsenderGuestQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.SenderIdentity;

            Server.OnewayAgreement onewayAgreementBA = agreement.MigrationEntity.GetOnewayAgreement(receiverHostDetailsParnter, senderGuestDetailsPartner);//sender agreement
            Server.X12ProtocolSettings protocolSettingsBA = onewayAgreementBA.GetProtocolSettings<Server.X12ProtocolSettings>();//sender agreement

            agreement.GuestPartnerTpmId = onewayAgreementABsenderGuestQualifierIdentity.Value;
            agreement.GuestPartnerQualifer = onewayAgreementABsenderGuestQualifierIdentity.Qualifier;
            agreement.HostPartnerId = onewayAgreementABreceiverHostQualifierIdentity.Value;
            agreement.HostPartnerQualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier;
            agreement.Protocol = agreement.MigrationEntity.Protocol;
            var receiveSideSchemaReferences = X12GetSchemaReference(protocolSettingsAB.SchemaSettings);
            var sendSideSchemaReferences = X12GetSchemaReference(protocolSettingsBA.SchemaSettings);
            agreement.Transactions = new List<string>();
            receiveSideSchemaReferences.ToList().ForEach(x => agreement.Transactions.Add(x.messageId));
            sendSideSchemaReferences.ToList().ForEach(x => agreement.Transactions.Add(x.messageId));
        }

        public void GetEdifactAgreementDetails(AgreementMigrationItemViewModel agreement)
        {
            string senderGuestDetailsPartner = (agreement.MigrationEntity.SenderDetails.Partner);
            string receiverHostDetailsParnter = (agreement.MigrationEntity.ReceiverDetails.Partner);

            Server.OnewayAgreement onewayAgreementAB = agreement.MigrationEntity.GetOnewayAgreement(senderGuestDetailsPartner, receiverHostDetailsParnter);//receive agreement
            Server.EDIFACTProtocolSettings protocolSettingsAB = onewayAgreementAB.GetProtocolSettings<Server.EDIFACTProtocolSettings>();

            Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABreceiverHostQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.ReceiverIdentity;
            Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity onewayAgreementABsenderGuestQualifierIdentity = (Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)onewayAgreementAB.SenderIdentity;

            Server.OnewayAgreement onewayAgreementBA = agreement.MigrationEntity.GetOnewayAgreement(receiverHostDetailsParnter, senderGuestDetailsPartner);//sender agreement
            Server.EDIFACTProtocolSettings protocolSettingsBA = onewayAgreementBA.GetProtocolSettings<Server.EDIFACTProtocolSettings>();//sender agreement

            agreement.GuestPartnerTpmId = onewayAgreementABsenderGuestQualifierIdentity.Value;
            agreement.GuestPartnerQualifer = onewayAgreementABsenderGuestQualifierIdentity.Qualifier;
            agreement.HostPartnerId = onewayAgreementABreceiverHostQualifierIdentity.Value;
            agreement.HostPartnerQualifier = onewayAgreementABreceiverHostQualifierIdentity.Qualifier;
            agreement.Protocol = agreement.MigrationEntity.Protocol;
            var receiveSideSchemaReferences = GetSchemaReference(protocolSettingsAB.SchemaSettings);
            var sendSideSchemaReferences = GetSchemaReference(protocolSettingsBA.SchemaSettings);
            agreement.Transactions = new List<string>();
            receiveSideSchemaReferences.ToList().ForEach(x => agreement.Transactions.Add(x.messageId));
            sendSideSchemaReferences.ToList().ForEach(x => agreement.Transactions.Add(x.messageId));

        }

        public string GetEbisDbConnectionString()
        {
            try
            {
                string connectionString = thisApplicationContext.GetProperty("DatabaseConnectionString") as string;
                string ebisDbName = ConfigurationManager.AppSettings["EbisDbName"];
                Match match = Regex.Match(connectionString, "Initial Catalog=(.*?);");
                if (match.Success)
                {
                    connectionString = connectionString.Replace(match.Groups[1].Value, ebisDbName);
                }
                return connectionString;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public List<AgreementMigrationItemViewModel> CheckAgreementsToBeConsolidated(List<AgreementMigrationItemViewModel> selectedAgreements)
        {
            Dictionary<string, string> tpmPartnerIdsInSelectedAgreements = new Dictionary<string, string>();
            Dictionary<string, string> hostPartnerIdsInSelectedAgreements = new Dictionary<string, string>();
            Dictionary<string, List<AgreementMigrationItemViewModel>> agreementsToBeConsolidated = new Dictionary<string, List<AgreementMigrationItemViewModel>>();
            Dictionary<string, List<AgreementMigrationItemViewModel>> finalAgreementsToBeConsolidated = new Dictionary<string, List<AgreementMigrationItemViewModel>>();
            List<AgreementMigrationItemViewModel> listAgreementsToConsolidate = new List<AgreementMigrationItemViewModel>();
            try
            {
                if (selectedAgreements != null && selectedAgreements.Count != 0)
                {
                    foreach (var agreement in selectedAgreements)
                    {
                        if (agreement.MigrationEntity.Protocol == AppConstants.X12ProtocolName)
                        {
                            GetX12AgreementDetails(agreement);
                            tpmPartnerIdsInSelectedAgreements.Add(agreement.Name, "'" + agreement.GuestPartnerTpmId + "'");
                        }
                        if (agreement.MigrationEntity.Protocol == AppConstants.EdifactProtocolName)
                        {
                            GetEdifactAgreementDetails(agreement);
                            tpmPartnerIdsInSelectedAgreements.Add(agreement.Name, "'" + agreement.GuestPartnerTpmId + "'");
                        }
                    }
                }

                Dictionary<string, string> originalAndTpmIdMapping = DatabaseHelper.FindAgreementsToBeConsolidated(GetEbisDbConnectionString(), tpmPartnerIdsInSelectedAgreements.Values.ToList());
                thisApplicationContext.SetProperty("OriginalAndTpmIdMappings", originalAndTpmIdMapping);
                foreach (var partnerId in originalAndTpmIdMapping.Values.ToList().Distinct())
                {
                    var tpmIds = originalAndTpmIdMapping.Where(x => x.Value == partnerId).ToDictionary(x => x.Key, y => y.Value).Keys.ToList();
                    if (!tpmIds.Contains(partnerId))
                    {
                        tpmIds.Add(partnerId);
                    }
                    selectedAgreements.Where(x => tpmIds.Contains(x.GuestPartnerTpmId)).ToList().ForEach(x => x.GuestPartnerId = partnerId);
                }
                foreach (var partnerId in originalAndTpmIdMapping.Values.ToList().Distinct())
                {
                    foreach (var agreement in selectedAgreements)
                    {
                        if (agreement.GuestPartnerId == partnerId)
                        {
                            if (!agreementsToBeConsolidated.ContainsKey(agreement.HostPartnerId + "_" + partnerId))
                            {
                                List<AgreementMigrationItemViewModel> list = new List<AgreementMigrationItemViewModel>();
                                list.Add(agreement);
                                agreementsToBeConsolidated.Add(agreement.HostPartnerId + "_" + partnerId, list);
                            }
                            else
                            {
                                agreementsToBeConsolidated[agreement.HostPartnerId + "_" + partnerId].Add(agreement);

                            }
                        }
                    }
                }
                foreach (var consolidatedAgreement in agreementsToBeConsolidated)
                {
                    if (consolidatedAgreement.Value.Count > 1)
                    {
                        finalAgreementsToBeConsolidated.Add(consolidatedAgreement.Key, consolidatedAgreement.Value);
                    }
                }
                foreach (var consolidatedAgreement in finalAgreementsToBeConsolidated)
                {
                    string guestPartnerId = consolidatedAgreement.Key.Split('_')[1];
                    AgreementMigrationItemViewModel baseAgreement;
                    if (consolidatedAgreement.Value.Count(x => x.GuestPartnerTpmId == guestPartnerId) != 0)
                    {
                        baseAgreement = consolidatedAgreement.Value.Where(x => x.GuestPartnerTpmId == guestPartnerId).First();
                    }
                    else
                    {
                        baseAgreement = consolidatedAgreement.Value.First();
                    }
                    List<string> transactionsSupported = new List<string>();
                    foreach (var agreement in consolidatedAgreement.Value)
                    {
                        transactionsSupported.AddRange(agreement.Transactions);
                    }
                    AgreementMigrationItemViewModel ConsolidatedAgreement = new AgreementMigrationItemViewModel(baseAgreement);
                    ConsolidatedAgreement.BaseAgreementName = baseAgreement.MigrationEntity.Name;
                    ConsolidatedAgreement.HostPartnerId = baseAgreement.HostPartnerId;
                    ConsolidatedAgreement.HostPartnerQualifier = baseAgreement.HostPartnerQualifier;
                    ConsolidatedAgreement.GuestPartnerId = baseAgreement.GuestPartnerId;
                    ConsolidatedAgreement.GuestPartnerQualifer = baseAgreement.GuestPartnerQualifer;
                    ConsolidatedAgreement.Protocol = baseAgreement.MigrationEntity.Protocol;
                    ConsolidatedAgreement.IsConsolidated = true;
                    ConsolidatedAgreement.Name = baseAgreement.HostedPartnerName + "_" + baseAgreement.GuestPartnerName + "_" + string.Join("_", transactionsSupported);
                    ConsolidatedAgreement.Transactions = transactionsSupported;
                    ConsolidatedAgreement.AgreementsToConsolidate = new List<AgreementMigrationItemViewModel>();
                    consolidatedAgreement.Value.ForEach(x => ConsolidatedAgreement.AgreementsToConsolidate.Add(x));
                    listAgreementsToConsolidate.Add(ConsolidatedAgreement);
                }
                return listAgreementsToConsolidate;
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine("Error while trying to identify agreements to consolidate. Reason : " + ExceptionHelper.GetExceptionMessage(ex));
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public List<AgreementMigrationItemViewModel> GetAgreementsList(List<AgreementMigrationItemViewModel> agreements, List<AgreementMigrationItemViewModel> consolidatedAgreements)
        {
            List<AgreementMigrationItemViewModel> listFinalAgreements = new List<AgreementMigrationItemViewModel>();
            foreach (var agreement in agreements)
            {
                if (consolidatedAgreements.Count(x => x.AgreementsToConsolidate.Contains(agreement)) == 0)
                {
                    listFinalAgreements.Add(agreement);
                }
            }
            consolidatedAgreements.ForEach(x => listFinalAgreements.Add(x));
            return listFinalAgreements;
        }

        public static bool partnerDetailsCreated = false;
        public void GenerateMetadataContext(string partnerName, AgreementMigrationItemViewModel agreementItem, Server.X12ProtocolSettings protocolSettingsAB, Server.X12ProtocolSettings protocolSettingsBA)
        {
            MetaDataContextMigrator metadata = new MetaDataContextMigrator();
            if (!partnerDetailsCreated)
            {
                metadata.CreatePartnerFiles(partnerName, agreementItem);
                partnerDetailsCreated = true;
            }
            if (agreementItem.MigrationEntity.Protocol.ToUpper() == "X12")
            {
                #region Inbound 
                if (protocolSettingsAB.SchemaSettings.TargetNamespace != "http://schemas.microsoft.com/BizTalk/EDI/X12/2006")
                {
                    string transactionType = protocolSettingsAB.SchemaSettings.TargetNamespace.Substring(protocolSettingsAB.SchemaSettings.TargetNamespace.Length - 3);
                    string mapNamespace = string.Empty, mapName = string.Empty, indoc = string.Empty, indocItem = string.Empty, outdoc = string.Empty, outdocItem = string.Empty;
                    string inboundSchemaId = string.Empty, outboundSchemaId = string.Empty;
                    string connectionString = thisApplicationContext.GetProperty("DatabaseConnectionString") as string;
                    string biztalkMgmtDb = ConfigurationManager.AppSettings["BizTalkMgmtDbName"];
                    Match match = Regex.Match(connectionString, "Initial Catalog=(.*?);");
                    if (match.Success)
                    {
                        connectionString = connectionString.Replace(match.Groups[1].Value, biztalkMgmtDb);
                    }
                    #region SqlQuery
                    using (SqlConnection cn = new SqlConnection(connectionString))
                    {
                        try
                        {
                            var query = string.Format(SqlQueries.inboundQueryMapName, partnerName, protocolSettingsAB.SchemaSettings.TargetNamespace);
                            using (var cmd = new SqlCommand(query, cn))
                            {
                                if (cn.State == System.Data.ConnectionState.Closed)
                                {
                                    try { cn.Open(); }
                                    catch (Exception e)
                                    {
                                        string message = $"ERROR! Unable to establish connection to the ebisDB database. \nErrorMessage:{e.Message}";
                                        TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                                        throw new Exception(message);
                                    }
                                }
                                using (var rdr = cmd.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        mapName = rdr["mapName"].ToString();
                                        indoc = rdr["InMsg"].ToString();
                                        indocItem = rdr["InMsgItem"].ToString();
                                        outdoc = rdr["OutMsg"].ToString();
                                        outdocItem = rdr["OutMsgItem"].ToString();
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    #endregion
                    #region DocumentDb
                    if (indoc.Contains('#'))
                    {
                        string[] temp = indoc.Split('#');
                        inboundSchemaId = metadata.QuerySchema(partnerName, temp[0], temp[1], indocItem, MetadataRecordTypes.Schema);
                    }
                    else
                    {
                        inboundSchemaId = metadata.QuerySchema(partnerName, indoc, indoc, indocItem, MetadataRecordTypes.Schema);
                    }
                    if (outdoc.Contains('#'))
                    {
                        string[] temp = outdoc.Split('#');
                        outboundSchemaId = metadata.QuerySchema(partnerName, temp[0], temp[1], outdocItem, MetadataRecordTypes.Schema);
                    }
                    else
                    {
                        outboundSchemaId = metadata.QuerySchema(partnerName, outdoc, outdoc, outdocItem, MetadataRecordTypes.Schema);
                    }
                    string transformId = string.Empty;
                    if (!string.IsNullOrEmpty(inboundSchemaId) && !string.IsNullOrEmpty(outboundSchemaId))
                    {
                        if (indoc.Contains("867") || indoc.Contains("846"))
                        {
                            string interchangeInboundSchemaId = metadata.QuerySchema(partnerName, "http://schemas.microsoft.com/BizTalk/EDI/X12/2006/InterchangeXML", "X12InterchangeXml", indocItem, MetadataRecordTypes.Schema);
                            transformId = metadata.QueryTransform(partnerName,interchangeInboundSchemaId, outboundSchemaId, mapName, MetadataRecordTypes.Transform);
                        }
                        else
                            transformId = metadata.QueryTransform(partnerName,inboundSchemaId, outboundSchemaId, mapName, MetadataRecordTypes.Transform);
                        string ibFlowContextId = metadata.QueryibFlowContext(partnerName, outboundSchemaId, AppConstants.schemaXpathValue, transactionType,transformId, MetadataRecordTypes.ibFlowContext);
                        if (!string.IsNullOrEmpty(transformId) && !string.IsNullOrEmpty(ibFlowContextId))
                            metadata.CreateibContextFiles(partnerName, agreementItem, inboundSchemaId, ibFlowContextId, transformId, transactionType);
                    }
                    #endregion
                }
                #endregion
                #region Outbound
                if (protocolSettingsBA.SchemaSettings.TargetNamespace != protocolSettingsAB.SchemaSettings.TargetNamespace)
                {
                    string transactionType = protocolSettingsBA.SchemaSettings.TargetNamespace.Substring(protocolSettingsBA.SchemaSettings.TargetNamespace.Length - 3);
                    string mapNamespace = string.Empty, mapName = string.Empty, indoc = string.Empty, indocItem = string.Empty, outdoc = string.Empty, outdocItem = string.Empty;
                    string inboundSchemaId = string.Empty, outboundSchemaId = string.Empty;
                    string connectionString = thisApplicationContext.GetProperty("DatabaseConnectionString") as string;
                    string biztalkMgmtDb = ConfigurationManager.AppSettings["BizTalkMgmtDbName"];
                    Match match = Regex.Match(connectionString, "Initial Catalog=(.*?);");
                    if (match.Success)
                    {
                        connectionString = connectionString.Replace(match.Groups[1].Value, biztalkMgmtDb);
                    }
                    #region SqlQuery
                    using (SqlConnection cn = new SqlConnection(connectionString))
                    {
                        try
                        {
                            var query = string.Format(SqlQueries.outboundQueryMapName, partnerName, protocolSettingsBA.SchemaSettings.TargetNamespace);
                            using (var cmd = new SqlCommand(query, cn))
                            {
                                if (cn.State == System.Data.ConnectionState.Closed)
                                {
                                    try { cn.Open(); }
                                    catch (Exception e)
                                    {
                                        string message = $"ERROR! Unable to establish connection to the ebisDB database. \nErrorMessage:{e.Message}";
                                        TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                                        throw new Exception(message);
                                    }
                                }
                                using (var rdr = cmd.ExecuteReader())
                                {
                                    while (rdr.Read())
                                    {
                                        mapName = rdr["MapName"].ToString();
                                        indoc = rdr["InMsgType"].ToString();
                                        outdoc = rdr["OutMsgType"].ToString();
                                        indocItem = rdr["InMsgItem"].ToString();
                                        outdocItem = rdr["OutMsgItem"].ToString();

                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            throw ex;
                        }
                    }
                    #endregion
                    #region DocumentDb
                    try
                    {
                        if (indoc.Contains('#'))
                        {
                            string[] temp = indoc.Split('#');
                            inboundSchemaId = metadata.QuerySchema(partnerName, temp[0], temp[1], indocItem, MetadataRecordTypes.Schema);
                        }
                        else
                        {
                            inboundSchemaId = metadata.QuerySchema(partnerName, indoc, indoc, indocItem, MetadataRecordTypes.Schema);
                        }
                        if (outdoc.Contains('#'))
                        {
                            string[] temp = outdoc.Split('#');
                            outboundSchemaId = metadata.QuerySchema(partnerName, temp[0], temp[1], outdocItem, MetadataRecordTypes.Schema);
                        }
                        else
                        {
                            outboundSchemaId = metadata.QuerySchema(partnerName, outdoc, outdoc, outdocItem, MetadataRecordTypes.Schema);
                        }
                        if (!string.IsNullOrEmpty(inboundSchemaId) && !string.IsNullOrEmpty(outboundSchemaId))
                        {
                            string transformId = metadata.QueryTransform(partnerName ,inboundSchemaId, outboundSchemaId, mapName, MetadataRecordTypes.Transform);
                            string obFlowContextId = metadata.QueryobFlowContext(partnerName,inboundSchemaId, indoc, transactionType,transformId, MetadataRecordTypes.obFlowContext);
                            if (!string.IsNullOrEmpty(transformId) && !string.IsNullOrEmpty(obFlowContextId))
                                metadata.CreateobX12ContextFiles(partnerName, agreementItem, protocolSettingsBA, outboundSchemaId, obFlowContextId, transformId, outdoc, transactionType);
                        }
                    }
                    catch (Exception)
                    {
                        //Do Nothing
                    }
                    #endregion
                }
                #endregion
            }
        }
    }
}
