//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Collections.ObjectModel;
    using System.Configuration;

    public class BizTalkManagementDBDetails
    {

        private string databaseName;

        public string ServerName { get; set; }

        public string DatabaseName
        {
            get
            {
                    return databaseName;
            }

            set
            {
                databaseName = value;
            }
        }

        public bool IsIntegratedSecurity { get; set; }

        public string UserName { get; set; }

        public string Password { get; set; }

        public BizTalkManagementDBDetails()
        {
            DatabaseName = Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.DatabaseName;
            ServerName = Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.ServerName;
        }
    }
}