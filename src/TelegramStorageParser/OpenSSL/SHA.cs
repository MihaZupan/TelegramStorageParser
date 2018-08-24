using System.Runtime.InteropServices;

namespace MihaZupan.TelegramStorageParser.OpenSSL
{
    internal static class SHA
    {
        public static byte[] SHA1(byte[] data)
            => SHA1(data, data.Length);
        public static byte[] SHA1(byte[] data, int length)
        {
            byte[] digest = new byte[20];
            SHA1(data, length, digest);
            return digest;
        }

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void SHA1(byte[] data, int length, byte[] messageDigest);        
    }
}
