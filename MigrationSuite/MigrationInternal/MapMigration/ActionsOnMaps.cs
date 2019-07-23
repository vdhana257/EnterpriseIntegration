using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using static MapMigration.MapDetails;

namespace MapMigration
{
    public class ActionsOnMaps
    {
        public List<MapDetails> mapDetailsList, originalMapDetailsList;
        public string mapOutputDir;
        public string dllLocation;
        public Dictionary<string, string> mapNamespaceVersionDict;
        public ActionsOnMaps()
        {
            mapDetailsList = new List<MapDetails>();
            originalMapDetailsList = new List<MapDetails>();
            mapNamespaceVersionDict = new Dictionary<string, string>();
        }

        /// <summary>
        /// Function to get the list of maps
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        /// <param name="outputDir"></param>
        /// <returns></returns>

        public List<MapDetails> GetListOfMaps(string sqlConnectionString, string outputDir, string parameter)
        {

            SetMapAndDllDirectories(outputDir);

            var mapDetailsObj = new MapDetails();
            var originalMapDetailsObj = new MapDetails();
            using (SqlConnection cn = new SqlConnection(sqlConnectionString))
            {
                var query = ConfigurationManager.AppSettings["MapQueryApplication"];
                query = string.Format(query, parameter);
                //or nvcFullName like 'MSIT.EAS.ICOE.VL.PosOrdrsp.Shared.Schemas.AP%'
                //or nvcFullName like 'MSIT.EAS.ICOE.VL.ZInvoic.Shared.Schemas.AP%'
                //or nvcFullName like 'MSIT.EAS.ICOE.VL.PropertySchemas%')

                //and nvcFullName like 'MSIT.EAS.ICOE.VL.Ordrsp.Shared.Schemas.AP%'";

                using (var cmd = new SqlCommand(query, cn))
                {
                    if (cn.State == System.Data.ConnectionState.Closed)
                    {
                        try { cn.Open(); }
                        catch (Exception e)
                        {
                            string message = $"ERROR! Unable to establish connection to the database. \nErrorMessage:{e.Message}";
                            TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                            //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                            throw new Exception(message);
                        }
                    }
                    using (var rdr = cmd.ExecuteReader())
                    {

                        while (rdr.Read())
                        {
                            mapDetailsObj = new MapDetails();
                            originalMapDetailsObj = new MapDetails();

                            mapDetailsObj.assemblyFullyQualifiedName = rdr["nvcFullName"].ToString();
                            originalMapDetailsObj.assemblyFullyQualifiedName = rdr["nvcFullName"].ToString();

                            mapDetailsObj.mapUploadToIAStatus = MapUploadToIAStatus.NotYetStarted;
                            mapDetailsObj.errorDetailsForMigration = "";
                            mapDetailsObj.errorDetailsForExtraction = "";
                            mapDetailsObj.mapFilesLocation = outputDir;

                            mapDetailsObj.mapFullName = rdr["FullName"].ToString();
                            originalMapDetailsObj.mapFullName = rdr["FullName"].ToString();

                            mapDetailsObj.mapName = rdr["Name"].ToString();
                            originalMapDetailsObj.mapName = rdr["Name"].ToString();

                            mapDetailsObj.mapNamespace = rdr["Namespace"].ToString();
                            mapDetailsObj.version = rdr["nvcVersion"].ToString();

                            mapDetailsList.Add(mapDetailsObj);
                            originalMapDetailsList.Add(originalMapDetailsObj);


                        }
                    }
                }

            }
            Directory.CreateDirectory(mapOutputDir);
            Directory.CreateDirectory(mapOutputDir + "\\AllMaps");

            PutAllMapsToLocalFolder(ref mapDetailsList, outputDir);
            return mapDetailsList;
        }

        /// <summary>
        /// Putting all maps to a local folder
        /// </summary>
        /// <param name="mapDetailsList"></param>
        /// <param name="outputDir"></param>
        public void PutAllMapsToLocalFolder(ref List<MapDetails> mapDetailsList, string outputDir)
        {

            var distinctAssembliesList = new HashSet<string>();
            foreach (var mapObj in mapDetailsList)
            {
                distinctAssembliesList.Add(mapObj.assemblyFullyQualifiedName);
            }

            try
            {
                foreach (var fqnForAssembly in distinctAssembliesList)
                {

                    var dllName = Path.GetFileName(fqnForAssembly).Replace(".dll", string.Empty).Replace('.', '_').Substring(0, Path.GetFileName(fqnForAssembly).IndexOf(','));
                    dllName = dllName + "_V" + fqnForAssembly.Substring(fqnForAssembly.IndexOf("Version"), 15).Replace(".", "_").Replace("Version=", "");
                    //Load the assembly into memory
                    Assembly asm;
                    try
                    {
                        asm = Assembly.Load(fqnForAssembly);
                    }
                    catch (FileNotFoundException e)
                    {
                        TraceProvider.WriteLine($"WARNING! Assembly with name {fqnForAssembly} to which identified Schemas belong is not found in GAC'ed DLLs. Message:{e.Message}");
                        //Console.WriteLine($"WARNING! Assembly with name {fqnForAssembly} to which identified Schemas belong is not found in GAC'ed DLLs. Message:{e.Message}");

                        continue;
                    }
                    var mapObj = new MapDetails();
                    var originalMapObj = new MapDetails();
                    var types = asm.GetTypes();
                    var classTypes = types.Where(type => type.IsClass == true && (type.BaseType.Name == "TransformBase") || (type.BaseType.BaseType != null && type.BaseType.BaseType.Name == "TransformBase"));
                    foreach (Type ty in types)
                    {
                        try
                        {
                            mapObj = mapDetailsList.First<MapDetails>(r => r.mapName == ty.Name && r.assemblyFullyQualifiedName == ty.Assembly.FullName);
                            originalMapObj = originalMapDetailsList.First<MapDetails>(r => r.mapName == ty.Name && r.assemblyFullyQualifiedName == ty.Assembly.FullName);

                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                        try
                        {
                            //// for every new schema in the DLL
                            //if (ty.BaseType != null && ty.BaseType.FullName == "Microsoft.XLANGs.BaseTypes.SchemaBase")
                            //{
                            //    var sb = System.Activator.CreateInstance(ty) as SchemaBase;
                            //    var schemaFileName = ty.Name;

                            //    if (ty.DeclaringType == null)
                            //    {
                            //        var schemaXmlContext = sb.XmlContent;
                            //        schemaXmlContext = schemaXmlContext.Replace("utf-16", "utf-8");
                            //        Match matchVersion = Regex.Match(schemaXmlContext, "standards_version=\"(.*?)\"");
                            //        Match matchNamespace = Regex.Match(schemaXmlContext, "targetNamespace=\"(.*?)\"");
                            //        if (matchVersion.Success && matchNamespace.Success)
                            //        {
                            //            if (!schemaNamespaceVersionDict.ContainsKey(matchNamespace.Groups[1].Value))
                            //            {
                            //                schemaNamespaceVersionDict.Add(matchNamespace.Groups[1].Value, matchVersion.Groups[1].Value);
                            //            }
                            //        }

                            //        schemaFileName = dllName + "_" + schemaFileName;
                            //        // truncate the length of the schema if its more than what the IA can handle
                            //        if (schemaFileName.Length > 79)
                            //            schemaFileName = schemaFileName.Replace("_", "");

                            //        schemaObj.fullNameOfSchemaToUpload = schemaFileName;
                            //        originalSchemaObj.fullNameOfSchemaToUpload = schemaFileName;

                            //        // Write to file
                            //        var schemaFilePath = string.Format(outputDir + "\\AllSchemas\\{0}.xsd", schemaFileName);
                            //        File.WriteAllText(schemaFilePath, schemaXmlContext, Encoding.UTF8);

                            //    }
                            //}

                            object mapObject = Activator.CreateInstance(ty);
                            object map = ty.InvokeMember("XmlContent", BindingFlags.GetProperty, null, mapObject, null);
                            string mapstr = map.ToString();

                            Match matchVersion = Regex.Match(mapstr, "standards_version=\"(.*?)\"");
                            Match matchNamespace = Regex.Match(mapstr, "targetNamespace=\"(.*?)\"");
                            if (matchVersion.Success && matchNamespace.Success)
                            {
                                if (!mapNamespaceVersionDict.ContainsKey(matchNamespace.Groups[1].Value))
                                {
                                    mapNamespaceVersionDict.Add(matchNamespace.Groups[1].Value, matchVersion.Groups[1].Value);
                                }
                            }

                            var mapFileName = ty.Name;
                            mapFileName = dllName + "_" + mapFileName;
                            // truncate the length of the schema if its more than what the IA can handle
                            if (mapFileName.Length > 79)
                                mapFileName = mapFileName.Replace("_", "");

                            mapObj.fullNameOfMapToUpload = mapFileName;
                            originalMapObj.fullNameOfMapToUpload = mapFileName;

                            

                            // Write to file
                            var mapFilePath = string.Format(outputDir + "\\AllMaps\\{0}.xslt", mapFileName);
                            File.WriteAllText(mapFilePath, mapstr, Encoding.UTF8);


                        }
                        catch (Exception e)
                        {
                            string message = $"ERROR! Problem reading schema content from MapBase and Writing the file to Local Folder. \nMap:{mapObj.mapFullName} \nAssembly:{mapObj.assemblyFullyQualifiedName} \nErrorMessage: {e.Message}";
                            TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                            throw new Exception(message);
                        }
                    }
                }

            }

            catch (Exception e)
            {
                string message = $"ERROR! Something went wrong. \nErrorMessage:{e.Message}";
                TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                throw new Exception(message);
            }


        }

        /// <summary>
        /// To extract selected maps from all the maps which we had retrieved by reflecting upon the DLLs
        /// </summary>
        /// <param name="selectiveMapDetailsList"></param>
        /// <param name="outputDir"></param>
        /// <param name="originalMapDetailsList"></param>
        /// <returns>Returns the order of the maps to be uploaded to IA</returns>
        public List<MapDetails> ExtractMapsFromDlls(ref List<MapDetails> selectiveMapDetailsList, string outputDir, List<MapDetails> originalMapDetailsList)
        {

            this.originalMapDetailsList = originalMapDetailsList;
            // Location where the all the retrieved maps are stored upon reflection of GAC'ed DLLs
            var d = new DirectoryInfo(outputDir + "\\AllMaps");
            var filesInfo = d.GetFiles("*.xslt");
            if (filesInfo.Length == 0)
            {
                string message = $"ERROR! No maps got extracted from the assembly into the local.";
                TraceProvider.WriteLine(message);
                //Console.WriteLine(message);

                throw new Exception(message);
            }

            // loop thru all the maps selected via the UI for extraction and enable their extraction flag on successful extraction
            foreach (var selectedMap in selectiveMapDetailsList)
            {
                try
                {
                    if (selectedMap.fullNameOfMapToUpload != null)
                    {
                        selectedMap.isMapExtractedFromDb = true;
                        TraceProvider.WriteLine($"Selected map extracted successfully: {selectedMap.fullNameOfMapToUpload}");
                    }
                    else
                    {
                        selectedMap.isMapExtractedFromDb = false;
                        selectedMap.errorDetailsForExtraction = selectedMap.errorDetailsForExtraction + " " + "ERROR! DLL to which this map belongs is not GAC'ed on your machine. Please GAC this required DLL (as warned in the Log File).";
                    }
                }
                catch (Exception ex)
                {
                    string message = $"ERROR! Problem during extracttion of selected maps. \nErrorMessage:{ex.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");

                    selectedMap.errorDetailsForExtraction = selectedMap.errorDetailsForExtraction + "\n" + message;
                    //throw new Exception(message);       
                }
            }

            // the dictionary of the schemas and all their dependencies are now with us, pass on this dictionary to below code to get the order of upload to IA
            var mapsToBeUploaded = new List<MapDetails>();
            foreach (var item in selectiveMapDetailsList)
            {
                string source = outputDir + "AllMaps\\" + item.fullNameOfMapToUpload +".xslt";
                string dest = outputDir + item.fullNameOfMapToUpload+".xslt";
                File.Copy(source, dest);
                mapsToBeUploaded.Add(item);
            }

            return mapsToBeUploaded;
        }

        /// <summary>
        /// Clear the directory for a new selection
        /// </summary>
        /// <param name="outputdir"></param>
        private void SetMapAndDllDirectories(string outputdir)
        {
            try
            {
                if (ConfigurationManager.AppSettings["mapOutputDir"].ToString() != "")
                {
                    mapOutputDir = ConfigurationManager.AppSettings["mapOutputDir"].ToString();
                }
                else
                {
                    mapOutputDir = outputdir;
                }
            }
            catch (Exception)
            {
                mapOutputDir = outputdir;
            }
            TraceProvider.WriteLine($"Map Output Directory is {mapOutputDir} ");

            if (Directory.Exists(mapOutputDir))
            {
                DeleteDirectory(mapOutputDir);
            }


            try
            {
                if (ConfigurationManager.AppSettings["dllLocation"].ToString() != "")
                {
                    dllLocation = ConfigurationManager.AppSettings["dllLocation"].ToString();
                    TraceProvider.WriteLine("DLLs are at location " + dllLocation);
                }
                else
                {
                    TraceProvider.WriteLine("The configured DLL Directory Location is Empty. Assuming DLLs are to be read from the GAC");
                    //Console.WriteLine("The configured DLL Directory Location is Empty. Assuming DLLs are to be read from the GAC");

                }

            }
            catch (Exception)
            {
                TraceProvider.WriteLine("No DLL Directory Location Configured. Assuming DLLs are to be read from the GAC");
                //Console.WriteLine("No DLL Directory Location Configured. Assuming DLLs are to be read from the GAC");

            }

        }

        private void DeleteDirectory(string mapOutputDir)
        {
            //Delete all existing files, subdirectories in map directory and the directory itself 
            var fs = Directory.GetFiles(mapOutputDir);
            var ds = Directory.GetDirectories(mapOutputDir);
            foreach (var f in fs)
            {
                File.Delete(f);
            }
            if (ds.Length > 0)
                DeleteDirectory(mapOutputDir + "\\AllMaps");



            Thread.Sleep(1000);
            Directory.Delete(mapOutputDir, true);

        }

        /// <summary>
        /// Call to start the process of upload to integration account
        /// </summary>
        /// <param name="mapsToBeUploaded"></param>
        /// <param name="mapsDetailsList"></param>
        /// <param name="outputDir"></param>
        /// <param name="overrideExistingMapsFlag"></param>
        /// <param name="aadInstance"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="iaName"></param>
        public void UploadToIntegrationAccount(List<MapDetails> mapsToBeUploaded, ref List<MapDetails> mapDetailsList, string outputDir, bool overrideExistingMapsFlag, string subscriptionId, string resourceGroupName, string iaName, AuthenticationResult authResult)
        {
            if (mapsToBeUploaded.Count > 0)
            {

                IntegrationAccountContextForMaps iacontext = new IntegrationAccountContextForMaps();
                try
                {
                    IntegrationAccountDetails iaDetails = new IntegrationAccountDetails
                    {
                        SubscriptionId = subscriptionId,
                        ResourceGroupName = resourceGroupName,
                        IntegrationAccountName = iaName
                    };
                    iacontext.SchemaUploadFromFolder(outputDir, mapsToBeUploaded, overrideExistingMapsFlag, iaDetails, authResult, ref mapDetailsList);
                }

                catch (Exception e)
                {
                    string message = $"ERROR! Something went wrong while doing a map upload from local folder ${outputDir}. \nErrorMessage:{e.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                    throw e;
                }
            }
        }

    }
}
