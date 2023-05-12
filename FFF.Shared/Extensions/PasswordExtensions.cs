using System;
using System.Security;
using System.Text.RegularExpressions;

namespace FFF.Shared
{
    public static class PasswordExtensions
    {
        public static bool IsStrongPassword(this string s)
        {
            bool isStrong = Regex.IsMatch(s, @"[\d]");
            if (isStrong) isStrong = Regex.IsMatch(s, @"[a-z]");
            if (isStrong) isStrong = Regex.IsMatch(s, @"[A-Z]");
            if (isStrong) isStrong = Regex.IsMatch(s, @"[\s~!@#\$%\^&\*\(\)\{\}\|\[\]\\:;'?,.`+=<>\/]");
            if (isStrong) isStrong = s.Length > 7;
            return isStrong;
        }

        /// <summary>
        /// Converts a string into a "SecureString"
        /// </summary>
        /// <param name="str">Input String</param>
        /// <returns></returns>
        public static SecureString AsSecureString(this string str)
        {
           SecureString secureString = new SecureString();
            foreach (Char c in str)
                secureString.AppendChar(c);

            return secureString;
        }
    }
}
