//-----------------------------------------------------------------------
// <copyright file="IDataCollector.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Interface which defines the contract for the required OMS logging capabilities</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.OMSTelemetryClient
{
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Interface which defines the contract for the required OMS logging capabilities
    /// </summary>
    [ContractClass(typeof(IDataCollectorContract))]
    public interface IDataCollector
    {
        /// <summary>
        /// Use to collect data into the OMS.
        /// </summary>
        /// <param name="logType">The custom log type</param>
        /// <param name="payload">The payload represented as a key value pair</param>
        void Collect(string logType, IDictionary<string, object> payload);
    }
}
