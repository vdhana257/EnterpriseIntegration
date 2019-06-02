using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.XLANGs.BaseTypes;
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
using static SchemaMigration.SchemaDetails;

namespace SchemaMigration
{
    public class ActionOnSchemas
    {
        public List<SchemaDetails> schemaDetailsList, originalSchemDetailsList;
        public string schemaOutputDir;
        public string dllLocation;
        public Dictionary<string, string> schemaNamespaceVersionDict;
        public ActionOnSchemas()
        {
            schemaDetailsList = new List<SchemaDetails>();
            originalSchemDetailsList = new List<SchemaDetails>();
            schemaNamespaceVersionDict = new Dictionary<string, string>();
        }

        /// <summary>
        /// Function to get the list of schemas
        /// </summary>
        /// <param name="sqlConnectionString"></param>
        /// <param name="outputDir"></param>
        /// <returns></returns>
        public List<SchemaDetails> GetListOfSchemas(string sqlConnectionString, string outputDir)
        {

            SetSchemAndDllDirectories(outputDir);

            var schemaDetailsObj = new SchemaDetails();
            var originalSchemaDetailsObj = new SchemaDetails();
            using (SqlConnection cn = new SqlConnection(sqlConnectionString))
            {
                var query = ConfigurationManager.AppSettings["SchemaQuery"];


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
                            schemaDetailsObj = new SchemaDetails();
                            originalSchemaDetailsObj = new SchemaDetails();

                            schemaDetailsObj.assemblyFullyQualifiedName = rdr["nvcFullName"].ToString();
                            originalSchemaDetailsObj.assemblyFullyQualifiedName = rdr["nvcFullName"].ToString();

                            schemaDetailsObj.schemaUploadToIAStatus = SchemaUploadToIAStatus.NotYetStarted;
                            schemaDetailsObj.errorDetailsForMigration = "";
                            schemaDetailsObj.errorDetailsForExtraction = "";
                            schemaDetailsObj.dependentSchemas = new List<string>();
                            schemaDetailsObj.schemaFilesLocation = outputDir;

                            schemaDetailsObj.schemaFullName = rdr["FullName"].ToString();
                            originalSchemaDetailsObj.schemaFullName = rdr["FullName"].ToString();

                            schemaDetailsObj.schemaName = rdr["Name"].ToString();
                            originalSchemaDetailsObj.schemaName = rdr["Name"].ToString();

                            schemaDetailsObj.schemaNamespace = rdr["Namespace"].ToString();
                            schemaDetailsObj.version = rdr["nvcVersion"].ToString();

                            schemaDetailsList.Add(schemaDetailsObj);
                            originalSchemDetailsList.Add(originalSchemaDetailsObj);


                        }
                    }
                }

            }
            Directory.CreateDirectory(schemaOutputDir);
            Directory.CreateDirectory(schemaOutputDir + "\\AllSchemas");

            PutAllSchemasToLocalFolder(ref schemaDetailsList, outputDir);
            return schemaDetailsList;
        }

        /// <summary>
        /// Putting all schemas to a local folder
        /// </summary>
        /// <param name="schemaDetailsList"></param>
        /// <param name="outputDir"></param>
        public void PutAllSchemasToLocalFolder(ref List<SchemaDetails> schemaDetailsList, string outputDir)
        {

            var distinctAssembliesList = new HashSet<string>();
            foreach (var schemaObj in schemaDetailsList)
            {
                distinctAssembliesList.Add(schemaObj.assemblyFullyQualifiedName);
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
                    var schemaObj = new SchemaDetails();
                    var originalSchemaObj = new SchemaDetails();
                    foreach (Type ty in asm.GetTypes())
                    {
                        try
                        {
                            schemaObj = schemaDetailsList.First<SchemaDetails>(r => r.schemaName == ty.Name && r.assemblyFullyQualifiedName == ty.Assembly.FullName);
                            originalSchemaObj = originalSchemDetailsList.First<SchemaDetails>(r => r.schemaName == ty.Name && r.assemblyFullyQualifiedName == ty.Assembly.FullName);

                        }
                        catch (Exception e)
                        {
                            continue;
                        }
                        try
                        {
                            // for every new schema in the DLL
                            if (ty.BaseType != null && ty.BaseType.FullName == "Microsoft.XLANGs.BaseTypes.SchemaBase")
                            {
                                var sb = System.Activator.CreateInstance(ty) as SchemaBase;
                                var schemaFileName = ty.Name;

                                if (ty.DeclaringType == null)
                                {
                                    var schemaXmlContext = sb.XmlContent;
                                    schemaXmlContext = schemaXmlContext.Replace("utf-16", "utf-8");
                                    Match matchVersion = Regex.Match(schemaXmlContext, "standards_version=\"(.*?)\"");
                                    Match matchNamespace = Regex.Match(schemaXmlContext,"targetNamespace=\"(.*?)\"");
                                    if (matchVersion.Success && matchNamespace.Success)
                                    {
                                        if (!schemaNamespaceVersionDict.ContainsKey(matchNamespace.Groups[1].Value))
                                        {
                                            schemaNamespaceVersionDict.Add(matchNamespace.Groups[1].Value, matchVersion.Groups[1].Value);
                                        }
                                    }

                                    schemaFileName = dllName + "_" + schemaFileName;
                                    // truncate the length of the schema if its more than what the IA can handle
                                    if (schemaFileName.Length > 79)
                                        schemaFileName = schemaFileName.Replace("_", "");

                                    schemaObj.fullNameOfSchemaToUpload = schemaFileName;
                                    originalSchemaObj.fullNameOfSchemaToUpload = schemaFileName;

                                    // Write to file
                                    var schemaFilePath = string.Format(outputDir + "\\AllSchemas\\{0}.xsd", schemaFileName);
                                    File.WriteAllText(schemaFilePath, schemaXmlContext, Encoding.UTF8);

                                }
                            }
                        }
                        catch (Exception e)
                        {
                            string message = $"ERROR! Problem reading schema content from SchemaBase and Writing the file to Local Folder. \nSchema:{schemaObj.schemaFullName} \nAssembly:{schemaObj.assemblyFullyQualifiedName} \nErrorMessage: {e.Message}";
                            TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                            //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

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
        /// To extract selected schemas from all the schemas which we had retrieved by reflecting upon the DLLs
        /// </summary>
        /// <param name="selectiveSchemaDetailsList"></param>
        /// <param name="outputDir"></param>
        /// <param name="originalSchemaDetailsList"></param>
        /// <returns>Returns the order of the schema to be uploaded to IA</returns>
        public List<SchemaDetails> ExtractSchemasFromDlls(ref List<SchemaDetails> selectiveSchemaDetailsList, string outputDir, List<SchemaDetails> originalSchemaDetailsList)
        {

            this.originalSchemDetailsList = originalSchemaDetailsList;
            // Location where the all the retrieved schemas are stored upon reflection of GAC'ed DLLs
            var d = new DirectoryInfo(outputDir + "\\AllSchemas");
            var filesInfo = d.GetFiles("*.xsd");
            if (filesInfo.Length == 0)
            {
                string message = $"ERROR! No schemas got extracted from the assembly into the local.";
                TraceProvider.WriteLine(message);
                //Console.WriteLine(message);

                throw new Exception(message);
            }
            var graphDict = new Dictionary<SchemaDetails, List<SchemaDetails>>();
            var dependentSchemaList = new List<SchemaDetails>();

            // loop thru all the schemas selected via the UI for extraction and enable their extraction flag on successful extraction
            foreach (var selectedSchema in selectiveSchemaDetailsList)
            {
                try
                {
                    if (selectedSchema.fullNameOfSchemaToUpload != null)
                    {
                        selectedSchema.isSchemaExtractedFromDb = true;
                        TraceProvider.WriteLine($"Selected schema extracted successfully: {selectedSchema.fullNameOfSchemaToUpload}");
                        //Console.WriteLine($"Selected schema extracted successfully: {selectedSchema.fullNameOfSchemaToUpload}");
                        graphDict.Add(selectedSchema, dependentSchemaList);
                    }
                    else
                    {
                        selectedSchema.isSchemaExtractedFromDb = false;
                        selectedSchema.errorDetailsForExtraction = selectedSchema.errorDetailsForExtraction + " " + "ERROR! DLL to which this schema belongs is not GAC'ed on your machine. Please GAC this required DLL (as warned in the Log File).";
                    }
                }
                catch (Exception ex)
                {
                    string message = $"ERROR! Problem during extracttion of selected schemas. \nErrorMessage:{ex.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{ex.StackTrace}");

                    selectedSchema.errorDetailsForExtraction = selectedSchema.errorDetailsForExtraction + "\n" + message;
                    //throw new Exception(message);       // THROW NEEDED HERE OR NOT. ****ASK STUTI****
                }
            }

            // out of the selected schemas, there may be a case where the schemas (which are not selected in UI) are also a dependency and their reference would be their in the selected schemas.
            var identifyDep = new IdentifyHiddenDependencies(originalSchemaDetailsList);
            var newGraphDict = new Dictionary<SchemaDetails, List<SchemaDetails>>();
            // Retrieve all the list of dependent schemas by reading thru the selected schemas' XSDs and identifying the references and then recursively reading thru them
            if (graphDict.Count > 0)
                newGraphDict = identifyDep.FileReadDFSCaller(graphDict, outputDir);

            // the dictionary of the schemas and all their dependencies are now with us, pass on this dictionary to below code to get the order of upload to IA
            var schemasToBeUploaded = new List<SchemaDetails>();
            foreach (var key in newGraphDict.Keys)
            {
                schemasToBeUploaded.Add(key);
            }
            try
            {
                var g = new DependencyGraph();

                if (newGraphDict.Count > 0)
                {

                    g.createAdjacencyList(newGraphDict);

                    schemasToBeUploaded = g.DFSCaller();
                }
            }
            catch (Exception ex)
            {
                string message = $"ERROR! Problem while identifying dependencies and generating the order of upload for selective (and dependent) schemas. \nErrorMessage:{ex.Message}";
                TraceProvider.WriteLine($"{message} \nStackTrace:{ ex.StackTrace}");
                // Console.WriteLine($"{message} \nStackTrace:{ ex.StackTrace}");

                //throw new Exception(message);
            }
            return schemasToBeUploaded;


        }

        /// <summary>
        /// Clear the directory for a new selection
        /// </summary>
        /// <param name="outputdir"></param>
        private void SetSchemAndDllDirectories(string outputdir)
        {
            try
            {
                if (ConfigurationManager.AppSettings["schemaOutputDir"].ToString() != "")
                {
                    schemaOutputDir = ConfigurationManager.AppSettings["schemaOutputDir"].ToString();
                }
                else
                {
                    schemaOutputDir = outputdir;
                }
            }
            catch (Exception)
            {
                schemaOutputDir = outputdir;
            }
            TraceProvider.WriteLine($"Schema Output Directory is {schemaOutputDir} ");
            //Console.WriteLine($"Schema Output Directory is {schemaOutputDir} ");

            if (Directory.Exists(schemaOutputDir))
            {
                DeleteDirectory(schemaOutputDir);
            }


            try
            {
                if (ConfigurationManager.AppSettings["dllLocation"].ToString() != "")
                {
                    dllLocation = ConfigurationManager.AppSettings["dllLocation"].ToString();
                    TraceProvider.WriteLine("DLLs are at location " + dllLocation);
                    //Console.WriteLine("DLLs are at location " + dllLocation);

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

        private void DeleteDirectory(string schemaOutputDir)
        {
            //Delete all existing files, subdirectories in schema directory and the directory itself 
            var fs = Directory.GetFiles(schemaOutputDir);
            var ds = Directory.GetDirectories(schemaOutputDir);
            foreach (var f in fs)
            {
                File.Delete(f);
            }
            if (ds.Length > 0)
                DeleteDirectory(schemaOutputDir + "\\AllSchemas");
            
        

            Thread.Sleep(1000);
            Directory.Delete(schemaOutputDir, true);
            
        }

        /// <summary>
        /// Call to start the process of upload to integration account
        /// </summary>
        /// <param name="schemasToBeUploaded"></param>
        /// <param name="schemaDetailsList"></param>
        /// <param name="outputDir"></param>
        /// <param name="overrideExistingSchemasFlag"></param>
        /// <param name="aadInstance"></param>
        /// <param name="resource"></param>
        /// <param name="clientId"></param>
        /// <param name="clientSecret"></param>
        /// <param name="subscriptionId"></param>
        /// <param name="resourceGroupName"></param>
        /// <param name="iaName"></param>
        public void UploadToIntegrationAccount(List<SchemaDetails> schemasToBeUploaded, ref List<SchemaDetails> schemaDetailsList, string outputDir, bool overrideExistingSchemasFlag, string subscriptionId, string resourceGroupName, string iaName, AuthenticationResult authResult)
        {
            if (schemasToBeUploaded.Count > 0)
            {

                IntegrationAccountContextForSchemas iacontext = new IntegrationAccountContextForSchemas();
                try
                {
                    IntegrationAccountDetails iaDetails = new IntegrationAccountDetails
                    {
                        SubscriptionId = subscriptionId,
                        ResourceGroupName = resourceGroupName,
                        IntegrationAccountName = iaName
                    };
                    iacontext.SchemaUploadFromFolder(outputDir, schemasToBeUploaded, overrideExistingSchemasFlag, iaDetails,authResult ,ref schemaDetailsList);
                }

                catch (Exception e)
                {
                    string message = $"ERROR! Something went wrong while doing a schema upload from local folder ${outputDir}. \nErrorMessage:{e.Message}";
                    TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                    //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");

                    throw e;
                }
            }
        }
    }
}
