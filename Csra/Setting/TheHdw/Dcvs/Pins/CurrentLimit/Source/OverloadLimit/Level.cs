using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Dcvs.Pins.CurrentLimit.Source.OverloadLimit {

    public class Level : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        internal Level(string value, string pinList) : this(double.Parse(value), pinList) { }

        public Level(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(22.5 * mA, "A", InitMode.OnProgramStarted, true);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.DCVS.Pins(pinList).CurrentLimit.Source.OverloadLimit.Level.Value = value;
        }

        private static double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.DCVS.Pins(pin).CurrentLimit.Source.OverloadLimit.Level.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
