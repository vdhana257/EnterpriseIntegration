//-----------------------------------------------------------------------
// <copyright file="CertificateHelper.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines utility methods for accessing the uploaded certificates</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.AzureManagementEndPoint.Helpers
{
    using System.Diagnostics.Contracts;
    using System.Security.Cryptography.X509Certificates;

    /// <summary>
    /// Class which defines utility methods for accessing the uploaded certificates
    /// </summary>
    public class CertificateHelper
    {
        /// <summary>
        /// Fetches the certificate by thumbprint
        /// </summary>
        /// <param name="certificateThumbprint">The certificate thumbprint</param>
        /// <returns>the certificate</returns>
        public static X509Certificate2 GetCertificateByThumbprint(string certificateThumbprint)
        {
            // Configure certificate retrieval
            X509Store certificateStore = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            certificateStore.Open(OpenFlags.ReadOnly);

            X509Certificate2Collection foundCertificateCollection = certificateStore.Certificates.Find(
                X509FindType.FindByThumbprint,
                certificateThumbprint,
                false);

            // Assert that the certificate exists
            Contract.Assume(foundCertificateCollection.Count > 0, $"No certifcate found with {nameof(certificateThumbprint)} : {certificateThumbprint}");

            // Get the first cert with the thumbprint
           var certificate = foundCertificateCollection[0];

            certificateStore.Close();

            return certificate;
        }
    }
}
