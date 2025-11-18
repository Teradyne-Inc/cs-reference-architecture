using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Csra.Interfaces;
using Csra.TheLib;

namespace Csra.TheLib.Setup {
    public class LevelsAndTiming : ILib.ISetup.ILevelsAndTiming {
        public void Apply(bool connectAllPins = false, bool unpowered = false, bool levelRampSequence = false) {
            if (levelRampSequence) {
                if (connectAllPins) {
                    if (unpowered) { TheHdw.PinLevels.PowerDown(); }
                    TheHdw.PinLevels.ConnectAllPins();
                    TheHdw.SettleWait(1.0);
                }
                TheHdw.LevelsAndTiming.ApplyRepeatableSequence();
            } else {
                TheHdw.Digital.ApplyLevelsTiming(connectAllPins, true, true, unpowered ? tlRelayMode.Unpowered : tlRelayMode.Powered);
            }
        }
        
        public void ApplyWithPinStates(bool connectAllPins = false, bool unpowered = false, bool levelRampSequence = false, Pins initPinsHi = null,
                Pins initPinsLo = null, Pins initPinsHiZ = null) {
            if (levelRampSequence) {
                if (connectAllPins) {
                    if (unpowered) { TheHdw.PinLevels.PowerDown(); }
                    TheHdw.PinLevels.ConnectAllPins();
                    TheHdw.SettleWait(1.0);
                }
                if (initPinsHi is not null) {
                    TheHdw.Digital.Pins(initPinsHi.ToString()).InitState = ChInitState.Hi;
                }
                if (initPinsLo is not null) {
                    TheHdw.Digital.Pins(initPinsLo.ToString()).InitState = ChInitState.Lo;
                }
                if (initPinsHiZ is not null) {
                    TheHdw.Digital.Pins(initPinsHiZ.ToString()).InitState = ChInitState.Off;
                }
                TheHdw.LevelsAndTiming.ApplyRepeatableSequence(); // ApplyRepeatableSequence will use programmed init states to determine powerup or down
            } else {
                TheHdw.Digital.ApplyLevelsTiming(connectAllPins, true, true, unpowered ? tlRelayMode.Unpowered : tlRelayMode.Powered,
                    initPinsHi?.ToString() ?? "", initPinsLo?.ToString() ?? "", initPinsHiZ?.ToString() ?? "");
            }
        }
    }
}
