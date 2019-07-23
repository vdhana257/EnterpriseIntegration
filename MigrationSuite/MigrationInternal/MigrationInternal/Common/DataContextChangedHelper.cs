//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System.Windows;
    using System.Windows.Data;

    // DataContextChanged event is internal is WPF. In order to run custom code when DataContext is changed,
    // we create this dummy DependencyProperty and on change of that call IDataContextChangedHandler<T>.DataContextChanged
    public static class DataContextChangedHelper<T> where T : FrameworkElement, IDataContextChangedHandler<T>
    {
        public static readonly DependencyProperty InternalDataContextProperty =
            DependencyProperty.Register(
            InternalDataContext,
            typeof(object),
            typeof(T),
            new PropertyMetadata(DataContextChanged));

        private const string InternalDataContext = "InternalDataContext";

        public static void Bind(T control)
        {
            control.SetBinding(InternalDataContextProperty, new Binding()); // new Binding() will bind InternalDataContextProperty to DataContext
        } 
        
        private static void DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            T control = (T)sender;
            (control as IDataContextChangedHandler<T>).DataContextChanged(control, e);
        }
    }
}
