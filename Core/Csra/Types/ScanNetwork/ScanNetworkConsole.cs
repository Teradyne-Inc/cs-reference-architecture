using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to manage the collection of scan network pattern results and provide utility methods for result processing and display.
    /// This class serves as a central console for handling the outcomes of ScanNetwork pattern executions, allowing for easy aggregation, analysis, and
    /// debugging of test results across multiple ScanNetwork patterns.
    /// </summary>
    [Serializable]
    public sealed class ScanNetworkConsole {

        #region Properties
        private readonly List<ScanNetworkPatternResults> _scanNetworkPatternResults = [];
        public ScanNetworkPatternResults MergedResults { get; private set; } = new();

        public string[] CoreList => _scanNetworkPatternResults.SelectMany(r => r.CoreInstance.Keys).Distinct().ToArray();

        #endregion

        #region Constructors
        public ScanNetworkConsole() {
            // user initialization if needed
        }
        #endregion

        #region Methods

        /// <summary>
        /// Resets the scan network pattern results to their initial state.
        /// </summary>
        /// <remarks>Call this method in OnProgramStarted() to clear all existing scan results and prepare for a new scan operation.
        /// After calling this method, previously merged results will be discarded and replaced with a new, empty result set.</remarks>
        public void Reset() {
            _scanNetworkPatternResults.Clear();
            MergedResults = new ScanNetworkPatternResults();
        }

        /// <summary>
        /// Adds the specified scan network pattern results to the collection and merges them into the aggregated results.
        /// </summary>
        /// <param name="results">The ScanNetwork pattern results to add and merge. Cannot be null.</param>
        public void AddScanNetworkPatternResults(ScanNetworkPatternResults results) {
            if (results is null) {
                throw new ArgumentNullException(nameof(results));
            }
            _scanNetworkPatternResults.Add(results);
            MergedResults.MergeWith(results);
        }

        /// <summary>
        /// incrementally adding failed cores to the ignore list in pattern info for the next pattern execution, so that icl instances in those cores will be
        /// masked/contribution-disabled in the next run. This is useful when you want to focus on still-good cores by excluding known failed cores.
        /// </summary>
        /// <param name="patternInfo">The ScanNetwork pattern object. Cannot be null.</param>
        public void DisableFailedCoresForPattern(ScanNetworkPatternInfo patternInfo) {
            patternInfo.IgnoreFailedInstances(MergedResults, increment: true, maskRepresentatives: false, debugWriteComment: false);
        }

        /// <summary>
        /// Displays the scan network pattern results in the Output window for debugging purposes. This format is subject to change after discussing with
        /// customers.
        /// </summary>
        /// <remarks>This method outputs detailed information about each core and ICL instance contained
        /// in the results to the console. Use this method to inspect scan outcomes during development or
        /// troubleshooting. The output includes failure status, result validity, and error status for each
        /// instance.</remarks>
        /// <param name="results">The results of the scan network pattern operation to be displayed. Cannot be null.</param>
        public void DebugDisplayResults(ScanNetworkPatternResults results) {
            // Implement logic to display the results in the console
            foreach (var core in results.CoreInstance.Values) {
                TheExec.AddOutput($"Core Instance: {core.InstanceName}", Bold: true);
                TheExec.AddOutput($"  Failed: {core.IsFailed}");
                TheExec.AddOutput($"  Valid Result: {core.IsResultValid}");
                TheExec.AddOutput("  ICL Instances:");
                foreach (var iclInstanceName in core) {
                    if (results.IclInstance.TryGetValue(iclInstanceName, out var iclResult)) {
                        TheExec.AddOutput($"    ICL Instance: {iclInstanceName}");
                        TheExec.AddOutput($"      Failed: {iclResult.IsFailed}");
                        TheExec.AddOutput($"      Valid Result: {iclResult.IsResultValid}");
                        TheExec.AddOutput($"      Error Status: {iclResult.ErrorStatus}");
                    }
                }
            }
        }
        #endregion
    }
}
