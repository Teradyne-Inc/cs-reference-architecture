using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Utility.Pins {

    public class State : Setting_Enum<tlUtilBitState> {

        private static readonly Dictionary<string, tlUtilBitState> _staticCache = [];

        internal State(string value, string pinList) : this(ParseEnum<tlUtilBitState>(value), pinList) { }

        public State(tlUtilBitState value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlUtilBitState.Off, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, tlUtilBitState value) {
            TestCodeBase.TheHdw.Utility.Pins(pinList).State = value;
        }

        private static tlUtilBitState[] ReadFunc(string pin) {
            IPinListData readback = TestCodeBase.TheHdw.Utility.Pins(pin).States(tlUBState.Programmed);
            tlUtilBitState[] result = new tlUtilBitState[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = (tlUtilBitState)readback.Pins[0].get_Value(site));
            return result;
        }

        public static void SetCache(tlUtilBitState value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
