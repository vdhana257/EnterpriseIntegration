//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Windows;
    using System.Windows.Controls;

    public class DataGridExtensions
    {
        // DataGridColumn Header property isn't a dependency property so we can't bind a resource string
        // to it. This attached property works around this limitation by explicitly setting the header
        // to the bound property when it changes.
        public static readonly DependencyProperty HeaderNameProperty = DependencyProperty.RegisterAttached(
            "HeaderName",
            typeof(string),
            typeof(DataGridExtensions),
            new PropertyMetadata(OnHeaderNamePropertyChanged));

        public static string GetHeaderName(DependencyObject element)
        {
            return (string)element.GetValue(HeaderNameProperty);
        }

        public static void SetHeaderName(DependencyObject element, string value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(HeaderNameProperty, value);
        }

        private static void OnHeaderNamePropertyChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DataGridColumn column = sender as DataGridColumn;
            if (column != null)
            {
                column.Header = e.NewValue as string;
            }
        }
    }
}
