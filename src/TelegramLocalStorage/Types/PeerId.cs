namespace MihaZupan.TelegramLocalStorage.Types
{
    public class PeerId
    {
        internal PeerId(ulong id)
        {
            Id = id;
        }

        public ulong Id;

        public override string ToString()
        {
            return Id.ToString();
        }

        public static implicit operator PeerId(ulong id)
            => new PeerId(id);
        public static implicit operator ulong(PeerId id)
                    => id.Id;
    }
}
