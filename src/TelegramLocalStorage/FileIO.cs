using System;
using System.IO;
using MihaZupan.TelegramLocalStorage.OpenSSL;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using static MihaZupan.TelegramLocalStorage.Extensions;

namespace MihaZupan.TelegramLocalStorage
{
    enum FileOptions
    {
        User,
        Safe // base path
    }

    sealed class FileIO
    {
        public static DataStream ReadFile(string name, FileOptions options)
        {
            string path = (options == FileOptions.User ? Constants.UserPath : Constants.BasePath) + name;
            if (File.Exists(path + "0")) path = path + "0";
            else path = path + "1";

            byte[] bytes = File.ReadAllBytes(path);
            int index = 0;

            if (!CompareBytes(bytes, Constants.TDFMagic, 0, 0, Constants.TDFMagic.Length))
                throw new Exception("bad magic");
            index += Constants.TDFMagic.Length;
            
            int version = BitConverter.ToInt32(bytes, index);
            index += sizeof(int);

            byte[] data = new byte[bytes.Length - index - 16];
            Array.Copy(bytes, index, data, 0, data.Length);

            Md5 md5 = new Md5();
            md5.Update(data);
            md5.Update(BitConverter.GetBytes(data.Length));
            md5.Update(BitConverter.GetBytes(version));
            md5.Update(Constants.TDFMagic);
            if (!CompareBytes(bytes, md5.Finalize(), bytes.Length - 16, 0, 16))
                throw new Exception("invalid md5 hash");

            return new DataStream(data);
        }
        public static DataStream ReadEncryptedFile(string name, FileOptions options, AuthKey key)
        {
            DataStream encrypted = ReadFile(name, options);
            if (Decrypt.TryDecryptLocal(encrypted.ReadByteArray(), key, out byte[] decrypted))
            {
                return new DataStream(decrypted);
            }
            else throw new Exception("Could not decrypt file");
        }
    }
}
