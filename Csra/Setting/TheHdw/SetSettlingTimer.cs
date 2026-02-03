using Csra.Setting;
using Csra;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using System;

namespace Csra.Setting.TheHdw {

    [Serializable]
    public class SetSettlingTimer : Setting_double {

        internal SetSettlingTimer(string value) : this(double.Parse(value)) { }

        public SetSettlingTimer(double value) {
            SetArguments(value, null, false);
            SetBehavior(0, "s", InitMode.Creation, false); // no cache -- no readback
            SetContext(false, null);
            if (TheExec.JobIsValid) Validate();
        }

        protected override void SetAction(string pinList, double value) => TestCodeBase.TheHdw.SetSettlingTimer(value);
        protected override double[] ReadFunc(string pin) => throw new NotImplementedException();
    }
}
