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

namespace SchemaMigration
{
    public class IntegrationAccountContextForSchemas
    {
        private string AadInstance;
        private string Resource;
        private string Tenant;
        private string ClientId;
        private string RedirectUri;
        private string KeyvaultResource;
        public IntegrationAccountContextForSchemas()
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
        private HttpResponseMessage UploadSchema(AuthenticationResult authResult, string url, string fileLocation, string schemaName)
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
                response = UploadToIA(authResult, url, fileLocation, schemaName);
                return response;
                
            }
            catch (Exception ex)
            {
                try
                {
                    authResult = RefreshAccessToken(authResult.UserInfo, AuthenticationAccessToken.IntegrationAccount);
                    response = UploadToIA(authResult, url, fileLocation, schemaName);
                    return response;
                }
                catch (Exception e)
                {
                    string message = $"ERROR! Exception while putting schema {schemaName} to Integration Account. \nErrorMessage: {e.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                    throw new Exception(message);
                }
            }
        }


        private HttpResponseMessage UploadToIA(AuthenticationResult authResult, string url, string fileLocation, string schemaName)
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
                    content = File.ReadAllText(fileLocation + "\\" + schemaName + ".xsd");
                    var jobj = JObject.Parse("{'properties':{'schemaType':'Xml','content':'null','contentType':'application/xml'},'tags':{'integrationAccountSchemaName':'null'}}");
                    jobj["properties"]["content"] = content;
                    jobj["tags"]["integrationAccountSchemaName"] = schemaName;
                    content = jobj.ToString();

                    HttpContent hc = new StringContent(content, Encoding.UTF8, "application/json");

                    response = client.PutAsync(client.BaseAddress, hc).Result;
                    return response;
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Starting point for upload of schema
        /// </summary>
        /// <param name="fileLocation">Location where schema XSDs are picked from after their extraction</param>
        /// <param name="schemaList">List of schemas to be uploaded</param>
        /// <param name="overrideExistingSchemasFlag">Flag abuot whether to override the existing schemas in the IA</param>
        /// <param name="iaDetails">IA auth details</param>
        /// <param name="schemaDetailsList">List of schemas to be uploaded</param>
        public void SchemaUploadFromFolder(string fileLocation, List<SchemaDetails> schemaList, bool overrideExistingSchemasFlag, IntegrationAccountDetails iaDetails, AuthenticationResult authResult, ref List<SchemaDetails> schemaDetailsList)
        {
            TraceProvider.WriteLine("Extraction of schemas and their dependencies completed. Starting upload to Integration Account...");
            //Console.WriteLine("Extraction of schemas and their dependencies completed. Starting upload to Integration Account...");

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
            foreach (var schemaName in schemaList)
            {
                var url = ConfigurationManager.AppSettings["SchemaRestUrl"];
                var response = new HttpResponseMessage();
                try
                {
                    url = string.Format(url, iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, schemaName.fullNameOfSchemaToUpload);


                    bool exists = CheckIfArtifactExists(url, authResult);

                    if (overrideExistingSchemasFlag)
                    {
                        if (schemaName.isSchemaExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine("Uploading unselected dependency. Schema:" + schemaName.fullNameOfSchemaToUpload);
                            // Console.WriteLine("Uploading unselected dependency. Schema:" + schemaName.fullNameOfSchemaToUpload);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Uploading schema {schemaName.fullNameOfSchemaToUpload}");
                            //Console.WriteLine($"Uploading schema {schemaName.fullNameOfSchemaToUpload}");

                        }
                        response = UploadSchema(authResult, url, fileLocation, schemaName.fullNameOfSchemaToUpload);


                    }
                    else if (exists)
                    {
                        response.StatusCode = System.Net.HttpStatusCode.Conflict;
                        if (schemaName.isSchemaExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine($"Unselected dependent schema {schemaName.fullNameOfSchemaToUpload} already exists");
                            //Console.WriteLine($"Unselected dependent schema {schemaName.fullNameOfSchemaToUpload} already exists");

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Schema {schemaName.fullNameOfSchemaToUpload} already exists");
                            //Console.WriteLine($"Schema {schemaName.fullNameOfSchemaToUpload} already exists");

                        }
                    }
                    else
                    {
                        if (schemaName.isSchemaExtractedFromDb == false)
                        {
                            TraceProvider.WriteLine($"Uploading unselected dependency. Schema:{schemaName.fullNameOfSchemaToUpload}");
                            //Console.WriteLine($"Uploading unselected dependency. Schema:{schemaName.fullNameOfSchemaToUpload}");

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Uploading schema {schemaName}");
                            //Console.WriteLine($"Uploading schema {schemaName}");

                        }
                        response = UploadSchema(authResult, url, fileLocation, schemaName.fullNameOfSchemaToUpload);
                    }

                    // response status code for a particular schema after upload rest operation has been performed on it
                    if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
                    {
                        if (schemaName.isSchemaExtractedFromDb)
                        {
                            string message = $"Conflict for schema {schemaName.fullNameOfSchemaToUpload}. Response:{response.StatusCode}";
                            schemaName.schemaUploadToIAStatus = SchemaUploadToIAStatus.Partial;
                            schemaName.errorDetailsForMigration = schemaName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
                            TraceProvider.WriteLine(message);
                            //Console.WriteLine(message);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Conflict for unselected dependency schema {schemaName.fullNameOfSchemaToUpload}. Response:{response.StatusCode}");
                            //Console.WriteLine($"Conflict for unselected dependency schema {schemaName.fullNameOfSchemaToUpload}. Response:{response.StatusCode}");

                        }
                    }
                    else if (!response.IsSuccessStatusCode)
                    {
                        if (schemaName.isSchemaExtractedFromDb)
                        {
                            string message = $"Failed for schema {schemaName.fullNameOfSchemaToUpload}. ResponseStatusCode:{response}";
                            schemaName.schemaUploadToIAStatus = SchemaUploadToIAStatus.Failure;
                            schemaName.errorDetailsForMigration = schemaName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
                            TraceProvider.WriteLine(message);
                            //Console.WriteLine(message);

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Failed for hidden dependency schema {schemaName.fullNameOfSchemaToUpload}. Response:{response}");
                            //Console.WriteLine($"Failed for hidden dependency schema {schemaName.fullNameOfSchemaToUpload}. Response:{response}");

                        }
                    }
                    else
                    {
                        if (schemaName.isSchemaExtractedFromDb)
                        {
                            schemaName.schemaUploadToIAStatus = SchemaUploadToIAStatus.Success;
                            TraceProvider.WriteLine($"Successfully uploaded schema {schemaName.fullNameOfSchemaToUpload} to Integration Account. Response:{response.StatusCode}");
                            // Console.WriteLine($"Successfully uploaded schema {schemaName.fullNameOfSchemaToUpload} to Integration Account. Response:{response.StatusCode}");

                        }
                        else
                        {
                            TraceProvider.WriteLine($"Successfully uploaded unselected dependent schema {schemaName.fullNameOfSchemaToUpload} to Integration Account. Response: {response.StatusCode}");
                            //Console.WriteLine($"Successfully uploaded unselected dependent schema {schemaName.fullNameOfSchemaToUpload} to Integration Account. Response: {response.StatusCode}");

                        }
                    }

                }
                catch (Exception ex)
                {
                    string message = $"Failed to upload schema {schemaName.fullNameOfSchemaToUpload}. IA Rest API returned a response {response}. \nErrorMessage:{ex.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");

                    schemaName.schemaUploadToIAStatus = SchemaUploadToIAStatus.Failure;
                    schemaName.errorDetailsForMigration = schemaName.errorDetailsForMigration + ". " + message + ". Check logs for details.";
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
              
                response = SendToIA(url,authResult);
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
            catch(Exception ex)
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
        public bool CheckIfArtifactExists(string url, AuthenticationResult  authResult)
        {
            IntegrationAccountContextForSchemas sclient = new IntegrationAccountContextForSchemas();
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
