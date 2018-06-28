using System.Collections.Generic;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    internal class Settings
    {
        public Dictionary<BlockIDs, Dictionary<string, object>> SettingsMap { get; private set; }

        private Settings()
        {
            SettingsMap = new Dictionary<BlockIDs, Dictionary<string, object>>();
        }

        public static ParsingState TryParseSettings(FileIO fileIO, out Settings settings)
        {
            settings = null;

            if (!fileIO.FileExists("settings", FilePath.Base)) return ParsingState.FileNotFound;

            DataStream file = fileIO.ReadFile("settings", FilePath.Base);
            byte[] salt = file.ReadByteArray();
            if (salt.Length != Constants.LocalEncryptSaltSize) return ParsingState.InvalidData;
            byte[] settingsEncrypted = file.ReadByteArray();

            AuthKey settingsKey = AuthKey.CreateLocalKey(null, salt);
            bool result = Decrypt.TryDecryptLocal(settingsEncrypted, settingsKey, out byte[] settingsData);
            if (!result) return ParsingState.InvalidData;

            return TryParse(new DataStream(settingsData), out settings);
        }
        public static ParsingState TryReadMtpData(FileIO fileIO, FileKey fileKey, AuthKey key, out Settings mtpData)
        {
            mtpData = null;

            if (!fileIO.FileExists(fileKey, FilePath.Base)) return ParsingState.FileNotFound;

            DataStream file;
            try
            {
                file = fileIO.ReadEncryptedFile(fileKey.ToFilePart(), FilePath.Base, key);
            }
            catch
            {
                mtpData = null;
                return ParsingState.InvalidData;
            }
            return TryParse(file, out mtpData);
        }

        private static ParsingState TryParse(DataStream stream, out Settings settings)
        {
            settings = new Settings();

            while (!stream.AtEnd)
            {
                uint blockId = stream.ReadUInt32();
                if (!settings.ReadSetting((BlockIDs)blockId, stream))
                    return ParsingState.InvalidData;
            }

            return ParsingState.Success;
        }

        // Not thoroughly tested since I don't have enough test data
        private bool ReadSetting(BlockIDs blockId, DataStream stream)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            switch (blockId)
            {
                case BlockIDs.dbiDcOptionOldOld:
                    {
                        values.Add("dcId", stream.ReadUInt32());
                        values.Add("port", stream.ReadUInt32());
                        values.Add("host", stream.ReadString());
                        values.Add("ip", stream.ReadString());
                    }
                    break;

                case BlockIDs.dbiDcOptionOld:
                    {
                        values.Add("dcIdWithShift", stream.ReadUInt32());
                        values.Add("flags", stream.ReadInt32());
                        values.Add("ip", stream.ReadString());
                        values.Add("port", stream.ReadUInt32());
                    }
                    break;

                case BlockIDs.dbiDcOptions:
                    {
                        values.Add("serialized", stream.ReadByteArray());
                    }
                    break;

                case BlockIDs.dbiChatSizeMax:
                    {
                        values.Add("maxSize", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSavedGifsLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiStickersRecentLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiStickersFavedLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiMegagroupSizeMax:
                    {
                        values.Add("maxSize", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiUser:
                    {
                        values.Add("dcId", stream.ReadUInt32());
                        values.Add("userId", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiKey:
                    {
                        values.Add("dcId", stream.ReadInt32());
                        values.Add("key", stream.ReadRawData(Constants.AuthKeySize));
                    }
                    break;

                case BlockIDs.dbiMtpAuthorization:
                    {
                        values.Add("serialized", stream.ReadByteArray());
                    }
                    break;

                case BlockIDs.dbiAutoStart:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiStartMinimized:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSendToMenu:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiUseExternalVideoPlayer:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSoundNotify:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAutoDownload:
                    {
                        values.Add("photo", stream.ReadInt32());
                        values.Add("audio", stream.ReadInt32());
                        values.Add("gif", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAutoPlay:
                    {
                        values.Add("gif", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiDialogsMode:
                    {
                        values.Add("enabled", stream.ReadInt32());
                        values.Add("modeInt", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiModerateMode:
                    {
                        values.Add("enabled", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiIncludeMuted:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiShowingSavedGifsOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiDesktopNotify:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiWindowsNotificationsOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiNativeNotifications:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiNotificationsCount:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiNotificationsCorner:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiDialogsWidthRatioOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiLastSeenWarningSeenOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAuthSessionSettings:
                    {
                        values.Add("v", stream.ReadByteArray());
                    }
                    break;

                case BlockIDs.dbiWorkMode:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiConnectionTypeOld:
                    {
                        Dictionary<string, object> proxy = new Dictionary<string, object>(5)
                        {
                            { "proxyType", stream.ReadInt32() },
                            { "proxyHost", stream.ReadString() },
                            { "proxyPort", stream.ReadInt32() },
                            { "proxyUser", stream.ReadString() },
                            { "proxyPassword", stream.ReadString() }
                        };
                        values.Add("proxy", proxy);
                    }
                    break;

                case BlockIDs.dbiConnectionType:
                    {
                        int connectionType = stream.ReadInt32();
                        values.Add("connectionType", connectionType);
                        
                        int count = 1;
                        if (connectionType == 4)
                        {
                            count = stream.ReadInt32();
                            int index = stream.ReadInt32();
                        }

                        List<Dictionary<string, object>> proxies = new List<Dictionary<string, object>>(count);
                        for (int i = 0; i < count; i++)
                        {
                            Dictionary<string, object> proxy = new Dictionary<string, object>(5)
                            {
                                { "proxyType", stream.ReadInt32() },
                                { "proxyHost", stream.ReadString() },
                                { "proxyPort", stream.ReadInt32() },
                                { "proxyUser", stream.ReadString() },
                                { "proxyPassword", stream.ReadString() }
                            };
                            proxies.Add(proxy);
                        }
                        values.Add("proxies", proxies);
                    }
                    break;

                case BlockIDs.dbiThemeKey:
                    {
                        values.Add("themeKey", stream.ReadUInt64());
                    }
                    break;

                case BlockIDs.dbiLangPackKey:
                    {
                        values.Add("langPackKey", stream.ReadUInt64());
                    }
                    break;

                case BlockIDs.dbiTryIPv6:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSeenTrayTooltip:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAutoUpdate:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiLastUpdateCheck:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiScale:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiLangOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiLangFileOld:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case BlockIDs.dbiWindowPosition:
                    {
                        values.Add("x", stream.ReadInt32());
                        values.Add("y", stream.ReadInt32());
                        values.Add("w", stream.ReadInt32());
                        values.Add("h", stream.ReadInt32());
                        values.Add("moncrc", stream.ReadInt32());
                        values.Add("maximized", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiLoggedPhoneNumber:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case BlockIDs.dbiMutePeer:
                    {
                        values.Add("peerId", stream.ReadUInt64());
                    }
                    break;

                case BlockIDs.dbiMutedPeers:
                    {
                        values.Add("count", stream.ReadPeerIds());
                    }
                    break;

                case BlockIDs.dbiSendKey:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiCatsAndDogs:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiTileBackground:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAdaptiveForWide:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAutoLock:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiReplaceEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSuggestEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiSuggestStickersByEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiDefaultAttach:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiNotifyView:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiAskDownloadPath:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiDownloadPathOld:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case BlockIDs.dbiDownloadPath:
                    {
                        values.Add("v", stream.ReadString());
                        values.Add("bookmark", stream.ReadByteArray());
                    }
                    break;

                case BlockIDs.dbiCompressPastedImage:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiEmojiTabOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiRecentEmojiOldOld:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreloadOldOld());
                    }
                    break;

                case BlockIDs.dbiRecentEmojiOld:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreloadOld());
                    }
                    break;

                case BlockIDs.dbiRecentEmoji:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreload());
                    }
                    break;

                case BlockIDs.dbiRecentStickers:
                    {
                        values.Add("v", stream.ReadRecentStickerPreload());
                    }
                    break;

                case BlockIDs.dbiEmojiVariantsOld:
                    {
                        values.Add("v", stream.ReadEmojiColorVariantsOld());
                    }
                    break;

                case BlockIDs.dbiEmojiVariants:
                    {
                        values.Add("v", stream.ReadEmojiColorVariants());
                    }
                    break;

                case BlockIDs.dbiHiddenPinnedMessages:
                    {
                        values.Add("v", stream.ReadHiddenPinnedMessagesMap());
                    }
                    break;

                case BlockIDs.dbiDialogLastPath:
                    {
                        values.Add("path", stream.ReadString());
                    }
                    break;

                case BlockIDs.dbiSongVolume:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case BlockIDs.dbiVideoVolume:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                default:                    
                    return false;
            }

            SettingsMap.Add(blockId, values);

            return true;
        }

        public enum BlockIDs
        {
            dbiKey = 0x00,
            dbiUser = 0x01,
            dbiDcOptionOldOld = 0x02,
            dbiChatSizeMax = 0x03,
            dbiMutePeer = 0x04,
            dbiSendKey = 0x05,
            dbiAutoStart = 0x06,
            dbiStartMinimized = 0x07,
            dbiSoundNotify = 0x08,
            dbiWorkMode = 0x09,
            dbiSeenTrayTooltip = 0x0a,
            dbiDesktopNotify = 0x0b,
            dbiAutoUpdate = 0x0c,
            dbiLastUpdateCheck = 0x0d,
            dbiWindowPosition = 0x0e,
            dbiConnectionTypeOld = 0x0f,
            // 0x10 reserved
            dbiDefaultAttach = 0x11,
            dbiCatsAndDogs = 0x12,
            dbiReplaceEmoji = 0x13,
            dbiAskDownloadPath = 0x14,
            dbiDownloadPathOld = 0x15,
            dbiScale = 0x16,
            dbiEmojiTabOld = 0x17,
            dbiRecentEmojiOldOld = 0x18,
            dbiLoggedPhoneNumber = 0x19,
            dbiMutedPeers = 0x1a,
            // 0x1b reserved
            dbiNotifyView = 0x1c,
            dbiSendToMenu = 0x1d,
            dbiCompressPastedImage = 0x1e,
            dbiLangOld = 0x1f,
            dbiLangFileOld = 0x20,
            dbiTileBackground = 0x21,
            dbiAutoLock = 0x22,
            dbiDialogLastPath = 0x23,
            dbiRecentEmojiOld = 0x24,
            dbiEmojiVariantsOld = 0x25,
            dbiRecentStickers = 0x26,
            dbiDcOptionOld = 0x27,
            dbiTryIPv6 = 0x28,
            dbiSongVolume = 0x29,
            dbiWindowsNotificationsOld = 0x30,
            dbiIncludeMuted = 0x31,
            dbiMegagroupSizeMax = 0x32,
            dbiDownloadPath = 0x33,
            dbiAutoDownload = 0x34,
            dbiSavedGifsLimit = 0x35,
            dbiShowingSavedGifsOld = 0x36,
            dbiAutoPlay = 0x37,
            dbiAdaptiveForWide = 0x38,
            dbiHiddenPinnedMessages = 0x39,
            dbiRecentEmoji = 0x3a,
            dbiEmojiVariants = 0x3b,
            dbiDialogsMode = 0x40,
            dbiModerateMode = 0x41,
            dbiVideoVolume = 0x42,
            dbiStickersRecentLimit = 0x43,
            dbiNativeNotifications = 0x44,
            dbiNotificationsCount = 0x45,
            dbiNotificationsCorner = 0x46,
            dbiThemeKey = 0x47,
            dbiDialogsWidthRatioOld = 0x48,
            dbiUseExternalVideoPlayer = 0x49,
            dbiDcOptions = 0x4a,
            dbiMtpAuthorization = 0x4b,
            dbiLastSeenWarningSeenOld = 0x4c,
            dbiAuthSessionSettings = 0x4d,
            dbiLangPackKey = 0x4e,
            dbiConnectionType = 0x4f,
            dbiStickersFavedLimit = 0x50,
            dbiSuggestStickersByEmoji = 0x51,
            dbiSuggestEmoji = 0x52,

            dbiEncryptedWithSalt = 333,
            dbiEncrypted = 444,

            // 500-600 reserved

            dbiVersion = 666,
        }
    }
}
