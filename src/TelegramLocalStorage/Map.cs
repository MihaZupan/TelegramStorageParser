using System.Collections.Generic;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    internal class Map
    {
        public Dictionary<PeerId, FileKey> DraftsMap;
        public Dictionary<PeerId, FileKey> DraftCursorsMap;
        public Dictionary<PeerId, bool> DraftsNotReadMap;
        public Dictionary<StorageKey, FileDesc> ImagesMap;
        public Dictionary<StorageKey, FileDesc> StickerImagesMap;
        public Dictionary<StorageKey, FileDesc> AudiosMap;

        public long StorageImagesSize = 0;
        public long StorageStickersSize = 0;
        public long StorageAudiosSize = 0;
        public FileKey LocationsKey = null;
        public FileKey ReportSpamStatusesKey = null;
        public FileKey TrustedBotsKey = null;
        public FileKey RecentStickersKeyOld = null;
        public FileKey InstalledStickersKey = null;
        public FileKey FeaturedStickersKey = null;
        public FileKey RecentStickersKey = null;
        public FileKey FavedStickersKey = null;
        public FileKey ArchivedStickersKey = null;
        public FileKey SavedGifsKey = null;
        public FileKey BackgroundKey = null;
        public FileKey UserSettingsKey = null;
        public FileKey RecentHashtagsAndBotsKey = null;
        public FileKey SavedPeersKey = null;

        private Map()
        {
            DraftsMap = new Dictionary<PeerId, FileKey>();
            DraftCursorsMap = new Dictionary<PeerId, FileKey>();
            DraftsNotReadMap = new Dictionary<PeerId, bool>();
            ImagesMap = new Dictionary<StorageKey, FileDesc>();
            StickerImagesMap = new Dictionary<StorageKey, FileDesc>();
            AudiosMap = new Dictionary<StorageKey, FileDesc>();
        }

        public static ParsingState TryParseMap(FileIO fileIO, byte[] password, out Map map, out AuthKey localKey)
        {
            map = null;
            localKey = null;

            if (!fileIO.FileExists("map", FilePath.User)) return ParsingState.FileNotFound;

            DataStream stream = fileIO.ReadFile("map", FilePath.User);
            byte[] salt = stream.ReadByteArray();
            byte[] keyEncrypted = stream.ReadByteArray();
            byte[] mapEncrypted = stream.ReadByteArray();
            if (salt.Length != Constants.LocalEncryptSaltSize) return ParsingState.InvalidData;

            AuthKey passKey = AuthKey.CreateLocalKey(password, salt);
            bool result = Decrypt.TryDecryptLocal(keyEncrypted, passKey, out byte[] keyData);
            if (!result) return ParsingState.InvalidPasscode;

            localKey = new AuthKey(keyData);
            result = Decrypt.TryDecryptLocal(mapEncrypted, localKey, out byte[] mapData);
            if (!result) return ParsingState.InvalidData;

            return TryParse(new DataStream(mapData), out map);
        }

        private static ParsingState TryParse(DataStream stream, out Map map)
        {
            map = new Map();

            while (!stream.AtEnd)
            {
                uint storageKey = stream.ReadUInt32();
                if (!map.ReadKey((LocalStorageKeys)storageKey, stream))
                    return ParsingState.InvalidData;
            }

            return ParsingState.Success;
        }
        private bool ReadKey(LocalStorageKeys storageKey, DataStream stream)
        {
            switch (storageKey)
            {
                case LocalStorageKeys.lskDraft:
                    {
                        DraftsMap = stream.ReadDraftsMap();
                        foreach (var draft in DraftsMap)
                            DraftsNotReadMap.Add(draft.Key, true);
                    }
                    break;

                case LocalStorageKeys.lskDraftPosition:
                    {
                        DraftCursorsMap = stream.ReadDraftsMap();
                    }
                    break;

                case LocalStorageKeys.lskImages:
                    {
                        ImagesMap = stream.ReadStorageMap(out long storageImagesSize);
                        StorageImagesSize = storageImagesSize;
                    }
                    break;

                case LocalStorageKeys.lskStickerImages:
                    {
                        StickerImagesMap = stream.ReadStorageMap(out long storageStickersSize);
                        StorageStickersSize = storageStickersSize;
                    }
                    break;

                case LocalStorageKeys.lskAudios:
                    {
                        AudiosMap = stream.ReadStorageMap(out long storageAudiosSize);
                        StorageAudiosSize = storageAudiosSize;
                    }
                    break;

                case LocalStorageKeys.lskLocations:
                    {
                        LocationsKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskReportSpamStatuses:
                    {
                        ReportSpamStatusesKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskTrustedBots:
                    {
                        TrustedBotsKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskRecentStickersOld:
                    {
                        RecentStickersKeyOld = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskBackground:
                    {
                        BackgroundKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskUserSettings:
                    {
                        UserSettingsKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskRecentHashtagsAndBots:
                    {
                        RecentHashtagsAndBotsKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskStickersOld:
                    {
                        InstalledStickersKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskStickersKeys:
                    {
                        InstalledStickersKey = stream.ReadUInt64();
                        FeaturedStickersKey = stream.ReadUInt64();
                        RecentStickersKey = stream.ReadUInt64();
                        ArchivedStickersKey = stream.ReadUInt64();                        
                    }
                    break;

                case LocalStorageKeys.lskFavedStickers:
                    {
                        FavedStickersKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskSavedGifsOld:
                    {
                        stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskSavedGifs:
                    {
                        SavedGifsKey = stream.ReadUInt64();
                    }
                    break;

                case LocalStorageKeys.lskSavedPeers:
                    {
                        SavedPeersKey = stream.ReadUInt64();
                    }
                    break;

                default:
                    return false;
            }

            return true;
        }
        private enum LocalStorageKeys
        { // Local Storage Keys
            lskUserMap = 0x00,
            lskDraft = 0x01, // data: PeerId peer
            lskDraftPosition = 0x02, // data: PeerId peer
            lskImages = 0x03, // data: StorageKey location
            lskLocations = 0x04, // no data
            lskStickerImages = 0x05, // data: StorageKey location
            lskAudios = 0x06, // data: StorageKey location
            lskRecentStickersOld = 0x07, // no data
            lskBackground = 0x08, // no data
            lskUserSettings = 0x09, // no data
            lskRecentHashtagsAndBots = 0x0a, // no data
            lskStickersOld = 0x0b, // no data
            lskSavedPeers = 0x0c, // no data
            lskReportSpamStatuses = 0x0d, // no data
            lskSavedGifsOld = 0x0e, // no data
            lskSavedGifs = 0x0f, // no data
            lskStickersKeys = 0x10, // no data
            lskTrustedBots = 0x11, // no data
            lskFavedStickers = 0x12, // no data
        };
    }
}
