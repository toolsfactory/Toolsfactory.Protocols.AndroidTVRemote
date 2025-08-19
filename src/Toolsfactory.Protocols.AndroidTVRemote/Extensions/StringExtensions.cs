using System.Text.RegularExpressions;

namespace Toolsfactory.Protocols.AndroidTVRemote.Extensions
{
    public static class StringExtensions
    {
        public static bool IsHexString(this string input)
        {
            if (string.IsNullOrEmpty(input)) return false;
            
            Regex hexPattern = new Regex("^[0-9A-Fa-f]+$");
            return hexPattern.IsMatch(input);
        }

        public static byte[] HexToByteArray(this string hex)
        {
            int numberChars = hex.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            
            return bytes;
        }

        public static string ToHex(this byte[] bytes)
        {
            return BitConverter.ToString(bytes).Replace("-", "").ToLowerInvariant();
        }
    }
}
