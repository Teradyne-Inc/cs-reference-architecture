using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Digital.Pins {

    public class StartState : Setting_Enum<ChStartState> {

        private static readonly Dictionary<string, ChStartState> _staticCache = [];

        internal StartState(string value, string pinList) : this(ParseEnum<ChStartState>(value), pinList) { }

        public StartState(ChStartState value, string pinList) {
            SetArguments(value, pinList, true);
            // ChStartState.None is used as a default value to keep the pins' current state
            SetBehavior(ChStartState.None, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, ChStartState value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).StartState = value;
        }

        private static ChStartState[] ReadFunc(string pin) {
            ChStartState[] result = new ChStartState[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).StartState);
            return result;
        }

        public static void SetCache(ChStartState value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
