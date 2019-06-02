using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public class Mappings
    {
        public static Dictionary<string, string> hashingAlgorithmMappings
        {
            get
            {
                return GetHashingAlgorithmsMappings();
            }
        }

        public static Dictionary<string, string> GetHashingAlgorithmsMappings()
        {
            Dictionary<string, string> hashingAlgorithmMappings = new Dictionary<string, string>();
            hashingAlgorithmMappings.Add("SHA256", "SHA2256");
            hashingAlgorithmMappings.Add("SHA384", "SHA2384");
            hashingAlgorithmMappings.Add("SHA512", "SHA2512");
            return hashingAlgorithmMappings;
        }
    }
}
