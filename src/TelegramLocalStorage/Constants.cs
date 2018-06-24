using System;
using System.Text;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    class Constants
    {
        public const int AuthKeySize = 256;
        public const int LocalEncryptIterCount = 4000;
        public const int LocalEncryptNoPwdIterCount = 4;
        public const int LocalEncryptSaltSize = 32;

        public const string BasePath = "tdata/";
        public static readonly FileKey DataNameKey = BitConverter.ToUInt64(Encoding.UTF8.GetBytes("data").Md5(), 0);
        public static readonly string UserPath = BasePath + DataNameKey.ToFilePart() + "/";
        public static readonly byte[] TDFMagic = { (byte)'T', (byte)'D', (byte)'F', (byte)'$' };
    }
}
