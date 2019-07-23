//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using MigrationTool;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Navigation;
    using System.Windows.Threading;

    public partial class ManagementWorkspaceView
    {
        private readonly Navigator navigator;
        private readonly IApplicationContext applicationContext = new ApplicationContext();
        private readonly ManagementWorkspaceViewModel managementWorkspaceViewModel;
        
        private IDictionary<Type, Type> viewModelTypes = new Dictionary<Type, Type>();
        private IPageViewModel currentPageViewModel;
        
        public ManagementWorkspaceView()
        {
            this.InitializeComponent();
            
            this.viewModelTypes.Add(typeof(WelcomePageView), typeof(WelcomePageViewModel));
            this.viewModelTypes.Add(typeof(BizTalkManagementDatabaseDetailsPageView), typeof(BizTalkManagementDbDetailsPageViewModel));
            this.viewModelTypes.Add(typeof(IntegrationServiceDetailsPageView), typeof(IntegrationServiceDetailsPageViewModel));
            this.viewModelTypes.Add(typeof(PartnerSelectionPageView), typeof(PartnerSelectionPageViewModel));
            this.viewModelTypes.Add(typeof(PartnerMigrationPageView), typeof(PartnerMigrationPageViewModel));
            this.viewModelTypes.Add(typeof(AgreementSelectionPageView), typeof(AgreementSelectionPageViewModel));
            this.viewModelTypes.Add(typeof(AgreementMigrationPageView), typeof(AgreementMigrationPageViewModel));
            this.viewModelTypes.Add(typeof(SchemaSelectionPageView), typeof(SchemaSelectionPageViewModel));
            this.viewModelTypes.Add(typeof(SchemaMigrationPageView), typeof(SchemaMigrationPageViewModel));
            this.viewModelTypes.Add(typeof(ExportToIntegrationAccountPageView), typeof(ExportToIntegrationAccountPageViewModel));
            this.viewModelTypes.Add(typeof(ExportArtifactsStatusPageView), typeof(ExportArtifactsStatusPageViewModel));
            this.viewModelTypes.Add(typeof(SummaryPageView), typeof(SummaryPageViewModel));


            
            this.navigator = new Navigator(this.Navigate, CloseApplication);
            this.WizardContent.Navigated += this.OnNavigationFrameNavigated;
            
            this.managementWorkspaceViewModel = new ManagementWorkspaceViewModel(this.navigator);
            this.managementWorkspaceViewModel.Initialize(this.applicationContext);
            this.DataContext = this.managementWorkspaceViewModel;

            this.applicationContext.AddService(this.managementWorkspaceViewModel.StatusBarViewModel);
            this.applicationContext.AddService(this.managementWorkspaceViewModel.ProgressBarViewModel);
            this.applicationContext.AddService(this.managementWorkspaceViewModel.WizardNavigationViewModel);           
        }
        
        private static void CloseApplication()
        {
            Application.Current.Shutdown();
        }
        
        private void OnNavigationFrameNavigated(object sender, NavigationEventArgs e)
        {
            UserControl userControl = e.Content as UserControl;
            if (userControl != null)
            {
                this.currentPageViewModel = this.GetViewModel(userControl.GetType());
                Debug.Assert(this.currentPageViewModel != null, "Expected an instance of IPageViewModel");
                this.currentPageViewModel.Initialize(this.applicationContext);
                (this.currentPageViewModel as BaseViewModel).FatalErrorHandler += this.HandleException;
                userControl.DataContext = this.currentPageViewModel;
                this.Title.Text = this.currentPageViewModel.Title;               
            }
        }

        private void HandleException(object sender, ErrorEventArgs args)
        {
            this.applicationContext.ApplicationException = args.Error;
            this.managementWorkspaceViewModel.WizardNavigationViewModel.ShowSummary();
        }   
        
        private IPageViewModel GetViewModel(Type viewType)
        {
            var targetViewModelType = this.viewModelTypes[viewType];
            return Activator.CreateInstance(targetViewModelType) as IPageViewModel;
        }

        private bool Navigate(Uri uri, bool validateCurrentPage)
        {
            this.managementWorkspaceViewModel.ClearStatus();

            // Every time a page navigation occurs, let the current page do validation.
            // If the validation is not successful, navigator should not navigate to the next page.
            var validatorPageViewModel = this.currentPageViewModel as IValidatorPageViewModel;
            
            if (!validateCurrentPage || validatorPageViewModel == null || validatorPageViewModel.Validate())
            {
                this.WizardContent.Navigate(uri);
                return true;
            }

            return false;
        }

        private void Click_Cancel(object sender, RoutedEventArgs e)
        {
            CloseApplication();
        }

        private void Click_Minimize(object sender, RoutedEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.WindowState = WindowState.Minimized;
        }
        private void HomePage_Click(object sender, RoutedEventArgs e)
        {
            MessageBoxResult result = MessageBox.Show("Do you want to close the tool and navigate to Home Page?", "Warning", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                HomePage hp = new HomePage();
                Main.Children.Clear();
                Main.Children.Add(hp);
            }
        }
        private void TextBlock_MouseLeftButtonDown_1(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.DragMove();
        }
    }
}
