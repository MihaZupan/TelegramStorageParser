namespace MihaZupan.TelegramLocalStorage.Types
{
    public class FileKey
    {
        internal FileKey(ulong key)
        {
            Key = key;
        }

        public ulong Key;

        public override string ToString()
        {
            return Key.ToString();
        }

        public static implicit operator FileKey(ulong key)
            => new FileKey(key);
        public static implicit operator ulong(FileKey key)
                    => key.Key;
    }
}
