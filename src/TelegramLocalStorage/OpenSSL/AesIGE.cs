using System.Runtime.InteropServices;
using System.Security;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    internal class AesIGE
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void AES_ige_encrypt(byte[] @in, byte[] @out, int length, byte[] aesKey, byte[] ivec, AesEncrypt enc);
    }
}
