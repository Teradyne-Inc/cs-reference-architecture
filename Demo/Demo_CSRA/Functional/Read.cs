using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.Functional {

    [TestClass(Creation.TestInstance), Serializable]
    public class Read : TestCodeBase {

        PatternInfo _patternInfo;
        Site<bool> _patResult;
        PinSite<Samples<int>> _readWords;
        Pins _pins;
        tlBitOrder _bitOrder;

        /// <summary>
        /// Executes a functional read from the device and logs the results.
        /// </summary>
        /// <param name="pattern">The pattern to be executed during the test.</param>
        /// <param name="readPins">Pins for data read, must contain at least 1 digital pin.</param>
        /// <param name="startIndex">Index to start read.</param>
        /// <param name="bitLength">Length of data read.</param>
        /// <param name="wordLength">Length of each data word from 1 to 32.</param>
        /// <param name="msbFirst">Data bit order.</param>
        /// <param name="testFunctional">Whether to log the functional result.</param>
        /// <param name="testValues">Whether to log the read results.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern is run.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, PinList readPins, int startIndex, int bitLength, int wordLength, bool msbFirst, bool testFunctional,
            bool testValues, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pins(readPins, nameof(readPins), out _pins);
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _patternInfo);
                TheLib.Validate.GreaterOrEqual(startIndex, 0, nameof(startIndex));
                TheLib.Validate.GreaterOrEqual(bitLength, 1, nameof(bitLength));
                TheLib.Validate.InRange(wordLength, 1, 32, nameof(wordLength));
                _bitOrder = msbFirst ? tlBitOrder.MsbFirst : tlBitOrder.LsbFirst;
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Setup.Digital.ReadAll();
                TheLib.Execute.Digital.RunPattern(_patternInfo);
                if (testFunctional) _patResult = TheLib.Acquire.Digital.PatternResults();
                _readWords = TheLib.Acquire.Digital.ReadWords(_pins, startIndex, bitLength, wordLength, _bitOrder);
            }

            if (ShouldRunPostBody) {
                if (testFunctional) TheLib.Datalog.TestFunctional(_patResult, pattern);
                if (testValues) TheLib.Datalog.TestParametric(_readWords);
            }
        }
        #endregion
    }
}
