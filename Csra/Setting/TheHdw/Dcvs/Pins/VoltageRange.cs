using System;
using System.Collections.Generic;
using System.Linq;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvs.Pins {

    [Serializable]
    public class VoltageRange : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal VoltageRange(string value, string pinList) : this(double.Parse(value), pinList) { }

        public VoltageRange(double value, string pinList) {
            SetArguments(value, pinList, true);
            double defaultVoltageRange = GetDefaultVoltageRange();
            SetBehavior(defaultVoltageRange, "V", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private bool IsSameDcvsType() {
            string[] dcvsType = new string[_pins.Count];
            for (int i = 0; i < dcvsType.Length; i++) {
                string pin = _pins[i];
                dcvsType[i] = TestCodeBase.TheHdw.DCVS.Pins(pin).DCVSType;
            }
            return dcvsType.All(v => v == dcvsType[0]);
        }

        private double GetDefaultVoltageRange() {
            if (_pins?.Count > 1 && !IsSameDcvsType()) {
                Api.Services.Alert.Error("Mixed Pin Types Detected! VoltageRange does not support setting up different types of DCVS pins together. Please " +
                    "ensure all pins are of the same type.");
            }

            string pinName = string.Join(", ", _pins);
            // get default voltage by channel type
            // UFLEX DCVS DO NOT support .DCVS.Pins().VoltageRange()
            string dcvsType = TestCodeBase.TheHdw.DCVS.Pins(pinName).DCVSType;
            switch (dcvsType) {
                case "VS-800mA": // UVS256HP
                    return 18.0;
                case "VS-5A": // UVS64
                    return 5.5;
                default:
                    Api.Services.Alert.Error("Wrong DCVS pin type! Only support UVS64 & UVS256HP currently.");
                    return 0.0; // return 0 when not DCVS pin type not list here 
            }
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).VoltageRange.Value = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).VoltageRange.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
