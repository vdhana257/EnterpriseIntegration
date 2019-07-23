namespace SchemaMigration
{
    using System.Collections.Generic;

    /// <summary>
    /// Enumeration to restrict the status value for a schema upload
    /// </summary>
    public enum SchemaUploadToIAStatus
    {
        NotYetStarted,
        Success,
        Failure, 
        Partial
    }

    /// <summary>
    /// Class to hold the details of a schema
    /// </summary>
    public class SchemaDetails
    {
        /// <summary>
        /// Gets or sets the naame of the schema
        /// </summary>
        public string schemaName { get; set; }

        /// <summary>
        /// Gets or sets the namespace of the schema
        /// </summary>
        public string schemaNamespace { get; set; }

        /// <summary>
        /// Gets or sets the full name of the schema i.e. namespace.name
        /// </summary>
        public string schemaFullName { get; set; }

        /// <summary>
        /// Gets or sets the version of the DLL
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name of the assembly 
        /// </summary>
        public string assemblyFullyQualifiedName { get; set; }

        /// <summary>
        /// Gets or sets the location where the schema file is downloaded on local
        /// </summary>
        public string schemaFilesLocation { get; set;}

        /// <summary>
        /// Gets or sets the boolean flag to test whether the schema has already been extracted from DB
        /// </summary>
        public bool isSchemaExtractedFromDb { get; set; }

        /// <summary>
        /// Gets or sets the location where the DLL file is stored in local (if not GAC'ed on the system where the migration tool runs)
        /// </summary>
        public string dllFilesLocation { get; set; }

        /// <summary>
        /// Gets or sets the status of the schema i.e. whether it has been uploaded to Integration Account successfully or not
        /// </summary>
        public SchemaUploadToIAStatus schemaUploadToIAStatus { get; set; }

        /// <summary>
        /// Gets or sets the error details about why schema upload failed to Integration account (in case, any)
        /// </summary>
        public string errorDetailsForExtraction { get; set; }

        public string errorDetailsForMigration { get; set; }

        /// <summary>
        /// Gets or sets the list of dependent schemas for a particular schema
        /// </summary>
        public List<string> dependentSchemas { get; set; }

        /// <summary>
        /// Gets or sets the full name of schema to be uploaded to IA to avoid conflicts of same names in IA
        /// </summary>
        public string fullNameOfSchemaToUpload { get; set; }
    }
}
