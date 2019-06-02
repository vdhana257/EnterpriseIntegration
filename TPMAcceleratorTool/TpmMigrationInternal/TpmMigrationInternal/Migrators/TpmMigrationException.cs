//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;

    class TpmMigrationException : Exception
    {
        public TpmMigrationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        public TpmMigrationException(string message) : base(message)
        {
        }
    }
}
