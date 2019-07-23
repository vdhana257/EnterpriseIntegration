//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.JsonAs2Agreement;
    using System;
    using System.Windows;
    using System.Globalization;

    class AgreementSelectionPageViewModel : SelectionPageViewModel<AgreementSelectionItemViewModel, Server.Agreement>
    {
        private bool _dataGridEnabled;
        private bool isConsolidationSelected;
        private bool isContextGenerationSelected;
        public override string Title
        {
            get { return Resources.AgreementSelectionPageTitle; }
        }

        protected override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllAgreementsContextPropertyName; }
        }

        public bool AgreementDataGridEnabled
        {
            get { return _dataGridEnabled; }
            set
            {
                _dataGridEnabled = value;
                this.RaisePropertyChanged("AgreementDataGridEnabled");
            }
        }

        public bool IsContextGenerationSelected
        {
            get
            {
                return isContextGenerationSelected;
            }
            set
            {
                isContextGenerationSelected = value;
                this.ApplicationContext.SetProperty(AppConstants.ContextGenerationEnabled, isContextGenerationSelected);
                this.RaisePropertyChanged("IsContextGenerationSelected");
            }

        }

        public bool IsConsolidationSelected
        {
            get
            {
                return isConsolidationSelected;
            }
            set
            {
                isConsolidationSelected = value;
                this.ApplicationContext.SetProperty(AppConstants.ConsolidationEnabled, isConsolidationSelected);
                this.RaisePropertyChanged("IsConsolidationSelected");
            }

        }

        protected override void CheckItemsList()
        {
            if (this.SelectionItems == null)
            {
                MessageBox.Show("No Agreement(s) found corresponding to imported Partners. Please try again.");
                this.AgreementDataGridEnabled = false;
            }
            else if (this.SelectionItems.Count == 0)
            {
                MessageBox.Show("No Agreement(s) found corresponding to imported Partners. Please try again.");
                this.AgreementDataGridEnabled = false;
            }
            else
            {
                this.AgreementDataGridEnabled = true;
            }          
        }

        protected override Task<IEnumerable<Server.Agreement>> GetEntitiesAsync()
        {

            var bizTalkTpmContext = this.ApplicationContext.GetBizTalkServerTpmContext();
            var selectedPartners = this.ApplicationContext.GetProperty("SelectedPartners") as IEnumerable<PartnerMigrationItemViewModel>;
            List<string> selectedPartnersName = new List<string>();
            List<Server.Agreement> agreements = new List<BizTalk.B2B.PartnerManagement.Agreement>();
            List<Server.Partnership> partnerships = new List<Server.Partnership>();
            if (selectedPartners != null)
            {
                foreach (var partner in selectedPartners)
                {
                    if (partner.ImportStatus == MigrationStatus.Succeeded)
                    {
                        selectedPartnersName.Add(partner.Name);
                    }
                }
            }

            return Task.Factory.StartNew<IEnumerable<Server.Agreement>>(() =>
                {
                    try
                {
                    if (selectedPartnersName.Count != 0)
                        {
                            var partners = bizTalkTpmContext.Partners.Where(X => selectedPartnersName.Contains(X.Name));
                            foreach (var partner in partners)
                            {
                                partnerships.AddRange(partner.GetPartnerships());
                            }
                            foreach (var partnership in partnerships)
                            {
                                agreements.AddRange(partnership.GetAgreements());
                            }
                        }
                        else
                        {
                            agreements.AddRange(bizTalkTpmContext.Agreements.Where(x => x.Protocol == "x12" || x.Protocol == "as2" || x.Protocol == "edifact").Take(10).OrderBy(x => x.Name).ToList());
                        }

                        agreements = agreements.Distinct().ToList();
                    }
                    catch (Exception ex)
                    {
                        TraceProvider.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not able to retrieve agreements. {0}", ExceptionHelper.GetExceptionMessage(ex)));
                        var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                        statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                        statusBarViewModel.ShowError("Error. Failed to retrieve agreements from Biztalk Server. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
                    }
                    return agreements;
                });
            
        }
       
    }
}


