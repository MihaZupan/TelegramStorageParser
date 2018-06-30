using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    internal class SHA
    {
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void SHA1(byte[] data, int length, byte[] messageDigest);        
    }
}
