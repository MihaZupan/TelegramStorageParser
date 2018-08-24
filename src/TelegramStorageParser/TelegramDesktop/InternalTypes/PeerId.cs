namespace MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes
{
    internal struct PeerId
    {
        internal PeerId(ulong id)
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
