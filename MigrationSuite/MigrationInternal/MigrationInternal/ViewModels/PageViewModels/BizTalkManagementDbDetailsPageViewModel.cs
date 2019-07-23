//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Net;

    class BizTalkManagementDbDetailsPageViewModel : BaseViewModel, IValidatorPageViewModel
    {
        public BizTalkManagementDBDetails BizTalkManagementDbDetails { get; set; }
        public BiztalkServerDetails BizTalkServerDetails { get; set; }

        public string Title
        {
            get
            {
                return Resources.BizTalkManagementDbDetailsPageTitle;
            }
        }

        public string ServerName
        {
            get
            {
                return this.BizTalkManagementDbDetails.ServerName;
            }

            set
            {
                if (this.BizTalkManagementDbDetails.ServerName != value)
                {
                    this.BizTalkManagementDbDetails.ServerName = value;
                    this.RaisePropertyChanged("ServerName");
                }
            }
        }

        public string DatabaseName
        {
            get
            {
                return this.BizTalkManagementDbDetails.DatabaseName;
            }

            set
            {
                if (this.BizTalkManagementDbDetails.DatabaseName != value)
                {
                    this.BizTalkManagementDbDetails.DatabaseName = value;
                    this.RaisePropertyChanged("DatabaseName");
                }
            }
        }

        public string UserName
        {
            get
            {
                return this.BizTalkManagementDbDetails.UserName;
            }

            set
            {
                if (this.BizTalkManagementDbDetails.UserName != value)
                {
                    this.BizTalkManagementDbDetails.UserName = value;
                    this.RaisePropertyChanged("UserName");
                }
            }
        }

        public string Password
        {
            get
            {
                return this.BizTalkManagementDbDetails.Password;
            }

            set
            {
                if (this.BizTalkManagementDbDetails.Password != value)
                {
                    this.BizTalkManagementDbDetails.Password = value;
                    this.RaisePropertyChanged("Password");
                }
            }
        }


        public bool IsIntegratedSecurity
        {
            get
            {
                return this.BizTalkManagementDbDetails.IsIntegratedSecurity;
            }

            set
            {
                if (this.BizTalkManagementDbDetails.IsIntegratedSecurity != value)
                {
                    this.BizTalkManagementDbDetails.IsIntegratedSecurity = value;
                    this.RaisePropertyChanged("IsIntegratedSecurity");
                }
            }
        }

        //public string RemoteDomainName
        //{
        //    get
        //    {
        //        return this.BizTalkServerDetails.RemoteDomainName;
        //    }
        //    set
        //    {
        //        if (this.BizTalkServerDetails.RemoteDomainName != value)
        //        {
        //            this.BizTalkServerDetails.RemoteDomainName = value;
        //            this.RaisePropertyChanged("RemoteDomainName");
        //        }
        //    }
        //}

        //public string RemoteUserName
        //{
        //    get
        //    {
        //        return this.BizTalkServerDetails.RemoteUserName;
        //    }
        //    set
        //    {
        //        if (this.BizTalkServerDetails.RemoteUserName != value)
        //        {
        //            this.BizTalkServerDetails.RemoteUserName = value;
        //            this.RaisePropertyChanged("RemoteUserName");
        //        }
        //    }
        //}

        //public string RemoteUserPassword
        //{
        //    get
        //    {
        //        return this.BizTalkServerDetails.RemoteUserPassword;
        //    }
        //    set
        //    {
        //        if (this.BizTalkServerDetails.RemoteUserPassword != value)
        //        {
        //            this.BizTalkServerDetails.RemoteUserPassword = value;
        //            this.RaisePropertyChanged("RemoteUserPassword");
        //        }
        //    }
        //}

        //public string RemoteServerName
        //{
        //    get
        //    {
        //        return this.BizTalkServerDetails.RemoteServerName;
        //    }
        //    set
        //    {
        //        if (this.BizTalkServerDetails.RemoteServerName != value)
        //        {
        //            this.BizTalkServerDetails.RemoteServerName = value;
        //            this.RaisePropertyChanged("RemoteServerName");
        //        }
        //    }
        //}

        //public bool UseDifferentAccount
        //{
        //    get
        //    {
        //        return this.BizTalkServerDetails.UseDifferentAccount;
        //    }
        //    set
        //    {
        //        if (this.BizTalkServerDetails.UseDifferentAccount != value)
        //        {
        //            this.BizTalkServerDetails.UseDifferentAccount = value;
        //            this.RaisePropertyChanged("UseDifferentAccount");
        //        }
        //    }
        //}
        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);

            var cachedBizTalkManagementDbDetails = this.ApplicationContext.GetService<BizTalkManagementDBDetails>();
            //var cachedBizTalkServerDetails = this.ApplicationContext.GetService<BiztalkServerDetails>();
            if (cachedBizTalkManagementDbDetails != null)
            {
                this.BizTalkManagementDbDetails = cachedBizTalkManagementDbDetails;
            }
            else
            {
                this.BizTalkManagementDbDetails = new BizTalkManagementDBDetails()
                {
                    IsIntegratedSecurity = true
                };
                this.ApplicationContext.AddService(this.BizTalkManagementDbDetails);
            }
            //if(cachedBizTalkServerDetails != null)
            //{
            //    this.BizTalkServerDetails = cachedBizTalkServerDetails;
            //}
            //else
            //{
            //    this.BizTalkServerDetails = new BiztalkServerDetails()
            //    {
            //        UseDifferentAccount = false
            //    };
            //    this.ApplicationContext.AddService(this.BizTalkServerDetails);
            //}
        }

        public bool Validate()
        {
            var statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();
            Debug.Assert(statusBarViewModel != null, "StatusBarViewModel has not been initialized in application context");
            if (string.IsNullOrWhiteSpace(this.ServerName))
            {
                statusBarViewModel.ShowError(Resources.ServerNameValidationError);
                return false;
            }
            else if (string.IsNullOrWhiteSpace(this.DatabaseName))
            {
                statusBarViewModel.ShowError(Resources.DatabaseNameValidationError);
                return false;
            }
            else if (!this.IsIntegratedSecurity)
            {
                if (string.IsNullOrWhiteSpace(this.UserName))
                {
                    statusBarViewModel.ShowError(Resources.UserIdValidationError);
                    return false;
                }
                else if (string.IsNullOrWhiteSpace(this.Password))
                {
                    statusBarViewModel.ShowError(Resources.PasswordValidationError);
                    return false;
                }
            }
            //else if (string.IsNullOrWhiteSpace(this.RemoteServerName))
            //{
            //    statusBarViewModel.ShowError(string.Format(Resources.RemoteValidationError, "Biztalk Server Name"));
            //    return false;
            //}
            //else if (UseDifferentAccount)
            //{ 
            //    if (string.IsNullOrWhiteSpace(this.RemoteDomainName))
            //    {
            //        statusBarViewModel.ShowError(string.Format(Resources.RemoteValidationError, "Domain"));
            //        return false;
            //    }
            //    else if (string.IsNullOrWhiteSpace(this.RemoteUserName))
            //    {
            //        statusBarViewModel.ShowError(string.Format(Resources.RemoteValidationError, "Biztalk Server"));
            //        return false;
            //    }
            //    else if (string.IsNullOrWhiteSpace(this.RemoteUserPassword))
            //    {
            //        statusBarViewModel.ShowError(string.Format(Resources.RemoteValidationError, "Biztalk Server"));
            //        return false;
            //    }
            //}
            var bizTalkManagementDbDetails = ApplicationContext.GetService<BizTalkManagementDBDetails>();
            SqlConnectionStringBuilder builder;
            if (this.IsIntegratedSecurity)
            {
                builder = new SqlConnectionStringBuilder
                {
                    InitialCatalog = bizTalkManagementDbDetails.DatabaseName,
                    //MultipleActiveResultSets = true,
                    DataSource = bizTalkManagementDbDetails.ServerName,
                    IntegratedSecurity = bizTalkManagementDbDetails.IsIntegratedSecurity,
                };
            }
            else
            {
                builder = new SqlConnectionStringBuilder
                {
                    InitialCatalog = bizTalkManagementDbDetails.DatabaseName,
                    //MultipleActiveResultSets = true,
                    DataSource = bizTalkManagementDbDetails.ServerName,
                    UserID = bizTalkManagementDbDetails.UserName,
                    Password = bizTalkManagementDbDetails.Password
                };
            }

            using (SqlConnection conn = new SqlConnection(builder.ConnectionString))
            {
                try
                {
                    conn.Open();
                    this.ApplicationContext.SetProperty("DatabaseConnectionString", builder.ConnectionString);
                }
                catch (SqlException ex)
                {
                    TraceProvider.WriteLine(ExceptionHelper.GetExceptionMessage(ex));
                    statusBarViewModel.ShowError(ExceptionHelper.GetExceptionMessage(ex));
                    return false;
                }
            }
            if (!string.IsNullOrWhiteSpace(this.DatabaseName))
            {
                Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.DatabaseName = this.DatabaseName;
               
            }
            if (!string.IsNullOrWhiteSpace(this.ServerName))
            {
                Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.ServerName = this.ServerName;
            }
            if (!string.IsNullOrWhiteSpace(this.DatabaseName))
            {
                Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.DatabaseName = this.DatabaseName;

            }
            //if (!string.IsNullOrWhiteSpace(this.RemoteServerName))
            //{
            //    Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.RemoteServerName = this.RemoteServerName;
            //}
            //if (!string.IsNullOrWhiteSpace(this.RemoteDomainName))
            //{
            //    Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.RemoteDomainName = this.RemoteDomainName;

            //}
            //if (!string.IsNullOrWhiteSpace(this.RemoteUserName))
            //{
            //    Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.RemoteUserName = this.RemoteUserName;
            //}
            Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration.Properties.Settings.Default.Save();
            return true;
        }
    }
}