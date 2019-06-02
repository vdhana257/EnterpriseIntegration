using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Configuration;
using System.Globalization;
using Microsoft.Azure.Documents;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.Azure.Documents.Client;


namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    
    public enum MetadataSourceType
    {
        /// <summary>
        /// The cosmos sql db.
        /// </summary>
        CosmosSqlDb,

        /// <summary>
        /// The file.
        /// </summary>
        File
    }
    /// <summary>
    /// Metadata record types
    /// </summary>
    public enum MetadataRecordTypes
    {
        /// <summary>
        /// The schema.
        /// </summary>
        Schema,

        /// <summary>
        /// The transform.
        /// </summary>
        Transform,

        /// <summary>
        /// The custom processing function.
        /// </summary>
        CustomProcessingFunction,

        /// <summary>
        /// The partner details.
        /// </summary>
        PartnerDetails,

        /// <summary>
        /// The end point details.
        /// </summary>
        EndPointDetails,

        /// <summary>
        /// The ib flow context.
        /// </summary>
        ibFlowContext,

        /// <summary>
        /// The ib flow partner context.
        /// </summary>
        ibFlowPartnerContext,

        /// <summary>
        /// The ob flow context.
        /// </summary>
        obFlowContext,

        /// <summary>
        /// The ob flow partner context.
        /// </summary>
        obFlowPartnerContext
    }

    /// <summary>
    /// The MetadataSource interface.
    /// </summary>
    public interface IMetadataSource
    {
        /// <summary>
        /// Gets the source type.
        /// </summary>
        MetadataSourceType SourceType { get; }

        /// <summary>
        /// The query all documents.
        /// </summary>
        /// <returns>
        /// The <see cref="MetadataQueryResponse"/>.
        /// </returns>
        MetadataQueryResponse QueryAllDocuments();
    }

    /// <summary>
    /// The metadata query response.
    /// </summary>
    public class MetadataQueryResponse
    {
        /// <summary>
        /// The metadata dictionary.
        /// </summary>
        private Dictionary<string, JObject> metadataDictionary;

        /// <summary>
        /// The query time stamp.
        /// </summary>
        private string queryTimeStamp;

        /// <summary>
        /// The source type.
        /// </summary>
        private MetadataSourceType sourceType;

        /// <summary>
        /// Gets or sets the metadata dictionary.
        /// </summary>
        public Dictionary<string, JObject> MetadataDictionary
        {
            get
            {
                return this.metadataDictionary;
            }

            set
            {
                this.metadataDictionary = value;
            }
        }

        /// <summary>
        /// Gets or sets the query time stamp.
        /// </summary>
        public string QueryTimeStamp
        {
            get
            {
                return this.queryTimeStamp;
            }

            set
            {
                this.queryTimeStamp = value;
            }
        }

        /// <summary>
        /// Gets or sets the source type.
        /// </summary>
        public MetadataSourceType SourceType
        {
            get
            {
                return this.sourceType;
            }

            set
            {
                this.sourceType = value;
            }
        }
    }

    public class DocumentDbClass
    {
        public static Dictionary<string, JObject> metadataDictionary = null;
        private object clientLock = new Object();
        private DocumentClient client = null;
        private Uri collectionUri;
        private DocumentClient Client
        {
            get
            {
                lock (clientLock)
                {
                    if (null == client)
                    {
                        string CosmosSqlDbAccountName = ConfigurationManager.AppSettings["CosmosSqlDbAccountName"];
                        string CosmosSqlDbPrimaryKey = ConfigurationManager.AppSettings["CosmosSqlDbPrimaryKey"];
                        client = new DocumentClient(
    new Uri(
         string.Format(CultureInfo.InvariantCulture, AppConstants.FormatString_CosmosDbUri, CosmosSqlDbAccountName)),
    CosmosSqlDbPrimaryKey,
    new ConnectionPolicy
    {
        ConnectionMode = ConnectionMode.Direct,
        ConnectionProtocol = Protocol.Tcp
    });
                    }
                    string CosmosSqlDbName = ConfigurationManager.AppSettings["CosmosSqlDbName"];
                    string CosmosSqlDbCollectionName = ConfigurationManager.AppSettings["CosmosSqlDbCollectionName"];
                    collectionUri = UriFactory.CreateDocumentCollectionUri(CosmosSqlDbName, CosmosSqlDbCollectionName);
                    return client;
                }

            }
        }
        public MetadataQueryResponse QueryAllDocuments()
        {
            lock (clientLock)
            {
                IQueryable<JObject> queryResults = Client.CreateDocumentQuery<JObject>(collectionUri);
                metadataDictionary = queryResults.ToDictionary(k => k.Value<string>("id"), k => k);
            }

            string metadataCacheRefreshTime =
                DateTime.UtcNow.ToString(
                    CultureInfo.InvariantCulture.DateTimeFormat.FullDateTimePattern,
                    CultureInfo.InvariantCulture);
            return new MetadataQueryResponse
            {
                MetadataDictionary = metadataDictionary,
                QueryTimeStamp = metadataCacheRefreshTime,
                SourceType = MetadataSourceType.CosmosSqlDb
            };
        }
    }
}
