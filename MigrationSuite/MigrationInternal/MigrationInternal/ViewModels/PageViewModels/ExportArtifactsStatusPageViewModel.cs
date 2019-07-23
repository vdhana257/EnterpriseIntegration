using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class ExportArtifactsStatusPageViewModel : BaseViewModel, IPageViewModel
    {
        private CerificateMigrator<PartnerMigrationItemViewModel> c1Migrator { get; set; }
        private CerificateMigrator<Certificate> c2Migrator { get; set; }
        private PartnerMigrator pMigrator { get; set; }
        private AgreementMigrator aMigrator { get; set; }
        private SchemaMigrator sMigrator { get; set; }
        private MapMigrator mMigrator { get; set; }
        public ObservableCollection<ExportedArtifact> ExportedItems { get; private set; }

        public IEnumerable<SchemaMigrationItemViewModel> selectedSchemas;
        public IEnumerable<SchemaMigrationItemViewModel> importedSchemas;
        public IEnumerable<PartnerMigrationItemViewModel> selectedPartners;
        public IEnumerable<PartnerMigrationItemViewModel> importedPartners;
        public IEnumerable<MapMigrationItemViewModel> selectedMaps;
        public IEnumerable<MapMigrationItemViewModel> importedMaps;
        public IEnumerable<PartnerMigrationItemViewModel> importedPartnersWithCertificates;

        public IEnumerable<AgreementMigrationItemViewModel> selectedAgreements;
        public IEnumerable<AgreementMigrationItemViewModel> importedAgreements;
        public List<Certificate> otherCerts;
        public IEnumerable<Certificate> otherExtractedCerts;

        private bool ExportSchemasFlag { get; set; }
        private bool ExportCertificatesFlag { get; set; }
        private bool ExportPartnersFlag { get; set; }
        private bool ExportAgreementsFlag { get; set; }
        private bool ExportMapsFlag { get; set; }

        private int countArtifactsMigrated;
        private int countTotalArtifactsToMigrate;

        private double progress;
        private string progressBarStatusText;
        private bool progressVisible;

        public int TotalArtifacts { get; set; }

        public string Title
        {
            get
            {
                return Resources.ExportArtifactsStatusPageTitle;
            }
        }

        public StatusBarViewModel StatusBarViewModel
        {
            get
            {
                return this.ApplicationContext.GetService<StatusBarViewModel>();
            }
        }

        public double Progress
        {
            get
            {

                return progress;
            }
            set
            {
                progress = value;
                this.RaisePropertyChanged("Progress");
            }
        }
        public string ProgressBarStatusText
        {
            get
            {
                return progressBarStatusText;
            }
            set
            {
                progressBarStatusText = value;
                this.RaisePropertyChanged("ProgressBarStatusText");
            }
        }

        public bool ProgressVisible
        {
            get
            {
                return progressVisible;
            }
            set
            {
                progressVisible = value;
                this.RaisePropertyChanged("ProgressVisible");
            }
        }


        public void UpdateProgress()
        {
            Progress = Convert.ToDouble(countArtifactsMigrated);
            ProgressBarStatusText = string.Format("   Progress Status: {0}/{1} completed", countArtifactsMigrated, countTotalArtifactsToMigrate);
        }


        private async Task ExportArtifacts()
        {
            this.ProgressVisible = true;
            if (ExportSchemasFlag)
            {
                await this.ExportSchemas();
            }
            if (ExportCertificatesFlag)
            {
                await this.ExportCertificates();
            }
            if (ExportPartnersFlag)
            {
                await this.ExportPartners();
            }
            if (ExportAgreementsFlag)
            {
                await this.ExportAgreements();
            }
            if (ExportMapsFlag)
            {
                await this.ExportMaps();
            }
            this.ProgressVisible = false;
        }

        private async Task ExportSchemas()
        {
            if (importedSchemas != null)
            {
                try
                {
                    await sMigrator.MigrateToCloudIA(importedSchemas.ToList(), this.ApplicationContext.GetService<IntegrationAccountDetails>());

                }
                catch (Exception ex)
                {
                    StatusBarViewModel.ShowError("(Error) " + ExceptionHelper.GetExceptionMessage(ex));
                }
                finally
                {
                    foreach (var schema in importedSchemas)
                    {
                        ExportedItems.Where(item => item.ArtifactName == schema.MigrationEntity.fullNameOfSchemaToUpload).First().ExportStatus = schema.ExportStatus;
                        ExportedItems.Where(item => item.ArtifactName == schema.MigrationEntity.fullNameOfSchemaToUpload).First().ExportStatusText = schema.ExportStatusText;
                        countArtifactsMigrated++;
                        UpdateProgress();
                    }
                }

                foreach (var item in importedSchemas)
                {
                    if (item.ExportStatus == MigrationStatus.Failed)
                    {
                        StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                        StatusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                        break;
                    }
                }
            }
        }

        private async Task ExportMaps()
        {
            if (importedMaps != null)
            {
                try
                {
                    await mMigrator.MigrateToCloudIA(importedMaps.ToList(), this.ApplicationContext.GetService<IntegrationAccountDetails>());

                }
                catch (Exception ex)
                {
                    StatusBarViewModel.ShowError("(Error) " + ExceptionHelper.GetExceptionMessage(ex));
                }
                finally
                {
                    foreach (var map in importedMaps)
                    {
                        ExportedItems.Where(item => item.ArtifactName == map.MigrationEntity.fullNameOfMapToUpload).First().ExportStatus = map.ExportStatus;
                        ExportedItems.Where(item => item.ArtifactName == map.MigrationEntity.fullNameOfMapToUpload).First().ExportStatusText = map.ExportStatusText;
                        countArtifactsMigrated++;
                        UpdateProgress();
                    }
                }

                foreach (var item in importedMaps)
                {
                    if (item.ExportStatus == MigrationStatus.Failed)
                    {
                        StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                        StatusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                        break;
                    }
                }
            }
        }
        private async Task ExportCertificates()
        {
            try
            {
                if (importedPartnersWithCertificates != null)
                {
                    foreach (var partner in importedPartnersWithCertificates)
                    {
                        try
                        {
                            await c1Migrator.ExportToIA(partner, this.ApplicationContext.GetService<IntegrationAccountDetails>());
                        }
                        catch (Exception ex)
                        {
                            partner.CertificateExportStatus = MigrationStatus.Failed;
                            partner.CertificateExportStatusText = string.Format("An exception occured:{0}", ex.Message);
                            TraceProvider.WriteLine(string.Format("An exception occured:{0}", ex.Message));
                        }
                        finally
                        {
                            ExportedItems.Where(item => item.ArtifactName == partner.MigrationEntity.CertificateName).First().ExportStatus = partner.CertificateExportStatus;
                            ExportedItems.Where(item => item.ArtifactName == partner.MigrationEntity.CertificateName).First().ExportStatusText = partner.CertificateExportStatusText;
                            countArtifactsMigrated++;
                            UpdateProgress();
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                StatusBarViewModel.ShowError("Error was encountered. Reason: " + ex.Message);
                TraceProvider.WriteLine("Error was encountered. Reason: " + ex.InnerException.StackTrace);
            }
            try
            {
                if (otherExtractedCerts != null)
                {
                    foreach (var cert in otherExtractedCerts)
                    {
                        try
                        {
                            await c2Migrator.ExportToIA(cert, this.ApplicationContext.GetService<IntegrationAccountDetails>());
                        }
                        catch (Exception ex)
                        {
                            cert.ExportStatus = MigrationStatus.Failed;
                            cert.ExportStatusText = string.Format("An exception occured:{0}", ex.Message);
                            TraceProvider.WriteLine(string.Format("An exception occured:{0}", ex.Message));
                        }
                        finally
                        {
                            ExportedItems.Where(item => item.ArtifactName == cert.certificateName).First().ExportStatus = cert.ExportStatus;
                            ExportedItems.Where(item => item.ArtifactName == cert.certificateName).First().ExportStatusText = cert.ExportStatusText;
                            countArtifactsMigrated++;
                            UpdateProgress();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                StatusBarViewModel.ShowError("Error was encountered. Reason: " + ex.Message);
                TraceProvider.WriteLine("Error was encountered. Reason: " + ex.InnerException.StackTrace);
            }
            if (ExportedItems != null)
            {
                foreach (var item in ExportedItems)
                {
                    if (item.ExportStatus == MigrationStatus.Failed)
                    {
                        StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                        StatusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                        break;
                    }
                }
            }

        }

        private async Task ExportPartners()
        {
            try
            {
                if (importedPartners != null)
                {
                    foreach (var partner in importedPartners)
                    {
                        try
                        {
                            await pMigrator.ExportToIA(partner, this.ApplicationContext.GetService<IntegrationAccountDetails>());
                        }
                        catch (Exception ex)
                        {
                            partner.ExportStatus = MigrationStatus.Failed;
                            partner.ExportStatusText = ex.Message;
                            TraceProvider.WriteLine(string.Format("An exception occured:{0}", ex.Message));
                        }
                        finally
                        {
                            ExportedItems.Where(item => item.ArtifactName == partner.MigrationEntity.Name).First().ExportStatus = partner.ExportStatus;
                            ExportedItems.Where(item => item.ArtifactName == partner.MigrationEntity.Name).First().ExportStatusText = partner.ExportStatusText;
                            countArtifactsMigrated++;
                            UpdateProgress();
                        }
                    }

                    foreach (var item in importedPartners)
                    {
                        if (item.ExportStatus == MigrationStatus.Failed)
                        {
                            StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                            StatusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                StatusBarViewModel.ShowError("Error was encountered. Reason: " + ex.Message);
                TraceProvider.WriteLine("Error was encountered. Reason: " + ex.InnerException.StackTrace);

            }
        }

        private async Task ExportAgreements()
        {

            try
            {
                AuthenticationResult authresult = this.ApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                Dictionary<string, string> schemasInIA = await AgreementMigrator.GetAllSchemasInIA(this.ApplicationContext.GetService<IntegrationAccountDetails>(), authresult);
                Dictionary<KeyValuePair<string, string>, string> partnerIdentitiesInIA = await AgreementMigrator.GetAllPartnerIdentitiesInIA(this.ApplicationContext.GetService<IntegrationAccountDetails>(), authresult);
                this.ApplicationContext.SetProperty("SchemasInIntegrationAccount", schemasInIA);
                this.ApplicationContext.SetProperty("PartnerIdentitiesInIntegrationAccount", partnerIdentitiesInIA);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Following error occured : " + ExceptionHelper.GetExceptionMessage(ex));
                TraceProvider.WriteLine("Following error occured : " + ExceptionHelper.GetExceptionMessage(ex));
            }
            try
            {
                if (importedAgreements != null)
                {
                    foreach (var agreement in importedAgreements)
                    {
                        try
                        {
                            await aMigrator.ExportToIA(agreement, this.ApplicationContext.GetService<IntegrationAccountDetails>());

                        }
                        catch (Exception ex)
                        {
                            agreement.ExportStatus = MigrationStatus.Failed;
                            agreement.ExportStatusText = ex.Message;
                            TraceProvider.WriteLine(string.Format("An exception occured:{0}", ex.Message));
                            TraceProvider.WriteLine();
                        }
                        finally
                        {
                            ExportedItems.Where(item => item.ArtifactName == agreement.Name).First().ExportStatus = agreement.ExportStatus;
                            ExportedItems.Where(item => item.ArtifactName == agreement.Name).First().ExportStatusText = agreement.ExportStatusText;
                            countArtifactsMigrated++;
                            UpdateProgress();
                        }
                    }

                    foreach (var item in importedAgreements)
                    {
                        if (item.ExportStatus == MigrationStatus.Failed)
                        {
                            StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                            StatusBarViewModel.ShowError("Error(s) were encountered. Please refer the Status Tool Tip / Log File for more details.");
                            break;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                StatusBarViewModel.ShowError("Error was encountered. Reason: " + ex.Message);
                TraceProvider.WriteLine("Error was encountered. Reason: " + ex.InnerException.StackTrace);
            }
        }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            pMigrator = new PartnerMigrator(this.ApplicationContext);
            aMigrator = new AgreementMigrator(this.ApplicationContext);
            sMigrator = new SchemaMigrator(this.ApplicationContext);
            mMigrator = new MapMigrator(this.ApplicationContext);
            c1Migrator = new CerificateMigrator<PartnerMigrationItemViewModel>(this.ApplicationContext);
            c2Migrator = new CerificateMigrator<Certificate>(this.ApplicationContext);

            ExportedItems = new ObservableCollection<ExportedArtifact>();

            selectedSchemas = this.ApplicationContext.GetProperty(AppConstants.SelectedSchemasContextPropertyName) as IEnumerable<SchemaMigrationItemViewModel>;
            importedSchemas = selectedSchemas.Where(item => item.ImportStatus == MigrationStatus.Succeeded);

            selectedMaps = this.ApplicationContext.GetProperty(AppConstants.SelectedMapsContextPropertyName) as IEnumerable<MapMigrationItemViewModel>;
            importedMaps = selectedMaps.Where(item => item.ImportStatus == MigrationStatus.Succeeded);

            selectedPartners = this.ApplicationContext.GetProperty(AppConstants.SelectedPartnersContextPropertyName) as IEnumerable<PartnerMigrationItemViewModel>;
            if(applicationContext.GetProperty("MigrationCriteria").ToString() == "Partner")
                importedPartners = selectedPartners.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
            if (applicationContext.GetProperty("MigrationCriteria").ToString() == "Partner")
                importedPartnersWithCertificates = selectedPartners.Where(item => item.CertificationRequired == true && item.CertificateImportStatus == MigrationStatus.Succeeded);

            selectedAgreements = this.ApplicationContext.GetProperty(AppConstants.SelectedAgreementsContextPropertyName) as IEnumerable<AgreementMigrationItemViewModel>;
            if (applicationContext.GetProperty("MigrationCriteria").ToString() == "Partner")
                importedAgreements = selectedAgreements.Where(item => item.ImportStatus == MigrationStatus.Succeeded);

            otherCerts = this.ApplicationContext.GetProperty("OtherCertificates") as List<Certificate>;

            ExportSchemasFlag = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportSchemas"));
            ExportMapsFlag = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportMaps"));
            ExportCertificatesFlag = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportCertificates"));
            ExportPartnersFlag = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportPartners"));
            ExportAgreementsFlag = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportAgreements"));

            bool consolidateAgreements = Convert.ToBoolean(this.ApplicationContext.GetProperty(AppConstants.ConsolidationEnabled));
            bool generateMetadataContext = Convert.ToBoolean(this.ApplicationContext.GetProperty(AppConstants.ContextGenerationEnabled));

            if (ExportSchemasFlag || ExportCertificatesFlag || ExportPartnersFlag || ExportAgreementsFlag || ExportMapsFlag)
            {
                if (ExportSchemasFlag)
                {
                    foreach (var item in importedSchemas)
                    {
                        ExportedItems.Add(new ExportedArtifact
                        {
                            ArtifactName = item.MigrationEntity.fullNameOfSchemaToUpload,
                            ArtifactType = ArtifactTypes.Schema,
                            ExportStatus = MigrationStatus.NotStarted,
                            ExportStatusText = null
                        });

                    }
                }
                if (ExportMapsFlag)
                {
                    foreach (var item in importedMaps)
                    {
                        ExportedItems.Add(new ExportedArtifact
                        {
                            ArtifactName = item.MigrationEntity.fullNameOfMapToUpload,
                            ArtifactType = ArtifactTypes.Map,
                            ExportStatus = MigrationStatus.NotStarted,
                            ExportStatusText = null
                        });

                    }
                }
                if (ExportCertificatesFlag)
                {
                    foreach (var item in importedPartnersWithCertificates)
                    {
                        ExportedItems.Add(new ExportedArtifact
                        {
                            ArtifactName = item.MigrationEntity.CertificateName,
                            ArtifactType = ArtifactTypes.Certificate,
                            ExportStatus = MigrationStatus.NotStarted,
                            ExportStatusText = null
                        });

                    }
                    if (otherCerts != null)
                    {
                        otherExtractedCerts = otherCerts.Where(item => item.ImportStatus == MigrationStatus.Succeeded);
                        foreach (var item in otherExtractedCerts)
                        {
                            ExportedItems.Add(new ExportedArtifact
                            {
                                ArtifactName = item.certificateName,
                                ArtifactType = ArtifactTypes.Certificate,
                                ExportStatus = MigrationStatus.NotStarted,
                                ExportStatusText = null
                            });

                        }
                    }
                }
                if (ExportPartnersFlag)
                {
                    foreach (var item in importedPartners)
                    {
                        ExportedItems.Add(new ExportedArtifact
                        {
                            ArtifactName = item.MigrationEntity.Name,
                            ArtifactType = ArtifactTypes.Partner,
                            ExportStatus = MigrationStatus.NotStarted,
                            ExportStatusText = null
                        });

                    }
                }

                if (ExportAgreementsFlag)
                {
                    if (consolidateAgreements)
                    {
                        AuthenticationResult authresult = this.ApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
                        List<X12AgreementJson.Rootobject> x12AgreementsInIA = AgreementMigrator.GetAllX12AgreementsInIA(this.ApplicationContext.GetService<IntegrationAccountDetails>(), authresult).Result;
                        this.ApplicationContext.SetProperty("X12AgreementsInIntegrationAccount", x12AgreementsInIA);

                        List<EDIFACTAgreementJson.Rootobject> edifactAgreementsInIA = AgreementMigrator.GetAllEdifactAgreementsInIA(this.ApplicationContext.GetService<IntegrationAccountDetails>(), authresult).Result;
                        this.ApplicationContext.SetProperty("EdifactAgreementsInIntegrationAccount", edifactAgreementsInIA);
                    }
                    foreach (var item in importedAgreements)
                    {
                        if (consolidateAgreements)
                        {
                            TraceProvider.WriteLine(string.Format("Checking {0} for Agreement consolidation..", item.Name));
                            AgreementMigrator agmtMigrator = new AgreementMigrator(this.ApplicationContext);
                            try
                            {
                                agmtMigrator.CheckIfAgreementHasToBeConsolidated(item);
                            }
                            catch (Exception ex)
                            {
                                StatusBarViewModel.StatusInfoType = StatusInfoType.Error;
                                StatusBarViewModel.ShowError(string.Format("Error was encountered during the check for consolidating agreement {0} with IA agreements. Reason: {1}. The agreement will be migrated as such", item.Name, ExceptionHelper.GetExceptionMessage(ex)));
                                TraceProvider.WriteLine(string.Format("Error was encountered during the check for consolidating agreement {0} with IA agreements. Reason: {1}. The agreement will be migrated as such", item.Name, ExceptionHelper.GetExceptionMessage(ex)));
                                TraceProvider.WriteLine();
                            }
                        }
                        ExportedItems.Add(new ExportedArtifact
                        {
                            ArtifactName = item.Name,
                            ArtifactType = ArtifactTypes.Agreement,
                            ExportStatus = MigrationStatus.NotStarted,
                            ExportStatusText = null
                        });

                    }
                }
                countArtifactsMigrated = 0;
                countTotalArtifactsToMigrate = ExportedItems.Count();
                TotalArtifacts = countTotalArtifactsToMigrate;
                ProgressVisible = false;
                UpdateProgress();
                ExportArtifacts();
            }
            else
            {
                var lastNavigation = this.ApplicationContext.LastNavigation;
                if (lastNavigation == NavigateAction.Next)
                {
                    this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToNextStep();
                }
                else if (lastNavigation == NavigateAction.Previous)
                {
                    this.ApplicationContext.GetService<WizardNavigationViewModel>().MoveToPreviousStep();
                }
            }
        }
    }
}
