
using System;
namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.JsonPartner
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
        public string partnerType { get; set; }
        public Content content { get; set; }
        public DateTime createdTime { get; set; }
        public DateTime changedTime { get; set; }
        public Metadata metadata { get; set; }
    }

    public class Content
    {
        public B2b b2b { get; set; }
    }

    public class B2b
    {
        public Businessidentity[] businessIdentities { get; set; }
    }

    public class Businessidentity
    {
        public string qualifier { get; set; }
        public string value { get; set; }
    }

    public class Metadata
    {
        public MigrationAuditInformation migrationInfo { get; set; }
    }

}