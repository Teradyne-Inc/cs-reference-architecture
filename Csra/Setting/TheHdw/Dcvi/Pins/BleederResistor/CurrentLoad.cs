using System.Collections.Generic;
using Csra.Services;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins.BleederResistor {

    public class CurrentLoad : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal CurrentLoad(string value, string pinList) : this(double.Parse(value), pinList) { }

        public CurrentLoad(double value, string pinList) {
            SetArguments(value, pinList, true);
            if (!IsSupportedPinType()) return;
            SetBehavior(10 * uA, "A", InitMode.OnProgramStarted, true);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private bool IsSupportedPinType() {
            // check whether support this command or not by channel type
            foreach (string pin in _pins) {
                if(TestCodeBase.TheHdw.DCVI.Pins(pin).DCVIType != "DC-8p5V500mA") {
                    Api.Services.Alert.Error($"Wrong DCVI pin type! Only support UVI264 LVVI!");
                    return false; // return false when not same DCVI type
                }
            }
            return true;
        }

        private static void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).BleederResistor.CurrentLoad.Value = value;
        }

        private static double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).BleederResistor.CurrentLoad.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
