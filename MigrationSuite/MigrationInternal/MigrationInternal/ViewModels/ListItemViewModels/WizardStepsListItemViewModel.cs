//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    class WizardStepsListItemViewModel : BaseViewModel
    {
        private bool isCurrentStep;
        
        public WizardStepsListItemViewModel(string description, bool isCurrentStep)
        {
            this.Description = description;
            this.IsCurrentStep = isCurrentStep;
        }

        public string Description { get; private set; }

        public bool IsCurrentStep
        {
            get
            {
                return this.isCurrentStep;
            } 
            
            set
            {
                this.isCurrentStep = value;
                this.RaisePropertyChanged("IsCurrentStep");
            }
        }
    }
}
