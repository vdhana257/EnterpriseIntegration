using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace MapMigration
{
    public class IntegrationAccountContextForMaps
    {
        private string AadInstance;
        private string Resource;
        private string Tenant;
        private string ClientId;
        private string RedirectUri;
        private string KeyvaultResource;
        public IntegrationAccountContextForMaps()
        {
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
                throw ex;
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
                throw new Exception(string.Format("Error reading config file.Reason:{0}", ex.Message));
            }

        }
        private UserIdentifier ConvertToUniqueIdentifier(UserInfo info)
        {
            UserIdentifier user = new UserIdentifier(info.UniqueId, UserIdentifierType.UniqueId);
            return user;
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
                throw x;
            }
        }


        /// <summary>
        /// Upload schema to IA
        /// </summary>
        /// <param name="authorizationKey"></param>
        /// <param name="url">Rest URL</param>
        /// <param name="fileLocation">Location from where the schema XSD is to be picked</param>
        /// <param name="schemaName">name of the schema</param>
        /// <returns></returns>
        private HttpResponseMessage UploadMap(AuthenticationResult authResult, string url, string fileLocation, string mapName)
        {

            HttpResponseMessage response = null;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            try
            {
                response = UploadToIA(authResult, url, fileLocation, mapName);
                return response;

            }
            catch (Exception ex)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = UploadToIA(authResult, url, fileLocation, mapName);
                    return response;
                }
                catch (Exception e)
                {
                    string message = $"ERROR! Exception while putting map {mapName} to Integration Account. \nErrorMessage: {e.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                    throw new Exception(message);
                }
            }
        }


        private HttpResponseMessage UploadToIA(AuthenticationResult authResult, string url, string fileLocation, string mapName)
        {

            HttpResponseMessage response = null;
            try
            {
                using (var client = new HttpClient())
                {
                    Uri uri = new Uri(url);
                    string content = "";

                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + authResult.AccessToken);
                    client.Timeout = new TimeSpan(0, 1, 1);
                    client.BaseAddress = uri;
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));//ACCEPT header
                    client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json");
                    HttpRequestMessage requestMessage = new HttpRequestMessage(HttpMethod.Put, client.BaseAddress);
                    content = File.ReadAllText(fileLocation + "\\" + mapName + ".xslt");
                    var jobj = JObject.Parse("{'properties':{'mapType':'Xslt','content':'null','contentType':'application/xml'},'tags':{'integrationAccountMapName':'null'}}");
                    jobj["properties"]["content"] = content;
                    jobj["tags"]["integrationAccountMapName"] = mapName;
                    content = jobj.ToString();

                    HttpContent hc = new StringContent(content, Encoding.UTF8, "application/json");

                    response = client.PutAsync(client.BaseAddress, hc).Result;
                    return response;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Starting point for upload of map
        /// </summary>
        /// <param name="fileLocation">Location where map XSLXs are picked from after their extraction</param>
        /// <param name="schemaList">List of maps to be uploaded</param>
        /// <param name="overrideExistingMapsFlag">Flag about whether to override the existing maps in the IA</param>
        /// <param name="iaDetails">IA auth details</param>
        /// <param name="mapDetailsList">List of schemas to be uploaded</param>
        public void SchemaUploadFromFolder(string fileLocation, List<MapDetails> mapList, bool overrideExistingMapsFlag, IntegrationAccountDetails iaDetails, AuthenticationResult authResult, ref List<MapDetails> mapDetailsList)
        {
            TraceProvider.WriteLine("Extraction of maps. Starting upload to Integration Account...");

            try
            {
                // AAD Authentication - Getting of Token
                if (string.IsNullOrEmpty(authResult.AccessToken))
                {
                    string message = $"ERROR! Problem during AAD authentication. \nErrorMessage: Invalid access token";
                    TraceProvider.WriteLine($"{message}");
                    //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                    throw new Exception(message);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            // Do one by one upload on each schema XSD based on flags - whether to override, whether schema already exists, whether schema got successfully extracted
            foreach (var mapName in mapList)
            {
                var url = ConfigurationManager.AppSettings["MapRestUrl"];
                var response = new HttpResponseMessage();
                try
                {
                    url = string.Format(url, iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, mapName.fullNameOfMapToUpload);


                    bool exists = CheckIfArtifactExists(url, authResult);

                    if (overrideExistingMapsFlag)
                    {
                        if (mapName.isMapExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine("Uploading unselected dependency. Schema:" + mapName.fullNameOfMapToUpload);
                            // Console.WriteLine("Uploading unselected dependency. Schema:" + schemaName.fullNameOfSchemaToUpload);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Uploading map {mapName.fullNameOfMapToUpload}");
                            //Console.WriteLine($"Uploading schema {schemaName.fullNameOfSchemaToUpload}");

                        }
                        response = UploadMap(authResult, url, fileLocation, mapName.fullNameOfMapToUpload);


                    }
                    else if (exists)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.Conflict;
                        if (mapName.isMapExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine($"Unselected dependent schema {mapName.fullNameOfMapToUpload} already exists");
                        }
                        else
                        {
                            TraceProvider.WriteLine($"Schema {mapName.fullNameOfMapToUpload} already exists");
                        }
                    }
                    else
                    {
                        if (mapName.isMapExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine($"Uploading unselected dependency. Map:{mapName.fullNameOfMapToUpload}");
                        }
                        else
                        {
                            TraceProvider.WriteLine($"Uploading map {mapName}");
                        }
                        response = UploadMap(authResult, url, fileLocation, mapName.fullNameOfMapToUpload);
                    }

                    // response status code for a particular map after upload rest operation has been performed on it
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        if (mapName.isMapExtractedFromDb)
                        {
                            string message = $"Conflict for map {mapName.fullNameOfMapToUpload}. Response:{response.StatusCode}";
                            mapName.mapUploadToIAStatus = MapUploadToIAStatus.Partial;
                            mapName.errorDetailsForMigration = mapName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
                            TraceProvider.WriteLine(message);
                            //Console.WriteLine(message);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Conflict for unselected dependency map {mapName.fullNameOfMapToUpload}. Response:{response.StatusCode}");
                        }
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        if (mapName.isMapExtractedFromDb)
                        {
                            string message = $"Failed for map {mapName.fullNameOfMapToUpload}. ResponseStatusCode:{response}";
                            mapName.mapUploadToIAStatus = MapUploadToIAStatus.Failure;
                            mapName.errorDetailsForMigration = mapName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
                            TraceProvider.WriteLine(message);
                            //Console.WriteLine(message);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Failed for hidden dependency map {mapName.fullNameOfMapToUpload}. Response:{response}");

                        }
                    }
                    else
                    {
                        if (mapName.isMapExtractedFromDb)
                        {
                            mapName.mapUploadToIAStatus = MapUploadToIAStatus.Success;
                            TraceProvider.WriteLine($"Successfully uploaded map {mapName.fullNameOfMapToUpload} to Integration Account. Response:{response.StatusCode}");

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Successfully uploaded unselected dependent map {mapName.fullNameOfMapToUpload} to Integration Account. Response: {response.StatusCode}");

                        }
                    }

                }
                catch (Exception ex)
                {
                    string message = $"Failed to upload map {mapName.fullNameOfMapToUpload}. IA Rest API returned a response {response}. \nErrorMessage:{ex.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");

                    mapName.mapUploadToIAStatus = MapUploadToIAStatus.Failure;
                    mapName.errorDetailsForMigration = mapName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
                }
            }
        }

        /// <summary>
        /// To check whether the schema already exists or not in the IA by making a get call for that particular schema and reading through its response
        /// </summary>
        /// <param name="url">make a rest get call to the schema to IA</param>
        /// <param name="authResult">Authentication Access Token</param>
        /// <returns></returns>
        public HttpResponseMessage SendGetRequestToIA(string url, AuthenticationResult authResult)
        {

            HttpResponseMessage response = null;
            try
            {
                InitializeAuthorizationCredentials();
            }
            catch (Exception ex)
            {
                throw ex;
            }
            try
            {

                response = SendToIA(url, authResult);
                return response;

            }
            catch (Exception e)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = SendToIA(url, authResult);
                    return response;
                }
                catch (Exception ex)
                {
                    throw new Exception("Integrationaccount api: " + ex);
                }
            }
            return response;
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
                    return response;
                }

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Retuns true if the schema artifact already exists, false if it does not.
        /// </summary>
        /// <param name="url"></param>
        /// <param name="iaDetails"></param>
        /// <returns></returns>
        public bool CheckIfArtifactExists(string url, AuthenticationResult authResult)
        {
            IntegrationAccountContextForMaps sclient = new IntegrationAccountContextForMaps();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = sclient.SendGetRequestToIA(url, authResult);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

    }
}
