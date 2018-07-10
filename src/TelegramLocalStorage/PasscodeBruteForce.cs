using System;
using System.Text;
using MihaZupan.TelegramLocalStorage.OpenSSL;
using MihaZupan.TelegramLocalStorage.TgCrypto;

namespace MihaZupan.TelegramLocalStorage
{
    public static class PasscodeBruteForce
    {
        public static string GenerateJohnTheRipperHashString(string tDataPath)
        {
            return GenerateJohnTheRipperHashString(new FileIO(tDataPath).ReadFile("map", FilePath.User).DataStream);
        }
        public static string GenerateJohnTheRipperHashString(byte[] mapBytes)
        {
            return GenerateJohnTheRipperHashString(FileIO.ReadMemoryFile(mapBytes).DataStream);
        }
        private static string GenerateJohnTheRipperHashString(DataStream mapStream)
        {
            byte[] salt = mapStream.ReadByteArray();
            byte[] encryptedKey = mapStream.ReadByteArray();

            StringBuilder sb = new StringBuilder(658);
            sb.Append("$telegram$1");
            sb.Append('*');
            sb.Append(Constants.LocalEncryptIterCount.ToString());
            sb.Append('*');
            sb.Append(ToLowercaseHex(salt));
            sb.Append('*');
            sb.Append(ToLowercaseHex(encryptedKey));
            return sb.ToString();
        }

        private static char[] ToLowercaseHex(byte[] bytes)
        {
            char[] hex = new char[bytes.Length * 2];
            string alphabet = "0123456789abcdef";
            for (int i = 0; i < bytes.Length; i++)
            {
                hex[i * 2] = alphabet[(bytes[i] >> 4)];
                hex[i * 2 + 1] = alphabet[(bytes[i] & 0xF)];
            }
            return hex;
        }

        private static Random _random = new Random();
        public static byte[] GenerateTestMapData(byte[] passcode)
        {
            byte[] localKeyData = new byte[256];
            _random.NextBytes(localKeyData);
            byte[] decryptedData = new byte[272];
            byte[] localKeyDataLengthBytes = BitConverter.GetBytes(256u);
            Array.Copy(localKeyDataLengthBytes, 0, decryptedData, 0, 4);
            Array.Copy(localKeyData, 0, decryptedData, 4, 256);

            byte[] decryptedDataSha1 = new byte[20];
            SHA.SHA1(decryptedData, 272, decryptedDataSha1);
            byte[] messageKey = new byte[16];
            Array.Copy(decryptedDataSha1, 0, messageKey, 0, 16);

            byte[] salt = new byte[32];
            _random.NextBytes(salt);

            byte[] authKeyBytes = new byte[256];
            KDF.PKCS5_PBKDF2_HMAC_SHA1(passcode, salt, Constants.LocalEncryptIterCount, authKeyBytes);
            AuthKey authKey = new AuthKey(authKeyBytes);
            authKey.PrepareAES_oldmtp(messageKey, out byte[] aesKey, out byte[] aesIV, false);
            
            byte[] encKey = new byte[AesCore.AesKeyStructSize];
            AesCore.AES_set_encrypt_key(aesKey, 256, encKey);

            byte[] encrypted = new byte[272];
            AesIGE.AES_ige_encrypt(decryptedData, encrypted, 272, encKey, aesIV, AesEncrypt.Encrypt);

            byte[] encryptedData = new byte[288];
            Array.Copy(messageKey, 0, encryptedData, 0, 16);
            Array.Copy(encrypted, 0, encryptedData, 16, 272);

            DataStreamWriter stream = new DataStreamWriter(328);
            stream.Write(salt);
            stream.Write(encryptedData);

            byte[] streamBytes = stream.Data;
            byte[] versionBytes = BitConverter.GetBytes(1003009);
            Md5 md5 = new Md5();
            md5.Update(streamBytes);
            md5.Update(BitConverter.GetBytes(328));
            md5.Update(versionBytes);
            md5.Update(Constants.TDFMagic);
            byte[] dataMd5 = md5.Finalize();

            byte[] mapBytes = new byte[352];
            Array.Copy(Constants.TDFMagic, 0, mapBytes, 0, 4);
            Array.Copy(versionBytes, 0, mapBytes, 4, 4);
            Array.Copy(streamBytes, 0, mapBytes, 8, 328);
            Array.Copy(dataMd5, 0, mapBytes, 336, 16);

            return mapBytes;
        }

        public static string GenerateTestJohnTheRipperHashString(string passcode)
        {
            return GenerateJohnTheRipperHashString(GenerateTestMapData(Encoding.UTF8.GetBytes(passcode)));
        }
    }
}
