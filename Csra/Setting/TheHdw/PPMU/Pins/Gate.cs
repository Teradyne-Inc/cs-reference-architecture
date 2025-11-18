using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Ppmu.Pins {

    public class Gate : Setting_Enum<tlOnOff> {

        private static readonly Dictionary<string, tlOnOff> _staticCache = [];

        internal Gate(string value, string pinList) : this(ParseEnum<tlOnOff>(value), pinList) { }

        public Gate(tlOnOff value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlOnOff.On, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, tlOnOff value) {
            TestCodeBase.TheHdw.PPMU.Pins(pinList).Gate = value;
        }

        private static tlOnOff[] ReadFunc(string pin) {
            tlOnOff[] result = new tlOnOff[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.PPMU.Pins(pin).Gate);
            return result;
        }

        public static void SetCache(tlOnOff value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
