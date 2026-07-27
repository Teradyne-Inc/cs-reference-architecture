using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra;
using static Csra.Api;
using Teradyne.Igxl.Interfaces.Public;
using static CsraTestMethods.Parametric.PatternHandshake;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace CsraTestMethods.CustomCode {

    /// <summary>
    /// To use PatternHandshakeBaselineClass in a TestMethod, it is specified by the user on the flowsheet via fully-qualified-name
    /// </summary>
    [Serializable]
    public class PatternHandshakeBaselineClass : IPatternHandshakeBaselineExecutable {
        public Pins Vcc { get; set; }
        public List<PinSite<double>> Result { get; } = [];
        public void Execute() {
            TheLib.Setup.Dc.SetMeter(Vcc, DcMeterMode.Current, rangeValue: 0.2 * A, outputRangeValue: 0.2 * A);
            Result.Add(TheLib.Acquire.Dc.Measure(Vcc, DcMeterMode.Current));
        }
        public void Clear() {
            Result.Clear();
        }
    }
}
