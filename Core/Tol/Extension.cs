using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Tol {

    internal static class Extension {

        public static PinSite<Samples<T>> ToPinSiteSamples<T>(this IPinListData pld) {
            PinSite<Samples<T>> result = new(pld.Pins.Count);
            for (int i = 0; i < pld.Pins.Count; i++) {
                Site<Samples<T>> data = new();
                ForEachSite(site => {
                    T[] values = (T[])pld.Pins[i].get_Value(site);
                    data[site] = new Samples<T>(values);
                });
                result[i].PinName = pld.Pins[i].Name;
                result[i] = data;
            }
            return result;
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
            string[] returnTypeNamesDcvs = ["VS-5A", "VS-800mA", "VS-20A"];
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
