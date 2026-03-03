using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Dcvs.Pins {

    [Serializable]
    public class Mode : Setting_Enum<tlDCVSMode> {

        private static readonly Dictionary<string, tlDCVSMode> _staticCache = [];

        internal Mode(string value, string pinList) : this(ParseEnum<tlDCVSMode>(value), pinList) { }

        public Mode(tlDCVSMode value, string pinList) {
            SetArguments(value, pinList, true);
            // tlDCVSMode.Voltage is used as a default value for HighImpedance/HighRegulation are not for all instrument
            SetBehavior(tlDCVSMode.Voltage, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlDCVSMode value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).Mode = value;
        }

        protected override tlDCVSMode[] ReadFunc(string pin) {
            tlDCVSMode[] result = new tlDCVSMode[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).Mode);
            return result;
        }

        public static void SetCache(tlDCVSMode value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
