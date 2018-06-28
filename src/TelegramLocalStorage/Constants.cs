namespace MihaZupan.TelegramLocalStorage
{
    internal class Constants
    {
        public const int AuthKeySize = 256;
        public const int NeededAuthKeySize = 136;
        public const int LocalEncryptIterCount = 4000;
        public const int LocalEncryptNoPwdIterCount = 4;
        public const int LocalEncryptSaltSize = 32;

        public static readonly byte[] TDFMagic = { (byte)'T', (byte)'D', (byte)'F', (byte)'$' };
    }
}
