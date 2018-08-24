namespace MihaZupan.TelegramStorageParser.TelegramDesktop.BlockIDs
{
    internal enum LocalStorageKey
    {
        lskUserMap = 0x00,
        lskDraft = 0x01, // data: PeerId peer
        lskDraftPosition = 0x02, // data: PeerId peer
        lskImages = 0x03, // data: StorageKey location
        lskLocations = 0x04, // no data
        lskStickerImages = 0x05, // data: StorageKey location
        lskAudios = 0x06, // data: StorageKey location
        lskRecentStickersOld = 0x07, // no data
        lskBackgroundOld = 0x08, // no data
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
        lskExportSettings = 0x13, // no data
        lskBackground = 0x14, // no data
    }
}
