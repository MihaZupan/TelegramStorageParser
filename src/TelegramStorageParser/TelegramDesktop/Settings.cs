using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using MihaZupan.TelegramStorageParser.TelegramDesktop.BlockIDs;
using MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes.Enums;
using MihaZupan.TelegramStorageParser.TelegramDesktop.IO;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types.Enums;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop
{
    internal static class Settings
    {
        public static ParsingState TryParseSettings(LocalStorage storage)
        {
            try
            {
                var parsingState = storage._fileIO.TryReadFile("settings", FilePath.Base, out FileReadDescriptor file);
                if (parsingState.NotSuccessful()) return parsingState;

                DataStream stream = file.DataStream;
                byte[] salt = stream.ReadByteArray();
                byte[] settingsEncrypted = stream.ReadByteArray();
                if (salt.Length != Constants.LocalEncryptSaltSize)
                    return ParsingState.InvalidData;

                AuthKey settingsKey = AuthKey.CreateLocalKey(null, salt);
                if (!settingsKey.TryDecryptLocal(settingsEncrypted, out byte[] settingsData))
                    return ParsingState.InvalidData;

                return TryReadSettings(new DataStream(settingsData), storage);
            }
            catch
            {
                return ParsingState.InvalidData;
            }
        }

        public static ParsingState TryReadSettings(DataStream stream, LocalStorage storage)
        {
            try
            {
                while (!stream.AtEnd)
                {
                    var blockId = (DataBlockID)stream.ReadUInt32();
                    if (!TryReadSetting(blockId, stream, storage))
                        return ParsingState.InvalidData;
                }
                return ParsingState.Success;
            }
            catch
            {
                return ParsingState.InvalidData;
            }
        }

        private static bool TryReadSetting(DataBlockID blockId, DataStream stream, LocalStorage storage)
        {            
            switch (blockId)
            {
                case DataBlockID.dbiDcOptions:
                    storage.TryDeserialize_DcOptions(stream.ReadByteArray());
                    return true;

                case DataBlockID.dbiChatSizeMax:
                    storage.ChatSizeMax = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiSavedGifsLimit:
                    storage.SavedGifsLimit = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiStickersRecentLimit:
                    storage.StickersRecentLimit = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiStickersFavedLimit:
                    storage.StickersFavedLimit = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiMegagroupSizeMax:
                    storage.MegagroupSizeMax = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiUser:
                    storage.AuthInfo.UserId = stream.ReadInt32();
                    storage.AuthInfo.DataCenters.Add(new DataCenter(stream.ReadInt32()));
                    return true;

                case DataBlockID.dbiKey:
                    storage.AuthInfo.DataCenters.Add(new DataCenter(stream.ReadInt32(), stream.ReadByteArray()));
                    return true;

                case DataBlockID.dbiMtpAuthorization:
                    storage.TryDeserialize_MtpAuthorization(stream.ReadByteArray());
                    return true;

                case DataBlockID.dbiAutoStart:
                    storage.AutoStart = stream.ReadBool();
                    return true;

                case DataBlockID.dbiStartMinimized:
                    storage.StartMinimized = stream.ReadBool();
                    return true;

                case DataBlockID.dbiSendToMenu:
                    storage.SendToMenu = stream.ReadBool();
                    return true;

                case DataBlockID.dbiUseExternalVideoPlayer:
                    storage.UseExternalVideoPlayer = stream.ReadBool();
                    return true;

                case DataBlockID.dbiSoundNotify:
                    storage.SoundNotify = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAutoDownload:
                    storage.AutoDownloadPhoto = stream.ReadBool();
                    storage.AutoDownloadAudio = stream.ReadBool();
                    storage.AutoDownloadGif = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAutoPlay:
                    storage.AutoPlayGif = stream.ReadBool();
                    return true;

                case DataBlockID.dbiDialogsMode:
                    storage.DialogsModeEnabled = stream.ReadBool();
                    DialogMode dialogMode = (DialogMode)stream.ReadInt32();
                    storage.DialogMode = dialogMode == DialogMode.Important ? DialogMode.Important : DialogMode.All;
                    return true;

                case DataBlockID.dbiModerateMode:
                    storage.ModerateModeEnabled = stream.ReadBool();
                    return true;

                case DataBlockID.dbiIncludeMuted:
                    storage.IncludeMuted = stream.ReadBool();
                    return true;

                case DataBlockID.dbiDesktopNotify:
                    storage.DesktopNotify = stream.ReadBool();
                    return true;

                case DataBlockID.dbiNativeNotifications:
                    storage.NativeNotifications = stream.ReadBool();
                    return true;

                case DataBlockID.dbiNotificationsCount:
                    int notificationsCount = stream.ReadInt32();
                    storage.NotificationsCount = notificationsCount > 0 ? notificationsCount : 3;
                    return true;

                case DataBlockID.dbiNotificationsCorner:
                    int corner = stream.ReadInt32();
                    storage.NotificationsCorner = (ScreenCorner)((corner >= 0 && corner < 4) ? corner : 2);
                    return true;

                case DataBlockID.dbiDialogsWidthRatioOld:
                    storage.DialogsWidthRatio = stream.ReadInt32() / 1e6f;
                    return true;

                case DataBlockID.dbiLastSeenWarningSeenOld:
                    storage.LastSeenWarningSeen = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAuthSessionSettings:
                    storage.TryDeserialize_AuthSessionSettings(stream.ReadByteArray());
                    return true;

                case DataBlockID.dbiWorkMode:
                    storage.WorkMode = (WorkMode)stream.ReadInt32();
                    return true;

                case DataBlockID.dbiTxtDomainString:
                    storage.TxtDomainString = stream.ReadString();
                    return true;

                case DataBlockID.dbiConnectionTypeOld:
                    ConnectionType connectionTypeOld = (ConnectionType)stream.ReadInt32();
                    if (connectionTypeOld == ConnectionType.HttpProxy || connectionTypeOld == ConnectionType.TcpProxy)
                    {
                        ProxyData proxyData = stream.ReadProxy(
                            proxyType: connectionTypeOld == ConnectionType.HttpProxy ? ProxyType.Http : ProxyType.Socks5);
                        storage.ProxyInfo.ProxiesList.Add(proxyData);
                        storage.ProxyInfo.UsedProxy = proxyData;
                    }
                    return true;

                case DataBlockID.dbiConnectionType:
                    ConnectionType connectionType = (ConnectionType)stream.ReadInt32();
                    if (connectionType == ConnectionType.ProxiesList)
                    {
                        int proxyCount = stream.ReadInt32();
                        int proxyIndex = stream.ReadInt32();
                        if (proxyIndex > proxyCount)
                        {
                            storage.ProxyInfo.UseProxyForCalls = true;
                            proxyIndex -= (proxyIndex > 0 ? proxyCount : -proxyCount);
                        }
                        List<ProxyData> proxyList = new List<ProxyData>();
                        for (int i = 0; i < proxyCount; i++)
                        {
                            proxyList.Add(stream.ReadProxy());
                        }
                        if (proxyIndex > 0 && proxyIndex <= proxyCount)
                        {
                            storage.ProxyInfo.UsedProxy = proxyList[proxyIndex];
                        }
                        storage.ProxyInfo.ProxiesList.AddRange(proxyList);
                    }
                    else
                    {
                        storage.ProxyInfo.ProxiesList.Add(stream.ReadProxy());
                        if (connectionType == ConnectionType.HttpProxy || connectionType == ConnectionType.TcpProxy)
                        {
                            storage.ProxyInfo.UsedProxy = storage.ProxyInfo.ProxiesList.Last();
                        }
                    }
                    return true;

                case DataBlockID.dbiThemeKeyOld:
                    storage.TryParse_ThemeLegacy(stream.ReadUInt64());
                    return true;

                case DataBlockID.dbiThemeKey:
                    storage.TryParse_ThemeDay(stream.ReadUInt64());
                    storage.TryParse_ThemeNight(stream.ReadUInt64());
                    storage.NightMode = stream.ReadBool();
                    return true;

                case DataBlockID.dbiLangPackKey:
                    storage.TryParse_LangPack(stream.ReadUInt64());
                    return true;

                case DataBlockID.dbiTryIPv6:
                    storage.TryIPv6 = stream.ReadBool();
                    return true;

                case DataBlockID.dbiSeenTrayTooltip:
                    storage.SeenTrayTooltip = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAutoUpdate:
                    storage.AutoUpdate = stream.ReadBool();
                    return true;

                case DataBlockID.dbiLastUpdateCheck:
                    storage.LastUpdateCheck = new DateTime(1970, 1, 1).AddSeconds(stream.ReadInt32());
                    return true;

                case DataBlockID.dbiLangOld:
                    storage.LegacyLanguageId = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiLangFileOld:
                    storage.LegacyLanguageFile = stream.ReadString();
                    return true;

                case DataBlockID.dbiWindowPosition:
                    storage.WindowPosition = new WindowPosition(
                        stream.ReadInt32(), stream.ReadInt32(),
                        stream.ReadInt32(), stream.ReadInt32(),
                        stream.ReadInt32(), stream.ReadInt32());
                    return true;

                case DataBlockID.dbiLoggedPhoneNumber:
                    storage.LoggedPhoneNumber = stream.ReadString().Replace(" ", "");
                    return true;

                case DataBlockID.dbiSendKey:
                    storage.SendKey = (SendKey)stream.ReadInt32();
                    return true;

                case DataBlockID.dbiTileBackground:
                    storage.TileDay = stream.ReadBool();
                    storage.TileNight = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAdaptiveForWide:
                    storage.AdaptiveForWide = stream.ReadBool();
                    return true;

                case DataBlockID.dbiAutoLock:
                    storage.AutoLockSeconds = stream.ReadInt32();
                    return true;

                case DataBlockID.dbiReplaceEmoji:
                    storage.ReplaceEmoji = stream.ReadBool();
                    return true;

                case DataBlockID.dbiSuggestEmoji:
                    storage.SuggestEmoji = stream.ReadBool();
                    return true;

                case DataBlockID.dbiSuggestStickersByEmoji:
                    storage.SuggestStickersByEmoji = stream.ReadBool();
                    return true;

                case DataBlockID.dbiNotifyView:
                    NotifyView notifyView = (NotifyView)stream.ReadInt32();
                    storage.NotifyView = (notifyView == NotifyView.ShowName || notifyView == NotifyView.ShowNothing)
                        ? notifyView
                        : NotifyView.ShowPreview;
                    return true;

                case DataBlockID.dbiAskDownloadPath:
                    storage.AskDownloadPath = stream.ReadBool();
                    return true;

                case DataBlockID.dbiDownloadPathOld:
                    storage.DownloadPath = stream.ReadString();
                    return true;

                case DataBlockID.dbiDownloadPath:
                    storage.DownloadPath = stream.ReadString();
                    storage.DownloadPathBookmark = stream.ReadString();
                    return true;

                case DataBlockID.dbiCompressPastedImage:
                    storage.CompressPastedImage = stream.ReadBool();
                    return true;

                case DataBlockID.dbiHiddenPinnedMessages:
                    storage._hiddenPinnedMessageMap = stream.ReadHiddenPinnedMessagesMap();
                    return true;

                case DataBlockID.dbiDialogLastPath:
                    storage.DialogLastPath = stream.ReadString();
                    return true;

                case DataBlockID.dbiSongVolume:
                    storage.SongVolume = (stream.ReadInt32() / 1e6f).Snap(0f, 1f);
                    return true;

                case DataBlockID.dbiVideoVolume:
                    storage.VideoVolume = (stream.ReadInt32() / 1e6f).Snap(0f, 1f);
                    return true;


                // Discard - either discarded by the client or just not useful for anything

                case DataBlockID.dbiScale:
                case DataBlockID.dbiEmojiTabOld:
                case DataBlockID.dbiDefaultAttach:
                case DataBlockID.dbiShowingSavedGifsOld:
                case DataBlockID.dbiWindowsNotificationsOld:
                case DataBlockID.dbiCatsAndDogs:
                case DataBlockID.dbiTileBackgroundOld:
                    stream.SeekForward(4);
                    return true;

                case DataBlockID.dbiMutePeer:
                    stream.SeekForward(8);
                    return true;

                case DataBlockID.dbiRecentEmojiOldOld:
                    stream.SeekForwardCollection(6);
                    return true;

                case DataBlockID.dbiMutedPeers:
                    stream.SeekForwardCollection(8);
                    return true;

                case DataBlockID.dbiRecentEmojiOld:
                case DataBlockID.dbiRecentStickers:
                    stream.SeekForwardCollection(10);
                    return true;

                case DataBlockID.dbiEmojiVariantsOld:
                    stream.SeekForwardCollection(12);
                    return true;

                case DataBlockID.dbiRecentEmoji:
                    uint recentEmojiCount = stream.ReadUInt32();
                    for (int i = 0; i < recentEmojiCount; i++)
                    {
                        stream.SeekForwardString();
                        stream.SeekForward(2);
                    }
                    return true;

                case DataBlockID.dbiEmojiVariants:
                    uint emojiVariantsCount = stream.ReadUInt32();
                    for (int i = 0; i < emojiVariantsCount; i++)
                    {
                        stream.SeekForwardString();
                        stream.SeekForward(4);
                    }
                    return true;

                case DataBlockID.dbiDcOptionOldOld:
                    stream.SeekForward(4);
                    stream.SeekForwardString();
                    stream.SeekForwardString();
                    stream.SeekForward(4);
                    return true;

                case DataBlockID.dbiDcOptionOld:
                    stream.SeekForward(8);
                    stream.SeekForwardString();
                    stream.SeekForward(4);
                    return true;


                default:
                    return false;
            }
        }
    }
}
