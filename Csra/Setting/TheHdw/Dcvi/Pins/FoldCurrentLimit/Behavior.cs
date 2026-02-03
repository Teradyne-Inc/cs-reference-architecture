using System;
using System.Collections.Generic;
using Csra;
using Csra.Setting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins.FoldCurrentLimit {

    [Serializable]
    public class Behavior : Setting_Enum<tlDCVIFoldCurrentLimitBehavior> {

        private static readonly Dictionary<string, tlDCVIFoldCurrentLimitBehavior> _staticCache = [];

        internal Behavior(string value, string pinList) : this(ParseEnum<tlDCVIFoldCurrentLimitBehavior>(value), pinList) { }

        public Behavior(tlDCVIFoldCurrentLimitBehavior value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlDCVIFoldCurrentLimitBehavior.DoNotGateOff, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlDCVIFoldCurrentLimitBehavior value) {
            TestCodeBase.TheHdw.DCVI.Pins(pinList).FoldCurrentLimit.Behavior = value;
        }

        protected override tlDCVIFoldCurrentLimitBehavior[] ReadFunc(string pin) {
            tlDCVIFoldCurrentLimitBehavior[] result = new tlDCVIFoldCurrentLimitBehavior[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).FoldCurrentLimit.Behavior);
            return result;
        }

        public static void SetCache(tlDCVIFoldCurrentLimitBehavior value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
