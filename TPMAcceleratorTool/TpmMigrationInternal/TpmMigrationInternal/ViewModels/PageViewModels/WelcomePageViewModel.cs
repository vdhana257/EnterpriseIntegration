//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System;
using System.Diagnostics;
using System.IO;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class WelcomePageViewModel : BaseViewModel, IPageViewModel
    {
        public string Title 
        { 
            get
            {
                return Resources.WelcomePageTitle;
            }
        }

        public string WelcomeText
        {
            get
            {
                return Resources.WelcomeText;
            }
        }

        private Command openDocumentLinkCommand;
        public Command OpenDocumentLinkCommand
        {
            get
            {
                if (this.openDocumentLinkCommand == null)
                {
                    this.openDocumentLinkCommand = new Command(
                        "here",
                        "OpenDocumentLinkCommand",
                        (o) => true,
                        (o) => this.OpenLink());
                }

                return this.openDocumentLinkCommand;
            }
        }

        private void OpenLink()
        {
            var filePath = Resources.DocumentationLink;
            try
            {
                if (File.Exists(filePath))
                {
                    Process.Start(filePath);
                }
            }
            catch(Exception ex)
            {
                TraceProvider.WriteLine("Error. Failed to open the Documentation. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
                var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                statusBarViewModel.ShowError("Error. Failed to open the Documentation. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
            }
        }
    }
}
