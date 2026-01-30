using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Csra.Services;
using Microsoft.Win32;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra {
    public class IGComponentManagerParser {
        public IGComponentManagerParser(string componentManagerFile) {
            if (componentManagerFile == null)
                Alert.Instance.Error<ArgumentNullException>("Missing IG-ComponentManager file");
            if (!System.IO.File.Exists(componentManagerFile))
                Alert.Instance.Error<System.IO.FileNotFoundException>($"IG-ComponentManager file not found: {componentManagerFile}");

           
            string jsonContent = System.IO.File.ReadAllText(componentManagerFile);
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var data = JsonSerializer.Deserialize<ComponentManagerData>(jsonContent, options);

            if (data?.Configurations != null) {
                foreach (var config in data.Configurations) {
                    if (config.Items != null) {
                        foreach (var item in config.Items) {
                            Alert.Instance.Log($"IG-Component Item: Publisher={item.Publisher}, Name={item.Name}, Require Version={item.Version}");
                            if (item.Verification == "Default" && item.Publisher == "Teradyne")
                                TeradyneComponentVerification(item);
                            else {
                                if (item.Verification == "ByRegistry") {                                    
                                    VersionCompare(item.Version, RegistryKeyExists(item.Location));                                    
                                } else if (item.Verification == "ByAssembly") {
                                    string assemblyVersion = AssemblyName.GetAssemblyName(item.Location).Version.ToString();
                                    VersionCompare(item.Version, assemblyVersion);
                                } else if (item.Verification == "ByFile") {
                                    if (File.Exists(item.Location))
                                        VersionCompare(item.Version, FileVersionInfo.GetVersionInfo(item.Location).ProductVersion);                                    
                                } else {
                                    Alert.Instance.Error<ArgumentException>($"IG-ComponentManagerParser Unsupported request");
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TeradyneComponentVerification(Item item) {                     
            switch (item.Name) {
                case "IG-XL":
                    VerifyIGXLVersion(item);
                    break;
                case "Oasis":
                    VerifyOasisVersion(item);
                    break;
                case "SSL":
                    VerifySslVersion(item);
                    break;
                case "Version Selector Plus":
                    VerifyVSPlusVersion(item);
                    break;
                default:
                    Alert.Instance.Warning($"No verification method defined for Teradyne component: {item.Name}");
                    break;  
            }
        }

        private string RegistryKeyExists(string fullPath) {
            // Split the root key from the subkey path
            string[] parts = fullPath.Split(new char[] { '\\' }, 2);
            if (parts.Length != 2) {
                Alert.Instance.Warning("Invalid registry path format.");
                return null;
            }

            RegistryKey baseKey = null;
            switch (parts[0].ToUpper()) {
                case "HKEY_LOCAL_MACHINE":
                    baseKey = Registry.LocalMachine;
                    break;
                case "HKEY_CURRENT_USER":
                    baseKey = Registry.CurrentUser;
                    break;
                case "HKEY_CLASSES_ROOT":
                    baseKey = Registry.ClassesRoot;
                    break;
                case "HKEY_USERS":
                    baseKey = Registry.Users;
                    break;
                case "HKEY_CURRENT_CONFIG":
                    baseKey = Registry.CurrentConfig;
                    break;
                default:
                    throw new ArgumentException("Unknown registry root: " + parts[0]);
            }

            using (RegistryKey subKey = baseKey.OpenSubKey(parts[1])) {
                if (subKey != null) {
                    return fullPath.Substring(fullPath.LastIndexOf('\\') + 1);
                }
                return null;
            }
        }
       
        private void VerifyVSPlusVersion(Item item) {
            string vsPlusPath = Environment.GetEnvironmentVariable("VSPLUS");
            string vsPlusVersion = string.Empty;

            if (!string.IsNullOrEmpty(vsPlusPath)) {
                string versionFile = System.IO.Path.Combine(vsPlusPath, "VSPVersion.txt");
                if (System.IO.File.Exists(versionFile)) {
                   
                    string[] lines = System.IO.File.ReadAllLines(versionFile);

                    if (lines[0] != null) {
                        vsPlusVersion = lines[0].Split(new[] { ':' }, 2)[1].Trim();
                    }
                }
                VersionCompare(item.Version, vsPlusVersion);
            }
        }

        private void VerifySslVersion(Item item) {
            string sslPath = Environment.GetEnvironmentVariable("SSLROOT");
            string sslVersion = string.Empty;

            if (!string.IsNullOrEmpty(sslPath)) {
                string versionFile = System.IO.Path.Combine(sslPath, "Version.txt");
                if (System.IO.File.Exists(versionFile)) {
                    sslVersion = System.IO.File.ReadAllText(versionFile).Trim();
                }
                VersionCompare(item.Version, sslVersion);
            }
        }
        
        private void VerifyIGXLVersion(Item item) {
            string igxlRoot = Environment.GetEnvironmentVariable("IGXLROOT");
            if (!string.IsNullOrEmpty(igxlRoot)) {
                string version = System.IO.Path.GetFileName(igxlRoot.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                VersionCompare(item.Version, version);                
            }
        }
        private void VerifyOasisVersion(Item item) {
            string oasisVersion = Environment.GetEnvironmentVariable("OASIS_VERSION");
            if (!string.IsNullOrEmpty(oasisVersion)) {
                VersionCompare(item.Version, oasisVersion); 
            }
        }

        // Extracts the leading numeric part from a segment (e.g., "03_uflx" -> 3)
        private int GetNumericPrefix(string s) {
            var match = Regex.Match(s, @"^\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        // Compares two version strings and throws an error if they do not meet the specified operand condition         
        private void VersionCompare(string expected, string found) {

            //string can be 11.00 or 11.00.00 or 11.00.00-11.00.11 each has different usage
            string[] v1parts = expected.Split(new char[] { '.', '-', ' ' }, StringSplitOptions.RemoveEmptyEntries);
            string[] v2parts = found.Split('.');

            int length = Math.Min(v1parts.Length, v2parts.Length);
            
            for (int i = 0; i < length; i++) {
                int num1 = i < v1parts.Length ? GetNumericPrefix(v1parts[i]) : 0;
                int num2 = i < v2parts.Length ? GetNumericPrefix(v2parts[i]) : 0;

                //Minimun 2 digit version is like expected <= found
                if (v1parts.Length == 2) {
                    if (num1 > num2) {
                        Alert.Instance.Error<ArgumentException>($"Version mismatch: expected {expected}, found {found}");
                    }
                }
                //Minimun 3 digit version is like expected == found
                else if (v1parts.Length == 3) {
                    if (num1 != num2) {
                        Alert.Instance.Error<ArgumentException>($"Version mismatch: expected {expected}, found {found}");
                    }
                }
                else if (v1parts.Length >= 5) {
                    int num3 = i + 3 < v1parts.Length ? GetNumericPrefix(v1parts[i + 3]) : 0;
                    if(!(num1 <= num2 && num2 <= num3))
                    {
                        Alert.Instance.Error<ArgumentException>($"Version mismatch: expected {expected}, found {found}");
                    }
                }             
            }            
        }       

        private class ComponentManagerData {
            public string TestProgram { get; set; }
            public string TpChecksum { get; set; }
            public List<object> Licenses { get; set; }
            public List<Configuration> Configurations { get; set; }
        }

        private class Configuration {
            public string ID { get; set; }
            public List<Item> Items { get; set; }
        }

        private class Item {
            public string Publisher { get; set; }
            public string Name { get; set; }
            public string Version { get; set; }
            public string Notes { get; set; }
            public string Verification { get; set; }
            public string Location { get; set; }
        }
    }
}
