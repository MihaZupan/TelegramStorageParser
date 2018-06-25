using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    class SHA
    {
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void SHA1(byte[] data, int length, byte[] messageDigest);        
    }
}
