//-----------------------------------------------------------------------
// <copyright file="AzureManagementEndPoint.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Abstract Class which defines wrapper methods for some of the functionalities provided by the Azure Management REST API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.AzureManagementEndpoint
{
    using System;
    using System.Diagnostics.Contracts;
    using Constants;
    using IdentityModel.Clients.ActiveDirectory;
    using TelemetryClient.IntegrationAccountTelemetryClient.AzureManagementEndPoint.Helpers;

    /// <summary>
    /// Abstract Class which defines wrapper methods for some of the functionalities provided by the Azure Management REST API
    /// </summary>
    public abstract class AzureManagementEndPoint
    {
        /// <summary>
        /// The authority end point
        /// </summary>
        private string authorityEndPoint;

        /// <summary>
        /// The azure management gateway end point
        /// </summary>
        private string gatewayEndPoint;

        /// <summary>
        /// The client Id of the application is used by the application to uniquely identify itself to Azure AD
        /// </summary>
        private string clientId;

        /// <summary>
        /// The authorization secret is a credential used to authenticate the application to Azure AD (either application key or certificate thumbprint)
        /// </summary>
        private string authorizationSecret;

        /// <summary>
        /// Gets or sets the authority end point
        /// </summary>
        protected string AuthorityEndPoint
        {
            get
            {
                return this.authorityEndPoint;
            }

            set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.AuthorityEndPoint));
                this.authorityEndPoint = value;
            }
        }

        /// <summary>
        /// Gets or sets the gateway end point
        /// </summary>
        protected string GatewayEndPoint
        {
            get
            {
                return this.gatewayEndPoint;
            }

            set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.GatewayEndPoint));
                this.gatewayEndPoint = value;
            }
        }

        /// <summary>
        /// Gets or sets the client Id
        /// </summary>
        protected string ClientId
        {
            get
            {
                return this.clientId;
            }

            set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.ClientId));
                this.clientId = value;
            }
        }

        /// <summary>
        /// Gets or sets the authorization secret used to authenticate the application using azure active directory (either by application key or certificate thumbprint)
        /// </summary>
        protected string AuthorizationSecret
        {
            get
            {
                return this.authorizationSecret;
            }

            set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.AuthorizationSecret));
                this.authorizationSecret = value;
            }
        }

        /// <summary>
        /// Gets or sets the current authorization credential type
        /// </summary>
        protected CredentialType CurrentAuthorizationCredentialtype { private get; set; }

        /// <summary>
        /// Get the bearer token
        /// </summary>
        /// <returns>The bearer token</returns>
        protected string GetAzureActiveDirectoryAuthorizationToken()
        {
            // Configure AAD
            var authorizationContext = new AuthenticationContext(this.AuthorityEndPoint);

            // Find the access token based on current authentication type
            AuthenticationResult authenticationResult;

            switch (this.CurrentAuthorizationCredentialtype)
            {
                case CredentialType.Certificate:
                    var clientAssertioncertificate = new ClientAssertionCertificate(this.clientId, CertificateHelper.GetCertificateByThumbprint(this.AuthorizationSecret));
                    authenticationResult = authorizationContext.AcquireTokenAsync(this.GatewayEndPoint, clientAssertioncertificate).GetAwaiter().GetResult();
                    break;
                case CredentialType.Password:
                    var clientCredential = new ClientCredential(this.ClientId, this.AuthorizationSecret);
                    authenticationResult = authorizationContext.AcquireTokenAsync(this.GatewayEndPoint, clientCredential).GetAwaiter().GetResult();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(this.CurrentAuthorizationCredentialtype), this.CurrentAuthorizationCredentialtype, null);
            }

            // Get the access/bearer token from the result
            return authenticationResult.AccessToken;
        }
    }
}
