using System;
using MihaZupan.TelegramStorageParser.OpenSSL;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop
{
    internal class AuthKey
    {
        private static readonly byte[] EmptyPassword = new byte[0];

        private readonly byte[] _key;

        public AuthKey(byte[] key)
        {
            if (key.Length < Constants.NeededAuthKeySize) throw new ArgumentException("Key is too small");
            _key = key;
        }

        public static AuthKey CreateLocalKey(byte[] password, byte[] salt)
            => new AuthKey(
                KDF.PKCS5_PBKDF2_HMAC_SHA1(
                    pass: password ?? EmptyPassword,
                    salt: salt,
                    iter: password == null ? Constants.LocalKeyNoPwdIterCount : Constants.LocalKeyIterCount,
                    keylen: Constants.NeededAuthKeySize));

        public bool TryDecryptLocal(byte[] encrypted, out byte[] decrypted)
        {
            decrypted = null;

            if (encrypted.Length <= 16 || (encrypted.Length & 0x0F) != 0)
                return false;

            int fullLen = encrypted.Length - 16;

            byte[] encryptedActual = new byte[fullLen];
            Array.Copy(encrypted, 16, encryptedActual, 0, fullLen);
            byte[] messageKey = new byte[16];
            Array.Copy(encrypted, 0, messageKey, 0, 16);

            byte[] decryptedBlob = AesDecryptLocal(encryptedActual, fullLen, messageKey);

            if (!SHA.SHA1(decryptedBlob).CompareBytes(messageKey, 16))
                return false;

            uint dataLen = BitConverter.ToUInt32(decryptedBlob, 0);
            if (dataLen > decryptedBlob.Length || dataLen <= fullLen - 16 || dataLen < sizeof(uint))
                return false;

            decrypted = new byte[dataLen - 4];
            Array.Copy(decryptedBlob, 4, decrypted, 0, decrypted.Length);
            return true;
        }

        private byte[] AesDecryptLocal(byte[] encrypted, int length, byte[] key128)
        {
            PrepareAES_oldmtp(key128, out byte[] aesKey, out byte[] aesIV, false);
            return AES.AES_IGE_Decrypt(encrypted, length, aesKey, aesIV);
        }
        private void PrepareAES_oldmtp(byte[] msgKey, out byte[] aesKey, out byte[] aesIV, bool send)
        {
            aesKey = new byte[32];
            aesIV = new byte[32];

            int x = send ? 0 : 8;

            byte[] data_a = new byte[48];
            Array.Copy(msgKey, 0, data_a, 0, 16);
            Array.Copy(_key, x, data_a, 16, 32);
            byte[] sha1_a = new byte[20];
            SHA.SHA1(data_a, 48, sha1_a);

            byte[] data_b = new byte[48];
            Array.Copy(_key, 32 + x, data_b, 0, 16);
            Array.Copy(msgKey, 0, data_b, 16, 16);
            Array.Copy(_key, 48 + x, data_b, 32, 16);
            byte[] sha1_b = new byte[20];
            SHA.SHA1(data_b, 48, sha1_b);

            byte[] data_c = new byte[48];
            Array.Copy(_key, 64 + x, data_c, 0, 32);
            Array.Copy(msgKey, 0, data_c, 32, 16);
            byte[] sha1_c = new byte[20];
            SHA.SHA1(data_c, 48, sha1_c);

            byte[] data_d = new byte[48];
            Array.Copy(msgKey, 0, data_d, 0, 16);
            Array.Copy(_key, 96 + x, data_d, 16, 32);
            byte[] sha1_d = new byte[20];
            SHA.SHA1(data_d, 48, sha1_d);

            Array.Copy(sha1_a, 0, aesKey, 0, 8);
            Array.Copy(sha1_b, 8, aesKey, 8, 12);
            Array.Copy(sha1_c, 4, aesKey, 20, 12);
            Array.Copy(sha1_a, 8, aesIV, 0, 12);
            Array.Copy(sha1_b, 0, aesIV, 12, 8);
            Array.Copy(sha1_c, 16, aesIV, 20, 4);
            Array.Copy(sha1_d, 0, aesIV, 24, 8);
        }        
    }
}
