using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapMigration;
using System.IO;
using System.Diagnostics;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class MapMigrationPageViewModel : MigrationPageViewModel<MapMigrationItemViewModel, MapDetails>
    {
        private Command openMapDirCommand;
        public override string Title
        {
            get { return Resources.MapImportPageTitle; }
        }

        public override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllMapsContextPropertyName; }
        }

        public override string MigrationItemsContextPropertyName
        {
            get { return AppConstants.SelectedMapsContextPropertyName; }
        }

        public Command OpenMapDirCommand
        {
            get
            {
                if (this.openMapDirCommand == null)
                {
                    this.openMapDirCommand = new Command(
                        "here",
                        "OpenMapDirCommand",
                        (o) => true,
                        (o) => this.OpenDirectory());
                }

                return this.openMapDirCommand;
            }
        }

        private void OpenDirectory()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace(@"file:\", "") + "\\" + Resources.JsonMapsFilesLocalPath.Replace("{0}{1}", "");

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}
