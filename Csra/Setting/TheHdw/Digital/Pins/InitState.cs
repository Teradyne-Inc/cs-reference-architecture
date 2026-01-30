using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Digital.Pins {

    [Serializable]
    public class InitState : Setting_Enum<ChInitState> {

        private static readonly Dictionary<string, ChInitState> _staticCache = [];

        internal InitState(string value, string pinList) : this(ParseEnum<ChInitState>(value), pinList) { }

        public InitState(ChInitState value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(ChInitState.Lo, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, ChInitState value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).InitState = value;
        }

        protected override ChInitState[] ReadFunc(string pin) {
            ChInitState[] result = new ChInitState[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).InitState);
            return result;
        }

        public static void SetCache(ChInitState value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
