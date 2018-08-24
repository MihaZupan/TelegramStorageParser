using MihaZupan.TelegramStorageParser.TelegramDesktop.Types.Enums;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop.Types
{
    public class ProxyData
    {
        internal ProxyData(ProxyType type, string host, int port, string user, string password)
        {
            Type = (ProxyType)((int)type % 1024);
            Host = host;
            Port = port;
            User = user;
            Password = password;
        }

        public readonly ProxyType Type;
        public readonly string Host;
        public readonly int Port;
        public readonly string User;
        public readonly string Password;
    }
}
