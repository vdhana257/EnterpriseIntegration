//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;

    class BaseViewModel : INotifyPropertyChanged
    {
        public event EventHandler<ErrorEventArgs> FatalErrorHandler;

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected IApplicationContext ApplicationContext
        {
            get;
            set;
        }       
        
        public virtual void Initialize(IApplicationContext applicationContext)
        {
            this.ApplicationContext = applicationContext;
        }

        public string ContactSupport
        {
            get
            {
                return string.Format("{0}: {1}", "For any help/queries, please contact ", Resources.SupportDL);
            }
        }
        protected void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedHelper.Notify(this.PropertyChanged, this, propertyName);
        }

        protected void RaiseFatalError(Exception e)
        {
            if (this.FatalErrorHandler != null)
            {
                this.FatalErrorHandler(this, new ErrorEventArgs(e));
            }
        }
    }
}
