using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    public enum ArtifactTypes
    {
        Schema,
        Certificate,
        Partner,
        Agreement
    }
    class ExportedArtifact : BaseViewModel
    {
        private string artifactName;
        private MigrationStatus exportStatus;
        private ArtifactTypes artifactType;
        private string exportStatustext;

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
        public MigrationStatus ExportStatus
        {
            get
            {
                return exportStatus;
            }
            set
            {
                exportStatus = value;
                this.RaisePropertyChanged("ExportStatus");
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

        public string ExportStatusText
        {
            get
            {
                return exportStatustext;
            }
            set
            {
                exportStatustext = value;
                this.RaisePropertyChanged("ExportStatusText");
            }
        }
    }
}
