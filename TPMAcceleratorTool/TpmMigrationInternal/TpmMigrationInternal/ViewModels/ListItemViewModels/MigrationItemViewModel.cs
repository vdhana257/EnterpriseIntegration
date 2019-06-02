//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class MigrationItemViewModel<TEntity> : BaseViewModel where TEntity : class
    {
        private MigrationStatus importStatus;
        private MigrationStatus exportSatus;
        private TEntity migrationEntity;
        private string importStatusText;
        private string exportStatusText;
        private string name;

        public MigrationItemViewModel(SelectionItemViewModel<TEntity> selectionItemViewModel)
        {
            this.ImportStatus = MigrationStatus.NotStarted;
            this.ExportStatus = MigrationStatus.NotStarted;
            this.MigrationEntity = selectionItemViewModel.MigrationEntity;
        }

        public MigrationItemViewModel(MigrationItemViewModel<TEntity> migrationItemViewModel)
        {
            this.ImportStatus = MigrationStatus.NotStarted;
            this.ExportStatus = MigrationStatus.NotStarted;
            this.MigrationEntity = migrationItemViewModel.MigrationEntity;
        }
        public MigrationStatus ImportStatus
        {
            get
            {
                return this.importStatus;
            }

            set
            {
                this.importStatus = value;
                this.RaisePropertyChanged("ImportStatus");
            }
        }

        public MigrationStatus ExportStatus
        {
            get
            {
                return this.exportSatus;
            }

            set
            {
                this.exportSatus = value;
                this.RaisePropertyChanged("ExportStatus");
            }
        }

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

        public virtual string Name
        {
            get
            {
                return this.name;      
            }

            set
            {
                this.name = value;
            }
        }

        public string ImportStatusText
        {
            get
            {
                return this.importStatusText;
            }

            set
            {
                this.importStatusText = value;
                this.RaisePropertyChanged("ImportStatusText");
            }
        }

        public string ExportStatusText
        {
            get
            {
                return this.exportStatusText;
            }

            set
            {
                this.exportStatusText = value;
                this.RaisePropertyChanged("ExportStatusText");
            }
        }
    }
}