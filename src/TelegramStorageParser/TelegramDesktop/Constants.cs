namespace MihaZupan.TelegramStorageParser.TelegramDesktop
{
    internal static class Constants
    {
        public const int LocalKeyNoPwdIterCount = 4;
        public const int LocalKeyIterCount = 4000;

        public const int AuthKeySize = 256;
        /// <summary>
        /// If local key ends up being used somewhere else, this should be changed to AuthKeySize
        /// </summary>
        public const int NeededAuthKeySize = 136;

        public const int LocalEncryptSaltSize = 32;
    }
}
