using System;
using System.Threading;
using System.Threading.Tasks;

namespace FFF.Shared
{
    public static class TaskExtensions
    {
        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout)
        {
            return GenericTimeoutAfter<TResult>(task, millisecondsTimeout) as Task<TResult>;
        }

        public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, TimeSpan time)
        {
            return GenericTimeoutAfter<TResult>(task, time) as Task<TResult>;
        }

        public static Task TimeoutAfter(this Task task, int millisecondsTimeout)
        {
            return GenericTimeoutAfter<bool>(task, millisecondsTimeout);
        }

        public static Task TimeoutAfter(this Task task, TimeSpan time)
        {
            return GenericTimeoutAfter<bool>(task, time);
        }

        private static Task GenericTimeoutAfter<TResult>(Task task, TimeSpan time)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (time == Timeout.InfiniteTimeSpan))
            {
                //Console.WriteLine("task.IsCompleted");
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }
            // tcs.Task will be returned as a proxy to the caller
            var tcs = new TaskCompletionSource<TResult>();

            // Short-circuit #2: zero timeout
            if (time.TotalMilliseconds == 0)
            {
                // Console.WriteLine("millisecondsTimeout == 0");
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            var timer = new Timer(state => tcs.TrySetException(new TimeoutException()), null, time, Timeout.InfiniteTimeSpan);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith(antecedent =>
            {
                timer.Dispose();
                MarshalTaskResults(antecedent, tcs);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        private static Task GenericTimeoutAfter<TResult>(Task task, int millisecondsTimeout)
        {
            // Short-circuit #1: infinite timeout or task already completed
            if (task.IsCompleted || (millisecondsTimeout == Timeout.Infinite))
            {
                //Console.WriteLine("task.IsCompleted");
                // Either the task has already completed or timeout will never occur.
                // No proxy necessary.
                return task;
            }
            // tcs.Task will be returned as a proxy to the caller
            var tcs = new TaskCompletionSource<TResult>();

            // Short-circuit #2: zero timeout
            if (millisecondsTimeout == 0)
            {
                // Console.WriteLine("millisecondsTimeout == 0");
                // We've already timed out.
                tcs.SetException(new TimeoutException());
                return tcs.Task;
            }

            // Set up a timer to complete after the specified timeout period
            var timer = new Timer(state => tcs.TrySetException(new TimeoutException()), null, millisecondsTimeout, Timeout.Infinite);

            // Wire up the logic for what happens when source task completes
            task.ContinueWith(antecedent =>
            {
                timer.Dispose();
                MarshalTaskResults(antecedent, tcs);
            }, CancellationToken.None, TaskContinuationOptions.ExecuteSynchronously, TaskScheduler.Default);

            return tcs.Task;
        }

        private static void MarshalTaskResults<TResult>(Task source, TaskCompletionSource<TResult> proxy)
        {
            if (source == null)
                return;

            switch (source.Status)
            {
                case TaskStatus.Faulted:
                    proxy.TrySetException(source.Exception ?? new Exception());
                    break;
                case TaskStatus.Canceled:
                    proxy.TrySetCanceled();
                    break;
                case TaskStatus.RanToCompletion:
                    Task<TResult> castedSource = source as Task<TResult>;
                    proxy.TrySetResult(castedSource == null
                        ? default // source is a Task
                        : castedSource.Result); // source is a Task<TResult>
                    break;
            }
        }

    }
}
