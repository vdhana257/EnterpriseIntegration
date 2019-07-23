//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    /// <summary>
    /// Represents migration status of an individual entity (agreement/partner) as well as the collective migration status of all 
    /// partners and agreements that were selected for migration. 'Partial' is applicable to the collective migration status meaning that some 
    /// entities were successfully migrated while the rest failed.
    /// </summary>
    public enum MigrationStatus
    {
        NotStarted,
        Partial,
        Succeeded,
        Failed
    }
}
