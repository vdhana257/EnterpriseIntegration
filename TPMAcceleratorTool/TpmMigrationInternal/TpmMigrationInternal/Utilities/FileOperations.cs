using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class FileOperations
    {
        public static string GetFileName(string Name)
        {
            string filePath = Name;

            if (!string.IsNullOrEmpty(filePath))
            {
                string[] partnerWords = filePath.Split(' ');
                while (partnerWords.Count() > 1)
                {
                    filePath = string.Join("", partnerWords);
                    partnerWords = filePath.Split(' ');
                }
            }
            string nameWithoutSpecialChars = StringOperations.RemoveSpecialCharacters(filePath);
            if (nameWithoutSpecialChars.Length > 80)
            {
                return nameWithoutSpecialChars.Substring(0, 80);
            }
            else
            {
                return nameWithoutSpecialChars;
            }
        }


        public static string GetLogFilePath()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(Resources.LogFilePath));
            string logFileName = "LogFile_" + DateTime.Now.ToFileTime().ToString() + ".txt";
            return string.Format(Resources.LogFilePath, logFileName);
        }
        public static string GetPartnerJsonFilePath(string partner)
        {
            var fileLocation = Resources.JsonPartnerFilesLocalPath;
            var fileName = string.Format(fileLocation, GetFileName(partner), ".json");
            return fileName;
        }

        public static string GetCertificateJsonFilePath(string certificate)
        {
            var fileLocation = Resources.JsonCertificateFilesLocalPath;
            var fileName = string.Format(fileLocation, GetFileName(certificate), ".json");
            return fileName;
        }

        public static string GetKeyVaultJsonFilePath(string keyvault)
        {
            var fileLocation = Resources.JsonKeyVaultFilesLocalPath;
            var fileName = string.Format(fileLocation, StringOperations.RemoveAllSpecialCharacters(GetFileName(keyvault)), ".json");
            return fileName;
        }
        public static string GetAgreementJsonFilePath(string agreement)
        {
            var fileLocation = Resources.JsonAgreementFilesLocalPath;
            var fileName = string.Format(fileLocation, GetFileName(agreement), ".json");
            return fileName;
        }

        public static string GetSchemaJsonFilePath(string schema)
        {
            var fileLocation = Resources.JsonSchemaFilesLocalPath;
            var fileName = string.Format(fileLocation, GetFileName(schema), ".json");
            return fileName;
        }


        public static void CreateFolder(string path)
        {
            DirectoryInfo di = Directory.CreateDirectory(Path.GetDirectoryName(path));
        }

        public static Dictionary<string, KeyValuePair<string,string>> ReadPartnerCertificateMappingFile()
        {

            Dictionary<string, KeyValuePair<string, string>> partnerCertificateMappings = new Dictionary<string, KeyValuePair<string, string>>();
            XmlDocument xmldoc = new XmlDocument();
            XmlNodeList xmlnode;
            try
            {
                FileStream fs = new FileStream(ConfigurationManager.AppSettings["PartnerCertMappingFilePath"], FileMode.Open, FileAccess.Read);
                xmldoc.Load(fs);
                xmlnode = xmldoc.SelectNodes("PartnerCertificateMappings/Mapping");
                for (int i = 0; i <= xmlnode.Count - 1; i++)
                {
                    KeyValuePair<string, string> kv = new KeyValuePair<string, string>(xmlnode[i].Attributes["certName"].Value.ToString(), xmlnode[i].Attributes["certThumbprint"].Value.ToString());
                    partnerCertificateMappings.Add(xmlnode[i].Attributes["key"].Value.ToString(),kv);
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
            return partnerCertificateMappings;
        }
    }
}
