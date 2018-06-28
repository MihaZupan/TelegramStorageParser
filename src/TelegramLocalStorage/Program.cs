using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Collections.Generic;
using MihaZupan.TelegramLocalStorage.TgCrypto;
using MihaZupan.TelegramLocalStorage.Types;

namespace MihaZupan.TelegramLocalStorage
{
    class Program
    {
        static void Main(string[] args)
        {
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

                //SaveAllImages(map.ImagesMap, localKey);
                //SaveAllAudios(map.AudiosMap, localKey);
                
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
            int coreCount = Environment.ProcessorCount - 1;
            bool[] active = new bool[coreCount];
            for (int i = 0; i < coreCount; i++) active[i] = false;
            object threadLock = new object();
            string correctPasscode = "";
            int tested = 0;
            bool found = false;
            MapPasscodeBruteForce bruteForce = new MapPasscodeBruteForce(coreCount);

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
                        File.WriteAllText("passcode.txt", correctPasscode);
                        Console.WriteLine("Correct passcode: " + correctPasscode);
                    }
                    active[thread] = false;
                });

            stopwatch.Stop();
            Console.WriteLine("Total time elapsed: {0} s", stopwatch.ElapsedMilliseconds / 1000);
            Console.WriteLine("Average speed: {0} hashes/s", (int)((float)tested / stopwatch.ElapsedMilliseconds * 1000));
        }
        public static void Shuffle(byte[][] array)
        {	
            Random rng = new Random();	
            int n = array.Length;	
            while (n > 1)	
            {	
                int k = rng.Next(n--);	
                byte[] temp = array[n];	
                array[n] = array[k];	
                array[k] = temp;	
            }	
        }

        // Helper to serialize the object to a file
        static void WriteJson(string fileName, object obj)
        {
            File.WriteAllText(fileName, JsonConvert.SerializeObject(obj, Formatting.Indented));
        }        

        static void SaveAllImages(Dictionary<StorageKey, FileDesc> imagesMap, AuthKey localKey)
        {
            Directory.CreateDirectory("images");
            int imageCount = 0;
            foreach (var imageFile in imagesMap.Values)
            {
                if (FileIO.FileExists(imageFile, FilePath.User))
                {
                    var image = FileIO.ReadEncryptedFile(imageFile, FilePath.User, localKey);
                    image.SeekForward(20);
                    byte[] data = image.ReadByteArray();
                    File.WriteAllBytes("images\\" + ++imageCount + ".jpg", data);
                }
            }
        }
        static void SaveAllAudios(Dictionary<StorageKey, FileDesc> audiosMap, AuthKey localKey)
        {
            Directory.CreateDirectory("audio");
            int audioCount = 0;
            foreach (var audioFile in audiosMap.Values)
            {
                if (FileIO.FileExists(audioFile, FilePath.User))
                {
                    var audio = FileIO.ReadEncryptedFile(audioFile, FilePath.User, localKey);
                    audio.SeekForward(16);
                    byte[] data = audio.ReadByteArray();
                    File.WriteAllBytes("audio\\" + ++audioCount + ".ogg", data);
                }
            }
        }
    }         
}
