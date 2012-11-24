using System;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace Doredis
{
    public static class StringExtensions
    {
        /// <summary>
        /// Compares the string against a given pattern.
        /// </summary>
        /// <param name="str">The string.</param>
        /// <param name="wildcard">The wildcard, where "*" means any sequence of characters, and "?" means any single character.</param>
        /// <returns><c>true</c> if the string matches the given pattern; otherwise <c>false</c>.</returns>
        public static bool Like(this string str, string wildcard)
        {
            return new Regex(
                "^" + Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", ".") + "$",
                RegexOptions.IgnoreCase | RegexOptions.Singleline
            ).IsMatch(str);
        }

        public static string Utf8Sha1Hash(this string str)
        {
            SHA1 hash = SHA1CryptoServiceProvider.Create();
            byte[] plainTextBytes = Encoding.UTF8.GetBytes(str);
            byte[] hashBytes = hash.ComputeHash(plainTextBytes);
            return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
