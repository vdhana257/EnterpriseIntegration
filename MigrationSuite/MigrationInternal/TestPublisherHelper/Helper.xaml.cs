using ABTestAdapter.Adapters;
using ABTestAdapter.Contracts;
using ABTestAdapter.Helpers;
using ABTestAdapter.Readers;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Windows.Forms;
using System.Windows.Forms.Integration;

namespace TestPublisherHelper
{
    /// <summary>
    /// Interaction logic for Helper.xaml
    /// </summary>
    public partial class Helper : System.Windows.Controls.UserControl
    {
        public Helper()
        {
            InitializeComponent();
            
        }

        public static Dictionary<string, string> appSettingDictionary;

        private static Action EmptyDelegate = delegate () { };

        public static void Refresh(UIElement uiElement)
        {
            uiElement.Dispatcher.Invoke(DispatcherPriority.Render, EmptyDelegate);
        }

        

        /// <summary>
        /// 
        /// </summary>
        /// <param name="args"></param>
        public void Main()
        {
            TraceListener debugListener = new MyTraceListener(rtf);
            //Debug.Listeners.Add(debugListener);
            Trace.Listeners.Add(debugListener);
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
                       // rtf.AppendText(" started Main Thread");
                        rtf.Refresh();
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

        private void button_Click(object sender, RoutedEventArgs e)
        {
            Main();
        }
    }
}
