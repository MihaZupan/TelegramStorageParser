using System.Runtime.InteropServices;
using System.Security;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    internal class AesCore
    {
        public const int AesKeyStructSize = 244;

        [SuppressUnmanagedCodeSecurity]
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AES_set_decrypt_key(byte[] userKey, int bits, byte[] aesKey);

        [SuppressUnmanagedCodeSecurity]
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AES_set_encrypt_key(byte[] userKey, int bits, byte[] aesKey);
    }
}
