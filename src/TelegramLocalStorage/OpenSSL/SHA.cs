using System.Runtime.InteropServices;
using System.Security;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    internal class SHA
    {
        [SuppressUnmanagedCodeSecurity]
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void SHA1(byte[] data, int length, byte[] messageDigest);        
    }
}
