//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Diagnostics;
    using System.Windows.Input;

    /// <summary>
    /// Command property of a Button needs to be data bound to an object that implements
    /// System.Windows.Input.ICommand interface. This is an implementation of that.
    /// </summary>
    public class Command : ICommand
    {
        readonly string displayName;
        readonly Func<object, bool> canExecute;
        readonly Action<object> execute;

        public Command(Action<object> execute)
            : this(null, execute)
        {
        }

        public Command(Func<object, bool> canExecute, Action<object> execute)
            : this(null, canExecute, execute)
        {
        }

        public Command(string displayName, Func<object, bool> canExecute, Action<object> execute)
            : this(displayName, null, canExecute, execute)
        {
        }

        public Command(string displayName, string commandName, Action<object> execute)
            : this(displayName, commandName, null, execute)
        {
        }

        public Command(string displayName, string commandName, Func<object, bool> canExecute, Action<object> execute)
        {
            this.displayName = displayName;
            this.CommandName = commandName;
            this.canExecute = canExecute;
            this.execute = execute;
        }

        public event EventHandler CanExecuteChanged;

        // Used for AutomationId (non-localizable)
        public string CommandName
        {
            get;
            private set;
        }

        public string DisplayName
        {
            get
            {
                return this.displayName;
            }
        }

        public bool CanExecute(object parameter)
        {
            return this.canExecute == null || this.canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            Debug.Assert(this.CanExecute(parameter), "Can not Execute since CanExecute is false");
            if (this.execute != null && this.CanExecute(parameter))
            {
                this.execute(parameter);
            }
        }

        public void Refresh()
        {
            if (this.CanExecuteChanged != null)
            {
                this.CanExecuteChanged(this, EventArgs.Empty);
            }
        }
    }
}