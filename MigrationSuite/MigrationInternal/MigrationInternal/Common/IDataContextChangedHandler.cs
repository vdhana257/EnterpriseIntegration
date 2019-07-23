//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Windows;

    public interface IDataContextChangedHandler<T> where T : FrameworkElement
    {
        void DataContextChanged(T sender, DependencyPropertyChangedEventArgs e);
    } 
}
