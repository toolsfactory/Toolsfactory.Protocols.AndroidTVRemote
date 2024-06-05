using System;
using System.Linq;
using System.Text.RegularExpressions;
namespace System
{
    public static class StringExtensions
    {
        public static bool IsHexString(this string input)
        {
            if (string.IsNullOrEmpty(input))
                return false;
            Regex hexPattern = new Regex("^[0-9A-Fa-f]+$");
            return hexPattern.IsMatch(input);
        }

        public static byte[] HexToByteArray(this String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}