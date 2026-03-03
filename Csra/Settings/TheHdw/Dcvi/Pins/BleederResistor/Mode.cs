using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Dcvi.Pins.BleederResistor {

    [Serializable]
    public class Mode : Setting_Enum<tlDCVIBleederResistor> {

        private static readonly Dictionary<string, tlDCVIBleederResistor> _staticCache = [];

        internal Mode(string value, string pinList) : this(ParseEnum<tlDCVIBleederResistor>(value), pinList) { }


        public Mode(tlDCVIBleederResistor value, string pinList) {
            SetArguments(value, pinList, true);
            if (CheckPinType() == false) return;
            SetBehavior(tlDCVIBleederResistor.Auto, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private bool CheckPinType() {
            // check whether support this command or not by channel type
            foreach (string pin in _pins) {
                if (TestCodeBase.TheHdw.DCVI.Pins(pin).DCVIType != "DC-8p5V500mA") {
                    Api.Services.Alert.Error($"Wrong DCVI pin type! Only support UVI264 LVVI!");
                    return false; // return false when not same DCVI type
                }
            }
            return true;
        }

        protected override void SetAction(string pinList, tlDCVIBleederResistor value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).BleederResistor.Mode = value;
        }

        protected override tlDCVIBleederResistor[] ReadFunc(string pin) {
            tlDCVIBleederResistor[] result = new tlDCVIBleederResistor[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).BleederResistor.Mode);
            return result;
        }

        public static void SetCache(tlDCVIBleederResistor value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
