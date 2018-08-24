namespace MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes
{
    internal struct FileDesc
    {
        internal FileDesc(FileKey key, int size)
        {
            Key = key;
            Size = size;
        }

        public FileKey Key;
        public int Size;
    }
}
