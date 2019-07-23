using ABTestAdapter.Contracts;
using ABTestAdapter.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace ABTestAdapter.Publishers
{
    public class LogicAppPublisher : IPublisher
    {
        private string contentFormat = "STREAM";
        public JObject LARequest { get; set; }
        public HttpResponseMessage LogicAppResponse { get; set; }

        public LogicAppPublisher(JObject testsuite, string contentType)
        {
            LARequest = testsuite;
            contentFormat = contentType;
        }
        public void Publish(Payload payloadObject)
        {
            HttpResponseMessage response;
            using (var httpClient = new HttpClient())
            using (response = new HttpResponseMessage())
            {
                Dictionary<string, string> actualFilePublisherSettings = JsonConvert.DeserializeObject<Dictionary<string, string>>(LARequest["ActualFilePublisher"].ToString());
                string requestEndpoint = actualFilePublisherSettings["LogicAppUri"].ToString();
                string stringToReplace = actualFilePublisherSettings["StringToReplace"].ToString();
                string stringReplaceWith = actualFilePublisherSettings["StringReplaceWith"].ToString();
                httpClient.BaseAddress = new Uri(requestEndpoint);
                httpClient.DefaultRequestHeaders.Accept.Clear();
                Dictionary<string, string> headers = null;
                if (LARequest["Headers"] != null)
                {
                    headers = JsonConvert.DeserializeObject<Dictionary<string, string>>(LARequest["Headers"].ToString());

                    if (headers["AS2-To"].ToString().Contains("TEST") || headers["AS2-To"].ToString().Contains("MICROSOFT"))
                    {
                        headers.Add("flow-direction", "CanaryToMicrosoft");
                        headers.Add("Message-Id", Guid.NewGuid().ToString());
                        headers.Add("ExecutionContext", "{\"FP_MetadataRetrievalApiApp_URL\":\"MetadataRetrievalApiV2App_URL\"}");
                    }
                    foreach (var header in headers)
                    {
                        httpClient.DefaultRequestHeaders.Add(header.Key, header.Value);
                    }

                    switch (actualFilePublisherSettings["HttpVerb"].ToString().ToLower())
                    {
                        case "get":
                            response = httpClient.GetAsync(requestEndpoint).Result;
                            break;
                        case "post":
                            using (var request = new HttpRequestMessage(HttpMethod.Post, string.Empty))
                            {
                                if (contentFormat.ToUpper() == AppConstants.FormatStream)
                                {
                                    string requestContent = Utility.StreamToString(payloadObject.Stream);
                                    requestContent = requestContent.Replace(stringToReplace, stringReplaceWith);
                                    request.Content = new StringContent(requestContent);
                                    response = httpClient.SendAsync(request).Result;
                                    //request.Content = new StreamContent(payloadObject.Stream);
                                    //response = httpClient.SendAsync(request).Result;
                                }
                                else
                                {
                                    string requestContent = Utility.StreamToStringUtf16(payloadObject.Stream);
                                    requestContent = requestContent.Replace(stringToReplace, stringReplaceWith);
                                    request.Content = new StringContent(requestContent);
                                    response = httpClient.SendAsync(request).Result;
                                }
                            }

                            break;
                    }
                }

                LogicAppResponse = response;
            }
        }


    }
}
