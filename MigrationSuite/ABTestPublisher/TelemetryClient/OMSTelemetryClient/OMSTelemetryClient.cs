//-----------------------------------------------------------------------
// <copyright file="OMSTelemetryClient.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which implements the ITelemetryClient contract/interface using the Azure Log Analytics(OMS) Data Collector API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.OMSTelemetryClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using Helper;
    using Practices.Unity;
    using TelemetryClient.Constants;

    /// <summary>
    /// Class which implements the ITelemetryClient contract/interface using the Azure Log Analytics(OMS) Data Collector API
    /// </summary>
    public class OMSTelemetryClient : ITelemetryClient
    {
        /// <summary>
        /// The OMS Data Collector
        /// </summary>
        private readonly IDataCollector dataCollector;

        /// <summary>
        /// The log type
        /// </summary>
        private string logtype;

        /// <summary>
        /// Determines whether to throw exceptions or not in case the logging fails at the endpoint (a code apart from 2xx is sent).
        /// </summary>
        private object throwTrackingException;

        /// <summary>
        /// Determines the minimum logging level for the telemetry client.
        /// </summary>
        private object mimimumLogginglevel;

        /// <summary>
        /// Initializes a new instance of the <see cref="OMSTelemetryClient"/> class
        /// </summary>
        /// <param name="logType">The OMS Log Type</param>
        public OMSTelemetryClient(string logType)
        {
            this.dataCollector = UnityManager.UnityManager.Container.Resolve<IDataCollector>();
            this.LogType = logType;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="OMSTelemetryClient"/> class
        /// </summary>
        /// <param name="logType">The OMS Log Type</param>
        /// <param name="instanceName">Gets the dependencies based on instance name</param>
        public OMSTelemetryClient(string logType, string instanceName)
        {
            this.dataCollector = UnityManager.UnityManager.Container.Resolve<IDataCollector>(instanceName);
            this.LogType = logType;
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
        /// Gets the Log Type
        /// </summary>
        public string LogType
        {
            get
            {
                return this.logtype;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.LogType));
                Contract.Requires<ArgumentException>(!value.Contains(" "), nameof(this.LogType));
                this.logtype = value;
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
                Dictionary<string, object> trackEvents = new Dictionary<string, object>
                {
                    { Constants.OMSTrackingAPIConstants.SourceEventSourceName, eventSource },
                    { $"{Constants.OMSTrackingAPIConstants.SourceRunInstance}{Constants.OMSTrackingAPIConstants.OmsCorrelationTrackingId}", correlationClientTrackingId },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsEventName}", eventName }
                };

                this.dataCollector.Collect(this.LogType, trackEvents);
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
                Dictionary<string, object> trackMetrics = new Dictionary<string, object>
                {
                    { Constants.OMSTrackingAPIConstants.SourceEventSourceName, eventSource },
                    { $"{Constants.OMSTrackingAPIConstants.SourceRunInstance}{Constants.OMSTrackingAPIConstants.OmsCorrelationTrackingId}", correlationClientTrackingId },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{metricName}", metricValue }
                };

                this.dataCollector.Collect(this.LogType, trackMetrics);
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
                    { Constants.OMSTrackingAPIConstants.SourceEventSourceName, eventSource },
                    { $"{Constants.OMSTrackingAPIConstants.SourceRunInstance}{Constants.OMSTrackingAPIConstants.OmsCorrelationTrackingId}", correlationClientTrackingId },
                    { $"{Constants.OMSTrackingAPIConstants.Event}{Constants.OMSTrackingAPIConstants.OmsEventLevel}", severityLevel.ToString() },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsMesssage}", message },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsExceptionType}", exception.GetType().ToString() },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsExceptionMesssage}", exception.Message },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsExceptionStackTrace}", exception.StackTrace }
                };

                var aggregateException = exception as AggregateException;
                if (aggregateException != null)
                {
                    var i = 0;
                    foreach (Exception innerException in aggregateException.InnerExceptions)
                    {
                        trackedExceptions.Add($"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsInnerExceptionType}_{i}", innerException.GetType().ToString());
                        trackedExceptions.Add($"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsInnerExceptionMesssage}_{i}", innerException.Message);
                        trackedExceptions.Add($"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsInnerExceptionStackTrace}_{i}", innerException.StackTrace);
                        i++;
                    }
                }

                this.dataCollector.Collect(this.LogType, trackedExceptions);
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
                Dictionary<string, object> trackTraces = new Dictionary<string, object>
                {
                    { Constants.OMSTrackingAPIConstants.SourceEventSourceName, eventSource },
                    { $"{Constants.OMSTrackingAPIConstants.SourceRunInstance}{Constants.OMSTrackingAPIConstants.OmsCorrelationTrackingId}", correlationClientTrackingId },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsEventLevel}", severityLevel.ToString() },
                    { $"{Constants.OMSTrackingAPIConstants.EventRecord}{Constants.OMSTrackingAPIConstants.OmsMesssage}", message }
                };

                foreach (KeyValuePair<string, object> property in properties)
                {
                    trackTraces.Add($"{Constants.OMSTrackingAPIConstants.EventRecord}{property.Key}", property.Value);
                }

                this.dataCollector.Collect(this.LogType, trackTraces);
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
            this.TrackTrace(eventSource, correlationClientTrackingId, message, severityLevel, new Dictionary<string, object>());
        }
    }
}
