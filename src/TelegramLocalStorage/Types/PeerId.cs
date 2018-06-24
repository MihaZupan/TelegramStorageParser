using Newtonsoft.Json;

namespace MihaZupan.TelegramLocalStorage.Types
{
    [JsonConverter(typeof(PeerIdConverter))]
    public class PeerId
    {
        public PeerId(ulong id)
        {
            Id = id;
        }

        public ulong Id;

        public static implicit operator PeerId(ulong id)
            => new PeerId(id);
        public static implicit operator ulong(PeerId id)
                    => id.Id;
    }
}
