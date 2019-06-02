//------------------------------------------------------------
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

    class PartnerMigrationItemViewModel : MigrationItemViewModel<Server.Partner>
    {
        private int count;
                     
        public PartnerMigrationItemViewModel(PartnerSelectionItemViewModel partnerItem) : base(partnerItem)
        {
            Server.Partner partner = partnerItem.MigrationEntity as Server.Partner;
            this.GetBusinessProfilesCount(partner);
            base.Name = partner.Name;
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }

            set
            {
                base.Name = value;
            }
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

        public bool CertificationRequired { get; set; }

        public MigrationStatus CertificateImportStatus { get; set; }

        public MigrationStatus CertificateExportStatus { get; set; }

        public string CertificateExportStatusText { get; set; }

        private void GetBusinessProfilesCount(Server.Partner p)
        {
            if (p != null)
            {
                this.count = p.GetBusinessProfiles().Count;
            }
        }
    }
}
