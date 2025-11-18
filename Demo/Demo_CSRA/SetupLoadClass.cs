using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Csra;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Csra.Api;

namespace Demo_CSRA {

    [TestClass]
    public class SetupLoadClass : TestCodeBase {

        [TestMethod]
        public void SetupLoad() {

            // TODO: Temporary workaround for SetupService.Add not working in ExecInterpose_OnProgramValidated
            // (https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/705)
            Services.Setup.Reset();

            Setup initLeakage = new Setup("InitLeakageTest");
            initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "nLEAB, nOEAB"));
            initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, "nLEBA, nOEBA"));
            initLeakage.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Off, "porta"));
            Services.Setup.Add(initLeakage);

            Setup initIccStatic = new Setup("InitIccStaticTest");
            initIccStatic.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "nOEBA"));
            initIccStatic.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, "nLEAB, nLEBA, nOEAB, porta"));
            Services.Setup.Add(initIccStatic);

            //TODO define a naming convention for setups 
            Setup initParametricBaseline = new Setup("InitParametricSingleConditionBaseline");
            initParametricBaseline.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, "nLEAB, nOEAB"));
            initParametricBaseline.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "nLEBA, nOEBA"));
            initParametricBaseline.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "porta"));
            Services.Setup.Add(initParametricBaseline);

            Setup initSearchParametric = new Setup("initSearchParametric");
            initSearchParametric.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "nLEAB, nOEAB"));
            initSearchParametric.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Lo, "nLEBA, nOEBA"));
            Services.Setup.Add(initSearchParametric);

            Setup initClkSrcSetHi = new Setup("InitClkSrcSetHi");
            initClkSrcSetHi.Add(new Csra.Setting.TheHdw.Digital.Pins.InitState(ChInitState.Hi, "clk_src"));
            Services.Setup.Add(initClkSrcSetHi);
        }

        public static List<PinSite<double>> MeasAction(PatternInfo patt, int stops) {
            _ = patt; // Prevent unused variable warning - compiles but does nothing with the pattern info.
            _ = stops; // Prevent unused variable warning - compiles but does nothing with the pattern info.
            return new List<PinSite<double>>();
        }
    }
}
