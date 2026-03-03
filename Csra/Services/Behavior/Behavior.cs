using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra.Services {

    /// <summary>
    /// BehaviorService - when logic gets a personality.
    /// </summary>
    [Serializable]
    public class Behavior : IBehaviorService {

        private static IBehaviorService _instance = null;
        private readonly Type[] _supportedTypes;
        private readonly Dictionary<string, object> _behaviors;

        protected Behavior() {
            _supportedTypes = [typeof(int), typeof(double), typeof(bool), typeof(string)];
            _behaviors = [];
            FilePath = "";
        }

        public static IBehaviorService Instance => _instance ??= new Behavior();

        public string FilePath { get; set; }

        public IEnumerable<string> Features => _behaviors.Keys;

        public void Export(string filePath = "") {
            if (!string.IsNullOrWhiteSpace(filePath)) FilePath = filePath;
            if (string.IsNullOrWhiteSpace(FilePath)) throw new ArgumentException("File path is empty, please specify or set the property.", nameof(filePath));
            StringBuilder sb = new();
            foreach (KeyValuePair<string, object> kvp in _behaviors) {
                object value = kvp.Value;
                foreach (Type type in _supportedTypes) {
                    if (type.IsInstanceOfType(value)) {
                        sb.AppendLine($"{kvp.Key}|{type.Name}|{value}");
                        break;
                    }
                }
            }
            File.WriteAllText(FilePath, sb.ToString());
        }

        public T GetFeature<T>(string feature) {
            if (string.IsNullOrEmpty(feature)) throw new ArgumentException("Feature name cannot be empty string.");
            if (feature.IndexOfAny(['|', '\n', '\r']) >= 0) throw new ArgumentException("Feature names cannot contain the '|', NL or CR character.",
              nameof(feature));
            if (!_supportedTypes.Contains(typeof(T))) throw TypeNotSupported<T>(feature);
            if (_behaviors.TryGetValue(feature, out object boxed)) {
                if (boxed is T unboxed) return unboxed;
                throw new InvalidCastException($"Value for feature '{feature}' is type '{boxed.GetType().Name}', and not of the requested type " +
                    $"'{typeof(T).Name}'.");
            }
            return NonNullDefault<T>(feature);
        }

        public void Import(string filePath = "") {
            if (!string.IsNullOrWhiteSpace(filePath)) FilePath = filePath;
            if (string.IsNullOrWhiteSpace(FilePath)) throw new ArgumentException("File path is empty, please specify or set the property.", nameof(filePath));
            if (!File.Exists(FilePath)) throw new FileNotFoundException($"The file '{FilePath}' does not exist.");
            foreach (string line in File.ReadAllLines(FilePath)) {
                if (!string.IsNullOrWhiteSpace(line)) {
                    string[] parts = line.Split('|');
                    if (parts.Length >= 3) {
                        string feature = parts[0].Trim();
                        switch (parts[1].Trim()) {
                            case "Int32":
                                SetFeature(feature, int.Parse(parts[2]));
                                break;
                            case "Double":
                                SetFeature(feature, double.Parse(parts[2]));
                                break;
                            case "Boolean":
                                SetFeature(feature, bool.Parse(parts[2]));
                                break;
                            case "String":
                                SetFeature(feature, string.Join("|", parts.Skip(2)).Trim()); // in case the value contains '|', we rejoin the rest of the parts
                                break;
                            default:
                                throw TypeNotSupported<object>(feature, parts[1].Trim()); // fallback - handle unsupported types, we should never get here
                        }
                    } else throw new FormatException($"Line '{line}' in file '{FilePath}' is not in the expected format. Expected format: " +
                        $"'FeatureName|Type|Value'.");
                }
            }
        }

        public void Reset() {
            _behaviors.Clear();
            FilePath = "";
        }

        public void SetFeature<T>(string feature, T value) {
            if (string.IsNullOrEmpty(feature)) throw new ArgumentException("Feature name cannot be empty string.");
            if (feature.IndexOfAny(['|', '\n', '\r']) >= 0) throw new ArgumentException("Feature names cannot contain the '|', NL or CR character.",
                nameof(feature));
            if (!_supportedTypes.Contains(typeof(T))) throw TypeNotSupported<T>(feature);
            if (value is null) throw new ArgumentNullException(nameof(value), "Value cannot be null.");
            if (value is string strValue) _behaviors[feature] = strValue.Trim(); // ensure strings are trimmed, to avoid issues with leading/trailing spaces
            else _behaviors[feature] = value;
        }
        private static T NonNullDefault<T>(string feature) {
            if (typeof(T) == typeof(string)) return (T)(object)string.Empty; // default(string) would be null
            return default;
        }

        private NotSupportedException TypeNotSupported<T>(string feature, string typeOverride = null) => new($"Feature '{feature}' - type " +
            $"'{typeOverride ?? typeof(T).Name}' is not supported. Only '{string.Join(", ", _supportedTypes.Select(t => t.Name))}' are allowed.");
    }
}
