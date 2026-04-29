using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store information about a pattern.
    /// </summary>
    [Serializable]
    public class PatternInfo {

        private bool? _threadingEnabled = null;
        private string _timeDomain = null;
        private int _setFlags = (int)CpuFlag.A;
        private int _clearFlags = 0;

        public const int MaxFlags = (int)CpuFlag.A + (int)CpuFlag.B + (int)CpuFlag.C + (int)CpuFlag.D;

        /// <summary>
        /// Construct a new <see cref="PatternInfo"/> object and load the specified pattern.
        /// </summary>
        /// <param name="pattern">Specifies the pattern to load. This can be a pattern module name, pattern file, pattern set name, or a combination of these
        /// items.</param>
        /// <param name="threading">Indicate whether threading should be used. Validation will fail if threading is not supported by the pattern.</param>
        public PatternInfo(string pattern, bool threading) {
            Name = pattern;
            TheHdw.Patterns(Name).Load();
            ThreadingEnabled = threading;
        }

        /// <summary>
        /// Construct a new <see cref="PatternInfo"/> object and load the specified pattern.
        /// </summary>
        /// <param name="pattern">Specifies the pattern to load. This can be a pattern module name, pattern file, pattern set name, or a combination of these
        /// items.</param>
        /// <param name="threading">Indicate whether threading should be used. Validation will fail if threading is not supported by the pattern.</param>
        public PatternInfo(Pattern pattern, bool threading) : this(pattern.Value, threading) { }

        /// <summary>
        /// The name of the pattern setup in IGXL.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Modifies vector block data for the specified pins in the given pattern module. The data is expected in vector-major order,
        /// where each element represents a vector: { "P1P2P3P4", "P1P2P3P4", ... }.
        /// </summary>
        /// <param name="pins">The pins to modify.</param>
        /// <param name="patternModule">The pattern module containing the vector block.</param>
        /// <param name="label">The label identifying the vector block.</param>
        /// <param name="offset">The offset within the vector block.</param>
        /// <param name="pinDataArray">Vector-major data array passed by reference.</param>
        public void ModifyVectorBlockData(Pins pins, string patternModule, string label, int offset, ref string[] pinDataArray) {
            TheHdw.Digital.Pins(pins.ToString()).Patterns(patternModule).ModifyVectorBlockData(label, offset, ref pinDataArray);
        }

        /// <summary>
        /// Modifies vector block data for the specified pins using a single string where each character represents one vector.
        /// The string is split into individual characters before being sent to IGXL.
        /// </summary>
        /// <param name="pins">The pins to modify.</param>
        /// <param name="patternModule">The pattern module containing the vector block.</param>
        /// <param name="label">The label identifying the vector block.</param>
        /// <param name="offset">The offset within the vector block.</param>
        /// <param name="pinData">A string where each character is a vector entry.</param>
        public void ModifyVectorBlockDataPinOrder(Pins pins, string patternModule, string label, int offset, ref string pinData) {           
            string[] dataArray = pinData.Select(c => c.ToString()).ToArray();
            TheHdw.Digital.Pins(pins.ToString()).Patterns(patternModule).ModifyVectorBlockData(label, offset, ref dataArray);
        }

        /// <summary>
        /// Modifies vector block data for the specified pins using pin-major order. The data is expected as one string per pin,
        /// where each string contains that pin's values across all vectors: { "P1P1P1P1", "P2P2P2P2", ... }.
        /// The data is transposed to vector-major order before being sent to IGXL.
        /// </summary>
        /// <param name="pins">The pins to modify.</param>
        /// <param name="patternModule">The pattern module containing the vector block.</param>
        /// <param name="label">The label identifying the vector block.</param>
        /// <param name="offset">The offset within the vector block.</param>
        /// <param name="pinDataArray">Pin-major data array passed by reference.</param>
        public void ModifyVectorBlockDataPinOrder(Pins pins, string patternModule, string label, int offset, ref string[] pinDataArray) {
            string[] dataArray = TransposePinDataToVectorOrder(pinDataArray);
            TheHdw.Digital.Pins(pins.ToString()).Patterns(patternModule).ModifyVectorBlockData(label, offset, ref dataArray);
        }

        /// <summary>
        /// Transposes pin data from pin-major order to vector-major order.
        /// Input:  { "P1P1P1P1", "P2P2P2P2", ... } (one string per pin, characters are vectors).
        /// Output: { "P1P2...", "P1P2...", ... } (one string per vector, characters are pins).
        /// </summary>
        /// <param name="pinDataArray">Pin-major data where each element holds all vector values for one pin.</param>
        /// <returns>Vector-major data where each element holds all pin values for one vector.</returns>
        /// <exception cref="ArgumentException">Thrown when the input contains null, empty, or inconsistent-length entries.</exception>
        private static string[] TransposePinDataToVectorOrder(string[] pinDataArray) {
            if (pinDataArray is null || pinDataArray.Length == 0) return Array.Empty<string>();
            if (string.IsNullOrEmpty(pinDataArray[0]))
                throw new ArgumentException("Pin data cannot be null or empty.", nameof(pinDataArray));

            int vectorCount = pinDataArray[0].Length;

            for (int i = 0; i < pinDataArray.Length; i++) {
                if (string.IsNullOrEmpty(pinDataArray[i]))
                    throw new ArgumentException("Pin data cannot be null or empty.", nameof(pinDataArray));
                if (pinDataArray[i].Length != vectorCount)
                    throw new ArgumentException("All pin data strings must have the same length.", nameof(pinDataArray));
            }

            int pinCount = pinDataArray.Length;
            string[] result = new string[vectorCount];

            for (int v = 0; v < vectorCount; v++) {
                char[] vector = new char[pinCount];
                for (int p = 0; p < pinCount; p++) {
                    vector[p] = pinDataArray[p][v];
                }
                result[v] = new string(vector);
            }

            return result;
        }

        /// <summary>
        /// Returns the list of labels defined in the specified pattern module. The module must exist in the pattern's
        /// module list; otherwise, IGXL will raise an error.
        /// </summary>        
        /// <param name="module">The name of the pattern module for which to retrieve the defined labels.</param>
        /// <returns>A string array of label names defined in the specified pattern module.</returns>
        public string[] GetModuleLabelList(string module) {
            return TheHdw.Digital.Patterns().Modules[module].DefinedLabels.List;
        }

        /// <summary>
        /// Whether or not the pattern can and will use threading.
        /// </summary>
        public bool ThreadingEnabled {
            get {
                if (_threadingEnabled is null) _threadingEnabled = TheHdw.Patterns(Name).Threading.Enable;
                return _threadingEnabled.Value;
            }
            set {
                TheHdw.Patterns(Name).Threading.Enable = value;
                if (value) TheHdw.Patterns(Name).ValidateThreading();
                _threadingEnabled = value;
            }
        }

        /// <summary>
        /// Use this property to return a string that lists the time domain names associated with the specified PatternSpecification.
        /// If a pattern is not specified, this syntax returns a comma-separated list of all domain names.
        /// </summary>
        public string TimeDomain {
            get {
                if (_timeDomain is null) _timeDomain = TheHdw.Patterns(Name).TimeDomains;
                return _timeDomain;
            }
        }

        /// <summary>
        /// Set flag value for the pattern, default is cpuA (1).
        /// </summary>
        public int SetFlags {
            get => _setFlags;
            set {
                if (value >= 0 && value <= MaxFlags) _setFlags = value;
                else Api.Services.Alert.Error($"Value must be between 0 and '{MaxFlags}' (inclusive).");
            }
        }

        /// <summary>
        /// Clear flag value for the pattern, default is all flags (0).
        /// </summary>
        public int ClearFlags {
            get => _clearFlags;
            set {
                if (value >= 0 && value <= MaxFlags) _clearFlags = value;
                else Api.Services.Alert.Error($"Value must be between 0 and '{MaxFlags}' (inclusive).");
            }
        }

        /// <summary>
        /// Determines whether the specified <see cref="PatternInfo"/> instance is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="PatternInfo"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="PatternInfo"/> has the same public properties; otherwise, <c>false</c>.</returns>
        public bool Equals(PatternInfo other) {
            if (other is null) return false;
            return Name == other.Name
            && ThreadingEnabled == other.ThreadingEnabled
            && TimeDomain == other.TimeDomain
            && SetFlags == other.SetFlags
            && ClearFlags == other.ClearFlags;
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="PatternInfo"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="PatternInfo"/> and has the same public properties; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj) => Equals(obj as PatternInfo);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="PatternInfo"/>.</returns>
        public override int GetHashCode() {
            int hash = 17;
            hash = hash * 23 + (Name?.GetHashCode() ?? 0);
            hash = hash * 23 + ThreadingEnabled.GetHashCode();
            hash = hash * 23 + (TimeDomain?.GetHashCode() ?? 0);
            hash = hash * 23 + SetFlags.GetHashCode();
            hash = hash * 23 + ClearFlags.GetHashCode();
            return hash;
        }

        /// <summary>
        /// Determines whether two <see cref="PatternInfo"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="PatternInfo"/> to compare.</param>
        /// <param name="right">The second <see cref="PatternInfo"/> to compare.</param>
        /// <returns><c>true</c> if both instances are equal or both are <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(PatternInfo left, PatternInfo right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="PatternInfo"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="PatternInfo"/> to compare.</param>
        /// <param name="right">The second <see cref="PatternInfo"/> to compare.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(PatternInfo left, PatternInfo right) => !(left == right);
    }
}
