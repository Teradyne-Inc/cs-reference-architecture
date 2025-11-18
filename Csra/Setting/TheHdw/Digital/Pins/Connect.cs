using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Digital.Pins {

    public class Connect : Setting_bool {

        private static readonly Dictionary<string, bool> _staticCache = [];

        internal Connect(string value, string pinList) : this(bool.Parse(value), pinList) { }

        public Connect(bool value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(false, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, bool value) {
            if (value) TestCodeBase.TheHdw.Digital.Pins(pinList).Connect();
            else TestCodeBase.TheHdw.Digital.Pins(pinList).Disconnect();
        }

        private static bool[] ReadFunc(string pin) {
            bool[] result = new bool[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).Connected);
            return result;
        }

        public static void SetCache(bool value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
