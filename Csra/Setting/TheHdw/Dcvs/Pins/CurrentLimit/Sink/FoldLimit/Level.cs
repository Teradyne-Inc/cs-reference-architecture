using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Dcvs.Pins.CurrentLimit.Sink.FoldLimit {

    [Serializable]
    public class Level : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal Level(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Level(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(20.0 * mA, "A", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).CurrentLimit.Sink.FoldLimit.Level.Value = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).CurrentLimit.Sink.FoldLimit.Level.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
