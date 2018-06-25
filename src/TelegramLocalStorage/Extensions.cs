using System;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    internal static class Extensions
    {
        public static bool IsSameAs(this byte[] a, byte[] b)
        {
            if (a.Length != b.Length) return false;
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }
        public static byte[] Copy(this byte[] bytes)
        {
            byte[] copy = new byte[bytes.Length];
            Array.Copy(bytes, 0, copy, 0, copy.Length);
            return copy;
        }

        public static bool CompareBytes(byte[] a, byte[] b, int indexA, int indexB, int len)
        {
            for (int i = 0; i < len; i++)
            {
                if (a[indexA + i] != b[indexB + i]) return false;
            }
            return true;
        }
        public static string ToFilePart(this FileKey val)
        {
            char[] result = new char[16];
            for (int i = 0; i < 16; i++)
            {
                byte v = (byte)(val & 0x0F);
                result[i] = (char)((v < 0x0A) ? ('0' + v) : ('A' + (v - 0x0A)));
                val >>= 4;
            }
            return new string(result);
        }
    }
}
