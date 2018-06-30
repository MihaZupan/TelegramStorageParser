using System;
using MihaZupan.TelegramLocalStorage.OpenSSL;
using static MihaZupan.TelegramLocalStorage.Extensions;

namespace MihaZupan.TelegramLocalStorage.TgCrypto
{
    internal class Decrypt
    {
        public static bool TryDecryptLocal(byte[] encrypted, AuthKey key, out byte[] data)
        {
            data = null;

            if (encrypted.Length <= 16 || (encrypted.Length & 0x0F) != 0)
                return false;

            int fullLen = encrypted.Length - 16;

            byte[] encryptedActual = new byte[fullLen];
            Array.Copy(encrypted, 16, encryptedActual, 0, fullLen);
            byte[] messageKey = new byte[16];
            Array.Copy(encrypted, 0, messageKey, 0, 16);

            byte[] decrypted = AesDecryptLocal(encryptedActual, fullLen, key, messageKey);

            byte[] sha1 = new byte[20];
            SHA.SHA1(decrypted, decrypted.Length, sha1);
            if (!CompareBytes(sha1, messageKey, 0, 0, 16))
                return false;

            uint dataLen = BitConverter.ToUInt32(decrypted, 0);
            if (dataLen > decrypted.Length || dataLen <= fullLen - 16 || dataLen < sizeof(uint))
                return false;

            data = new byte[dataLen - 4];
            Array.Copy(decrypted, 4, data, 0, data.Length);
            return true;
        }
        private static byte[] AesDecryptLocal(byte[] encrypted, int length, AuthKey authKey, byte[] key128)
        {
            authKey.PrepareAES_oldmtp(key128, out byte[] aesKey, out byte[] aesIV, false);
            return AesIgeDecryptRaw(encrypted, length, aesKey, aesIV);
        }
        private static byte[] AesIgeDecryptRaw(byte[] encrypted, int length, byte[] key, byte[] iv)
        {
            byte[] decKey = new byte[AesCore.AesKeyStructSize];
            AesCore.AES_set_decrypt_key(key, key.Length * 8, decKey);

            byte[] decrypted = new byte[length];
            AesIGE.AES_ige_encrypt(encrypted, decrypted, length, decKey, iv.Copy(), AesEncrypt.Decrypt);

            return decrypted;
        }
    }
}
