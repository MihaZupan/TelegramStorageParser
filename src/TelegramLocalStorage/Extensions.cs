using System;
using System.Security.Cryptography;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    internal static class Extensions
    {
        public static int TransformBlock(this MD5 md5, byte[] bytes)
        {
            return md5.TransformBlock(bytes, 0, bytes.Length, bytes, 0);
        }
        public static byte[] TransformFinalBlock(this MD5 md5, byte[] bytes)
        {
            return md5.TransformFinalBlock(bytes, 0, bytes.Length);
        }
        public static byte[] Md5(this byte[] bytes)
        {
            using (MD5 md5 = MD5.Create())
            {
                return md5.ComputeHash(bytes);
            }
        }
        public static byte[] Sha1(this byte[] bytes)
        {
            using (SHA1 sha1 = SHA1.Create())
            {
                return sha1.ComputeHash(bytes);
            }
        }
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
        public static byte[] Reverse(this byte[] bytes)
        {
            Array.Reverse(bytes);
            return bytes;
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
        public static void Shuffle(this byte[][] array)
        {
            Random rng = new Random();
            int n = array.Length;
            while (n > 1)
            {
                int k = rng.Next(n--);
                byte[] temp = array[n];
                array[n] = array[k];
                array[k] = temp;
            }
        }
    }
}
