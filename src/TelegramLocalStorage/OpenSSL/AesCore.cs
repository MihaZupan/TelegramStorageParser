using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    class AesCore
    {
        public const int AesKeyStructSize = 244;

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AES_set_decrypt_key(byte[] userKey, int bits, byte[] aesKey);

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static int AES_set_encrypt_key(byte[] userKey, int bits, byte[] aesKey);
    }
}
