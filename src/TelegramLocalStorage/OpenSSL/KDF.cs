using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    class KDF
    {
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int PKCS5_PBKDF2_HMAC_SHA1(byte[] pass, int passlen, byte[] salt, int saltlen, int iter, int keylen, byte[] @out);
    }
}
