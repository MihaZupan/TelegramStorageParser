namespace MihaZupan.TelegramLocalStorage
{
    public enum ParsingState
    {
        /// <summary>
        /// A-OK
        /// </summary>
        Success,

        /// <summary>
        /// The passcode supplied was not valid
        /// </summary>
        InvalidPasscode,

        /// <summary>
        /// One of the files has not been found.
        /// Either the directory is invalid or files were removed manually
        /// </summary>
        FileNotFound,

        /// <summary>
        /// Could mean an internal error
        /// </summary>
        InvalidData,

        /// <summary>
        /// Could mean the library is outdated
        /// </summary>
        UnsupportedVersion
    }
}