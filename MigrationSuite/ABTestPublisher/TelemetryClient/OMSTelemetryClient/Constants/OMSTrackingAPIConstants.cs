//-----------------------------------------------------------------------
// <copyright file="OMSTrackingAPIConstants.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Defines the constants for OMS Telemetry Client</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.OMSTelemetryClient.Constants
{
    /// <summary>
    /// Defines the constants for OMS Telemetry Client
    /// </summary>
    public class OMSTrackingAPIConstants
    {
        /// <summary>
        /// The default data collector API version
        /// </summary>
        public const string DefaultApiVersion = "2016-04-01";

        /// <summary>
        /// The source type
        /// </summary>
        public const string SourceEventSourceName = "source_eventSourceName";

        /// <summary>
        /// The event record prefix
        /// </summary>
        public const string EventRecord = "event_record_";

        /// <summary>
        /// The event prefix
        /// </summary>
        public const string Event = "event_";

        /// <summary>
        /// The source run instance prefix
        /// </summary>
        public const string SourceRunInstance = "source_runInstance_";

        /// <summary>
        /// The OMS Correlation Tracking ID
        /// </summary>
        public const string OmsCorrelationTrackingId = "clientTrackingId";

        /// <summary>
        /// The OMS event name
        /// </summary>
        public const string OmsEventName = "eventName";

        /// <summary>
        /// The OMS severity level
        /// </summary>
        public const string OmsEventLevel = "eventLevel";

        /// <summary>
        /// The OMS exception type
        /// </summary>
        public const string OmsExceptionType = "exceptionType";

        /// <summary>
        /// The OMS exception message
        /// </summary>
        public const string OmsExceptionMesssage = "exceptionMessage";

        /// <summary>
        /// The OMS exception stack trace
        /// </summary>
        public const string OmsExceptionStackTrace = "exceptionStackTrace";

        /// <summary>
        /// The OMS inner exception type
        /// </summary>
        public const string OmsInnerExceptionType = "innerExceptionType";

        /// <summary>
        /// The OMS inner exception message
        /// </summary>
        public const string OmsInnerExceptionMesssage = "innerExceptionMessage";

        /// <summary>
        /// The OMS inner exception stack trace
        /// </summary>
        public const string OmsInnerExceptionStackTrace = "innerExceptionStackTrace";

        /// <summary>
        /// The OMS message
        /// </summary>
        public const string OmsMesssage = "message";
    }
}
