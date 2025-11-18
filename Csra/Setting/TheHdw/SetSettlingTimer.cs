using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra.Setting.TheHdw {

    public class SetSettlingTimer : Setting_double {

        internal SetSettlingTimer(string value) : this(double.Parse(value)) { }

        public SetSettlingTimer(double value) {
            SetArguments(value, null, false);
            SetBehavior(0, "s", InitMode.Creation, false); // no cache -- no readback
            SetContext(SetAction, null, null);
            if (TheExec.JobIsValid) Validate();
        }

        private static void SetAction(string pinList, double value) => TestCodeBase.TheHdw.SetSettlingTimer(value);
    }
}
