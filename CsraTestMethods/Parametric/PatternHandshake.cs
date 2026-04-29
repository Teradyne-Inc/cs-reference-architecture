using System;
using System.Collections.Generic;
using System.Linq;
using Csra;
using Csra.Interfaces;
using Teradyne.Igxl.Interfaces.Public;
using static Csra.Api;

namespace CsraTestMethods.Parametric {

    [TestClass(Creation.TestInstance), Serializable]
    public class PatternHandshake : TestCodeBase {

        /// <summary>
        /// The interface implements IExecutable and should be implemented by the class
        /// that is passed to the TestMethod by its fully-qualified name.
        /// Result must be cleared before use otherwise it contains results from previous runs.
        /// </summary>
        public interface IPatternHandshakeBaselineExecutable : IExecutable {
            public Pins Vcc { get; set; }
            public List<PinSite<double>> Result { get; }
        }

        private List<PatternInfo> _pattern;
        private Pins _pins;
        private IPatternHandshakeBaselineExecutable _executableObject;
        private Site<bool> _patResult;


        /// <summary>
        /// Runs the specified pattern and executes executableClass's Execute() at each occurance of stopFlag in the pattern. Every
        /// result will be datalogged.
        /// </summary>
        /// <param name="pattern">Pattern name to be executed.</param>
        /// <param name="stopFlag">Pattern flag to stop at.</param>
        /// <param name="numberOfStops">Number of total stop in the pattern.</param>
        /// <param name="executableClass">Fully-qualified name of class which contains properties and Execute() to be called at each stop.</param>
        /// <param name="testFunctional">Whether to test the functional results.</param>
        /// <param name="setup">Optional. Action to configure the dib or device.</param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern pattern, int stopFlag, int numberOfStops, string executableClass, bool testFunctional, string setup = "") {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.GreaterOrEqual(stopFlag, 0, nameof(stopFlag));
                TheLib.Validate.GreaterOrEqual(numberOfStops, 0, nameof(numberOfStops));
                _pattern[0].SetFlags = stopFlag;
                TheLib.Validate.GetObjectByClassName(executableClass, out _executableObject);
                _executableObject.Vcc = new Pins("vcc");
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                TheLib.Execute.Digital.RunPatternConditionalStop(_pattern[0], numberOfStops, _executableObject);
                if (testFunctional) _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
                if (testFunctional) TheLib.Datalog.TestFunctional(_patResult, pattern);
                foreach (PinSite<double> value in _executableObject.Result) {
                    TheLib.Datalog.TestParametric(value);
                }
            }
        }
        #endregion


        /// <summary>
        /// Demonstrates modifying vector block data using different data ordering methods and reading back pattern values.
        /// </summary>
        /// <param name="forcePinList">The pins to modify in the pattern.</param>
        /// <param name="pattern">The pattern to execute.</param>
        /// <param name="patternModule">The pattern module containing the vector block.</param>
        #region PatternLabelOverWrite
        [TestMethod, Steppable, CustomValidation]
        public void PatternLabelOverWrite(PinList forcePinList, Pattern pattern, string patternModule) {

            if (TheExec.Flow.IsValidating) {
                TheLib.Validate.Pattern(pattern, nameof(pattern), out _pattern);
                TheLib.Validate.Pins(forcePinList, nameof(forcePinList), out _pins);
                string[] labels = _pattern[0].GetModuleLabelList(patternModule);
                if (labels is null || labels.Length == 0) {
                    throw new ArgumentException($"Pattern module '{patternModule}' does not contain any labels.", nameof(patternModule));
                }

                string label = labels[0];
                string[] pinDataArray = { "0000"/*P1P2P3P4*/, "1100", "1010" };
                _pattern[0].ModifyVectorBlockData(_pins, patternModule, label, 0, ref pinDataArray);
                PrintPatternsValues(_pins, label, patternModule);

                string[] pinDataArray1 = { "000"/*P1P1P1*/, "100"/*P2P3P4*/, "101", "101" };
                _pattern[0].ModifyVectorBlockDataPinOrder(_pins, patternModule, label, 0, ref pinDataArray1);
                PrintPatternsValues(_pins, label, patternModule);

                string pinData = "010"; /*P1P1P1*/
                string firstPin = _pins.FirstOrDefault() ?? string.Empty;
                _pattern[0].ModifyVectorBlockDataPinOrder(new Pins(firstPin), patternModule, label, 0, ref pinData);
                PrintPatternsValues(_pins, label, patternModule);
            }

            if (ShouldRunPreBody) {
            //    TheLib.Setup.LevelsAndTiming.Apply(true);
            }

            if (ShouldRunBody) {
            //    TheLib.Execute.Digital.RunPattern(_pattern[0]);
            //    _patResult = TheLib.Acquire.Digital.PatternResults();
            }

            if (ShouldRunPostBody) {
            //    TheLib.Datalog.TestFunctional(_patResult, pattern);
            }
        }
        #endregion

        /// <summary>
        /// Reads back vector data from the pattern and logs each site's values to the output file.
        /// </summary>
        /// <param name="pins">The pins to read data from.</param>
        /// <param name="pattern">The pattern info list containing the pattern name.</param>
        /// <param name="label">The label identifying the vector block.</param>
        /// <param name="patternModule">The pattern module name.</param>
        #region Internal Helper Methods
        private void PrintPatternsValues(Pins pins, string label, string patternModule) {

            Services.Alert.OutputFile = TheProgram.Path + "\\PrintPatternsValues.txt";

            TheHdw.Digital.Pins(pins.ToString()).Patterns(patternModule)
                .GetVectorData(label, 0, 5, tlVectorPinDataOrientation.StringOfVectors, out ISiteVariant dataStr, out _);

            Site<string[]> output = dataStr.ToSite<string[]>();
            ForEachSite(site => {
                string[] siteData = output[site];
                if (siteData == null || siteData.Length == 0) {
                    Services.Alert.Log($"Site {site}: <empty>");
                    return;
                }

                for (int i = 0; i < siteData.Length; i++) {
                    Services.Alert.LogToFile("IG-XL-PatternLabelOverWrite", $"Site {site} P[{i}]: {siteData[i]}");
                }
            });
        }
        #endregion
    }
}





