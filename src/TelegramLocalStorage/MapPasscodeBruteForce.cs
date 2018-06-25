using System;
using MihaZupan.TelegramLocalStorage.OpenSSL;

namespace MihaZupan.TelegramLocalStorage
{
    class MapPasscodeBruteForce
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

            public byte[] AuthKey = new byte[Constants.AuthKeySize];
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

        private SessionBuffers[] Sessions;

        // Prepare as many things as we can ahead-of-time once
        // Try to reuse byte arrays where possible
        public MapPasscodeBruteForce(int maxThreads)
        {
            DataStream stream = FileIO.ReadFile("map", FileOptions.User);
            Salt = stream.ReadByteArray();
            byte[] keyEncryptedData = stream.ReadByteArray();

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
    }
}
