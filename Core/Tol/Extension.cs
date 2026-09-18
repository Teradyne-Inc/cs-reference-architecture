using System;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    internal static class Extension {

        public static PinSite<Samples<T>> ToPinSiteSamples<T>(this IPinListData pld) {
            PinSite<Samples<T>> result = new(pld.Pins.Count);
            for (int i = 0; i < pld.Pins.Count; i++) {
                Site<Samples<T>> data = new();
                string pinName = pld.Pins[i].Name;
                ForEachSite(site => {
                    data[site] = new Samples<T>(
                        ToValueArray<T>(pld.Pins[i].get_Value(site), pinName, site));
                });
                result[i].PinName = pinName;
                result[i] = data;
            }
            return result;
        }

        // Samples<T> is a COM-backed facade, so wrapping a T[] in one only to call ToArray() again
        // marshals every sample twice. Callers that want the raw values take this path instead.
        public static PinSite<T[]> ToPinSiteArray<T>(this IPinListData pld) {
            PinSite<T[]> result = new();
            for (int i = 0; i < pld.Pins.Count; i++) {
                Site<T[]> data = new();
                string pinName = pld.Pins[i].Name;
                ForEachSite(site => {
                    data[site] = ToValueArray<T>(pld.Pins[i].get_Value(site), pinName, site);
                });
                data.PinName = pinName;
                result.Add(data);
            }
            return result;
        }

        // A meter array read yields T[] per site; a capture signal yields an IDspWave COM object,
        // which only QueryInterface can unwrap - casting it straight to T[] always fails.
        private static T[] ToValueArray<T>(
            object value, string pinName, int site, [CallerMemberName] string caller = null) {
            if (value is T[] typed) {
                return typed;
            }

            // The wave is kept alongside its payload, so the throw below can distinguish a wave
            // carrying something unusable from a value that was never a wave at all.
            IDspWave wave = value as IDspWave;
            object unwrapped = wave is null ? value : wave.Data;

            if (unwrapped is T[] waveTyped) {
                return waveTyped;
            }

            // Rank > 1 falls through to the throw - flattening it would guess at the sample order.
            if (unwrapped is Array array && array.Rank == 1) {
                T[] converted = new T[array.Length];
                for (int i = 0; i < array.Length; i++) {
                    // Invariant, not the ambient culture: a comma-decimal locale reads "1.5" as 15.
                    converted[i] = (T)Convert.ChangeType(
                        array.GetValue(i), typeof(T), CultureInfo.InvariantCulture);
                }
                return converted;
            }

            string detail = wave is null
                ? DescribeType(unwrapped)
                : $"an IDspWave whose Data is {DescribeType(unwrapped)}";

            throw new InvalidCastException(
                $"{caller} cannot convert the value of pin '{pinName}' on site {site} " +
                $"to {typeof(T).Name}[]: it is {detail}.");
        }

        private static string DescribeType(object value) {
            if (value is null) {
                return "null";
            }

            return value is Array array && array.Rank != 1
                ? $"a rank-{array.Rank} array of type '{value.GetType().FullName}'"
                : $"of type '{value.GetType().FullName}'";
        }

        private static string GetIndividualPinType(this string singlePin) {
            TheExec.DataManager.DecomposePinList(singlePin, out string[] individualPins, out _);
            TheExec.DataManager.GetChannelTypes(individualPins[0], out int numTypes, out string[] channelTypes);

            if(numTypes < 1) return string.Empty;

            string channel = TheHdw.ChanFromPinSite(individualPins[0], 0, channelTypes[0]);
            int slot = Convert.ToInt32(channel.Split('.').First());
            return TheHdw.Config.Slots[slot].Type;
        }

        public static bool AreAllPinsOfType<IPins>(this string pinList) {
            string[] returnTypeNamesPpmu = ["HSDP", "HSDPx"];
            string[] returnTypeNamesDcvi = ["DC-8p5V90V"];
            string[] returnTypeNamesDcvs = DcvsSlotType.SupportedByTol;
            string[] returnTypeNamesDigital = ["HSDP", "HSDPx"];

            var pinTypes = pinList
                .Split([','], StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim().GetIndividualPinType());

            Func<string[], Func<string, bool>> checkType = (types) => (pin) => types.Contains(pin);

            if (typeof(IPins) == typeof(IPpmuPins)) {
                return pinTypes.All(checkType(returnTypeNamesPpmu));
            } else if (typeof(IPins) == typeof(IDcviPins)) {
                return pinTypes.All(checkType(returnTypeNamesDcvi));
            } else if (typeof(IPins) == typeof(IDcvsPins)) {
                return pinTypes.All(checkType(returnTypeNamesDcvs));
            } else if (typeof(IPins) == typeof(IDigitalPins)) {
                return pinTypes.All(checkType(returnTypeNamesDigital));
            }

            return false;
        }
    }
}
