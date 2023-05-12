using System;
using System.Collections.Generic;
using System.Net;
using System.Security;

namespace FFF.Shared
{
    public static class SecureStringExtensions
    {
        public static void AppendString(this SecureString secureString, string text)
        {
            if (secureString == null)
                throw new ArgumentNullException(nameof(secureString));
            if (text == null)
                return;
            ((IEnumerable<char>)text.ToCharArray()).ForEach<char>((Action<char>)(c => secureString.AppendChar(c)));
        }

        public static string Decrypt(this SecureString secureString) => 
            secureString == null ? (string)null : new NetworkCredential(string.Empty, secureString).Password;

        public static SecureString FromString(string text)
        {
            SecureString secureString = new SecureString();
            secureString.AppendString(text);
            return secureString;
        }
    }
}
