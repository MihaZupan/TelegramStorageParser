using System;
using MihaZupan.TelegramLocalStorage.OpenSSL;
using MihaZupan.TelegramLocalStorage.TgCrypto;

namespace MihaZupan.TelegramLocalStorage
{
    public class MapPasscodeBruteForce
    {
        private class SessionBuffers
        {
            public SessionBuffers(int encryptedDataLength, byte[] messageKey)
            {
                Decrypted = new byte[encryptedDataLength];
                Array.Copy(messageKey, 0, Data_A, 0, 16);
                Array.Copy(messageKey, 0, Data_B, 16, 16);
                Array.Copy(messageKey, 0, Data_C, 32, 16);
                Array.Copy(messageKey, 0, Data_D, 0, 16);
            }

            public byte[] AuthKey = new byte[Constants.NeededAuthKeySize];            
            public byte[] AesKey = new byte[32];
            public byte[] AesIV = new byte[32];
            public byte[] DecKey = new byte[AesCore.AesKeyStructSize];
            public byte[] Decrypted;

            public byte[] Data_A = new byte[48];
            public byte[] Data_B = new byte[48];
            public byte[] Data_C = new byte[48];
            public byte[] Data_D = new byte[48];

            public byte[] Sha1_A = new byte[20];
            public byte[] Sha1_B = new byte[20];
            public byte[] Sha1_C = new byte[20];
            public byte[] Sha1_D = new byte[20];

            public byte[] FinalSha1 = new byte[20];
        }

        private readonly byte[] Salt;
        private readonly byte[] MessageKey;
        private readonly byte[] EncryptedData;
        private readonly int EncryptedDataLength;
        private readonly SessionBuffers[] Sessions;

        // Prepare as many things as we can ahead-of-time once
        // Try to reuse byte arrays where possible
        public MapPasscodeBruteForce(string tDataPath, int maxThreads)
            : this(new FileIO(tDataPath).ReadFile("map", FilePath.User).DataStream, maxThreads) { }
        public MapPasscodeBruteForce(byte[] mapFileBytes, int maxThreads)
            : this(FileIO.ReadMemoryFile(mapFileBytes).DataStream, maxThreads) { }
        private MapPasscodeBruteForce(DataStream mapStream, int maxThreads)
        {
            Salt = mapStream.ReadByteArray();
            byte[] keyEncryptedData = mapStream.ReadByteArray();

            EncryptedDataLength = keyEncryptedData.Length - 16;
            EncryptedData = new byte[EncryptedDataLength];
            MessageKey = new byte[16];
            Array.Copy(keyEncryptedData, 16, EncryptedData, 0, EncryptedDataLength);
            Array.Copy(keyEncryptedData, 0, MessageKey, 0, 16);

            Sessions = new SessionBuffers[maxThreads];
            for (int i = 0; i < maxThreads; i++)
                Sessions[i] = new SessionBuffers(EncryptedDataLength, MessageKey);
        }

        public bool Try(byte[] passcodeUtf8, int thread)
        {
            SessionBuffers session = Sessions[thread];

            KDF.PKCS5_PBKDF2_HMAC_SHA1(passcodeUtf8, Salt, Constants.LocalEncryptIterCount, session.AuthKey);

            Array.Copy(session.AuthKey, 8, session.Data_A, 16, 32);
            Array.Copy(session.AuthKey, 40, session.Data_B, 0, 16);
            Array.Copy(session.AuthKey, 56, session.Data_B, 32, 16);
            Array.Copy(session.AuthKey, 72, session.Data_C, 0, 32);
            Array.Copy(session.AuthKey, 104, session.Data_D, 16, 32);

            SHA.SHA1(session.Data_A, 48, session.Sha1_A);
            SHA.SHA1(session.Data_B, 48, session.Sha1_B);
            SHA.SHA1(session.Data_C, 48, session.Sha1_C);
            SHA.SHA1(session.Data_D, 48, session.Sha1_D);

            Array.Copy(session.Sha1_A, 0, session.AesKey, 0, 8);
            Array.Copy(session.Sha1_B, 8, session.AesKey, 8, 12);
            Array.Copy(session.Sha1_C, 4, session.AesKey, 20, 12);
            Array.Copy(session.Sha1_A, 8, session.AesIV, 0, 12);
            Array.Copy(session.Sha1_B, 0, session.AesIV, 12, 8);
            Array.Copy(session.Sha1_C, 16, session.AesIV, 20, 4);
            Array.Copy(session.Sha1_D, 0, session.AesIV, 24, 8);
            
            AesCore.AES_set_decrypt_key(session.AesKey, 256, session.DecKey);
            AesIGE.AES_ige_encrypt(EncryptedData, session.Decrypted, EncryptedDataLength, session.DecKey, session.AesIV, AesEncrypt.Decrypt);

            SHA.SHA1(session.Decrypted, EncryptedDataLength, session.FinalSha1);
            for (int i = 0; i < 16; i++)
            {
                if (session.FinalSha1[i] != MessageKey[i]) return false;
            }
            return true;
        }

        private static readonly Random _random = new Random();
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
    }
}
