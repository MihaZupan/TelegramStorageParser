using System;
using MihaZupan.TelegramLocalStorage.OpenSSL;

namespace MihaZupan.TelegramLocalStorage
{
    class MapPasscodeBruteForce
    {
        private readonly byte[] Salt;
        private readonly byte[] MessageKey;
        private readonly byte[] EncryptedData;
        private readonly int EncryptedDataLength;

        private byte[][] AuthKeyBuffers;
        private byte[][] AesKeyBuffers;
        private byte[][] AesIVBuffers;
        private byte[][] DecKeyBuffers;
        private byte[][] DecryptedBuffers;

        private byte[][] DataA_Buffers;
        private byte[][] DataB_Buffers;
        private byte[][] DataC_Buffers;
        private byte[][] DataD_Buffers;

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

            AuthKeyBuffers      = new byte[maxThreads][];
            AesKeyBuffers       = new byte[maxThreads][];
            AesIVBuffers        = new byte[maxThreads][];
            DecKeyBuffers       = new byte[maxThreads][];
            DecryptedBuffers    = new byte[maxThreads][];
            DataA_Buffers       = new byte[maxThreads][];
            DataB_Buffers       = new byte[maxThreads][];
            DataC_Buffers       = new byte[maxThreads][];
            DataD_Buffers       = new byte[maxThreads][];

            for (int i = 0; i < maxThreads; i++)
            {
                AuthKeyBuffers[i]   = new byte[Constants.AuthKeySize];
                AesKeyBuffers[i]    = new byte[32];
                AesIVBuffers[i]     = new byte[32];
                DecKeyBuffers[i]    = new byte[AesCore.AesKeyStructSize];
                DecryptedBuffers[i] = new byte[EncryptedDataLength];
                DataA_Buffers[i]    = new byte[48];
                DataB_Buffers[i]    = new byte[48];
                DataC_Buffers[i]    = new byte[48];
                DataD_Buffers[i]    = new byte[48];
                Array.Copy(MessageKey, 0, DataA_Buffers[i], 0, 16);
                Array.Copy(MessageKey, 0, DataB_Buffers[i], 16, 16);
                Array.Copy(MessageKey, 0, DataC_Buffers[i], 32, 16);
                Array.Copy(MessageKey, 0, DataD_Buffers[i], 0, 16);
            }
        }

        public bool Try(byte[] passcodeUtf8, int thread)
        {
            byte[] authKey = AuthKeyBuffers[thread];
            KDF.PKCS5_PBKDF2_HMAC_SHA1(passcodeUtf8, passcodeUtf8.Length, Salt, 32, Constants.LocalEncryptIterCount, Constants.AuthKeySize, authKey);

            byte[] aesKey = AesKeyBuffers[thread];
            byte[] aesIV = AesIVBuffers[thread];
            byte[] data_a = DataA_Buffers[thread];
            byte[] data_b = DataB_Buffers[thread];
            byte[] data_c = DataC_Buffers[thread];
            byte[] data_d = DataD_Buffers[thread];

            Array.Copy(authKey, 8, data_a, 16, 32);
            Array.Copy(authKey, 40, data_b, 0, 16);
            Array.Copy(authKey, 56, data_b, 32, 16);
            Array.Copy(authKey, 72, data_c, 0, 32);
            Array.Copy(authKey, 104, data_d, 16, 32);

            byte[] sha1_a = data_a.Sha1();
            byte[] sha1_b = data_b.Sha1();
            byte[] sha1_c = data_c.Sha1();
            byte[] sha1_d = data_d.Sha1();

            Array.Copy(sha1_a, 0, aesKey, 0, 8);
            Array.Copy(sha1_b, 8, aesKey, 8, 12);
            Array.Copy(sha1_c, 4, aesKey, 20, 12);
            Array.Copy(sha1_a, 8, aesIV, 0, 12);
            Array.Copy(sha1_b, 0, aesIV, 12, 8);
            Array.Copy(sha1_c, 16, aesIV, 20, 4);
            Array.Copy(sha1_d, 0, aesIV, 24, 8);

            byte[] decKey = DecKeyBuffers[thread];
            byte[] decrypted = DecryptedBuffers[thread];
            AesCore.AES_set_decrypt_key(aesKey, 256, decKey);
            AesIGE.AES_ige_encrypt(EncryptedData, decrypted, EncryptedDataLength, decKey, aesIV, AesEncrypt.Decrypt);

            byte[] sha1 = decrypted.Sha1();
            for (int i = 0; i < 16; i++)
            {
                if (sha1[i] != MessageKey[i]) return false;
            }
            return true;
        }
    }
}
