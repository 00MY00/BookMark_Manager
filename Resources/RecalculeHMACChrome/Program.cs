using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;
using Microsoft.Win32;
using System.Security.Principal;
using System.Runtime.InteropServices;
using System.Collections.Generic;

class RecalculeHMACChrome
{
    [DllImport("advapi32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool LookupAccountName(string lpSystemName, string lpAccountName, IntPtr Sid, ref uint cbSid, StringBuilder ReferencedDomainName, ref uint cchReferencedDomainName, out int peUse);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    public static extern bool GetVolumeInformation(
        string rootPathName,
        StringBuilder volumeNameBuffer,
        uint volumeNameSize,
        out uint volumeSerialNumber,
        out uint maximumComponentLength,
        out uint fileSystemFlags,
        StringBuilder fileSystemNameBuffer,
        uint fileSystemNameSize);

    static void Main(string[] args)
    {
        if (args.Length < 3)
        {
            Console.WriteLine("Usage: RecalculeHMACChrome.exe --preferences <path to browser preferences> --securepreferences <path to secure preferences> --seed <path to seed file>");
            return;
        }

        string preferencesPath = string.Empty;
        string securePreferencesPath = string.Empty;
        string seedPath = string.Empty;

        for (int i = 0; i < args.Length; i++)
        {
            if (args[i] == "--preferences" && i + 1 < args.Length)
            {
                preferencesPath = args[i + 1];
            }
            else if (args[i] == "--securepreferences" && i + 1 < args.Length)
            {
                securePreferencesPath = args[i + 1];
            }
            else if (args[i] == "--seed" && i + 1 < args.Length)
            {
                seedPath = args[i + 1];
            }
        }

        Console.WriteLine($"Preferences Path: {preferencesPath}");
        Console.WriteLine($"Secure Preferences Path: {securePreferencesPath}");
        Console.WriteLine($"Seed Path: {seedPath}");

        if (!File.Exists(preferencesPath) || !File.Exists(securePreferencesPath) || !File.Exists(seedPath))
        {
            Console.WriteLine("One or more files not found. Exiting...");
            return;
        }

        Console.WriteLine("Files found, starting HMAC calculation...");
        RecalculateAndCompareHMACs(preferencesPath, securePreferencesPath, seedPath);
    }

    public static void RecalculateAndCompareHMACs(string preferencesPath, string securePreferencesPath, string seedPath)
    {
        try
        {
            string preferencesContent = File.ReadAllText(preferencesPath);
            JObject preferencesJson = JObject.Parse(preferencesContent);

            string securePreferencesContent = File.ReadAllText(securePreferencesPath);
            JObject securePreferencesJson = JObject.Parse(securePreferencesContent);

            string machineId = GetMachineId();
            string seed = ExtractSeedFromPak(seedPath);

            Console.WriteLine("\nComparing calculated and stored HMACs:");

            foreach (var property in securePreferencesJson["protection"]["macs"].Children<JProperty>())
            {
                string key = property.Name;
                JToken storedHmacToken = property.Value;

                // Retrieve corresponding value in preferences
                JToken preferenceValue = preferencesJson.SelectToken(key);

                if (preferenceValue != null)
                {
                    string jsonContent = CleanJsonContent(preferenceValue.ToString());

                    string recalculatedHmac = GenerateHmac(machineId, key, jsonContent, seed);
                    string storedHmac = storedHmacToken.ToString();

                    Console.WriteLine($"Key: {key}");
                    Console.WriteLine($"Calculated HMAC: {recalculatedHmac}");
                    Console.WriteLine($"Stored HMAC: {storedHmac}");
                    Console.WriteLine($"Result: {(recalculatedHmac == storedHmac ? "Identical" : "Different")}\n");
                }
                else
                {
                    Console.WriteLine($"Key: {key} - Value not found in Preferences.\n");
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
        }
    }

    private static string GetMachineId()
    {
        string machineSid = GetMachineSid();
        string volumeId = GetVolumeId();
        string machineId = ComputeSha1Hash(machineSid) + volumeId;
        machineId += ComputeCrc8(machineId);
        return machineId;
    }

    private static string GetMachineSid()
    {
        string machineName = Environment.MachineName;
        uint sidSize = 0;
        uint domainNameSize = 0;
        StringBuilder domainName = new StringBuilder();
        int peUse;

        LookupAccountName(null, machineName, IntPtr.Zero, ref sidSize, domainName, ref domainNameSize, out peUse);
        IntPtr sidPtr = Marshal.AllocHGlobal((int)sidSize);
        domainName = new StringBuilder((int)domainNameSize);

        if (!LookupAccountName(null, machineName, sidPtr, ref sidSize, domainName, ref domainNameSize, out peUse))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        SecurityIdentifier sid = new SecurityIdentifier(sidPtr);
        Marshal.FreeHGlobal(sidPtr);
        return sid.Value;
    }

    private static string GetVolumeId()
    {
        uint serialNumber, maxComponentLen, fileSystemFlags;
        StringBuilder volumeName = new StringBuilder(256);
        StringBuilder fileSystemName = new StringBuilder(256);

        if (!GetVolumeInformation(@"C:\", volumeName, (uint)volumeName.Capacity, out serialNumber, out maxComponentLen, out fileSystemFlags, fileSystemName, (uint)fileSystemName.Capacity))
        {
            throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
        }

        return serialNumber.ToString();
    }

    private static string ExtractSeedFromPak(string seedPath)
    {
        using (FileStream fs = new FileStream(seedPath, FileMode.Open, FileAccess.Read))
        {
            byte[] buffer = new byte[64];
            fs.Seek(0, SeekOrigin.Begin);
            fs.Read(buffer, 0, 64);
            return BitConverter.ToString(buffer).Replace("-", "");
        }
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

    private static string CleanJsonContent(string jsonContent)
    {
        jsonContent = jsonContent.Replace("<", "\\u003C");
        return jsonContent;
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
