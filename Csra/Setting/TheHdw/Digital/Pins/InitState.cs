using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Digital.Pins {

    public class InitState : Setting_Enum<ChInitState> {

        private static readonly Dictionary<string, ChInitState> _staticCache = [];

        internal InitState(string value, string pinList) : this(ParseEnum<ChInitState>(value), pinList) { }

        public InitState(ChInitState value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(ChInitState.Lo, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, ChInitState value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).InitState = value;
        }

        private static ChInitState[] ReadFunc(string pin) {
            ChInitState[] result = new ChInitState[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).InitState);
            return result;
        }

        public static void SetCache(ChInitState value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
