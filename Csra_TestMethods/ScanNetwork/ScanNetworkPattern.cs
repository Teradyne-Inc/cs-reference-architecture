using System;
using Teradyne.Igxl.Interfaces.Public;
using Csra;
using static Csra.Api;

namespace Demo_CSRA.ScanNetwork {

    [TestClass(Creation.TestInstance), Serializable]
    public class ScanNetworkPattern : TestCodeBase {

        private ScanNetworkPatternInfo _scanNetworkPattern;
        private ScanNetworkPatternResults _nonDiagnosisResults;

        /// <summary>
        /// Executes a ScanNetwork pattern(set) and retrieve per core/icl instance results for each site.
        /// </summary>
        /// <param name="scanNetworkPattern">The ScanNetwork pattern(set) to be executed.</param>
        /// <param name="setupPatternCsv">Optional. The setup.csv file that comes with the ScanNetwork_setup pattern or the concatenated ScanNetwork pattern
        /// (stil)</param>
        /// <param name="endPatternCsv">Optional. The end.csv file that comes with the ScanNetwork_end pattern. Leave blank if the pattern(stil) is a
        /// concatenated ScanNetwork pattern.</param>
        /// <param name="setup">Optional. Setup to be applied before the pattern runs.</param>
        /// <param name="runDiagnosis">Optional. Whether to perform Diagnosis on failed cores after pattern execution.</param>
        /// <param name="multiCoreDiagnosis">Optional. Whether to enable multiple core instances during Diagnosis reburst.<br/>
        ///     <c>false:</c> Diagnosis will be performed one core instance at a time;<br/>
        ///     <c>true:</c> Diagnosis will be performed on as many cores as possible to minimize the number of reburst.
        /// </param>
        #region Baseline
        [TestMethod, Steppable, CustomValidation]
        public void Baseline(Pattern scanNetworkPattern, string setupPatternCsv = "", string endPatternCsv = "",
            string setup = "", bool runDiagnosis = false, bool multiCoreDiagnosis = false, int perPinCaptureLimit = 2048) {

            if (TheExec.Flow.IsValidating) {
                // Load the pattern and initialize the pattern info object (of type <ScanNetworkPatternInfo>) during validation.
                _scanNetworkPattern = new ScanNetworkPatternInfo(scanNetworkPattern, setupPatternCsv, endPatternCsv);
            }

            if (ShouldRunPreBody) {
                TheLib.Setup.LevelsAndTiming.Apply(true);
                Services.Setup.Apply(setup);
            }

            if (ShouldRunBody) {
                // run the ScanNetwork pattern(set) to get a complete overview of pass/fail results of each core/icl instance.
                TheLib.Execute.ScanNetwork.RunPattern(_scanNetworkPattern);

                // fetch the per core/ssh-icl instance pass/fail result and store them in an object of type <ScanNetworkPatternResults>.
                _nonDiagnosisResults = TheLib.Acquire.ScanNetwork.PatternResults(_scanNetworkPattern);

                // user may optionally perform diagnosis on those failed core instances.
                // to do that, 2 prerequisites need to be fulfilled:
                //      1. the pattern(set) must be run with the 'RunPattern' method. This ensures that all icl-instances results are captured.
                //      2. the failed core list is fetched so that diagnosis reburst only enables those failed cores instances for each site.
                if (runDiagnosis) {
                    // sample datalogging of the ScanNetworkPatternResults object. Two flavors demonstrated, this one logs the result by core-instance.
                    TheLib.Datalog.TestScanNetwork(_nonDiagnosisResults,
                        ScanNetworkDatalogOption.LogByCoreInstance | ScanNetworkDatalogOption.LogFailedInstancesOnly);

                    // run the Scan Diagnosis: enable/disable certain cores and reburst the ScanNetwork pattern(set) for failed core instances.
                    // diagnosis can be performed one-core-instance-at-a-time (default, preferred by Siemens) 
                    // or max-number-of-cores-per-reburst (saving test time), switch with 'multiCoreDiagnosis'
                    TheLib.Execute.ScanNetwork.RunDiagnosis(_scanNetworkPattern, _nonDiagnosisResults, perPinCaptureLimit, multiCoreDiagnosis);
                }
            }

            if (ShouldRunPostBody) {
                // if diagnosis is not enabled, datalog of each core-instance results in here.
                if (!runDiagnosis)
                    TheLib.Datalog.TestScanNetwork(_nonDiagnosisResults,
                        ScanNetworkDatalogOption.LogByCoreInstance | ScanNetworkDatalogOption.LogPatGenWithFTR);

                // sample datalog of the ScanNetworkPatternResults object. Two flavors demonstrated, this one logs the result by icl-instance.
                TheLib.Datalog.TestScanNetwork(_nonDiagnosisResults,
                    ScanNetworkDatalogOption.LogByIclInstance | ScanNetworkDatalogOption.LogVerboseInfoByDTR);
            }
        }
        #endregion
    }
}
