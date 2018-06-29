using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using MihaZupan.TelegramLocalStorage.TgCrypto;

namespace MihaZupan.TelegramLocalStorage
{
    public class LocalStorage
    {
        #region Public properties
        public string TelegramDataPath { get; private set; }

        public int AppVersion
        {
            get
            {
                EnsureParsedState();
                return _map.AppVersion;
            }
        }
        public long StorageImagesSize
        {
            get
            {
                EnsureParsedState();
                return _map.StorageImagesSize;
            }
        }
        public long StorageStickersSize
        {
            get
            {
                EnsureParsedState();
                return _map.StorageStickersSize;
            }
        }
        public long StorageAudiosSize
        {
            get
            {
                EnsureParsedState();
                return _map.StorageAudiosSize;
            }
        }

        public Dictionary<BlockIDs, Dictionary<string, object>> SettingsMap
        {
            get
            {
                EnsureParsedState();
                return _settings.SettingsMap;
            }
        }
        #endregion

        private FileIO _fileIO;        
        private AuthKey _localKey = null;
        private Map _map;
        private Settings _settings;
        private bool _localStorageParsed = false;

        private LocalStorage(string tDataPath)
        {
            TelegramDataPath = tDataPath;
            _fileIO = new FileIO(tDataPath);            
        }

        public static ParsingState TryParse(string tDataPath, out LocalStorage localStorage)
        {
            return TryParse(tDataPath, null, out localStorage);
        }
        public static ParsingState TryParse(string tDataPath, string passcode, out LocalStorage localStorage)
        {
            localStorage = new LocalStorage(tDataPath);
            
            if (!Directory.Exists(tDataPath)) return ParsingState.FileNotFound;
            
            ParsingState settingsState = Settings.TryParseSettings(localStorage._fileIO, out localStorage._settings);
            if (settingsState != ParsingState.Success) return settingsState;
            
            byte[] passcodeUtf8 = passcode == null ? null : Encoding.UTF8.GetBytes(passcode);
            ParsingState mapState = Map.TryParseMap(localStorage._fileIO, passcodeUtf8, out localStorage._map, out localStorage._localKey);
            if (mapState != ParsingState.Success) return mapState;
            
            localStorage._localStorageParsed = true;
            return ParsingState.Success;
        }

        public void ExportImages(string outputDirectory)
        {
            EnsureParsedState();

            Directory.CreateDirectory(outputDirectory);
            int imageCount = 0;
            Console.WriteLine("Exporting {0} images", _map.ImagesMap.Count);
            foreach (var imageFile in _map.ImagesMap.Values)
            {
                if (_fileIO.FileExists(imageFile, FilePath.User))
                {
                    var image = _fileIO.ReadEncryptedFile(imageFile, FilePath.User, _localKey).DataStream;
                    image.SeekForward(20);
                    byte[] data = image.ReadByteArray();
                    File.WriteAllBytes(Path.Combine(outputDirectory, ++imageCount + ".jpg"), data);
                }
            }
        }
        public void ExportAudios(string outputDirectory)
        {
            EnsureParsedState();

            Directory.CreateDirectory(outputDirectory);
            int audioCount = 0;
            Console.WriteLine("Exporting {0} audio files", _map.AudiosMap.Count);
            foreach (var audioFile in _map.AudiosMap.Values)
            {
                if (_fileIO.FileExists(audioFile, FilePath.User))
                {
                    var audio = _fileIO.ReadEncryptedFile(audioFile, FilePath.User, _localKey).DataStream;
                    audio.SeekForward(16);
                    byte[] data = audio.ReadByteArray();
                    File.WriteAllBytes(Path.Combine(outputDirectory, ++audioCount + ".ogg"), data);
                }
            }
        }

        private void EnsureParsedState()
        {
            if (!_localStorageParsed) throw new NotParsedException();
        }
    }

    public class NotParsedException : Exception
    {
        internal NotParsedException()
            : base("Local Storage has not been parsed") { }
    }
}
