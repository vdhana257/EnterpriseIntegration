namespace MapMigration
{
    using System.Collections.Generic;

    /// <summary>
    /// Enumeration to restrict the status value for a map upload
    /// </summary>
    public enum MapUploadToIAStatus
    {
        NotYetStarted,
        Success,
        Failure,
        Partial
    }

    /// <summary>
    /// Class to hold the details of a map
    /// </summary>
    public class MapDetails
    {
        /// <summary>
        /// Gets or sets the naame of the map
        /// </summary>
        public string mapName { get; set; }

        /// <summary>
        /// Gets or sets the namespace of the map
        /// </summary>
        public string mapNamespace { get; set; }

        /// <summary>
        /// Gets or sets the full name of the map i.e. namespace.name
        /// </summary>
        public string mapFullName { get; set; }

        /// <summary>
        /// Gets or sets the version of the DLL
        /// </summary>
        public string version { get; set; }

        /// <summary>
        /// Gets or sets the fully qualified name of the assembly 
        /// </summary>
        public string assemblyFullyQualifiedName { get; set; }

        /// <summary>
        /// Gets or sets the location where the map file is downloaded on local
        /// </summary>
        public string mapFilesLocation { get; set; }

        /// <summary>
        /// Gets or sets the boolean flag to test whether the map has already been extracted from DB
        /// </summary>
        public bool isMapExtractedFromDb { get; set; }

        /// <summary>
        /// Gets or sets the location where the DLL file is stored in local (if not GAC'ed on the system where the migration tool runs)
        /// </summary>
        public string dllFilesLocation { get; set; }

        /// <summary>
        /// Gets or sets the status of the map i.e. whether it has been uploaded to Integration Account successfully or not
        /// </summary>
        public MapUploadToIAStatus mapUploadToIAStatus { get; set; }

        /// <summary>
        /// Gets or sets the error details about why map upload failed to Integration account (in case, any)
        /// </summary>
        public string errorDetailsForExtraction { get; set; }

        public string errorDetailsForMigration { get; set; }

        /// <summary>
        /// Gets or sets the list of dependent schemas for a particular schema
        /// </summary>
        ///public List<string> dependentSchemas { get; set; }

        /// <summary>
        /// Gets or sets the full name of map to be uploaded to IA to avoid conflicts of same names in IA
        /// </summary>
        public string fullNameOfMapToUpload { get; set; }
    }
}
