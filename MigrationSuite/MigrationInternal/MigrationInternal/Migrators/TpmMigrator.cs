//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Common;
    using System;
    using System.Net.Http;
    using System.Threading.Tasks;
    using SchemaMigration;
    using MapMigration;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;
    using Services = Microsoft.ApplicationServer.Integration.PartnerManagement;
    using IdentityModel.Clients.ActiveDirectory;

    abstract class TpmMigrator<TMigrationItem, TEntity>
        where TEntity : class
        where TMigrationItem : MigrationItemViewModel<TEntity>
    {
        private IApplicationContext applicationContext;
        private Services.TpmContext cloudContext;

        protected TpmMigrator(IApplicationContext applicationContext)
        {
            this.applicationContext = applicationContext;
        }

        protected Services.TpmContext CloudContext
        {
            get
            {
                if (this.cloudContext == null)
                {
                    this.cloudContext = TpmContextFactory.CreateTpmContext<Services.TpmContext>(this.applicationContext);
                }

                return this.cloudContext;
            }
        }

        public static TpmMigrator<TMigrationItem, TEntity> CreateMigrator(IApplicationContext applicationContext)
        {
            if (typeof(TEntity) == typeof(Server.Partner))
            {
                return new PartnerMigrator(applicationContext) as TpmMigrator<TMigrationItem, TEntity>;
            }
            else if (typeof(TEntity) == typeof(Server.Agreement))
            {
                return new AgreementMigrator(applicationContext) as TpmMigrator<TMigrationItem, TEntity>;
            }
            else if (typeof(TEntity) == typeof(SchemaDetails))
            {
                return new SchemaMigrator(applicationContext) as TpmMigrator<TMigrationItem, TEntity>;
            }
            else if (typeof(TEntity) == typeof(MapDetails))
            {
                return new MapMigrator(applicationContext) as TpmMigrator<TMigrationItem, TEntity>;
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        public abstract Task ImportAsync(TMigrationItem migrationItem);

        // The cloud context is refereshed each time an exception occurs if the entity is not able to migrate, as it tries to save the previous context too in the database.
        public void RefreshContext()
        {
            this.cloudContext = null;
        }

        public abstract Task ExportToIA(TMigrationItem migrationItem, IntegrationAccountDetails iaDetails);

        public async Task<bool> CheckIfArtifactExists(string migrationItem, string migrationEntity, IntegrationAccountDetails iaDetails, AuthenticationResult authResult)
        {
            IntegrationAccountContext sclient = new IntegrationAccountContext();
            HttpResponseMessage response = new HttpResponseMessage();
            try
            {
                if (migrationEntity == "Agreement")
                {
                    response = sclient.GetArtifactsFromIA(UrlHelper.GetAgreementUrl(migrationItem, iaDetails), authResult);
                }
                if (migrationEntity == "Partner")
                {
                    response = sclient.GetArtifactsFromIA(UrlHelper.GetPartnerUrl(migrationItem, iaDetails), authResult);
                }
                if (migrationEntity == "Certificate")
                {
                    response = sclient.GetArtifactsFromIA(UrlHelper.GetCertificateUrl(migrationItem, iaDetails), authResult);
                }
                if (migrationEntity == "Schema")
                {
                    response = sclient.GetArtifactsFromIA(UrlHelper.GetSchemaUrl(migrationItem, iaDetails), authResult);
                }
                if (migrationEntity == "Map")
                {
                    response = sclient.GetArtifactsFromIA(UrlHelper.GetMapUrl(migrationItem, iaDetails), authResult);
                }
                if (!response.IsSuccessStatusCode)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception)
            {
                return false;
            }
        }

        public Metadata GenerateMetadata(string originalname)
        {
            Metadata metadata = new Metadata()
            {
                migrationInfo = new MigrationAuditInformation()
                {
                    originalName = originalname
                }
            };
            return metadata;
        }

    }
}
