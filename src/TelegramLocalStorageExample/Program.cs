using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using MihaZupan.TelegramLocalStorage;

namespace MihaZupan.TelegramLocalStorageExample
{
    class Program
    {
        static void Main(string[] args)
        {
            TestGeneratedMapData();

            //string tDataPath = args.Length != 0 ? args[0] : "tdata";
            string tDataPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/Telegram Desktop/tdata";
            
            // Local passcode
            ParsingState parsingState = LocalStorage.TryParse(tDataPath, "passcode123", out LocalStorage localStorage);

            // No passcode - same as passing null as the passcode
            //ParsingState parsingState = LocalStorage.TryParse(tDataPath, out LocalStorage localStorage);

            if (parsingState != ParsingState.Success)
            {
                if (parsingState == ParsingState.InvalidPasscode)
                {
                    Console.WriteLine("Invalid passcode, trying to brute force it now");
                    if (TryBruteforceMapPasscode(tDataPath, out string passcode))
                    {
                        Console.WriteLine("Found the passcode: " + passcode);
                        parsingState = LocalStorage.TryParse(tDataPath, passcode, out localStorage);
                    }
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


                //localStorage.ExportImages("TG-Images");
                //localStorage.ExportAudios("TG-Audios");
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void TestGeneratedMapData()
        {
            Random random = new Random();
            bool works = true;
            for (int i = 0; i < 10; i++)
            {
                byte[] passcode = new byte[random.Next(1, 20)];
                random.NextBytes(passcode);
                byte[] mapBytes = MapPasscodeBruteForce.GenerateTestMapData(passcode);
                MapPasscodeBruteForce bf = new MapPasscodeBruteForce(mapBytes, 1);
                if (!bf.Try(passcode, 0)) works = false;
            }
            Console.WriteLine("Generated test map data " + (works ? "works" : "doesn't work"));
        }        

        // Sample method that tests all 4-character combinations of lowercase, uppercase English letters and numbers
        static bool TryBruteforceMapPasscode(string tDataPath, out string pass)
        {
            // Prepare some variables
            int coreCount = Environment.ProcessorCount - 1;
            bool[] active = new bool[coreCount];
            for (int i = 0; i < coreCount; i++) active[i] = false;
            object threadLock = new object();
            string correctPasscode = "";
            int tested = 0;
            bool found = false;
            MapPasscodeBruteForce bruteForce = new MapPasscodeBruteForce(tDataPath, coreCount);

            // Testing all 4-character combinations of upper and lowercase English letters and numbers
            const int numChars = 4;
            const int optionsPerChar = 62;
            int numOptions = (int)Math.Pow(optionsPerChar, numChars);
            byte[][] passcodeBuffers = new byte[coreCount][];
            for (int i = 0; i < coreCount; i++) passcodeBuffers[i] = new byte[numChars];

            Console.WriteLine("Number of combinations to test: " + numOptions);
            Stopwatch stopwatch = new Stopwatch();

            // Let us know how we're doing
            Task.Run(() =>
            {
                while (true)
                {
                    Thread.Sleep(5000);
                    if (!found && tested < numOptions)
                    {
                        Console.WriteLine("Tested {0} passcodes - {1}% - avg. speed: {2} h/s",
                            tested,
                            ((float)tested / numOptions * 100).ToString("0.00"),
                            (int)((float)tested / stopwatch.ElapsedMilliseconds * 1000));
                    }
                    else break;
                }
            });

            stopwatch.Start();

            // Run the tests in parralel
            Parallel.For(0, numOptions, new ParallelOptions() { MaxDegreeOfParallelism = coreCount },
                i =>
                {
                    // See which thread number is still free - this is so MapPasscodeBruteForce.cs byte arrays don't get messed up
                    if (found) return;
                    int thread = 0;
                    lock (threadLock)
                    {
                        tested++;
                        for (int j = 0; j < coreCount; j++)
                        {
                            if (!active[j])
                            {
                                active[j] = true;
                                thread = j;
                                break;
                            }
                        }
                    }

                    // Derive the passcode from i
                    byte[] passcode = passcodeBuffers[thread];
                    int tmpI = i;
                    for (int j = 0; j < numChars; j++)
                    {
                        int x = tmpI % optionsPerChar;

                        if (x < 26) passcode[j] = (byte)('a' + x);
                        else if (x < 52) passcode[j] = (byte)('A' + x - 26);
                        else passcode[j] = (byte)('0' + x - 52);

                        tmpI /= optionsPerChar;
                    }

                    // Try that passcode
                    if (bruteForce.Try(passcode, thread))
                    {
                        found = true;
                        correctPasscode = Encoding.UTF8.GetString(passcode);
                    }
                    active[thread] = false;
                });

            stopwatch.Stop();
            Console.WriteLine("Total time elapsed: {0} s", stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine("Average speed: {0} hashes/s", (int)((float)tested / stopwatch.ElapsedMilliseconds * 1000));

            pass = found ? correctPasscode : null;
            return found;
        }
    }
}
