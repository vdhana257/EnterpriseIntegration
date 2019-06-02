using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.X12AgreementJson
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
        public X12 x12 { get; set; }
    }

    public class X12
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
        public Securitysettings securitySettings { get; set; }
        public Processingsettings processingSettings { get; set; }
        public EnvelopeOverrides[] envelopeOverrides { get; set; }
        public ValidationOverrides[] validationOverrides { get; set; }
        public Schemareference[] schemaReferences { get; set; }

        public X12DelimiterOverrides[] x12DelimiterOverrides { get; set; }


    }

    public class X12DelimiterOverrides
    {
        public string protocolVersion { get; set; }
        public string messageId { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
        public int replaceCharacter { get; set; }
        public bool replaceSeparatorsInPayload { get; set; }
        public string targetNamespace { get; set; }
    }


    public class ValidationOverrides
    {
        public bool AllowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string MessageId { get; set; }
        public string TrailingSeparatorPolicy { get; set; }
        public bool TrimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool ValidateCharacterSet { get; set; }
        public bool ValidateEDITypes { get; set; }
        public bool ValidateXSDTypes { get; set; }
    }

    public class EnvelopeOverrides
    {
        public string DateFormat { get; set; }
        public string FunctionalIdentifierCode { get; set; }
        public string HeaderVersion { get; set; }
        public string MessageId { get; set; }
        public string ProtocolVersion { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string ResponsibleAgencyCode { get; set; }
        public string SenderApplicationId { get; set; }
        public string TargetNamespace { get; set; }
        // public string TargetNamespaceString { get; set; }
        public string TimeFormat { get; set; }
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
        public bool allowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool trimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string trailingSeparatorPolicy { get; set; }


    }

    public class Framingsettings
    {

        public string CharacterSet { get; set; }
        public int ComponentSeparator { get; set; }
        public int DataElementSeparator { get; set; }
        public int replaceCharacter { get; set; }
        public bool ReplaceSeparatorsInPayload { get; set; }
        public int SegmentTerminator { get; set; }
        public string SegmentTerminatorSuffix { get; set; }


    }

    public class Envelopesettings
    {
        public int ControlStandardsId { get; set; }
        public string ControlVersionNumber { get; set; }
        public bool EnableDefaultGroupHeaders { get; set; }
        public string FunctionalGroupId { get; set; }
        public int GroupControlNumberLowerBound { get; set; }
        public bool rolloverGroupControlNumber { get; set; }//rolloverGroupControlNumber
        public int GroupControlNumberUpperBound { get; set; }
        public string GroupHeaderAgencyCode { get; set; }
        public string GroupHeaderDateFormat { get; set; }
        public string GroupHeaderTimeFormat { get; set; }
        public string GroupHeaderVersion { get; set; }
        public int InterchangeControlNumberLowerBound { get; set; }
        public bool rolloverInterchangeControlNumber { get; set; }//rolloverInterchangeControlNumber
        public int InterchangeControlNumberUpperBound { get; set; }
        public bool OverwriteExistingTransactionSetControlNumber { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string SenderApplicationId { get; set; }
        public int TransactionSetControlNumberLowerBound { get; set; }
        public string TransactionSetControlNumberPrefix { get; set; }
        public bool rolloverTransactionSetControlNumber { get; set; }
        public string TransactionSetControlNumberSuffix { get; set; }
        public int TransactionSetControlNumberUpperBound { get; set; }
        public string UsageIndicator { get; set; }
        public bool UseControlStandardsIdAsRepetitionCharacter { get; set; }

    }

    public class Acknowledgementsettings
    {
        public string ImplementationAcknowledgementVersion { get; set; }

        public bool needTechnicalAcknowledgement { get; set; }
        public bool batchTechnicalAcknowledgements { get; set; }
        public bool needFunctionalAcknowledgement { get; set; }
        public string functionalAcknowledgementVersion { get; set; }
        public bool batchFunctionalAcknowledgements { get; set; }
        public bool needImplementationAcknowledgement { get; set; }
        public bool batchImplementationAcknowledgements { get; set; }
        public bool needLoopForValidMessages { get; set; }
        public bool sendSynchronousAcknowledgement { get; set; }
        public long acknowledgementControlNumberLowerBound { get; set; }
        public long acknowledgementControlNumberUpperBound { get; set; }
        public bool rolloverAcknowledgementControlNumber { get; set; }
        public string AcknowledgementControlNumberSuffix { get; set; }
        public string AcknowledgementControlNumberPrefix { get; set; }

    }

    public class Messagefilter
    {
        public string messageFilterType { get; set; }
    }

    public class Securitysettings
    {
        public string AuthorizationQualifier { get; set; }
        public string AuthorizationValue { get; set; }
        public string PasswordValue { get; set; }
        public string SecurityQualifier { get; set; }
    }

    public class Processingsettings
    {
        public bool maskSecurityInfo { get; set; }
        public bool convertImpliedDecimal { get; set; }
        public bool preserveInterchange { get; set; }
        public bool suspendInterchangeOnError { get; set; }
        public bool createEmptyXmlTagsForTrailingSeparators { get; set; }
        public bool useDotAsDecimalSeparator { get; set; }
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
        public Securitysettings1 securitySettings { get; set; }
        public Processingsettings1 processingSettings { get; set; }
        public EnvelopeOverrides1[] envelopeOverrides { get; set; }
        public ValidationOverrides1[] validationOverrides { get; set; }
        public Schemareference[] schemaReferences { get; set; }

        public X12DelimiterOverrides1[] x12DelimiterOverrides { get; set; }
    }

    public class ValidationOverrides1
    {
        public bool AllowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string MessageId { get; set; }
        public string TrailingSeparatorPolicy { get; set; }
        public bool TrimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool ValidateCharacterSet { get; set; }
        public bool ValidateEDITypes { get; set; }
        public bool ValidateXSDTypes { get; set; }
    }

    public class EnvelopeOverrides1
    {
        public string DateFormat { get; set; }
        public string FunctionalIdentifierCode { get; set; }
        public string HeaderVersion { get; set; }
        public string MessageId { get; set; }
        public string ProtocolVersion { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string ResponsibleAgencyCode { get; set; }
        public string SenderApplicationId { get; set; }
        public string TargetNamespace { get; set; }
        // public string TargetNamespaceString { get; set; }
        public string TimeFormat { get; set; }
    }

    public class X12DelimiterOverrides1
    {
        public string protocolVersion { get; set; }
        public string messageId { get; set; }
        public int dataElementSeparator { get; set; }
        public int componentSeparator { get; set; }
        public int segmentTerminator { get; set; }
        public string segmentTerminatorSuffix { get; set; }
        public int replaceCharacter { get; set; }
        public bool replaceSeparatorsInPayload { get; set; }
        public string targetNamespace { get; set; }
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
        public bool allowLeadingAndTrailingSpacesAndZeroes { get; set; }
        public bool trimLeadingAndTrailingSpacesAndZeroes { get; set; }
        public string trailingSeparatorPolicy { get; set; }
    }

    public class Framingsettings1
    {
        public string CharacterSet { get; set; }
        public int ComponentSeparator { get; set; }
        public int DataElementSeparator { get; set; }
        public int replaceCharacter { get; set; }
        public bool ReplaceSeparatorsInPayload { get; set; }
        public int SegmentTerminator { get; set; }
        public string SegmentTerminatorSuffix { get; set; }

    }

    public class Envelopesettings1
    {
        public int ControlStandardsId { get; set; }
        public string ControlVersionNumber { get; set; }
        public bool EnableDefaultGroupHeaders { get; set; }
        public string FunctionalGroupId { get; set; }
        public int GroupControlNumberLowerBound { get; set; }
        public bool rolloverGroupControlNumber { get; set; }
        public int GroupControlNumberUpperBound { get; set; }
        public string GroupHeaderAgencyCode { get; set; }
        public string GroupHeaderDateFormat { get; set; }
        public string GroupHeaderTimeFormat { get; set; }
        public string GroupHeaderVersion { get; set; }
        public int InterchangeControlNumberLowerBound { get; set; }
        public bool rolloverInterchangeControlNumber { get; set; }
        public int InterchangeControlNumberUpperBound { get; set; }
        public bool OverwriteExistingTransactionSetControlNumber { get; set; }
        public string ReceiverApplicationId { get; set; }
        public string SenderApplicationId { get; set; }
        public int TransactionSetControlNumberLowerBound { get; set; }
        public string TransactionSetControlNumberPrefix { get; set; }
        public bool rolloverTransactionSetControlNumber { get; set; }
        public string TransactionSetControlNumberSuffix { get; set; }
        public int TransactionSetControlNumberUpperBound { get; set; }
        public string UsageIndicator { get; set; }
        public bool UseControlStandardsIdAsRepetitionCharacter { get; set; }
    }

    public class Acknowledgementsettings1
    {
        public string ImplementationAcknowledgementVersion { get; set; }
        public bool needTechnicalAcknowledgement { get; set; }
        public bool batchTechnicalAcknowledgements { get; set; }
        public bool needFunctionalAcknowledgement { get; set; }
        public string functionalAcknowledgementVersion { get; set; }
        public bool batchFunctionalAcknowledgements { get; set; }
        public bool needImplementationAcknowledgement { get; set; }
        public bool batchImplementationAcknowledgements { get; set; }
        public bool needLoopForValidMessages { get; set; }
        public bool sendSynchronousAcknowledgement { get; set; }
        public long acknowledgementControlNumberLowerBound { get; set; }
        public long acknowledgementControlNumberUpperBound { get; set; }
        public bool rolloverAcknowledgementControlNumber { get; set; }
        public string AcknowledgementControlNumberSuffix { get; set; }
        public string AcknowledgementControlNumberPrefix { get; set; }

    }

    public class Messagefilter1
    {
        public string messageFilterType { get; set; }
    }

    public class Securitysettings1
    {
        public string AuthorizationQualifier { get; set; }
        public string AuthorizationValue { get; set; }
        public string PasswordValue { get; set; }
        public string SecurityQualifier { get; set; }
    }

    public class Processingsettings1
    {
        public bool maskSecurityInfo { get; set; }
        public bool convertImpliedDecimal { get; set; }
        public bool preserveInterchange { get; set; }
        public bool suspendInterchangeOnError { get; set; }
        public bool createEmptyXmlTagsForTrailingSeparators { get; set; }
        public bool useDotAsDecimalSeparator { get; set; }
    }

    public class Schemareference
    {
        public string messageId { get; set; }
        public string schemaName { get; set; }
        public string schemaVersion { get; set; }
        public string senderApplicationId { get; set; }

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
    public class DHL_810
    {
        public Pretransformprocessing[] preTransformProcessing { get; set; }
        public Sourceschemapropertypromotion[] sourceSchemaPropertyPromotion { get; set; }
        public Transformation transformation { get; set; }
        public object[] targetSchemaPropertyPromotion { get; set; }
        public object[] postTransformProcessing { get; set; }
        public As2encodeconfig as2EncodeConfig { get; set; }
        public Transportconfig transportConfig { get; set; }
    }

    public class Transformation
    {
        public Transformlookup transformLookup { get; set; }
        public string transformName { get; set; }
        public string preTransformSchemaName { get; set; }
    }

    public class Transformlookup
    {
    }

    public class As2encodeconfig
    {
        public string aS2From { get; set; }
        public string aS2To { get; set; }
    }

    public class Transportconfig
    {
        public string destinationURI { get; set; }
        public string transportType { get; set; }
        public Authenticationparams authenticationParams { get; set; }
    }

    public class Authenticationparams
    {
        public string certificateIdentifiers { get; set; }
        public string userName { get; set; }
        public string password { get; set; }
    }

    public class Pretransformprocessing
    {
        public string Name { get; set; }
        public string isEnabled { get; set; }
        public Parameter[] Parameters { get; set; }
    }

    public class Parameter
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    public class Sourceschemapropertypromotion
    {
        public string name { get; set; }
        public string value { get; set; }
    }

}