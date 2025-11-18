using System.Collections.Generic;
using System.Linq;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins.FoldCurrentLimit {

    public class Timeout : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal Timeout(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Timeout(double value, string pinList) {
            SetArguments(value, pinList, true);
            double defaultTimeout = GetDefaultTimeout();
            SetBehavior(defaultTimeout, "s", InitMode.OnProgramStarted, true);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private bool IsSameDcviType() {
            string[] dcviType = new string[_pins.Count];
            for (int i = 0; i < dcviType.Length; i++) {
                string pin = _pins[i];
                dcviType[i] = TestCodeBase.TheHdw.DCVI.Pins(pin).DCVIType;
            }
            return dcviType.All(v => v == dcviType[0]);
        }

        private double GetDefaultTimeout() {
            if (_pins.Count > 1 && !IsSameDcviType()) {
                Api.Services.Alert.Error("Mixed pin type! Timeout does not support setting up different types of DCVI pins together.");
                return 0; // dummy - will not be reachable in product code
            }
            // get default ComplianceRange Negative value by channel type
            string allPins = string.Join(", ", _pins);
            string dcviType = TestCodeBase.TheHdw.DCVI.Pins(allPins).DCVIType;
            switch (dcviType) {
                case "DC-90V50mA": // UVI264 HVVI
                    return 0.0;
                case "DC-8p5V500mA": // UVI264 LVVI
                    return 1.0;
                default:
                    Api.Services.Alert.Error("Wrong DCVI pin type! Only support UVI264 currently.");
                    return 0.0; // return 0 when not DCVI pin type not list here
            }
        }

        private static void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).FoldCurrentLimit.Timeout.Value = value;
        }

        private static double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).FoldCurrentLimit.Timeout.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
