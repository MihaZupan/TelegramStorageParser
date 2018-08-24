using System;
using System.Collections.Generic;
using System.IO;
using MihaZupan.TelegramStorageParser.TelegramDesktop.IO;
using MihaZupan.TelegramStorageParser.TelegramDesktop.InternalTypes;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types;
using MihaZupan.TelegramStorageParser.TelegramDesktop.Types.Enums;

namespace MihaZupan.TelegramStorageParser.TelegramDesktop
{
    public class LocalStorage
    {
        #region Public data
        public int ClientVersion { get; internal set; }

        public bool? AutoStart              { get; internal set; }
        public bool? StartMinimized         { get; internal set; }
        public bool? SendToMenu             { get; internal set; }
        public bool? UseExternalVideoPlayer { get; internal set; }
        public bool? SoundNotify            { get; internal set; }
        public bool? AutoDownloadPhoto      { get; internal set; }
        public bool? AutoDownloadAudio      { get; internal set; }
        public bool? AutoDownloadGif        { get; internal set; }
        public bool? AutoPlayGif            { get; internal set; }
        public bool? ModerateModeEnabled    { get; internal set; }
        public bool? IncludeMuted           { get; internal set; }
        public bool? DesktopNotify          { get; internal set; }
        public bool? NativeNotifications    { get; internal set; }
        public bool? LastSeenWarningSeen    { get; internal set; }
        public bool? TryIPv6                { get; internal set; }
        public bool? SeenTrayTooltip        { get; internal set; }
        public bool? AutoUpdate             { get; internal set; }
        public bool? AdaptiveForWide        { get; internal set; }
        public bool? ReplaceEmoji           { get; internal set; }
        public bool? SuggestEmoji           { get; internal set; }
        public bool? SuggestStickersByEmoji { get; internal set; }
        public bool? AskDownloadPath        { get; internal set; }
        public bool? CompressPastedImage    { get; internal set; }
        public bool? TileDay                { get; internal set; }
        public bool? TileNight              { get; internal set; }
        public bool? DialogsModeEnabled     { get; internal set; }
        public bool? NightMode              { get; internal set; }

        public int? ChatSizeMax             { get; internal set; }
        public int? MegagroupSizeMax        { get; internal set; }
        public int? SavedGifsLimit          { get; internal set; }
        public int? StickersRecentLimit     { get; internal set; }
        public int? StickersFavedLimit      { get; internal set; }
        public int? NotificationsCount      { get; internal set; }
        public int? LegacyLanguageId        { get; internal set; }
        public int? AutoLockSeconds         { get; internal set; }

        public float? DialogsWidthRatio     { get; internal set; }
        public float? SongVolume            { get; internal set; }
        public float? VideoVolume           { get; internal set; }

        public string TxtDomainString       { get; internal set; }
        public string LegacyLanguageFile    { get; internal set; }
        public string LoggedPhoneNumber     { get; internal set; }
        public string DownloadPath          { get; internal set; }
        public string DownloadPathBookmark  { get; internal set; }
        public string DialogLastPath        { get; internal set; }

        public AuthInfo AuthInfo { get; internal set; } = new AuthInfo();
        public ProxyInfo ProxyInfo { get; internal set; } = new ProxyInfo();
        public WindowPosition WindowPosition { get; internal set; }
        public DateTime? LastUpdateCheck { get; internal set; }
        public ScreenCorner? NotificationsCorner { get; internal set; }
        public DialogMode? DialogMode { get; internal set; }
        public WorkMode? WorkMode { get; internal set; }
        public SendKey? SendKey { get; internal set; }
        public NotifyView? NotifyView { get; internal set; }

        /// <summary>
        /// Actual number of images may be lower
        /// </summary>
        public int ImageCount => _imagesMap?.Count ?? 0;
        /// <summary>
        /// Actual number of stickers may be lower
        /// </summary>
        public int StickerCount => _stickersMap?.Count ?? 0;
        /// <summary>
        /// Actual number of voice recordings may be lower
        /// </summary>
        public int VoiceRecordingCount => _audiosMap?.Count ?? 0;

        /// <summary>
        /// Reads and decrypts images on the fly - implements no internal caching
        /// <para>Images are jpg binary files</para>
        /// </summary>
        public IEnumerable<byte[]> Images
        {
            get
            {
                if (_imagesMap == null) yield break;

                foreach (var imageFile in _imagesMap.Values)
                {
                    if (_fileIO.TryReadEncryptedFile(imageFile, FilePath.User, _localKey, out FileReadDescriptor image).IsSuccessful())
                    {
                        image.DataStream.SeekForward(20);
                        yield return image.DataStream.ReadByteArray();
                    }
                }
            }
        }
        /// <summary>
        /// Reads and decrypts sticker on the fly - implements no internal caching
        /// <para>Stickers are jpg binary files</para>
        /// </summary>
        private IEnumerable<byte[]> Stickers
        {
            get
            {
                if (_stickersMap == null) yield break;

                foreach (var stickerFile in _stickersMap.Values)
                {
                    if (_fileIO.TryReadEncryptedFile(stickerFile, FilePath.User, _localKey, out FileReadDescriptor sticker).IsSuccessful())
                    {
                        sticker.DataStream.SeekForward(16);
                        yield return sticker.DataStream.ReadByteArray();
                    }
                }
            }
        } // Disable public access for now
        /// <summary>
        /// Reads and decrypts voice messages on the fly - implements no internal caching
        /// <para>Recordings are opus files</para>
        /// </summary>
        public IEnumerable<byte[]> VoiceRecordings
        {
            get
            {
                if (_audiosMap == null) yield break;

                foreach (var audioFile in _audiosMap.Values)
                {
                    if (_fileIO.TryReadEncryptedFile(audioFile, FilePath.User, _localKey, out FileReadDescriptor audio).IsSuccessful())
                    {
                        audio.DataStream.SeekForward(16);
                        yield return audio.DataStream.ReadByteArray();
                    }
                }
            }
        }
        #endregion

        #region Internal fields
        internal FileIO _fileIO;
        internal AuthKey _localKey;
        internal bool _minimizeFileIo;

        internal Dictionary<PeerId, FileKey> _draftsMap;
        internal Dictionary<PeerId, FileKey> _draftCursorsMap;
        internal Dictionary<StorageKey, FileDesc> _imagesMap;
        internal Dictionary<StorageKey, FileDesc> _stickersMap;
        internal Dictionary<StorageKey, FileDesc> _audiosMap;
        internal Dictionary<PeerId, MsgId> _hiddenPinnedMessageMap;
        #endregion

        #region Constructors
        private LocalStorage(FileProvider fileProvider, bool minimizeFileIo)
        {
            _fileIO = new FileIO(fileProvider);
            _minimizeFileIo = minimizeFileIo;

            // Until the parsing is actually implemented
            _minimizeFileIo = true;
        }

        public static ParsingState TryParse(string tDataPath, out LocalStorage localStorage, bool minimizeFileIo = false)
            => TryParse(tDataPath, null, out localStorage, minimizeFileIo);
        public static ParsingState TryParse(string tDataPath, string passcode, out LocalStorage localStorage, bool minimizeFileIo = false)
        {
            localStorage = null;

            if (!Directory.Exists(tDataPath))
                return ParsingState.FileNotFound;

            return TryParse(new FileSystemFileProvider(tDataPath), passcode, out localStorage, minimizeFileIo);
        }
        public static ParsingState TryParse(FileProvider fileProvider, out LocalStorage localStorage, bool minimizeFileIo = false)
            => TryParse(fileProvider, null, out localStorage, minimizeFileIo);
        public static ParsingState TryParse(FileProvider fileProvider, string passcode, out LocalStorage localStorage, bool minimizeFileIo = false)
        {
            localStorage = new LocalStorage(fileProvider, minimizeFileIo);

            var parsingState = Map.TryParseMap(localStorage, passcode);
            if (parsingState.NotSuccessful()) return parsingState;

            parsingState = Settings.TryParseSettings(localStorage);
            if (parsingState.NotSuccessful()) return parsingState;

            localStorage.TryParse_MtpData();
            localStorage.TryParse_DraftsWithCursors();

            return ParsingState.Success;
        }
        #endregion

        #region Internal parsing
        private void TryParse_MtpData()
        {
            if (_fileIO.TryReadEncryptedFile(FileIO.DataNameFilePart, FilePath.Base, _localKey, out FileReadDescriptor file).IsSuccessful())
                Settings.TryReadSettings(file.DataStream, this);
        }
        internal void TryParse_UserSettings(FileKey fileKey)
        {
            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).IsSuccessful())
                Settings.TryReadSettings(file.DataStream, this);
        }
        internal void TryDeserialize_MtpAuthorization(byte[] serialized)
        {
            DataStream stream = new DataStream(serialized);

            AuthInfo.UserId = stream.ReadInt32();
            AuthInfo.MainDcId = stream.ReadInt32();

            int count = stream.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int dcId = stream.ReadInt32();
                byte[] secret = stream.ReadRawData(Constants.AuthKeySize);
                AuthInfo.DataCenters.Add(new DataCenter(dcId, secret));
            }

            // Discard mtpKeysToDestroy
        }
        internal void TryDeserialize_DcOptions(byte[] serialized)
        {
            DataStream stream = new DataStream(serialized);

            int minusVersion = stream.ReadInt32();
            int version = (minusVersion < 0) ? (-minusVersion) : 0;

            int count = version > 0 ? stream.ReadInt32() : minusVersion;

            for (int i = 0; i < count; i++)
            {
                int id = stream.ReadInt32();
                int flags = stream.ReadInt32(); // Discard for now
                int port = stream.ReadInt32();
                string ip = stream.ReadString();
                byte[] secret = version > 0 ? stream.ReadByteArray() : null;
                AuthInfo.DataCenters.Add(new DataCenter(id, ip, port, secret));
            }

            if (!stream.AtEnd)
            {
                // Ignore CDN config
            }
        }

        #region ToDo
        internal void TryDeserialize_AuthSessionSettings(byte[] serialized)
        {
            DataStream stream = new DataStream(serialized);


        }
        #region MinimizeFileIO
        internal void TryParse_DraftsWithCursors()
        {
            if (_minimizeFileIo) return;


        }
        internal void TryParse_Locations(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ReportSpamStatuses(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_TrustedBots(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_RecentStickersOld(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_InstalledStickers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_FeaturedStickers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_RecentStickers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_FavedStickers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ArchivedStickers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_SavedGifs(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_BackgroundDay(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_BackgroundNight(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_RecentHashtagsAndBots(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_SavedPeers(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ExportSettings(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ThemeLegacy(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ThemeDay(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_ThemeNight(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        internal void TryParse_LangPack(FileKey fileKey)
        {
            if (_minimizeFileIo) return;

            if (_fileIO.TryReadEncryptedFile(fileKey, FilePath.User, _localKey, out FileReadDescriptor file).NotSuccessful())
                return;

            DataStream stream = file.DataStream;

        }
        #endregion MinimizeFileIO
        #endregion ToDo
        #endregion Internal parsing
    }
}
