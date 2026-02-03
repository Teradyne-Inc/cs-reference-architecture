using System;
using System.Collections.Generic;
using Csra;
using Csra.Setting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins {

    [Serializable]
    public class Gate : Setting_Enum<tlDCVGate> {

        private static readonly Dictionary<string, tlDCVGate> _staticCache = [];

        internal Gate(string value, string pinList) : this(ParseEnum<tlDCVGate>(value), pinList) { }

        public Gate(tlDCVGate value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlDCVGate.GateOff, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlDCVGate value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).Gate = value;
        }

        protected override tlDCVGate[] ReadFunc(string pin) {
            tlDCVGate[] result = new tlDCVGate[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).Gate);
            return result;
        }

        public static void SetCache(tlDCVGate value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
