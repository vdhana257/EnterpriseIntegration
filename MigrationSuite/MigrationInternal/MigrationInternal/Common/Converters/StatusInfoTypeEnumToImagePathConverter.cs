//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Globalization;
    using System.Windows.Data;

    class StatusInfoTypeEnumToImagePathConverter : IValueConverter
    {
        private static readonly string[] StatusInfoTypeIconPaths = new[]
            {
                null,
                "/Images/warning.png",
                "/Images/error.png"
            };

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is StatusInfoType)
            {
                var infoType = (StatusInfoType)value;
                return StatusInfoTypeIconPaths[(int)infoType];
            }

            throw new InvalidOperationException("Value must be of type MigrationStatus");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
