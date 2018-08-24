using System.Runtime.InteropServices;

namespace MihaZupan.TelegramStorageParser.OpenSSL
{
    internal static class KDF
    {
        public static byte[] PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, byte[] salt, int iter, int keylen)
        {
            byte[] key = new byte[keylen];
            PKCS5_PBKDF2_HMAC_SHA1(pass, pass.Length, salt, salt.Length, iter, keylen, key);
            return key;
        }
        public static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, byte[] salt, int iter, byte[] @out)
            => PKCS5_PBKDF2_HMAC_SHA1(pass, pass.Length, salt, salt.Length, iter, @out.Length, @out);

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, int passlen, byte[] salt, int saltlen, int iter, int keylen, byte[] @out);
    }
}
