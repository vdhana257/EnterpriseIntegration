
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.IT.Aisap.TelemetryClient;
using Microsoft.IT.Aisap.TelemetryClient.Constants;
using Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient;
using Microsoft.IT.Aisap.TelemetryClient.IntegrationAccountTelemetryClient.IntegrationAccountCallbackEndPoint;
using Microsoft.IT.Aisap.UnityManager;
using Microsoft.Practices.Unity;

namespace ABTestAdapter.Helpers
{
    public static class Utility
    {
        public static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }

        public static string StreamToStringUtf16(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.Unicode))
            {
                return reader.ReadToEnd();
            }
        }

        public static void InitializeOmsIntegrationAccountTelemetryClientUsingCallbackUrl(string integrationAccountCallbackUrl, string functionName)
        {
            var callbackUrl = integrationAccountCallbackUrl;

            UnityManager.Container.RegisterInstance<IOMSIntegrationAccountTrackingEndPoint>(functionName, new IntegrationAccountCallbackEndPoint(callbackUrl));
            UnityManager.Container.RegisterInstance<ITelemetryClient>(functionName, new IntegrationAccountTelemetryClient(functionName));
        }

        public static object GetTelemetryClient(object p, string applicationName, string v)
        {
            throw new NotImplementedException();
        }

        public static ITelemetryClient GetTelemetryClient(string integrationAccountCallbackUrl, string functionName, string loggingLevel)
        {
            // Get the telemetry client
            ITelemetryClient telemetryClient;
            try
            {
                // Check if the singleton instance is still there
                Utility.InitializeOmsIntegrationAccountTelemetryClientUsingCallbackUrl(integrationAccountCallbackUrl,functionName);
                telemetryClient = UnityManager.Container.Resolve<ITelemetryClient>(functionName);
                telemetryClient.AllowRaisingTrackingException(true);
            }
            catch (Exception)
            {
                // If not, re-initialize the telemetry client
                InitializeOmsIntegrationAccountTelemetryClientUsingCallbackUrl(integrationAccountCallbackUrl,functionName);
                telemetryClient = UnityManager.Container.Resolve<ITelemetryClient>(functionName);

                // Get the logging level
                SeverityLevel applicationLoggingLevel;
                Enum.TryParse<SeverityLevel>(loggingLevel, true, out applicationLoggingLevel);

                // Update the logging level for the telemetry client.
                telemetryClient.SetMinimumApplicationLoggingLevel(applicationLoggingLevel);
            }

            TraceProvider.WriteLine("Telemetry client initialized with Logging severity level {0}", loggingLevel);
            return telemetryClient;
        }




    }
}
