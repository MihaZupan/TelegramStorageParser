using System.IO;

namespace MihaZupan.TelegramStorageParser
{
    internal class FileSystemFileProvider : FileProvider
    {
        public readonly string Root;

        public FileSystemFileProvider(string root)
            => Root = root;

        public override bool FileExists(string filePath)
            => File.Exists(Path.Combine(Root, filePath));

        public override byte[] ReadFile(string filePath)
            => File.ReadAllBytes(Path.Combine(Root, filePath));
    }
}
