using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    /// <summary>
    /// Helper class for reading data from a byte[] created by a QDataStream v5.1 from the QT framework
    /// </summary>
    internal class DataStream
    {
        public DataStream(byte[] bytes)
        {
            Data = bytes;
        }

        public byte[] Data { get; }
        public int Position { get; private set; } = 0;

        public bool AtEnd => Data.Length <= Position;

        private void EnsureSpace(int length)
        {
            if (Position + length > Data.Length)
                throw new ArgumentException("No more data");
        }

        public void SeekForward(int count)
        {
            EnsureSpace(count);
            Position += count;
        }
        
        public byte[] ReadRawData(int length)
        {
            EnsureSpace(length);
            if (length <= 0) return new byte[0];
            byte[] data = new byte[length];
            Array.Copy(Data, Position, data, 0, length);
            Position += length;
            return data;
        }
        public byte[] ReadShortRawDataReversed(int length)
        {
            EnsureSpace(length);
            if (length <= 0) return new byte[0];
            byte[] data = new byte[length];
            for (int i = 0; i < length; i++)
            {
                data[i] = Data[Position + length - i - 1];
            }
            Position += length;
            return data;
        }
        public byte[] ReadByteArray()
        {
            uint length = ReadUInt32();
            if (length <= 0) return new byte[0];
            byte[] bytes = ReadRawData((int)length);
            return bytes;
        }
        public string ReadString()
        {
            int len = (int)ReadUInt32();
            if (len <= 0) return "";
            string str = Encoding.ASCII.GetString(Data, Position, len);
            Position += len;
            return str;
        }

        public ushort ReadUInt16()
        {
            return BitConverter.ToUInt16(ReadShortRawDataReversed(2), 0);
        }
        public uint ReadUInt32()
        {
            return BitConverter.ToUInt32(ReadShortRawDataReversed(4), 0);
        }
        public int ReadInt32()
        {
            return BitConverter.ToInt32(ReadShortRawDataReversed(4), 0);
        }
        public ulong ReadUInt64()
        {
            return BitConverter.ToUInt64(ReadShortRawDataReversed(8), 0);
        }
        public long ReadInt64()
        {
            return BitConverter.ToInt64(ReadShortRawDataReversed(8), 0);
        }

        public PeerId[] ReadPeerIds()
        {
            uint n = ReadUInt32();
            PeerId[] ret = new PeerId[(int)n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = ReadUInt64();
            }
            return ret;
        }
        public Tuple<uint, ushort>[] ReadRecentEmojiPreloadOldOld()
        {
            uint n = ReadUInt32();
            Tuple<uint, ushort>[] ret = new Tuple<uint, ushort>[(int)n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = new Tuple<uint, ushort>(ReadUInt32(), ReadUInt16());
            }
            return ret;
        }
        public Tuple<ulong, ushort>[] ReadRecentEmojiPreloadOld()
        {
            uint n = ReadUInt32();
            Tuple<ulong, ushort>[] ret = new Tuple<ulong, ushort>[(int)n];
            for (int i = 0; i < n; i++)
            {
                ret[i]= new Tuple<ulong, ushort>(ReadUInt64(), ReadUInt16());
            }
            return ret;
        }
        public Tuple<string, ushort>[] ReadRecentEmojiPreload()
        {
            uint n = ReadUInt32();
            Tuple<string, ushort>[] ret = new Tuple<string, ushort>[(int)n];
            for (int i = 0; i < n; i++)
            {
                ret[i] = new Tuple<string, ushort>(ReadString(), ReadUInt16());
            }
            return ret;
        }
        public Tuple<ulong, ushort>[] ReadRecentStickerPreload()
        {
            // Happens to be the same type
            return ReadRecentEmojiPreloadOld();
        }
        public Dictionary<uint, ulong> ReadEmojiColorVariantsOld()
        {
            uint n = ReadUInt32();
            Dictionary<uint, ulong> ret = new Dictionary<uint, ulong>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(ReadUInt32(), ReadUInt64());
            }
            return ret;
        }
        public Dictionary<string, int> ReadEmojiColorVariants()
        {
            uint n = ReadUInt32();
            Dictionary<string, int> ret = new Dictionary<string, int>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(ReadString(), ReadInt32());
            }
            return ret;
        }
        public Dictionary<ulong, int> ReadHiddenPinnedMessagesMap()
        {
            uint n = ReadUInt32();
            Dictionary<ulong, int> ret = new Dictionary<ulong, int>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(ReadUInt64(), ReadInt32());
            }
            return ret;
        }
        public Dictionary<PeerId, FileKey> ReadDraftsMap()
        {
            uint n = ReadUInt32();
            Dictionary<PeerId, FileKey> ret = new Dictionary<PeerId, FileKey>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(ReadUInt64(), ReadUInt64());
            }
            return ret;
        }
        public Dictionary<StorageKey, FileDesc> ReadStorageMap(out long totalSize)
        {
            uint n = ReadUInt32();
            Dictionary<StorageKey, FileDesc> ret = new Dictionary<StorageKey, FileDesc>((int)n);
            totalSize = 0;
            for (int i = 0; i < n; i++)
            {
                ulong key = ReadUInt64();
                StorageKey storageKey = new StorageKey(ReadUInt64(), ReadUInt64());
                int size = ReadInt32();
                ret.Add(storageKey, new FileDesc(key, size));
                totalSize += size;
            }
            return ret;
        }
    }
    internal class DataStreamWriter
    {
        private MemoryStream _stream;

        public byte[] Data => _stream.ToArray();

        public DataStreamWriter()
        {
             _stream = new MemoryStream();
        }
        public DataStreamWriter(int capacity)
        {
            _stream = new MemoryStream(capacity);
        }

        public void Write(byte[] data)
        {
            Write((uint)data.Length);
            _stream.Write(data, 0, data.Length);
        }
        public void Write(uint value)
        {
            _stream.WriteByte((byte)(value >> 24));
            _stream.WriteByte((byte)(value >> 16));
            _stream.WriteByte((byte)(value >> 8));
            _stream.WriteByte((byte)(value));
        }
    }
}
