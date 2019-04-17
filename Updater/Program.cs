using Ionic.Zip;
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Updater
{
    class Program
    {
        static string PastebinURL = @"https://pastebin.com/raw/V8ZjRVfw";

        public enum webpageindexes
        {
            Name, ImageUrl, FileUrl, VersionNumber, password
        }

        static Dictionary<string, string> ToolVersions = new Dictionary<string, string> { };

        static string[] Entries;


        const int SW_HIDE = 0;
        const int SW_SHOW = 5;
        private const int STD_OUTPUT_HANDLE = -11;
        private const int MY_CODE_PAGE = 437;

        [DllImport("kernel32.dll")]
        static extern IntPtr GetConsoleWindow();

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("kernel32.dll", EntryPoint = "GetStdHandle", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern IntPtr GetStdHandle(int nStdHandle);

        [DllImport("kernel32.dll", EntryPoint = "AllocConsole", SetLastError = true, CharSet = CharSet.Auto, CallingConvention = CallingConvention.StdCall)]
        public static extern int AllocConsole();

        [DllImport("kernel32.dll")]
        static extern bool FreeConsole();

        static void Main(string[] args)
        {
            //Allocate console if not hidden
            if (!args.Contains("-hidden"))
            {
              AllocConsole();
              IntPtr stdHandle = GetStdHandle(STD_OUTPUT_HANDLE);
              SafeFileHandle safeFileHandle = new SafeFileHandle(stdHandle, true);
              FileStream fileStream = new FileStream(safeFileHandle, FileAccess.Write);
              Encoding encoding = System.Text.Encoding.GetEncoding(MY_CODE_PAGE);
              StreamWriter standardOutput = new StreamWriter(fileStream, encoding);
              standardOutput.AutoFlush = true;
              Console.SetOut(standardOutput);
            }

            Console.WriteLine("Checking For Updates");

            //Read the version of all installed tools
            Console.WriteLine("Read version log...");
            ToolVersions = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText("VersionLog"));
            
            //Read the most current version of all tools
            Console.WriteLine("Reading online versions...");
            var result = string.Empty;
            using (var webClient = new System.Net.WebClient())
            {
                result = webClient.DownloadString(PastebinURL);
            }
            Entries = result.Split(new string[] { "\r\n" }, StringSplitOptions.None);

            Console.WriteLine("Comparing Versions");
            Console.WriteLine("------------");

            //Loop through all the tools, check if they are downloaded.
            //If they are downloaded, check if they are up to date
            foreach (string Entry in Entries)
            {
                string[] entry = Entry.Split('|');
                string name = entry[(int)webpageindexes.Name];
                string version = entry[(int)webpageindexes.VersionNumber];

                Console.WriteLine($"Checking {name} for updates");
                if(ToolVersions.ContainsKey(name))
                {
                    Console.WriteLine($"{name}] Installed Version: {ToolVersions[name]}. Current Version: {version}");
                    //If up to date
                    if(ToolVersions[name] == version)
                    {
                        Console.WriteLine($"{name} is up to date");
                    }
                    else
                    {
                        //If not up to date, download tool
                        Console.WriteLine($"{name} Needs Updating");
                        Console.WriteLine($"Downloading {name} from {entry[(int)webpageindexes.FileUrl]}");

                        using (var client = new WebClient())
                        {
                            client.DownloadFile(entry[(int)webpageindexes.FileUrl], $@"Downloads\{name}_{version.Replace('.', '_').Replace(' ', '_')}.zip");
                        }
                        Console.WriteLine("Download Finished. Extracting...");


                        //Set up zip extractor
                        using (ZipFile archive = new ZipFile($@"Downloads\{name}_{version.Replace('.', '_').Replace(' ', '_')}.zip"))
                        {
                            if (entry[(int)webpageindexes.password] != "none")
                            {
                                archive.Password = entry[(int)webpageindexes.password];
                            }
                            archive.Encryption = EncryptionAlgorithm.PkzipWeak; // the default: you might need to select the proper value here


                            //Delete old files (only .exe and .dll so that caches and presets are not deleted)
                            DirectoryInfo di = new DirectoryInfo($@"Installations\{name}\");
                            FileInfo[] files = di.GetFiles("*.exe", SearchOption.AllDirectories)
                                                 .Where(p => p.Extension == ".exe").ToArray();
                            foreach (FileInfo file in files)
                                try
                                {
                                    file.Attributes = FileAttributes.Normal;
                                    File.Delete(file.FullName);
                                }
                                catch { }

                            di = new DirectoryInfo($@"Installations\{name}\");
                            files = di.GetFiles("*.dll", SearchOption.AllDirectories).Where(p => p.Extension == ".dll").ToArray();
                            foreach (FileInfo file in files)
                                try
                                {
                                    file.Attributes = FileAttributes.Normal;
                                    File.Delete(file.FullName);
                                }
                                catch { }

                            //Extract files
                            archive.ExtractAll($@"Installations\{name}\", ExtractExistingFileAction.OverwriteSilently);
                            archive.StatusMessageTextWriter = Console.Out;
                            
                            //Update the version log
                            if (ToolVersions.ContainsKey(name))
                            {
                                ToolVersions[name] = version;
                            }
                            else
                            {
                                ToolVersions.Add(name, version);
                            }
                            File.WriteAllText("VersionLog", Newtonsoft.Json.JsonConvert.SerializeObject(ToolVersions));
                        }
                    }
                }
                else
                {
                    Console.WriteLine($"{name} is not currently installed");
                }
                Console.WriteLine("------------");
            }


            Console.WriteLine("Done");
            System.Threading.Thread.Sleep(3000);
            FreeConsole();
            Environment.Exit(0);
        }
    }
}
