using System;
using System.Collections.Generic;
using System.Data.Services.Client;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using MapMigration;
using System.Collections.ObjectModel;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Common;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MapMigrator : TpmMigrator<MapMigrationItemViewModel, MapDetails>
    {
        private IntegrationAccountDetails iaDetails;
        private IApplicationContext thisApplicationContext;

        public MapMigrator(IApplicationContext applicationContext) : base(applicationContext)
        {
            this.thisApplicationContext = applicationContext;
        }

        public override async Task ImportAsync(MapMigrationItemViewModel serverMapItem)
        {
            try
            {
                var serverMap = serverMapItem.MigrationEntity;
                TraceProvider.WriteLine();
                TraceProvider.WriteLine("Exporting Map to Json: {0}", serverMapItem.MigrationEntity.fullNameOfMapToUpload);
                serverMapItem.ImportStatus = MigrationStatus.NotStarted;
                serverMapItem.ExportStatus = MigrationStatus.NotStarted;
                serverMapItem.ImportStatusText = null;
                serverMapItem.ExportStatusText = null;
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        // CreateMaps(serverMapItem);
                        serverMapItem.ImportStatus = MigrationStatus.Succeeded;
                        StringBuilder successMessageText = new StringBuilder();
                        successMessageText.Append(string.Format(Resources.ImportSuccessMessageText, serverMapItem.MigrationEntity.fullNameOfMapToUpload));
                        serverMapItem.ImportStatusText = successMessageText.ToString();
                        TraceProvider.WriteLine(string.Format("Map {0} exported to Json with Name {1}", serverMapItem.MigrationEntity.fullNameOfMapToUpload, FileOperations.GetFileName(serverMapItem.MigrationEntity.fullNameOfMapToUpload)));
                        TraceProvider.WriteLine(string.Format("Map Export to Json Successfull: {0}", serverMapItem.Name));
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
        //public JsonSchema.RootObject CreateSchemas(SchemaMigrationItemViewModel schemaItem)
        //{
        //    var schema = schemaItem.MigrationEntity;
        //    string schemaName = schema.fullNameOfSchemaToUpload;
        //    JsonSchema.RootObject schemaRootObject = new JsonSchema.RootObject();
        //    try
        //    {
        //        string directroyPathForJsonFiles = Resources.JsonSchemaFilesLocalPath;
        //        string fileName = string.Format(directroyPathForJsonFiles, FileOperations.GetFileName(schemaName), ".json");
        //        string schemaJsonFileContent = Newtonsoft.Json.JsonConvert.SerializeObject(schemaRootObject);
        //        FileOperations.CreateFolder(fileName);
        //        System.IO.File.WriteAllText(fileName, schemaJsonFileContent);
        //        return schemaRootObject;
        //    }
        //    catch (Exception ex)
        //    {
        //        schemaItem.ImportStatus = MigrationStatus.Failed;
        //        schemaItem.ImportStatusText = ex.Message;
        //        TraceProvider.WriteLine(string.Format("Schema Export to Json Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
        //        TraceProvider.WriteLine();
        //        throw ex;
        //    }
        //}

        public async Task CreateSchemas(List<MapMigrationItemViewModel> mapItems)
        {
            ActionsOnMaps action = new ActionsOnMaps();
            List<MapDetails> mapDetailsList = new List<MapDetails>();
            List<MapDetails> response = new List<MapDetails>();
            ObservableCollection<MapSelectionItemViewModel> originalCollection = this.thisApplicationContext.GetProperty(AppConstants.AllMapsContextPropertyName) as ObservableCollection<MapSelectionItemViewModel>;
            List<MapSelectionItemViewModel> orignalList = originalCollection.ToList();
            List<MapDetails> originalMapDetailsList = new List<MapDetails>();
            foreach (var mapItem in mapItems)
            {
                mapItem.MigrationEntity.isMapExtractedFromDb = false;
                mapItem.MigrationEntity.errorDetailsForExtraction = "";
                mapDetailsList.Add(mapItem.MigrationEntity);
            }
            foreach (var mapItem in orignalList)
            {
                originalMapDetailsList.Add(mapItem.MigrationEntity);
            }
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    response = action.ExtractMapsFromDlls(ref mapDetailsList, Resources.JsonMapFilesLocalPath, originalMapDetailsList);
                    thisApplicationContext.SetProperty("MapsToUploadOrder", response);
                    SetExtractionStatus(mapDetailsList, mapItems);
                }
                catch (Exception ex)
                {
                    SetExtractionStatus(mapDetailsList, mapItems);
                    TraceProvider.WriteLine(string.Format("Maps Extraction Failed:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                    TraceProvider.WriteLine();
                    throw new Exception(string.Format("Maps Extraction Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                }
            });
        }
        #endregion

        #region Export to IA
        public override async Task ExportToIA(MapMigrationItemViewModel serverMapItem, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            try
            {
                TraceProvider.WriteLine("Migrating map : {0}", serverMapItem.MigrationEntity.fullNameOfMapToUpload);
                await Task.Factory.StartNew(() =>
                {
                    try
                    {
                        AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        bool overwrite = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
                        if (!overwrite)
                        {
                            bool exists = CheckIfArtifactExists(serverMapItem.MigrationEntity.fullNameOfMapToUpload, "Map", this.iaDetails, authresult).Result;
                            if (exists)
                            {
                                serverMapItem.ExportStatus = MigrationStatus.Partial;
                                serverMapItem.ExportStatusText = string.Format("The Map {0} already exists on IA with name {1}. Since the Overwrite option was disabled, the certificate was not overwritten.", serverMapItem.MigrationEntity.fullNameOfMapToUpload, FileOperations.GetFileName(serverMapItem.MigrationEntity.fullNameOfMapToUpload));
                                TraceProvider.WriteLine(serverMapItem.ExportStatusText);
                                TraceProvider.WriteLine();
                            }
                            else
                            {
                                MigrateToCloudIA(FileOperations.GetSchemaJsonFilePath(serverMapItem.MigrationEntity.fullNameOfMapToUpload), FileOperations.GetFileName(serverMapItem.MigrationEntity.fullNameOfMapToUpload), serverMapItem, iaDetails, authresult).Wait();
                                serverMapItem.ExportStatus = MigrationStatus.Succeeded;
                                serverMapItem.ExportStatusText = string.Format("Map {0} migrated succesfully", serverMapItem.MigrationEntity.fullNameOfMapToUpload);
                                TraceProvider.WriteLine(string.Format("Schema Migration Successfull: {0}", serverMapItem.MigrationEntity.fullNameOfMapToUpload));
                                TraceProvider.WriteLine();
                            }
                        }
                        else
                        {
                            MigrateToCloudIA(FileOperations.GetSchemaJsonFilePath(serverMapItem.MigrationEntity.fullNameOfMapToUpload), FileOperations.GetFileName(serverMapItem.MigrationEntity.fullNameOfMapToUpload), serverMapItem, iaDetails, authresult).Wait();
                            serverMapItem.ExportStatus = MigrationStatus.Succeeded;
                            serverMapItem.ExportStatusText = string.Format("Map {0} migrated succesfully", serverMapItem.MigrationEntity.fullNameOfMapToUpload);
                            TraceProvider.WriteLine(string.Format("Map Migration Successfull: {0}", serverMapItem.MigrationEntity.fullNameOfMapToUpload));
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

        public async Task<HttpResponseMessage> MigrateToCloudIA(string filePath, string name, MapMigrationItemViewModel serverMapItem, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            try
            {
                IntegrationAccountContext sclient = new IntegrationAccountContext();
                var x = await sclient.LAIntegrationFromFile(UrlHelper.GetMapUrl(name, iaDetails), filePath, authResult);
                return x;
            }
            catch (Exception ex)
            {
                serverMapItem.ExportStatus = MigrationStatus.Failed;
                serverMapItem.ExportStatusText = ex.Message;
                TraceProvider.WriteLine(string.Format("Map Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                TraceProvider.WriteLine();
                throw ex;
            }
        }

        public async Task MigrateToCloudIA(List<MapMigrationItemViewModel> mapItems, IntegrationAccountDetails iaDetails)
        {
            this.iaDetails = iaDetails;
            ActionsOnMaps action = new ActionsOnMaps();
            var mapsToBeUploaded = thisApplicationContext.GetProperty("MapsToUploadOrder") as List<MapDetails>;
            AuthenticationResult authresult = thisApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
            bool overwriteExistingArtifacts = Convert.ToBoolean(thisApplicationContext.GetProperty("OverwriteEnabled"));
            List<MapDetails> mapDetailsList = new List<MapDetails>();
            foreach (var mapItem in mapItems)
            {
                mapItem.MigrationEntity.mapUploadToIAStatus = MapUploadToIAStatus.NotYetStarted;
                mapItem.MigrationEntity.errorDetailsForMigration = "";
                mapDetailsList.Add(mapItem.MigrationEntity);
            }
            await Task.Factory.StartNew(() =>
            {
                try
                {
                    action.UploadToIntegrationAccount(mapsToBeUploaded, ref mapDetailsList, Resources.JsonMapFilesLocalPath, overwriteExistingArtifacts, iaDetails.SubscriptionId, iaDetails.ResourceGroupName, iaDetails.IntegrationAccountName, authresult);
                    SetMigrationStatus(mapDetailsList, mapItems);
                }
                catch (Exception ex)
                {
                    SetMigrationStatus(mapDetailsList, mapItems);
                    TraceProvider.WriteLine(string.Format("Maps Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                    TraceProvider.WriteLine();
                    throw new Exception(string.Format("Maps Migration Failed. Reason:{0}", ExceptionHelper.GetExceptionMessage(ex)));
                }
            });
        }
        #endregion

        public void SetMigrationStatus(List<MapDetails> schemaDetailsList, List<MapMigrationItemViewModel> mapItems)
        {
            try
            {
                foreach (var item in schemaDetailsList)
                {
                    TraceProvider.WriteLine(string.Format("Migrating Map {0}", item.fullNameOfMapToUpload));
                    if (item.mapUploadToIAStatus == MapUploadToIAStatus.Success)
                    {
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Succeeded;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Map {0} migrated succesfully", item.fullNameOfMapToUpload);
                        TraceProvider.WriteLine(string.Format("Map {0} Migration Successfull", item.fullNameOfMapToUpload));
                    }
                    else if (item.mapUploadToIAStatus == MapUploadToIAStatus.Partial)
                    {
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Partial;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Map {0} already exists in IA. Since overwrite was disabled, it was not overwritten.", item.fullNameOfMapToUpload);
                        TraceProvider.WriteLine(string.Format("Map {0} Not Migrated", item.fullNameOfMapToUpload));
                    }
                    else
                    {
                        string error = string.IsNullOrEmpty(item.errorDetailsForMigration) ? "Unknown" : item.errorDetailsForMigration;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatus = MigrationStatus.Failed;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ExportStatusText = string.Format("Map {0} migration failed.Reason : {1}", item.fullNameOfMapToUpload, item.errorDetailsForMigration);
                        TraceProvider.WriteLine(string.Format("Map {0} Migration Failed.Reason : {1} ", item.fullNameOfMapToUpload, error));
                    }
                    TraceProvider.WriteLine();
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void SetExtractionStatus(List<MapDetails> mapDetailsList, List<MapMigrationItemViewModel> mapItems)
        {
            try
            {
                foreach (var item in mapDetailsList)
                {
                    TraceProvider.WriteLine(string.Format("Extracting Map {0}", item.fullNameOfMapToUpload));
                    if (item.isMapExtractedFromDb)
                    {
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatus = MigrationStatus.Succeeded;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatusText = string.Format("Map {0} extracted succesfully", item.fullNameOfMapToUpload);
                        TraceProvider.WriteLine(string.Format("Map {0} succesfully extracted with name {1}", item.fullNameOfMapToUpload, item.fullNameOfMapToUpload));
                        TraceProvider.WriteLine(string.Format("Map {0} extracted succesfully", item.fullNameOfMapToUpload));
                    }
                    else
                    {
                        string error = string.IsNullOrEmpty(item.errorDetailsForExtraction) ? "Unknown" : item.errorDetailsForExtraction;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatus = MigrationStatus.Failed;
                        mapItems.Where(x => x.MigrationEntity.mapName == item.mapName && x.MigrationEntity.assemblyFullyQualifiedName == item.assemblyFullyQualifiedName).First().ImportStatusText = string.Format("Map {0} extraction failed. Reason : {1}", item.fullNameOfMapToUpload, item.errorDetailsForExtraction);
                        TraceProvider.WriteLine(string.Format("Map {0} extraction failed. Reason:{1} ", item.fullNameOfMapToUpload, error));
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
