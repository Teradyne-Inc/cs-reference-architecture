using System.Collections.Generic;
using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw.Ppmu.Pins {

    [Serializable]
    public class ClampVLo : Setting_double {

        private static readonly Dictionary<string, double> _staticCache = [];

        public ClampVLo(double value, string pinList) {
            SetArguments(value, pinList, true);
            SetBehavior(-1.6 * V, "V", InitMode.OnProgramStarted, true);
            SetContext(true, _staticCache);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, double value) {
            TestCodeBase.TheHdw.PPMU.Pins(pinList).ClampVLo.Value = value;
        }

        protected override double[] ReadFunc(string pin) {
            double[] result = new double[TheExec.Sites.Existing.Count];
            ForEachSite(site => result[site] = TestCodeBase.TheHdw.PPMU.Pins(pin).ClampVLo.Value);
            return result;
        }

        public static void SetCache(double value, string pinList) => SetCacheInternal(value, pinList, _staticCache);
    }
}
