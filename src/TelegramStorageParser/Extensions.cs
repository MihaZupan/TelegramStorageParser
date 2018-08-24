using System;

namespace MihaZupan.TelegramStorageParser
{
    internal static class Extensions
    {
        public static byte[] Copy(this byte[] bytes)
        {
            byte[] copy = new byte[bytes.Length];
            Array.Copy(bytes, 0, copy, 0, copy.Length);
            return copy;
        }

        public static bool CompareBytes(this byte[] a, byte[] b, int indexA, int indexB, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (a[indexA++] != b[indexB++]) return false;
            }
            return true;
        }
        public static bool CompareBytes(this byte[] a, byte[] b, int len)
            => CompareBytes(a, b, 0, 0, len);

        public static bool NotSuccessful(this ParsingState state)
            => state != ParsingState.Success;
        public static bool IsSuccessful(this ParsingState state)
            => state == ParsingState.Success;

        public static float Snap(this float x, float min, float max)
        {
            if (x < min) return min;
            if (x > max) return max;
            return x;
        }
    }
}
