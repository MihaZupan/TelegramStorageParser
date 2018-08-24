using System.Collections.Generic;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop.Types
{
    public class ProxyInfo
    {
        internal ProxyInfo()
        { }

        public bool UseProxyForCalls { get; internal set; } = false;
        public bool UsesProxy => UsedProxy != null;
        public ProxyData UsedProxy { get; internal set; }
        public readonly List<ProxyData> ProxiesList = new List<ProxyData>();
    }
}
