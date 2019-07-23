//-----------------------------------------------------------------------
// <copyright file="IDataCollectorContract.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which defines the restrictions for IDataCollector class using code contracts</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.OMSTelemetryClient
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Class which defines the restrictions for IDataCollector class using code contracts
    /// </summary>
    [ContractClassFor(typeof(IDataCollector))]
    public abstract class IDataCollectorContract : IDataCollector
    {
        /// <summary>
        /// Use to collect data into the OMS.
        /// </summary>
        /// <param name="logType">The custom log type</param>
        /// <param name="payload">The payload represented as a key value pair</param>
        public void Collect(string logType, IDictionary<string, object> payload)
        {
            Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(logType), nameof(logType));
            Contract.Requires<ArgumentException>(!logType.Contains(" "), nameof(logType));
            Contract.Requires<ArgumentNullException>(payload != null, nameof(payload));
            Contract.Requires<ArgumentException>(payload != null && Contract.ForAll(payload, pair => !(string.IsNullOrWhiteSpace(pair.Key) || pair.Key.Contains(" "))), nameof(payload));
        }
    }
}
