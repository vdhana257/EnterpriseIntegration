//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Services.Client;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Xml.Linq;

    abstract class MigrationPageViewModel<TItemViewModel, TEntity> : BaseViewModel, IPageViewModel
        where TEntity : class
        where TItemViewModel : MigrationItemViewModel<TEntity>
    {
        public abstract string Title { get; }

        public ProgressBarViewModel ProgressBarViewModel
        {
            get
            {
                return this.ApplicationContext.GetService<ProgressBarViewModel>();
            }
        }

        public ObservableCollection<AgreementMigrationItemViewModel> MigrationAgreements { get; private set; }
        public ObservableCollection<PartnerMigrationItemViewModel> MigrationPartners { get; private set; }
        public ObservableCollection<SchemaMigrationItemViewModel> MigrationSchemas { get; private set; }
        public ObservableCollection<TItemViewModel> MigrationItems{ get; private set; }
        public abstract string SelectionItemsContextPropertyName { get; }

        public abstract string MigrationItemsContextPropertyName { get; }

        private TpmMigrator<TItemViewModel, TEntity> Migrator { get; set; }

        public override void Initialize(IApplicationContext applicationContext)
        {
             base.Initialize(applicationContext);

            this.Migrator = TpmMigrator<TItemViewModel, TEntity>.CreateMigrator(applicationContext);

            var cachedItems =
                this.ApplicationContext.GetProperty(this.MigrationItemsContextPropertyName) as ObservableCollection<TItemViewModel>;
            if (this.ApplicationContext.LastNavigation != NavigateAction.Next && cachedItems != null)
            {
                this.MigrationItems = cachedItems;
            }
            else
            {
                var selectionItems = this.ApplicationContext.GetProperty(this.SelectionItemsContextPropertyName) as IEnumerable<SelectionItemViewModel<TEntity>>;
                IEnumerable<SelectionItemViewModel<TEntity>> selectedItems = null;
                if (selectionItems != null && selectionItems.Count() != 0)
                {
                    selectedItems = selectionItems.Where(item => item.IsSelected);
                    if ((selectedItems == null || !selectedItems.Any()))
                    {
                        var lastNavigation = this.ApplicationContext.LastNavigation;
                        if (lastNavigation == NavigateAction.Next)
                        {
                            var item = this as SchemaMigrationPageViewModel;
                            if (item == null)
                            {
                                MessageBox.Show("Nothing selected. Please select to proceed. ");
                                this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToPreviousStep();
                            }
                            else
                            {
                                this.MigrationItems = new ObservableCollection<TItemViewModel>();
                                this.ApplicationContext.SetProperty(this.MigrationItemsContextPropertyName, this.MigrationItems);
                                this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToNextStep();
                            }
                            
                        }
                        else if (lastNavigation == NavigateAction.Previous)
                        {
                            this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToPreviousStep();
                        }
                    }
                    else
                    {
                        try
                        {
                            this.MigrationItems = new ObservableCollection<TItemViewModel>(selectedItems.Select(selectedItem =>
                                                  Activator.CreateInstance(typeof(TItemViewModel), selectedItem) as TItemViewModel));

                            var agmtItem = this as AgreementMigrationPageViewModel;               
                            if (agmtItem != null)
                            {
                                bool consolidateAgreements = Convert.ToBoolean(this.ApplicationContext.GetProperty(AppConstants.ConsolidationEnabled));
                                if (consolidateAgreements)
                                {
                                    AgreementMigrator agmtMigrator = new AgreementMigrator(this.ApplicationContext);
                                    var listConsolidatedAgreements = agmtMigrator.CheckAgreementsToBeConsolidated(this.MigrationItems.ToList() as List<AgreementMigrationItemViewModel>);
                                    var listFinalAgreements = agmtMigrator.GetAgreementsList(this.MigrationItems.ToList() as List<AgreementMigrationItemViewModel>, listConsolidatedAgreements);
                                    this.MigrationItems = new ObservableCollection<TItemViewModel>(listFinalAgreements as List<TItemViewModel>);
                                }
                            }
                        }
                        catch(Exception ex)
                        {
                            var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                            statusBarViewModel.ShowError("Error while trying to identify agreements to consolidate. Reason : " + ExceptionHelper.GetExceptionMessage(ex));
                        }
                        this.ApplicationContext.SetProperty(this.MigrationItemsContextPropertyName, this.MigrationItems);
                        this.MigrateItems();
                    }
                }
                else
                {
                    var lastNavigation = this.ApplicationContext.LastNavigation;
                    if (lastNavigation == NavigateAction.Next)
                    {
                        var item = this as SchemaMigrationPageViewModel;
                        if (item == null)
                        {
                            MessageBox.Show("Nothing to Import.");
                            this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToPreviousStep();
                        }
                        else
                        {
                            this.MigrationItems = new ObservableCollection<TItemViewModel>();
                            this.ApplicationContext.SetProperty(this.MigrationItemsContextPropertyName, this.MigrationItems);
                            this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToNextStep();
                        }
                    }
                    else if (lastNavigation == NavigateAction.Previous)
                    {
                        this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToPreviousStep();
                    }
                }
            }
        }

        public async void MigrateItems()
        {
            var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>(); 
            
            string progressBarText = string.Empty;
            var partnerType = this as PartnerMigrationPageViewModel;
            var agreementType = this as AgreementMigrationPageViewModel;
            var schemaType = this as SchemaMigrationPageViewModel;
            if (partnerType != null)
            {
                progressBarText = Resources.ImportingPartnersProgressBarText;
                this.ProgressBarViewModel.Update(true, progressBarText);
            }

            if (agreementType != null)
            {
                progressBarText = Resources.ImportingAgreementsProgressBarText;
                this.ProgressBarViewModel.Update(true, progressBarText);

            }

            if(schemaType != null)
            {
                progressBarText = Resources.ImportingSchemasProgressBarText;
                this.ProgressBarViewModel.Update(true, progressBarText);
            }

            if (schemaType != null)
            {
                try
                {
                    SchemaMigrator sMigrator = new SchemaMigrator(this.ApplicationContext);
                    await sMigrator.CreateSchemas(this.MigrationItems.ToList() as List<SchemaMigrationItemViewModel>);
                }
                catch (Exception ex)
                {
                    statusBarViewModel.ShowError("(Error) :"+ ExceptionHelper.GetExceptionMessage(ex));
                }
            }
            else
            {
                foreach (var migrationItem in this.MigrationItems)
                {
                    try
                    {
                        await Migrator.ImportAsync(migrationItem);
                        if (migrationItem.ImportStatus == MigrationStatus.Partial)
                        {
                            migrationItem.ImportStatusText = Resources.MigrationPartialMessageText;
                        }


                    }
                    catch (Exception ex)
                    {
                        migrationItem.ImportStatus = MigrationStatus.Failed;
                        migrationItem.ImportStatusText = ExceptionHelper.GetExceptionMessage(ex);
                        this.Migrator.RefreshContext();
                    }
                }
            }
            foreach (var item in this.MigrationItems)
            {
                if (item.ImportStatus == MigrationStatus.Failed)
                {
                    statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                    statusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                    break;
                }
            }
            this.ProgressBarViewModel.Update(false);
        }
    }
}
