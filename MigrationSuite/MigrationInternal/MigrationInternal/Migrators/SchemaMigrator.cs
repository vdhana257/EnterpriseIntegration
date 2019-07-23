
using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using SchemaMigration;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Common;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class SchemaMigrator : TpmMigrator<SchemaMigrationItemViewModel, SchemaDetails>
    {
        private IntegrationAccountDetails iaDetails;
        private IApplicationContext thisApplicationContext;

        public SchemaMigrator(IApplicationContext applicationContext) : base(applicationContext)
        {
            this.thisApplicationContext = applicationContext;
        }

        public override async Task ImportAsync(SchemaMigrationItemViewModel serverSchemaItem)
        {
            try
            {
                var serverSchema = serverSchemaItem.MigrationEntity;
                TraceProvider.WriteLine();
                TraceProvider.WriteLine("Exporting Schema to Json: {0}", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload);
                serverSchemaItem.ImportStatus = MigrationStatus.NotStarted;
                serverSchemaItem.ExportStatus = MigrationStatus.NotStarted;
                serverSchemaItem.ImportStatusText = null;
                serverSchemaItem.ExportStatusText = null;
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        CreateSchemas(serverSchemaItem);
                        serverSchemaItem.ImportStatus = MigrationStatus.Succeeded;
                        StringBuilder successMessageText = new StringBuilder();
                        successMessageText.Append(string.Format(Resources.ImportSuccessMessageText, serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload));
                        serverSchemaItem.ImportStatusText = successMessageText.ToString();
                        TraceProvider.WriteLine(string.Format("Schema {0} exported to Json with Name {1}", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload, FileOperations.GetFileName(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload)));
                        TraceProvider.WriteLine(string.Format("Schema Export to Json Successfull: {0}", serverSchemaItem.Name));
                        TraceProvider.WriteLine();
                    }
                    catch (Exception)
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


        #region SchemaCreation
        public JsonSchema.RootObject CreateSchemas(SchemaMigrationItemViewModel schemaItem)
        {
            var schema = schemaItem.MigrationEntity;
            string schemaName = schema.fullNameOfSchemaToUpload;
            JsonSchema.RootObject schemaRootObject = new JsonSchema.RootObject();
            try
            {
                string directroyPathForJsonFiles = Resources.JsonSchemaFilesLocalPath;
                string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(schemaName), ".json");
                string schemaJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(schemaRootObject);
                FileOperations.CreateFolder(fileName);
                System.IO.File.WriteAllText(fileName, schemaJsonFileContent);
                return schemaRootObject;
            }
            catch (Exception ex)
            {
                schemaItem.ImportStatus = MigrationStatus.Failed;
                schemaItem.ImportStatusText = ex.Message;
                TraceProvider.WriteLine(string.Format("Schema Export to Json Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public async Task CreateSchemas(List<SchemaMigrationItemViewModel> schemaItems)
        {
            ActionOnSchemas action = new ActionOnSchemas();
            List<SchemaDetails> schemaDetailsList = new List<SchemaDetails>();
            List<SchemaDetails> response = new List<SchemaDetails>();
            ObservableCollection<SchemaSelectionItemViewModel> originalCollection = this.thisApplicationContext.GetProperty(AppConstants.AllSchemasContextPropertyName) as ObservableCollection<SchemaSelectionItemViewModel>;
            List<SchemaSelectionItemViewModel> orignalList = originalCollection.ToList();
            List<SchemaDetails> originalSchemaDetailsList = new List<SchemaDetails>();
            foreach (var schemaItem in schemaItems)
            {
                schemaItem.MigrationEntity.isSchemaExtractedFromDb = false;
                schemaItem.MigrationEntity.errorDetailsForExtraction = "";
                schemaDetailsList.Add(schemaItem.MigrationEntity);
            }
            foreach (var schemaItem in orignalList)
            {
                originalSchemaDetailsList.Add(schemaItem.MigrationEntity);
            }
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    response = action.ExtractSchemasFromDlls(ref schemaDetailsList, Resources.JsonSchemaFilesLocalPath, originalSchemaDetailsList);
                    thisApplicationContext.SetProperty("SchemasToUploadOrder", response);
                    SetExtractionStatus(schemaDetailsList, schemaItems);
                }
                catch (Exception ex)
                {
                    SetExtractionStatus(schemaDetailsList, schemaItems);
                    TraceProvider.WriteLine(string.Format("Schemas Extraction Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                    TraceProvider.WriteLine();
                    throw new Exception(string.Format("Schemas Extraction Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                }
            });
        }
        #endregion



        #region Export to IA
        public override async Task ExportToIA(SchemaMigrationItemViewModel serverSchemaItem, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            try
            {
                TraceProvider.WriteLine("Migrating schema : {0}", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload);
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        bool overwrite = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
                        if (!overwrite)
                        {
                            bool exists = CheckIfArtifactExists(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload, "Schema", this.iaDetails, authresult).Result;
                            if (exists)
                            {
                                serverSchemaItem.ExportStatus = MigrationStatus.Partial;
                                serverSchemaItem.ExportStatusText = string.Format("The Schema {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the certificate was not overwritten.", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload, FileOperations.GetFileName(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload));
                                TraceProvider.WriteLine(serverSchemaItem.ExportStatusText);
                                TraceProvider.WriteLine();
                            }
                            else
                            {
                                MigrateToCloudIA(FileOperations.GetSchemaJsonFilePath(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload), FileOperations.GetFileName(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload), serverSchemaItem, iaDetails, authresult).Wait();
                                serverSchemaItem.ExportStatus = MigrationStatus.Succeeded;
                                serverSchemaItem.ExportStatusText = string.Format("Schema {0} migrated succesfully", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload);
                                TraceProvider.WriteLine(string.Format("Schema Migration Successfull: {0}", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload));
                                TraceProvider.WriteLine();
                            }
                        }
                        else
                        {
                            MigrateToCloudIA(FileOperations.GetSchemaJsonFilePath(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload), FileOperations.GetFileName(serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload), serverSchemaItem, iaDetails, authresult).Wait();
                            serverSchemaItem.ExportStatus = MigrationStatus.Succeeded;
                            serverSchemaItem.ExportStatusText = string.Format("Schema {0} migrated succesfully", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload);
                            TraceProvider.WriteLine(string.Format("Schema Migration Successfull: {0}", serverSchemaItem.MigrationEntity.fullNameOfSchemaToUpload));
                            TraceProvider.WriteLine();
                        }
                    }
                    catch (Exception)
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
        #endregion

        #region Migrate To Cloud IA

        public async Task<HttpResponseMessage> MigrateToCloudIA(string filePath, string name, SchemaMigrationItemViewModel serverSchemaItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                var x = await sclient.LAIntegrationFromFile(UrlHelper.GetSchemaUrl(name, iaDetails), filePath, authResult);
                return x;
            }
            catch (Exception ex)
            {
                serverSchemaItem.ExportStatus = MigrationStatus.Failed;
                serverSchemaItem.ExportStatusText = ex.Message;
                TraceProvider.WriteLine(string.Format("Schema Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public async Task MigrateToCloudIA(List<SchemaMigrationItemViewModel> schemaItems, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            ActionOnSchemas action = new ActionOnSchemas();
            var schemasToBeUploaded = thisApplicationContext.GetProperty("SchemasToUploadOrder") as List<SchemaDetails>;
            AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
            bool overwriteExistingArtifacts = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
            List<SchemaDetails> schemaDetailsList = new List<SchemaDetails>();
            foreach (var schemaItem in schemaItems)
            {
                schemaItem.MigrationEntity.schemaUploadToIAStatus = SchemaUploadToIAStatus.NotYetStarted;
                schemaItem.MigrationEntity.errorDetailsForMigration = "";
                schemaDetailsList.Add(schemaItem.MigrationEntity);
            }
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    action.UploadToIntegrationAccount(schemasToBeUploaded, ref schemaDetailsList, Resources.JsonSchemaFilesLocalPath, overwriteExistingArtifacts,iaDetails.SubscriptionId,iaDetails.ResourceGroupName,iaDetails.IntegrationAccountName,authresult);
                    SetMigrationStatus(schemaDetailsList, schemaItems);
                }
                catch (Exception ex)
                {
                    SetMigrationStatus(schemaDetailsList, schemaItems);
                    TraceProvider.WriteLine(string.Format("Schemas Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                    TraceProvider.WriteLine();
                    throw new Exception(string.Format("Schemas Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                }
            });
        }
        #endregion

        public void SetMigrationStatus(List<SchemaDetails> schemaDetailsList, List<SchemaMigrationItemViewModel> schemaItems)
        {
            try
            {
                foreach (var item in schemaDetailsList)
                {
                    TraceProvider.WriteLine(string.Format("Migrating Schema {0}", item.fullNameOfSchemaToUpload));
                    if (item.schemaUploadToIAStatus == SchemaUploadToIAStatus.Success)
                    {
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Succeeded;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Schema {0} migrated succesfully", item.fullNameOfSchemaToUpload);
                        TraceProvider.WriteLine(string.Format("Schema {0} Migration Successfull", item.fullNameOfSchemaToUpload));
                    }
                    else if (item.schemaUploadToIAStatus == SchemaUploadToIAStatus.Partial)
                    {
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Partial;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Schema {0} already exists in IA. Since overwrite was disabled, it was not overwritten.", item.fullNameOfSchemaToUpload);
                        TraceProvider.WriteLine(string.Format("Schema {0} Not Migrated", item.fullNameOfSchemaToUpload));
                    }
                    else
                    {
                        string error = string.IsNullOrEmpty(item.errorDetailsForMigration) ? "Unknown" : item.errorDetailsForMigration;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Failed;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Schema {0} migration failed.Reason : {1}", item.fullNameOfSchemaToUpload, item.errorDetailsForMigration);
                        TraceProvider.WriteLine(string.Format("Schema {0} Migration Failed.Reason : {1} ", item.fullNameOfSchemaToUpload, error));
                    }
                    TraceProvider.WriteLine();
                }
            }
            catch(Exception ex)
            {
                throw ex;
            }
        }

        public void SetExtractionStatus(List<SchemaDetails> schemaDetailsList, List<SchemaMigrationItemViewModel> schemaItems)
        {
            try
            {
                foreach (var item in schemaDetailsList)
                {
                    TraceProvider.WriteLine(string.Format("Extracting Schema {0}", item.fullNameOfSchemaToUpload));
                    if (item.isSchemaExtractedFromDb)
                    {
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatus = MigrationStatus.Succeeded;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatusText = string.Format("Schema {0} extracted succesfully", item.fullNameOfSchemaToUpload);
                        TraceProvider.WriteLine(string.Format("Schema {0} succesfully extracted with name {1}", item.fullNameOfSchemaToUpload, item.fullNameOfSchemaToUpload));
                        TraceProvider.WriteLine(string.Format("Schema {0} extracted succesfully", item.fullNameOfSchemaToUpload));
                    }
                    else
                    {
                        string error = string.IsNullOrEmpty(item.errorDetailsForExtraction) ? "Unknown" : item.errorDetailsForExtraction;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatus = MigrationStatus.Failed;
                        schemaItems.Where(x => x.MigrationEntity.schemaName == item.schemaName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatusText = string.Format("Schema {0} extraction failed. Reason : {1}", item.fullNameOfSchemaToUpload, item.errorDetailsForExtraction);
                        TraceProvider.WriteLine(string.Format("Schema {0} extraction failed. Reason:{1} ", item.fullNameOfSchemaToUpload, error));
                    }
                    TraceProvider.WriteLine();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
