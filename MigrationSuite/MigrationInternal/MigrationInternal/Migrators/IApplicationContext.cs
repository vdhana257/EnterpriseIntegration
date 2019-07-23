//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    public interface IApplicationContext
    {
        NavigateAction LastNavigation { get; set; }

        Exception ApplicationException { get; set; }

        Server.TpmContext GetBizTalkServerTpmContext();

        object GetProperty(string name);

        void SetProperty(string name, object value); 
        
        T GetService<T>() where T : class;

        void AddService<T>(T service) where T : class;
    }
}
