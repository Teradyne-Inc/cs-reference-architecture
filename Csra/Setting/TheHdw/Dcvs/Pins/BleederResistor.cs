using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Csra.Setting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvs.Pins {

    [Serializable]
    public class BleederResistor : Setting_Enum<tlDCVSOnOffAuto> {

        private static readonly Dictionary<string, tlDCVSOnOffAuto> _staticCache = [];

        internal BleederResistor(string value, string pinList) : this(ParseEnum<tlDCVSOnOffAuto>(value), pinList) { }

        public BleederResistor(tlDCVSOnOffAuto value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlDCVSOnOffAuto.Auto, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlDCVSOnOffAuto value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).BleederResistor = value;
        }

        protected override tlDCVSOnOffAuto[] ReadFunc(string pin) {
            tlDCVSOnOffAuto[] result = new tlDCVSOnOffAuto[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).BleederResistor);
            return result;
        }

        public static void SetCache(tlDCVSOnOffAuto value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
