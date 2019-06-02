//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;

    class ErrorEventArgs
    {
        public ErrorEventArgs(Exception e)
        {
            this.Error = e;
        }

        public Exception Error { get; set; }
    }
}
