using System;
using MihaZupan.TelegramLocalStorage.OpenSSL;

namespace MihaZupan.TelegramLocalStorage.TgCrypto
{
    internal class AuthKey
    {
        private static byte[] EmptyPassword = new byte[0];

        public AuthKey(byte[] key)
        {
            if (key.Length < Constants.NeededAuthKeySize) throw new ArgumentException("key is too small");
            _key = key;
        }

        private byte[] _key;

        public void PrepareAES_oldmtp(byte[] msgKey, out byte[] aesKey, out byte[] aesIV, bool send)
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

        public static AuthKey CreateLocalKey(byte[] password, byte[] salt)
        {
            byte[] key = new byte[Constants.NeededAuthKeySize];

            if (password == null || password.Length == 0)
            {
                KDF.PKCS5_PBKDF2_HMAC_SHA1(EmptyPassword, salt, Constants.LocalEncryptNoPwdIterCount, key);
            }
            else
            {
                KDF.PKCS5_PBKDF2_HMAC_SHA1(password, salt, Constants.LocalEncryptIterCount, key);
            }

            return new AuthKey(key);
        }
    }
}
