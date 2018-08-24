namespace MihaZupan.TelegramStorageParser.TelegramDesktop.Types
{
    public class WindowPosition
    {
        internal WindowPosition(int x, int y, int w, int h, int moncrc, int maximized)
        {
            X = x;
            Y = y;
            Width = w;
            Height = h;
            MonitorCRC = moncrc;
            Maximized = maximized;
        }

        public readonly int X;
        public readonly int Y;
        public readonly int Width;
        public readonly int Height;
        public readonly int MonitorCRC;
        public readonly int Maximized;
    }
}
