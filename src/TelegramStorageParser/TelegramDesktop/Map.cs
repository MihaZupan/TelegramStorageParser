using System.Text;
using MihaZupan.TelegramStorageParser.TelegramDesktop.BlockIDs;
using MihaZupan.TelegramStorageParser.TelegramDesktop.IO;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop
{
    internal static class Map
    {
        public static ParsingState TryParseMap(LocalStorage storage, string passcode)
        {
            try
            {
                var parsingState = storage._fileIO.TryReadFile("map", FilePath.User, out FileReadDescriptor file);
                if (parsingState.NotSuccessful()) return parsingState;

                storage.ClientVersion = file.Version;

                DataStream stream = file.DataStream;
                byte[] salt = stream.ReadByteArray();
                byte[] keyEncrypted = stream.ReadByteArray();
                byte[] mapEncrypted = stream.ReadByteArray();
                if (salt.Length != Constants.LocalEncryptSaltSize)
                    return ParsingState.InvalidData;

                byte[] passcodeBytes = passcode == null ? null : Encoding.UTF8.GetBytes(passcode);
                AuthKey passKey = AuthKey.CreateLocalKey(passcodeBytes, salt);
                if (!passKey.TryDecryptLocal(keyEncrypted, out byte[] keyData))
                    return ParsingState.InvalidPasscode;

                storage._localKey = new AuthKey(keyData);
                if (!storage._localKey.TryDecryptLocal(mapEncrypted, out byte[] mapData))
                    return ParsingState.InvalidData;

                var mapStream = new DataStream(mapData);
                while (!mapStream.AtEnd)
                {
                    var storageKey = (LocalStorageKey)mapStream.ReadUInt32();
                    if (!ReadKey(storageKey, mapStream, storage))
                        return ParsingState.InvalidData;
                }

                return ParsingState.Success;
            }
            catch
            {
                return ParsingState.InvalidData;
            }
        }

        private static bool ReadKey(LocalStorageKey storageKey, DataStream stream, LocalStorage storage)
        {
            switch (storageKey)
            {
                case LocalStorageKey.lskDraft:
                    storage._draftsMap = stream.ReadDraftsMap();
                    return true;

                case LocalStorageKey.lskSelfSerialized:
                    storage.TryParse_SelfSerialized(stream.ReadByteArray());
                    return true;

                case LocalStorageKey.lskDraftPosition:
                    storage._draftCursorsMap = stream.ReadDraftsMap();
                    return true;

                case LocalStorageKey.lskLegacyImages:
                    storage._imagesMap = stream.ReadStorageMap();
                    return true;

                case LocalStorageKey.lskLegacyStickerImages:
                    storage._stickersMap = stream.ReadStorageMap();
                    return true;

                case LocalStorageKey.lskLegacyAudios:
                    storage._audiosMap = stream.ReadStorageMap();
                    return true;

                case LocalStorageKey.lskLocations:
                    storage.TryParse_Locations(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskReportSpamStatusesOld:
                    storage.TryParse_ReportSpamStatuses(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskTrustedBots:
                    storage.TryParse_TrustedBots(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskRecentStickersOld:
                    storage.TryParse_RecentStickersOld(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskBackgroundOld:
                    storage.TryParse_BackgroundDay(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskBackground:
                    storage.TryParse_BackgroundDay(stream.ReadUInt64());
                    storage.TryParse_BackgroundNight(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskUserSettings:
                    storage.TryParse_UserSettings(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskRecentHashtagsAndBots:
                    storage.TryParse_RecentHashtagsAndBots(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskStickersOld:
                    storage.TryParse_InstalledStickers(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskStickersKeys:
                    storage.TryParse_InstalledStickers(stream.ReadUInt64());
                    storage.TryParse_FeaturedStickers(stream.ReadUInt64());
                    storage.TryParse_RecentStickers(stream.ReadUInt64());
                    storage.TryParse_ArchivedStickers(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskFavedStickers:
                    storage.TryParse_FavedStickers(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskSavedGifsOld:
                    stream.ReadUInt64(); // Discard
                    return true;

                case LocalStorageKey.lskSavedGifs:
                    storage.TryParse_SavedGifs(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskSavedPeersOld:
                    storage.TryParse_SavedPeers(stream.ReadUInt64());
                    return true;

                case LocalStorageKey.lskExportSettings:
                    storage.TryParse_ExportSettings(stream.ReadUInt64());
                    return true;

                default:
                    return false;
            }
        }
    }
}
