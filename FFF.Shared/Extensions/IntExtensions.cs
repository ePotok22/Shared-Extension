using System;

namespace FFF.Shared
{
    public static class IntExtensions
    {
        public static bool IsPrime(this int number)
        {
            if ((number % 2) == 0)
                return number == 2;
            int sqrt = (int)Math.Sqrt(number);
            for (int t = 3; t <= sqrt; t = t + 2)
            {
                if (number % t == 0)
                    return false;
            }
            return number != 1;
        }
    }
}
