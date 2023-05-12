using System;
using System.Threading;

namespace FFF.Shared
{
    public static class ActionExtensions
    {
        public static void ExecuteWithRetry(
          this Action func,
          int retryCount = 3,
          int retryInterval = 500,
          bool rethrowLastException = true)
        {
            for (int index = 0; index < retryCount; ++index)
            {
                bool flag = index == retryCount - 1;
                try
                {
                    func();
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceError(ex.ToString());
                    if (!flag)
                        Thread.Sleep(retryInterval);
                    else if (rethrowLastException)
                        throw;
                }
            }
        }
    }
}
