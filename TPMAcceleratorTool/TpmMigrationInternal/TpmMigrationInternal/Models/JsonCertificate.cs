using System;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.JsonCertificate
{
    public class Rootobject
    {
        public Properties properties { get; set; }
        public string id { get; set; }
        public string name { get; set; }
        public string type { get; set; }
    }

    public class Properties
    {
        public Key key { get; set; }
        public string publicCertificate { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime changedTime { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Key
    {
        public KeyVault keyVault { get; set; }

        public string keyName { get; set; }

        public string keyVersion { get; set; }
    }

    public class KeyVault
    {
        public string name { get; set; }

        public string id { get; set; }

        public string type = "Microsoft.KeyVault/vaults";
    }
}
