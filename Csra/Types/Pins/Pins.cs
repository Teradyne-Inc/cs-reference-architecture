using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Csra.Types;
using Teradyne.Igxl.Interfaces.Public;
using Tol;

namespace Csra {

    /// <summary>
    /// Pins class - a collection of Pin objects.
    /// </summary>
    [Serializable]
    public class Pins : IEnumerable<string>, IEquatable<Pins> {

        private List<string> _pins;
        private int _pinsCount;

        private IPpmuPins _ppmu;
        private IDcviPins _dcvi;
        private IDcvsPins _dcvs;
        private IDigitalPins _digital;
        private IUtilityPins _utility;

        /// <summary>
        /// Construct a new <see cref=" Pins"/> object. Resolves (nested) pin groups and lists.
        /// </summary>
        /// <param name="pinList">The pin list to create the <see cref="Pins"/> object for.</param>
        public Pins(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) _pins = [];
            else ResolvePinList(pinList);
        }

        public IPpmuPins Ppmu => _ppmu;
        public IDcviPins Dcvi => _dcvi;
        public IDcvsPins Dcvs => _dcvs;
        public IDigitalPins Digital => _digital;
        public IUtilityPins Utility => _utility;

        private void ResolvePinList(string pinList) {
            _pins = PinsFactory.GetPins(pinList, out _ppmu, out _dcvi, out _dcvs, out _digital, out _utility);
        }

        /// <summary>
        /// Get an enumerator for the <see cref="Pins"/> object to support <code>foreach</code>.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<string> GetEnumerator() => _pins.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        /// <summary>
        /// Add pins to the <see cref="Pins"/> object. Resolves (nested) pin groups and lists.
        /// </summary>
        /// <param name="pinList">The pin list to add to the <see cref="Pins"/> object.</param>
        public void Add(string pinList) {
            if (string.IsNullOrWhiteSpace(pinList)) return;
            List<string> temp = new(_pins);
            temp.Add(pinList);
            ResolvePinList(string.Join(", ", temp));
        }

        /// <summary>
        /// Return the number of pins in the <see cref="Pins"/> object.
        /// </summary>
        public int Count() => _pins.Count;

        /// <summary>
        /// Extract pins by instrument feature.
        /// </summary>
        /// <param name="feature">The instrument feature to look for.</param>
        /// <returns>A new <see cref="Pins"/> object only containing pins with the specified feature.</returns>
        public Pins ExtractByFeature(InstrumentFeature feature) {
            if (!ContainsFeature(feature)) return new("");
            switch (feature) {
                case InstrumentFeature.Ppmu:
                    return new(Ppmu.Name);
                case InstrumentFeature.Dcvi:
                    return new(Dcvi.Name);
                case InstrumentFeature.Dcvs:
                    return new(Dcvs.Name);
                case InstrumentFeature.Digital:
                    return new(Digital.Name);
                case InstrumentFeature.Utility:
                    return new(Utility.Name);
                default:
                    Api.Services.Alert.Error($"Unsupported instrument feature '{feature}'.");
                    break;
            }
            return new("");
        }

        /// <summary>
        /// Extract pins by instrument domain.
        /// </summary>
        /// <param name="domain">The instrument domain to look for.</param>
        /// <returns>A new <see cref="Pins"/> object only containing pins with the specified domain.</returns>
        public Pins ExtractByDomain(InstrumentDomain domain) {
            if (!ContainsDomain(domain)) return new("");
            switch (domain) {
                case InstrumentDomain.Dc:
                    return new(string.Join(", ", Ppmu.Name, Dcvi.Name, Dcvs.Name));
                case InstrumentDomain.Digital:
                    return new(Digital.Name);
                case InstrumentDomain.Utility:
                    return new(Utility.Name);
                default:
                    Api.Services.Alert.Error($"Unsupported instrument domain '{domain}'.");
                    break;
            }
            return new("");
        }

        /// <summary>
        /// Test if at least one pin has the specified feature.
        /// </summary>
        /// <param name="feature">The instrument feature to look for.</param>
        /// <returns>True if one or more pins have the feature.</returns>
        public bool ContainsFeature(InstrumentFeature feature) {
            switch (feature) {
                case InstrumentFeature.Ppmu:
                    return Ppmu is not null;
                case InstrumentFeature.Dcvi:
                    return Dcvi is not null;
                case InstrumentFeature.Dcvs:
                    return Dcvs is not null;
                case InstrumentFeature.Digital:
                    return Digital is not null;
                case InstrumentFeature.Utility:
                    return Utility is not null;
                default:
                    Api.Services.Alert.Error($"Unsupported instrument feature '{feature}'.");
                    break;
            }
            return false;
        }

        /// <summary>
        /// Test if at least one pin has the specified domain.
        /// </summary>
        /// <param name="domain">The instrument domain to look for.</param>
        /// <returns>True if one or more pins have the domain.</returns>
        public bool ContainsDomain(InstrumentDomain domain) {
            switch (domain) {
                case InstrumentDomain.Dc:
                    return Ppmu is not null || Dcvi is not null || Dcvs is not null;
                case InstrumentDomain.Digital:
                    return Digital is not null;
                case InstrumentDomain.Utility:
                    return Utility is not null;
                default:
                    Api.Services.Alert.Error($"Unsupported instrument domain '{domain}'.");
                    break;
            }
            return false;
        }

        /// <summary>
        /// Convert the <see cref="Pins"/> object to a comma-separated list of all pin names.
        /// </summary>
        /// <returns>A comma-separated list of all pin names.</returns>
        public override string ToString() => ConvertToString(_pins);

        private static string ConvertToString(IEnumerable<string> pins) => string.Join(", ", pins);

        /// <summary>
        /// Combines a collection of <see cref="Pins"/> objects into a single <see cref="Pins"/> object.
        /// </summary>
        /// <param name="pinGroups">An array of <see cref="Pins"/> objects.</param>
        /// <returns>A new <see cref="Pins"/> object with all pins combined.</returns>
        public static Pins Join(Pins[] pinGroups) => new(string.Join(", ", pinGroups.Select(pg => pg.ToString())));

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
        public override bool Equals(object obj) => obj is Pins other && Equals(other);

        /// <summary>
        /// Serves as the default hash function.
        /// </summary>
        /// <returns>A hash code for the current <see cref="Pins"/>.</returns>
        public override int GetHashCode() {
            int hash = 17;
            foreach (var pin in _pins) {
                hash = hash * 31 + pin.GetHashCode();
            }
            return hash;
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
        public static bool operator !=(Pins left, Pins right) {
            if (left is null) return right is not null;
            return !left.Equals(right);
        }

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
            return new(this.Select(s => pinSiteAll[s]).ToList());
        }

        /// <summary>
        /// Combines multiple <see cref="PinSite{T}"/> objects into a single one maintaining this <see cref="Pins"/> object's pin sequence. Excessive objects
        /// are quietly ignored, a runtime exception is thrown for missing ones.
        /// </summary>
        /// <typeparam name="T">The type of <see cref="PinSite{T}"/> elements.</typeparam>
        /// <param name="pinSite">A collection of <see cref="PinSite{T}"/> instances to be combined and arranged.</param>
        /// <returns>A new <see cref="PinSite{T}"/> containing elements from all input <see cref="PinSite{T}"/> instances, arranged in the right
        /// order.</returns> 
        public PinSite<Samples<T>> ArrangePinSite<T>(IEnumerable<PinSite<Samples<T>>> pinSite) {
            int pinCount = Count();
            PinSite<Samples<T>> reordered = new(pinCount);

            var flat = pinSite
                .SelectMany(site => Enumerable.Range(0, site.Count)
                .Select(i => site[i]))
                .ToDictionary(s => s.PinName, s => s);

            // Fill `reordered` by order of `pins`
            for (int i = 0; i < pinCount; i++) {
                string pinName = _pins[i];

                if (!flat.TryGetValue(pinName, out var samples)) {
                    Api.Services.Alert.Error($"Pin '{pinName}' not found in reference.");
                }
                reordered[i].PinName = pinName;
                reordered[pinName] = samples;
            }
            return reordered;
        }
    }
}
