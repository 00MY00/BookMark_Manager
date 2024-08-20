using CommandLine;
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using System.Threading;

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

            [Option('h', "homepage", Required = false, HelpText = "Homepage URL (optional, typically used with 'set-startup' mode).")]
            public string Homepage { get; set; }

            [Option('e', "export-file", Required = false, HelpText = "Path and filename for exporting bookmarks (only for 'export' mode).")]
            public string ExportFile { get; set; }

            [Option('i', "import-file", Required = false, HelpText = "Path and filename for importing bookmarks (only for 'import' mode).")]
            public string ImportFile { get; set; }

            [Option('q', "silent", Required = false, Default = 0, HelpText = "Silent mode: 0 (default) - normal output, 1 - minimal output.")]
            public int Silent { get; set; }
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("Usage: BookmarkManager.exe --mode <import|export|set-startup> --path <browser path> [--startup <url>] [--homepage <url>] [--export-file <file path>] [--import-file <file path>] [--silent <0|1>]");
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
            else if (opts.Mode == "set-startup" && (!string.IsNullOrEmpty(opts.StartupPage) || !string.IsNullOrEmpty(opts.Homepage)))
            {
                success = SetStartupPage(opts.Path, opts.StartupPage, opts.Homepage, opts.Silent);
            }
            else
            {
                if (opts.Silent == 0)
                {
                    Console.WriteLine("Invalid mode or missing URL for 'set-startup' mode.");
                }
                return;
            }

            if (opts.Silent == 0)
            {
                Console.WriteLine(success ? "Operation completed successfully." : "Operation failed.");
            }
        }

        public static bool SetStartupPage(string browserPath, string startupPage, string homepage, int silent)
        {
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

                // Assurer que les sections nécessaires existent
                prefs["browser"] = prefs["browser"] ?? new JObject();
                prefs["session"] = prefs["session"] ?? new JObject();

                string formattedUrl = startupPage;
                if (!startupPage.Contains("://"))
                {
                    formattedUrl = "https://" + startupPage;
                }

                // Configurer la page de démarrage
                if (!string.IsNullOrEmpty(startupPage))
                {
                    prefs["browser"]["first_run_tabs"] = new JArray { formattedUrl };
                    prefs["session"]["restore_on_startup"] = 4;
                    prefs["session"]["startup_urls"] = new JArray { formattedUrl };
                }

                // Configurer la page d'accueil
                if (!string.IsNullOrEmpty(homepage))
                {
                    prefs["homepage"] = homepage;
                    prefs["browser"]["homepage"] = homepage;
                    prefs["browser"]["homepage_is_newtabpage"] = false;
                }

                // Sauvegarder le fichier
                File.WriteAllText(prefsFile, prefs.ToString());
                File.WriteAllText(backupPrefsFile, prefs.ToString());

                return true;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Error modifying preferences: {ex.Message}");
                }
                return false;
            }
        }

        public static bool ImportBookmarks(string browserPath, string importFilePath, int silent)
        {
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

                return true;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Error during bookmark import: {ex.Message}");
                }
                return false;
            }
        }

        public static bool ExportBookmarks(string browserPath, string exportFilePath, int silent)
        {
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

                return true;
            }
            catch (IOException ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"File access error: {ex.Message}");
                }
                return false;
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"An error occurred: {ex.Message}");
                }
                return false;
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
                    var matchingItem = existingChildren.FirstOrDefault(
                        child => child["name"]?.ToString() == importedItem["name"]?.ToString() &&
                        child["url"]?.ToString() == importedItem["url"]?.ToString()
                    );

                    if (matchingItem != null)
                    {
                        continue;
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
