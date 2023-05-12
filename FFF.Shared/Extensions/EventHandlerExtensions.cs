using System;
using System.Diagnostics;

namespace FFF.Shared
{
    public static class EventHandlerExtensions
    {
        public static void SafeInvoke(this EventHandler eventHandler, object sender)
        {
            Delegate[] invocationList = eventHandler?.GetInvocationList();
            if (invocationList == null || invocationList.Length < 1)
                return;
            foreach (Delegate @delegate in invocationList)
            {
                try
                {
                    ((EventHandler)@delegate)(sender, EventArgs.Empty);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

        public static void SafeInvoke<TArgs>(this EventHandler<TArgs> eventHandler, object sender, TArgs args)
        {
            Delegate[] invocationList = eventHandler?.GetInvocationList();
            if (invocationList == null || invocationList.Length < 1)
                return;
            foreach (Delegate @delegate in invocationList)
            {
                try
                {
                    ((EventHandler<TArgs>)@delegate)(sender, args);
                }
                catch (Exception ex)
                {
                    Trace.TraceError(ex.ToString());
                }
            }
        }

    }
}
