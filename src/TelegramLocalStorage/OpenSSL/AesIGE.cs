using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    class AesIGE
    {
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void AES_ige_encrypt(byte[] @in, byte[] @out, int length, byte[] aesKey, byte[] ivec, AesEncrypt enc);
    }
}
