using System;
using System.Runtime.CompilerServices;

namespace FFF.Shared
{
    public static class FuncExtensions
    {
        public static U SafeInvoke<T, U>(this Func<T, U> func, T arg, [CallerMemberName] string operation = "")
        {
            try
            {
                return func(arg);
            }
            catch (Exception ex)
            {
                ex.Trace(operation);
            }
            return default(U);
        }
    }
}
