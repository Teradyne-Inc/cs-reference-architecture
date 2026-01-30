using System.Collections.Generic;
using System.Linq;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Dcvi.Pins {

    [Serializable]
    public class ComplianceRange_Negative : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal ComplianceRange_Negative(string value, string pinList) : this(double.Parse(value), pinList) { }

        public ComplianceRange_Negative(double value, string pinList) {
            SetArguments(value, pinList, true);
            double defaultComplianceRange = GetDefaultComplianceRange();
            SetBehavior(defaultComplianceRange, "V", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
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

        private double GetDefaultComplianceRange() {
            if (_pins.Count > 1 && !IsSameDcviType()) {
                Api.Services.Alert.Error("Mixed pin type! ComplianceRange_Negative not support to setup different types of DCVI pin together!");
            }
            // get default ComplianceRange Negative value by channel type
            string allPins = string.Join(", ", _pins);
            string dcviType = TestCodeBase.TheHdw.DCVI.Pins(allPins).DCVIType;
            switch (dcviType) {
                case "DC-90V50mA": // UVI264 HVVI
                    return 0.0;
                case "DC-8p5V500mA": // UVI264 LVVI
                    return 2.0;
                default:
                    Api.Services.Alert.Error("Wrong DCVI pin type! Only support UVI264 currently.");
                    return 0.0; // return 0 when not DCVI pin type not list here
            }
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).ComplianceRange(tlDCVICompliance.Negative).Value = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).ComplianceRange(tlDCVICompliance.Negative).Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
