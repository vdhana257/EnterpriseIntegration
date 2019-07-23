//-----------------------------------------------------------------------
// <copyright file="ITelemetryClientContract.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines the restrictions for the ITelemetryClient class using code contracts</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Constants;

    /// <summary>
    /// Class which defines the restrictions for the ITelemetryClient class using code contracts
    /// </summary>
    [ContractClassFor(typeof(ITelemetryClient))]
    public abstract class ITelemetryClientContract : ITelemetryClient
    {
        /// <summary>
        /// Set the minimum logging level in case you don't want to push all the logs.
        /// All the logs below minimum logging level will be ignored. By default, all the logs will be pushed.
        /// </summary>
        /// <param name="mimimumLogginglevel">The minimum logging level</param>
        public void SetMinimumApplicationLoggingLevel(SeverityLevel mimimumLogginglevel)
        {
        }

        /// <summary>
        /// Set whether to raise the tracking exception. By default, no exception will be thrown if the logs are not being pushed to the end point.
        /// </summary>
        /// <param name="throwTrackingException">Whether to allow throwing tracking endpoint related exceptions during runtime</param>
        public void AllowRaisingTrackingException(bool throwTrackingException)
        { 
        }

        /// <summary>
        /// Use to track certain events. The count of the events is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="eventName">The event name</param>
        public void TrackEvent(string eventSource, string correlationClientTrackingId, string eventName)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventName), nameof(eventName));
        }

        /// <summary>
        /// Use to monitor certain quantitative values at certain interval.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="metricName">The metric name</param>
        /// <param name="metricValue">Corresponding metric value</param>
        public void TrackMetric(string eventSource, string correlationClientTrackingId, string metricName, double metricValue)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(metricName), nameof(metricName));
            Contract.Requires<ArgumentException>(!metricName.Contains(" "), nameof(metricName));
        }

        /// <summary>
        /// Use to track exceptions. The frequency of the exception and examining their individual occurrences is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="exception">The exception</param>
        public void TrackException(string eventSource, string correlationClientTrackingId, SeverityLevel severityLevel, Exception exception)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
        }

        /// <summary>
        /// Use to track exceptions. The frequency of the exception and examining their individual occurrences is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="exception">The exception</param>
        public void TrackException(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel, Exception exception)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentException>(message != null, nameof(message));
            Contract.Requires<ArgumentNullException>(exception != null, nameof(exception));
        }

        /// <summary>
        /// Similar to event viewer. Filter out the events based on <paramref name="severityLevel"/> is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        /// <param name="properties">The properties to be added along with the message</param>
        public void TrackTrace(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel, IDictionary<string, object> properties)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentException>(message != null, nameof(message));
            Contract.Requires<ArgumentException>(properties != null && Contract.ForAll(properties, pair => !(string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Contains(" "))), nameof(properties));
        }

        /// <summary>
        /// Similar to event viewer. Filter out the events based on <paramref name="severityLevel"/> is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="message">The message to be displayed</param>
        /// <param name="severityLevel">The severity level</param>
        public void TrackTrace(string eventSource, string correlationClientTrackingId, string message, SeverityLevel severityLevel)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(eventSource), nameof(eventSource));
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(correlationClientTrackingId), nameof(correlationClientTrackingId));
            Contract.Requires<ArgumentException>(message != null, nameof(message));
        }
    }
}
