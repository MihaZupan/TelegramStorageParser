using System;
using System.Collections.Generic;
using System.Text;
using MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types.Enums;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop.IO
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

        public int DataLeft => AtEnd ? 0 : Data.Length - Position;
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
        public void SeekForwardCollection(int elementSize)
        {
            int n = (int)ReadUInt32();
            SeekForward(n * elementSize);
        }
        public void SeekForwardString()
        {
            int len = (int)ReadUInt32();
            SeekForward(len);
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
        private byte[] ReadShortRawDataReversed(int length)
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
            EnsureSpace(len);
            string str = Encoding.UTF8.GetString(Data, Position, len);
            Position += len;
            return str;
        }

        public ushort ReadUInt16()
            => BitConverter.ToUInt16(ReadShortRawDataReversed(2), 0);
        public uint ReadUInt32()
            => BitConverter.ToUInt32(ReadShortRawDataReversed(4), 0);
        public int ReadInt32()
            => BitConverter.ToInt32(ReadShortRawDataReversed(4), 0);
        public ulong ReadUInt64()
            => BitConverter.ToUInt64(ReadShortRawDataReversed(8), 0);
        public long ReadInt64()
            => BitConverter.ToInt64(ReadShortRawDataReversed(8), 0);
        public bool ReadBool()
            => ReadInt32() == 1;

        public Dictionary<PeerId, MsgId> ReadHiddenPinnedMessagesMap()
        {
            uint n = ReadUInt32();
            Dictionary<PeerId, MsgId> ret = new Dictionary<PeerId, MsgId>((int)n);
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
        public Dictionary<StorageKey, FileDesc> ReadStorageMap()
        {
            uint n = ReadUInt32();
            Dictionary<StorageKey, FileDesc> ret = new Dictionary<StorageKey, FileDesc>((int)n);
            for (int i = 0; i < n; i++)
            {
                ulong key = ReadUInt64();
                StorageKey storageKey = new StorageKey(ReadUInt64(), ReadUInt64());
                int size = ReadInt32();
                ret.Add(storageKey, new FileDesc(key, size));
            }
            return ret;
        }
        public ProxyData ReadProxy(ProxyType? proxyType = null)
        {
            return new ProxyData(
                proxyType == null ? (ProxyType)ReadInt32() : proxyType.Value,
                ReadString(), ReadInt32(),
                ReadString(), ReadString());
        }
    }
}
