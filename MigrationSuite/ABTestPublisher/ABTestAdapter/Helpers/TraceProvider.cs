namespace ABTestAdapter.Helpers
{
    using System;
    using System.Diagnostics;
    using System.Globalization;

    public static class TraceProvider
    {
        public static void WriteLine(string formatStr, params object[] args)
        {
            Trace.WriteLine(string.Format(CultureInfo.InvariantCulture, DateTime.Now + " " + formatStr, args));
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
