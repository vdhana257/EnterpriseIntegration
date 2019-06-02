//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Windows;
    using System.Windows.Input;

    public partial class MainWindow
    {
        public MainWindow()
        {
            ManagementWorkspaceView managementWorkspaceView = new ManagementWorkspaceView();
            this.Navigate(managementWorkspaceView);
        }

        private void MainWindow_OnMouseMove(object sender, MouseEventArgs e)
        {
        }
    }
}