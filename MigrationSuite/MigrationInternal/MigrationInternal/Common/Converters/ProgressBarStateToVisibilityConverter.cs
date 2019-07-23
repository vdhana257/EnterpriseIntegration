//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Windows;
    using System.Windows.Data;

    class ProgressBarStateToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ProgressBarState)
            {
                var state = (ProgressBarState)value;
                return state == ProgressBarState.Disabled ? Visibility.Collapsed : Visibility.Visible;
            }

            throw new InvalidOperationException("Value must be of type MigrationStatus");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
