using CommandLine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;
using System.Linq;

namespace BookmarkManager
{
    class Program
    {
        public class Options
        {
            [Option('m', "mode", Required = true, HelpText = "Mode of operation: 'import', 'export', or 'set-startup'.")]
            public string Mode { get; set; }

            [Option('p', "path", Required = true, HelpText = "Path to the browser where bookmarks or preferences are stored.")]
            public string Path { get; set; }

            [Option('s', "startup", Required = false, HelpText = "Startup page URL (only required for 'set-startup' mode).")]
            public string StartupPage { get; set; }

            [Option('e', "export-file", Required = false, HelpText = "Path and filename for exporting bookmarks (only for 'export' mode).")]
            public string ExportFile { get; set; }

            [Option('i', "import-file", Required = false, HelpText = "Path and filename for importing bookmarks (only for 'import' mode).")]
            public string ImportFile { get; set; }

            [Option('b', "browser", Required = false, HelpText = "Specify the browser (optional, e.g., 'chrome', 'firefox').")]
            public string Browser { get; set; }

            [Option('q', "silent", Required = false, Default = 0, HelpText = "Silent mode: 0 (default) - normal output, 1 - minimal output (true/false).")]
            public int Silent { get; set; }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: BookmarkManager.exe --mode <import|export|set-startup> --path <browser path> [--startup <url>] [--export-file <file path>] [--import-file <file path>] [--browser <browser>] [--silent <0|1>]");
                Console.WriteLine("Options:");
                Console.WriteLine("  -m, --mode           Mode of operation: 'import', 'export', or 'set-startup'.");
                Console.WriteLine("  -p, --path           Path to the browser where bookmarks or preferences are stored.");
                Console.WriteLine("  -s, --startup        (Optional) Startup page URL (only required for 'set-startup' mode).");
                Console.WriteLine("  -e, --export-file    (Optional) Path and filename for exporting bookmarks (only for 'export' mode).");
                Console.WriteLine("  -i, --import-file    (Optional) Path and filename for importing bookmarks (only for 'import' mode).");
                Console.WriteLine("  -b, --browser        (Optional) Specify the browser (e.g., 'chrome', 'firefox').");
                Console.WriteLine("  -q, --silent         (Optional) Silent mode: 0 (default) - normal output, 1 - minimal output (true/false).");

                Thread.Sleep(20000);
                return;
            }

            Parser.Default.ParseArguments<Options>(args)
                  .WithParsed<Options>(opts => RunOptionsAndReturnExitCode(opts));
        }

        private static void RunOptionsAndReturnExitCode(Options opts)
        {
            bool success = false;

            if (opts.Mode == "import")
            {
                success = ImportBookmarks(opts.Path, opts.ImportFile, opts.Silent);
            }
            else if (opts.Mode == "export")
            {
                success = ExportBookmarks(opts.Path, opts.ExportFile, opts.Silent);
            }
            else if (opts.Mode == "set-startup" && !string.IsNullOrEmpty(opts.StartupPage))
            {
                success = SetStartupPage(opts.Path, opts.StartupPage, opts.Silent);
            }
            else
            {
                if (opts.Silent == 0)
                {
                    Console.WriteLine("Invalid mode or missing URL for 'set-startup' mode.");
                }
                return;
            }

            if (opts.Silent == 1)
            {
                Console.WriteLine(success ? "true" : "false");
            }
        }

        public static bool ImportBookmarks(string browserPath, string importFilePath, int silent)
        {
            bool exceptionOccurred = false;

            try
            {
                string bookmarkFile = Path.Combine(browserPath, "Bookmarks");
                string backupFile = Path.Combine(browserPath, "Bookmarks.bak");

                if (string.IsNullOrEmpty(importFilePath))
                {
                    importFilePath = Path.Combine(browserPath, "ExportedBookmarks.json");
                }

                if (!File.Exists(importFilePath))
                {
                    if (silent == 0)
                    {
                        Console.WriteLine($"The import file '{importFilePath}' does not exist.");
                    }
                    return false;
                }

                JObject existingBookmarks;

                if (!File.Exists(bookmarkFile))
                {
                    existingBookmarks = CreateEmptyBookmarkStructure();
                }
                else
                {
                    existingBookmarks = JObject.Parse(File.ReadAllText(bookmarkFile));
                }

                var importedBookmarks = JObject.Parse(File.ReadAllText(importFilePath));

                MergeBookmarks(existingBookmarks["roots"]["bookmark_bar"] as JObject, importedBookmarks["roots"]["bookmark_bar"] as JObject);

                File.WriteAllText(bookmarkFile, existingBookmarks.ToString());
                File.WriteAllText(backupFile, existingBookmarks.ToString());

                return exceptionOccurred ? throw new Exception("trueWithExceptions") : true;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Error during bookmark import: {ex.Message}");
                }
                return exceptionOccurred ? throw new Exception("trueWithExceptions") : false;
            }
        }

        public static bool ExportBookmarks(string browserPath, string exportFilePath, int silent)
        {
            bool exceptionOccurred = false;

            try
            {
                string bookmarkFile = Path.Combine(browserPath, "Bookmarks");

                if (!File.Exists(bookmarkFile))
                {
                    if (silent == 0)
                    {
                        Console.WriteLine($"The bookmark file '{bookmarkFile}' does not exist.");
                    }
                    return false;
                }

                var json = File.ReadAllText(bookmarkFile);
                var bookmarks = JObject.Parse(json);

                if (string.IsNullOrEmpty(exportFilePath))
                {
                    exportFilePath = Path.Combine(browserPath, "ExportedBookmarks.json");
                }

                File.WriteAllText(exportFilePath, bookmarks.ToString());

                return exceptionOccurred ? throw new Exception("trueWithExceptions") : true;
            }
            catch (IOException ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"File access error: {ex.Message}");
                }
                return exceptionOccurred ? throw new Exception("trueWithExceptions") : false;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                return exceptionOccurred ? throw new Exception("trueWithExceptions") : false;
            }
        }

        public static bool SetStartupPage(string browserPath, string startupPage, int silent)
        {
            bool exceptionOccurred = false;

            try
            {
                string prefsFile = Path.Combine(browserPath, "Preferences");
                string backupPrefsFile = Path.Combine(browserPath, "Preferences.bak");

                if (!File.Exists(prefsFile))
                {
                    if (silent == 0)
                    {
                        Console.WriteLine($"The preferences file '{prefsFile}' does not exist.");
                    }
                    return false;
                }

                var json = File.ReadAllText(prefsFile);
                var prefs = JObject.Parse(json);

                prefs["session"] = prefs["session"] ?? new JObject();
                prefs["session"]["restore_on_startup"] = 4;

                prefs["session"]["startup_urls"] = new JArray { startupPage };
                prefs["homepage"] = startupPage;

                File.WriteAllText(prefsFile, prefs.ToString());
                File.WriteAllText(backupPrefsFile, prefs.ToString());

                return exceptionOccurred ? throw new Exception("trueWithExceptions") : true;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Error modifying preferences: {ex.Message}");
                }
                return exceptionOccurred ? throw new Exception("trueWithExceptions") : false;
            }
        }

        private static JObject CreateEmptyBookmarkStructure()
        {
            return new JObject
            {
                ["checksum"] = "",
                ["roots"] = new JObject
                {
                    ["bookmark_bar"] = new JObject
                    {
                        ["children"] = new JArray(),
                        ["name"] = "Bookmarks bar",
                        ["type"] = "folder"
                    },
                    ["other"] = new JObject
                    {
                        ["children"] = new JArray(),
                        ["name"] = "Other bookmarks",
                        ["type"] = "folder"
                    },
                    ["synced"] = new JObject
                    {
                        ["children"] = new JArray(),
                        ["name"] = "Mobile bookmarks",
                        ["type"] = "folder"
                    }
                },
                ["version"] = 1
            };
        }

        private static void MergeBookmarks(JObject existing, JObject imported)
        {
            if (existing["children"] is JArray existingChildren && imported["children"] is JArray importedChildren)
            {
                foreach (var importedItem in importedChildren)
                {
                    // Vérifier si l'élément importé existe déjà pour éviter les doublons
                    var matchingItem = existingChildren.FirstOrDefault(
                        child => child["name"]?.ToString() == importedItem["name"]?.ToString() &&
                        child["url"]?.ToString() == importedItem["url"]?.ToString()
                    );

                    if (matchingItem != null)
                    {
                        continue; // Ignorer l'élément s'il existe déjà
                    }

                    var matchingFolder = existingChildren.FirstOrDefault(
                        child => child["name"]?.ToString() == importedItem["name"]?.ToString() &&
                        child["type"]?.ToString() == "folder"
                    ) as JObject;

                    if (matchingFolder != null)
                    {
                        MergeBookmarks(matchingFolder, importedItem as JObject);
                    }
                    else
                    {
                        existingChildren.Add(importedItem);
                    }
                }
            }
        }
    }
}
