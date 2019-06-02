//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Diagnostics;
    using System.Threading.Tasks;
    using System.Windows.Threading;

    // This is what is exposed to the View, and is used by the view to either show the wait cursor, or to show the progress
    // bar, or to remove the progress bar
    public enum ProgressBarState
    {
        EnablePending,
        Enabled,
        Disabled
    }

    // Represents the internal states of the progress bar state machine. The progress bar goes through the following states.
    // After creation, it is in Disabled state. 
    // When an enable command is given, it goes into EnablePending state. In this state the progress bar is not visible.
    // It will become visible after a reasonable delay (such as 2-3 secs). This is to ensure that the progress bar does not
    // show up for really small duration async operations.
    // Once the progress bar goes into Enabled state, we ensure that it stays visible for certain minimum duration (such as
    // 1-2 secs) so that we do not show a flickering effect. After that it goes into OkToDisable state, where it will be immediately 
    // disabled on a disable command. While it's in Enabled state, if a disable command is given, it will go into DisablePending
    // state, but will still be visible. The timeouts are implemented using DispatcherTimer.
    enum InternalProgressBarState
    {
        Disabled,
        EnablePending, 
        Enabled,
        OkToDisable, 
        DisablePending
    }

    class ProgressBarViewModel : BaseViewModel
    {
        private const int ENABLEDELAY = 0; 
        private const int DISABLEDELAY = 0; 

        private InternalProgressBarState state;
        private string progressBarText;
        private object lockObject;
        private DispatcherTimer timer;

        public ProgressBarViewModel()
        {
            this.state = InternalProgressBarState.Disabled;
            this.lockObject = new object();
            this.timer = new DispatcherTimer();
            this.timer.Tick += this.OnTimer;
        }

        public ProgressBarState ProgressBarState
        {
            get
            {
                if (this.state == InternalProgressBarState.Enabled || this.state == InternalProgressBarState.OkToDisable || this.state == InternalProgressBarState.DisablePending)
                {
                    return ProgressBarState.Enabled;
                }
                else if (this.state == InternalProgressBarState.EnablePending)
                {
                    return ProgressBarState.EnablePending;
                }
                else
                {
                    return ProgressBarState.Disabled;
                }
            }
        }

        public string ProgressBarText
        {
            get
            {
                return this.progressBarText;
            }

            private set
            {
                this.progressBarText = value;
                this.RaisePropertyChanged("ProgressBarText");
            }
        }

        public void Update(bool enable)
        {
            this.ChangeState(enable);
        }

        // This is just so that we can update the View.
        // There is a possibility where in the client ViewModel might
        // have already set Enabled to true before the DataContext
        // for ProgressBarViewModel has been assigned to the view.
        public void Refresh()
        {
            this.RaisePropertyChanged("ProgressBarState");
        }

        public void Update(string progressBarText)
        {
            this.ProgressBarText = progressBarText;
        }

        public void Update(bool enable, string progressBarText)
        {
            this.ProgressBarText = progressBarText;
            this.ChangeState(enable);
        }

        public void ExecuteBackgroundTask(string progressBarText, Action backgroundAction)
        {
            // Bring up the progress bar
            this.Update(true, progressBarText);

            // Fire the background task
            var scheduler = TaskScheduler.FromCurrentSynchronizationContext();
            var backgroundTask = Task.Factory.StartNew(backgroundAction);

            backgroundTask.ContinueWith(this.OnBackgroundTaskComplete, TaskScheduler.FromCurrentSynchronizationContext());
        }

        private void OnBackgroundTaskComplete(Task task)
        {
            // When the background task completes, turn off the progress bar.
            // Todo, harsh, handle exceptions from the background task
            this.Update(false);
        }

        private void ChangeState(bool enable)
        {
            lock (this.lockObject)
            {
                switch (this.state)
                {
                    case InternalProgressBarState.Disabled:
                        if (enable)
                        {
                            this.state = InternalProgressBarState.EnablePending;
                            
                            // start the enable delay timer
                            this.timer.Interval = new TimeSpan(0, 0, ENABLEDELAY);
                            this.timer.Start();
                        }

                        break;

                    case InternalProgressBarState.EnablePending:
                        if (!enable)
                        {
                            this.state = InternalProgressBarState.Disabled;

                            // stop the enable delay timer
                            this.timer.Stop();
                        }

                        break;

                    case InternalProgressBarState.Enabled:
                        if (!enable)
                        {
                            this.state = InternalProgressBarState.DisablePending;
                        }

                        break;

                    case InternalProgressBarState.OkToDisable:
                        if (!enable)
                        {
                            this.state = InternalProgressBarState.Disabled;
                        }

                        break;

                    case InternalProgressBarState.DisablePending:
                        if (enable)
                        {
                            this.state = InternalProgressBarState.Enabled;
                        }

                        break;
                }
            }

            this.RaisePropertyChanged("ProgressBarState");
        }

        private void OnTimer(object o, EventArgs sender)
        {
            lock (this.lockObject)
            {
                switch (this.state)
                {
                    case InternalProgressBarState.EnablePending:
                        // Stop the enable delay timer
                        this.state = InternalProgressBarState.Enabled;
                        this.timer.Stop();

                        // Start the disable delay timer
                        this.timer.Interval = new TimeSpan(0, 0, DISABLEDELAY);
                        this.timer.Start();

                        break;

                    case InternalProgressBarState.Enabled:
                        this.state = InternalProgressBarState.OkToDisable;
                        this.timer.Stop();
                        break;
                        
                    case InternalProgressBarState.DisablePending:
                        this.state = InternalProgressBarState.Disabled;
                        this.timer.Stop();
                        break;

                    default:
                        Debug.Assert(true, "Progress bar state is incorrect on timer tick: " + this.state);
                        this.state = InternalProgressBarState.Disabled;
                        this.timer.Stop();
                        break;
                }
            }

            this.RaisePropertyChanged("ProgressBarState");
        }
    }
}
