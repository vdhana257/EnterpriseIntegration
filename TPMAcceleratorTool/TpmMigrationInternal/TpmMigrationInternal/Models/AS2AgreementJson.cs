using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.JsonAs2Agreement
{

    public class Rootobject
    {
        public Properties properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }

    }

    public class Properties
    {
        public string hostPartner { get; set; }
        public string guestPartner { get; set; }
        public Hostidentity hostIdentity { get; set; }
        public Guestidentity guestIdentity { get; set; }
        public string agreementType { get; set; }
        public Content content { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime changedTime { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Hostidentity
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Guestidentity
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Content
    {
        public As2 aS2 { get; set; }
    }

    public class As2
    {
        public Receiveagreement receiveAgreement { get; set; }
        public Sendagreement sendAgreement { get; set; }
    }

    public class Receiveagreement
    {
        public Protocolsettings protocolSettings { get; set; }
        public Senderbusinessidentity senderBusinessIdentity { get; set; }
        public Receiverbusinessidentity receiverBusinessIdentity { get; set; }
    }

    public class Protocolsettings
    {
        public Messageconnectionsettings messageConnectionSettings { get; set; }
        public Acknowledgementconnectionsettings acknowledgementConnectionSettings { get; set; }
        public Mdnsettings mdnSettings { get; set; }
        public Securitysettings securitySettings { get; set; }
        public Validationsettings validationSettings { get; set; }
        public Envelopesettings envelopeSettings { get; set; }
        public Errorsettings errorSettings { get; set; }
    }

    public class Messageconnectionsettings
    {
        public bool ignoreCertificateNameMismatch { get; set; }
        public bool supportHttpStatusCodeContinue { get; set; }
        public bool keepHttpConnectionAlive { get; set; }
        public bool unfoldHttpHeaders { get; set; }
    }

    public class Acknowledgementconnectionsettings
    {
        public bool ignoreCertificateNameMismatch { get; set; }
        public bool supportHttpStatusCodeContinue { get; set; }
        public bool keepHttpConnectionAlive { get; set; }
        public bool unfoldHttpHeaders { get; set; }
    }

    public class Mdnsettings
    {
        public bool needMDN { get; set; }
        public bool signMDN { get; set; }
        public bool sendMDNAsynchronously { get; set; }
        public string receiptDeliveryUrl { get; set; }
        public string dispositionNotificationTo { get; set; }
        public bool signOutboundMDNIfOptional { get; set; }
        public bool sendInboundMDNToMessageBox { get; set; }
        public string micHashingAlgorithm { get; set; }
    }

    public class Securitysettings
    {
        public bool overrideGroupSigningCertificate { get; set; }
        public bool enableNRRForInboundEncodedMessages { get; set; }
        public bool enableNRRForInboundDecodedMessages { get; set; }
        public bool enableNRRForOutboundMDN { get; set; }
        public bool enableNRRForOutboundEncodedMessages { get; set; }
        public bool enableNRRForOutboundDecodedMessages { get; set; }
        public bool enableNRRForInboundMDN { get; set; }
        public string signingCertificateName { get; set; }
        public string encryptionCertificateName { get; set; }

    }

    public class Validationsettings
    {
        public bool overrideMessageProperties { get; set; }
        public bool encryptMessage { get; set; }
        public bool signMessage { get; set; }
        public bool compressMessage { get; set; }
        public bool checkDuplicateMessage { get; set; }
        public int interchangeDuplicatesValidityDays { get; set; }
        public bool checkCertificateRevocationListOnSend { get; set; }
        public bool checkCertificateRevocationListOnReceive { get; set; }
        public string encryptionAlgorithm { get; set; }
    }

    public class Envelopesettings
    {
        public string messageContentType { get; set; }
        public bool transmitFileNameInMimeHeader { get; set; }
        public string fileNameTemplate { get; set; }
        public bool suspendMessageOnFileNameGenerationError { get; set; }
        public bool autogenerateFileName { get; set; }
    }

    public class Errorsettings
    {
        public bool suspendDuplicateMessage { get; set; }
        public bool resendIfMDNNotReceived { get; set; }
    }

    public class Senderbusinessidentity
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Receiverbusinessidentity
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Sendagreement
    {
        public Protocolsettings1 protocolSettings { get; set; }
        public Senderbusinessidentity1 senderBusinessIdentity { get; set; }
        public Receiverbusinessidentity1 receiverBusinessIdentity { get; set; }
    }

    public class Protocolsettings1
    {
        public Messageconnectionsettings1 messageConnectionSettings { get; set; }
        public Acknowledgementconnectionsettings1 acknowledgementConnectionSettings { get; set; }
        public Mdnsettings1 mdnSettings { get; set; }
        public Securitysettings1 securitySettings { get; set; }
        public Validationsettings1 validationSettings { get; set; }
        public Envelopesettings1 envelopeSettings { get; set; }
        public Errorsettings1 errorSettings { get; set; }
    }

    public class Messageconnectionsettings1
    {
        public bool ignoreCertificateNameMismatch { get; set; }
        public bool supportHttpStatusCodeContinue { get; set; }
        public bool keepHttpConnectionAlive { get; set; }
        public bool unfoldHttpHeaders { get; set; }
    }

    public class Acknowledgementconnectionsettings1
    {
        public bool ignoreCertificateNameMismatch { get; set; }
        public bool supportHttpStatusCodeContinue { get; set; }
        public bool keepHttpConnectionAlive { get; set; }
        public bool unfoldHttpHeaders { get; set; }
    }

    public class Mdnsettings1
    {
        public bool needMDN { get; set; }
        public bool signMDN { get; set; }
        public bool sendMDNAsynchronously { get; set; }
        public string receiptDeliveryUrl { get; set; }
        public string dispositionNotificationTo { get; set; }
        public bool signOutboundMDNIfOptional { get; set; }
        public bool sendInboundMDNToMessageBox { get; set; }
        public string micHashingAlgorithm { get; set; }
    }

    public class Securitysettings1
    {
        public bool overrideGroupSigningCertificate { get; set; }
        public bool enableNRRForInboundEncodedMessages { get; set; }
        public bool enableNRRForInboundDecodedMessages { get; set; }
        public bool enableNRRForOutboundMDN { get; set; }
        public bool enableNRRForOutboundEncodedMessages { get; set; }
        public bool enableNRRForOutboundDecodedMessages { get; set; }
        public bool enableNRRForInboundMDN { get; set; }
        public string signingCertificateName { get; set; }
        public string encryptionCertificateName { get; set; }

    }

    public class Validationsettings1
    {
        public bool overrideMessageProperties { get; set; }
        public bool encryptMessage { get; set; }
        public bool signMessage { get; set; }
        public bool compressMessage { get; set; }
        public bool checkDuplicateMessage { get; set; }
        public int interchangeDuplicatesValidityDays { get; set; }
        public bool checkCertificateRevocationListOnSend { get; set; }
        public bool checkCertificateRevocationListOnReceive { get; set; }
        public string encryptionAlgorithm { get; set; }
    }

    public class Envelopesettings1
    {
        public string messageContentType { get; set; }
        public bool transmitFileNameInMimeHeader { get; set; }
        public string fileNameTemplate { get; set; }
        public bool suspendMessageOnFileNameGenerationError { get; set; }
        public bool autogenerateFileName { get; set; }
    }

    public class Errorsettings1
    {
        public bool suspendDuplicateMessage { get; set; }
        public bool resendIfMDNNotReceived { get; set; }
    }

    public class Senderbusinessidentity1
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Receiverbusinessidentity1
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Tpcontext
    {
        public As2decodeconfig as2DecodeConfig { get; set; }
        public Predecodeprocessing[] preDecodeProcessing { get; set; }
        public Edidecodeconfig ediDecodeConfig { get; set; }
    }

    public class As2decodeconfig
    {
        public string aS2From { get; set; }
        public string aS2To { get; set; }
    }

    public class Edidecodeconfig
    {
        public string key1 { get; set; }
        public string key2 { get; set; }
    }

    public class Predecodeprocessing
    {
        public string name { get; set; }
        public string isEnabled { get; set; }
        public Parameter[] parameters { get; set; }
    }

    public class Parameter
    {
        public string name { get; set; }
        public string Value { get; set; }
    }




}
