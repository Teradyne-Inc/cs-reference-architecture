using System;
using System.Collections.Generic;
using Csra;
using Csra.Setting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins {

    [Serializable]
    public class Mode : Setting_Enum<tlDCVIMode> {

        private static readonly Dictionary<string, tlDCVIMode> _staticCache = [];

        internal Mode(string value, string pinList) : this(ParseEnum<tlDCVIMode>(value), pinList) { }

        public Mode(tlDCVIMode value, string pinList) {
            SetArguments(value, pinList, true);
            // tlDCVIMode.Voltage is used as a default value for HighImpedance/HighRegulation are not for all instrument
            SetBehavior(tlDCVIMode.Voltage, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }
        
        protected override void SetAction(string pinList, tlDCVIMode value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).Mode = value;
        }

        protected override tlDCVIMode[] ReadFunc(string pin) {
            tlDCVIMode[] result = new tlDCVIMode[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).Mode);
            return result;
        }

        public static void SetCache(tlDCVIMode value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
