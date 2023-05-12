using System;
using System.Security.Cryptography;
using System.Text;

namespace FFF.Shared.Utilities
{
    public static class HashUtilities
    {
        public static string ComputeHash(string input, HashAlgorithm hashAlgorithm)
        {
            if (input == null)
                return (string)null;
            byte[] bytes = Encoding.UTF8.GetBytes(input);
            return ToHashString(hashAlgorithm.ComputeHash(bytes));
        }

        public static string ToHashString(byte[] hash)
        {
            StringBuilder stringBuilder = new StringBuilder(hash.Length * 2);
            for (int index = 0; index < hash.Length; ++index)
                stringBuilder.Append(hash[index].ToString("x2"));
            return stringBuilder.ToString();
        }

        public static string ComputeSHA256Hash(string input)
            => ComputeHash(input, (HashAlgorithm)SHA256.Create());

        public static string ComputeSHA256HashBase64Encoded(string input)
        {
            using (SHA256 shA256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(input);
                return Convert.ToBase64String(shA256.ComputeHash(bytes));
            }
        }
    }
}
