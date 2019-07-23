using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using SchemaMigration;
    using MapMigration;
    using System.Windows;
    using System.Windows.Input;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class ExportToIntegrationAccountPageViewModel : BaseViewModel, IPageViewModel
    {
        private bool isSchemaSelected;
        private bool isMapSelected;
        private bool isCertSelected;
        private bool isPartnerSelected;
        private bool isAgmtSelected;
        private bool isSchemaExportEnabled;
        private bool isMapExportEnabled;
        private bool isCertExportEnabled;
        private bool isPartnerExportEnabled;
        private bool isAgmtExportEnabled;
        private bool isOverwriteSelected;
        private bool isOverwriteEnabled;

        private string schemaLabelContent;
        private string mapLabelContent;
        private string certLabelContent;
        private string partnerLabelContent;
        private string agmtLabelContent;

        private string schemasToExport;
        private string mapsToExport;
        private string certificatesToExport;
        private string partnersToExport;
        private string agreementsToExport;

        private PartnerMigrator pMigrator { get; set; }
        private AgreementMigrator aMigrator { get; set; }
        private SchemaMigrator sMigrator { get; set; }
        private MapMigrator mMigrator { get; set; }

        public IEnumerable<SchemaMigrationItemViewModel> selectedSchemas;
        public IEnumerable<SchemaMigrationItemViewModel> importedSchemas;

        public IEnumerable<MapMigrationItemViewModel> selectedMaps;
        public IEnumerable<MapMigrationItemViewModel> importedMaps;

        public IEnumerable<PartnerMigrationItemViewModel> selectedPartners;
        public IEnumerable<PartnerMigrationItemViewModel> importedPartners;

        public IEnumerable<AgreementMigrationItemViewModel> selectedAgreements;
        public IEnumerable<AgreementMigrationItemViewModel> importedAgreements;

        public List<Certificate> otherExtractedCerts;

        public string Title
        {
            get
            {
                return Resources.ExportToIAPageTitle;
            }
        }

        public string CertificatesToExport
        {
            get
            {
                return certificatesToExport;
            }
            set
            {
                certificatesToExport = value;
                this.RaisePropertyChanged("CertificatesToExport");
            }
        }

        public string SchemasToExport
        {
            get
            {
                return schemasToExport;
            }
            set
            {
                schemasToExport = value;
                this.RaisePropertyChanged("SchemasToExport");
            }
        }

        public string MapsToExport
        {
            get
            {
                return mapsToExport;
            }
            set
            {
                mapsToExport = value;
                this.RaisePropertyChanged("MapsToExport");
            }
        }

        public string PartnersToExport
        {
            get
            {
                return partnersToExport;
            }
            set
            {
                partnersToExport = value;
                this.RaisePropertyChanged("PartnersToExport");
            }
        }

        public string AgreementsToExport
        {
            get
            {
                return agreementsToExport;
            }
            set
            {
                agreementsToExport = value;
                this.RaisePropertyChanged("AgreementsToExport");
            }
        }
        public string MigrationWarningText
        {
            get
            {
                bool warning = Convert.ToBoolean(this.ApplicationContext.GetProperty("CertificateMigrationWarning"));
                string warningText = "";
                if (warning == true)
                {
                    warningText = "Warning: A few Certificate(s) need to be migrated before migrating the Agreement(s) through the tool. \nFailure to do so might result in failures in agreement(s) migration to IA.";
                    return warningText;
                }
                else
                {
                    return null;
                }

            }
        }

        public bool IsSchemaSelected
        {
            get
            {
                return this.isSchemaSelected;
            }

            set
            {
                this.isSchemaSelected = value;
                this.ApplicationContext.SetProperty("ExportSchemas", value);
                this.RaisePropertyChanged("IsSchemaSelected");
            }
        }

        public bool IsMapSelected
        {
            get
            {
                return this.isMapSelected;
            }

            set
            {
                this.isMapSelected = value;
                this.ApplicationContext.SetProperty("ExportMaps", value);
                this.RaisePropertyChanged("IsMapSelected");
            }
        }

        public bool IsCertSelected
        {
            get
            {
                return this.isCertSelected;
            }

            set
            {
                this.isCertSelected = value;
                this.ApplicationContext.SetProperty("ExportCertificates", value);
                this.RaisePropertyChanged("IsCertSelected");
            }
        }


        public bool IsPartnerSelected
        {
            get
            {
                return this.isPartnerSelected;
            }

            set
            {
                this.isPartnerSelected = value;
                this.ApplicationContext.SetProperty("ExportPartners", value);
                this.RaisePropertyChanged("IsPartnerSelected");
            }
        }

        public bool IsAgmtSelected
        {
            get
            {
                return this.isAgmtSelected;
            }

            set
            {
                this.isAgmtSelected = value;
                this.ApplicationContext.SetProperty("ExportAgreements", value);
                this.RaisePropertyChanged("IsAgmtSelected");
            }
        }

        public bool IsSchemaExportEnabled
        {
            get
            {
                return this.isSchemaExportEnabled;
            }

            set
            {
                this.isSchemaExportEnabled = value;
                this.RaisePropertyChanged("IsSchemaExportEnabled");
            }
        }

        public bool IsMapExportEnabled
        {
            get
            {
                return this.isMapExportEnabled;
            }

            set
            {
                this.isMapExportEnabled = value;
                this.RaisePropertyChanged("IsMapExportEnabled");
            }
        }

        public bool IsCertExportEnabled
        {
            get
            {
                return this.isCertExportEnabled;
            }

            set
            {
                this.isCertExportEnabled = value;
                this.RaisePropertyChanged("IsCertExportEnabled");
            }
        }

        public bool IsPartnerExportEnabled
        {
            get
            {
                return this.isPartnerExportEnabled;
            }

            set
            {
                this.isPartnerExportEnabled = value;
                this.RaisePropertyChanged("IsPartnerExportEnabled");
            }
        }

        public bool IsAgmtExportEnabled
        {
            get
            {
                return this.isAgmtExportEnabled;
            }

            set
            {
                this.isAgmtExportEnabled = value;
                this.RaisePropertyChanged("IsAgmtExportEnabled");
            }
        }


        public string SchemaLabelContent
        {
            get
            {
                return schemaLabelContent;
            }
            set
            {
                schemaLabelContent = value;
                this.RaisePropertyChanged("SchemaLabelContent");
            }
        }

        public string MapLabelContent
        {
            get
            {
                return mapLabelContent;
            }
            set
            {
                mapLabelContent = value;
                this.RaisePropertyChanged("MapLabelContent");
            }
        }

        public string CertLabelContent
        {
            get
            {
                return certLabelContent;
            }
            set
            {
                certLabelContent = value;
                this.RaisePropertyChanged("CertLabelContent");
            }
        }

        public string PartnerLabelContent
        {
            get
            {
                return partnerLabelContent;
            }
            set
            {
                partnerLabelContent = value;
                this.RaisePropertyChanged("PartnerLabelContent");
            }
        }


        public string AgmtLabelContent
        {
            get
            {
                return agmtLabelContent;
            }
            set
            {
                agmtLabelContent = value;
                this.RaisePropertyChanged("AgmtLabelContent");
            }
        }

        public bool IsOverwriteSelected
        {
            get
            {
                return isOverwriteSelected;
            }
            set
            {
                isOverwriteSelected = value;
                this.ApplicationContext.SetProperty("OverwriteEnabled", value);
                this.RaisePropertyChanged("IsOverwriteSelected");
            }
        }
        public bool IsOverwriteEnabled
        {
            get
            {
                return isOverwriteEnabled;
            }
            set
            {
                isOverwriteEnabled = value;
                this.RaisePropertyChanged("IsOverwriteEnabled");
            }
        }



        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            selectedSchemas = this.ApplicationContext.GetProperty(AppConstants.SelectedSchemasContextPropertyName) as IEnumerable<SchemaMigrationItemViewModel>;
            importedSchemas = selectedSchemas.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            selectedMaps = this.ApplicationContext.GetProperty(AppConstants.SelectedMapsContextPropertyName) as IEnumerable<MapMigrationItemViewModel>;
            importedMaps = selectedMaps.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            selectedPartners = this.ApplicationContext.GetProperty(AppConstants.SelectedPartnersContextPropertyName) as IEnumerable<PartnerMigrationItemViewModel>;
            if (applicationContext.GetProperty("MigrationCriteria").ToString() == "Partner")
                importedPartners = selectedPartners.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            selectedAgreements = this.ApplicationContext.GetProperty(AppConstants.SelectedAgreementsContextPropertyName) as IEnumerable<AgreementMigrationItemViewModel>;
            if (applicationContext.GetProperty("MigrationCriteria").ToString() == "Partner")
                importedAgreements = selectedAgreements.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            otherExtractedCerts = this.ApplicationContext.GetProperty("OtherCertificates") as List<Certificate>;
            var SchemasToUploadOrder = this.ApplicationContext.GetProperty("SchemasToUploadOrder") as List<SchemaDetails>;
            var MapsToUploadOrder = this.ApplicationContext.GetProperty("MapsToUploadOrder") as List<MapDetails>;
            StringBuilder sb1 = new StringBuilder();
            int i = 1;
            IsOverwriteEnabled = true;
            if (importedSchemas.Count(item => item.ImportStatus == MigrationStatus.Succeeded) != 0 && SchemasToUploadOrder != null)
            {
                IsSchemaExportEnabled = true;
                SchemaLabelContent = string.Format("({0} Schema(s) to Migrate)", importedSchemas.Count(item => item.ImportStatus == MigrationStatus.Succeeded));
                SchemasToExport = "Schema(s) to migrate are:\n";
                i = 1;
                importedSchemas.Where(item => item.ImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
                {
                    SchemasToExport = string.Concat(SchemasToExport, i, ") ", x.MigrationEntity.fullNameOfSchemaToUpload, "\n");
                    i = i + 1;
                });
                SchemasToExport = SchemasToExport.TrimEnd('\n');
            }
            else
            {
                IsSchemaExportEnabled = false;
                if (importedSchemas.Count(item => item.ImportStatus == MigrationStatus.Succeeded) == 0)
                {
                    SchemaLabelContent = "(No Schemas to Migrate)";
                }
                else
                {
                    SchemaLabelContent = "(Schemas can't be Migrated to IA)";
                    SchemasToExport = "Schemas cannot be migrated to IA as dependency oreder could not be known";
                }
            }

            i = 1;
            if (importedMaps.Count(item => item.ImportStatus == MigrationStatus.Succeeded) != 0 && MapsToUploadOrder != null)
            {
                IsMapExportEnabled = true;
                MapLabelContent = string.Format("({0} Map(s) to Migrate)", importedMaps.Count(item => item.ImportStatus == MigrationStatus.Succeeded));
                MapsToExport = "Map(s) to migrate are:\n";
                i = 1;
                importedMaps.Where(item => item.ImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
                {
                    MapsToExport = string.Concat(MapsToExport, i, ") ", x.MigrationEntity.fullNameOfMapToUpload, "\n");
                    i = i + 1;
                });
                MapsToExport = MapsToExport.TrimEnd('\n');
            }
            else
            {
                IsMapExportEnabled = false;
                if (importedMaps.Count(item => item.ImportStatus == MigrationStatus.Succeeded) == 0)
                {
                    MapLabelContent = "(No Maps to Migrate)";
                }
                else
                {
                    MapLabelContent = "(Maps can't be Migrated to IA)";
                    MapsToExport = "Maps cannot be migrated to IA as dependency order could not be known";
                }
            }

            //if (importedPartners.Count(item => item.CertificateImportStatus == MigrationStatus.Succeeded) != 0
            //    || (otherExtractedCerts != null && otherExtractedCerts.Count != 0
            //    && otherExtractedCerts.Count(item => item.ImportStatus == MigrationStatus.Succeeded) != 0))
            //{
            //    IsCertExportEnabled = true;
            //    int certs = 0;
            //    if (importedPartners != null)
            //    {
            //        certs += importedPartners.Count(item => item.CertificateImportStatus == MigrationStatus.Succeeded);
            //    }
            //    if (otherExtractedCerts != null)
            //    {
            //        certs += otherExtractedCerts.Count(item => item.ImportStatus == MigrationStatus.Succeeded);
            //    }
            //    CertLabelContent = string.Format("({0} Certificate(s) to Migrate)", certs);
            //    if (certs != 0)
            //    {
            //        CertificatesToExport = "Certificate(s) to migrate are:\n";
            //        i = 1;
            //        if (importedPartners != null)
            //        {
            //            importedPartners.Where(item => item.CertificateImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
            //            {
            //                CertificatesToExport = string.Concat(CertificatesToExport, i, ") ", x.MigrationEntity.CertificateName, "\n");
            //                i = i + 1;
            //            });
            //        }
            //        if (otherExtractedCerts != null)
            //        {
            //            otherExtractedCerts.Where(item => item.ImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
            //            {
            //                CertificatesToExport = string.Concat(CertificatesToExport, i, ") ", x.certificateName, "\n");
            //                i = i + 1;
            //            });
            //        }
            //        CertificatesToExport = CertificatesToExport.TrimEnd('\n');
            //    }
            //}
            //else
            //{
            //    IsCertExportEnabled = false;
            //    CertLabelContent = "(No Certificates to Migrate)";
            //}
            //if (importedPartners.Count(item => item.ImportStatus == MigrationStatus.Succeeded) != 0)
            //{
            //    IsPartnerExportEnabled = true;
            //    PartnerLabelContent = string.Format("({0} Partner(s) to Migrate)", importedPartners.Count(item => item.ImportStatus == MigrationStatus.Succeeded));
            //    PartnersToExport = "Partner(s) to migrate are:\n";
            //    i = 1;
            //    importedPartners.Where(item => item.ImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
            //    {
            //        PartnersToExport = string.Concat(PartnersToExport, i, ") ", x.MigrationEntity.Name, "\n");
            //        i = i + 1;
            //    });
            //    PartnersToExport = PartnersToExport.TrimEnd('\n');
            //}
            //else
            //{
            //    IsPartnerExportEnabled = false;
            //    PartnerLabelContent = "(No Partners to Migrate)";
            //}
            //if (importedAgreements.Count(item => item.ImportStatus == MigrationStatus.Succeeded) != 0)
            //{
            //    IsAgmtExportEnabled = true;
            //    AgmtLabelContent = string.Format("({0} Agreement(s) to Migrate)", importedAgreements.Count(item => item.ImportStatus == MigrationStatus.Succeeded));
            //    AgreementsToExport = "Agreement(s) to migrate are:\n";
            //    i = 1;
            //    importedAgreements.Where(item => item.ImportStatus == MigrationStatus.Succeeded).ToList().ForEach(x =>
            //    {
            //        AgreementsToExport = string.Concat(AgreementsToExport, i, ") ", x.MigrationEntity.Name, "\n");
            //        i = i + 1;
            //    });
            //    AgreementsToExport = AgreementsToExport.TrimEnd('\n');
            //}
            //else
            //{
            //    IsAgmtExportEnabled = false;
            //    AgmtLabelContent = "(No Agreements to Migrate)";
            //}

            //if (!IsCertExportEnabled && !IsPartnerExportEnabled && !IsAgmtExportEnabled)
            //{
            //    MessageBox.Show("Nothing to Migrate. Try Again.");
            //}
            this.ApplicationContext.SetProperty("ExportSchemas", false);
            this.ApplicationContext.SetProperty("ExportCertificates", false);
            this.ApplicationContext.SetProperty("ExportPartners", false);
            this.ApplicationContext.SetProperty("ExportAgreements", false);
            this.ApplicationContext.SetProperty("OverwriteEnabled", false);
            this.ApplicationContext.SetProperty("ExportMaps", false);
        }
    }
}
