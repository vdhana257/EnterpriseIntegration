using Microsoft.Azure;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.LogicConfiguration
{
    public class LogicAppsContext
    {
        private string AadInstance;
        private string Resource;
        private string Tenant;
        private string ClientId;
        private string RedirectUri;

        private UserIdentifier ConvertToUniqueIdentifier(UserInfo info)
        {
            UserIdentifier user = new UserIdentifier(info.UniqueId, UserIdentifierType.UniqueId);
            return user;
        }

        private void ReadConfig()
        {
            try
            {
                StringBuilder exception = new StringBuilder(string.Empty);
                AadInstance = ConfigurationManager.AppSettings["Config_AADInstance"];
                if (string.IsNullOrEmpty(AadInstance))
                {
                    exception.AppendLine("AAD Instance not present in config file");
                }
                Resource = ConfigurationManager.AppSettings["Config_Resource"];
                if (string.IsNullOrEmpty(Resource))
                {
                    exception.AppendLine("Resource not present in config file");
                }
                Tenant = ConfigurationManager.AppSettings["Config_Tenant"];
                if (string.IsNullOrEmpty(Tenant))
                {
                    exception.AppendLine("Tenant not present in config file");
                }
                ClientId = ConfigurationManager.AppSettings["Config_ClientID"];
                if (string.IsNullOrEmpty(ClientId))
                {
                    exception.AppendLine("ClientId not present in config file");
                }
                RedirectUri = ConfigurationManager.AppSettings["Config_RedirectUri"];
                 if (string.IsNullOrEmpty(RedirectUri))
                  {
                       exception.AppendLine("Reditect URI not present in config file");
                  }
                if (exception.ToString() != "")
                {
                    throw new Exception("\n" + exception.ToString());
                }
            }
            catch (Exception ex)
            {
                throw (ex);
            }
        }

        private void InitializeAuthorizationCredentials()
        {
            try
            {
                ReadConfig();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Error reading config file.Reason:{0}", ex));
            }
        }

        private AuthenticationResult RefreshAccessToken(UserInfo userInfo)
        {
            AuthenticationResult result = null;
            UserIdentifier userId = ConvertToUniqueIdentifier(userInfo);
            try
            {
                var authority = $"{AadInstance}{Tenant}";
                var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);

                //var authparams = new PlatformParameters(PromptBehavior.Auto);
                result = authContext.AcquireTokenSilentAsync(Resource, ClientId, userId).Result;
                if (result != null && !string.IsNullOrEmpty(result.AccessToken))
                {
                    return result;
                }
                else
                {
                    throw new Exception("Failed to retrieve the access token");
                }
            }
            catch (Exception x)
            {
                throw (x);
            }
        }

        private async Task<AuthenticationResult> GetAccessToken()
        {
            AuthenticationResult authResult = null;
            try
            {
                var authority = $"{AadInstance}{Tenant}";
                var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
                //PromptBehavior
                //Auto == prompt only when necessary (use cached token if it exists)
                //Always == useful for debug as you will always have to authenticate
                var authParms = new PlatformParameters(PromptBehavior.SelectAccount);
                authResult = authContext.AcquireTokenAsync(Resource, ClientId, new Uri(RedirectUri), authParms).Result;
                if (authResult != null && !string.IsNullOrEmpty(authResult.AccessToken))
                {
                    return authResult;
                }
                else
                {
                    throw new Exception("Failed to retrieve the access token");
                }
            }
            catch (Exception x)
            {
                throw (x);
            }

        }

        public AuthenticationResult GetAccessTokenFromAAD()
        {
            try
            {
                InitializeAuthorizationCredentials();
                AuthenticationResult accessTokens = GetAccessToken().Result;
                return accessTokens;
            }
            catch (Exception)
            {
                MessageBox.Show("Error in acquiring access token!  ");
                return null;
            }
        }

        public List<Subscriptions> GetSubscription(string URI, string token)
        {
            Uri uri = new Uri(String.Format(URI));

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            // Get the response
            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error from : " + uri + ": " + ex.Message,
                                "HttpWebResponse exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string result = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            JsonList subs = new JsonList(result);
            List<Subscriptions> res = new List<Subscriptions>();
            foreach (var json in subs.subString)
            {
                string js = JsonConvert.SerializeObject(json);
                Subscriptions temp = new Subscriptions(js);
                res.Add(temp);

            }

            return res;
        }


        public List<ResourceGroups> GetResourceGroups(string URI, string token)
        {
            Uri uri = new Uri(String.Format(URI));

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            // Get the response
            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error from : " + uri + ": " + ex.Message,
                                "HttpWebResponse exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string result = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            JsonList subs = new JsonList(result);
            List<ResourceGroups> res = new List<ResourceGroups>();
            foreach (var json in subs.subString)
            {
                string js = JsonConvert.SerializeObject(json);
                ResourceGroups temp = new ResourceGroups(js);
                res.Add(temp);

            }

            return res;
        }

        public List<Workflows> GetWorkflows(string URI, string token)
        {
            Uri uri = new Uri(String.Format(URI));

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            // Get the response
            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error from : " + uri + ": " + ex.Message,
                                "HttpWebResponse exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string result = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            JsonList subs = new JsonList(result);
            List<Workflows> res = new List<Workflows>();
            foreach (var json in subs.subString)
            {
                string js = JsonConvert.SerializeObject(json);
                Workflows temp = new Workflows(js);
                res.Add(temp);

            }

            return res;
        }

        public List<WorkflowVersion> GetWorkflowVersions(string URI, string token)
        {
            Uri uri = new Uri(String.Format(URI));

            // Create the request
            var httpWebRequest = (HttpWebRequest)WebRequest.Create(uri);
            httpWebRequest.Headers.Add(HttpRequestHeader.Authorization, "Bearer " + token);
            httpWebRequest.ContentType = "application/json";
            httpWebRequest.Method = "GET";

            // Get the response
            HttpWebResponse httpResponse = null;
            try
            {
                httpResponse = (HttpWebResponse)httpWebRequest.GetResponse();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error from : " + uri + ": " + ex.Message,
                                "HttpWebResponse exception", MessageBoxButton.OK, MessageBoxImage.Error);
                return null;
            }

            string result = null;
            using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
            {
                result = streamReader.ReadToEnd();
            }

            JsonList subs = new JsonList(result);
            List<WorkflowVersion> res = new List<WorkflowVersion>();
            foreach (var json in subs.subString)
            {
                string js = JsonConvert.SerializeObject(json);
                WorkflowVersion temp = new WorkflowVersion(js);
                res.Add(temp);

            }

            return res;
        }


    }
}
