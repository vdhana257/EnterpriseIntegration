//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Reflection;

    static class PropertyChangedHelper
    {
        public static void Notify(PropertyChangedEventHandler handler, object sender, string propertyName)
        {
            ValidatePropertyName(sender, propertyName);
            if (handler != null)
            {
                handler(sender, new PropertyChangedEventArgs(propertyName));
            }
        }

        [Conditional("DEBUG")]
        static void ValidatePropertyName(object sender, string propertyName)
        {
            PropertyInfo property = sender.GetType().GetProperty(propertyName);
            Debug.Assert(property != null, "Unable to bind to property named " + propertyName);
        }
    }
}
