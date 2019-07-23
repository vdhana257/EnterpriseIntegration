//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using MigrationTool;
    using System.Windows;
    using System.Windows.Input;

    public partial class MainWindow
    {
        public MainWindow()
        {
            HomePage hp = new HomePage();
            this.Navigate(hp);
            //ManagementWorkspaceView managementWorkspaceView = new ManagementWorkspaceView();
            //this.Navigate(managementWorkspaceView);
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}