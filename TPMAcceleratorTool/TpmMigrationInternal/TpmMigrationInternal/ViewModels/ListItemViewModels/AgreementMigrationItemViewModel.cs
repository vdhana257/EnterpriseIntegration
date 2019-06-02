//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Collections.Generic;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

     class AgreementMigrationItemViewModel : MigrationItemViewModel<Server.Agreement>
    {
        public AgreementMigrationItemViewModel(AgreementSelectionItemViewModel agreementItem)
            : base(agreementItem)
        {
            Server.Agreement agreement = agreementItem.MigrationEntity as Server.Agreement;
            base.Name = agreement.Name;
        }

        public AgreementMigrationItemViewModel(AgreementMigrationItemViewModel agreementItem) : base(agreementItem)
        {
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

        public string HostedPartnerName
        {
            get
            {
                return base.MigrationEntity.ReceiverDetails.Partner;
            }
        }

        public string GuestPartnerName
        {
            get
            {
                return base.MigrationEntity.SenderDetails.Partner;
            }
        }

        public string GuestPartnerTpmId { get; set; }
        public string GuestPartnerId { get; set; }
        public string HostPartnerId { get; set; }
        public string GuestPartnerQualifer { get; set; }
        public string HostPartnerQualifier { get; set; }
        public bool IsConsolidated { get; set; }
        public string Protocol { get; set; }
        public List<string> Transactions { get; set; }
        public List<AgreementMigrationItemViewModel> AgreementsToConsolidate { get; set; }
        public string BaseAgreementName { get; set; }

        public string BaseAgreementInIA { get; set; }
    }
}
