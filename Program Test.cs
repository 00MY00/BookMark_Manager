using CommandLine;
using System;
using System.IO;
using Newtonsoft.Json;
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
            if (opts.Silent == 0)
            {
                Console.WriteLine($"Executing in mode: {opts.Mode}");
                Console.WriteLine($"Using path: {opts.Path}");
            }

            bool success = false;

            var profiles = GetBrowserProfiles(opts.Path);
            foreach (var profile in profiles)
            {
                if (opts.Mode == "import")
                {
                    success = ImportBookmarks(profile, opts.ImportFile, opts.Silent);
                }
                else if (opts.Mode == "export")
                {
                    success = ExportBookmarks(profile, opts.ExportFile, opts.Silent);
                }
                else if (opts.Mode == "set-startup" && !string.IsNullOrEmpty(opts.StartupPage))
                {
                    success = SetStartupPage(profile, opts.StartupPage, opts.Silent);
                }
                else
                {
                    if (opts.Silent == 0)
                    {
                        Console.WriteLine("Invalid mode or missing URL for 'set-startup' mode.");
                    }
                    return;
                }
            }

            if (opts.Silent == 1)
            {
                Console.WriteLine(success ? "true" : "false");
            }
        }

        private static string[] GetBrowserProfiles(string browserPath)
        {
            return Directory.GetDirectories(browserPath, "Profile*", SearchOption.TopDirectoryOnly);
        }

        public static bool ImportBookmarks(string profilePath, string importFilePath, int silent)
        {
            try
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Starting import of bookmarks for profile: {profilePath}");
                }

                string bookmarkFile = Path.Combine(profilePath, "Bookmarks");
                string backupFile = Path.Combine(profilePath, "Bookmarks.bak");

                if (string.IsNullOrEmpty(importFilePath))
                {
                    importFilePath = Path.Combine(profilePath, "ExportedBookmarks.json");
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

                File.WriteAllText(bookmarkFile, existingBookmarks.ToString(Formatting.Indented));
                File.WriteAllText(backupFile, existingBookmarks.ToString(Formatting.Indented));

                if (silent == 0)
                {
                    Console.WriteLine("Bookmarks imported successfully.");
                }

                DeleteBrowserCache(profilePath, silent, "import");

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

        public static bool ExportBookmarks(string profilePath, string exportFilePath, int silent)
        {
            try
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Starting export of bookmarks for profile: {profilePath}");
                }

                string bookmarkFile = Path.Combine(profilePath, "Bookmarks");

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
                    exportFilePath = Path.Combine(profilePath, "ExportedBookmarks.json");
                }

                File.WriteAllText(exportFilePath, bookmarks.ToString(Formatting.Indented));

                if (silent == 0)
                {
                    Console.WriteLine("Bookmarks exported successfully.");
                }

                DeleteBrowserCache(profilePath, silent, "export");

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

        public static bool SetStartupPage(string profilePath, string startupPage, int silent)
        {
            try
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Setting startup page to: {startupPage} for profile: {profilePath}");
                }

                string prefsFile = Path.Combine(profilePath, "Preferences");
                string backupPrefsFile = Path.Combine(profilePath, "Preferences.bak");

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

                // Activer l'option "Ouvrir une page ou un ensemble de pages spécifiques"
                prefs["session"] = prefs["session"] ?? new JObject();
                prefs["session"]["restore_on_startup"] = 4;

                // Ajouter l'URL de démarrage dans "startup_urls"
                if (prefs["session"]["startup_urls"] is JArray startupUrls)
                {
                    // Supprimer les URLs existantes et ajouter la nouvelle URL de démarrage
                    startupUrls.Clear();
                    startupUrls.Add(startupPage);
                }
                else
                {
                    prefs["session"]["startup_urls"] = new JArray { startupPage };
                }

                // Modifier également la page d'accueil
                prefs["homepage"] = startupPage;
                prefs["homepage_is_newtabpage"] = false;

                // Écrire les modifications dans le fichier de préférences principal
                File.WriteAllText(prefsFile, prefs.ToString(Formatting.Indented));
                File.WriteAllText(backupPrefsFile, prefs.ToString(Formatting.Indented));

                if (silent == 0)
                {
                    Console.WriteLine("Startup page set successfully.");
                }

                DeleteBrowserCache(profilePath, silent, "set-startup");

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


        private static void DeleteBrowserCache(string profilePath, int silent, string mode)
        {
            try
            {
                string cachePath = Path.Combine(profilePath, "Cache");
                string codeCachePath = Path.Combine(profilePath, "Code Cache");

                if (Directory.Exists(cachePath))
                {
                    Directory.Delete(cachePath, true);
                    if (silent == 0)
                    {
                        Console.WriteLine($"Cache deleted for mode: {mode} in profile: {profilePath}");
                    }
                }

                if (Directory.Exists(codeCachePath))
                {
                    Directory.Delete(codeCachePath, true);
                    if (silent == 0)
                    {
                        Console.WriteLine($"Code Cache deleted for mode: {mode} in profile: {profilePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                if (silent == 0)
                {
                    Console.WriteLine($"Error deleting cache for mode: {mode} in profile: {profilePath}, Error: {ex.Message}");
                }
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
