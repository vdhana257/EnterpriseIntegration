using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class ImportedArtifact : BaseViewModel
    {
        private string artifactName;
        private MigrationStatus importStatus;
        private ArtifactTypes artifactType;
        private string importStatustext;

        public string ArtifactName
        {
            get
            {
                return artifactName;
            }
            set
            {
                artifactName = value;
                this.RaisePropertyChanged("ArtifactName");
            }
        }
        public MigrationStatus ImportStatus
        {
            get
            {
                return importStatus;
            }
            set
            {
                importStatus = value;
                this.RaisePropertyChanged("ImportStatus");
            }
        }

        public ArtifactTypes ArtifactType
        {
            get
            {
                return artifactType;
            }
            set
            {
                artifactType = value;
                this.RaisePropertyChanged("ArtifactType");
            }
        }

        public string ImportStatusText
        {
            get
            {
                return importStatustext;
            }
            set
            {
                importStatustext = value;
                this.RaisePropertyChanged("ImportStatusText");
            }
        }
    }
}
