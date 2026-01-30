using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Csra.Setting;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvs.Pins {

    [Serializable]
    public class Connect : Setting_Enum<tlDCVSConnectWhat> {

        private static readonly Dictionary<string, tlDCVSConnectWhat> _staticCache = [];

        internal Connect(string value, string pinList) : this(ParseEnum<tlDCVSConnectWhat>(value), pinList) { }

        public Connect(tlDCVSConnectWhat value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(tlDCVSConnectWhat.None, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, tlDCVSConnectWhat value) {
            tlDCVSConnectWhat turnOn = tlDCVSConnectWhat.None;
            tlDCVSConnectWhat turnOff = tlDCVSConnectWhat.None;
            List<string> pins = pinList.Split(',').Select(p => p.Trim()).ToList();
            foreach (string pin in pins) {
                turnOn |= ~_staticCache[pin] & value;
                turnOff |= _staticCache[pin] & ~value;
            }
            if (turnOff != tlDCVSConnectWhat.None) TestCodeBase.TheHdw.DCVS.Pins(pinList).Disconnect(turnOff);
            if (turnOn != tlDCVSConnectWhat.None) TestCodeBase.TheHdw.DCVS.Pins(pinList).Connect(turnOn);
        }

        protected override tlDCVSConnectWhat[] ReadFunc(string pin) {
            tlDCVSConnectWhat[] result = new tlDCVSConnectWhat[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).Connected);
            return result;
        }

        public static void SetCache(tlDCVSConnectWhat value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
