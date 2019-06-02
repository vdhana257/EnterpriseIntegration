//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Collections.Generic;

    using Server = Microsoft.BizTalk.B2B.PartnerManagement;

    public enum NavigateAction
    {
        Previous,
        None,
        Next
    }

    public class ApplicationContext : IApplicationContext
    {
        private const string LastNavigationPropertyName = "LastNavigation";
        private const string ApplicationExceptionPropertyName = "ApplicationException";

        private Server.TpmContext bizTalkTpmContext;
        private IDictionary<Type, object> services = new Dictionary<Type, object>();
        private IDictionary<string, object> properties = new Dictionary<string, object>();
        object propertiesLock = new object();
        object servicesLock = new object();

        public ApplicationContext()
        {
            this.LastNavigation = NavigateAction.None;
        }

        public NavigateAction LastNavigation
        {
            get
            {
                return (NavigateAction)this.GetProperty(LastNavigationPropertyName);
            }

            set
            {
                this.SetProperty(LastNavigationPropertyName, value);
            }
        }

        public Exception ApplicationException
        {
            get
            {
                return (Exception)this.GetProperty(ApplicationExceptionPropertyName);
            }

            set
            {
                this.SetProperty(ApplicationExceptionPropertyName, value);
            }
        }

        public Server.TpmContext GetBizTalkServerTpmContext()
        {
                // Removed the IF Condition to create a new context for new entry of server details. Else it takes from the cached server details.
                // One Way Agreement migrator and Protocol Settings have the same application context. So , even if we create a new context if db details are changed, the two classes will have the same context.
                var bizTalkDbDetails = this.GetService<BizTalkManagementDBDetails>();
                if (bizTalkDbDetails == null)
                {
                    throw new InvalidOperationException("BizTalk Management DB Details have not been initialized");
                }

                this.bizTalkTpmContext = TpmContextFactory.CreateTpmContext<Server.TpmContext>(this);
                return this.bizTalkTpmContext;
        }

        public object GetProperty(string name)
        {
            lock (this.propertiesLock)
            {
                object retObj = null;
                if (this.properties.TryGetValue(name, out retObj))
                {
                    return retObj;
                }
                else
                {
                    return null;
                }
            }
        }

        public void SetProperty(string name, object value)
        {
            lock (this.propertiesLock)
            {
                if (this.properties.ContainsKey(name))
                {
                    this.properties[name] = value;
                }
                else
                {
                    this.properties.Add(name, value);
                }
            }
        }

        public T GetService<T>() where T : class
        {
            lock (this.servicesLock)
            {
                object retObj = null;
                if (this.services.TryGetValue(typeof(T), out retObj))
                {
                    return retObj as T;
                }
                else
                {
                    return null;
                }
            }
        }

        public void AddService<T>(T service) where T : class
        {
            lock (this.servicesLock)
            {
                if (this.services.ContainsKey(typeof(T)))
                {
                    throw new InvalidOperationException();
                }
                else
                {
                    this.services.Add(typeof(T), service);
                }
            }
        }
    }
}
