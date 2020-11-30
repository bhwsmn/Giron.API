using System.Security.Cryptography;

namespace Giron.API.Helpers
{
    public static class Generator
    {
        public static byte[] GetRandomBytes(int length = 512)
        {
            var key = new byte[length];
            RandomNumberGenerator.Create().GetBytes(key);

            return key;
        }
    }
}