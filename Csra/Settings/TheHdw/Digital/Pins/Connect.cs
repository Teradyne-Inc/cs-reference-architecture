using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Digital.Pins {

    [Serializable]
    public class Connect : Setting_bool {

        private static readonly Dictionary<string, bool> _staticCache = [];

        internal Connect(string value, string pinList) : this(bool.Parse(value), pinList) { }

        public Connect(bool value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(false, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, bool value) {
            if (value) TestCodeBase.TheHdw.Digital.Pins(pinList).Connect();
            else TestCodeBase.TheHdw.Digital.Pins(pinList).Disconnect();
        }

        protected override bool[] ReadFunc(string pin) {
            bool[] result = new bool[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).Connected);
            return result;
        }

        public static void SetCache(bool value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
