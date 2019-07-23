using ABTestAdapter.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using ABTestAdapter.Helpers;
using ABTestAdapter.Publishers;
using System.IO;
using ABTestAdapter.Readers;
using Newtonsoft.Json;
using Microsoft.IT.Aisap.TelemetryClient;
using Microsoft.IT.Aisap.TelemetryClient.Constants;
using System.Web;
using Microsoft.IT.Aisap.ConfigEncryptor;

namespace ABTestAdapter.Adapters
{
    public class AisABTestAdapter
    {
        #region InstanceVariables
        BizTrackAuthRequest authRequest;
        BizTrackApiRequest trackingDataRequest;
        string accessToken = string.Empty;
        Dictionary<string, string> omsEvent;
        Dictionary<string, string> adapterSettings;
        List<Payload> fileCollection;
        JObject testSuite;
        ITelemetryClient telemetryClient;
        string testSuiteRunId = string.Empty;
        string trackingId = string.Empty;
        #endregion

        #region constructor
        public AisABTestAdapter(Dictionary<string, string> applicationSettings, ITelemetryClient telemetryClient)
        {
            this.adapterSettings = applicationSettings;
            this.telemetryClient = telemetryClient;
        }
        #endregion

        #region SourceBizTrack
        private void ExecuteABTestFromBizTrack()
        {
            //Create tracking api request
            CreateTrackingDataRequest(testSuite);

            this.SetAccessToken();

            // Get master tracking data from BizTrack Api
            var masters = GetMasterTrackingRecords();

            IPublisher publisher = GetMessagePublisher(AppConstants.Expected);

            SetupExpectedOutputFiles(masters, publisher);

            // post test file to new system
            publisher = GetMessagePublisher(AppConstants.Actual);

            PostTestFilesToNewSystem(masters, publisher);
        }

        private void SetAccessToken()
        {
            // create Biztrack auth request
            authRequest = new BizTrackAuthRequest();
            try
            {
                authRequest.ITAuthRbacResourceUri = adapterSettings["ITAuthRbacResourceUri"];
                authRequest.ClientId = adapterSettings["TrackingService/ClientId"];
                authRequest.ClientSecret = EncryptionHelper.DecryptIfEncrypted(adapterSettings["TrackingService/ClientSecret"]);
                authRequest.AuthenticationContextUri = adapterSettings["AADUri"];

                var accessTokenTask = AuthenticationHelper.GetAccessTokenForBizTrackAsync(authRequest);
                accessTokenTask.Wait();
                accessToken = accessTokenTask.Result;
                TraceProvider.WriteLine("Executing test suit run id {0}, access token received for BizTrack API {1}", testSuiteRunId, accessToken);
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, error while getting access token for BizTrack API {1}", testSuiteRunId, ex.Message);
            }
        }

        private void CreateTrackingDataRequest(JObject testSuite)
        {
            trackingDataRequest = new BizTrackApiRequest();
            try
            {
                trackingDataRequest.partners = testSuite["Partners"].ToString().Split('|').ToList<string>();
                trackingDataRequest.transactions = testSuite["Transactions"].ToString().Split('|').ToList<string>();
                trackingDataRequest.masterFilter.Partners = trackingDataRequest.partners;
                trackingDataRequest.masterFilter.Transactions = trackingDataRequest.transactions;
                trackingDataRequest.masterFilter.StartDate = DateTime.Parse(testSuite["StartDate"].ToString());
                trackingDataRequest.masterFilter.EndDate = DateTime.Parse(testSuite["EndDate"].ToString());
                trackingDataRequest.upn = adapterSettings["Upn"];
                trackingDataRequest.baseUri = adapterSettings["BaseAddressBiztrackApi"];
                trackingDataRequest.relativeUriMaster = adapterSettings["RelativeAddressResourceMaster"];
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, error while creating BizTrack api request. Error description is {1}", testSuiteRunId, ex.Message);
            }
        }

        public JEnumerable<JObject> GetMasterTrackingRecords()
        {
            HttpClient client = new HttpClient();
            client.BaseAddress = new Uri(trackingDataRequest.baseUri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AppConstants.TokenHeaderName, accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AppConstants.ContentTypeJson));
            client.DefaultRequestHeaders.Add("upn", trackingDataRequest.upn);

            string jsonResult = string.Empty;
            JEnumerable<JObject> masterObjects;
            try
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, Getting master tracking data from BizTrack api {1}{2}", testSuiteRunId, client.BaseAddress, trackingDataRequest.relativeUriMaster);

                var response = client.PostAsJsonAsync(trackingDataRequest.relativeUriMaster, trackingDataRequest.masterFilter).Result;
                if (response.IsSuccessStatusCode)
                {
                    jsonResult = response.Content.ReadAsStringAsync().Result;
                }

                masterObjects = JsonHelper.AllChildren(JObject.Parse(jsonResult))
            .First(c => c.Type == JTokenType.Array && c.Path.Contains(AppConstants.MastersLiteral))
            .Children<JObject>();

                TraceProvider.WriteLine("Executing test suit run id {0}, master tracking data retrieved from BizTrack api. Number of Master records {1}", testSuiteRunId, masterObjects.Count<JObject>());
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, Error while getting master tracking data. Error message {1}", testSuiteRunId, ex.Message);
            }

            return masterObjects;
        }

        public JEnumerable<JObject> GetDetailedTrackingRecords(JObject masterObject)
        {
            string jsonResult = string.Empty;
            JEnumerable<JObject> detailObjects;

            using (HttpClient detailClient = new HttpClient())
            {
                detailClient.BaseAddress = new Uri(trackingDataRequest.baseUri);
                detailClient.DefaultRequestHeaders.Accept.Clear();
                detailClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(AppConstants.TokenHeaderName, accessToken);
                detailClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(AppConstants.ContentTypeJson));
                detailClient.DefaultRequestHeaders.Add(AppConstants.Upn, trackingDataRequest.upn);

                try
                {
                    string detailRelativeUri = string.Format("api/Track/Detail?MasterID={0}&PageNumber=1", masterObject.Property("ID").Value.ToString());
                    TraceProvider.WriteLine("Executing test suit run id {0}, Getting detailed tracking data from BizTrack api {1}{2}", testSuiteRunId, detailClient.BaseAddress, detailRelativeUri);
                    var response = detailClient.GetAsync(detailRelativeUri).Result;
                    if (response.IsSuccessStatusCode)
                    {
                        jsonResult = response.Content.ReadAsStringAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    TraceProvider.WriteLine("Executing test suit run id {0}, Error while retrieving detail tracking data from BizTrack API {1}", testSuiteRunId, ex.Message);
                }
            }

            detailObjects = JsonHelper.AllChildren(JObject.Parse(jsonResult))
            .First(c => c.Type == JTokenType.Array && c.Path.Contains(AppConstants.DetailsLiteral))
            .Children<JObject>();

            TraceProvider.WriteLine("Executing test suit run id {0}, detail tracking data retrieved from BizTrack API. Count is {1}", testSuiteRunId, detailObjects.Count<JObject>());

            return detailObjects;
        }

        public Payload GetPayload(string filePath)
        {

            Payload payLoadObject = new Payload();
            Stream s = new MemoryStream();
            using (HttpClient payloadClient = new HttpClient())
            {
                string trackingUri = adapterSettings["BaseAddressBiztrackApi"];
                payloadClient.BaseAddress = new Uri(trackingUri + "api/Track/Download?payloadPath=" + filePath);
                payloadClient.DefaultRequestHeaders.Accept.Clear();
                payloadClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                payloadClient.DefaultRequestHeaders.Add("upn", trackingDataRequest.upn);

                try
                {
                    TraceProvider.WriteLine("Executing test suit run id {0}, downloading payload from {1}", testSuiteRunId, payloadClient.BaseAddress.AbsoluteUri);
                    HttpResponseMessage response = payloadClient.GetAsync("").Result;
                    if (response.IsSuccessStatusCode)
                    {
                        payLoadObject.Stream = response.Content.ReadAsStreamAsync().Result;
                    }
                }
                catch (Exception ex)
                {
                    TraceProvider.WriteLine("Executing test suit run id {0}, error while downloading payload from {1}. Error message is {2}", testSuiteRunId, payloadClient.BaseAddress.AbsoluteUri,ex.Message);
                }

            }

            return payLoadObject;

        }


        public void SetupExpectedOutputFiles(JEnumerable<JObject> masterObjects, IPublisher publisherObject)
        {
            foreach (JObject masterObject in masterObjects)
            {
                JEnumerable<JObject> detailObjects = GetDetailedTrackingRecords(masterObject);
                foreach (JObject detailObject in detailObjects)
                {
                    string masterId = masterObject.Property("ID").Value.ToString();
                    string detailId = detailObject.Property("ID").Value.ToString();
                    string postTransformFilepath = detailObject.Property("FilePath").Value.ToString();

                    try
                    {
                        var payLoadObject = GetPayload(postTransformFilepath);
                        if ((payLoadObject.Stream != null) && (payLoadObject.Stream.Length > 0))
                        {
                            payLoadObject.FileName = string.Format("{0}_{1}", masterId, detailId);
                            TraceProvider.WriteLine("Executing test suit run id {0}, Setting up expected file {1}", testSuiteRunId, payLoadObject.FileName);
                            publisherObject.Publish(payLoadObject);
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceProvider.WriteLine("Executing test suit run id {0}, error while setting up expected files {1}", testSuiteRunId, ex.Message);
                    }
                }
            }
        }

        public void PostTestFilesToNewSystem(JEnumerable<JObject> masterObjects, IPublisher publisherObject)
        {
            foreach (JObject masterObject in masterObjects)
            {
                string controlNumber = masterObject.Property("ControlNumber").Value.ToString();
                string id = masterObject.Property("ID").Value.ToString();
                string preTransformFilepath = masterObject.Property("FilePath").Value.ToString();

                try
                {
                    var payLoadObject = GetPayload(preTransformFilepath);

                    if ((payLoadObject.Stream != null) && (payLoadObject.Stream.Length > 0))
                    {
                        payLoadObject.FileName = string.Format("{0}_{1}", id, controlNumber);

                        TraceProvider.WriteLine("Executing test suit run id {0}, Posting pre-transform file to LA {1}", testSuiteRunId, payLoadObject.FileName);
                        publisherObject.Publish(payLoadObject);

                        if (publisherObject.GetType().ToString() == "ABTestAdapter.Publishers.LogicAppPublisher")
                        {
                            HttpHeaders responseheaders = ((LogicAppPublisher)publisherObject).LogicAppResponse.Headers;
                            IEnumerable<string> values;

                            if (responseheaders.TryGetValues("correlationTrackingId", out values))
                            {
                                trackingId = values.First();
                            }
                            else if (responseheaders.TryGetValues("x-ms-client-tracking-id", out values))
                            {
                                trackingId = values.First();
                            }

                        }
                        else
                        {
                            trackingId = payLoadObject.FileName;
                        }

                        RaisePublisherEvent(payLoadObject);
                    }
                }
                catch (Exception ex)
                {
                    TraceProvider.WriteLine("Executing test suit run id {0}, Error posting pre-transform file to LA {1}", testSuiteRunId, ex.Message);
                }
            }
        }

        #endregion

        private void ExecuteABTestFromBlob()
        {
            bool isException = false;
            IPublisher publisher = GetMessagePublisher(AppConstants.Expected);
            BlobReader blobReader = new BlobReader(new Uri(HttpUtility.UrlDecode(adapterSettings["FileSourceExpected"])));
            try
            {
                fileCollection = blobReader.GetBlobPayloads();
            }
            catch(Exception ex)
            {
                isException = true;
                TraceProvider.WriteLine("Executing test suit run id {0}, error while reading expected files from Blob source. Error message {1}", testSuiteRunId, ex.Message);
            }

            TraceProvider.WriteLine("Executing test suit run id {0}, Setting up expected files. Number of files found {1}", testSuiteRunId, fileCollection.Count);
            if (!isException)
            {
                foreach (var payload in fileCollection)
                {
                    if ((payload.Stream != null) && (payload.Stream.Length > 0))
                    {
                        payload.FileName = string.Format("{0}_{1}", payload.FileName, Guid.NewGuid());
                        try
                        {
                            publisher.Publish(payload);
                        }
                        catch (Exception ex)
                        {
                            TraceProvider.WriteLine("Executing test suit run id {0}, error while writing expected file named {1}, error description is {2}", testSuiteRunId, payload.FileName, ex.Message);
                        }
                    }
                }
            }

            publisher = GetMessagePublisher(AppConstants.Actual);
            blobReader = new BlobReader(new Uri(HttpUtility.UrlDecode(adapterSettings["FileSourceActual"])));
            try
            {
                isException = false;
                fileCollection = blobReader.GetBlobPayloads();
            }
            catch (Exception ex)
            {
                isException = true;
                TraceProvider.WriteLine("Executing test suit run id {0}, error while reading original files from Blob source. Error message {1}", testSuiteRunId, ex.Message);
            }

            TraceProvider.WriteLine("Executing test suit run id {0}, Posting actual files to new system. Number of files posted are {1}", testSuiteRunId, fileCollection.Count);
            if (!isException)
            {
                foreach (var payload in fileCollection)
                {
                    if ((payload.Stream != null) && (payload.Stream.Length > 0))
                    {
                        payload.FileName = string.Format("{0}", payload.FileName);
                        try
                        {
                            publisher.Publish(payload);
                            trackingId = payload.FileName;
                        }
                        catch (Exception ex)
                        {
                            TraceProvider.WriteLine("Executing test suit run id {0}, error while writing actual file named {1}, error description is {2}", testSuiteRunId, payload.FileName, ex.Message);
                        }
                        RaisePublisherEvent(payload);
                    }
                }
            }
        }

        #region SourceFileshare

        private void ExecuteABtestFromFileshare()
        {
            IPublisher publisher = GetMessagePublisher(AppConstants.Expected);
            SetupExpectedOutputFiles(adapterSettings["FileSourceExpected"], publisher);

            publisher = GetMessagePublisher(AppConstants.Actual);
            PostTestFilesToNewSystem(adapterSettings["FileSourceActual"], publisher);
        }

        private void ReadAllFiles(string directoryPath)
        {
            fileCollection = new List<Payload>();
            try
            {
                DirectoryInfo testFileDirectory = new DirectoryInfo(directoryPath);
                FileInfo[] files = testFileDirectory.GetFiles();
                foreach (var file in files)
                {
                    MemoryStream content = new MemoryStream(File.ReadAllBytes(directoryPath + @"\" + file.Name));
                    var payload = new Payload();
                    payload.Stream = content;
                    payload.ContentType = "Application/Octet-Stream";
                    payload.FileName = file.Name;
                    fileCollection.Add(payload);
                }
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, error while reading files from file source. Error message {1}", testSuiteRunId, ex.Message);
            }


        }


        public void SetupExpectedOutputFiles(string directoryPath, IPublisher publisherObject)
        {
            ReadAllFiles(directoryPath);

            TraceProvider.WriteLine("Executing test suit run id {0}, Setting up expected files. Number of files found {1}", testSuiteRunId, fileCollection.Count);

            foreach (var payload in fileCollection)
            {
                if ((payload.Stream != null) && (payload.Stream.Length > 0))
                {
                    payload.FileName = string.Format("{0}_{1}", payload.FileName, Guid.NewGuid());
                    try
                    {
                        publisherObject.Publish(payload);
                    }
                    catch (Exception ex)
                    {
                        TraceProvider.WriteLine("Executing test suit run id {0}, error while writing expected file named {1}, error description is {2}", testSuiteRunId, payload.FileName, ex.Message);
                    }
                }
            }
        }

        public void PostTestFilesToNewSystem(string directorypath, IPublisher publisherObject)
        {
            ReadAllFiles(directorypath);

            TraceProvider.WriteLine("Executing test suit run id {0}, Posting actual files to new system. Number of files posted are {1}", testSuiteRunId, fileCollection.Count);

            foreach (var payload in fileCollection)
            {
                if ((payload.Stream != null) && (payload.Stream.Length > 0))
                {
                    payload.FileName = string.Format("{0}", payload.FileName);
                    try
                    {
                        publisherObject.Publish(payload);

                        if (publisherObject.GetType().ToString() == "ABTestAdapter.Publishers.LogicAppPublisher")
                        {
                            HttpHeaders responseheaders = ((LogicAppPublisher)publisherObject).LogicAppResponse.Headers;
                            IEnumerable<string> values;

                            if (responseheaders.TryGetValues("correlationTrackingId", out values))
                            {
                                trackingId = values.First();
                            }
                            else if (responseheaders.TryGetValues("x-ms-client-tracking-id", out values))
                            {
                                trackingId = values.First();
                            }

                        }
                        else
                        {
                            trackingId = payload.FileName;
                        }
                    }
                    catch (Exception ex)
                    {
                        TraceProvider.WriteLine("Executing test suit run id {0}, error while writing actual file named {1}, error description is {2}", testSuiteRunId, payload.FileName, ex.Message);
                    }
                    RaisePublisherEvent(payload);
                }
            }
        }

        #endregion

        public void ExecuteABTest(JObject testSuiteToProcess)
        {
            testSuite = testSuiteToProcess;
            // Assigning unique testsuit run id 
            testSuiteRunId = testSuite["TestSuiteName"].ToString() + "_" + Guid.NewGuid().ToString();

            //Identify the source of pre and post transform files for ABTest
            string source = String.IsNullOrEmpty(adapterSettings["FileSource"]) ? string.Empty : adapterSettings["FileSource"];


            switch (source.ToUpper())
            {
                case AppConstants.FilesourceFileShare:
                    TraceProvider.WriteLine("Executing test suit run id {0} from source {1}", testSuiteRunId, AppConstants.FilesourceFileShare);
                    ExecuteABtestFromFileshare();
                    break;

                case AppConstants.FilesourceBizTrack:
                    TraceProvider.WriteLine("Executing test suit run id {0} from source {1}", testSuiteRunId, AppConstants.FilesourceBizTrack);
                    ExecuteABTestFromBizTrack();
                    break;

                case AppConstants.FilesourceBlob:
                    TraceProvider.WriteLine("Executing test suit run id {0} from source {1}", testSuiteRunId, AppConstants.FilesourceBlob);
                    ExecuteABTestFromBlob();
                    break;

                default:
                    TraceProvider.WriteLine("Executing test suit run id {0} from source {1}", testSuiteRunId, "FileSource not defined");
                    break;
            }
        }

        

        /// <summary>
        /// Returns publisher object 
        /// </summary>
        /// <param name="publisherType">filepublisher, logicapppublisher, blobpublisher</param>
        /// <param name="fileType"> Expected or Actual</param>
        private IPublisher GetMessagePublisher(string fileType)
        {
            IPublisher publisher = null;

            Dictionary<string, string> publisherSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(testSuite[fileType].ToString());
            if (publisherSettings[AppConstants.PublisherType].ToString().ToLower() == AppConstants.BlobPublisher)
            {
                Uri sasUriOfBlobContainer = new Uri(publisherSettings[AppConstants.BlobUri].ToString());
                publisher = new BlobPublisher(sasUriOfBlobContainer, adapterSettings["ContentType"]);
                TraceProvider.WriteLine("Executing test suit run id {0}, selected publisher is {1} with uri {2}", testSuiteRunId, AppConstants.BlobPublisher, sasUriOfBlobContainer.AbsoluteUri);
            }
            else if (publisherSettings[AppConstants.PublisherType].ToString().ToLower() == AppConstants.FilePublisher)
            {
                publisher = new FilePublisher(HttpUtility.UrlDecode(publisherSettings[AppConstants.FilePath]), adapterSettings["ContentType"]);
                TraceProvider.WriteLine("Executing test suit run id {0}, selected publisher is {1} with path {2}", testSuiteRunId, AppConstants.FilePublisher, HttpUtility.UrlDecode(publisherSettings[AppConstants.FilePath]));
            }
            else if (publisherSettings[AppConstants.PublisherType].ToString().ToLower() == AppConstants.LogicAppPublisher)
            {
                publisher = new LogicAppPublisher(testSuite, adapterSettings["ContentType"]);
                TraceProvider.WriteLine("Executing test suit run id {0}, selected publisher is {1}", testSuiteRunId, AppConstants.LogicAppPublisher);
            }
            else
            {
                TraceProvider.WriteLine("Executing test suit run id {0}, Publisher type is not configured in test suit for {1}", testSuiteRunId, fileType);
            }

            return publisher;
        }

        private void RaisePublisherEvent(Payload payLoadObject)
        {
            omsEvent = new Dictionary<string, string>();
            string testSuiteName = testSuite["TestSuiteName"].ToString();
            omsEvent.Add("testsuitname", testSuiteName);
            omsEvent.Add("testsuitrunid", testSuiteRunId);
            omsEvent.Add("messagetrackingid", trackingId);
            omsEvent.Add("testfilename", payLoadObject.FileName);
            omsEvent.Add("eventTime", DateTime.UtcNow.ToLocalTime().ToString());
            omsEvent.Add("testcasestobevalidated", testSuite["TestCase"].ToString());

            this.telemetryClient.TrackTrace(
                       AppConstants.ApplicationName,
                       trackingId,
                       JsonConvert.SerializeObject(omsEvent, Formatting.Indented),
                       SeverityLevel.Informational);

            TraceProvider.WriteLine("Executing test suit run id {0}, OMS event {1} triggered with tracking id {2} and event source {3}", testSuiteRunId, JsonConvert.SerializeObject(omsEvent, Formatting.Indented), trackingId, AppConstants.ApplicationName);
        }


    }
}
