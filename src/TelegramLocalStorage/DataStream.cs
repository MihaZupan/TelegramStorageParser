using System;
using System.Collections.Generic;
using System.Text;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    /// <summary>
    /// Helper class for reading data from a byte[] created by a QDataStream v5.1 from the QT framework
    /// </summary>
    internal class DataStream
    {
        private byte[] _data;
        private int _offset = 0;

        public DataStream(byte[] bytes)
        {
            _data = bytes;
        }

        public byte[] Data => _data;

        public bool AtEnd => _data.Length <= _offset;

        public byte[] ReadRawData(int length)
        {
            if (length <= 0) return new byte[0];
            byte[] data = new byte[length];
            Array.Copy(_data, _offset, data, 0, length);
            _offset += length;
            return data;
        }
        public byte[] ReadByteArray()
        {
            uint length = ReadUInt32();
            if (length <= 0) return new byte[0];
            byte[] bytes = new byte[length];
            Array.Copy(_data, _offset, bytes, 0, length);
            _offset += (int)length;
            return bytes;
        }
        public string ReadString()
        {
            int len = (int)ReadUInt32();
            if (len <= 0) return "";
            string str = Encoding.ASCII.GetString(_data, _offset, len);
            _offset += len;
            return str;
        }

        public ushort ReadUInt16()
        {
            ushort value = (ushort)(
                _data[_offset] * 256u +
                _data[_offset + 1]);
            _offset += 2;
            return value;
        }
        public uint ReadUInt32()
        {
            uint value =
                _data[_offset] * 16777216u +
                _data[_offset + 1] * 65536u +
                _data[_offset + 2] * 256u +
                _data[_offset + 3];
            _offset += 4;
            return value;
        }
        public int ReadInt32()
        {
            byte[] intBytes = new byte[4];
            Array.Copy(_data, _offset, intBytes, 0, 4);
            Array.Reverse(intBytes);
            _offset += 4;
            return BitConverter.ToInt32(intBytes, 0);
        }
        public ulong ReadUInt64()
        {
            ulong value =
                _data[_offset] * 72057594037927936u +
                _data[_offset + 1] * 281474976710656u +
                _data[_offset + 2] * 1099511627776u +
                _data[_offset + 3] * 4294967296u +
                _data[_offset + 4] * 16777216u +
                _data[_offset + 5] * 65536u +
                _data[_offset + 6] * 256u +
                _data[_offset + 7];
            _offset += 8;
            return value;
        }
        public long ReadInt64()
        {
            byte[] intBytes = new byte[8];
            Array.Copy(_data, _offset, intBytes, 0, 8);
            Array.Reverse(intBytes);
            _offset += 8;
            return BitConverter.ToInt64(intBytes, 0);
        }

        public List<PeerId> ReadPeerIds()
        {
            uint n = ReadUInt32();
            List<PeerId> ret = new List<PeerId>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(ReadUInt64());
            }
            return ret;
        }
        public List<Tuple<uint, ushort>> ReadRecentEmojiPreloadOldOld()
        {
            uint n = ReadUInt32();
            List<Tuple<uint, ushort>> ret = new List<Tuple<uint, ushort>>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(new Tuple<uint, ushort>(ReadUInt32(), ReadUInt16()));
            }
            return ret;
        }
        public List<Tuple<ulong, ushort>> ReadRecentEmojiPreloadOld()
        {
            uint n = ReadUInt32();
            List<Tuple<ulong, ushort>> ret = new List<Tuple<ulong, ushort>>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(new Tuple<ulong, ushort>(ReadUInt64(), ReadUInt16()));
            }
            return ret;
        }
        public List<Tuple<string, ushort>> ReadRecentEmojiPreload()
        {
            uint n = ReadUInt32();
            List<Tuple<string, ushort>> ret = new List<Tuple<string, ushort>>((int)n);
            for (int i = 0; i < n; i++)
            {
                ret.Add(new Tuple<string, ushort>(ReadString(), ReadUInt16()));
            }
            return ret;
        }
        public List<Tuple<ulong, ushort>> ReadRecentStickerPreload()
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
}
