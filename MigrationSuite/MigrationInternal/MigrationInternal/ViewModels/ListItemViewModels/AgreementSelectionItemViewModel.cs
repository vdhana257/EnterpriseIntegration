//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class AgreementSelectionItemViewModel : SelectionItemViewModel<Server.Agreement>
    {
        public AgreementSelectionItemViewModel(Server.Agreement agreement) : base(agreement)
        {
        }

    }
}
