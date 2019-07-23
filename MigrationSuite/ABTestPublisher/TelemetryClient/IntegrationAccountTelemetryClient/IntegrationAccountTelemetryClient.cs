//-----------------------------------------------------------------------
// <copyright file="IntegrationAccountTelemetryClient.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which implements the ITelemetryClient contract/interface using the Integration Account Tracking API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient
{
    using System;
    using System.Collections.Generic;
    using Helper;
    using Practices.Unity;
    using TelemetryClient.Constants;

    /// <summary>
    /// Class which implements the ITelemetryClient contract/interface using the Integration Account Tracking API
    /// </summary>
    public class IntegrationAccountTelemetryClient : ITelemetryClient
    {
        /// <summary>
        /// The azure management end point
        /// </summary>
        private readonly IOMSIntegrationAccountTrackingEndPoint managementEndPoint;

        /// <summary>
        /// Determines whether to throw exceptions or not in case the logging fails at the endpoint (a code apart from 2xx is sent).
        /// </summary>
        private object throwTrackingException;

        /// <summary>
        /// Determines the minimum logging level for the telemetry client.
        /// </summary>
        private object mimimumLogginglevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationAccountTelemetryClient"/> class
        /// </summary>
        public IntegrationAccountTelemetryClient()
        {
            this.managementEndPoint = UnityManager.UnityManager.Container.Resolve<IOMSIntegrationAccountTrackingEndPoint>();
            this.MimimumLogginglevel = SeverityLevel.Verbose;
            this.ThrowTrackingException = false;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IntegrationAccountTelemetryClient"/> class
        /// </summary>
        /// <param name="instanceName">Gets the depndencies based on instance name</param>
        public IntegrationAccountTelemetryClient(string instanceName)
        {
            this.managementEndPoint = UnityManager.UnityManager.Container.Resolve<IOMSIntegrationAccountTrackingEndPoint>(instanceName);
            this.MimimumLogginglevel = SeverityLevel.Verbose;
            this.ThrowTrackingException = false;
        }

        /// <summary>
        /// Gets a value indicating whether to throw tracking exception
        /// </summary>
        public bool ThrowTrackingException
        {
            get
            {
                return Convert.ToBoolean(this.throwTrackingException);
            }

            private set
            {
                this.throwTrackingException = value;
            }
        }

        /// <summary>
        /// Gets the mimimum logging level
        /// </summary>
        public SeverityLevel MimimumLogginglevel
        {
            get
            {
                return (SeverityLevel)this.mimimumLogginglevel;
            }

            private set
            {
                this.mimimumLogginglevel = value;
            }
        }

        /// <summary>
        /// Set the minimum logging level in case you don't want to push all the logs.
        /// All the logs below minimum logging level will be ignored. By default, all the logs will be pushed.
        /// </summary>
        /// <param name="mimimumLogginglevel">The minimum logging level</param>
        public void SetMinimumApplicationLoggingLevel(SeverityLevel mimimumLogginglevel)
        {
            this.MimimumLogginglevel = mimimumLogginglevel;
        }

        /// <summary>
        /// Set whether to raise the tracking exception. By default, no exception will be thrown if the logs are not being pushed to the end point.
        /// </summary>
        /// <param name="throwTrackingException">Whether to allow throwing tracking endpoint related exceptions during runtime</param>
        public void AllowRaisingTrackingException(bool throwTrackingException)
        {
            this.ThrowTrackingException = throwTrackingException;
        }

        /// <summary>
        /// Use to track certain events. The count of the events is what we are interested in here.
        /// </summary>
        /// <param name="eventSource">The event source name</param>
        /// <param name="correlationClientTrackingId">The correlation tracking id of the workflow run</param>
        /// <param name="eventName">The event name</param>
        public void TrackEvent(string eventSource, string correlationClientTrackingId, string eventName)
        {
            if (!SeverityLevelHelper.IsAllowedForLogging(SeverityLevel.Informational, this.MimimumLogginglevel))
            {
                return;
            }

            try
            {
                this.managementEndPoint.PostCustomTrackingEvents(
                        eventSource,
                        correlationClientTrackingId,
                        SeverityLevel.Informational.ToString(),
                        DateTime.UtcNow.ToString("O"),
                        new Dictionary<string, object>(),
                        new Dictionary<string, object>()
                        {
                    { Constants.IntegrationAccountTrackingApiConstants.OmsEventName, eventName }
                        });
            }
            catch (Exception)
            {
                if (this.ThrowTrackingException)
                {
                    throw;
                }
            }
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
            if (!SeverityLevelHelper.IsAllowedForLogging(SeverityLevel.Informational, this.MimimumLogginglevel))
            {
                return;
            }

            try
            {
                this.managementEndPoint.PostCustomTrackingEvents(
                        eventSource,
                        correlationClientTrackingId,
                        SeverityLevel.Informational.ToString(),
                        DateTime.UtcNow.ToString("O"),
                        new Dictionary<string, object>(),
                        new Dictionary<string, object>()
                        {
                    { metricName, metricValue }
                        });
            }
            catch (Exception)
            {
                if (this.ThrowTrackingException)
                {
                    throw;
                }
            }
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
            this.TrackException(eventSource, correlationClientTrackingId, string.Empty, severityLevel, exception);
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
            if (!SeverityLevelHelper.IsAllowedForLogging(severityLevel, this.MimimumLogginglevel))
            {
                return;
            }

            try
            {
                Dictionary<string, object> trackedExceptions = new Dictionary<string, object>
                {
                    { Constants.IntegrationAccountTrackingApiConstants.OmsCorrelationTrackingId, correlationClientTrackingId },
                    { Constants.IntegrationAccountTrackingApiConstants.OmsMesssage, message },
                    { Constants.IntegrationAccountTrackingApiConstants.OmsSeverityLevel, severityLevel.ToString() },
                    { Constants.IntegrationAccountTrackingApiConstants.OmsExceptionType, exception.GetType().ToString() },
                    { Constants.IntegrationAccountTrackingApiConstants.OmsExceptionMesssage, exception.Message },
                    { Constants.IntegrationAccountTrackingApiConstants.OmsExceptionStackTrace, exception.StackTrace }
                };

                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                {
                    var i = 0;
                    foreach (Exception innerException in aggregateException.InnerExceptions)
                    {
                        trackedExceptions.Add($"{Constants.IntegrationAccountTrackingApiConstants.OmsInnerExceptionType}_{i}", innerException.GetType().ToString());
                        trackedExceptions.Add($"{Constants.IntegrationAccountTrackingApiConstants.OmsInnerExceptionMesssage}_{i}", innerException.Message);
                        trackedExceptions.Add($"{Constants.IntegrationAccountTrackingApiConstants.OmsInnerExceptionStackTrace}_{i}", innerException.StackTrace);
                        i++;
                    }
                }

                this.managementEndPoint.PostCustomTrackingEvents(
                    eventSource,
                    correlationClientTrackingId,
                    severityLevel.ToString(),
                    DateTime.UtcNow.ToString("O"),
                    new Dictionary<string, object>(),
                    trackedExceptions);
            }
            catch (Exception)
            {
                if (this.ThrowTrackingException)
                {
                    throw;
                }
            }
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
            if (!SeverityLevelHelper.IsAllowedForLogging(severityLevel, this.MimimumLogginglevel))
            {
                return;
            }

            try
            {
                properties.Add(Constants.IntegrationAccountTrackingApiConstants.OmsMesssage, message);

                this.managementEndPoint.PostCustomTrackingEvents(
                    eventSource,
                    correlationClientTrackingId,
                    severityLevel.ToString(),
                    DateTime.UtcNow.ToString("O"),
                    new Dictionary<string, object>(),
                    properties);
            }
            catch (Exception)
            {
                if (this.ThrowTrackingException)
                {
                    throw;
                }
            }
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
            if (!SeverityLevelHelper.IsAllowedForLogging(severityLevel, this.MimimumLogginglevel))
            {
                return;
            }

            try
            {
                Dictionary<string, object> properties = new Dictionary<string, object>
                {
                    { Constants.IntegrationAccountTrackingApiConstants.OmsMesssage, message }
                };

                this.managementEndPoint.PostCustomTrackingEvents(
                    eventSource,
                    correlationClientTrackingId,
                    severityLevel.ToString(),
                    DateTime.UtcNow.ToString("O"),
                    new Dictionary<string, object>(),
                    properties);
            }
            catch (Exception)
            {
                if (this.ThrowTrackingException)
                {
                    throw;
                }
            }
        }
    }
}
