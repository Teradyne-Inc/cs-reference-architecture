using System;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;

namespace Demo_CS.Functional {

    [TestClass]
    public class Read : TestCodeBase {

        

        /// <summary>
        /// Runs a pattern and logs the pass/fail result, also the read HRAM data from one or more pins.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="readPins">Pins for data read, must contain at least 1 digital pin.</param>
        [TestMethod]
        public void Baseline(Pattern pattern, PinList readPins) {

            Site<bool> patResult;
            PinSite<Samples<int>> readWords;

            TheHdw.Digital.ApplyLevelsTiming(true, true, true, tlRelayMode.Powered);

            TheHdw.Digital.Pins("nLEAB, nOEAB").InitState = ChInitState.Lo;
            TheHdw.Digital.Pins("nLEBA, nOEBA").InitState = ChInitState.Hi;
            TheHdw.Digital.Pins("porta").InitState = ChInitState.Off;
            TheHdw.SettleWait(1.0);

            TheHdw.Digital.HRAM.SetTrigger(TrigType.First, false, 0, true);
            TheHdw.Digital.HRAM.CaptureType = CaptType.All;
            TheHdw.Digital.HRAM.Size = 128;


            TheHdw.Patterns(pattern).Start();
            TheHdw.Patterns(pattern).HaltWait();

            patResult = TheHdw.Digital.Patgen.PatternBurstPassedPerSite.ToSite();

            readWords = new PinSite<Samples<int>>();
            TheExec.DataManager.DecomposePinList(readPins, out string[] pins, out _);
            foreach (string pin in pins) {
                ISiteLong[] hramWords = (ISiteLong[])TheHdw.Digital.Pins(pin).HRAM.ReadDataWord(8, 8, 8, tlBitOrder.LsbFirst);
                Site<Samples<int>> siteWords = new Site<Samples<int>>();
                siteWords.PinName = pin;
                ForEachSite(site => {
                    int[] wordValues = hramWords.Select(word => (int)word[site]).ToArray();
                    siteWords[site] = new Samples<int>(wordValues);
                });
                readWords.Add(siteWords);
            }

            TheExec.Flow.FunctionalTestLimit(patResult, pattern);
            TheExec.Flow.TestLimit(readWords, ForceResults: tlLimitForceResults.Flow);
        }
    }
}
