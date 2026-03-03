using System;
using System.Collections.Generic;
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
