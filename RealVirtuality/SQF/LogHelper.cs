using System;

namespace RealVirtuality.SQF
{
    internal static class LogHelper
    {
        internal enum EInfo
        {
        }
        internal static string GetName(EInfo log)
        {
            return Enum.GetName(typeof(EInfo), log);
        }
        internal static string GetLog(EInfo log, params object[] args)
        {
            string logString;
            switch (log)
            {
                default:
                    throw new NotImplementedException();
            }
            return string.Format(string.Concat('I', (int)log, '|', logString), args);
        }
        internal enum EWarning
        {
        }
        internal static string GetName(EWarning log)
        {
            return Enum.GetName(typeof(EWarning), log);
        }
        internal static string GetLog(EWarning log, params object[] args)
        {
            string logString;
            switch (log)
            {
                default:
                    throw new NotImplementedException();
            }
            return string.Format(string.Concat('W', (int)log, '|', logString), args);
        }
        internal enum EError
        {
            EOF_Reached,
            InvalidAssignment
        }
        internal static string GetName(EError log)
        {
            return Enum.GetName(typeof(EError), log);
        }
        internal static string GetLog(EError log, params object[] args)
        {
            string logString;
            switch (log)
            {
                case EError.EOF_Reached:
                    logString = Properties.Localization.SQF_PARSE_E0_EOFReached;
                    break;
                case EError.InvalidAssignment:
                    logString = Properties.Localization.SQF_PARSE_E1_InvalidAssignment;
                    break;
                default:
                    throw new NotImplementedException();
            }
            return string.Format(string.Concat('E', (int)log, '|', logString), args);
        }
    }
}