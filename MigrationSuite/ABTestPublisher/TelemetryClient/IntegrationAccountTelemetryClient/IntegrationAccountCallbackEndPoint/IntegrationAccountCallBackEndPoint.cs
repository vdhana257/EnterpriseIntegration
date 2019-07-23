//-----------------------------------------------------------------------
// <copyright file="IntegrationAccountCallbackEndPoint.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines wrapper methods for some of the functionalities provided by the Integration Account REST API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.IntegrationAccountCallbackEndPoint
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Net;
    using Constants;
    using Newtonsoft.Json.Linq;

    /// <summary>
    /// Class which defines wrapper methods for some of the functionalities provided by the Integration Account REST API
    /// </summary>
    public class IntegrationAccountCallbackEndPoint : IOMSIntegrationAccountTrackingEndPoint
    {
        /// <summary>
        /// The integration account callback url
        /// </summary>
        private string callbackUrl;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationAccountCallbackEndPoint"/> class
        /// </summary>
        /// <param name="callbackUrl">The callback url of the integration account</param>
        public IntegrationAccountCallbackEndPoint(string callbackUrl)
        {
            this.CallBackUrl = this.GenerateTrackingEndPointFromCallbackUrl(callbackUrl);
        }

        /// <summary>
        /// Gets the callback url
        /// </summary>
        public string CallBackUrl
        {
            get
            {
                return this.callbackUrl;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.CallBackUrl));
                this.callbackUrl = value;
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
        public void PostCustomTrackingEvents(
            string eventSource,
            string correlationId,
            string eventLevel,
            string eventDateTime,
            IDictionary<string, object> customSourceInformation,
            IDictionary<string, object> customEventTrackingProperties)
        {
            var apiUrl = this.CallBackUrl;

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
                // Configure the headers
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                
                // Upload the JSON payload
                client.UploadString(new Uri(apiUrl), "POST", jsonPayload);
            }
        }

        /// <summary>
        /// Generates the tracking API URL from callback URL
        /// </summary>
        /// <param name="callbackUrl">The callback url</param>
        /// <returns>The tracking API URL</returns>
        private string GenerateTrackingEndPointFromCallbackUrl(string callbackUrl)
        {
            // Split on parameters
            var urltokens = callbackUrl.Split('?');
            Contract.Assume(urltokens.Count() > 1, $"Need both the url and the SAS token in {callbackUrl}");

            // Add the tracking event route to the Url
            urltokens[0] = $"{urltokens[0]}/logTrackingEvents";

            return string.Join("?", urltokens);
        }
    }
}
