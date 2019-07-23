//-----------------------------------------------------------------------
// <copyright file="CredentialType.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Enumeration for determining azure active directory credential type</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.Constants
{
    /// <summary>
    /// Enumeration for determining azure active directory credential type
    /// </summary>
    public enum CredentialType
    {
        /// <summary>
        /// Certificate based authentication
        /// </summary>
        Certificate,

        /// <summary>
        /// Password based authentication
        /// </summary>
        Password
    }
}
