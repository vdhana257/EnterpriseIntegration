//-----------------------------------------------------------------------
// <copyright file="IOMSIntegrationAccountTrackingEndPoint.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Interface which defines the contract for wrapper methods for some of the functionalities provided by the Management REST API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Interface which defines the contract for wrapper methods for some of the functionalities provided by the Management REST API
    /// </summary>
    [ContractClass(typeof(IOMSIntegrationAccountTrackingEndPointContract))]
    public interface IOMSIntegrationAccountTrackingEndPoint
    {
        /// <summary>
        /// Post the custom tracking events to the OMS using Integration Account Tracking API
        /// </summary>
        /// <param name="eventSource">The event source</param>
        /// <param name="correlationId">The correlation client tracking ID</param>
        /// <param name="eventLevel">The event severity level</param>
        /// <param name="eventDateTime">The event date time</param>
        /// <param name="customSourceInformation">The custom event source information</param>
        /// <param name="customEventTrackingProperties">The custom tracking event properties to be posted to OMS</param>
        void PostCustomTrackingEvents(
            string eventSource,
            string correlationId,
            string eventLevel,
            string eventDateTime,
            IDictionary<string, object> customSourceInformation,
            IDictionary<string, object> customEventTrackingProperties);
    }
}
