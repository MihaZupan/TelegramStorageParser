using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    class KDF
    {
        public static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, byte[] salt, int iter, byte[] @out)
        {
            return PKCS5_PBKDF2_HMAC_SHA1(pass, pass.Length, salt, salt.Length, iter, @out.Length, @out);
        }

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, int passlen, byte[] salt, int saltlen, int iter, int keylen, byte[] @out);
    }
}
