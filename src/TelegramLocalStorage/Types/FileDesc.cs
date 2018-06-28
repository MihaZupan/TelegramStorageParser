namespace MihaZupan.TelegramLocalStorage.Types
{
    public class FileDesc
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
