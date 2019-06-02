//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    interface IPageViewModel
    {
        string Title { get; }

        void Initialize(IApplicationContext applicationContext);
    }

    interface IValidatorPageViewModel : IPageViewModel
    {
        bool Validate();
    }
}
