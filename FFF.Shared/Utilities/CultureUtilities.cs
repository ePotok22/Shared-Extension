using System.Globalization;
using System.Threading;

namespace FFF.Shared.Utilities
{
    public static class CultureUtilities
    {
        public static void SetCurrentThreadCultureInfo(string name) =>
             SetCurrentThreadCultureInfo(new CultureInfo(name));

        public static void SetCurrentThreadCultureInfo(int culture) =>
             SetCurrentThreadCultureInfo(new CultureInfo(culture));

        public static void SetCurrentThreadCultureInfo(CultureInfo culture)
        {
            CultureInfo.CurrentCulture = culture;
            CultureInfo.CurrentUICulture = culture;
            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
