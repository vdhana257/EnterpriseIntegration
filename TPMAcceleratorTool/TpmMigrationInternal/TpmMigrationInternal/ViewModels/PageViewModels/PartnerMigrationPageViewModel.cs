//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Diagnostics;
    using System.IO;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class PartnerMigrationPageViewModel : MigrationPageViewModel<PartnerMigrationItemViewModel, Server.Partner>
    {
        private Command openPartnerDirCommand;
        public override string Title
        {
            get
            {
                return Resources.PartnerImportPageTitle;
            }
        }

        public override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllPartnersContextPropertyName; }
        }

        public override string MigrationItemsContextPropertyName
        {
            get { return AppConstants.SelectedPartnersContextPropertyName; }
        }

        public Command OpenPartnerDirCommand
        {
            get
            {
                if (this.openPartnerDirCommand == null)
                {
                    this.openPartnerDirCommand = new Command(
                        "here",
                        "OpenPartnerDirCommand",
                        (o) => true,
                        (o) => this.OpenDirectory());
                }

                return this.openPartnerDirCommand;
            }
        }

        private void OpenDirectory()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace(@"file:\", "") + "\\" + Resources.JsonPartnerFilesLocalPath.Replace("{0}{1}", "");

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }

    }
}