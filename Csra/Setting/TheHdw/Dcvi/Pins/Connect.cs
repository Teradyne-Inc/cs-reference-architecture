using System;
using System.Collections.Generic;
using System.Linq;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvi.Pins {

    public class Connect : Setting_Enum<tlDCVIConnectWhat> {

        private static readonly Dictionary<string, tlDCVIConnectWhat> _staticCache = [];

        internal Connect(string value, string pinList) : this(ParseEnum<tlDCVIConnectWhat>(value), pinList) { }

        public Connect(tlDCVIConnectWhat value, string pinList) {
            value = value == tlDCVIConnectWhat.Default ? GetDefaultStatusValue(pinList) : value;
            SetArguments(value, pinList, true);
            SetBehavior(tlDCVIConnectWhat.None, string.Empty, InitMode.OnProgramStarted, false);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private tlDCVIConnectWhat GetDefaultStatusValue(string pinList) {
            // get default connect value by channel type
            string dcviType = TestCodeBase.TheHdw.DCVI.Pins(pinList).DCVIType;
            switch (dcviType) {
                case "DC-90V50mA": // UVI264 HVVI
                case "DC-8p5V500mA": // UVI264 LVVI
                    return tlDCVIConnectWhat.HighForce | tlDCVIConnectWhat.HighSense;
                default:
                    Api.Services.Alert.Error("Wrong DCVI pin type! Only support UVI264 currently."); // raise error when not DCVI pin type not list here
                    return tlDCVIConnectWhat.None;
            }
        }

        private static void SetAction(string pinList, tlDCVIConnectWhat value) {
            tlDCVIConnectWhat turnOn = tlDCVIConnectWhat.None;
            tlDCVIConnectWhat turnOff = tlDCVIConnectWhat.None;
            var pins = pinList.Split(',').Select(p => p.Trim());
            foreach (string pin in pins) {
                turnOn |= ~_staticCache[pin] & value;
                turnOff |= _staticCache[pin] & ~value;
            }
            if (turnOff != tlDCVIConnectWhat.None) TestCodeBase.TheHdw.DCVI.Pins(pinList).Disconnect(turnOff);
            if (turnOn != tlDCVIConnectWhat.None) TestCodeBase.TheHdw.DCVI.Pins(pinList).Connect(turnOn);
        }

        private static tlDCVIConnectWhat[] ReadFunc(string pin) {
            tlDCVIConnectWhat[] result = new tlDCVIConnectWhat[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVI.Pins(pin).Connected);
            return result;
        }

        public static void SetCache(tlDCVIConnectWhat value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
