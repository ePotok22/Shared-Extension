using System;
using System.ComponentModel;

namespace FFF.Shared
{
    public static class EventDescriptorExtensions
    {
        public static void SubscribeHandler(
          this EventDescriptor eventInfo,
          object eventSource,
          object handlerInstance,
          string methodName)
        {
            eventInfo.AddEventHandler(eventSource, Delegate.CreateDelegate(eventInfo.EventType, handlerInstance, methodName));
        }

        public static void ClearSubcribedHandlers(
          this EventDescriptor eventInfo,
          object eventSource,
          Delegate eventDelegate)
        {
            foreach (Delegate invocation in eventDelegate.GetInvocationList())
                eventInfo.RemoveEventHandler(eventSource, invocation);
        }
    }
}
