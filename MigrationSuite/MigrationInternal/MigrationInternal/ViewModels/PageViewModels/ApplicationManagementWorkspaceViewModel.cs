//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;

    internal class ApplicationManagementWorkspaceViewModel : BaseViewModel
    {
        public ApplicationManagementWorkspaceViewModel(Navigator navigator)
        {
            this.ProgressBarViewModel = new ProgressBarViewModel();
            this.ApplicationWizardNavigationViewModel = new ApplicationWizardNavigationViewModel(navigator);
            this.StatusBarViewModel = new StatusBarViewModel();
        }

        public ProgressBarViewModel ProgressBarViewModel { get; private set; }

        public ApplicationWizardNavigationViewModel ApplicationWizardNavigationViewModel { get; private set; }

        public StatusBarViewModel StatusBarViewModel { get; set; }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            this.ApplicationWizardNavigationViewModel.Initialize(applicationContext);

            // Trace will me created in a unique folder for every run. Hence a new Trace Listener is added which will flush to the generated folder.
            var logFilePath = FileOperations.GetLogFilePath();
            TextWriterTraceListener newListener = new TextWriterTraceListener(string.Format(CultureInfo.InvariantCulture, logFilePath));
            Trace.Listeners.Add(newListener);
            Trace.AutoFlush = true;
            this.ApplicationContext.SetProperty("LogFilePath", logFilePath);
            this.ApplicationContext.SetProperty("MigrationCriteria", "Application");
        }

        public void ClearStatus()
        {
            this.StatusBarViewModel.Clear();
        }

        // Function generates a unique trace text file path for migration log.
        private string GenerateUniquePath()
        {
            string uniquePath;
            Guid guid = Guid.NewGuid();
            string uniqueFilePrefixPath = guid.ToString();
            uniquePath = Path.GetTempPath() + uniqueFilePrefixPath + "_" + "Migration.log";
            return uniquePath;
        }
    }
}
