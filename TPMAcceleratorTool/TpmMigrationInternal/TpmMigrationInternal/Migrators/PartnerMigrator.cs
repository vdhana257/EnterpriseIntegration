//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Data.Services.Client;
    using System.Security.Cryptography.X509Certificates;
    using System.Threading.Tasks;
    using System.Linq;
    using System.Collections.Generic;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;
    using System.Net.Http;
    using Common;
    using System.Text;
    using System.IO;
    using System.Configuration;
    using Newtonsoft.Json.Linq;
    using Newtonsoft.Json;
    using Microsoft.Azure.KeyVault.Models;
    using Microsoft.Azure.KeyVault.WebKey;
    using System.Security.Cryptography;
    using System.Net.Http.Headers;
    using IdentityModel.Clients.ActiveDirectory;

    class PartnerMigrator : TpmMigrator<PartnerMigrationItemViewModel, Server.Partner>
    {
        private IntegrationAccountDetails iaDetails;
        private IApplicationContext thisApplicationContext;

        public bool certificateRequired = false;
        public MigrationStatus succesfullCertificateImport = MigrationStatus.Failed;
        public PartnerMigrator(IApplicationContext applicationContext) : base(applicationContext)
        {
            this.thisApplicationContext = applicationContext;
        }

        public override async Task ImportAsync(PartnerMigrationItemViewModel serverPartnerItem)
        {
            try
            {
                var serverPartner = serverPartnerItem.MigrationEntity;
                TraceProvider.WriteLine();
                TraceProvider.WriteLine("Exporting Partner to Json: {0}", serverPartnerItem.MigrationEntity.Name);
                serverPartnerItem.ImportStatus = MigrationStatus.NotStarted;
                serverPartnerItem.ExportStatus = MigrationStatus.NotStarted;
                serverPartnerItem.ImportStatusText = null;
                serverPartnerItem.ExportStatusText = null;
                serverPartnerItem.CertificationRequired = false;
                serverPartnerItem.CertificateImportStatus = MigrationStatus.NotStarted;
                serverPartnerItem.CertificateExportStatus = MigrationStatus.NotStarted;
                serverPartnerItem.ExportStatusText = null;
                await Task.Factory.StartNew(() =>
                    {
                        try
                        {
                            CreatePartners(serverPartnerItem);
                            serverPartnerItem.ImportStatus = MigrationStatus.Succeeded;
                            StringBuilder successMessageText = new StringBuilder();
                            successMessageText.Append(string.Format(Resources.ImportSuccessMessageText, serverPartnerItem.MigrationEntity.Name));
                            serverPartnerItem.ImportStatusText = successMessageText.ToString();
                            serverPartnerItem.CertificationRequired = certificateRequired;
                            serverPartnerItem.CertificateImportStatus = succesfullCertificateImport;
                            TraceProvider.WriteLine(string.Format("Partner {0} exported to Json with Name {1}", serverPartnerItem.MigrationEntity.Name, FileOperations.GetFileName(serverPartnerItem.MigrationEntity.Name)));
                            TraceProvider.WriteLine(string.Format("Partner Export to Json Successfull: {0}", serverPartner.Name));
                            TraceProvider.WriteLine();
                        }
                        catch(Exception ex)
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

        

        #region Partner Creation
        public JsonPartner.Rootobject CreatePartners(PartnerMigrationItemViewModel partnerItem)
        {
            var partner = partnerItem.MigrationEntity;
            string partnerName = partner.Name;
            JsonPartner.Rootobject partnerRootObject = new JsonPartner.Rootobject();
            #region Certificates
            try
            {
                if (partner.CertificateName != null && partner.CertificateThumbprint != null)
                {
                    try
                    {
                        certificateRequired = true;
                        CerificateMigrator<PartnerMigrationItemViewModel> cMigrator = new CerificateMigrator<PartnerMigrationItemViewModel>(thisApplicationContext);
                        cMigrator.CreateCertificates(partner.CertificateName, partner.CertificateThumbprint);
                        succesfullCertificateImport = MigrationStatus.Succeeded;

                    }
                    catch (Exception ex)
                    {
                        certificateRequired = true;
                        succesfullCertificateImport = MigrationStatus.Failed;
                    }

                }
                else
                {
                    certificateRequired = false;
                    succesfullCertificateImport = MigrationStatus.NotStarted;
                    TraceProvider.WriteLine(string.Format("No certificate is configured with the Partner"));
                }
            }
            catch (Exception ex)
            {
                certificateRequired = true;
                succesfullCertificateImport = MigrationStatus.Failed;
                TraceProvider.WriteLine(string.Format("Certificate Export to Json Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
            }
            #endregion

            #region Partner
            try
            {
                var businessProfile = partner.GetBusinessProfiles().ToList<Server.BusinessProfile>();
                List<Microsoft.BizTalk.B2B.PartnerManagement.BusinessIdentity> businessIdentities = new List<Microsoft.BizTalk.B2B.PartnerManagement.BusinessIdentity>();

                foreach (var item in businessProfile)
                {
                    businessIdentities.AddRange(item.GetBusinessIdentities().ToList());
                }

                partnerRootObject = new JsonPartner.Rootobject()
                {
                    id = "",
                    name = FileOperations.GetFileName(partnerName),
                    type = Resources.JsonPartnerType,
                    properties = new JsonPartner.Properties()
                    {
                        partnerType = Resources.JsonPartnerTypeB2B,
                        content = new JsonPartner.Content()
                        {
                            b2b = new JsonPartner.B2b()
                            {
                                businessIdentities = (businessIdentities.Select(delegate (BizTalk.B2B.PartnerManagement.BusinessIdentity bi)
                                {
                                    return new JsonPartner.Businessidentity()
                                    {
                                        qualifier = ((Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)bi).Qualifier,
                                        value = ((Microsoft.BizTalk.B2B.PartnerManagement.QualifierIdentity)bi).Value

                                    };
                                }
                              )).ToArray()
                            }
                        },
                        createdTime = DateTime.Now,
                        changedTime = DateTime.Now,
                        metadata = GenerateMetadata(partnerName)
                    }
                };
                string directroyPathForJsonFiles = Resources.JsonPartnerFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(partnerName), ".json");
                string partnerJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(partnerRootObject);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, partnerJsonFileContent);
                return partnerRootObject;
            }
            catch (Exception ex)
            {
                partnerItem.ImportStatus = MigrationStatus.Failed;
                partnerItem.ImportStatusText = ex.Message;
                TraceProvider.WriteLine(string.Format("Partner Export to Json Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
            #endregion
        }
        #endregion

        

        

        #region Export to IA      
        public override async Task ExportToIA(PartnerMigrationItemViewModel serverPartnerItem, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            try
            {
                TraceProvider.WriteLine("Migrating partner : {0}", serverPartnerItem.MigrationEntity.Name);
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        bool overwrite = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
                        if (!overwrite)
                        {
                            bool exists = CheckIfArtifactExists(serverPartnerItem.MigrationEntity.Name, "Partner", this.iaDetails,authresult).Result;
                            if (exists)
                            {
                                serverPartnerItem.ExportStatus = MigrationStatus.Partial;
                                serverPartnerItem.ExportStatusText = string.Format("The Partner {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the partner was not overwritten.", serverPartnerItem.MigrationEntity.Name, FileOperations.GetFileName(serverPartnerItem.MigrationEntity.Name));
                                TraceProvider.WriteLine(serverPartnerItem.ExportStatusText);
                                TraceProvider.WriteLine();
                            }
                            else
                            {
                                MigrateToCloudIAPartner(FileOperations.GetPartnerJsonFilePath(serverPartnerItem.MigrationEntity.Name), FileOperations.GetFileName(serverPartnerItem.MigrationEntity.Name), serverPartnerItem, iaDetails, authresult).Wait();
                                serverPartnerItem.ExportStatus = MigrationStatus.Succeeded;
                                StringBuilder successMessageText = new StringBuilder();
                                successMessageText.Append(string.Format(Resources.ExportSuccessMessageText, serverPartnerItem.MigrationEntity.Name));
                                serverPartnerItem.ExportStatusText = successMessageText.ToString();
                                TraceProvider.WriteLine(string.Format("Partner Migration Successfull: {0}", serverPartnerItem.MigrationEntity.Name));
                                TraceProvider.WriteLine();
                            }
                        }
                        else
                        {
                            MigrateToCloudIAPartner(FileOperations.GetPartnerJsonFilePath(serverPartnerItem.MigrationEntity.Name), FileOperations.GetFileName(serverPartnerItem.MigrationEntity.Name), serverPartnerItem, iaDetails, authresult).Wait();
                            serverPartnerItem.ExportStatus = MigrationStatus.Succeeded;
                            StringBuilder successMessageText = new StringBuilder();
                            successMessageText.Append(string.Format(Resources.ExportSuccessMessageText, serverPartnerItem.MigrationEntity.Name));
                            serverPartnerItem.ExportStatusText = successMessageText.ToString();
                            TraceProvider.WriteLine(string.Format("Partner Migration Successfull: {0}", serverPartnerItem.MigrationEntity.Name));
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
               //throw ex;
            }

        }
        #endregion


        #region Migrate To Cloud IA
        public async Task<HttpResponseMessage> MigrateToCloudIAPartner(string filePath, string name, PartnerMigrationItemViewModel serverPartnerItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                try
                {
                    List<KeyValuePair<string, string>> partnerIdentities = new List<KeyValuePair<string, string>>();
                    var partnerInIA = sclient.GetArtifactsFromIA(UrlHelper.GetPartnerUrl(name, iaDetails), authResult);
                    string responseAsString = await partnerInIA.Content.ReadAsStringAsync();
                    JObject responseAsJObject = JsonConvert.DeserializeObject<JObject>(responseAsString);
                    var partner = responseAsJObject;
                    if (partner["properties"] != null && partner["properties"]["content"] != null && partner["properties"]["content"]["b2b"] != null && partner["properties"]["content"]["b2b"]["businessIdentities"] != null)
                    {
                        foreach (var identity in partner["properties"]["content"]["b2b"]["businessIdentities"])
                        {
                            if (identity["qualifier"] != null && identity["value"] != null)
                            {
                                KeyValuePair<string, string> kvpair = new KeyValuePair<string, string>(identity["qualifier"].ToString(), identity["value"].ToString());
                                if (!partnerIdentities.Contains(kvpair))
                                {
                                    partnerIdentities.Add(kvpair);
                                }
                            }
                        }
                    }
                    var partnersFromLocalFile = File.ReadAllText(filePath);
                    var partnerContent = JsonConvert.DeserializeObject<JsonPartner.Rootobject>(partnersFromLocalFile);
                    var partnerIdentityDict = partnerContent.properties.content.b2b.businessIdentities.ToDictionary(xc => new KeyValuePair<string,string>(xc.qualifier,xc.value), xc => xc.value);
                    var partnerIdentityList = partnerContent.properties.content.b2b.businessIdentities.ToList();
                    foreach (var identity in partnerIdentities)
                    {
                        if (!partnerIdentityDict.ContainsKey(identity))
                        {
                            partnerIdentityDict.Add(identity, identity.Value);
                            partnerIdentityList.Add(new JsonPartner.Businessidentity { qualifier = identity.Key, value = identity.Value });
                        }
                    }
                    partnerContent.properties.content.b2b.businessIdentities = partnerIdentityList.ToArray();
                    var finalcontent = JsonConvert.SerializeObject(partnerContent);
                    File.WriteAllText(filePath, finalcontent);

                }
                catch(Exception ex)
                {
                    //do nothing
                }
                var x = await sclient.LAIntegrationFromFile(UrlHelper.GetPartnerUrl(name, iaDetails), filePath, authResult);
                return x;

            }
            catch(Exception ex)
            {
                serverPartnerItem.ExportStatus = MigrationStatus.Failed;
                serverPartnerItem.ExportStatusText = ex.Message;
                TraceProvider.WriteLine(string.Format("Partner Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
        }
        #endregion
    }
}