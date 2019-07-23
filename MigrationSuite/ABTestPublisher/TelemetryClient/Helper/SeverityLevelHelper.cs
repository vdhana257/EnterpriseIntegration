//-----------------------------------------------------------------------
// <copyright file="SeverityLevelHelper.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines helpers for managing the severity level</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.Helper
{
    using Constants;

    /// <summary>
    /// Class which defines helpers for managing the severity level
    /// </summary>
    public class SeverityLevelHelper
    {
        /// <summary>
        /// Method to check whether the current severity level of the information is allowed to be logged by the OMS
        /// </summary>
        /// <param name="currentSeverityLevel">The current severity level of your event</param>
        /// <param name="allowedSeverityLevel">The allowed severity level vy the application</param>
        /// <returns>Whether the event are supposed to be logged or not based on severity level</returns>
        public static bool IsAllowedForLogging(SeverityLevel currentSeverityLevel, SeverityLevel allowedSeverityLevel)
        {
            return currentSeverityLevel <= allowedSeverityLevel;
        }
    }
}
