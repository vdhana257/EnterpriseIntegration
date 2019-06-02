//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace Microsoft.Windows.Azure.BizTalkService.ClientTools.TpmMigration
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    static class TraceProvider
    {

        public static void WriteLine(string formatStr, params object[] args)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, DateTime.Now + " " + formatStr, args));
        }

        public static void WriteLine(string str)
        {
            Trace.WriteLine(DateTime.Now + " " + str);
        }

        public static void WriteLine(string formatStr, bool noDate,params object[] args)
        {
            if (noDate == true)
            {
                Trace.WriteLine(string.Format(CultureInfo.InvariantCulture,formatStr, args));
            }
        }

        public static void WriteLine(string str, bool noDate)
        {
            if (noDate == true)
            {
                Trace.WriteLine(str);
            }
        }


        public static void WriteLine()
        {
            Trace.WriteLine("-");
        }

        public static void WriteLineIf(bool condition, string formatStr, params object[] args)
        {
            Trace.WriteLineIf(condition, string.Format(CultureInfo.InvariantCulture, DateTime.Now + " " + formatStr, args));
        }

        public static void MethodStart(string methodName, string formatStr, params object[] args)
        {
            Trace.WriteLine(DateTime.Now + " [" + methodName + "]: " + string.Format(CultureInfo.InvariantCulture, formatStr, args)); 
        }
    }
}
