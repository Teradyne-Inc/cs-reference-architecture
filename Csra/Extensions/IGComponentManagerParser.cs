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
            ValidateJsonStructure(jsonContent, componentManagerFile);
            ComponentManagerData data = JsonSerializer.Deserialize<ComponentManagerData>(jsonContent, options);
            if (data?.Configurations != null) {
                foreach (var config in data.Configurations) {
                    if (config.Items != null) {
                        foreach (var item in config.Items) {
                            Alert.Instance.Log($"IG-Component Item: Publisher={item.Publisher}, Name={item.Name}, Require Version={item.Version}");
                            //if (item.Verification == "Default" && item.Publisher == "Teradyne")
                            TeradyneComponentVerification(item);
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
                case "PortBridge":
                    VerifyPbVersion(item);
                    break;
                case "Visual Studio":
                    VerifyVisualStudioVersion(item);
                    break;

                case "TEMS":
                case "APM":
                case "TimeLines":
                case "ESA Toolkit":
                case "Excel":
                case "C#":
                default:
                    Alert.Instance.Warning($"No verification method defined for Teradyne component: {item.Name}");
                    break;
            }
        }


        private void VerifyVisualStudioVersion(Item item) {
            string vswherePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86),
                @"Microsoft Visual Studio\Installer\vswhere.exe");

            if (!File.Exists(vswherePath))
                throw new FileNotFoundException("vswhere.exe not found. Please ensure Visual Studio 2022 or later is installed.", vswherePath);

            ProcessStartInfo startInfo = new() {
                FileName = vswherePath,
                Arguments = "-version [17.0,18.0) -latest -property installationVersion",
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo);
            string version = process.StandardOutput.ReadToEnd().Trim();
            process.WaitForExit();

            VersionCompare(item.Version, version, item.Comparison);

        }

        private void VerifyPbVersion(Item item) {
            string pbPath = Environment.GetEnvironmentVariable("PBROOT");
            string pbVersion = string.Empty;
            if (!string.IsNullOrEmpty(pbPath)) {
                string versionFile = Path.Combine(pbPath, "Teradyne.PortBridge.dll");
                if (System.IO.File.Exists(versionFile)) {
                    pbVersion = FileVersionInfo.GetVersionInfo(versionFile).FileVersion;
                }
                VersionCompare(item.Version, pbVersion, item.Comparison);
            }
        }

        private void VerifyVSPlusVersion(Item item) {

            string vsPlusVersion = string.Empty;
            string file = Environment.ExpandEnvironmentVariables(@"%VSPLUS%VSPVersion.txt");

            if (File.Exists(file)) {
                string[] lines = File.ReadAllLines(file);

                if (lines.Length > 0 && !string.IsNullOrWhiteSpace(lines[0])) {
                    string[] parts = lines[0].Split(new[] { ':' }, 2);
                    if (parts.Length == 2) {
                        vsPlusVersion = parts[1].Trim();
                    }
                }
                VersionCompare(item.Version, vsPlusVersion, item.Comparison);
            } else {
                Alert.Instance.Error<FileNotFoundException>($"Version Selector Plus version file not found: '{file}' (check that the VSPLUS environment variable is set correctly)");
            }
        }

        private void VerifySslVersion(Item item) {
            string sslPath = Environment.GetEnvironmentVariable("SSLROOT");
            string sslVersion = string.Empty;
            if (!string.IsNullOrEmpty(sslPath)) {
                string versionFile = Path.Combine(sslPath, "TerSSLMonitor.exe");
                if (System.IO.File.Exists(versionFile)) {
                    sslVersion = FileVersionInfo.GetVersionInfo(versionFile).ProductVersion;
                }
                VersionCompare(item.Version, sslVersion, item.Comparison);
            } else {
                Alert.Instance.Error<ArgumentException>("SSLROOT environment variable is not set, cannot verify SSL version");
            }
        }

        private void VerifyIGXLVersion(Item item) {
            string igxlRoot = Environment.GetEnvironmentVariable("IGXLROOT");
            if (!string.IsNullOrEmpty(igxlRoot)) {
                string version = System.IO.Path.GetFileName(igxlRoot.TrimEnd(System.IO.Path.DirectorySeparatorChar));
                VersionCompare(item.Version, version, item.Comparison);
            } else {
                Alert.Instance.Error<ArgumentException>("IGXLROOT environment variable is not set, cannot verify IG-XL version");
            }
        }
        private void VerifyOasisVersion(Item item) {
            string oasisVersion = Environment.GetEnvironmentVariable("OASIS_VERSION");
            if (!string.IsNullOrEmpty(oasisVersion)) {
                VersionCompare(item.Version, oasisVersion, item.Comparison);
            } else {
                Alert.Instance.Error<ArgumentException>("OASIS_VERSION environment variable is not set, cannot verify Oasis version");
            }
        }

        // Extracts the leading numeric part from a segment (e.g., "03_uflx" -> 3)
        private int GetNumericPrefix(string s) {
            var match = Regex.Match(s, @"^\d+");
            return match.Success ? int.Parse(match.Value) : 0;
        }

        // Removes any non-numeric suffix from each segment of a version string (e.g., "11.00.07_uflx" → "11.00.07")
        private string CleanVersionString(string version) {
            return Regex.Replace(version, @"[^0-9.\-\s]", "").Trim().TrimEnd('.');
        }

        // Compares two version strings and throws an error if they do not meet the specified operand condition         
        private void VersionCompare(string expected, string found, string comparison) {
            //expected = CleanVersionString(expected);
            found = CleanVersionString(found);

            string[] range = Regex.Split(expected, @"\s*-\s*(?=\d)");
            string[] loParts = CleanVersionString(range[0]).Split('.');
            string[] hiParts = range.Length > 1 ? CleanVersionString(range[1]).Split('.') : null;
            string[] foundParts = found.Split('.');

            switch (comparison) {
                case "Equal to":
                    if (hiParts != null)
                        Alert.Instance.Error<ArgumentException>("Invalid expected version format for 'Equal to' comparison: " + expected);
                    else
                        if (CompareVersionParts(foundParts, loParts) != 0)
                        Alert.Instance.Error<ArgumentException>($"Version mismatch (Equal to): expected {expected}, found {found}");

                    break;
                case "Minimum":
                case "Greater than or equal to":
                    if (hiParts != null)
                        Alert.Instance.Error<ArgumentException>("Invalid expected version format for 'Greater than or equal to' comparison: " + expected);
                    else if (CompareVersionParts(foundParts, loParts) < 0)
                        Alert.Instance.Error<ArgumentException>($"Version mismatch ({comparison}): expected {expected}, found {found}");
                    break;
                case "Greater than":
                    if (hiParts != null)
                        Alert.Instance.Error<ArgumentException>("Invalid expected version format for 'Greater than' comparison: " + expected);
                    else if (CompareVersionParts(foundParts, loParts) <= 0)
                        Alert.Instance.Error<ArgumentException>($"Version mismatch ({comparison}): expected {expected}, found {found}");
                    break;
                case "Between":
                    if (CompareVersionParts(foundParts, loParts) < 0 || CompareVersionParts(foundParts, hiParts) > 0)
                        Alert.Instance.Error<ArgumentException>($"Version mismatch (Between): expected {loParts} to {hiParts}, found {found}");
                    break;
                default:
                    Alert.Instance.Error<ArgumentException>($"Unknown comparison operator: {comparison}");
                    break;
            }
        }

        // Returns -1 if a < b, 0 if equal, 1 if a > b
        private int CompareVersionParts(string[] a, string[] b) {
            int length = Math.Max(a.Length, b.Length);
            for (int i = 0; i < length; i++) {
                int numA = i < a.Length ? GetNumericPrefix(a[i]) : 0;
                int numB = i < b.Length ? GetNumericPrefix(b[i]) : 0;
                if (numA < numB) return -1;
                if (numA > numB) return 1;
            }
            return 0;
        }

        private static void ValidateJsonStructure(string jsonContent, string filePath) {
            try {
                using (var doc = JsonDocument.Parse(jsonContent)) {
                    var root = doc.RootElement;
                    ValidateKeys<ComponentManagerData>(root, "ComponentManagerData", filePath);
                    if (root.TryGetProperty("Configurations", out JsonElement configs) && configs.ValueKind == JsonValueKind.Array) {
                        foreach (var config in configs.EnumerateArray()) {
                            ValidateKeys<Configuration>(config, "Configuration", filePath);
                            if (config.TryGetProperty("Items", out JsonElement items) && items.ValueKind == JsonValueKind.Array) {
                                foreach (var item in items.EnumerateArray())
                                    ValidateKeys<Item>(item, "Item", filePath);
                            }
                        }
                    }
                }
            } catch (JsonException ex) {
                Alert.Instance.Error<JsonException>($"IG-ComponentManager JSON is invalid: {ex.Message}");
            }
        }

        private static void ValidateKeys<T>(JsonElement element, string className, string filePath) {
            if (element.ValueKind != JsonValueKind.Object) return;
            var jsonKeys = element.EnumerateObject().Select(p => p.Name.ToLower()).ToHashSet();
            var classKeys = typeof(T).GetProperties().Select(p => p.Name.ToLower()).ToHashSet();
            var missingInClass = jsonKeys.Except(classKeys).ToList();
            var missingInJson = classKeys.Except(jsonKeys).ToList();
            var errors = new List<string>();
            if (missingInClass.Count > 0)
                errors.Add($"JSON has properties not in '{className}': {string.Join(", ", missingInClass)}");
            if (missingInJson.Count > 0)
                errors.Add($"'{className}' has properties not in JSON: {string.Join(", ", missingInJson)}");
            if (errors.Count > 0)
                Alert.Instance.Error<ArgumentException>($"IG-ComponentManager structure mismatch in {filePath} - {string.Join("; ", errors)}");
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
            public string Comparison { get; set; }
            public string Version { get; set; }
            public string Notes { get; set; }
            public string Verification { get; set; }
            public string Location { get; set; }
        }
    }
}
