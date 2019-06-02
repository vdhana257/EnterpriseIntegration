using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public enum PublicCertificateStore
    {
        AddressBook = 1,
        Root = 2,
        TrustedPeople = 3,
        TrustedPublisher = 4,
        My = 5
    }

    public enum PrivateCertificateStore
    {
        My = 1
    }
}
