using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Pins class - a collection of Pin objects.
    /// </summary>
    [Serializable]
    public class Pins : IEnumerable<Pins.Pin>, IEquatable<Pins> {

        private List<Pin> _pins;

        /// <summary>
        /// Construct a new <see cref=" Pins"/> object. Resolves (nested) pin groups and lists.
        /// </summary>
        /// <param name="pinList">The pin list to create the <see cref="Pins"/> object for.</param>
        public Pins(string pinList) => _pins = ResolvePinList(pinList);

        private List<Pin> ResolvePinList(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return [];
            int success = TheExec.DataManager.DecomposePinList(pinList, out string[] pinArray, out _);
            if (success == 0) return pinArray.Select(pin => new Pin(pin)).ToList();
            Api.Services.Alert.Error<ArgumentException>($"Pins: Failed to resolve pin list '{pinList}'"); // the "else" path ...
            return []; // needed to satisfy compiler
        }

        private Pins(IEnumerable<Pin> pins) => _pins = pins.ToList();

        /// <summary>
        /// Get an enumerator for the <see cref="Pins"/> object to support <code>foreach</code>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<Pin> GetEnumerator() => _pins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Add pins to the <see cref="Pins"/> object. Resolves (nested) pin groups and lists.
        /// </summary>
        /// <param name="pinList">The pin list to add to the <see cref="Pins"/> object.</param>
        public void Add(string pinList) => _pins = _pins.Union(ResolvePinList(pinList)).ToList();

        /// <summary>
        /// Return the number of pins in the <see cref="Pins"/> object.
        /// </summary>
        public int Count() => _pins.Count;

        /// <summary>
        /// Extract pins by instrument type.
        /// </summary>
        /// <param name="type">The instrument type to look for.</param>
        /// <returns>A new <see cref="Pins"/> object only containing pins of the specified instrument type.</returns>
        public Pins ExtractByType(InstrumentType type) => new(_pins.Where(p => p.Type == type));

        /// <summary>
        /// Extract pins by instrument feature.
        /// </summary>
        /// <param name="feature">The instrument feature to look for.</param>
        /// <returns>A new <see cref="Pins"/> object only containing pins with the specified feature.</returns>
        public Pins ExtractByFeature(InstrumentFeature feature) => new(_pins.Where(p => p.Features.Contains(feature)));

        /// <summary>
        /// Extract pins by instrument domain.
        /// </summary>
        /// <param name="domain">The instrument domain to look for.</param>
        /// <returns>A new <see cref="Pins"/> object only containing pins with the specified domain.</returns>
        public Pins ExtractByDomain(InstrumentDomain domain) => new(_pins.Where(p => p.Domains.Contains(domain)));

        /// <summary>
        /// Extract a range of pins from the <see cref="Pins"/> object.
        /// </summary>
        /// <param name="start">The index of the first pin to extract.</param>
        /// <param name="count">The total number of pins to extract.</param>
        /// <returns>A new <see cref="Pins"/> object only containing the subset of pins.</returns>
        public Pins ExtractRange(int start, int count) {
            if (start < 0 || count < 0 || start + count > _pins.Count) {
                Api.Services.Alert.Error<ArgumentOutOfRangeException>($"Pins: Invalid range specified: start={start}, count={count}, total pins={_pins.Count}");
                return new Pins([]); // dummy - will never execute
            }
            return new Pins(_pins.Skip(start).Take(count));
        }

        /// <summary>
        /// Test if at least one pin is of the specified instrument type.
        /// </summary>
        /// <param name="type">The instrument type to look for.</param>
        /// <returns>True if one or more pins have the type.</returns>
        public bool ContainsType(InstrumentType type) => _pins.Any(p => p.Type == type);

        /// <summary>
        /// Combined test and extraction of pins by instrument type.
        /// </summary>
        /// <param name="instrument">The instrument type to look for.</param>
        /// <param name="pinList">A new <see cref="Pins"/> object only containing pins of the specified instrument type.</param>
        /// <returns>True if one or more pins have the type.</returns>
        public bool ContainsType(InstrumentType instrument, out string pinList) => TryFilter(out pinList, _pins.Where(p => p.Type == instrument));

        /// <summary>
        /// Test if at least one pin has the specified feature.
        /// </summary>
        /// <param name="feature">The instrument feature to look for.</param>
        /// <returns>True if one or more pins have the feature.</returns>
        public bool ContainsFeature(InstrumentFeature feature) => _pins.Any(p => p.Features.Contains(feature));

        /// <summary>
        /// Combined test and extraction of pins by instrument feature.
        /// </summary>
        /// <param name="feature">The instrument feature to look for.</param>
        /// <param name="pinList">A new <see cref="Pins"/> object only containing pins of the specified type.</param>
        /// <returns>True if one or more pins have the feature.</returns>
        public bool ContainsFeature(InstrumentFeature feature, out string pinList) => TryFilter(out pinList, _pins.Where(p => p.Features.Contains(feature)));

        /// <summary>
        /// Test if at least one pin has the specified domain.
        /// </summary>
        /// <param name="domain">The instrument domain to look for.</param>
        /// <returns>True if one or more pins have the domain.</returns>
        public bool ContainsDomain(InstrumentDomain domain) => _pins.Any(p => p.Domains.Contains(domain));

        /// <summary>
        /// Combined test and extraction of pins by instrument domain.
        /// </summary>
        /// <param name="domain">The instrument domain to look for.</param>
        /// <param name="pinList">A new <see cref="Pins"/> object only containing pins of the specified type.</param>
        /// <returns>True if one or more pins have the domain.</returns>
        public bool ContainsDomain(InstrumentDomain domain, out string pinList) => TryFilter(out pinList, _pins.Where(p => p.Domains.Contains(domain)));

        private bool TryFilter(out string pinList, IEnumerable<Pin> filtered) {
            if (filtered.Any()) {
                pinList = ConvertToString(filtered);
                return true;
            }
            pinList = string.Empty;
            return false;
        }

        /// <summary>
        /// Rearranges the pins in the <see cref="Pins"/> object by name (in place).
        /// </summary>
#pragma warning disable IDE0305 // Simplify collection initialization... not working today as it will trip IG-XL serializer
        public void Sort() => _pins = _pins.OrderBy(p => p.Name).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

        /// <summary>
        /// Rearranges the pins in the <see cref="Pins"/> object by the specified key (in place).
        /// </summary>
        /// <typeparam name="TKey">The target kay type for the selector delegate.</typeparam>
        /// <param name="selector">The selector delegate creating the sort key.</param>
#pragma warning disable IDE0305 // Simplify collection initialization... not working today as it will trip IG-XL serializer
        public void Sort<TKey>(Func<Pin, TKey> selector) => _pins = _pins.OrderBy(selector).ToList();
#pragma warning restore IDE0305 // Simplify collection initialization

        /// <summary>
        /// Convert the <see cref="Pins"/> object to a comma-separated list of all pin names.
        /// </summary>
        /// <returns>A comma-separated list of all pin names.</returns>
        public override string ToString() => ConvertToString(_pins);

        private static string ConvertToString(IEnumerable<Pin> pins) => string.Join(", ", pins.Select(p => p.Name));

        /// <summary>
        /// Combines a collection of <see cref="Pins"/> objects into a single <see cref="Pins"/> object.
        /// </summary>
        /// <param name="pinGroups">An array of <see cref="Pins"/> objects.</param>
        /// <returns>A new <see cref="Pins"/> object with all pins combined.</returns>
        public static Pins Join(Pins[] pinGroups) => new(pinGroups.SelectMany(pg => pg));

        /// <summary>
        /// Determines whether the specified <see cref="Pins"/> instance is equal to the current instance.
        /// </summary>
        /// <param name="other">The <see cref="Pins"/> instance to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified <see cref="Pins"/> contains the same pins in the same order; otherwise, <c>false</c>.</returns>
        public bool Equals(Pins other) {
            if (other is null) return false;
            return _pins.SequenceEqual(other._pins);
        }

        /// <summary>
        /// Determines whether the specified object is equal to the current <see cref="Pins"/>.
        /// </summary>
        /// <param name="obj">The object to compare with the current instance.</param>
        /// <returns><c>true</c> if the specified object is a <see cref="Pins"/> and contains the same pins in the same order; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => Equals(obj as Pins);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Pins"/>.</returns>
        public override int GetHashCode() {
            unchecked {
                int hash = 17;
                foreach (Pin pin in _pins) hash = hash * 23 + (pin?.GetHashCode() ?? 0);
                return hash;
            }
        }

        /// <summary>
        /// Determines whether two <see cref="Pins"/> instances are equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pins"/> to compare.</param>
        /// <param name="right">The second <see cref="Pins"/> to compare.</param>
        /// <returns><c>true</c> if both instances are equal or both are <c>null</c>; otherwise, <c>false</c>.</returns>
        public static bool operator ==(Pins left, Pins right) {
            if (left is null) return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Determines whether two <see cref="Pins"/> instances are not equal.
        /// </summary>
        /// <param name="left">The first <see cref="Pins"/> to compare.</param>
        /// <param name="right">The second <see cref="Pins"/> to compare.</param>
        /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
        public static bool operator !=(Pins left, Pins right) => !(left == right);

        /// <summary>
        /// Combines multiple <see cref="PinSite{T}"/> objects into a single one maintaining this <see cref="Pins"/> object's pin sequence. Excessive objects
        /// are quietly ignored, a runtime exception is thrown for missing ones.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="PinSite{T}"/> elements.</typeparam>
        /// <param name="pinSite">A collection of <see cref="PinSite{T}"/> instances to be combined and arranged.</param>
        /// <returns>A new <see cref="PinSite{T}"/> containing elements from all input <see cref="PinSite{T}"/> instances, arranged in the right
        /// order.</returns> 
        public PinSite<T> ArrangePinSite<T>(IEnumerable<PinSite<T>> pinSite) {
            PinSite<T> pinSiteAll = new();
            foreach (PinSite<T> ps in pinSite) pinSiteAll.AddRange(ps);
            return new(this.Select(s => pinSiteAll[s.Name]).ToList());
        }

        /// <summary>
        /// Pin class - individual hardware pins with features.
        /// </summary>
        [Serializable]
        public class Pin : IEquatable<Pin> {

            private static readonly BiDictionary<string, InstrumentType> _typeConversion = new() {
                { "HSDP", InstrumentType.UP2200 },
                { "HSDPx", InstrumentType.UPHP },
                { "DC-8p5V90V", InstrumentType.UVI264 },
                { "VS-5A", InstrumentType.UVS64 },
                { "VS-800mA", InstrumentType.UVS256 },
                { "Support", InstrumentType.SupportBoard },
            };

            /// <summary>
            /// The (resolved) pin name.
            /// </summary>
            public string Name { get; }

            /// <summary>
            /// The instrument type of the pin.
            /// </summary>
            public InstrumentType Type { get; }

            /// <summary>
            /// A collection of features this pin supports.
            /// </summary>
            public List<InstrumentFeature> Features { get; }

            /// <summary>
            /// A collection of functional domains this pin supports.
            /// </summary>
            public List<InstrumentDomain> Domains { get; }

            /// <summary>
            /// Construct a new <see cref="Pin"/> object.
            /// </summary>
            /// <param name="name">The pin name.</param>
            public Pin(string name) {
                Name = name;
                Type = GetInstrumentType(name);
                Features = GetInstrumentFeatures(Type);
                Domains = GetInstrumentDomains(Type);
            }

            private static InstrumentType GetInstrumentType(string pin) {
                if (pin?.Length == 0) return InstrumentType.NC; // empty pin name (e.g. from pin list decomposition
                TheExec.DataManager.GetChannelTypes(pin, out int numTypes, out string[] chanTypes);
                switch (numTypes) {
                    //case 0: return Type.NC; // actually also returned if multiple pins are thrown into GetChannelTypes
                    case >= 1:
                        string channel = TheHdw.ChanFromPinSite(pin, 0, chanTypes[0]); // site 0 is good enough for now, don't support different inst per site
                        int slot = Convert.ToInt32(channel.Split('.').First());
                        string type = TheHdw.Config.Slots[slot].Type;
                        return GetType(type);
                    default: return InstrumentType.NC; // actually also returned if multiple or invalid pins are thrown into GetChannelTypes
                }
            }

            private static List<InstrumentFeature> GetInstrumentFeatures(InstrumentType type) {
#pragma warning disable IDE0028 // Simplify collection initialization ... not working today as it will trip IG-XL serializer
                return type switch {
                    InstrumentType.UP2200 => new List<InstrumentFeature> { InstrumentFeature.Ppmu, InstrumentFeature.Digital },
                    InstrumentType.UPHP => new List<InstrumentFeature> { InstrumentFeature.Ppmu, InstrumentFeature.Digital },
                    InstrumentType.UVI264 => new List<InstrumentFeature> { InstrumentFeature.Dcvi },
                    InstrumentType.UVS64 => new List<InstrumentFeature> { InstrumentFeature.Dcvs },
                    InstrumentType.UVS256 => new List<InstrumentFeature> { InstrumentFeature.Dcvs },
                    InstrumentType.SupportBoard => new List<InstrumentFeature> { InstrumentFeature.Utility },
                    _ => new List<InstrumentFeature>() // NC case - legitimate
                };
#pragma warning restore IDE0028 // Simplify collection initialization ... not working today as it will trip IG-XL serializer
            }

            private static List<InstrumentDomain> GetInstrumentDomains(InstrumentType type) {
#pragma warning disable IDE0028 // Simplify collection initialization ... not working today as it will trip IG-XL serializer
                return type switch {
                    InstrumentType.UP2200 => new List<InstrumentDomain> { InstrumentDomain.Dc, InstrumentDomain.Digital },
                    InstrumentType.UPHP => new List<InstrumentDomain> { InstrumentDomain.Dc, InstrumentDomain.Digital },
                    InstrumentType.UVI264 => new List<InstrumentDomain> { InstrumentDomain.Dc },
                    InstrumentType.UVS64 => new List<InstrumentDomain> { InstrumentDomain.Dc },
                    InstrumentType.UVS256 => new List<InstrumentDomain> { InstrumentDomain.Dc },
                    InstrumentType.SupportBoard => new List<InstrumentDomain> { InstrumentDomain.Utility },
                    _ => new List<InstrumentDomain>() // NC case - legitimate
                };
#pragma warning restore IDE0028 // Simplify collection initialization ... not working today as it will trip IG-XL serializer
            }

            /// <summary>
            /// This method is public but intended for internal use only.
            /// </summary>
            /// <param name="typeString">The type string to convert.</param>
            /// <returns>The corresponding Type.</returns>
            public static InstrumentType GetType(string typeString) {
                if (_typeConversion.TryGetValue(typeString, out InstrumentType value)) return value;
                return InstrumentType.NC;
            }

            /// <summary>
            /// This method is public but intended for internal use only.
            /// </summary>
            /// <param name="type">The Type to convert.</param>
            /// <returns>The corresponding type string.</returns>
            public static string GetInstrumentName(InstrumentType type) {
                if (_typeConversion.TryGetKey(type, out string key)) return key;
                return "N/A";
            }

            /// <summary>
            /// Determines whether the specified <see cref="Pin"/> instance is equal to the current instance.
            /// </summary>
            /// <param name="other">The <see cref="Pin"/> instance to compare with the current instance.</param>
            /// <returns><c>true</c> if the specified <see cref="Pin"/> has the same <see cref="Name"/>; otherwise, <c>false</c>.</returns>
            public bool Equals(Pin other) {
                if (other is null) return false;
                return Name == other.Name;
            }

            /// <summary>
            /// Determines whether the specified object is equal to the current <see cref="Pin"/>.
            /// </summary>
            /// <param name="obj">The object to compare with the current instance.</param>
            /// <returns><c>true</c> if the specified object is a <see cref="Pin"/> and has the same <see cref="Name"/>; otherwise, <c>false</c>.</returns>
            public override bool Equals(object obj) => Equals(obj as Pin);

            /// <summary>
            /// Serves as the default hash function.
            /// </summary>
            /// <returns>A hash code for the current <see cref="Pin"/>.</returns>
            public override int GetHashCode() => Name?.GetHashCode() ?? 0;

            /// <summary>
            /// Determines whether two <see cref="Pin"/> instances are equal.
            /// </summary>
            /// <param name="left">The first <see cref="Pin"/> to compare.</param>
            /// <param name="right">The second <see cref="Pin"/> to compare.</param>
            /// <returns><c>true</c> if both instances are equal or both are <c>null</c>; otherwise, <c>false</c>.</returns>
            public static bool operator ==(Pin left, Pin right) {
                if (left is null) return right is null;
                return left.Equals(right);
            }

            /// <summary>
            /// Determines whether two <see cref="Pin"/> instances are not equal.
            /// </summary>
            /// <param name="left">The first <see cref="Pin"/> to compare.</param>
            /// <param name="right">The second <see cref="Pin"/> to compare.</param>
            /// <returns><c>true</c> if the instances are not equal; otherwise, <c>false</c>.</returns>
            public static bool operator !=(Pin left, Pin right) => !(left == right);

        }
    }
}
