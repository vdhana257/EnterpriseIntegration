//-----------------------------------------------------------------------
// <copyright file="IOMSIntegrationAccountTrackingEndPointContract.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines the restrictions for the IOMSAzureManagementEndPoint class using code contracts</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class which defines the restrictions for the IOMSAzureManagementEndPoint class using code contracts
    /// </summary>
    [ContractClassFor(typeof(IOMSIntegrationAccountTrackingEndPoint))]
    public abstract class IOMSIntegrationAccountTrackingEndPointContract : IOMSIntegrationAccountTrackingEndPoint
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
        public void PostCustomTrackingEvents(
            string eventSource,
            string correlationId,
            string eventLevel,
            string eventDateTime,
            IDictionary<string, object> customSourceInformation,
            IDictionary<string, object> customEventTrackingProperties)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationId), nameof(correlationId));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventLevel), nameof(eventLevel));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventDateTime), nameof(eventDateTime));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationId), nameof(correlationId));
            Contract.Requires<ArgumentException>(customSourceInformation != null && Contract.ForAll(customSourceInformation, pair => !(string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Contains(" "))), nameof(customSourceInformation));
            Contract.Requires<ArgumentException>(customEventTrackingProperties != null && Contract.ForAll(customEventTrackingProperties, pair => !(string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Contains(" "))), nameof(customEventTrackingProperties));
        }
    }
}
