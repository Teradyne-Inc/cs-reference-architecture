using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Digital.Pins.Levels {

    public class Value_Vil : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal Value_Vil(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Value_Vil(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(0, "V", InitMode.OnProgramStarted, true);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).Levels.Value[ChPinLevel.Vil] = value;
        }

        private double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).Levels.Value[ChPinLevel.Vil]);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
