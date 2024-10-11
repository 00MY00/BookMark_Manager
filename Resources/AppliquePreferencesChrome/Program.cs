using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Newtonsoft.Json.Linq;

class ChromePreferencesValidator
{
    static void Main(string[] args)
    {
        string preferencesPath = @"path\to\Preferences"; // Chemin vers le fichier Preferences
        string seedPath = @"C:\Program Files (x86)\Google\Chrome\Application\ChromeVersion\resources.pak"; // Chemin vers le fichier seed

        try
        {
            // Charger le fichier Preferences
            string preferencesContent = File.ReadAllText(preferencesPath);
            JObject preferencesJson = JObject.Parse(preferencesContent);

            // Obtenir les éléments nécessaires
            string machineId = GetMachineId();
            string seed = GetHmacSeed(seedPath);

            // Mettre à jour les HMAC dans le fichier Preferences
            UpdateHmacs(preferencesJson, machineId, seed);

            // Sauvegarder les changements
            File.WriteAllText(preferencesPath, preferencesJson.ToString());

            Console.WriteLine("Le fichier Preferences a été revalidé avec succès.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Une erreur s'est produite : {ex.Message}");
        }
        finally
        {
            // Attendre 20 secondes avant de fermer le programme
            Thread.Sleep(20000);
        }
    }

    static string GetMachineId()
    {
        // Simule l'obtention de machine ID (cela doit être adapté en fonction des besoins spécifiques)
        string machineSid = "S-1-5-21-1650828501-840997873-2917006960";
        string volumeId = "1551496638";
        string machineId = ComputeSha1Hash(machineSid) + volumeId;
        machineId += ComputeCrc8(machineId);
        return machineId;
    }

    static string GetHmacSeed(string seedPath)
    {
        // Lire le fichier seed et obtenir la première ressource de 64 caractères
        string seed = File.ReadAllText(seedPath);
        return seed.Substring(0, 64); // Exemple de découpe
    }

    static void UpdateHmacs(JObject preferencesJson, string machineId, string seed)
    {
        foreach (var entry in preferencesJson["protection"]["macs"])
        {
            string jsonPath = entry.Path;
            string jsonContent = entry.ToString();
            string hmac = GenerateHmac(machineId, jsonPath, jsonContent, seed);
            preferencesJson["protection"]["macs"][jsonPath] = hmac;
        }

        // Mettre à jour le super_mac
        string superMacContent = preferencesJson["protection"]["macs"].ToString();
        string superMac = GenerateHmac(machineId, string.Empty, superMacContent, seed);
        preferencesJson["protection"]["super_mac"] = superMac;
    }

    static string GenerateHmac(string machineId, string jsonPath, string jsonContent, string seed)
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

    static string ComputeSha1Hash(string input)
    {
        using (SHA1 sha1 = SHA1.Create())
        {
            byte[] hashBytes = sha1.ComputeHash(Encoding.UTF8.GetBytes(input));
            return BitConverter.ToString(hashBytes).Replace("-", string.Empty);
        }
    }

    static string ComputeCrc8(string input)
    {
        byte crc = 0;
        foreach (byte b in Encoding.UTF8.GetBytes(input))
        {
            crc ^= b;
        }
        return crc.ToString("X2");
    }
}
