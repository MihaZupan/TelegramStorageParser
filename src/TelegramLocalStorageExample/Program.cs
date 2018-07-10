using System;
using MihaZupan.TelegramLocalStorage;

namespace MihaZupan.TelegramLocalStorageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            string tDataPath = args.Length != 0
                ? args[0]
                : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Telegram Desktop/tdata";

            // No passcode - same as passing null as the passcode
            ParsingState parsingState = LocalStorage.TryParse(tDataPath, out LocalStorage localStorage);

            if (parsingState == ParsingState.InvalidPasscode)
            {
                Console.WriteLine("Enter passcode:");
                string passcode = Console.ReadLine();
                // Local passcode
                parsingState = LocalStorage.TryParse(tDataPath, passcode, out localStorage);
            }

            if (parsingState != ParsingState.Success)
            {
                if (parsingState == ParsingState.InvalidPasscode)
                {
                    Console.WriteLine();
                    Console.WriteLine("Passcode was incorrect");
                    Console.WriteLine("You can try brute forcing it using John the Ripper");
                    Console.WriteLine("Use the format 'telegram' to use the CPU or 'telegram-opencl' to use your GPU");
                    Console.WriteLine("You can use the following hash string:");
                    Console.WriteLine(PasscodeBruteForce.GenerateJohnTheRipperHashString(tDataPath));
                    Console.WriteLine();
                }
                else
                {
                    Console.WriteLine("Something went wrong: " + parsingState);
                }
            }
            
            if (parsingState == ParsingState.Success)
            {
                Console.WriteLine("App version: " + localStorage.AppVersion);
                Console.WriteLine();

                int MB = 1024 * 1024;
                Console.WriteLine("Total storage size for images: {0} MB ", localStorage.StorageImagesSize / MB);
                Console.WriteLine("Total storage size for stickers: {0} MB", localStorage.StorageStickersSize / MB);
                Console.WriteLine("Total storage size for audio files: {0} MB", localStorage.StorageAudiosSize / MB);
                Console.WriteLine();

                Console.WriteLine("Settings:");
                foreach (var setting in localStorage.SettingsMap)
                {
                    Console.WriteLine("{0} with {1} value{2}",
                        setting.Key,
                        setting.Value.Count,
                        setting.Value.Count % 100 == 1 ? "" : "s");
                }
                Console.WriteLine();

                localStorage.ExportImages("TG-Images");
                localStorage.ExportAudios("TG-Audios");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }        
    }
}
