using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;

class AppliquePreferencesChrome
{
    static void Main(string[] args)
    {
        if (args.Length < 2)
        {
            Console.WriteLine("Usage: AppliquePreferencesChrome.exe --preferences <path to browser preferences> --seed <path to seed file>");
            return;
        }

        string preferencesPath = string.Empty;
        string seedPath = string.Empty;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--preferences" && i + 1 < args.Length)
            {
                preferencesPath = args[i + 1];
            }
            else if (args[i] == "--seed" && i + 1 < args.Length)
            {
                seedPath = args[i + 1];
            }
        }

        if (string.IsNullOrEmpty(preferencesPath) || string.IsNullOrEmpty(seedPath))
        {
            Console.WriteLine("Chemins invalides. Veuillez vérifier les arguments.");
            return;
        }

        Console.WriteLine($"Preferences Path: {preferencesPath}");
        Console.WriteLine($"Seed Path: {seedPath}");

        bool success = UpdatePreferencesAndRegistry(preferencesPath, seedPath);

        Console.WriteLine(success ? "Mise à jour des HMACs réussie." : "La mise à jour des HMACs a échoué.");
    }

    public static bool UpdatePreferencesAndRegistry(string preferencesPath, string seedPath)
    {
        try
        {
            if (!File.Exists(preferencesPath) || !File.Exists(seedPath))
            {
                Console.WriteLine("Fichier de préférences ou fichier de seed manquant.");
                return false;
            }

            string preferencesContent = File.ReadAllText(preferencesPath);
            JObject preferencesJson = JObject.Parse(preferencesContent);

            string machineId = GetMachineId();
            string seed = GetHmacSeed(seedPath);

            using (RegistryKey defaultKey = Registry.CurrentUser.OpenSubKey(@"Software\Google\Chrome\PreferenceMACs\Default", true))
            {
                if (defaultKey == null)
                {
                    Console.WriteLine("Clé de registre PreferenceMACs\\Default non trouvée.");
                    return false;
                }

                string[] keysToUpdate = new string[]
                {
                    "browser.show_home_button",
                    "default_search_provider_data.template_url_data",
                    "enterprise_signin.policy_recovery_token",
                    "google.services.account_id",
                    "google.services.last_account_id",
                    "google.services.last_signed_in_username",
                    "google.services.last_username",
                    "homepage",
                    "homepage_is_newtabpage",
                    "media.cdm.origin_data",
                    "media.storage_id_salt",
                    "pinned_tabs",
                    "prefs.preference_reset_time",
                    "safebrowsing.incidents_sent",
                    "search_provider_overrides",
                    "session.restore_on_startup",
                    "session.startup_urls"
                };

                foreach (string key in keysToUpdate)
                {
                    // Accès au token en utilisant les guillemets pour les clés contenant des points
                    var token = preferencesJson.SelectToken($"['{key.Replace(".", "']['")}']");
                    if (token != null)
                    {
                        string jsonContent = token.ToString();
                        string hmac = GenerateHmac(machineId, key, jsonContent, seed);

                        // Mise à jour du fichier Preferences
                        token.Replace(jsonContent);

                        // Mise à jour du registre
                        defaultKey.SetValue(key, hmac);
                        Console.WriteLine($"Mise à jour de la clé {key} avec la valeur HMAC {hmac}");
                    }
                    else
                    {
                        Console.WriteLine($"Clé {key} non trouvée dans les préférences.");
                    }
                }
            }

            // Mise à jour du fichier Preferences avec les nouvelles valeurs HMAC
            File.WriteAllText(preferencesPath, preferencesJson.ToString());

            // Mise à jour de super_mac
            using (RegistryKey mainKey = Registry.CurrentUser.OpenSubKey(@"Software\Google\Chrome\PreferenceMACs", true))
            {
                if (mainKey != null)
                {
                    string superMacContent = preferencesJson.ToString();
                    string superMac = GenerateHmac(machineId, string.Empty, superMacContent, seed);
                    mainKey.SetValue("super_mac", superMac);
                    Console.WriteLine($"Mise à jour de la clé super_mac avec la valeur {superMac}");
                }
            }

            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
            return false;
        }
    }

    private static string GetMachineId()
    {
        string machineSid = "S-1-5-21-1650828501-840997873-2917006960";
        string volumeId = "1551496638";
        string machineId = ComputeSha1Hash(machineSid) + volumeId;
        machineId += ComputeCrc8(machineId);
        return machineId;
    }

    private static string GetHmacSeed(string seedPath)
    {
        byte[] seedBytes = File.ReadAllBytes(seedPath);
        string seedHex = BitConverter.ToString(seedBytes, 0, 64).Replace("-", string.Empty);
        return seedHex;
    }

    private static string GenerateHmac(string machineId, string jsonPath, string jsonContent, string seed)
    {
        string message = machineId + jsonPath + jsonContent;
        byte[] key = Encoding.UTF8.GetBytes(seed);
        byte[] messageBytes = Encoding.UTF8.GetBytes(message);

        using (HMACSHA256 hmac = new HMACSHA256(key))
        {
            byte[] hashBytes = hmac.ComputeHash(messageBytes);
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }

    private static string ComputeSha1Hash(string input)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }

    private static string ComputeCrc8(string input)
    {
        byte crc = 0;
        foreach (byte b in Encoding.UTF8.GetBytes(input))
        {
            crc ^= b;
        }
        return crc.ToString("X2");
    }
}
