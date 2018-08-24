namespace MihaZupan.TelegramStorageParser
{
    /// <summary>
    /// Implement this class if you need different functionality for accessing files (e.g. if you have files in memory)
    /// </summary>
    public abstract class FileProvider
    {
        /// <summary>
        /// This method will always be called before <see cref="ReadFile(string)"/>.
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns></returns>
        public abstract bool FileExists(string filePath);

        /// <summary>
        /// <see cref="FileExists(string)"/> will always be called before this method
        /// </summary>
        /// <param name="filePath">Relative file path</param>
        /// <returns></returns>
        public abstract byte[] ReadFile(string filePath);
    }
}
