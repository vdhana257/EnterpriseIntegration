//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using System.Security.Cryptography.X509Certificates;
    using System.Xml;
    using System.IO;
    using System.Xml.Serialization;
    using System.Windows;

    class PartnerSelectionPageViewModel : SelectionPageViewModel<PartnerSelectionItemViewModel, Server.Partner>
    {
        private bool _dataGridEnabled;
        public override string Title
        {
            get
            {
                return Resources.PartnerSelectionPageTitle;
            }
        }

        private string partnerFilter;
         
        public string PartnerFilter
        {
            get
            {
                return partnerFilter;
            }
            set
            {
                partnerFilter = value;
                this.RaisePropertyChanged("PartnerFilter");
            }
        }


        public bool PartnerDataGridEnabled
        {
            get { return _dataGridEnabled; }
            set
            {
                _dataGridEnabled = value;
                this.RaisePropertyChanged("PartnerDataGridEnabled");
            }
        }

        protected override string SelectionItemsContextPropertyName
        {
            get { return AppConstants.AllPartnersContextPropertyName; }
        }

        protected override void CheckItemsList()
        {
            if (this.SelectionItems == null)
            {
                MessageBox.Show("No Partners found matching the filter criteria. Please try again.");
                this.PartnerDataGridEnabled = false;
            }
            else if(this.SelectionItems.Count == 0)
            {
                MessageBox.Show("No Partners found matching the filter criteria. Please try again.");
                this.PartnerDataGridEnabled = false;
            }
            else
            {
                this.PartnerDataGridEnabled = true;
            }
        }

        protected override Task<IEnumerable<Server.Partner>> GetEntitiesAsync()
        {
            PartnerDataGridEnabled = false;
            var bizTalkTpmContext = this.ApplicationContext.GetBizTalkServerTpmContext();
            return Task.Factory.StartNew<IEnumerable<Server.Partner>>(() =>
                {
                    try
                    {
                        //var partnerships = bizTalkTpmContext.Partners.Where(X => X.Name.Contains("PC Connect")).First<Server.Partner>().GetPartnerships();
                        // var str = partnerships.First().GetAgreements();
                        if (!string.IsNullOrEmpty(PartnerFilter))                        
                        {
                            string filter = partnerFilter.ToString();
                            return bizTalkTpmContext.Partners.Where(x => x.Name.ToLower().Contains(filter.ToLower())).OrderBy(x => x.Name).ToList();
                        }
                        else
                        {
                            return bizTalkTpmContext.Partners.OrderBy(x => x.Name).ToList();
                        }

                    }

                    catch (Exception ex)
                    {
                        TraceProvider.WriteLine(string.Format(CultureInfo.InvariantCulture, "Not able to retrieve partners. {0}", ExceptionHelper.GetExceptionMessage(ex)));
                        var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                        statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                        statusBarViewModel.ShowError("Error. Failed to retrieve partners from Biztalk Server. Reason: " + ExceptionHelper.GetExceptionMessage(ex));
                        return new List<Server.Partner>();
                    }
                });
        }
        
    }
}

