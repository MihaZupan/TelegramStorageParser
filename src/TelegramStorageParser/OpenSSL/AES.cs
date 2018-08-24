using System.Runtime.InteropServices;

namespace MihaZupan.TelegramStorageParser.OpenSSL
{
    internal static class AES
    {
        public static byte[] AES_IGE_Decrypt(byte[] encrypted, int decryptedLength, byte[] key, byte[] iv)
        {
            byte[] decKey = new byte[244];
            AES_set_decrypt_key(key, key.Length * 8, decKey);

            byte[] decrypted = new byte[decryptedLength];
            AES_ige_encrypt(encrypted, decrypted, decryptedLength, decKey, iv.Copy(), 0);

            return decrypted;
        }

        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static int AES_set_decrypt_key(byte[] userKey, int bits, byte[] aesKey);
        
        [DllImport("libcrypto.dll", CallingConvention = CallingConvention.Cdecl)]
        private extern static void AES_ige_encrypt(byte[] @in, byte[] @out, int length, byte[] aesKey, byte[] ivec, int encMode);
    }
}
