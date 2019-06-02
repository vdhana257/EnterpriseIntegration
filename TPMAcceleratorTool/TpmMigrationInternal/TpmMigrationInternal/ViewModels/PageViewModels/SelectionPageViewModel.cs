//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Globalization;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Input;

    abstract class SelectionPageViewModel<TItemViewModel, TEntity> : BaseViewModel, IPageViewModel
        where TEntity : class
        where TItemViewModel : SelectionItemViewModel<TEntity>
    {
        private ObservableCollection<TItemViewModel> selectionItems;
        private bool selectAll;
        public ICommand SearchButtonClickCommand { get; set; }


        public ProgressBarViewModel ProgressBarViewModel
        {
            get
            {
                return this.ApplicationContext.GetService<ProgressBarViewModel>();
            }
        }

        public ObservableCollection<TItemViewModel> SelectionItems
        {
            get
            {
                return this.selectionItems;
            }

            set
            {
                this.selectionItems = value;
                this.RaisePropertyChanged("SelectionItems");
            }
        }

        public abstract string Title { get; }

        public bool SelectAll
        {
            get
            {
                return this.selectAll;
            }

            set
            {
                this.selectAll = value;
                this.RaisePropertyChanged("SelectAll");
                foreach (var selectionListItem in this.SelectionItems)
                {
                    selectionListItem.IsSelected = this.selectAll;
                }
            }
        }

        protected abstract string SelectionItemsContextPropertyName { get; }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            var partnerType = this as PartnerSelectionPageViewModel;
            var agreementType = this as AgreementSelectionPageViewModel;
             var schemaType = this as SchemaSelectionPageViewModel;
            if (partnerType != null)
            {
                SearchButtonClickCommand = new RelayCommand(o =>  SearchButtonClick("PartnerSearchButton"));

            }
            if (agreementType != null)
            {
                this.SelectionItems = new ObservableCollection<TItemViewModel>();
                this.GetAllItems();
            }
            if (schemaType != null)
            {

                this.SelectionItems = new ObservableCollection<TItemViewModel>();
                this.GetAllItems();
            }
        }

        private void SearchButtonClick(object sender)
        {
            this.SelectionItems = new ObservableCollection<TItemViewModel>();
            this.GetAllItems();
        }

        protected abstract Task<IEnumerable<TEntity>> GetEntitiesAsync();
        protected abstract void CheckItemsList();

         // This function handles the Select All Functionality when the following scenario. 
         // 1. Select all selected - all gets selected
         // 2. two of them are unselected  or 1 of them is unselected
         // 3. select all should be de-selected
         // 4. Again when the ones unselected are selected selecte all should get selected again
        private void SelectionItemChanged(object sender, EventArgs e)
        {
            var isCheckboxSelected = false;
            if (sender != null)
            {
                isCheckboxSelected = (bool)sender;
            }

            if (isCheckboxSelected ^ this.selectAll == true)
            {
                if (isCheckboxSelected == false && this.selectAll == true)
                {
                    this.selectAll = false;
                }

                if (isCheckboxSelected == true)
                {
                    foreach (var selectionItem in this.SelectionItems)
                    {
                        if (selectionItem.IsSelected == false)
                        {
                            this.selectAll = false;
                            this.RaisePropertyChanged("SelectAll");
                            return;
                        }
                    }

                    this.selectAll = true;
                }

                this.RaisePropertyChanged("SelectAll");
            }
        }

        private void GetAllItems()
        {
            string progressBarText = string.Empty;
            var partnerType = this as PartnerSelectionPageViewModel;
            var agreementType = this as AgreementSelectionPageViewModel;
            var schemaType = this as SchemaSelectionPageViewModel;
            if (partnerType != null)
            {
                progressBarText = Resources.ReadingPartnersProgressBarText;
            }

            if (agreementType != null)
            {
                progressBarText = Resources.ReadingAgreementsProgressBarText;
            }
            if(schemaType !=  null)
            {
                progressBarText = Resources.ReadingSchemasProgressBarText;
            }
            this.ProgressBarViewModel.Update(true, progressBarText);

            this.GetEntitiesAsync().ContinueWith(this.OnGetAllItemsComplete, TaskScheduler.FromCurrentSynchronizationContext());
        }


        private void OnGetAllItemsComplete(Task<IEnumerable<TEntity>> getItemsTask)
        {
            this.ProgressBarViewModel.Update(false);
            if (getItemsTask.Exception != null)
            {
                var tpmMigrationException =
                    new Exception(Resources.ErrorReadingTpmConfig, getItemsTask.Exception.InnerException);
                this.RaiseFatalError(tpmMigrationException);
            }

            try
            {
                var items = getItemsTask.Result;
                if (items != null)
                {
                    foreach (var item in items)
                    {
                        TItemViewModel selectionItemViewModel =
                            Activator.CreateInstance(typeof(TItemViewModel), item) as TItemViewModel;

                        this.SelectionItems.Add(selectionItemViewModel);
                    }

                    foreach (var selectionItem in this.SelectionItems)
                    {
                        selectionItem.SelectionItemChangedEvent += new SelectionItemViewModel<TEntity>.SelectionItemChangedEventHandler(this.SelectionItemChanged);
                    }
                }
            }
            catch (Exception ex)
            {
                var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
                statusBarViewModel.StatusInfoType = StatusInfoType.Error;
                statusBarViewModel.ShowError("Error: " + ExceptionHelper.GetExceptionMessage(ex));
            }
            this.ApplicationContext.SetProperty(this.SelectionItemsContextPropertyName, this.SelectionItems);
            this.CheckItemsList();
        }

    }
}