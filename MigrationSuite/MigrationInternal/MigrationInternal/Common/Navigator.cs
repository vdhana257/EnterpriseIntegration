//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;

    public class Navigator
    {
        Func<Uri, bool, bool> validateCurrentPageAction;
        Action closeApplicationAction; 

        public Navigator(Func<Uri, bool, bool> validateCurrentPageAction, Action closeApplicationAction)
        {
            this.validateCurrentPageAction = validateCurrentPageAction;
            this.closeApplicationAction = closeApplicationAction;
        }

        public bool Navigate(Uri uri, bool validateCurrentPageAction = false)
        {
            if (this.validateCurrentPageAction != null)
            {
                return this.validateCurrentPageAction(uri, validateCurrentPageAction);
            }

            return false;
        }

        public void CloseApplication()
        {
            if (this.closeApplicationAction != null)
            {
                this.closeApplicationAction();
            }
        }
    }
}
