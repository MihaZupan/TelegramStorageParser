namespace MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes
{
    internal struct StorageKey
    {
        internal StorageKey(ulong first, ulong second)
        {
            First = first;
            Second = second;
        }

        public ulong First;
        public ulong Second;
    }
}
