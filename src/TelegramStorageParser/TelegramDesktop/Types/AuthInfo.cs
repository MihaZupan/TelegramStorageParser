using System.Collections.Generic;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop.Types
{
    public class AuthInfo
    {
        internal AuthInfo()
        { }

        public int? UserId { get; internal set; }
        public int? MainDcId { get; internal set; }
        public byte[] MtpKey { get; internal set; }
        public readonly List<DataCenter> DataCenters = new List<DataCenter>();
    }
}
