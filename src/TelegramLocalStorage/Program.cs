using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MihaZupan.TelegramLocalStorage.TgCrypto;

namespace MihaZupan.TelegramLocalStorage
{
    class Program
    {
        static void Main(string[] args)
        {
            // This will assume the 'tdata' directory is in the working directory - change it in Constants.cs otherwise

            if (Settings.TryParseSettings(out Settings settings))
            {
                WriteJson("settings.json", settings);
            }
            else
            {
                Console.WriteLine("Failed to read settings");
            }
            
            if (Map.TryParseMap(Encoding.UTF8.GetBytes("pass"), out Map map, out AuthKey localKey))
            {
                WriteJson("map.json", map);

                // Only try parsing MtpData if parsing the map was successful since we need the same key
                if (Settings.TryReadMtpData(Constants.DataNameKey, localKey, out Settings mtpData))
                {
                    WriteJson("mtpData.json", mtpData);
                }
                else
                {
                    Console.WriteLine("Failed to read mtp data");
                }
            }
            else
            {
                Console.WriteLine("Failed to parse map");
            }
            
            TryBruteforceMapPasscode();

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        static void TryBruteforceMapPasscode()
        {
            // Prepare some variables
            int coreCount = Environment.ProcessorCount / 4 * 3;
            bool[] active = new bool[coreCount];
            for (int i = 0; i < coreCount; i++) active[i] = false;
            object threadLock = new object();
            string correctPasscode = "";
            int tested = 0;
            bool found = false;
            MapPasscodeBruteForce bruteForce = new MapPasscodeBruteForce(coreCount);

            // Testing all 4-character combinations of upper and lowercase English letters
            const int numChars = 4;
            const int optionsPerChar = 52;
            int numOptions = (int)Math.Pow(optionsPerChar, numChars);

            // Generate all possible passcodes in UTF8 encoded byte arrays
            byte[][] options = new byte[numOptions][];
            for (int i = 0; i < numOptions; i++)
            {
                byte[] passcode = new byte[numChars];
                int tmpI = i;
                for (int j = 0; j < numChars; j++)
                {
                    int x = tmpI % optionsPerChar;

                    if (x < 26) passcode[j] = (byte)('a' + x);
                    else passcode[j] = (byte)('A' + x - 26);

                    tmpI /= optionsPerChar;
                }
                options[i] = passcode;
            }

            // Let us know how we're doing
            Task.Run(() =>
            {
                while (!found && tested < numOptions)
                {
                    Thread.Sleep(5000);
                    Console.WriteLine("Tested {0} passcodes - {1}%", tested, ((float)tested / numOptions * 100).ToString("0.000"));
                }
            });

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
                            }
                        }
                    }

                    // Try that password
                    if (bruteForce.Try(options[i], thread))
                    {
                        found = true;
                        correctPasscode = Encoding.UTF8.GetString(options[i]);
                        File.WriteAllText("passcode.txt", correctPasscode);
                        Console.WriteLine("Correct passcode: " + correctPasscode);
                    }
                    active[thread] = false;
                });
        }

        // Helper to serialize the object to a file
        static void WriteJson(string fileName, object obj)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }        
    }         
}
