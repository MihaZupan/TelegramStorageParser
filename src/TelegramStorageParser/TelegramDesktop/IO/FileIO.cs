using System;
using System.IO;
using MihaZupan.TelegramStorageParser.OpenSSL;
using MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop.IO
{
    internal class FileIO
    {
        private static readonly byte[] TDFMagic = { (byte)'T', (byte)'D', (byte)'F', (byte)'$' };

        public const string DataNameFilePart = "D877F783D5D3EF8C";
        private const string UserPath = DataNameFilePart + "/";
        private readonly FileProvider FileProvider;

        public FileIO(FileProvider fileProvider)
        {
            FileProvider = fileProvider;
        }

        public bool FileExists(string name, FilePath options, out string filePath)
        {
            string path = options == FilePath.Base ? name : UserPath + name;
            filePath = path + "0";
            if (FileProvider.FileExists(filePath)) return true;
            else
            {
                filePath = path + "1";
                if (FileProvider.FileExists(filePath)) return true;
                else
                {
                    filePath = null;
                    return false;
                }
            }
        }
        public bool FileExists(FileKey fileKey, FilePath options, out string filePath)
            => FileExists(fileKey.ToFilePart(), options, out filePath);
        public bool FileExists(FileDesc fileDesc, FilePath options, out string filePath)
            => FileExists(fileDesc.Key.ToFilePart(), options, out filePath);

        public static FileReadDescriptor ReadFile(byte[] fileBytes)
        {
            int dataLength = fileBytes.Length - 8 - 16;
            if (dataLength < 0)
                throw new Exception("Not enough data");

            if (!fileBytes.CompareBytes(TDFMagic, TDFMagic.Length))
                throw new Exception("Bad magic");

            int version = BitConverter.ToInt32(fileBytes, 4);

            byte[] data = new byte[dataLength];
            Array.Copy(fileBytes, 8, data, 0, dataLength);

            Md5 md5 = new Md5();
            md5.Update(data);
            md5.Update(BitConverter.GetBytes(dataLength));
            md5.Update(BitConverter.GetBytes(version));
            md5.Update(TDFMagic);
            if (!fileBytes.CompareBytes(md5.Finalize(), fileBytes.Length - 16, 0, 16))
                throw new Exception("Invalid md5 hash");

            return new FileReadDescriptor(new DataStream(data), version);
        }
        public FileReadDescriptor ReadFile(string name, FilePath options)
        {
            if (!FileExists(name, options, out string filePath))
                throw new FileNotFoundException(nameof(name));

            return ReadFile(FileProvider.ReadFile(filePath));
        }

        public FileReadDescriptor ReadEncryptedFile(string name, FilePath options, AuthKey key)
        {
            FileReadDescriptor file = ReadFile(name, options);
            if (!key.TryDecryptLocal(file.DataStream.ReadByteArray(), out byte[] decrypted))
                throw new Exception("Could not decrypt file");

            file.DataStream = new DataStream(decrypted);
            return file;
        }
        public FileReadDescriptor ReadEncryptedFile(FileKey fileKey, FilePath options, AuthKey key)
            => ReadEncryptedFile(fileKey.ToFilePart(), options, key);
        public FileReadDescriptor ReadEncryptedFile(FileDesc fileDesc, FilePath options, AuthKey key)
            => ReadEncryptedFile(fileDesc.Key.ToFilePart(), options, key);

        public static ParsingState TryReadFile(byte[] fileBytes, out FileReadDescriptor file)
        {
            file = null;

            int dataLength = fileBytes.Length - 8 - 16;
            if (dataLength < 0)
                return ParsingState.InvalidData;

            if (!fileBytes.CompareBytes(TDFMagic, TDFMagic.Length))
                return ParsingState.InvalidData;

            int version = BitConverter.ToInt32(fileBytes, 4);

            byte[] data = new byte[dataLength];
            Array.Copy(fileBytes, 8, data, 0, dataLength);

            Md5 md5 = new Md5();
            md5.Update(data);
            md5.Update(BitConverter.GetBytes(dataLength));
            md5.Update(BitConverter.GetBytes(version));
            md5.Update(TDFMagic);
            if (!fileBytes.CompareBytes(md5.Finalize(), fileBytes.Length - 16, 0, 16))
                return ParsingState.InvalidData;

            file = new FileReadDescriptor(new DataStream(data), version);
            return ParsingState.Success;
        }
        public ParsingState TryReadFile(string name, FilePath options, out FileReadDescriptor file)
        {
            file = null;

            if (!FileExists(name, options, out string filePath))
                return ParsingState.FileNotFound;

            return TryReadFile(FileProvider.ReadFile(filePath), out file);
        }

        public ParsingState TryReadEncryptedFile(string name, FilePath options, AuthKey key, out FileReadDescriptor file)
        {
            var parsingState = TryReadFile(name, options, out file);
            if (parsingState.NotSuccessful()) return parsingState;

            if (!key.TryDecryptLocal(file.DataStream.ReadByteArray(), out byte[] decrypted))
                return ParsingState.InvalidData;

            file.DataStream = new DataStream(decrypted);
            return ParsingState.Success;
        }
        public ParsingState TryReadEncryptedFile(FileKey fileKey, FilePath options, AuthKey key, out FileReadDescriptor descriptor)
            => TryReadEncryptedFile(fileKey.ToFilePart(), options, key, out descriptor);
        public ParsingState TryReadEncryptedFile(FileDesc fileDesc, FilePath options, AuthKey key, out FileReadDescriptor descriptor)
            => TryReadEncryptedFile(fileDesc.Key.ToFilePart(), options, key, out descriptor);
    }
}
