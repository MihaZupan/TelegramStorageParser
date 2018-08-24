namespace MihaZupan.TelegramStorageParser.TelegramDesktop.IO
{
    internal class FileReadDescriptor
    {
        public DataStream DataStream;
        public int Version;

        public FileReadDescriptor(DataStream stream, int version)
        {
            DataStream = stream;
            Version = version;
        }
    }
}
