//-----------------------------------------------------------------------
// <copyright file="IntegrationAccountManagementEndPoint.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines wrapper methods for some of the functionalities provided by the Integration Account REST API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.AzureManagementEndPoint
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net;
    using Constants;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class which defines wrapper methods for some of the functionalities provided by the Integration Account REST API
    /// </summary>
    public class IntegrationAccountManagementEndPoint : AzureManagementEndpoint.AzureManagementEndPoint, IOMSIntegrationAccountTrackingEndPoint
    {
        /// <summary>
        /// The subscription ID required by the azure management endpoint 
        /// </summary>
        private string subscriptionId;

        /// <summary>
        /// The resource group name required by the azure management endpoint 
        /// </summary>
        private string resourceGroupName;

        /// <summary>
        /// The integration account name required by the azure management endpoint 
        /// </summary>
        private string integrationAccountName;

        /// <summary>
        /// The integration account tracking API version
        /// </summary>
        private string apiVersion;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationAccountManagementEndPoint"/> class
        /// </summary>
        /// <param name="authorizationCredentialType">Authorization credential type</param>
        /// <param name="clientId">The client identification</param>
        /// <param name="authorizationSecret">The application key or certificate thumbprint based on credential type</param>
        /// <param name="subscriptionId">The azure subscription Id</param>
        /// <param name="resourceGroupName">The azure resource group name</param>
        /// <param name="integrationAccountName">The azure integration account name</param>
        /// <param name="apiversion">The integration account tracking API version</param>
        public IntegrationAccountManagementEndPoint(CredentialType authorizationCredentialType, string clientId, string authorizationSecret, string subscriptionId, string resourceGroupName, string integrationAccountName, string apiversion = Constants.IntegrationAccountTrackingApiConstants.DefaultApiVersion)
        {
            // Initialize the properties of the base Azure Management class
            this.GatewayEndPoint = AzureActiveDirectoryConstants.AzureManagementAPIEndPoint;
            this.AuthorityEndPoint = AzureActiveDirectoryConstants.MicrosoftAuthorizationEndPoint;
            this.CurrentAuthorizationCredentialtype = authorizationCredentialType;
            this.ClientId = clientId;
            this.AuthorizationSecret = authorizationSecret;

            // Initlaize the Integration Account Tracking API End Point
            this.SubscriptionId = subscriptionId;
            this.ResourceGroupName = resourceGroupName;
            this.IntegrationAccountName = integrationAccountName;
            this.ApiVersion = apiversion;
        }

        /// <summary>
        /// Gets the subscription ID
        /// </summary>
        public string SubscriptionId
        {
            get
            {
                return this.subscriptionId;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.SubscriptionId));
                this.subscriptionId = value;
            }
        }

        /// <summary>
        /// Gets the resource group name
        /// </summary>
        public string ResourceGroupName
        {
            get
            {
                return this.resourceGroupName;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.ResourceGroupName));
                this.resourceGroupName = value;
            }
        }

        /// <summary>
        /// Gets the integration account name
        /// </summary>
        public string IntegrationAccountName
        {
            get
            {
                return this.integrationAccountName;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.IntegrationAccountName));
                this.integrationAccountName = value;
            }
        }

        /// <summary>
        /// Gets the integration account API version
        /// </summary>
        public string ApiVersion
        {
            get
            {
                return this.apiVersion;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.ApiVersion));
                this.apiVersion = value;
            }
        }

        /// <summary>
        /// Post the custom tracking events to the OMS using Integration Account Tracking API
        /// </summary>
        /// <param name="eventSource">The event source</param>
        /// <param name="correlationId">The correlation client tracking ID</param>
        /// <param name="eventLevel">The event severity level</param>
        /// <param name="eventDateTime">The event date time</param>
        /// <param name="customSourceInformation">The custom event source information</param>
        /// <param name="customEventTrackingProperties">The custom tracking event properties to be posted to OMS</param>
        public void PostCustomTrackingEvents(string eventSource, string correlationId, string eventLevel, string eventDateTime, IDictionary<string, object> customSourceInformation, IDictionary<string, object> customEventTrackingProperties)
        {
            var apiUrl = $"https://management.azure.com/subscriptions/{this.SubscriptionId}/resourceGroups/{this.ResourceGroupName}/providers/Microsoft.Logic/integrationAccounts/{this.IntegrationAccountName}/logTrackingEvents?api-version={this.ApiVersion}";

            // Initialize the default JSON merge settings
            var defaultJsonMergeSettings = new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            };

            // Initialize the JSON payload
            var contentJObject = new JObject
            {
                { IntegrationAccountTrackingApiConstants.SourceType, IntegrationAccountTrackingApiConstants.Custom }
            };

            // Populate the source details
            var sourceJObject = new JObject
            {
                { IntegrationAccountTrackingApiConstants.EventSourceName, eventSource }
            };

            // Add run instance information to it
            var runInstanceJObject = new JProperty(IntegrationAccountTrackingApiConstants.RunInstance, new JObject(new JProperty(IntegrationAccountTrackingApiConstants.OmsCorrelationTrackingId, correlationId)));
            sourceJObject.Add(runInstanceJObject);

            // Add custom source properties to it
            var customSourceJObject = JObject.FromObject(customSourceInformation);
            sourceJObject.Merge(customSourceJObject, defaultJsonMergeSettings);

            // Add the source details to the JSON payload
            contentJObject.Add(new JProperty(IntegrationAccountTrackingApiConstants.Source, sourceJObject));

            // Populate the event details
            var eventJObject = new JObject()
            {
                { IntegrationAccountTrackingApiConstants.EventLevel, eventLevel },
                { IntegrationAccountTrackingApiConstants.EventTime, eventDateTime },
                { IntegrationAccountTrackingApiConstants.RecordType, IntegrationAccountTrackingApiConstants.Custom }
            };

            // Add custom event properties
            var customEventJObject = JObject.FromObject(customEventTrackingProperties);
            eventJObject.Add(new JProperty(IntegrationAccountTrackingApiConstants.Record, customEventJObject));

            // Add the event details to the JSON payload
            contentJObject.Add(
                new JProperty(
                    IntegrationAccountTrackingApiConstants.Events, 
                    new JArray()
                    {
                        eventJObject
                    }));

            var jsonPayload = contentJObject.ToString();
            using (var client = new WebClient())
            {
                // Get the bearer token
                var azureAdBearerToken = this.GetAzureActiveDirectoryAuthorizationToken();

                // Configure the headers
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add("Authorization", "Bearer " + azureAdBearerToken);

                // Upload the JSON payload
                client.UploadString(new Uri(apiUrl), "POST", jsonPayload);
            }
        }
    }
}
