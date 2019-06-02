//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class AppConstants
    {
        public const string SelectedAgreementsContextPropertyName = "SelectedAgreements";

        public const string SelectedPartnersContextPropertyName = "SelectedPartners";

        public const string SelectedSchemasContextPropertyName = "SelectedSchemas";

        public const string AllAgreementsContextPropertyName = "AllAgreements";

        public const string AllPartnersContextPropertyName = "AllPartners";

        public const string AllSchemasContextPropertyName = "AllSchemas";

        public const string AS2ProtocolName = "as2";

        public const string X12ProtocolName = "x12";

        public const string EdifactProtocolName = "edifact";

        public const string TotalPartnersWithCertificatesRequired = "TotalPartnersWithCertificatesRequired";
        public const string SuccesfullPartnersCertificateMigrations = "SuccesfullPartnersCertificateMigrations";

        public const string SchemaNamespaceVersionList = "SchemaNamespaceVersionMapping";
        public const string ConsolidationEnabled = "IsConsolidationSelected";

        public const string ContextGenerationEnabled = "IsContextGenerationSelected";
        public const string partnerDetails = "PartnerDetails";
        public const string ibFlowPartnerContext = "ibFlowPartnerContext";
        public const string obFlowPartnerContext = "obFlowPartnerContext";
        public const string schemaXpathValue = "Default";
        public const string programName = "VL";
        public const string DefaultDocumentType = "Xml";


        /// <summary>
        /// The format string_ cosmos db uri.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Reviewed. Suppression is OK here.")]
        public const string FormatString_CosmosDbUri = "https://aisv2cosmosdb-ppe1.documents.azure.com:443/";

        /// <summary>
        /// The format string_ exception_ invalid cache initialization.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Reviewed. Suppression is OK here.")]
        public const string FormatString_Exception_InvalidCacheInitialization = "Metadata cache is alredy initialized with source type = {0}. Please unintialize to reinitialize with different cache source.";

        /// <summary>
        /// The format string_ exception_ cache refresh failure.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Reviewed. Suppression is OK here.")]
        public const string FormatString_Exception_CacheRefreshFailure = "Metadata Refresh failed. metadataSource={0}";

        /// <summary>
        /// The format string_ information_ cache refresh success.
        /// </summary>
        [SuppressMessage("StyleCop.CSharp.NamingRules", "SA1310:FieldNamesMustNotContainUnderscore", Justification = "Reviewed. Suppression is OK here.")]
        public const string FormatString_Information_CacheRefreshSuccess = "Refreshing metadata cache successful. metadataCacheRefreshTime={0}";
    }
}
