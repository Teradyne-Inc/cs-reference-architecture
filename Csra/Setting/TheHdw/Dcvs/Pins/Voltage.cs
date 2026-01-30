using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Dcvs.Pins {

    [Serializable]
    public class Voltage : Setting_double {
        private static readonly Dictionary<string, double> _staticCache = [];

        internal Voltage(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Voltage(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(0 * V, "V", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).Voltage.Value = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).Voltage.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
