//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Windows.Data;
    using System.Windows.Media;

    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string strParam = parameter as string;
            if (strParam != null && strParam.Equals("background", StringComparison.OrdinalIgnoreCase))
            {
                return (bool)value ? Brushes.White : Brushes.Transparent;
            }
            else
            {
                return (bool)value ? Brushes.Black : new SolidColorBrush((Color)ColorConverter.ConvertFromString("#0080c0"));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
