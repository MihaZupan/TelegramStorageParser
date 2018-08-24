namespace MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes
{
    internal struct FileKey
    {
        internal FileKey(ulong key)
        {
            Key = key;
        }

        public ulong Key;

        public string ToFilePart()
        {
            // Esentially Key => Uppercase HEX, with every two characters swapped.
            // Example:
            // Upper HEX: 4D8201685C193545
            // File part: D4281086C5915349

            char[] result = new char[16];
            for (int i = 0; i < 16; i++)
            {
                byte v = (byte)(Key & 0x0F);
                result[i] = (char)((v < 0x0A) ? ('0' + v) : ('A' + (v - 0x0A)));
                Key >>= 4;
            }
            return new string(result);
        }

        public static implicit operator FileKey(ulong key)
            => new FileKey(key);
        public static implicit operator ulong(FileKey key)
            => key.Key;
    }
}
