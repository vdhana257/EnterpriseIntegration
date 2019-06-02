using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace SchemaMigration
{
    /// <summary>
    /// To identify dependencies by reading the content of a schema. Implemented a Depth First Directed Acyclic Forest Traversal to extract even the hidden schema dependencies.
    /// </summary>
    internal class IdentifyHiddenDependencies
    {

        Dictionary<SchemaDetails, bool> visited { get; set; }
        List<SchemaDetails> originalSchemDetailsList;

        internal IdentifyHiddenDependencies(List<SchemaDetails> origin)
        {
            this.originalSchemDetailsList = origin;
        }

        /// <summary>
        /// Read thru the schema XSD file content to scoop out any other dependency reference. Recurse thru all XSDs to extract all the dependenct schemas.
        /// </summary>
        /// <param name="graphDict"></param>
        /// <param name="outputDir"></param>
        /// <returns></returns>
        public Dictionary<SchemaDetails, List<SchemaDetails>> FileReadDFSCaller(Dictionary<SchemaDetails, List<SchemaDetails>> graphDict, string outputDir)
        {
            TraceProvider.WriteLine("Starting dependency extraction from selected Schema, if any...");
            //Console.WriteLine("Starting dependency extraction from selected Schema, if any...");

            var newGraphDict = new Dictionary<SchemaDetails, List<SchemaDetails>>();
            visited = new Dictionary<SchemaDetails, bool>();
            foreach (var x in graphDict.Keys)
                visited.Add(x, false);

            foreach (var elem in graphDict)
            {
                var dllName = elem.Key.assemblyFullyQualifiedName.Replace(".dll", string.Empty).Replace('.', '_').Substring(0, Path.GetFileName(elem.Key.assemblyFullyQualifiedName).IndexOf(','));
                dllName = dllName + "_V" + elem.Key.assemblyFullyQualifiedName.Substring(elem.Key.assemblyFullyQualifiedName.IndexOf("Version"), 15).Replace(".", "_").Replace("Version=", "");

                newGraphDict[elem.Key] = new List<SchemaDetails>();
                FileReadDFS(elem.Key, ref newGraphDict, dllName, outputDir);
            }
            return newGraphDict;
        }

        /// <summary>
        /// Start of the Depth First Directed Acyclic Forest Traversal Algorithm to identify and extract the dependencies from within their references in the schema XSD
        /// </summary>
        /// <param name="schemaAtKey"></param>
        /// <param name="newGraphDict"></param>
        /// <param name="dllName"></param>
        /// <param name="outputDir"></param>
        public void FileReadDFS(SchemaDetails schemaAtKey, ref Dictionary<SchemaDetails, List<SchemaDetails>> newGraphDict, string dllName, string outputDir)
        {
            try
            {
                if (visited[schemaAtKey] == false)
                {
                    visited[schemaAtKey] = true;
                    var dependencyList = UpdateSchemaContent(dllName, schemaAtKey, outputDir);
                    newGraphDict[schemaAtKey] = dependencyList;
                    if (dependencyList.Count == 0)
                        return;
                    foreach (var dep in dependencyList)
                        FileReadDFS(dep, ref newGraphDict, dllName, outputDir);
                }
            }
            catch (Exception e)
            {
                string message = $"ERROR! Problem during extraction of schema dependency from DLLs. Schema:{schemaAtKey.fullNameOfSchemaToUpload} \nErrorMessage:{e.Message}";
                TraceProvider.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                //Console.WriteLine($"{message} \nStackTrace:{e.StackTrace}");
                schemaAtKey.errorDetailsForExtraction = schemaAtKey.errorDetailsForExtraction + "\n" + message;
            }


        }

        /// <summary>
        /// Once you encounter a dependent schema name in the content of the current schema, replace the name of the schema as per the convention we have documented. 
        /// </summary>
        /// <param name="dllName"></param>
        /// <param name="thisSchemaObj"></param>
        /// <param name="outputDir"></param>
        /// <returns>Returns the dependent schema list</returns>
        private List<SchemaDetails> UpdateSchemaContent(string dllName, SchemaDetails thisSchemaObj, string outputDir)
        {

            var dependentSchemaList = new List<SchemaDetails>();

            var schemaXmlDoc = new XmlDocument();


            // PUT IT IN TRY
            var schemaXmlContext = File.ReadAllText(outputDir + "\\AllSchemas\\" + thisSchemaObj.fullNameOfSchemaToUpload + ".xsd");

            // Read dependencies
            schemaXmlDoc.LoadXml(schemaXmlContext);
            var schemaNodeChildren = schemaXmlDoc.DocumentElement;
            var x = schemaNodeChildren.ChildNodes;
            
            // loop thru the child nodes of the XSD schema to look for "import" or imports" nodes to extract names of dependent schemas from them
            foreach (XmlNode child in schemaNodeChildren)
            {
                try
                {

                    // looks for dependent schema in "imports" XML tag
                    if (child.Name.Contains("annotation") && child.ChildNodes.Count > 0)
                    {
                        foreach (XmlNode node in child)
                        {
                            if (node.Name.ToLower().Contains("appinfo") && node.ChildNodes.Count > 0)
                            {
                                foreach (XmlNode subnode in node)
                                {
                                    if (subnode.Name.ToLower().Contains("imports") && subnode.ChildNodes.Count > 0)
                                    {
                                        foreach (XmlNode importNode in subnode)
                                        {
                                            if (importNode.Name.ToLower().Contains("namespace") && importNode.ChildNodes.Count == 0)
                                            {
                                                var location = importNode.Attributes["location"].Value;
                                                var dependentSchemaOriginalName = location.Substring(location.LastIndexOf(".")).Remove(0, 1);
                                                var dependentSchemaObj = this.originalSchemDetailsList.First(r => r.schemaFullName == location);

                                                var dependentSchemaNewName = dependentSchemaObj.fullNameOfSchemaToUpload;
                                                if (dependentSchemaNewName.Length > 79)
                                                    dependentSchemaNewName = dependentSchemaNewName.Replace("_", "");
                                                importNode.Attributes["location"].Value = location.Replace(importNode.Attributes["location"].Value, ".\\" + dependentSchemaNewName + ".xsd");
                                                schemaXmlContext = schemaXmlContext.Replace("location=\"" + location + "\"", "location=\"" + importNode.Attributes["location"].Value + "\"");
                                                dependentSchemaList.Add(originalSchemDetailsList.First(r => r.fullNameOfSchemaToUpload == dependentSchemaNewName));
                                                thisSchemaObj.dependentSchemas.Add(dependentSchemaNewName);
                                                if (dependentSchemaObj.isSchemaExtractedFromDb == false)
                                                {
                                                    visited[dependentSchemaObj] = false;
                                                    TraceProvider.WriteLine($"Extracted an unselected dependency schema: {dependentSchemaObj.fullNameOfSchemaToUpload}");
                                                    Console.WriteLine($"Extracted an unselected dependency schema: {dependentSchemaObj.fullNameOfSchemaToUpload}");

                                                }
                                            }
                                        }
                                    }


                                }
                            }
                        }
                    }
                    // look for dependent schema in "import" XML tag
                    if (child.Name.ToLower().Contains("import") && child.ChildNodes.Count == 0)
                    {
                        var schemaLocation = child.Attributes["schemaLocation"].Value;

                        var dependentSchemaOriginalName = schemaLocation.Substring(schemaLocation.LastIndexOf(".")).Remove(0, 1);

                        var dependentSchemaObj = this.originalSchemDetailsList.First(r => r.schemaFullName == schemaLocation);

                        var m = schemaNodeChildren.GetElementsByTagName("import");
                        var dependentSchemaNewName = dependentSchemaObj.fullNameOfSchemaToUpload;



                        //var dependentSchemaNewName = dllName + "_" + dependentSchemaOriginalName;
                        if (dependentSchemaNewName.Length > 79)
                            dependentSchemaNewName = dependentSchemaNewName.Replace("_", "");

                        child.Attributes["schemaLocation"].Value = schemaLocation.Replace(child.Attributes["schemaLocation"].Value, ".\\" + dependentSchemaNewName + ".xsd");
                        schemaXmlContext = schemaXmlContext.Replace("schemaLocation=\"" + schemaLocation + "\"", "schemaLocation=\"" + child.Attributes["schemaLocation"].Value + "\"");
                        dependentSchemaList.Add(originalSchemDetailsList.First(r => r.fullNameOfSchemaToUpload == dependentSchemaNewName));
                        thisSchemaObj.dependentSchemas.Add(dependentSchemaNewName);
                        if (dependentSchemaObj.isSchemaExtractedFromDb == false)
                        {
                            visited[dependentSchemaObj] = false;
                            TraceProvider.WriteLine($"Extracted an unselected dependency schema: {dependentSchemaObj.fullNameOfSchemaToUpload}");
                           // Console.WriteLine($"Extracted an unselected dependency schema: {dependentSchemaObj.fullNameOfSchemaToUpload}");

                        }
                    }
                }
                catch (Exception e)
                {
                    string message = $"ERROR! Problem extracting dependencies from schema {thisSchemaObj.fullNameOfSchemaToUpload}. \nErrorMessage:{e.Message}";
                    TraceProvider.WriteLine(message + " \nStackTrace:{e.StackTrace}");
                    //Console.WriteLine(message + " \nStackTrace:{e.StackTrace}");

                    thisSchemaObj.errorDetailsForExtraction = thisSchemaObj.errorDetailsForExtraction + "\n" + message;
                }
            }

            var schemaFilePath = string.Format(outputDir + "\\{0}.xsd", thisSchemaObj.fullNameOfSchemaToUpload);
            File.WriteAllText(schemaFilePath, schemaXmlContext, Encoding.UTF8);

            return dependentSchemaList;

        }

    }
}
