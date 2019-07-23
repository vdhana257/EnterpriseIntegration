//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using Common;
    using IdentityModel.Clients.ActiveDirectory;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Diagnostics;
    using System.Linq;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Windows.Input;
    using System.Windows.Media;

    class IntegrationServiceDetailsPageViewModel : BaseViewModel, IValidatorPageViewModel
    {
        public StatusBarViewModel statusBarViewModel;
        private IntegrationServiceDetails integrationServiceDetails;
        public ICommand AzureLoginButtonClickCommand { get; set; }

        private bool subscriptionVisible;
        private bool resourceGroupVisible;
        private bool integrationAccountVisible;
        private bool keyVaultVisible;

        private bool isLoginButttonEnabled;

        private ObservableCollection<Subscription.RootObject> userSubscriptions;
        private ObservableCollection<ResourceGroup.RootObject> userResourceGroups;
        private ObservableCollection<IntegrationAccount.RootObject> userIntegrationAccounts;
        private ObservableCollection<KeyVault.RootObject> userKeyVaults;

        private Subscription.RootObject selectedSubscription;
        private ResourceGroup.RootObject selectedResourceGroup;
        private IntegrationAccount.RootObject selectedIntegrationAccount;
        private KeyVault.RootObject selectedKeyVault;

        public string Title
        {
            get
            {
                return Resources.IntegrationServiceDetailsPageTitle;
            }
        }

        public Subscription.RootObject SelectedSubscription
        {
            get
            {
                return selectedSubscription;
            }
            set
            {
                selectedSubscription = value;
                SubscriptionSelectionChanged();
                ResourceGroupVisible = true;
            }
        }

        public ResourceGroup.RootObject SelectedResourceGroup
        {
            get
            {
                return selectedResourceGroup;
            }
            set
            {
                selectedResourceGroup = value;
                ResourceGroupSelectionChanged();
                IntegrationAccountVisible = true;
                KeyVaultVisible = true;
            }
        }

        public IntegrationAccount.RootObject SelectedIntegrationAccount
        {
            get
            {
                return selectedIntegrationAccount;
            }
            set
            {
                selectedIntegrationAccount = value;
            }
        }

        public KeyVault.RootObject SelectedKeyVault
        {
            get
            {
                return selectedKeyVault;
            }
            set
            {
                selectedKeyVault = value;
            }
        }

        public ObservableCollection<Subscription.RootObject> UserSubscriptions
        {
            get
            {
                return userSubscriptions;
            }
            set
            {
                userSubscriptions = value;
                this.RaisePropertyChanged("UserSubscriptions");
            }
        }

        public ObservableCollection<ResourceGroup.RootObject> UserResourceGroups
        {
            get
            {
                return userResourceGroups;
            }
            set
            {
                userResourceGroups = value;
                this.RaisePropertyChanged("UserResourceGroups");
            }
        }

        public ObservableCollection<IntegrationAccount.RootObject> UserIntegrationAccounts
        {
            get
            {
                return userIntegrationAccounts;
            }
            set
            {
                userIntegrationAccounts = value;
                this.RaisePropertyChanged("UserIntegrationAccounts");
            }
        }

        public ObservableCollection<KeyVault.RootObject> UserKeyVaults
        {
            get
            {
                return userKeyVaults;
            }
            set
            {
                userKeyVaults = value;
                this.RaisePropertyChanged("UserKeyVaults");
            }
        }
        public bool IsLoginButttonEnabled
        {
            get
            {
                return isLoginButttonEnabled;
            }
            set
            {
                isLoginButttonEnabled = value;
                this.RaisePropertyChanged("IsLoginButttonEnabled");
            }
        }

        public bool SubscriptionVisible
        {
            get
            {
                return subscriptionVisible;
            }
            set
            {
                subscriptionVisible = value;
                this.RaisePropertyChanged("SubscriptionVisible");
            }
        }

        public bool ResourceGroupVisible
        {
            get
            {
                return resourceGroupVisible;
            }
            set
            {
                resourceGroupVisible = value;
                this.RaisePropertyChanged("ResourceGroupVisible");
            }
        }

        public bool IntegrationAccountVisible
        {
            get
            {
                return integrationAccountVisible;
            }
            set
            {
                integrationAccountVisible = value;
                this.RaisePropertyChanged("IntegrationAccountVisible");
            }
        }

        public bool KeyVaultVisible
        {
            get
            {
                return keyVaultVisible;
            }
            set
            {
                keyVaultVisible = value;
                this.RaisePropertyChanged("KeyVaultVisible");
            }
        }
        private void AzureLoginButtonClick(object sender)
        {
            try
            {
                IntegrationAccountContext iaContext = new IntegrationAccountContext();
                var authresult = iaContext.GetAccessTokenFromAAD();
                AuthenticationResult integrationAccountResult = authresult[AuthenticationAccessToken.IntegrationAccount];
                AuthenticationResult keyVaultResult = authresult[AuthenticationAccessToken.KeyVault];
                this.ApplicationContext.SetProperty("IntegrationAccountAuthorization", integrationAccountResult);
                this.ApplicationContext.SetProperty("KeyVaultAuthorization", keyVaultResult);
                string iaToken = integrationAccountResult?.AccessToken;
                string kvToken = keyVaultResult?.AccessToken;
                statusBarViewModel.Clear();
                try
                {
                    HttpResponseMessage response =iaContext.SendSyncGetRequestToIA(UrlHelper.GetSubscriptionsUrl(), integrationAccountResult);
                    var reponseObj = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                    if (reponseObj.GetValue("value")!= null)
                    {
                        UserSubscriptions = JsonConvert.DeserializeObject<ObservableCollection<Subscription.RootObject>>(reponseObj["value"].ToString());
                        if (userSubscriptions.Count != 0)
                        {
                            SubscriptionVisible = true;
                            IsLoginButttonEnabled = false;
                        }
                        else
                        {
                            TraceProvider.WriteLine("No Subscriptions are available for the user. Please login as different user");
                            statusBarViewModel.ShowError("No Subscriptions are available for the user. Please login as different user");
                            IsLoginButttonEnabled = true;
                            SubscriptionVisible = false;
                        }
                    }
                    else
                    {
                        TraceProvider.WriteLine("No Subscriptions are available for the user. Please login as different user");
                        statusBarViewModel.ShowError("No Subscriptions are available for the user. Please login as different user");
                        IsLoginButttonEnabled = true;
                        SubscriptionVisible = false;
                    }
                   
                }
                catch (Exception ex)
                {
                        TraceProvider.WriteLine(string.Format("Error reading user subscriptions from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                        statusBarViewModel.ShowError(string.Format("Error reading user subscriptions from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                        IsLoginButttonEnabled = true;
                        SubscriptionVisible = false;
                }
            }
            catch(Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error getting the Access Tokens. Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                statusBarViewModel.ShowError(string.Format("Error getting the Access Tokens. Reason: {0}", ExceptionHelper.GetExceptionMessage(ex)));
                IsLoginButttonEnabled = true;
                SubscriptionVisible = false;
            }
        }

        private void SubscriptionSelectionChanged()
        {

            IntegrationAccountContext iaContext = new IntegrationAccountContext();
            var authResult = this.ApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
            statusBarViewModel.Clear();
            try
            {
                HttpResponseMessage response = iaContext.SendSyncGetRequestToIA(UrlHelper.GetResourceGroupsUrl(SelectedSubscription.SubscriptionId), authResult);
                var reponseObj = JObject.Parse(response.Content.ReadAsStringAsync().Result);
                if (reponseObj.GetValue("value") != null)
                {

                    UserResourceGroups = JsonConvert.DeserializeObject<ObservableCollection<ResourceGroup.RootObject>>(reponseObj["value"].ToString());
                    if (userResourceGroups.Count != 0)
                    {
                        IsLoginButttonEnabled = false;
                    }
                    else
                    {
                        TraceProvider.WriteLine("No Resource Groups are available for the user. Please login as different user or select different Subscription");
                        statusBarViewModel.ShowError("No Resource Groups are available for the user. Please login as different user or select different Subscription");
                        IsLoginButttonEnabled = true;
                    }
                }
                else
                {
                    TraceProvider.WriteLine("No Resource Groups are available for the user. Please login as different user or select different Subscription");
                    statusBarViewModel.ShowError("No Resource Groups are available for the user. Please login as different user or select different Subscription");
                    IsLoginButttonEnabled = true;
                }
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error reading user RG's from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                statusBarViewModel.ShowError(string.Format("Error reading user RG's from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                IsLoginButttonEnabled = true;
            }
        }

        private void ResourceGroupSelectionChanged()
        {
            IntegrationAccountContext iaContext = new IntegrationAccountContext();
            var authResult = this.ApplicationContext.GetProperty("IntegrationAccountAuthorization") as AuthenticationResult;
            statusBarViewModel.Clear();
            try
            {
                HttpResponseMessage IAresponse = iaContext.SendSyncGetRequestToIA(UrlHelper.GetIntegrationAccountsUrl(SelectedSubscription.SubscriptionId, SelectedResourceGroup.Name), authResult);
                var IAreponseObj = JObject.Parse(IAresponse.Content.ReadAsStringAsync().Result);
                HttpResponseMessage KVresponse = iaContext.SendSyncGetRequestToIA(UrlHelper.GetKeyVaultsUrl(SelectedSubscription.SubscriptionId, SelectedResourceGroup.Name), authResult);
                var KVreponseObj = JObject.Parse(KVresponse.Content.ReadAsStringAsync().Result);
                if (IAreponseObj.GetValue("value") != null && KVreponseObj.GetValue("value") != null)
                {

                    UserIntegrationAccounts = JsonConvert.DeserializeObject<ObservableCollection<IntegrationAccount.RootObject>>(IAreponseObj["value"].ToString());
                    UserKeyVaults = JsonConvert.DeserializeObject<ObservableCollection<KeyVault.RootObject>>(KVreponseObj["value"].ToString());
                    if (UserIntegrationAccounts.Count != 0)
                    {
                        IsLoginButttonEnabled = false;
                    }
                    else
                    {
                        TraceProvider.WriteLine("No Integration Accounts are available for the user. Please login as different user or select a different ResourceGroup/Subscription");
                        statusBarViewModel.ShowError("No Integration Accounts are available for the user. Please login as different user or select a different ResourceGroup/Subscription");
                        IsLoginButttonEnabled = true;
                    }
                    if (UserKeyVaults.Count == 0)
                    {
                        TraceProvider.WriteLine("No Key Vaults are available for the user in the current RG. If you wish to migrate any Private cerificate to IA, please login as different user or select a different ResourceGroup/Subscription");
                        statusBarViewModel.ShowError("No Key Vaults are available for the user in the current RG. If you wish to migrate any Private cerificate to IA, please login as different user or select a different ResourceGroup/Subscription");
                        IsLoginButttonEnabled = true;
                    }
                }
                else
                {
                    TraceProvider.WriteLine("No Integration Accounts are available for the user. Please login as different user or select a different ResourceGroup/Subscription");
                    statusBarViewModel.ShowError("No Integration Accounts are available for the user. Please login as different user or select a different ResourceGroup/Subscription");
                    IsLoginButttonEnabled = true;
                }
            }
            catch (Exception ex)
            {
                TraceProvider.WriteLine(string.Format("Error reading user IA's from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                statusBarViewModel.ShowError(string.Format("Error reading user IA's  from Portal. Reason: {0}{1}", ExceptionHelper.GetExceptionMessage(ex), "Please Login again or as different user."));
                IsLoginButttonEnabled = true;
            }
        }

       
        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            statusBarViewModel = this.ApplicationContext.GetService<StatusBarViewModel>();

            var cachedintegrationAccountDetails = this.ApplicationContext.GetService<IntegrationAccountDetails>();
            var cachedintegrationServiceDetails = this.ApplicationContext.GetService<IntegrationServiceDetails>();

            AzureLoginButtonClickCommand = new RelayCommand(o => AzureLoginButtonClick("AzureLoginButton"));

            if (cachedintegrationAccountDetails != null)
            {
                
                UserSubscriptions = this.ApplicationContext.GetProperty("Subscriptions") as ObservableCollection<Subscription.RootObject>;
                UserResourceGroups = this.ApplicationContext.GetProperty("ResourceGroups") as ObservableCollection<ResourceGroup.RootObject>;
                UserIntegrationAccounts = this.ApplicationContext.GetProperty("IntegrationAccounts") as ObservableCollection<IntegrationAccount.RootObject>;
                UserKeyVaults = this.ApplicationContext.GetProperty("KeyVaults") as ObservableCollection<KeyVault.RootObject>;

                SelectedSubscription = UserSubscriptions.Where(x=>x.SubscriptionId == cachedintegrationAccountDetails.SubscriptionId).First();
                SelectedResourceGroup = UserResourceGroups.Where(x => x.Name == cachedintegrationAccountDetails.ResourceGroupName).First();
                SelectedIntegrationAccount = UserIntegrationAccounts.Where(x => x.Name == cachedintegrationAccountDetails.IntegrationAccountName).First();
                if (!string.IsNullOrEmpty(cachedintegrationAccountDetails.KeyVaultName))
                {
                    SelectedKeyVault = UserKeyVaults.Where(x => x.Name == cachedintegrationAccountDetails.KeyVaultName).First();
                }
                IsLoginButttonEnabled = false;
                SubscriptionVisible = true;

            }
            else
            {
                UserSubscriptions = new ObservableCollection<Subscription.RootObject>();
                UserResourceGroups = new ObservableCollection<ResourceGroup.RootObject>();
                UserIntegrationAccounts = new ObservableCollection<IntegrationAccount.RootObject>();
                UserKeyVaults = new ObservableCollection<KeyVault.RootObject>();

                IsLoginButttonEnabled = true;
                SubscriptionVisible = false;
                ResourceGroupVisible = false;
                IntegrationAccountVisible = false;
                KeyVaultVisible = false;

            }
            if (cachedintegrationServiceDetails != null)
            {
                this.integrationServiceDetails = cachedintegrationServiceDetails;
            }
            else
            {
                this.integrationServiceDetails = new IntegrationServiceDetails();
                this.ApplicationContext.AddService(this.integrationServiceDetails);
            }
        }

        public bool Validate()
        {
            Debug.Assert(statusBarViewModel != null, "StatusBarViewModel has not been initialized in application context");
            statusBarViewModel.Clear();
            if (SelectedSubscription == null)
            {
                statusBarViewModel.ShowError("Please select a Subscription before proceeding");
            }
            if (SelectedResourceGroup == null)
            {
                statusBarViewModel.ShowError("Please select a Resource Group before proceeding");
            }
            if (SelectedIntegrationAccount == null)
            {
                statusBarViewModel.ShowError("Please select a Integration Account before proceeding");
            }
            if (!string.IsNullOrEmpty(statusBarViewModel.StatusBarText))
            {
                return false;
            }
            var cachedintegrationAccountDetails = this.ApplicationContext.GetService<IntegrationAccountDetails>();
            if (cachedintegrationAccountDetails == null)
            {
                var integrationAccountDetails = new IntegrationAccountDetails();
                integrationAccountDetails.SubscriptionId = SelectedSubscription.SubscriptionId;
                integrationAccountDetails.ResourceGroupName = SelectedResourceGroup.Name;
                integrationAccountDetails.IntegrationAccountName = SelectedIntegrationAccount.Name;
                if (SelectedKeyVault != null)
                {
                    integrationAccountDetails.KeyVaultName = SelectedKeyVault.Name;
                }
                this.ApplicationContext.AddService(integrationAccountDetails);
            }
            else
            {
                cachedintegrationAccountDetails.SubscriptionId = SelectedSubscription.SubscriptionId;
                cachedintegrationAccountDetails.ResourceGroupName = SelectedResourceGroup.Name;
                cachedintegrationAccountDetails.IntegrationAccountName = SelectedIntegrationAccount.Name;
                if (SelectedKeyVault != null)
                {
                    cachedintegrationAccountDetails.KeyVaultName = SelectedKeyVault.Name;
                }
               
            }
            this.ApplicationContext.SetProperty("Subscriptions", UserSubscriptions);
            this.ApplicationContext.SetProperty("ResourceGroups", UserResourceGroups);
            this.ApplicationContext.SetProperty("IntegrationAccounts", UserIntegrationAccounts);
            this.ApplicationContext.SetProperty("KeyVaults", UserKeyVaults);

            return true;
        }
    }
}