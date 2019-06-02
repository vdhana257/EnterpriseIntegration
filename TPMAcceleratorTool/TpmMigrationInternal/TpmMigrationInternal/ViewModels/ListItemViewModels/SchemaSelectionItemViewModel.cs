using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchemaMigration;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class SchemaSelectionItemViewModel : SelectionItemViewModel<SchemaDetails>
    {
        public SchemaSelectionItemViewModel(SchemaDetails schema) : base(schema)
        {
        }
    }
}
