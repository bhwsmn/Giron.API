using System;
using System.Text;

namespace Giron.API.Extensions
{
    public static class StringExtensions
    {
        public static string DecodeBase64OrReturnOriginal(this string originalString)
        {
            var isValidBase64String = Convert.TryFromBase64String(
                s: originalString,
                bytes: new Span<byte>(new byte[originalString.Length]),
                bytesWritten: out _
            );

            if (isValidBase64String)
            {
                var base64EncodedBytes = Convert.FromBase64String(originalString);
                var decodedString = Encoding.UTF8.GetString(base64EncodedBytes);

                return decodedString;
            }

            return originalString;
        }
    }
}