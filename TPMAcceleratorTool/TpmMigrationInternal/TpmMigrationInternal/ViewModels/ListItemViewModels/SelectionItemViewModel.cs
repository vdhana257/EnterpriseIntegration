// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.ComponentModel;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    partial class SelectionItemViewModel<TEntity> : BaseViewModel where TEntity : class
    {
        private bool isSelected;
        private TEntity migrationEntity;

        public SelectionItemViewModel(TEntity item)
        {
            this.MigrationEntity = item;
            this.IsSelected = false;
        }

        public delegate void SelectionItemChangedEventHandler(object sender, EventArgs e);

        public event SelectionItemChangedEventHandler SelectionItemChangedEvent = delegate { };

        public TEntity MigrationEntity
        {
            get
            {
                return this.migrationEntity;
            }

            set
            {
                this.migrationEntity = value;
            }
        }

        public bool IsSelected
        {
            get
            {
                return this.isSelected;
            }

            set
            {
                this.isSelected = value;
                this.RaisePropertyChanged("IsSelected");
                this.SelectionItemChangedEvent(value, EventArgs.Empty);
            }
        }
    }
}
