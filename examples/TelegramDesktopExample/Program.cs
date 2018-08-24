using System;
using System.IO;
using MihaZupan.TelegramStorageParser;
using MihaZupan.TelegramStorageParser.TelegramDesktop;
using Newtonsoft.Json;

namespace TelegramDesktopExample
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

                while (parsingState == ParsingState.InvalidPasscode)
                {
                    Console.WriteLine("The passcode is incorrect, please try again");
                    passcode = Console.ReadLine();
                    parsingState = LocalStorage.TryParse(tDataPath, passcode, out localStorage);
                }
                Console.Clear();
                Console.WriteLine("Logged in successfully!");
            }

            if (parsingState != ParsingState.Success)
            {
                Console.WriteLine("Something went wrong: " + parsingState);
                Console.ReadLine();
                return;
            }

            Console.WriteLine("Exporting images. Count: {0}", localStorage.ImageCount);
            Directory.CreateDirectory("Images");
            int countSize = (int)Math.Ceiling(Math.Log10(localStorage.ImageCount));
            int count = 1;
            foreach (var image in localStorage.Images)
            {
                File.WriteAllBytes($"Images\\image_{count.ToString().PadLeft(countSize, '0')}.jpg", image);
                count++;
            }

            Console.WriteLine("Exporting voice recordings. Count: {0}", localStorage.VoiceRecordingCount);
            Directory.CreateDirectory("Voice");
            countSize = (int)Math.Ceiling(Math.Log10(localStorage.VoiceRecordingCount));
            count = 1;
            foreach (var voiceRecording in localStorage.VoiceRecordings)
            {
                File.WriteAllBytes($"Voice\\recording_{count.ToString().PadLeft(countSize, '0')}.ogg", voiceRecording);
                count++;
            }

            Console.WriteLine("Writing all data to json (the file will be pretty big because of the images) ...");
            File.WriteAllText("LocalStorage.json", JsonConvert.SerializeObject(localStorage, Formatting.Indented));

            Console.WriteLine("Done");
            Console.ReadLine();
        }
    }
}
