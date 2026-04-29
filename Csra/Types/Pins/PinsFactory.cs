using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tol;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Types {

    [Serializable]
    public class PinsFactory : Factory {

        private static Dictionary<string, IPpmuPins> _ppmuPinsCache = new();
        private static Dictionary<string, IDcviPins> _dcviPinsCache = new();
        private static Dictionary<string, IDcvsPins> _dcvsPinsCache = new();
        private static Dictionary<string, IDigitalPins> _digitalPinsCache = new();
        private static Dictionary<string, IUtilityPins> _utilityPinsCache = new();

        public static void Reset() {
            _ppmuPinsCache.Clear();
            _dcviPinsCache.Clear();
            _dcvsPinsCache.Clear();
            _digitalPinsCache.Clear();
            _utilityPinsCache.Clear();
        }

        public static List<string> GetPins(string pinList, out IPpmuPins ppmuPins, out IDcviPins dcviPins, out IDcvsPins dcvsPins, out IDigitalPins digitalPins, out IUtilityPins utilityPins) {

            string[] csvPins = pinList // split comma separated value to array
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .Distinct()
                .ToArray();

            List<string> pinListOfPpmu = new List<string>();
            List<string> pinListOfDcvi = new List<string>();
            List<string> pinListOfDcvs = new List<string>();
            List<string> pinListOfDigital = new List<string>();
            List<string> pinListOfUtility = new List<string>();
            string[] returnTypeNamesPpmu = ["HSDP", "HSDPx", "HSD-U"];
            string[] returnTypeNamesDcvi = ["DC-8p5V90V", "DC-07"];
            string[] returnTypeNamesDcvs = ["VS-5A", "VS-800mA", "VS-20A", "HexVS", "VSM"];
            string[] returnTypeNamesDigital = returnTypeNamesPpmu;
            string[] returnTypeNamesUtility = ["Support", "SupportBoard"];
            List<string> pins = new();
            foreach (var csvPin in csvPins) {
                int success = TheExec.DataManager.DecomposePinList(csvPin, out string[] individualPins, out _);
                if (success != 0) {
                    Api.Services.Alert.Error<ArgumentException>($"Pins: Failed to resolve pin list '{pinList}'.");
                    break;
                }
                pins.AddRange(individualPins);
                TheExec.DataManager.GetChannelTypes(individualPins[0], out int numTypes, out string[] channelTypes);
                if (numTypes >= 1) {
                    string channel = TheHdw.ChanFromPinSite(individualPins[0], 0, channelTypes[0]);
                    int slot = Convert.ToInt32(channel.Split('.').First());
                    string type = TheHdw.Config.Slots[slot].Type;
                    if (returnTypeNamesPpmu.Contains(type)) pinListOfPpmu.Add(csvPin);
                    if (returnTypeNamesDcvi.Contains(type)) pinListOfDcvi.Add(csvPin);
                    if (returnTypeNamesDcvs.Contains(type)) pinListOfDcvs.Add(csvPin);
                    if (returnTypeNamesDigital.Contains(type)) pinListOfDigital.Add(csvPin);
                    if (returnTypeNamesUtility.Contains(type)) pinListOfUtility.Add(csvPin);
                }
            }


            ppmuPins = GetCachedPins(string.Join(", ", pinListOfPpmu), _ppmuPinsCache, CreatePpmuPins);
            dcviPins = GetCachedPins(string.Join(", ", pinListOfDcvi), _dcviPinsCache, CreateDcviPins);
            dcvsPins = GetCachedPins(string.Join(", ", pinListOfDcvs), _dcvsPinsCache, CreateDcvsPins);
            digitalPins = GetCachedPins(string.Join(", ", pinListOfDigital), _digitalPinsCache, CreateDigitalPins);
            utilityPins = GetCachedPins(string.Join(", ", pinListOfUtility), _utilityPinsCache, CreateUtilityPins);

            return pins;
        }

        private static T GetCachedPins<T>(string pinList, Dictionary<string, T> cache, Func<string, T> createPinsFunc) where T : class {
            if (string.IsNullOrWhiteSpace(pinList)) return null;
            if (!cache.TryGetValue(pinList, out T pins)) {
                pins = createPinsFunc(pinList);
                cache[pinList] = pins;
            }
            return pins;
        }
    }
}
