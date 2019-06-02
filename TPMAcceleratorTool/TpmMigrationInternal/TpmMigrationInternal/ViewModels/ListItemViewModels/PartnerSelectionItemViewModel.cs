// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class PartnerSelectionItemViewModel : SelectionItemViewModel<Server.Partner>
    {
        private int count;
               
        public PartnerSelectionItemViewModel(Server.Partner partner) : base(partner)
        {
            this.GetBusinessProfilesCount(partner);
        }

        public int Count
        {
            get
            {
                return this.count;
            }

            set
            {
                this.count = value;
            }
        }

        private void GetBusinessProfilesCount(Server.Partner partner)
        {            
            if (partner != null)
            {
                this.count = partner.GetBusinessProfiles().Count;
            }
        }
    }
}
