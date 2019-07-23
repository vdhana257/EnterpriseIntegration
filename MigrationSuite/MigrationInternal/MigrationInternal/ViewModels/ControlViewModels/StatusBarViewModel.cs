//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;

    enum StatusInfoType
    {
        Info,
        Warning,
        Error
    }

    class StatusBarViewModel : BaseViewModel
    {
        private string statusBarText;
        private bool isVisible;
        private StatusInfoType statusInfoType;
        private List<string> statusTexts;

        public StatusBarViewModel()
        {
            this.statusTexts = new List<string>();
        }

        public string StatusBarText
        {
            get
            {
                if (this.statusTexts != null && this.statusTexts.Count > 0)
                {
                    var str = string.Join(Environment.NewLine, this.statusTexts);
                    return str;
                }

                return this.statusBarText;
            }

            set
            {
                this.statusBarText = value;
                this.RaisePropertyChanged("StatusBarText");
            }
        }

        public StatusInfoType StatusInfoType
        {
            get
            {
                return this.statusInfoType;
            }

            set
            {
                this.statusInfoType = value;
                this.RaisePropertyChanged("StatusInfoType");
            }
        }

        public bool IsVisible
        {
            get
            {
                return this.isVisible;
            }

            set
            {
                this.isVisible = value;
                this.RaisePropertyChanged("IsVisible");
            }
        }

        public Command StatusBarClearCommand
        {
            get
            {
                return new Command((o) => true, (o) => this.Clear());
            }
        }

        public void ShowError(string text)
        {
            this.ShowStatus(StatusInfoType.Error, text);
        }

        public void ShowWarning(string text)
        {
            this.ShowStatus(StatusInfoType.Warning, text);
        }

        public void ShowInfo(string text)
        {
            this.ShowStatus(StatusInfoType.Info, text);
        }

        public void Clear()
        {
            this.statusTexts.Clear();
            this.StatusBarText = null;
            this.IsVisible = false;
        }

        private void ShowStatus(StatusInfoType infoType, string text)
        {
            this.StatusInfoType = infoType;
            this.IsVisible = true;
            if (text.Length > 0)
            {
                this.statusTexts.Add(text);
            }

            this.RaisePropertyChanged("StatusBarText");
        }
    }
}
