using Microsoft.Azure.KeyVault.Models;
using Microsoft.Azure.KeyVault.WebKey;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class CerificateMigrator<TEntity> where TEntity : class
    {
        private IntegrationAccountDetails iaDetails;
        private IApplicationContext thisApplicationContext;

        public CerificateMigrator(IApplicationContext applicationContext)
        {
            this.thisApplicationContext = applicationContext;
        }

        #region Certificate Creation
        public void CreateCertificates(string certName, string certThumbprint)
        {
            try
            {
                X509Certificate2 partnerCert;
                string partnerCertBase64String;
                bool certFound = false;
                X509Certificate2Collection collection = new X509Certificate2Collection();
                //BiztalkServerDetails BizTalkServerDetails = this.thisApplicationContext.GetService<BiztalkServerDetails>();
                //string serverName = BizTalkServerDetails.RemoteServerName;
                //if (BizTalkServerDetails.UseDifferentAccount)
                //{
                //    string domain = BizTalkServerDetails.RemoteDomainName;
                //    string username = BizTalkServerDetails.RemoteUserName;
                //    string password = BizTalkServerDetails.RemoteUserPassword;
                //    using (UserImpersonation user = new UserImpersonation(username, domain, password))
                //    {
                //        if (user.ImpersonateValidUser())
                //        {
                //collection = GetCertificate(certThumbprint, serverName);
                //if (collection.Count == 0)
                //{
                //    certFound = false;
                //}
                //else
                //{
                //    certFound = true;
                //}
                //}
                //else
                //{
                //    throw new Exception(string.Format(@"Failed to read Certificates from the Server {0}\{1}. Invalid Credentails used to read Certificates.", domain, username));
                //}
                // }
                // }
                //else
                //{
                string serverName = System.Environment.MachineName;
                collection = GetCertificate(certThumbprint, serverName);
                if (collection.Count == 0)
                {
                    certFound = false;
                }
                else
                {
                    certFound = true;
                }
                // }
                if (certFound)
                {
                    partnerCert = collection[0];
                    partnerCertBase64String = Convert.ToBase64String(partnerCert.GetRawCertData());
                    JsonCertificate.Rootobject certificateRootObject = new JsonCertificate.Rootobject()
                    {
                        name = FileOperations.GetFileName(certName),
                        properties = new JsonCertificate.Properties()
                        {
                            publicCertificate = partnerCertBase64String,
                            metadata = GenerateMetadata(certName)
                        }
                    };

                    if (partnerCert.HasPrivateKey)
                    {
                        //CHECK IF IT's EXPORTABLE, and ensure you write traces
                        certificateRootObject.properties.key = new JsonCertificate.Key()
                        {
                            keyName = "<KeyNameHere>",
                            keyVersion = "<KeyVersionHere>",
                            keyVault = new JsonCertificate.KeyVault()
                            {
                                id = "<ResourceUriHere>"
                            }
                        };

                        //TODO: Create KeyVault Secret JSON.
                        string keyvaultjson = this.GetKeyVaultSecretJsonForPrivateKey(partnerCert);
                        string keyvaultfilename = FileOperations.GetKeyVaultJsonFilePath(certName + "Privatekey");
                        FileOperations.CreateFolder(keyvaultfilename);
                        System.IO.File.WriteAllText(keyvaultfilename, keyvaultjson);

                    }
                    string fileName = FileOperations.GetCertificateJsonFilePath(certName);
                    string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(certificateRootObject);
                    FileOperations.CreateFolder(fileName);
                    System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                    TraceProvider.WriteLine(string.Format("Certificate {0} for partner has been succesfully exported to Json with Name {1}", certName, FileOperations.GetFileName(certName)));
                }
                else
                {
                    TraceProvider.WriteLine(string.Format("Certificate Export to Json Failed. Reason : Certificate {0} Not Found in LocalCertificate store in Server {1}", certName, serverName));
                    throw new Exception(string.Format("Certificate {0} Not Found in LocalCertificate store in Server {1}", certName, serverName));
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public X509Certificate2Collection GetCertificate(string thumbprint, string serverName)
        {
            X509Certificate2Collection collection = new X509Certificate2Collection();
            string serverPath = string.Concat(@"\\", serverName, @"\");
            try
            {
                foreach (PublicCertificateStore storeName in Enum.GetValues(typeof(PublicCertificateStore)))
                {
                    X509Store store;
                    if (serverName == System.Environment.MachineName)
                    {
                        store = new X509Store(storeName.ToString(), StoreLocation.LocalMachine);
                    }
                    else
                    {
                        store = new X509Store(serverPath + storeName.ToString(), StoreLocation.LocalMachine);
                    }
                    store.Open(OpenFlags.ReadOnly);
                    collection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
                    //"C11361C208FFD742E8B1F78F9923B829016688BE"
                    store.Close();
                    if (collection.Count != 0)
                    {
                        break;

                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Failed to read certificates from server " + serverName + " : " + ExceptionHelper.GetExceptionMessage(ex));
            }
            return collection;
        }
        #endregion



        #region KeyValult
        private string GetKeyVaultSecretJsonForPrivateKey(X509Certificate2 certificate)
        {
            try
            {
                using (RSA rsa = (RSA)certificate.PrivateKey)
                {
                    var parameters = rsa.ExportParameters(true);

                    KeyBundle bundle = new KeyBundle
                    {
                        Key = new JsonWebKey
                        {
                            Kty = JsonWebKeyType.Rsa,
                            // Private stuff
                            D = parameters.D,
                            DP = parameters.DP,
                            DQ = parameters.DQ,
                            P = parameters.P,
                            Q = parameters.Q,
                            QI = parameters.InverseQ,
                            // Public stuff
                            N = parameters.Modulus,
                            E = parameters.Exponent,
                        },
                    };

                    KeyAttributes attributes = new KeyAttributes
                    {
                        Enabled = true,
                        Expires = DateTime.Parse(certificate.GetExpirationDateString())
                    };

                    Dictionary<string, object> body = new Dictionary<string, object>();
                    body.Add("attributes", attributes);
                    body.Add("key", bundle.Key);

                    JObject obj = JObject.FromObject(body);
                    return obj.ToString();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Private certificate set as non-exportable : " + ex.Message);
            }

        }
        #endregion

        public Metadata GenerateMetadata(string originalname)
        {
            Metadata metadata = new Metadata()
            {
                migrationInfo = new MigrationAuditInformation()
                {
                    originalName = originalname
                }
            };
            return metadata;
        }

        public async Task<bool> CheckIfCertificateExists(string migrationItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                response = sclient.GetArtifactsFromIA(UrlHelper.GetCertificateUrl(migrationItem, iaDetails), authResult);
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        public void SetStatus(TEntity item, MigrationStatus status, string statusText)
        {
            var serverPartnerItem = item as PartnerMigrationItemViewModel;
            var certItem = item as Certificate;
            if(serverPartnerItem != null)
            {
                serverPartnerItem.CertificateExportStatus = status;
                serverPartnerItem.CertificateExportStatusText = statusText;
            }
            else
            {
                certItem.ExportStatus = status;
                certItem.ExportStatusText = statusText;
            }
        }
        public async Task ExportToIA(TEntity item, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            var serverPartnerItem = item as PartnerMigrationItemViewModel;
            var certItem = item as Certificate;
            string certName = string.Empty;
            if(serverPartnerItem != null)
            {
                certName = serverPartnerItem.MigrationEntity.CertificateName;
            }
            else if(certItem != null)
            {
                certName = certItem.certificateName;
            }
            try
            {
                TraceProvider.WriteLine("Migrating certificate : {0}", certName);
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        bool overwrite = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
                        if (!overwrite)
                        {
                            bool exists = CheckIfCertificateExists(certName, this.iaDetails, authresult).Result;
                            if (exists)
                            {
                                SetStatus(item,MigrationStatus.Partial, string.Format("The Certificate {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the certificate was not overwritten.", certName, FileOperations.GetFileName(certName)));
                                TraceProvider.WriteLine(string.Format("The Certificate {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the certificate was not overwritten.", certName, FileOperations.GetFileName(certName)));
                                TraceProvider.WriteLine();
                            }
                            else
                            {
                                CheckIfCertificateIsprivate(item, certName,iaDetails);
                                MigrateToCloudIA(FileOperations.GetCertificateJsonFilePath(certName), FileOperations.GetFileName(certName), item, iaDetails, authresult).Wait();
                                SetStatus(item,MigrationStatus.Succeeded, string.Format("Certificate {0} migrated succesfully", certName));
                                TraceProvider.WriteLine(string.Format("Certificate Migration Successfull: {0}", certName));
                                TraceProvider.WriteLine();
                            }
                        }
                        else
                        {
                            CheckIfCertificateIsprivate(item, certName, iaDetails);
                            MigrateToCloudIA(FileOperations.GetCertificateJsonFilePath(certName), FileOperations.GetFileName(certName), item, iaDetails, authresult).Wait();
                            SetStatus(item, MigrationStatus.Succeeded, string.Format("Certificate {0} migrated succesfully", certName));
                            TraceProvider.WriteLine(string.Format("Certificate Migration Successfull: {0}", certName));
                            TraceProvider.WriteLine();
                        }
                    }
                    catch (Exception ex)
                    {
                        //throw ex;
                    }

                });

            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

         public void CheckIfCertificateIsprivate(TEntity item,string certificateName, IntegrationAccountDetails iaDetails)
        {
            try
            {
                string filepath = FileOperations.GetCertificateJsonFilePath(certificateName);
                string content = File.ReadAllText(filepath);
                var certificate = JsonConvert.DeserializeObject<JsonCertificate.Rootobject>(content);
                if (certificate.properties.key != null)
                {
                    string keyvaultname = iaDetails.KeyVaultName;
                    if(string.IsNullOrEmpty(keyvaultname))
                    {
                        throw new Exception("Couldn't find the name of the Key Vault to upload the Private key for the Private Certificate. Make sure you selected a Key Vault at the Integration Account Details Screen");
                    }
                    string certName = FileOperations.GetFileName(certificateName);
                    string keyvaultfilepath = FileOperations.GetKeyVaultJsonFilePath(certName + "Privatekey");
                    IntegrationAccountContext sclient = new IntegrationAccountContext();
                    AuthenticationResult authresult = thisApplicationContext.GetProperty("KeyVaultAuthorization") as AuthenticationResult;
                    string kid = sclient.UploadCertificatePrivateKeyToKeyVault(StringOperations.RemoveAllSpecialCharacters(certName) + "Privatekey", keyvaultname, keyvaultfilepath, authresult);
                    certificate.properties.key.keyName = StringOperations.RemoveAllSpecialCharacters(certName) + "Privatekey";
                    certificate.properties.key.keyVersion = kid;
                    certificate.properties.key.keyVault.id = string.Format(ConfigurationManager.AppSettings["KeyVaultResourceIdTemplate"], iaDetails.SubscriptionId, iaDetails.ResourceGroupName, keyvaultname);
                    //certificate.properties.key.keyVault.name = certName + "Privatekey";

                    string fileName = FileOperations.GetCertificateJsonFilePath(certName);
                    string partnerJsonFileContent = JsonConvert.SerializeObject(certificate);
                    File.WriteAllText(fileName, partnerJsonFileContent);
                }
            }
            catch (Exception ex)
            {
                SetStatus(item, MigrationStatus.Failed, string.Format("Certificate Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine(string.Format("Certificate Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public async Task<HttpResponseMessage> MigrateToCloudIA(string filePath, string name, TEntity item, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                var x = await sclient.LAIntegrationFromFile(UrlHelper.GetCertificateUrl(name, iaDetails), filePath, authResult);
                return x;
            }
            catch (Exception ex)
            {
                SetStatus(item,MigrationStatus.Failed, string.Format("Certificate Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine(string.Format("Certificate Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }

        }
    }
}
