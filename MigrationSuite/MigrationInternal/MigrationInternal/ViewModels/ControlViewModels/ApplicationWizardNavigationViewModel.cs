namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.ObjectModel;

    internal class ApplicationWizardNavigationViewModel : BaseViewModel
    {
        private readonly string[] pageUris = new string[]
            {
                "Views/WelcomePageView.xaml",
                "Views/BizTalkManagementDatabaseDetailsPageView.xaml",
                "Views/ApplicationSelectionView.xaml",
                "Views/SchemaSelectionPageView.xaml",
                "Views/SchemaMigrationPageView.xaml",
                "Views/MapSelectionPageView.xaml",
                "Views/MapMigrationPageView.xaml",
                "Views/IntegrationServiceDetailsPageView.xaml",
                "Views/ApplicationExportToIntegrationAccountPageView.xaml",
                 "Views/ExportArtifactsStatusPageView.xaml",
                "Views/SummaryPageView.xaml"
            };

        private readonly WizardStepsListItemViewModel[] wizardSteps = new WizardStepsListItemViewModel[]
            {
                new WizardStepsListItemViewModel(Resources.WelcomeStepDescriptionText, true),
                new WizardStepsListItemViewModel(Resources.SourceDetailsStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.ApplicationSelectionStepDescriptionText,false),
                new WizardStepsListItemViewModel(Resources.SchemaSelectionStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.SchemaImportStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.MapSelectionStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.MapImportStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.DestinationDetailsStepDescriptionText, false),
                new WizardStepsListItemViewModel(Resources.ExportToIntegrationAccountStepDescription, false),
                new WizardStepsListItemViewModel(Resources.ExportArtifactsStatusStepDescription, false),
                new WizardStepsListItemViewModel(Resources.SummaryStepDescriptionText, false)
            };

        private int currentStepIndex;
        private Command moveNextCommand;
        private Command movePreviousCommand;
        private Command closeCommand;
        private Navigator navigator;
        private bool isApplicationClosing;
        private IApplicationContext applicationContext;

        public ApplicationWizardNavigationViewModel(Navigator navigator)
        {
            this.WizardSteps = new ReadOnlyCollection<WizardStepsListItemViewModel>(this.wizardSteps);
            this.navigator = navigator;
        }

        public ReadOnlyCollection<WizardStepsListItemViewModel> WizardSteps { get; private set; }

        public Command MoveNextCommand
        {
            get
            {
                if (this.moveNextCommand == null)
                {
                    this.moveNextCommand = new Command(
                        Resources.NextNavigationButtonLabel,
                        "MoveNextCommand",
                        (o) => this.IsMoveNextCommandEnabled,
                        (o) => this.MoveToNextStep());
                }

                return this.moveNextCommand;
            }
        }

        public bool IsApplicationClosing
        {
            get
            {
                return this.isApplicationClosing;
            }

            set
            {
                this.isApplicationClosing = value;
                this.RaisePropertyChanged("IsApplicationClosing");
            }
        }

        public bool IsPreviousVisible
        {
            get
            {
                if (this.applicationContext.ApplicationException == null)
                {
                    return this.currentStepIndex == 0;
                }
                else
                {
                    return true;
                }
            }
        }

        public bool IsLastStep
        {
            get
            {
                return this.currentStepIndex == this.wizardSteps.Length - 1;
            }
        }

        public Command CloseCommand
        {
            get
            {
                if (this.closeCommand == null)
                {
                    this.closeCommand = new Command(
                        Resources.CloseNavigationButtonLabel,
                        "CloseCommand",
                        (o) => this.IsLastStep,
                        (o) => this.navigator.CloseApplication());
                }

                return this.closeCommand;
            }
        }

        public bool IsMoveNextCommandEnabled
        {
            get
            {
                return this.currentStepIndex != this.wizardSteps.Length - 1;
            }
        }

        public Command MovePreviousCommand
        {
            get
            {
                if (this.movePreviousCommand == null)
                {
                    this.movePreviousCommand = new Command(
                        Resources.PreviousNavigationButtonLabel,
                        "MovePreviousCommand",
                        (o) => this.IsMovePreviousCommandEnabled,
                        (o) => this.MoveToPreviousStep());
                }

                return this.movePreviousCommand;
            }
        }

        public bool IsMovePreviousCommandEnabled
        {
            get
            {
                return this.currentStepIndex != 0 && !this.IsApplicationClosing;
            }
        }

        public override void Initialize(IApplicationContext applicationContext)
        {
            base.Initialize(applicationContext);
            this.applicationContext = applicationContext;
            this.currentStepIndex = 0;
            this.navigator.Navigate(new Uri(this.pageUris[this.currentStepIndex], UriKind.Relative));
        }

        public void MoveToNextStep()
        {
            if (this.currentStepIndex != this.wizardSteps.Length)
            {
                bool navigated = this.navigator.Navigate(
                    new Uri(this.pageUris[this.currentStepIndex + 1], UriKind.Relative), true);
                if (navigated)
                {
                    this.wizardSteps[this.currentStepIndex++].IsCurrentStep = false;
                    this.wizardSteps[this.currentStepIndex].IsCurrentStep = true;
                    this.ApplicationContext.LastNavigation = NavigateAction.Next;
                }

                this.Refresh();
            }
        }

        public void MoveToPreviousStep()
        {
            if (this.currentStepIndex != 0)
            {
                this.wizardSteps[this.currentStepIndex--].IsCurrentStep = false;
                this.wizardSteps[this.currentStepIndex].IsCurrentStep = true;
                this.navigator.Navigate(new Uri(this.pageUris[this.currentStepIndex], UriKind.Relative));
                this.ApplicationContext.LastNavigation = NavigateAction.Previous;
                this.Refresh();
            }
        }

        public void ShowSummary()
        {
            this.wizardSteps[this.currentStepIndex].IsCurrentStep = false;
            this.currentStepIndex = this.wizardSteps.Length - 1;
            this.wizardSteps[this.currentStepIndex].IsCurrentStep = true;
            this.navigator.Navigate(new Uri(this.pageUris[this.wizardSteps.Length - 1], UriKind.Relative));
            this.IsApplicationClosing = true;
            this.Refresh();
        }

        private void Refresh()
        {
            this.MoveNextCommand.Refresh();
            this.MovePreviousCommand.Refresh();
            this.CloseCommand.Refresh();
            this.RaisePropertyChanged("IsLastStep");
            this.RaisePropertyChanged("IsPreviousVisible");
        }
    }
}
