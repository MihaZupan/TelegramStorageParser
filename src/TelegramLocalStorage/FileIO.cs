using System;
using System.IO;
using MihaZupan.TelegramLocalStorage.OpenSSL;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using MihaZupan.TelegramLocalStorage.Types;
using static MihaZupan.TelegramLocalStorage.Extensions;

namespace MihaZupan.TelegramLocalStorage
{
    internal enum FilePath
    {
        User,
        Base // base path
    }

    internal class FileReadDescriptor
    {
        public DataStream DataStream;
        public int Version;

        public FileReadDescriptor(DataStream stream, int version)
        {
            DataStream = stream;
            Version = version;
        }
    }

    internal class FileIO
    {
        private string BasePath;
        private string UserPath;

        public FileIO(string tDataPath)
        {
            BasePath = tDataPath[tDataPath.Length - 1] == '/' ? tDataPath : tDataPath + "/";
            UserPath = BasePath + "D877F783D5D3EF8C/";
        }

        public bool FileExists(string name, FilePath options)
        {
            string path = (options == FilePath.User ? UserPath : BasePath) + name;
            if (File.Exists(path + "0")) return true;
            else return File.Exists(path + "1");
        }
        public bool FileExists(FileKey fileKey, FilePath options)
        {
            return FileExists(fileKey.ToFilePart(), options);
        }
        public bool FileExists(FileDesc fileDesc, FilePath options)
        {
            return FileExists(fileDesc.Key.ToFilePart(), options);
        }

        public string GetFilePath(string name, FilePath options)
        {
            string path = (options == FilePath.User ? UserPath : BasePath) + name;
            if (File.Exists(path + "0")) path = path + "0";
            else path = path + "1";
            return path;
        }

        public static FileReadDescriptor ReadMemoryFile(byte[] bytes)
        {
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

            return new FileReadDescriptor(new DataStream(data), version);
        }
        public static FileReadDescriptor ReadFile(string filePath)
        {
            byte[] bytes = File.ReadAllBytes(filePath);
            return ReadMemoryFile(bytes);
        }
        public FileReadDescriptor ReadFile(string name, FilePath options)
        {
            string path = GetFilePath(name, options);
            return ReadFile(path);
        }
        
        public FileReadDescriptor ReadEncryptedFile(string name, FilePath options, AuthKey key)
        {
            FileReadDescriptor file = ReadFile(name, options);
            if (Decrypt.TryDecryptLocal(file.DataStream.ReadByteArray(), key, out byte[] decrypted))
            {
                file.DataStream = new DataStream(decrypted);
                return file;
            }
            else throw new Exception("Could not decrypt file");
        }
        public FileReadDescriptor ReadEncryptedFile(FileKey fileKey, FilePath options, AuthKey key)
        {
            return ReadEncryptedFile(fileKey.ToFilePart(), options, key);
        }
        public FileReadDescriptor ReadEncryptedFile(FileDesc fileDesc, FilePath options, AuthKey key)
        {
            return ReadEncryptedFile(fileDesc.Key.ToFilePart(), options, key);
        }
    }
}
