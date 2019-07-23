//-----------------------------------------------------------------------
// <copyright file="SeverityLevel.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Enumeration which defines the severity level of the record.</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.Constants
{
    /// <summary>
    /// Enumeration which defines the severity level of the record.
    /// </summary>
    public enum SeverityLevel
    {
        /// <summary>
        /// Severity level: LogAlways
        /// </summary>
        LogAlways,

        /// <summary>
        /// Severity level: Critical
        /// </summary>
        Critical,

        /// <summary>
        /// Severity level: Error
        /// </summary>
        Error,

        /// <summary>
        /// Severity level: Warning
        /// </summary>
        Warning,

        /// <summary>
        /// Severity level: Informational
        /// </summary>
        Informational,

        /// <summary>
        /// Severity level: Verbose
        /// </summary>
        Verbose,
    }
}
