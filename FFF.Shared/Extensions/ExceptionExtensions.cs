using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace FFF.Shared
{
    public static class ExceptionExtensions
    {
        public const int ERROR_SHARING_VIOLATION = -2147024864;

        public static bool IsFileInUse(this IOException ioEx) =>
            ioEx != null && ioEx.HResult == -2147024864;

        public static Exception GetInnermostException(this Exception exception) =>
            WalkInnermostException(exception);

        public static string ToInnerString(this Exception ex) =>
            FindMessageInner(ex);

        public static Exception[] GetAllInnermostException(this Exception exception) =>
            WalkInnerException(exception).ToArray();

        public static bool IsInnerAny(this Exception exception) =>
            exception.InnerException != null;

        private static IEnumerable<Exception> WalkInnerException(this Exception exception)
        {
            yield return exception;

            if (exception.InnerException != null)
                WalkInnerException(exception.InnerException);
        }

        private static string FindMessageInner(Exception ex)
        {
            string temp = string.Empty;
            if (ex != null)
            {
                if (ex.InnerException != null)
                    temp += FindMessageInner(ex.InnerException);

                if (string.IsNullOrWhiteSpace(temp))
                    temp += $"{ex.Message}";
                else
                    temp = $"{ex.Message}{Environment.NewLine}{temp}";
            }
            return temp;
        }

        private static Exception WalkInnermostException(Exception root) =>
            root.InnerException == null ? root : WalkInnermostException(root.InnerException);

        public static string GetTopmostNonemptyMessage(this Exception exception) =>
            WalkInnermostExceptionMessage(exception);

        private static string WalkInnermostExceptionMessage(Exception root)
        {
            if (root == null)
                return string.Empty;
            return !string.IsNullOrWhiteSpace(root.Message) ? root.Message : WalkInnermostExceptionMessage(root.InnerException);
        }

        public static bool IsFatal(this Exception exception)
        {
            while (true)
            {
                switch (exception)
                {
                    case OutOfMemoryException _ when !(exception is InsufficientMemoryException):
                    case ThreadAbortException _:
                        goto label_1;
                    case TypeInitializationException _:
                    case TargetInvocationException _:
                        exception = exception.InnerException;
                        continue;
                    case AggregateException _:
                        goto label_3;
                    default:
                        goto label_12;
                }
            }
        label_1:
            return true;
        label_3:
            foreach (Exception innerException in ((AggregateException)exception).InnerExceptions)
            {
                if (innerException.IsFatal())
                    return true;
            }
        label_12:
            return false;
        }

        public static string Trace(this Exception exception, string label = null)
        {
            string message = string.Format("{0}{1}, HResult {2}", string.IsNullOrWhiteSpace(label) ? (object)string.Empty : (object)(label + ": "), (object)exception, (object)exception.HResult);
            System.Diagnostics.Trace.TraceError(message);
            return message;
        }
    }
}
