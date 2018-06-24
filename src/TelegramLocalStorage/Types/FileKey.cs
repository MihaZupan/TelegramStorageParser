using Newtonsoft.Json;

namespace MihaZupan.TelegramLocalStorage.Types
{
    [JsonConverter(typeof(FileKeyConverter))]
    public class FileKey
    {
        public FileKey(ulong key)
        {
            Key = key;
        }

        public ulong Key;

        public static implicit operator FileKey(ulong key)
            => new FileKey(key);
        public static implicit operator ulong(FileKey key)
                    => key.Key;
    }
}
