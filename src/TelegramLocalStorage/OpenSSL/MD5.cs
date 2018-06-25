using System.Runtime.InteropServices;

namespace MihaZupan.TelegramLocalStorage.OpenSSL
{
    public class Md5
    {
        private byte[] md5_ctx = new byte[92];

        public Md5()
        {
            MD5_Init(md5_ctx);
        }

        public void Update(byte[] data)
        {
            MD5_Update(md5_ctx, data, data.Length);
        }

        public byte[] Finalize()
        {
            byte[] md = new byte[16];
            MD5_Final(md, md5_ctx);
            return md;
        }

        public static byte[] ComputeHash(byte[] data)
        {
            byte[] md = new byte[16];
            MD5(data, data.Length, md);
            return md;
        }

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        public extern static void MD5(byte[] data, int length, byte[] messageDigest);

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void MD5_Init(byte[] ctx);

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void MD5_Update(byte[] ctx, byte[] data, int length);

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void MD5_Final(byte[] md, byte[] ctx);
    }
}
