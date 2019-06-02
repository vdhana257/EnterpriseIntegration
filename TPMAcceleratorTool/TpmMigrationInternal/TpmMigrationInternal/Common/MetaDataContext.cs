using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    //partnerDetails
    #region PartnerDetails
    public class PartnerContext
    {
        public string id { get; set; }
        public string recordType { get; set; }
        public string description { get; set; }
        public Dockey docKey { get; set; }
        public As2config as2Config { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Preserveinterchange preserveInterchange { get; set; }
        public JArray preDecodeProcessing { get; set; }
        public JArray postDecodeProcessing { get; set; }
        public PreEncodeprocessing[] preEncodeProcessing { get; set; }
        public PostEncodeprocessing[] postEncodeProcessing { get; set; }
        public string epDetailId { get; set; }
    }

    public class Dockey
    {
        public string messageFormat { get; set; }
        public string senderQualifier { get; set; }
        public string senderIdentifier { get; set; }
        public string receiverQualifier { get; set; }
        public string receiverIdentifier { get; set; }
    }

    public class As2config
    {
        public string as2From { get; set; }
        public string as2To { get; set; }
    }

    public class Preserveinterchange
    {
        public bool IB { get; set; }
        public bool PT { get; set; }
        public bool PO { get; set; }
    }

    public class PreEncodeprocessing
    {
        public string name { get; set; }
        public string isEnabled { get; set; }
        public string version { get; set; }
        public Parameter[] parameters { get; set; }
    }
    public class PostEncodeprocessing
    {
        public string name { get; set; }
        public string isEnabled { get; set; }
        public string version { get; set; }
        public Parameter[] parameters { get; set; }
    }
    public class Parameter
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    #endregion

    //ibFlowPartnerContext
    #region iblowPartnerContext
    public class ibFlowPartnerContext
    {
        public string id { get; set; }
        public string ibFlowContextId { get; set; }
        public string recordType { get; set; }
        public string transformId { get; set; }
        public string description { get; set; }
        public ibFlowPartnerDockey docKey { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Targettrackedschemaproperty[] targetTrackedSchemaProperties { get; set; }
        public JArray preTransformProcessing { get; set; }
        public bool schemaValidationRequired { get; set; }
    }

    public class ibFlowPartnerDockey
    {
        public string schemaXpathValue { get; set; }
        public string schemaId { get; set; }
        public string partnerDetailId { get; set; }
    }
    public class Targettrackedschemaproperty
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    #endregion

    //obFlowPartnerContext
    #region obFlowPartnerContext
    public class obFlowPartnerContext
    {
        public string id { get; set; }
        public string schemaId { get; set; }
        public string recordType { get; set; }
        public string transformId { get; set; }
        public string description { get; set; }
        public obFlowPartnerDockey docKey { get; set; }
        public string documentKey { get; set; }
        public string documentType { get; set; }
        public JArray preTransformProcessing { get; set; }
        public JArray postTransformProcessing { get; set; }
        public bool schemaValidationRequired { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Targettrackedschemaproperty[] targetTrackedSchemaProperties { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public Flowpartnertags flowPartnerTags { get; set; }
        public Edioverridedelimiters ediOverrideDelimiters { get; set; }
    }

    public class obFlowPartnerDockey
    {
        public string partnerIdentifierValue { get; set; }
        public string obFlowContextId { get; set; }
        public string partnerDetailsId { get; set; }
    }
    public class Flowpartnertags
    {
        public string gs02 { get; set; }
        public string gs03 { get; set; }
    }
    public class Edioverridedelimiters
    {
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int dataElementSeparator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int componentSeparator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? releaseIndicator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? repetitionSeparator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int? replacementCharacter { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string decimalIndicator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public int segmentTerminator { get; set; }
        [JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
        public string segmentTerminatorSuffix { get; set; }
    }

    #endregion

    //schemaContext
    #region schema
    public class Schema
    {
        public string id { get; set; }
        public string recordType { get; set; }
        public SchemaDockey docKey { get; set; }
        public string description { get; set; }
        public Trackedschemaproperty[] trackedSchemaProperties { get; set; }
        public JArray contentBasedRoutingFilter { get; set; }
        public string fileName { get; set; }
    }
    public class SchemaDockey
    {
        public string @namespace { get; set; }
        public string rootnode { get; set; }
    }

    public class Trackedschemaproperty
    {
        public string name { get; set; }
        public string value { get; set; }
    }
    #endregion

    //transformContext
    #region Transform
    public class TransformContext
    {
        public string id { get; set; }
        public string recordType { get; set; }
        public TransformDockey docKey { get; set; }
        public string description { get; set; }
        public JArray preTransformProcessing { get; set; }
        public JArray postTransformProcessing { get; set; }
        public string fileName { get; set; }
        public Transformlookup transformLookup { get; set; }
    }
    public class TransformDockey
    {
        public string sourceSchemaId { get; set; }
        public string targetSchemaId { get; set; }
        public string version { get; set; }
    }
    public class Transformlookup
    {
    }
    #endregion

    //ibFlowContext
    #region ibFlowContext
    public class ibFlowContext
    {
        public string id { get; set; }
        public string recordType { get; set; }
        public ibFlowDockey docKey { get; set; }
        public string description { get; set; }
        public string businessUnit { get; set; }
        public string programName { get; set; }
        public string lobName { get; set; }
        public string transactionType { get; set; }
        public bool encodeRequired { get; set; }
        public string documentType { get; set; }
        public bool schemaValidationRequired { get; set; }
        public JArray postTransformProcessing { get; set; }
        public JArray postEncodeProcessing { get; set; }
        public string defaultTransformId { get; set; }
        public string epDetailId { get; set; }
    }

    public class ibFlowDockey
    {
        public string schemaId { get; set; }
        public string schemaXpathValue { get; set; }
    }
    #endregion

    //obFlowContext
    #region obFlowContext
    public class obFlowContext
    {
        public string id { get; set; }
        public string recordType { get; set; }
        public obFlowDockey docKey { get; set; }
        public string schemaId { get; set; }
        public string lobName { get; set; }
        public string transactionType { get; set; }
        public string description { get; set; }
        public string programName { get; set; }
        public string businessUnit { get; set; }
        public bool decodeRequired { get; set; }
        public bool schemaValidationRequired { get; set; }
        public JArray preDecodeProcessing { get; set; }
        public JArray postDecodeProcessing { get; set; }
        public string epDetailId { get; set; }
        public string defaultTransformId { get; set; }
    }
    public class obFlowDockey
    {
        public string docMsgType { get; set; }
        public string docType { get; set; }
        public string docFormat { get; set; }
    }
    #endregion
    //Sql Queries
    public class SqlQueries
    {
        public const string inboundQueryMapName = @"select distinct (select top 1 Name from bts_item a1 where m.itemid = a1.id) as mapName, d.msgtype as InMsg,(select top 1 Name from bts_item a1, bt_documentspec b1 where b1.itemid = a1.id and b1.docspec_name = m.indoc_docspec_name)  as InMsgItem,
        (select top 1 msgtype from bt_documentspec where docspec_name = m.outdoc_docspec_name) as OutMsg,
        (select top 1 Name from bts_item a2, bt_documentspec b2 where b2.itemid = a2.id and b2.docspec_name = m.outdoc_docspec_name)  as OutMsgItem
        from tpm.Partner p, tpm.BusinessProfile b, tpm.BusinessIdentity c, tpm.OnewayAgreement o, tpm.X12ProtocolSettings x,
        bt_DocumentSpec d, bt_MapSpec m, bts_item i where p.PartnerId = b.PartnerId and b.ProfileId = c.ProfileId and c.Id = o.SenderId and
        o.ProtocolSettingsId = x.SettingsId and x.TargetNamespace<> 'http://schemas.microsoft.com/BizTalk/EDI/X12/2006' and
        d.msgtype like CONCAT(x.TargetNamespace, '%') and d.docspec_name = m.indoc_docspec_name and p.Name = '{0}' and d.msgtype like '{1}%'";

        public const string outboundQueryMapName = @"
        Select distinct i.Name as MapName,
        (select top 1 b1.msgtype from bts_item a1, bt_documentspec b1 where b1.itemid = a1.id and b1.docspec_name = m.indoc_docspec_name) 
        as InMsgType, 
        (select top 1 Name from bts_item a1, bt_documentspec b1 where b1.itemid = a1.id and b1.docspec_name = m.indoc_docspec_name)  
        as InMsgItem,
        d.msgtype as OutMsgType,
        (select top 1 Name from bts_item a2, bt_documentspec b2 where b2.itemid = a2.id and b2.docspec_name = m.outdoc_docspec_name)  as OutMsgItem
        from tpm.BusinessProfile b, bt_MapSpec m, bts_item i, bt_documentspec d, tpm.x12Protocolsettings x where 
		m.itemid = i.id and m.outdoc_docspec_name = d.docspec_name and b.Name = '{0}_Profile' and d.msgtype like '{1}%'";
    }
}
