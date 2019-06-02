using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SchemaMigration;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class SchemaMigrationPageViewModel : MigrationPageViewModel<SchemaMigrationItemViewModel, SchemaDetails>
    {
        private Command openSchemaDirCommand;
        public override string Title
        {
            get { return Resources.SchemaImportPageTitle; }
        }

        public override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllSchemasContextPropertyName; }
        }

        public override string MigrationItemsContextPropertyName
        {
            get { return AppConstants.SelectedSchemasContextPropertyName; }
        }

        public Command OpenSchemaDirCommand
        {
            get
            {
                if (this.openSchemaDirCommand == null)
                {
                    this.openSchemaDirCommand = new Command(
                        "here",
                        "OpenSchemaDirCommand",
                        (o) => true,
                        (o) => this.OpenDirectory());
                }

                return this.openSchemaDirCommand;
            }
        }

        private void OpenDirectory()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace(@"file:\", "") + "\\" + Resources.JsonSchemasFilesLocalPath.Replace("{0}{1}", "");

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}
