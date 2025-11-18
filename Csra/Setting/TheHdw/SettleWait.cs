using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw {

    public class SettleWait : Setting_double {

        internal SettleWait(string value) : this(double.Parse(value)) { }

        public SettleWait(double value) {
            SetArguments(value, null, false);
            SetBehavior(0, "s", InitMode.Creation, false); // no cache -- no readback
            SetContext(SetAction, null, null);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, double value) => TestCodeBase.TheHdw.SettleWait(value);
    }
}
