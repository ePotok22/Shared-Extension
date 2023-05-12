using System;
using System.Collections.Generic;

namespace FFF.Shared
{
    public static class Memoization
    {
        public static Func<T, R> Memoize<T, R>(Func<T, R> expensiveGet)
        {
            Dictionary<T, R> cache = new Dictionary<T, R>();
            return (Func<T, R>)(key =>
            {
                R r1;
                if (cache.TryGetValue(key, out r1))
                    return r1;
                R r2 = expensiveGet(key);
                cache[key] = r2;
                return r2;
            });
        }
    }
}
