using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Ppmu.Pins {

    [Serializable]
    public class Gate : Setting_Enum<tlOnOff> {

        private static readonly Dictionary<string, tlOnOff> _staticCache = [];

        internal Gate(string value, string pinList) : this(ParseEnum<tlOnOff>(value), pinList) { }

        public Gate(tlOnOff value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlOnOff.On, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlOnOff value) {
            TestCodeBase.TheHdw.PPMU.Pins(pinList).Gate = value;
        }

        protected override tlOnOff[] ReadFunc(string pin) {
            tlOnOff[] result = new tlOnOff[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.PPMU.Pins(pin).Gate);
            return result;
        }

        public static void SetCache(tlOnOff value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
