namespace MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes
{
    internal struct MsgId
    {
        internal MsgId(int id)
        {
            Id = id;
        }

        public int Id;

        public static implicit operator MsgId(int id)
            => new MsgId(id);
        public static implicit operator int(MsgId id)
            => id.Id;
    }
}
