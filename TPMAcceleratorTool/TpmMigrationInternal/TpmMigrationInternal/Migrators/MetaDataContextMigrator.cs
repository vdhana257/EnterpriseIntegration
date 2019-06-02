using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.IO;
using Server = Microsoft.BizTalk.B2B.PartnerManagement;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MetaDataContextMigrator
    {
        public static string inboundPartnerDetails = string.Empty;
        public static string outboundPartnerDetails = string.Empty;
        public string CreateNewGuid()
        {
            string newGuid = string.Empty;
            bool success = false;
            while (!success)
            {
                newGuid = System.Guid.NewGuid().ToString();
                var queryResults = (from r in DocumentDbClass.metadataDictionary
                                    where (r.Value.Value<string>("id") == newGuid
                                          )
                                    select r.Value).ToArray();
                if (queryResults.Count() == 0)
                {
                    success = true;
                }
                else
                {
                    success = false;
                }
            }
            return newGuid;
        }
        public void CreatePartnerFiles(string partnerName, AgreementMigrationItemViewModel agreementItem)
        {
            string directroyPathForJsonFiles = Resources.MetaDataContextPath;
            string fileName = string.Format(directroyPathForJsonFiles, partnerName);
            if (!Directory.Exists(fileName))
            {
                FileOperations.CreateFolder(fileName);
            }
            else
            {
                Directory.Delete(fileName, true);
                FileOperations.CreateFolder(fileName);
            }
            // Inbound Partner Details
            string newGuid = CreateNewGuid();
            PartnerContext partnrCtxt = new PartnerContext();
            partnrCtxt.id = newGuid;
            inboundPartnerDetails = newGuid;
            partnrCtxt.recordType = AppConstants.partnerDetails;
            partnrCtxt.description = "EDI AS2 Partner Details For " + partnerName;
            partnrCtxt.docKey = new Dockey()
            {
                messageFormat = agreementItem.MigrationEntity.Protocol.ToUpper(),
                senderQualifier = agreementItem.GuestPartnerQualifer,
                senderIdentifier = agreementItem.GuestPartnerId,
                receiverQualifier = agreementItem.HostPartnerQualifier,
                receiverIdentifier = agreementItem.HostPartnerId
            };
            partnrCtxt.as2Config = new As2config()
            {
                as2From = "NA",
                as2To = "NA"
            };
            partnrCtxt.preserveInterchange = new Preserveinterchange()
            {
                IB = true,
                PT = true
            };
            partnrCtxt.preDecodeProcessing = new JArray();
            partnrCtxt.postDecodeProcessing = new JArray();
            partnrCtxt.preEncodeProcessing = new PreEncodeprocessing[0];
            partnrCtxt.postEncodeProcessing = new PostEncodeprocessing[0];
            partnrCtxt.epDetailId = CreateNewGuid();
            fileName = fileName + partnerName + "_InboundPartnerDetails.json";
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(partnrCtxt)).ToString());
            }

            // Outbound Partner Details
            newGuid = CreateNewGuid();
            PartnerContext obpartnrCtxt = new PartnerContext();
            obpartnrCtxt.id = newGuid;
            outboundPartnerDetails = newGuid;
            obpartnrCtxt.recordType = AppConstants.partnerDetails;
            obpartnrCtxt.description = "EDI AS2 Partner Details For " + partnerName;
            obpartnrCtxt.docKey = new Dockey()
            {
                messageFormat = agreementItem.MigrationEntity.Protocol.ToUpper(),
                senderQualifier = agreementItem.HostPartnerQualifier,
                senderIdentifier = agreementItem.HostPartnerId,
                receiverQualifier = agreementItem.GuestPartnerQualifer,
                receiverIdentifier = agreementItem.GuestPartnerId
            };
            obpartnrCtxt.as2Config = new As2config()
            {
                as2From = "NA",
                as2To = "NA"
            };
            obpartnrCtxt.preDecodeProcessing = new JArray();
            obpartnrCtxt.postDecodeProcessing = new JArray();
            obpartnrCtxt.preEncodeProcessing = new PreEncodeprocessing[0];
            obpartnrCtxt.postEncodeProcessing = new PostEncodeprocessing[0];
            obpartnrCtxt.epDetailId = CreateNewGuid();
            fileName = string.Empty;
            fileName = string.Format(directroyPathForJsonFiles, partnerName);
            fileName = fileName + partnerName + "_OutboundPartnerDetails.json";
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(obpartnrCtxt)).ToString());
            }
        }
        public void CreateibContextFiles(string partnerName, AgreementMigrationItemViewModel agreementItem, string sourceSchemaId, string ibFlowId, string transformId, string txnType)
        {
            ibFlowPartnerContext ibctxt = new ibFlowPartnerContext();
            ibctxt.id = CreateNewGuid();
            ibctxt.recordType = AppConstants.ibFlowPartnerContext;
            ibctxt.description = agreementItem.Protocol + " " + txnType + " Message from " + partnerName;
            ibctxt.docKey = new ibFlowPartnerDockey()
            {
                schemaXpathValue = AppConstants.schemaXpathValue,
                schemaId = sourceSchemaId,
                partnerDetailId = inboundPartnerDetails
            };
            ibctxt.ibFlowContextId = ibFlowId;
            ibctxt.transformId = transformId;
            ibctxt.preTransformProcessing = new JArray();
            ibctxt.schemaValidationRequired = true;
            string fileName = string.Format(Resources.MetaDataContextPath, partnerName);
            fileName = fileName + partnerName + "_ibFlowPartnerContext_" + txnType + ".json";
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(ibctxt).ToString()).ToString());
            }
            else
            {
                File.Delete(fileName);
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(ibctxt).ToString()).ToString());
            }
        }
        public void CreateobX12ContextFiles(string partnerName, AgreementMigrationItemViewModel agreementItem, Server.X12ProtocolSettings protocolSettingsBA, string targetSchemaId, string obFlowId, string transformId, string documentKey, string txnType)
        {
            obFlowPartnerContext obCtxt = new obFlowPartnerContext();
            obCtxt.id = CreateNewGuid();
            obCtxt.recordType = AppConstants.obFlowPartnerContext;
            obCtxt.description = agreementItem.Protocol + " " + txnType + " Message from " + partnerName;
            obCtxt.schemaId = targetSchemaId;
            obCtxt.transformId = transformId;
            obCtxt.docKey = new obFlowPartnerDockey()
            {
                obFlowContextId = obFlowId,
                partnerDetailsId = outboundPartnerDetails,
                partnerIdentifierValue = txnType
            };
            obCtxt.documentKey = documentKey;
            obCtxt.documentType = AppConstants.DefaultDocumentType;
            obCtxt.preTransformProcessing = new JArray();
            obCtxt.postTransformProcessing = new JArray();
            obCtxt.schemaValidationRequired = true;
            obCtxt.ediOverrideDelimiters = new Edioverridedelimiters()
            {
                dataElementSeparator = protocolSettingsBA.FramingSettings.DataElementSeparator,
                componentSeparator = protocolSettingsBA.FramingSettings.ComponentSeparator,
                replacementCharacter = protocolSettingsBA.FramingSettings.ReplaceChar,
                segmentTerminator = protocolSettingsBA.FramingSettings.SegmentTerminator,
                segmentTerminatorSuffix = protocolSettingsBA.FramingSettings.SegmentTerminatorSuffix.ToString()
            };
            string fileName = string.Format(Resources.MetaDataContextPath, partnerName);
            fileName = fileName + partnerName + "_obFlowPartnerContext_" + txnType + ".json";
            if (!File.Exists(fileName))
            {
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(obCtxt).ToString()).ToString());
            }
            else
            {
                File.Delete(fileName);
                File.Create(fileName).Close();
                System.IO.File.WriteAllText(fileName, JObject.Parse(JsonConvert.SerializeObject(obCtxt).ToString()).ToString());
            }
        }
        public string QuerySchema(string partnerName, string nameSpace, string rootNode, string fileName, MetadataRecordTypes recordType)
        {
            string schemaGuid = string.Empty;
            try
            {
                string recordTypeString = Enum.GetName(typeof(MetadataRecordTypes), recordType);
                // Get the Source Schema Details
                var queryResults = (from r in DocumentDbClass.metadataDictionary
                                    where (r.Value.Value<string>("recordType") == recordTypeString
                                           && r.Value["docKey"].Value<string>("namespace") == nameSpace
                                           && r.Value["docKey"].Value<string>("rootnode").Contains(rootNode))
                                    select r.Value).ToArray();
                if (queryResults.Count() > 0)
                {
                    schemaGuid = queryResults[0].Value<string>("id").ToString();
                }
                else
                {
                    queryResults = (from r in DocumentDbClass.metadataDictionary
                                    where (r.Value.Value<string>("recordType") == recordTypeString
                                           && r.Value.Value<string>("fileName") == fileName)
                                    select r.Value).ToArray();
                    if (queryResults.Count() > 0)
                        schemaGuid = queryResults[0].Value<string>("id").ToString();
                    else
                    {
                        schemaGuid = GenerateSchemaJson(nameSpace, rootNode, fileName, partnerName);
                    }
                }
            }
            catch (Exception)
            {
                // Do Nothing
            }
            return schemaGuid;
        }
        private string GenerateSchemaJson(string nameSpace, string rootNode, string fileName, string partnerName)
        {
            Schema schemaCtxt = new Schema();
            schemaCtxt.id = CreateNewGuid();
            schemaCtxt.recordType = MetadataRecordTypes.Schema.ToString();
            schemaCtxt.description = rootNode + "_Schema";
            schemaCtxt.docKey = new SchemaDockey()
            {
                @namespace = nameSpace,
                rootnode = rootNode
            };
            schemaCtxt.contentBasedRoutingFilter = new JArray();
            schemaCtxt.trackedSchemaProperties = new Trackedschemaproperty[0];
            schemaCtxt.fileName = fileName;
            string saveFileName = string.Format(Resources.MetaDataContextPath, partnerName);
            saveFileName = saveFileName + "Schema_" + rootNode + ".json";
            if (!File.Exists(saveFileName))
            {
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(schemaCtxt).ToString()).ToString());
            }
            else
            {
                File.Delete(saveFileName);
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(schemaCtxt).ToString()).ToString());
            }
            return schemaCtxt.id;
        }
        public string QueryTransform(string partnerName, string sourceGuid, string targetGuid, string mapName, MetadataRecordTypes recordType)
        {
            string transformGuid = string.Empty;
            try
            {
                string recordTypeString = Enum.GetName(typeof(MetadataRecordTypes), recordType);
                // Get the Source Schema Details
                var queryResults = (from r in DocumentDbClass.metadataDictionary
                                    where (r.Value.Value<string>("recordType") == recordTypeString
                                           && r.Value["docKey"].Value<string>("sourceSchemaId") == sourceGuid
                                           && r.Value["docKey"].Value<string>("targetSchemaId") == targetGuid)
                                    select r.Value).ToArray();
                if (queryResults.Count() > 0)
                    transformGuid = queryResults[0].Value<string>("id").ToString();
                else
                    transformGuid = GenerateTransformJson(partnerName, sourceGuid, targetGuid, mapName);
            }
            catch (Exception)
            {
                // Do Nothing
            }
            return transformGuid;
        }
        private string GenerateTransformJson(string partnerName, string sourceGuid, string targetGuid, string mapName)
        {
            TransformContext transform = new TransformContext();
            transform.id = CreateNewGuid();
            transform.recordType = MetadataRecordTypes.Transform.ToString();
            transform.description = mapName;
            transform.docKey = new TransformDockey()
            {
                sourceSchemaId = sourceGuid,
                targetSchemaId = targetGuid,
                version = "1.0"
            };
            transform.transformLookup = new Transformlookup() { };
            transform.preTransformProcessing = new JArray();
            transform.postTransformProcessing = new JArray();
            transform.fileName = mapName;
            string saveFileName = string.Format(Resources.MetaDataContextPath, partnerName);
            saveFileName = saveFileName + "Transform_" + mapName + ".json";
            if (!File.Exists(saveFileName))
            {
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(transform).ToString()).ToString());
            }
            else
            {
                File.Delete(saveFileName);
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(transform).ToString()).ToString());
            }
            return transform.id;
        }
        public string QueryibFlowContext(string partnerName, string targetGuid, string schemaXpath, string txnType, string transformId, MetadataRecordTypes recordType)
        {
            string ibFlowGuid = string.Empty;
            try
            {
                string recordTypeString = Enum.GetName(typeof(MetadataRecordTypes), recordType);
                // Get the Source Schema Details
                var queryResults = (from r in DocumentDbClass.metadataDictionary
                                    where (r.Value.Value<string>("recordType") == recordTypeString
                                           && r.Value["docKey"].Value<string>("schemaId") == targetGuid
                                           && r.Value["docKey"].Value<string>("schemaXpathValue") == schemaXpath)
                                    select r.Value).ToArray();
                if (queryResults.Length > 0)
                    ibFlowGuid = queryResults[0].Value<string>("id").ToString();
                else
                    ibFlowGuid = GenerateibFlowContext(partnerName, targetGuid, schemaXpath, txnType, transformId);
            }
            catch (Exception)
            {
                // Do Nothing
            }
            return ibFlowGuid;
        }
        private string GenerateibFlowContext(string partnerName, string targetGuid, string schemaXpath, string txnType, string transformId)
        {
            ibFlowContext ibFlow = new ibFlowContext();
            ibFlow.id = CreateNewGuid();
            ibFlow.recordType = MetadataRecordTypes.ibFlowContext.ToString();
            ibFlow.docKey = new ibFlowDockey()
            {
                schemaId = targetGuid,
                schemaXpathValue = schemaXpath
            };
            ibFlow.description = "";
            ibFlow.businessUnit = "BusinessUnit";
            ibFlow.programName = "ProgramName";
            ibFlow.lobName = "LobName";
            ibFlow.transactionType = txnType;
            ibFlow.encodeRequired = false;
            ibFlow.documentType = "XML";
            ibFlow.schemaValidationRequired = true;
            ibFlow.postEncodeProcessing = new JArray();
            ibFlow.postTransformProcessing = new JArray();
            ibFlow.defaultTransformId = transformId;
            ibFlow.epDetailId = CreateNewGuid();
            string saveFileName = string.Format(Resources.MetaDataContextPath, partnerName);
            saveFileName = saveFileName + "ibFlowContext_" + txnType + ".json";
            if (!File.Exists(saveFileName))
            {
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(ibFlow).ToString()).ToString());
            }
            else
            {
                File.Delete(saveFileName);
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(ibFlow).ToString()).ToString());
            }
            return ibFlow.id;
        }
        public string QueryobFlowContext(string partnerName, string sourceGuid, string docType, string txnType, string transformId, MetadataRecordTypes recordType)
        {
            string obFlowGuid = string.Empty;
            string recordTypeString = Enum.GetName(typeof(MetadataRecordTypes), recordType);
            // Get the Source Schema Details
            var queryResults = (from r in DocumentDbClass.metadataDictionary
                                where (r.Value.Value<string>("recordType") == recordTypeString
                                       && r.Value.Value<string>("schemaId") == sourceGuid
                                       && r.Value["docKey"].Value<string>("docType") == docType)
                                select r.Value).ToArray();
            if (queryResults.Length > 0)
                obFlowGuid = queryResults[0].Value<string>("id").ToString();
            else
                obFlowGuid = GenerateObFlowContext(partnerName, sourceGuid, docType, txnType, transformId);
            return obFlowGuid;
        }
        private string GenerateObFlowContext(string partnerName, string sourceGuid, string docType, string txnType, string transformId)
        {
            obFlowContext obFlow = new obFlowContext();
            obFlow.id = CreateNewGuid();
            obFlow.recordType = MetadataRecordTypes.obFlowContext.ToString();
            obFlow.docKey = new obFlowDockey()
            {
                docMsgType = "Default",
                docType = docType,
                docFormat = "XML"
            };
            obFlow.schemaId = sourceGuid;
            obFlow.lobName = "LobName";
            obFlow.transactionType = txnType;
            obFlow.description = "";
            obFlow.programName = "ProgramName";
            obFlow.businessUnit = "BusinessUnit";
            obFlow.decodeRequired = false;
            obFlow.schemaValidationRequired = false;
            obFlow.preDecodeProcessing = new JArray();
            obFlow.postDecodeProcessing = new JArray();
            obFlow.defaultTransformId = transformId;
            obFlow.epDetailId = CreateNewGuid();
            string saveFileName = string.Format(Resources.MetaDataContextPath, partnerName);
            saveFileName = saveFileName + "obFlowContext_" + txnType + ".json";
            if (!File.Exists(saveFileName))
            {
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(obFlow).ToString()).ToString());
            }
            else
            {
                File.Delete(saveFileName);
                File.Create(saveFileName).Close();
                System.IO.File.WriteAllText(saveFileName, JObject.Parse(JsonConvert.SerializeObject(obFlow).ToString()).ToString());
            }
            return obFlow.id;
        }
    }
}
