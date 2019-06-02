using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.EDIFACTAgreementJson
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
        public Edifact edifact { get; set; }
    }

    public class Edifact
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
        public Validationsettings validationSettings { get; set; }
        public Framingsettings framingSettings { get; set; }
        public Envelopesettings envelopeSettings { get; set; }
        public Acknowledgementsettings acknowledgementSettings { get; set; }
        public Messagefilter messageFilter { get; set; }
        public Processingsettings processingSettings { get; set; }
        public EnvelopeOverrides[] envelopeOverrides { get; set; }
        public EDIFACTMessageIdentifier[] messageFilterList { get; set; }
        public Schemareference[] schemaReferences { get; set; }

        public ValidationOverrides[] validationOverrides { get; set; }
        public EdifactDelimiterOverrides[] edifactDelimiterOverrides { get; set; }

    }

    public class EDIFACTMessageIdentifier
    {
        public string MessageId { get; set; }

    }

    public class EnvelopeOverrides
    {
        public string ApplicationPassword { get; set; }
        public string AssociationAssignedCode { get; set; }
        public string ControllingAgencyCode { get; set; }
        public string FunctionalGroupId { get; set; }
        public string GroupHeaderMessageRelease { get; set; }
        public string GroupHeaderMessageVersion { get; set; }
        public string MessageAssociationAssignedCode { get; set; }
        public string MessageId { get; set; }
        public string MessageRelease { get; set; }
        public string MessageVersion { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string ReceiverApplicationQualifier { get; set; }
        public string SenderApplicationId { get; set; }
        public string SenderApplicationQualifier { get; set; }
        public string TargetNamespace { get; set; }

    }

    public class ValidationOverrides
    {
        public bool AllowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool EnforceCharacterSet { get; set; }
        public string TrailingSeparatorPolicy { get; set; }
        public string MessageId { get; set; }
        public bool TrimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool ValidateEDITypes { get; set; }
        public bool ValidateXSDTypes { get; set; }
    }


    public class Validationsettings
    {
        public bool validateCharacterSet { get; set; }
        public bool checkDuplicateInterchangeControlNumber { get; set; }
        public int interchangeControlNumberValidityDays { get; set; }
        public bool checkDuplicateGroupControlNumber { get; set; }
        public bool checkDuplicateTransactionSetControlNumber { get; set; }
        public bool validateEDITypes { get; set; }
        public bool validateXSDTypes { get; set; }
        public bool trimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool allowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string trailingSeparatorPolicy { get; set; }
    }

    public class Framingsettings
    {
        public int ComponentSeparator { get; set; }
        public string CharacterEncoding { get; set; }
        public int protocolVersion { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public int releaseIndicator { get; set; }
        public int repetitionSeparator { get; set; }
        public string characterSet { get; set; }
        public string decimalPointIndicator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
    }

    public class Envelopesettings
    {
        public bool applyDelimiterStringAdvice { get; set; }
        public bool createGroupingSegments { get; set; }
        public bool enableDefaultGroupHeaders { get; set; }
        public long interchangeControlNumberLowerBound { get; set; }
        public long interchangeControlNumberUpperBound { get; set; }
        public bool rolloverInterchangeControlNumber { get; set; }
        public long groupControlNumberLowerBound { get; set; }
        public long groupControlNumberUpperBound { get; set; }
        public bool rolloverGroupControlNumber { get; set; }
        public bool overwriteExistingTransactionSetControlNumber { get; set; }
        public long transactionSetControlNumberLowerBound { get; set; }
        public long transactionSetControlNumberUpperBound { get; set; }
        public bool rolloverTransactionSetControlNumber { get; set; }
        public bool isTestInterchange { get; set; }
        public string ApplicationReferenceId { get; set; }

        public string CommunicationAgreementId { get; set; }


        public string FunctionalGroupId { get; set; }
        public string GroupApplicationPassword { get; set; }
        public string GroupApplicationReceiverId { get; set; }
        public string GroupApplicationReceiverQualifier { get; set; }
        public string GroupApplicationSenderId { get; set; }
        public string GroupApplicationSenderQualifier { get; set; }
        public string GroupAssociationAssignedCode { get; set; }
        public string GroupControllingAgencyCode { get; set; }

        public string GroupControlNumberPrefix { get; set; }
        public string GroupControlNumberSuffix { get; set; }

        public string GroupMessageRelease { get; set; }
        public string GroupMessageVersion { get; set; }

        public string InterchangeControlNumberPrefix { get; set; }
        public string InterchangeControlNumberSuffix { get; set; }



        public string ProcessingPriorityCode { get; set; }
        public string ReceiverInternalIdentification { get; set; }
        public string ReceiverInternalSubIdentification { get; set; }
        public string ReceiverReverseRoutingAddress { get; set; }
        public string RecipientReferencePasswordQualifier { get; set; }
        public string RecipientReferencePasswordValue { get; set; }
        public string SenderInternalIdentification { get; set; }
        public string SenderInternalSubIdentification { get; set; }
        public string SenderReverseRoutingAddress { get; set; }

        public string TransactionSetControlNumberPrefix { get; set; }
        public string TransactionSetControlNumberSuffix { get; set; }
    }

    public class Acknowledgementsettings
    {
        public bool needTechnicalAcknowledgement { get; set; }
        public bool batchTechnicalAcknowledgements { get; set; }
        public bool needFunctionalAcknowledgement { get; set; }
        public bool batchFunctionalAcknowledgements { get; set; }
        public bool needLoopForValidMessages { get; set; }
        public bool sendSynchronousAcknowledgement { get; set; }
        public long acknowledgementControlNumberLowerBound { get; set; }
        public long acknowledgementControlNumberUpperBound { get; set; }
        public bool rolloverAcknowledgementControlNumber { get; set; }
        public string AcknowledgementControlNumberPrefix { get; set; }
        public string AcknowledgementControlNumberSuffix { get; set; }
    }

    public class Messagefilter
    {
        public string messageFilterType { get; set; }
    }

    public class Processingsettings
    {
        public bool maskSecurityInfo { get; set; }
        public bool preserveInterchange { get; set; }
        public bool suspendInterchangeOnError { get; set; }
        public bool createEmptyXmlTagsForTrailingSeparators { get; set; }
        public bool useDotAsDecimalSeparator { get; set; }
    }

    public class Schemareference
    {
        public string messageId { get; set; }
        public string messageVersion { get; set; }
        public string messageRelease { get; set; }
        public string schemaName { get; set; }
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
        public Validationsettings1 validationSettings { get; set; }
        public Framingsettings1 framingSettings { get; set; }
        public Envelopesettings1 envelopeSettings { get; set; }
        public Acknowledgementsettings1 acknowledgementSettings { get; set; }
        public Messagefilter1 messageFilter { get; set; }
        public Processingsettings1 processingSettings { get; set; }
        public EnvelopeOverrides1[] envelopeOverrides { get; set; }
        public EDIFACTMessageIdentifier[] messageFilterList { get; set; }
        public Schemareference1[] schemaReferences { get; set; }
        public ValidationOverrides1[] validationOverrides { get; set; }
        public EdifactDelimiterOverrides1[] edifactDelimiterOverrides { get; set; }
    }

    public class EdifactDelimiterOverrides
    {
        public string messageId { get;set;}
        public string messageVersion { get; set; }
        public string messageRelease { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public int repetitionSeparator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
        public string decimalPointIndicator { get; set; }
        public int releaseIndicator { get; set; }
        public string targetNamespace { get; set; }
    }
    public class EdifactDelimiterOverrides1
    {
        public string messageId { get; set; }
        public string messageVersion { get; set; }
        public string messageRelease { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public int repetitionSeparator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
        public string decimalPointIndicator { get; set; }
        public int releaseIndicator { get; set; }
        public string targetNamespace { get; set; }
    }

    public class EnvelopeOverrides1
    {
        public string ApplicationPassword { get; set; }
        public string AssociationAssignedCode { get; set; }
        public string ControllingAgencyCode { get; set; }
        public string FunctionalGroupId { get; set; }
        public string GroupHeaderMessageRelease { get; set; }
        public string GroupHeaderMessageVersion { get; set; }
        public string MessageAssociationAssignedCode { get; set; }
        public string MessageId { get; set; }
        public string MessageRelease { get; set; }
        public string MessageVersion { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string ReceiverApplicationQualifier { get; set; }
        public string SenderApplicationId { get; set; }
        public string SenderApplicationQualifier { get; set; }
        public string TargetNamespace { get; set; }

    }

    public class ValidationOverrides1
    {
        public bool AllowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool EnforceCharacterSet { get; set; }
        public string TrailingSeparatorPolicy { get; set; }
        public string MessageId { get; set; }
        public bool TrimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool ValidateEDITypes { get; set; }
        public bool ValidateXSDTypes { get; set; }
    }

    public class Validationsettings1
    {
        public bool validateCharacterSet { get; set; }
        public bool checkDuplicateInterchangeControlNumber { get; set; }
        public int interchangeControlNumberValidityDays { get; set; }
        public bool checkDuplicateGroupControlNumber { get; set; }
        public bool checkDuplicateTransactionSetControlNumber { get; set; }
        public bool validateEDITypes { get; set; }
        public bool validateXSDTypes { get; set; }
        public bool trimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool allowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string trailingSeparatorPolicy { get; set; }
    }

    public class Framingsettings1
    {
        public int protocolVersion { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public int releaseIndicator { get; set; }
        public int repetitionSeparator { get; set; }
        public string characterSet { get; set; }
        public string decimalPointIndicator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
        public int ComponentSeparator { get; set; }
        public string CharacterEncoding { get; set; }
    }

    public class Envelopesettings1
    {
        public bool applyDelimiterStringAdvice { get; set; }
        public bool createGroupingSegments { get; set; }
        public bool enableDefaultGroupHeaders { get; set; }
        public long interchangeControlNumberLowerBound { get; set; }
        public long interchangeControlNumberUpperBound { get; set; }
        public bool rolloverInterchangeControlNumber { get; set; }
        public long groupControlNumberLowerBound { get; set; }
        public long groupControlNumberUpperBound { get; set; }
        public bool rolloverGroupControlNumber { get; set; }
        public bool overwriteExistingTransactionSetControlNumber { get; set; }
        public long transactionSetControlNumberLowerBound { get; set; }
        public long transactionSetControlNumberUpperBound { get; set; }
        public bool rolloverTransactionSetControlNumber { get; set; }
        public bool isTestInterchange { get; set; }

        public string ApplicationReferenceId { get; set; }

        public string CommunicationAgreementId { get; set; }


        public string FunctionalGroupId { get; set; }
        public string GroupApplicationPassword { get; set; }
        public string GroupApplicationReceiverId { get; set; }
        public string GroupApplicationReceiverQualifier { get; set; }
        public string GroupApplicationSenderId { get; set; }
        public string GroupApplicationSenderQualifier { get; set; }
        public string GroupAssociationAssignedCode { get; set; }
        public string GroupControllingAgencyCode { get; set; }

        public string GroupControlNumberPrefix { get; set; }
        public string GroupControlNumberSuffix { get; set; }

        public string GroupMessageRelease { get; set; }
        public string GroupMessageVersion { get; set; }

        public string InterchangeControlNumberPrefix { get; set; }
        public string InterchangeControlNumberSuffix { get; set; }



        public string ProcessingPriorityCode { get; set; }
        public string ReceiverInternalIdentification { get; set; }
        public string ReceiverInternalSubIdentification { get; set; }
        public string ReceiverReverseRoutingAddress { get; set; }
        public string RecipientReferencePasswordQualifier { get; set; }
        public string RecipientReferencePasswordValue { get; set; }
        public string SenderInternalIdentification { get; set; }
        public string SenderInternalSubIdentification { get; set; }
        public string SenderReverseRoutingAddress { get; set; }

        public string TransactionSetControlNumberPrefix { get; set; }
        public string TransactionSetControlNumberSuffix { get; set; }
    }

    public class Acknowledgementsettings1
    {
        public bool needTechnicalAcknowledgement { get; set; }
        public bool batchTechnicalAcknowledgements { get; set; }
        public bool needFunctionalAcknowledgement { get; set; }
        public bool batchFunctionalAcknowledgements { get; set; }
        public bool needLoopForValidMessages { get; set; }
        public bool sendSynchronousAcknowledgement { get; set; }
        public long acknowledgementControlNumberLowerBound { get; set; }
        public long acknowledgementControlNumberUpperBound { get; set; }
        public bool rolloverAcknowledgementControlNumber { get; set; }
        public string AcknowledgementControlNumberPrefix { get; set; }
        public string AcknowledgementControlNumberSuffix { get; set; }
    }

    public class Messagefilter1
    {
        public string messageFilterType { get; set; }
    }

    public class Processingsettings1
    {
        public bool maskSecurityInfo { get; set; }
        public bool preserveInterchange { get; set; }
        public bool suspendInterchangeOnError { get; set; }
        public bool createEmptyXmlTagsForTrailingSeparators { get; set; }
        public bool useDotAsDecimalSeparator { get; set; }
    }

    public class Schemareference1
    {
        public string messageId { get; set; }
        public string messageVersion { get; set; }
        public string messageRelease { get; set; }
        public string schemaName { get; set; }
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

}