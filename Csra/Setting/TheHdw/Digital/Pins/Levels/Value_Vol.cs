using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Digital.Pins.Levels {

    [Serializable]
    public class Value_Vol : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal Value_Vol(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Value_Vol(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(0, "V", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.Digital.Pins(pinList).Levels.Value[ChPinLevel.Vol] = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.Digital.Pins(pin).Levels.Value[ChPinLevel.Vol]);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
