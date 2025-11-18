using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Digital.Pins.Levels {

    public class DriverMode : Setting_Enum<tlDriverMode> {

        private static readonly Dictionary<string, tlDriverMode> _staticCache = [];

        internal DriverMode(string value, string pinList) : this(ParseEnum<tlDriverMode>(value), pinList) { }

        public DriverMode(tlDriverMode value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlDriverMode.HiZ, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, tlDriverMode value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).Levels.DriverMode = value;
        }

        private static tlDriverMode[] ReadFunc(string pin) {
            tlDriverMode[] result = new tlDriverMode[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).Levels.DriverMode);
            return result;
        }

        public static void SetCache(tlDriverMode value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
