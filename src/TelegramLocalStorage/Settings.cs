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

            DataStream file = fileIO.ReadFile("settings", FilePath.Base).DataStream;
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
                file = fileIO.ReadEncryptedFile(fileKey.ToFilePart(), FilePath.Base, key).DataStream;
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

            try
            {
                while (!stream.AtEnd)
                {
                    uint blockId = stream.ReadUInt32();
                    ParsingState state = settings.ReadSetting((BlockIDs)blockId, stream);
                    if (state != ParsingState.Success) return state;
                }
            }
            catch
            {
                return ParsingState.InvalidData;
            }

            return ParsingState.Success;
        }

        // Not thoroughly tested since I don't have enough test data
        private ParsingState ReadSetting(BlockIDs blockId, DataStream stream)
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

                case BlockIDs.dbiTxtDomainString:
                    {
                        values.Add("v", stream.ReadString());
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
                    return ParsingState.UnsupportedVersion;
            }

            SettingsMap.Add(blockId, values);

            return ParsingState.Success;
        }
    }
}
