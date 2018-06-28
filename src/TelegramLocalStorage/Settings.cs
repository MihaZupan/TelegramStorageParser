using System.Collections.Generic;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    public class Settings
    {
        public Dictionary<DataBlockIDs, Dictionary<string, object>> SettingsMap;

        private Settings()
        {
            SettingsMap = new Dictionary<DataBlockIDs, Dictionary<string, object>>();
        }
        
        public static bool TryParseSettings(out Settings settings)
        {
            settings = null;

            DataStream file = FileIO.ReadFile("settings", FilePath.Base);
            byte[] salt = file.ReadByteArray();
            if (salt.Length != Constants.LocalEncryptSaltSize) return false;
            byte[] settingsEncrypted = file.ReadByteArray();

            AuthKey settingsKey = AuthKey.CreateLocalKey(null, salt);
            bool result = Decrypt.TryDecryptLocal(settingsEncrypted, settingsKey, out byte[] settingsData);
            if (!result) return false;

            return TryParse(new DataStream(settingsData), out settings);
        }
        public static bool TryReadMtpData(FileKey fileKey, AuthKey key, out Settings mtpData)
        {            
            DataStream file;
            try
            {
                file = FileIO.ReadEncryptedFile(fileKey.ToFilePart(), FilePath.Base, key);
            }
            catch
            {
                mtpData = null;
                return false;
            }
            return TryParse(file, out mtpData);
        }

        private static bool TryParse(DataStream stream, out Settings settings)
        {
            settings = new Settings();

            while (!stream.AtEnd)
            {
                uint blockId = stream.ReadUInt32();
                if (!settings.ReadSetting((DataBlockIDs)blockId, stream))
                    return false;
            }

            return true;
        }

        // Not thoroughly tested since I don't have enough test data
        private bool ReadSetting(DataBlockIDs blockId, DataStream stream)
        {
            Dictionary<string, object> values = new Dictionary<string, object>();
            switch (blockId)
            {
                case DataBlockIDs.dbiDcOptionOldOld:
                    {
                        values.Add("dcId", stream.ReadUInt32());
                        values.Add("port", stream.ReadUInt32());
                        values.Add("host", stream.ReadString());
                        values.Add("ip", stream.ReadString());
                    }
                    break;

                case DataBlockIDs.dbiDcOptionOld:
                    {
                        values.Add("dcIdWithShift", stream.ReadUInt32());
                        values.Add("flags", stream.ReadInt32());
                        values.Add("ip", stream.ReadString());
                        values.Add("port", stream.ReadUInt32());
                    }
                    break;

                case DataBlockIDs.dbiDcOptions:
                    {
                        values.Add("serialized", stream.ReadByteArray());
                    }
                    break;

                case DataBlockIDs.dbiChatSizeMax:
                    {
                        values.Add("maxSize", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSavedGifsLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiStickersRecentLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiStickersFavedLimit:
                    {
                        values.Add("limit", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiMegagroupSizeMax:
                    {
                        values.Add("maxSize", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiUser:
                    {
                        values.Add("dcId", stream.ReadUInt32());
                        values.Add("userId", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiKey:
                    {
                        values.Add("dcId", stream.ReadInt32());
                        values.Add("key", stream.ReadRawData(Constants.AuthKeySize));
                    }
                    break;

                case DataBlockIDs.dbiMtpAuthorization:
                    {
                        values.Add("serialized", stream.ReadByteArray());
                    }
                    break;

                case DataBlockIDs.dbiAutoStart:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiStartMinimized:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSendToMenu:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiUseExternalVideoPlayer:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSoundNotify:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAutoDownload:
                    {
                        values.Add("photo", stream.ReadInt32());
                        values.Add("audio", stream.ReadInt32());
                        values.Add("gif", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAutoPlay:
                    {
                        values.Add("gif", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiDialogsMode:
                    {
                        values.Add("enabled", stream.ReadInt32());
                        values.Add("modeInt", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiModerateMode:
                    {
                        values.Add("enabled", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiIncludeMuted:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiShowingSavedGifsOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiDesktopNotify:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiWindowsNotificationsOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiNativeNotifications:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiNotificationsCount:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiNotificationsCorner:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiDialogsWidthRatioOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiLastSeenWarningSeenOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAuthSessionSettings:
                    {
                        values.Add("v", stream.ReadByteArray());
                    }
                    break;

                case DataBlockIDs.dbiWorkMode:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiConnectionTypeOld:
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

                case DataBlockIDs.dbiConnectionType:
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

                case DataBlockIDs.dbiThemeKey:
                    {
                        values.Add("themeKey", stream.ReadUInt64());
                    }
                    break;

                case DataBlockIDs.dbiLangPackKey:
                    {
                        values.Add("langPackKey", stream.ReadUInt64());
                    }
                    break;

                case DataBlockIDs.dbiTryIPv6:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSeenTrayTooltip:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAutoUpdate:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiLastUpdateCheck:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiScale:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiLangOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiLangFileOld:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case DataBlockIDs.dbiWindowPosition:
                    {
                        values.Add("x", stream.ReadInt32());
                        values.Add("y", stream.ReadInt32());
                        values.Add("w", stream.ReadInt32());
                        values.Add("h", stream.ReadInt32());
                        values.Add("moncrc", stream.ReadInt32());
                        values.Add("maximized", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiLoggedPhoneNumber:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case DataBlockIDs.dbiMutePeer:
                    {
                        values.Add("peerId", stream.ReadUInt64());
                    }
                    break;

                case DataBlockIDs.dbiMutedPeers:
                    {
                        values.Add("count", stream.ReadPeerIds());
                    }
                    break;

                case DataBlockIDs.dbiSendKey:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiCatsAndDogs:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiTileBackground:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAdaptiveForWide:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAutoLock:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiReplaceEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSuggestEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiSuggestStickersByEmoji:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiDefaultAttach:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiNotifyView:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiAskDownloadPath:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiDownloadPathOld:
                    {
                        values.Add("v", stream.ReadString());
                    }
                    break;

                case DataBlockIDs.dbiDownloadPath:
                    {
                        values.Add("v", stream.ReadString());
                        values.Add("bookmark", stream.ReadByteArray());
                    }
                    break;

                case DataBlockIDs.dbiCompressPastedImage:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiEmojiTabOld:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiRecentEmojiOldOld:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreloadOldOld());
                    }
                    break;

                case DataBlockIDs.dbiRecentEmojiOld:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreloadOld());
                    }
                    break;

                case DataBlockIDs.dbiRecentEmoji:
                    {
                        values.Add("v", stream.ReadRecentEmojiPreload());
                    }
                    break;

                case DataBlockIDs.dbiRecentStickers:
                    {
                        values.Add("v", stream.ReadRecentStickerPreload());
                    }
                    break;

                case DataBlockIDs.dbiEmojiVariantsOld:
                    {
                        values.Add("v", stream.ReadEmojiColorVariantsOld());
                    }
                    break;

                case DataBlockIDs.dbiEmojiVariants:
                    {
                        values.Add("v", stream.ReadEmojiColorVariants());
                    }
                    break;

                case DataBlockIDs.dbiHiddenPinnedMessages:
                    {
                        values.Add("v", stream.ReadHiddenPinnedMessagesMap());
                    }
                    break;

                case DataBlockIDs.dbiDialogLastPath:
                    {
                        values.Add("path", stream.ReadString());
                    }
                    break;

                case DataBlockIDs.dbiSongVolume:
                    {
                        values.Add("v", stream.ReadInt32());
                    }
                    break;

                case DataBlockIDs.dbiVideoVolume:
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

        public enum DataBlockIDs
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
