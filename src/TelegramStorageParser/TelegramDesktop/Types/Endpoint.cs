namespace MihaZupan.TelegramStorageParser.TelegramDesktop.Types
{
    public class Endpoint
    {
        internal Endpoint(string address, int port)
        {
            Address = address;
            Port = port;
        }

        public readonly string Address;
        public readonly int Port;
    }
}
