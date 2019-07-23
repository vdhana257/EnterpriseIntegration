using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABTestAdapter.Contracts;
using ABTestAdapter.Adapters;
using ABTestAdapter.Publishers;
using ABTestAdapter.Readers;
using Newtonsoft.Json.Linq;
using ABTestAdapter.Helpers;
using System.Web;
using Microsoft.IT.Aisap.TelemetryClient;


namespace AISAdapter
{
    class Program
    {
        public static Dictionary<string, string> appSettingDictionary;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            TraceProvider.WriteLine("ABTestPublisher {0} started", "Main Thread");
            GetApplicationSettings();

            var telemetryClient = Utility.GetTelemetryClient(HttpUtility.UrlDecode(appSettingDictionary["IntegrationAccountCallbackUrl"]), AppConstants.ApplicationName, appSettingDictionary["ApplicationLoggingLevel"]);

            List<JObject> testSuites = GetTestSuites();          

            foreach (JObject testSuite in testSuites)
            {
                if (Convert.ToBoolean(testSuite["Enabled"].ToString()))
                {
                    if (testSuite["ABTestAdapterType"].ToString().ToUpper() == "AISABTESTADAPTER")
                    {
                        string testSuiteName = String.IsNullOrEmpty(testSuite["TestSuiteName"].ToString()) ? string.Empty : testSuite["TestSuiteName"].ToString();
                        TraceProvider.WriteLine("Executing test suite named {0} of type {1}", testSuiteName, testSuite["ABTestAdapterType"].ToString());

                        AisABTestAdapter aisAdapter = new AisABTestAdapter(appSettingDictionary, telemetryClient);
                        aisAdapter.ExecuteABTest(testSuite);
                    }
                }
                else
                {
                    string testSuiteName = String.IsNullOrEmpty(testSuite["TestSuiteName"].ToString()) ? string.Empty : testSuite["TestSuiteName"].ToString();
                    TraceProvider.WriteLine("Test suit named {0} is not enabled", testSuiteName);
                }
            }         

        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        private static List<JObject> GetTestSuites()
        {
            Uri abTestSuiteBlobContainerUri = new Uri(HttpUtility.UrlDecode(appSettingDictionary["ABTestSuiteBlobContainerSASToken"]));
            TraceProvider.WriteLine("Reading test suites to be executed from {0}", abTestSuiteBlobContainerUri.AbsoluteUri);
            IReader blobReader = new BlobReader(abTestSuiteBlobContainerUri);
            var testSuites = blobReader.GetJsonBlobs();
            TraceProvider.WriteLine("Number of test suites found = {0}", testSuites.Count);
            return testSuites;
        }

        /// <summary>
        /// 
        /// </summary>
        private static void GetApplicationSettings()
        {
            TraceProvider.WriteLine("Reading all application settings", string.Empty);
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            KeyValueConfigurationCollection settings = config.AppSettings.Settings;

            appSettingDictionary = settings.AllKeys.ToDictionary(key => key, key => settings[key].Value);
                        
        }
    }
}
