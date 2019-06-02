using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchemaMigration;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class SchemaMigrationItemViewModel : MigrationItemViewModel<SchemaDetails>
    {
        public SchemaMigrationItemViewModel(SchemaSelectionItemViewModel schemaItem) : base(schemaItem)
        {
            SchemaDetails schema = schemaItem.MigrationEntity as SchemaDetails;
           base.Name = schema.fullNameOfSchemaToUpload;
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
        }
    }
}
