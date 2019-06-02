//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Windows.Documents;
    using System.Xml.Linq;
    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    class SummaryPageViewModel : BaseViewModel, IPageViewModel
    {
        private string summaryText;
        private Command openLogFileCommand;

        public string Title
        {
            get
            {
                return Resources.SummaryPageTitle;
            }
        }

        public string SummaryText
        {
            get
            {
                return this.summaryText;
            }

            set
            {
                this.summaryText = value;
                this.RaisePropertyChanged("SummaryText");
            }
        }

        // command to open the Trace file
        public Command OpenLogFileCommand
        {
            get
            {
                if (this.openLogFileCommand == null)
                {
                    this.openLogFileCommand = new Command(
                        Resources.LogFileLabel,
                        "OpenLogFileCommand",
                        (o) => true,
                        (o) => this.OpenLogFile());
                }

                return this.openLogFileCommand;
            }
        }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            this.InitializeSummaryText();
            this.OpenLogFileCommand.Refresh();
        }

        private static string GetErrorMessage(Exception exception)
        {
            if (exception is TpmMigrationException)
            {
                return exception.Message;
            }
            else
            {
                return Resources.UnknownErrorMessage;
            }
        }

        private void InitializeSummaryText()
        {
            StringBuilder summaryBuilder = new StringBuilder();

            var appException = this.ApplicationContext.ApplicationException;
            if (appException != null)
            {
                summaryBuilder.Append(Resources.ErrorSummaryMessage);
                summaryBuilder.Append(' ');
                summaryBuilder.Append(GetErrorMessage(appException));
                summaryBuilder.Append(Environment.NewLine);
            }

            var selectedPartners = this.ApplicationContext.GetProperty(AppConstants.SelectedPartnersContextPropertyName) as IEnumerable<PartnerMigrationItemViewModel>;
            var selectedAgreements = this.ApplicationContext.GetProperty(AppConstants.SelectedAgreementsContextPropertyName) as IEnumerable<AgreementMigrationItemViewModel>;
            var selectedSchemas = this.ApplicationContext.GetProperty(AppConstants.SelectedSchemasContextPropertyName) as IEnumerable<SchemaMigrationItemViewModel>;
            var otherCerts = this.ApplicationContext.GetProperty("OtherCertificates") as List<Certificate>;

            #region ImportStatus
            summaryBuilder.Append(Resources.ExportToJsonSummaryTitle);
            summaryBuilder.Append(Environment.NewLine);
            if (selectedPartners != null)
            {
                summaryBuilder.AppendLine("Certificate(s) :");
                var totalPartnerCertificates = selectedPartners.Count(item => item.CertificationRequired == true);
                var totalOtherCerts = otherCerts != null ? otherCerts.Count() : 0;
                var failedPartnerCertificateImports = selectedPartners.Count(item => item.CertificateImportStatus == MigrationStatus.Failed && item.CertificationRequired == true);
                var failedOtherCertificateImports = otherCerts != null ? otherCerts.Count(item => item.ImportStatus == MigrationStatus.Failed) : 0;
                int totalcerts = totalPartnerCertificates + totalOtherCerts;
                int totalFailedCertImports = failedPartnerCertificateImports + failedOtherCertificateImports;
                if (totalcerts > 0)
                {
                    
                    summaryBuilder.Append(string.Format(
                           CultureInfo.InvariantCulture,
                           Resources.CertificateImportSummaryFormat,
                           totalcerts - totalFailedCertImports,
                           totalcerts));
                    summaryBuilder.Append(Environment.NewLine);

                    if (failedPartnerCertificateImports > 0)
                    {
                        summaryBuilder.Append(string.Format(Resources.MigrationErrorSummaryMessageForCert, Resources.ExportToJsonText));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append(Resources.NoCertificatesRequiredSummary);
                    summaryBuilder.Append(Environment.NewLine);
                }
                summaryBuilder.AppendLine("Partner(s) :");
                if (selectedPartners.Count() != 0)
                {                   
                    var numSelectedPartners = selectedPartners.Count();
                    var numFailedPartnerImports = selectedPartners.Count(p => p.ImportStatus == MigrationStatus.Failed);
                    summaryBuilder.Append(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.PartnerImportSummaryFormat,
                        numSelectedPartners - numFailedPartnerImports,
                        numSelectedPartners));
                    summaryBuilder.Append(Environment.NewLine);
                    if (failedPartnerCertificateImports > 0)
                    {
                        summaryBuilder.Append(string.Format(Resources.MigrationErrorSummaryMessageForCert, Resources.ExportToJsonText));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Partners were selected");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }


            if (selectedAgreements != null)
            {
                summaryBuilder.AppendLine("Agreement(s):");
                if (selectedAgreements.Count() != 0)
                {
                    var numSelectedAgreements = selectedAgreements.Count();
                    var numFailedAgreementImports = selectedAgreements.Count(agr => agr.ImportStatus == MigrationStatus.Failed);
                    summaryBuilder.Append(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.AgreementImportSummaryFormat,
                        numSelectedAgreements - numFailedAgreementImports,
                        numSelectedAgreements));
                    summaryBuilder.Append(Environment.NewLine);
                    if (numFailedAgreementImports > 0)
                    {
                        summaryBuilder.Append(string.Format(Resources.MigrationErrorSummaryMessageForAgreement, Resources.ExportToJsonText));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Agreements were selected");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }

            if(selectedSchemas!= null)
            {
                summaryBuilder.AppendLine("Schema(s):");
                if (selectedSchemas.Count() != 0)
                {
                    var numSelectedSchemas= selectedSchemas.Count();
                    var numFailedSchemasImports = selectedSchemas.Count(agr => agr.ImportStatus == MigrationStatus.Failed);
                    summaryBuilder.Append(string.Format(
                        CultureInfo.InvariantCulture,
                        Resources.SchemaImportSummaryFormat,
                        numSelectedSchemas - numFailedSchemasImports,
                        numSelectedSchemas));
                    summaryBuilder.Append(Environment.NewLine);
                    if (numFailedSchemasImports > 0)
                    {
                        summaryBuilder.Append(string.Format(Resources.MigrationErrorSummaryMessageForSchema, "preparing definitions"));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Schemas were selected");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }
            #endregion

            summaryBuilder.Append(Environment.NewLine);
            summaryBuilder.Append(Environment.NewLine);

            #region ExportStatus
            summaryBuilder.Append(Resources.MigrateToIASumaryTitle);
            summaryBuilder.Append(Environment.NewLine);

            bool exportedSchemas = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportSchemas"));
            bool exportedCerts = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportCertificates"));
            bool exportedPartners = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportPartners"));
            bool exportedAgreements = Convert.ToBoolean(this.ApplicationContext.GetProperty("ExportAgreements"));
            bool overwrite = Convert.ToBoolean(this.ApplicationContext.GetProperty("OverwriteEnabled"));
             
            if(selectedSchemas != null)
            {
                summaryBuilder.AppendLine("Schema(s):");
                if (selectedSchemas.Count() != 0)
                {
                    var numSelectedSchemas = selectedSchemas.Count();
                    var numFailedSchemaExports = selectedSchemas.Count(p => p.ExportStatus != MigrationStatus.Succeeded && p.ExportStatus != MigrationStatus.Partial);
                    var numFailedSchemasImports = selectedSchemas.Count(agr => agr.ImportStatus == MigrationStatus.Failed);
                    if (exportedSchemas)
                    {
                        if (numFailedSchemasImports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullImport, numFailedSchemasImports, numSelectedSchemas, "Schema(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        int schemasAlreadyInIA = selectedSchemas.Count(item => item.ExportStatusText != null && item.ExportStatusText.Contains("already exists in IA"));
                        if (!overwrite && schemasAlreadyInIA != 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.NoOverWriteArtfact, schemasAlreadyInIA, numSelectedSchemas, "Schema(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        if (numFailedSchemasImports == 0 && numFailedSchemaExports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullExport, numFailedSchemaExports, numSelectedSchemas, "Schema(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        summaryBuilder.Append(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.SchemaExportSummaryFormat,
                            numSelectedSchemas - (numFailedSchemaExports + schemasAlreadyInIA),
                            numSelectedSchemas));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                    else
                    {
                        summaryBuilder.Append("Schema(s) migration was not selected");
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Schemas to migrate");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }
            if (selectedPartners != null)
            {
                summaryBuilder.AppendLine("Certificate(s):");
                var totalPartnerCertificates = selectedPartners.Count(item => item.CertificationRequired == true);
                var totalOtherCerts = otherCerts != null ? otherCerts.Count() : 0;
                var failedPartnerCertificateExports = selectedPartners.Count(item => item.CertificateExportStatus != MigrationStatus.Succeeded && item.CertificateExportStatus != MigrationStatus.Partial && item.CertificationRequired == true);
                var failedOtherCertificateExports = otherCerts != null ? otherCerts.Count(item => item.ExportStatus != MigrationStatus.Succeeded && item.ExportStatus != MigrationStatus.Partial) : 0;
                var failedOtherCertificateImports = otherCerts != null ? otherCerts.Count(item => item.ImportStatus == MigrationStatus.Failed) : 0;
                var failedPartnerCertificateImports = selectedPartners.Count(item => item.CertificateImportStatus == MigrationStatus.Failed && item.CertificationRequired == true);
                int totalCerts = totalPartnerCertificates + totalOtherCerts;
                int totalFailedCertImports = failedOtherCertificateImports + failedPartnerCertificateImports;
                int totalFailedCertExports = failedOtherCertificateExports + failedPartnerCertificateExports;
                if (totalCerts > 0)
                {
                    if (totalFailedCertImports != totalCerts)
                    {
                        if (exportedCerts)
                        {
                            if (totalFailedCertImports > 0)
                            {
                                summaryBuilder.Append(string.Format(Resources.UnSuccesfullImport, totalFailedCertImports, totalCerts, "Certificate(s)"));
                                summaryBuilder.Append(Environment.NewLine);
                            }
                            int partnercertsAlreadyInIA = selectedPartners.Count(item => item.CertificateExportStatusText != null && item.CertificateExportStatusText.Contains("already exists on IA"));
                            int otherCertsAlreadyInIA = otherCerts != null ? otherCerts.Count(item => item.ExportStatusText != null && item.ExportStatusText.Contains("already exists on IA")) :0 ;
                            int certsAlreadyInIA = partnercertsAlreadyInIA + otherCertsAlreadyInIA;
                            if (!overwrite && certsAlreadyInIA != 0)
                            {
                                summaryBuilder.Append(string.Format(Resources.NoOverWriteArtfact, certsAlreadyInIA, totalCerts, "Certificate(s)"));
                                summaryBuilder.Append(Environment.NewLine);
                            }
                            if (failedPartnerCertificateImports == 0 && failedPartnerCertificateExports > 0)
                            {
                                summaryBuilder.Append(string.Format(Resources.UnSuccesfullExport, totalFailedCertExports, totalCerts, "Certificate(s)"));
                                summaryBuilder.Append(Environment.NewLine);
                            }
                            summaryBuilder.Append(string.Format(
                                   CultureInfo.InvariantCulture,
                                   Resources.CertificateExportSummaryFormat,
                                   totalCerts - (totalFailedCertExports + certsAlreadyInIA),
                                   totalCerts));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        else
                        {
                            summaryBuilder.Append("Certificate(s) migration was not selected");
                            summaryBuilder.Append(Environment.NewLine);
                        }
                    }
                    else
                    {
                        summaryBuilder.Append("No Certifiacte(s) to migrate");
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append(Resources.NoCertificatesRequiredSummary);
                    summaryBuilder.Append(Environment.NewLine);
                }

                summaryBuilder.AppendLine("Partner(s):");
                if (selectedPartners.Count() != 0)
                {
                    var numSelectedPartners = selectedPartners.Count();
                    var numFailedPartnerExports = selectedPartners.Count(p => p.ExportStatus != MigrationStatus.Succeeded && p.ExportStatus != MigrationStatus.Partial);
                    var numFailedPartnerImports = selectedPartners.Count(p => p.ImportStatus == MigrationStatus.Failed);
                    if (exportedPartners)
                    {
                        if (numFailedPartnerImports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullImport, numFailedPartnerImports, numSelectedPartners, "Partner(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        int partnersAlreadyInIA = selectedPartners.Count(item => item.ExportStatusText != null && item.ExportStatusText.Contains("already exists on IA"));
                        if (!overwrite && partnersAlreadyInIA != 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.NoOverWriteArtfact, partnersAlreadyInIA, numSelectedPartners, "Partner(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        if (numFailedPartnerImports == 0 && numFailedPartnerExports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullExport, numFailedPartnerExports, numSelectedPartners, "Partner(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        summaryBuilder.Append(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.PartnerExportSummaryFormat,
                            numSelectedPartners - (numFailedPartnerExports + partnersAlreadyInIA),
                            numSelectedPartners));
                        summaryBuilder.Append(Environment.NewLine);
                    }
                    else
                    {
                        summaryBuilder.Append("Partner(s) migration was not selected");
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Partner(s) to Migrate");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }


            if (selectedAgreements != null)
            {
                summaryBuilder.AppendLine("Agreement(s):");
                if (selectedAgreements.Count() != 0)
                {
                    var numSelectedAgreements = selectedAgreements.Count();
                    var numFailedAgreementExports = selectedAgreements.Count(agr => agr.ExportStatus != MigrationStatus.Succeeded && agr.ExportStatus != MigrationStatus.Partial);
                    var numFailedAgreementImports = selectedAgreements.Count(agr => agr.ImportStatus == MigrationStatus.Failed);
                    if (exportedAgreements)
                    {
                        if (numFailedAgreementImports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullImport, numFailedAgreementImports, numSelectedAgreements, "Agreement(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        int agreementsAlreadyInIA = selectedAgreements.Count(item => item.ExportStatusText != null && item.ExportStatusText.Contains("already exists on IA"));
                        if (!overwrite && agreementsAlreadyInIA != 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.NoOverWriteArtfact, agreementsAlreadyInIA, numSelectedAgreements, "Agreement(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        if (numFailedAgreementImports == 0 && numFailedAgreementExports > 0)
                        {
                            summaryBuilder.Append(string.Format(Resources.UnSuccesfullExport, numFailedAgreementExports, numSelectedAgreements, "Agreement(s)"));
                            summaryBuilder.Append(Environment.NewLine);
                        }
                        summaryBuilder.Append(string.Format(
                            CultureInfo.InvariantCulture,
                            Resources.AgreementExportSummaryFormat,
                            numSelectedAgreements - (numFailedAgreementExports + agreementsAlreadyInIA),
                            numSelectedAgreements));
                    }
                    else
                    {
                        summaryBuilder.Append("Agreement(s) migration was not selected");
                        summaryBuilder.Append(Environment.NewLine);
                    }
                }
                else
                {
                    summaryBuilder.Append("No Agreement(s) to Migrate");
                    summaryBuilder.Append(Environment.NewLine);
                }
            }
            #endregion

            if (appException == null && selectedPartners == null && selectedAgreements == null && selectedSchemas == null)
            {
                summaryBuilder.Append(Resources.NoAgreementsOrPartnersSelectedErrorMessage);
            }

            this.SummaryText = summaryBuilder.ToString();
            TraceProvider.WriteLine(this.SummaryText,true);
         }

        private void OpenLogFile()
        {
            var logFilePath = this.ApplicationContext.GetProperty("LogFilePath").ToString();

           
            if (File.Exists(logFilePath))
            {
                Process.Start(logFilePath);
            }            
        }
    }
}
