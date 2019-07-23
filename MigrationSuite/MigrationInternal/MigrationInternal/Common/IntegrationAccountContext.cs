using Microsoft.Azure;
using Microsoft.Azure.KeyVault.Models;
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

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Common
{
    public class IntegrationAccountContext
    {
        private  string AadInstance;
        private  string Resource;
        private  string Tenant;
        private  string ClientId;
        private string RedirectUri;
        private  string KeyvaultResource;


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
                AadInstance = ConfigurationManager.AppSettings["AADInstance"];
                if (string.IsNullOrEmpty(AadInstance))
                {
                    exception.AppendLine("AAD Instance not present in config file");
                }
                Resource = ConfigurationManager.AppSettings["Resource"];
                if (string.IsNullOrEmpty(Resource))
                {
                    exception.AppendLine("Resource not present in config file");
                }
                Tenant = ConfigurationManager.AppSettings["Tenant"];
                if (string.IsNullOrEmpty(Tenant))
                {
                    exception.AppendLine("Tenant not present in config file");
                }
                ClientId = ConfigurationManager.AppSettings["ClientID"];
                if (string.IsNullOrEmpty(ClientId))
                {
                    exception.AppendLine("ClientId not present in config file");
                }
                RedirectUri = ConfigurationManager.AppSettings["RedirectUri"];
                if (string.IsNullOrEmpty(RedirectUri))
                {
                    exception.AppendLine("Reditect URI not present in config file");
                }
                KeyvaultResource = ConfigurationManager.AppSettings["KeyVaultResource"];
                if (string.IsNullOrEmpty(KeyvaultResource))
                {
                    exception.AppendLine("Keyvault Resource not present in config file");
                }
                if (exception.ToString() != "")
                {
                    throw new Exception("\n" + exception.ToString());
                }
            }
            catch (Exception ex)
            {
                throw new Exception (ExceptionHelper.GetExceptionMessage(ex));
            }
        }
        private void InitializeAuthorizationCredentials()
        {
            try
            {
                ReadConfig();
            }
            catch(Exception ex)
            {
                throw new Exception(string.Format("Error reading config file.Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
        }

        private AuthenticationResult RefreshAccessToken(UserInfo userInfo, AuthenticationAccessToken type)
        {
            AuthenticationResult result = null;
            UserIdentifier userId = ConvertToUniqueIdentifier(userInfo);
            try
            {
                var authority = $"{AadInstance}{Tenant}";
                var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);

                //var authparams = new PlatformParameters(PromptBehavior.Auto);
                string resource = type == AuthenticationAccessToken.IntegrationAccount ? Resource : KeyvaultResource;
                result = authContext.AcquireTokenSilentAsync(resource, ClientId, userId).Result;
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
                throw new Exception(ExceptionHelper.GetExceptionMessage(x));
            }
        }

        private async Task<Dictionary<AuthenticationAccessToken, AuthenticationResult>> GetAccessToken()
        {
            Dictionary < AuthenticationAccessToken,AuthenticationResult > authResult = new Dictionary<AuthenticationAccessToken, AuthenticationResult>();
            AuthenticationResult result1 = null;
            AuthenticationResult result2 = null;
            try
            {
                var authority = $"{AadInstance}{Tenant}";
                var authContext = new AuthenticationContext(authority, TokenCache.DefaultShared);
                //PromptBehavior
                //Auto == prompt only when necessary (use cached token if it exists)
                //Always == useful for debug as you will always have to authenticate
                var authParms = new PlatformParameters(PromptBehavior.SelectAccount);
                var keyvaultauthparams = new PlatformParameters(PromptBehavior.Auto);
                result1 = authContext.AcquireTokenAsync(Resource, ClientId, new Uri(RedirectUri), authParms).Result;
                result2 = authContext.AcquireTokenAsync(KeyvaultResource, ClientId, new Uri(RedirectUri), keyvaultauthparams).Result;
                if (result1 != null && result2 != null && !string.IsNullOrEmpty(result1.AccessToken) && !string.IsNullOrEmpty(result2.AccessToken))
                {
                    authResult.Add(AuthenticationAccessToken.IntegrationAccount, result1);
                    authResult.Add(AuthenticationAccessToken.KeyVault, result2);
                    return authResult;
                }
                else
                {
                    throw new Exception("Failed to retrieve the access token");
                }
            }
            catch (Exception x)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(x));
            }

        }

        public Dictionary<AuthenticationAccessToken, AuthenticationResult> GetAccessTokenFromAAD()
        {
            try
            {
                InitializeAuthorizationCredentials();
                var accessTokens = GetAccessToken().Result;
                return accessTokens;
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
        }

        public HttpResponseMessage SendSyncGetRequestToIA(string url, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch(Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
            try
            {
                response = SendToIA(url, authResult);
                return response;
            }
            catch (Exception)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = SendToIA(url, authResult);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Integrationaccount api: " + ExceptionHelper.GetExceptionMessage(ex));
                }
            }
        }

        private HttpResponseMessage SendToIA(string url, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri uri = new Uri(url);
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                    client.Timeout = new TimeSpan(0, 1, 1);
                    response = client.GetAsync(uri).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(response.ReasonPhrase);
                    }
                }
                return response;

            }

            catch(Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
        }

        public async Task<HttpResponseMessage> LAIntegrationFromFile(string url, string fileLocation, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
            try
            {
                response = UploadToIA(url, fileLocation, authResult).Result;
                return response;

            }
            catch (Exception)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = UploadToIA(url, fileLocation, authResult).Result;
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Integrationaccount api: " + ExceptionHelper.GetExceptionMessage(ex));
                }
            }
        }


        private async Task<HttpResponseMessage> UploadToIA(string url, string fileLocation, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri uri = new Uri(url);
                    string content = "";

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, "relativeAddress");
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                    client.Timeout = new TimeSpan(0, 1, 1);

                    content = File.ReadAllText(fileLocation);
                    HttpContent hc = new StringContent(content, Encoding.UTF8, "application/json");
                    response = await client.PutAsync(client.BaseAddress, hc);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new TaskCanceledException("Status Code: " + response.StatusCode);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
        }

        public async Task<HttpResponseMessage> DeleteFromIA(string url, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri uri = new Uri(url);
                    //string content = "";

                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Delete, "relativeAddress");
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                    client.Timeout = new TimeSpan(0, 1, 1);

                    response = await client.DeleteAsync(client.BaseAddress);
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new TaskCanceledException("Status Code: " + response.StatusCode);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("Failed to delete original agreement from IA. Reason {0} ",ExceptionHelper.GetExceptionMessage(ex)));
            }
        }

        public HttpResponseMessage GetArtifactsFromIA(string url, AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
            try
            {
                response = GetFromIA(url, authResult);
                return response;
            }
            catch (Exception)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = GetFromIA(url, authResult);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Integrationaccount api: " + ExceptionHelper.GetExceptionMessage(ex));
                }
            }
        }

        private  HttpResponseMessage GetFromIA(string url,AuthenticationResult authResult)
        {
            HttpResponseMessage response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri uri = new Uri(url);
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                    client.Timeout = new TimeSpan(0, 1, 1);
                    response = client.GetAsync(uri).Result;
                    if (!response.IsSuccessStatusCode)
                    {
                        throw new Exception(response.ReasonPhrase);
                    }
                }
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
        }

        public string UploadCertificatePrivateKeyToKeyVault(string secretName, string keyVaultName, string keyvaultFilePath, AuthenticationResult authResult)
        {
            string response;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch (Exception ex)
            {
                throw new Exception(ExceptionHelper.GetExceptionMessage(ex));
            }
            try
            {
                response = UploadPrivateKeyToIA(secretName, keyVaultName, keyvaultFilePath, authResult);
                return response;
            }
            catch (Exception)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = UploadPrivateKeyToIA(secretName, keyVaultName, keyvaultFilePath, authResult);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Integrationaccount api: " + ExceptionHelper.GetExceptionMessage(ex));
                }
            }
        }

        private string UploadPrivateKeyToIA(string secretName, string keyVaultName, string keyvaultFilePath, AuthenticationResult authResult)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    string keyvaultSecretBody = File.ReadAllText(keyvaultFilePath);
                    string url = string.Format(ConfigurationManager.AppSettings["KeyVaultUrlFormat"], keyVaultName, secretName);
                    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Put, url);
                    request.Headers.Add("Authorization", "Bearer " + authResult.AccessToken);
                    request.Content = new StringContent(keyvaultSecretBody, Encoding.UTF8, "application/json");
                    var resp = client.SendAsync(request).Result;
                    if (resp.StatusCode == HttpStatusCode.OK)
                    {
                        var kid = JObject.Parse(resp.Content.ReadAsStringAsync().Result)["key"]["kid"].ToString();
                        var keyVersion = kid.Split('/').Last();
                        return keyVersion;
                    }
                    else
                    {
                        throw new KeyVaultErrorException();
                    }
                }

            }
            catch(Exception ex)
            {
                throw new Exception("Integrationaccount api: " + ExceptionHelper.GetExceptionMessage(ex));
            }
        }

    }
}
