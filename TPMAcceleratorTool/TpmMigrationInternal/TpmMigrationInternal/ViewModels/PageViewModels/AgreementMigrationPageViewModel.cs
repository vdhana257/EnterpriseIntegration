//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Diagnostics;
    using System.IO;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class AgreementMigrationPageViewModel : MigrationPageViewModel<AgreementMigrationItemViewModel, Server.Agreement>
    {
        private Command openAgreementDirCommand;
        public override string Title
        {
            get { return Resources.AgreementImportPageTitle; }
        }

        public override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllAgreementsContextPropertyName; }
        }

        public override string MigrationItemsContextPropertyName
        {
            get { return AppConstants.SelectedAgreementsContextPropertyName; }
        }


        public Command OpenAgreementDirCommand
        {
            get
            {
                if (this.openAgreementDirCommand == null)
                {
                    this.openAgreementDirCommand = new Command(
                        "here",
                        "OpenAgreementDirCommand",
                        (o) => true,
                        (o) => this.OpenDirectory());
                }

                return this.openAgreementDirCommand;
            }
        }

        private void OpenDirectory()
        {
            var path = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase);
            path = path.Replace(@"file:\", "") + "\\" + Resources.JsonAgreementFilesLocalPath.Replace("{0}{1}", "");

            if (Directory.Exists(path))
            {
                Process.Start(path);
            }
        }
    }
}