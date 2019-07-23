//-----------------------------------------------------------------------
// <copyright file="ITelemetryClient.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Interface which defines the contract for the required telemetry capabilities</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Constants;

    /// <summary>
    /// Interface which defines the contract for the required telemetry capabilities across Azure resources
    /// </summary>
    [ContractClass(typeof(ITelemetryClientContract))]
    public interface ITelemetryClient
    {
        /// <summary>
        /// Set the minimum logging level in case you don't want to push all the logs.
        /// All the logs below minimum logging level will be ignored. By default, all the logs will be pushed.
        /// </summary>
        /// <param name="level">The minimum logging level</param>
        void SetMinimumApplicationLoggingLevel(SeverityLevel level);

        /// <summary>
        /// Set whether to throw the tracking exception. (In case a code apart from 2xx is sent) 
        /// By default, no exception will be thrown if the logs are not being pushed to the end point.
        /// </summary>
        /// <param name="throwTrackingException">Whether to allow throwing tracking endpoint related exceptions during runtime</param>
        void AllowRaisingTrackingException(bool throwTrackingException);

        /// <summary>
        /// Use to track certain events. The count of the events is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="eventName">The event name</param>
        void TrackEvent(string eventSource, string correlationClientTrackingId, string eventName);

        /// <summary>
        /// Use to monitor certain quantitative values at certain interval.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="metricName">The metric name</param>
        /// <param name="metricValue">Corresponding metric value</param>
        void TrackMetric(string eventSource, string correlationClientTrackingId, string metricName, double metricValue);

        /// <summary>
        /// Use to track exceptions. The frequency of the exception and examining their individual occurrences is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="exception">The exception</param>
        void TrackException(string eventSource, string correlationClientTrackingId, SeverityLevel severityLevel, Exception exception);

        /// <summary>
        /// Use to track exceptions. The frequency of the exception and examining their individual occurrences is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="exception">The exception</param>
        void TrackException(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel, Exception exception);

        /// <summary>
        /// Similar to event viewer. Filter out the events based on <paramref name="severityLevel"/> is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="properties">The properties to be added along with the message</param>
        void TrackTrace(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel, IDictionary<string, object> properties);

        /// <summary>
        /// Similar to event viewer. Filter out the events based on <paramref name="severityLevel"/> is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        void TrackTrace(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel);
    }
}
