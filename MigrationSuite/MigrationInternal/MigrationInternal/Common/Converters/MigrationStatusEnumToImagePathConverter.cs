//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    class MigrationStatusEnumToImagePathConverter : IValueConverter
    {
        private static readonly string[] MigrationStatusIconPaths = new[]
            {
                "/Images/loading.gif",
                "/Images/warning.png",
                "/Images/success.png",
                "/Images/error.png"
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is MigrationStatus)
            {
                var status = (MigrationStatus)value;
                return MigrationStatusIconPaths[(int)status]; 
            }

            throw new InvalidOperationException("Value must be of type MigrationStatus");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}