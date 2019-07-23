//-----------------------------------------------------------------------
// <copyright file="DataCollectorApiWrapper.cs" company="Microsoft">
//     Microsoft Copyright.
// </copyright>
// <summary>Class which implements the IDataCollector contract/interface using the request/response architecture of the Azure Log Analytics(OMS) Data Collector API</summary>
//-----------------------------------------------------------------------

namespace Microsoft.IT.Aisap.TelemetryClient.OMSTelemetryClient.DataCollectorAPI
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.Contracts;
    using System.Net;
    using System.Security.Cryptography;
    using Newtonsoft.Json;

    /// <summary>
    /// Class which implements the IDataCollector contract/interface using the request/response architecture of the Azure Log Analytics(OMS) Data Collector API
    /// </summary>
    public class DataCollectorApiWrapper : IDataCollector
    {
        /// <summary>
        /// The workspace Id
        /// </summary>
        private string workspaceId;

        /// <summary>
        /// The client authentication key
        /// </summary>
        private string sharedkey;

        /// <summary>
        /// The data collector API version
        /// </summary>
        private string apiVersion;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataCollectorApiWrapper"/> class
        /// </summary>
        /// <param name="workspaceId">The workspace ID of the OMS workspace</param>
        /// <param name="sharedKey">The client authentication key of the workspace</param>
        /// <param name="apiVersion">The API version of the Data Collector API</param>
        public DataCollectorApiWrapper(string workspaceId, string sharedKey, string apiVersion = Constants.OMSTrackingAPIConstants.DefaultApiVersion)
        {
            this.WorkspaceId = workspaceId;
            this.Sharedkey = sharedKey;
            this.ApiVersion = apiVersion;
        }

        /// <summary>
        /// Gets the workspace id
        /// </summary>
        public string WorkspaceId
        {
            get
            {
                return this.workspaceId;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.WorkspaceId));
                this.workspaceId = value;
            }
        }

        /// <summary>
        /// Gets the client authentication key (can be primary or secondary)
        /// </summary>
        public string Sharedkey
        {
            get
            {
                return this.sharedkey;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.Sharedkey));
                this.sharedkey = value;
            }
        }
        
        /// <summary>
        /// Gets the API version of the Data Collector API
        /// </summary>
        public string ApiVersion
        {
            get
            {
                return this.apiVersion;
            }

            private set
            {
                Contract.Requires<ArgumentException>(!string.IsNullOrWhiteSpace(value), nameof(this.ApiVersion));
                this.apiVersion = value;
            }
        }

        /// <summary>
        /// Use to collect data into the OMS.
        /// </summary>
        /// <param name="logType">The custom log type</param>
        /// <param name="payload">The payload represented as a key value pair</param>
        public void Collect(string logType, IDictionary<string, object> payload)
        {
            string payloadJson = JsonConvert.SerializeObject(payload);

            // Create a hash for the API signature
            var datestring = DateTime.UtcNow.ToString("r");
            string stringToHash = $"POST\n{payloadJson.Length}\napplication/json\nx-ms-date:{datestring}\n/api/logs";
            string hashedString = BuildSignature(stringToHash, this.Sharedkey);
            string signature = $"SharedKey {this.WorkspaceId}:{hashedString}";
            
            this.PostData(signature, datestring, payloadJson, logType);
        }

        /// <summary>
        /// Builds the signature token to be used in the post request headers
        /// </summary>
        /// <param name="message">The message to be signed</param>
        /// <param name="secret">The signing key</param>
        /// <returns>The signature token to be used in the post request headers</returns>
        private static string BuildSignature(string message, string secret)
        {
            var encoding = new System.Text.ASCIIEncoding();
            byte[] keyByte = Convert.FromBase64String(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hash = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hash);
            }
        }
        
        /// <summary>
        /// Send a request to the POST API endpoint
        /// </summary>
        /// <param name="signature">The signed message</param>
        /// <param name="date">The date header</param>
        /// <param name="json">The JSON payload</param>
        /// <param name="logType">The log type to be used in the Log Analytics(OMS)</param>
        private void PostData(string signature, string date, string json, string logType)
        {
            string url = $"https://{this.WorkspaceId}.ods.opinsights.azure.com/api/logs?api-version={this.ApiVersion}";
            using (var client = new WebClient())
            {
                client.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                client.Headers.Add("Log-Type", logType);
                client.Headers.Add("Authorization", signature);
                client.Headers.Add("x-ms-date", date);
                client.UploadString(new Uri(url), "POST", json);
            }
        }
    }
}