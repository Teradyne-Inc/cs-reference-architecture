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
        public static void GetPins(string pinList, out IPpmuPins ppmuPins, out IDcviPins dcviPins, out IDcvsPins dcvsPins, out IDigitalPins digitalPins) {

            string[] csvPins = pinList // split comma separated value to array
                .Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(pin => pin.Trim())
                .ToArray();

            List<string> pinListOfPpmu = new List<string>();
            List<string> pinListOfDcvi = new List<string>();
            List<string> pinListOfDcvs = new List<string>();
            List<string> pinListOfDigital = new List<string>();
            string[] returnTypeNamesPpmu = ["HSDP", "HSDPs"];
            string[] returnTypeNamesDcvi = ["DC-8p5V90V"];
            string[] returnTypeNamesDcvs = ["VS-5A", "VS-800mA"];
            string[] returnTypeNamesDigital = ["HSDP", "HSDPx"];

            foreach (var csvPin in csvPins) {
                TheExec.DataManager.DecomposePinList(csvPin, out string[] individualPins, out _);
                TheExec.DataManager.GetChannelTypes(individualPins[0], out int numTypes, out string[] channelTypes);
                if (numTypes >= 1) {
                    string channel = TheHdw.ChanFromPinSite(individualPins[0], 0, channelTypes[0]);
                    int slot = Convert.ToInt32(channel.Split('.').First());
                    string type = TheHdw.Config.Slots[slot].Type;
                    if (returnTypeNamesPpmu.Contains(type)) pinListOfPpmu.Add(csvPin);
                    if (returnTypeNamesDcvi.Contains(type)) pinListOfDcvi.Add(csvPin);
                    if (returnTypeNamesDcvs.Contains(type)) pinListOfDcvs.Add(csvPin);
                    if (returnTypeNamesDigital.Contains(type)) pinListOfDigital.Add(csvPin);
                }
            }

            ppmuPins = CreatePpmuPins(string.Join(", ", pinListOfPpmu));
            dcviPins = CreateDcviPins(string.Join(", ", pinListOfDcvi));
            dcvsPins = CreateDcvsPins(string.Join(", ", pinListOfDcvs));
            digitalPins = CreateDigitalPins(string.Join(", ", pinListOfDigital));
        }

        // protected static ICustomIDcvsPins CreateCustomDcvsPins(String pinList) {
        //     if (string.IsNullOrWhiteSpace(pinList)) return null;
        //     return new CustomDcvsPins(pinList);
        // }
    }
}
