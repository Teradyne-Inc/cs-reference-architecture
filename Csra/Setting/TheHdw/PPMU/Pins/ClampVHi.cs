using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw.Ppmu.Pins {

    public class ClampVHi : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        public ClampVHi(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(6.5 * V, "V", InitMode.OnProgramStarted, true);
            SetContext(SetAction, ReadFunc, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.PPMU.Pins(pinList).ClampVHi.Value = value;
        }

        private static double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.PPMU.Pins(pin).ClampVHi.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
