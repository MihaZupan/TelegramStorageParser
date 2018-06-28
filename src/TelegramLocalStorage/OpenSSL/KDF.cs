using System.Runtime.InteropServices;
using System.Security;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    internal class KDF
    {
        public static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, byte[] salt, int iter, byte[] @out)
        {
            return PKCS5_PBKDF2_HMAC_SHA1(pass, pass.Length, salt, salt.Length, iter, @out.Length, @out);
        }

        [SuppressUnmanagedCodeSecurity]
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, int passlen, byte[] salt, int saltlen, int iter, int keylen, byte[] @out);
    }
}
