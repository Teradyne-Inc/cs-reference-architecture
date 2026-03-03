using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store the per core/icl instance test results of a ScanNetwork pattern(set).
    /// </summary>
    [Serializable]
    public class ScanNetworkPatternResults {

        #region Properties

        /// <summary>
        /// Gets the dictionary of test results for each icl-instance, indexed by instance name.
        /// </summary>
        public Dictionary<string, IclInstanceTestResult> IclInstance { get; } = new();

        /// <summary>
        /// Gets the dictionary of test results for each core instances, indexed by instance name.
        /// </summary>
        public Dictionary<string, CoreInstanceTestResult> CoreInstance { get; } = new();

        /// <summary>
        /// User-Defined Test Conditions, such as Voltage/Frequency/Retest#, etc.
        /// </summary>
        public string TestConditions { get; private set; } = string.Empty;
        #endregion

        #region Constructors

        /// <summary>
        /// Create a new instance of <see cref="ScanNetworkPatternResults"/> that can hold the per icl instance test result from a ScanNetwork pattern.
        /// </summary>
        /// <param name="scanNetworkPatternInfo">The <see cref="ScanNetworkPatternInfo"/> object for the ScanNetwork pattern</param>
        public ScanNetworkPatternResults(ScanNetworkPatternInfo scanNetworkPatternInfo) {
            foreach (var instance in scanNetworkPatternInfo.IclInstance) {
                IclInstance.Add(instance.Key, new IclInstanceTestResult(instance.Value));
            }
            foreach (var core in scanNetworkPatternInfo.CoreInstance) {
                CoreInstance.Add(core.Key, new CoreInstanceTestResult(core.Key, core.Value));
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Internal method called by the framework to process the core instance results based on the icl instance results.
        /// </summary>
        internal void ProcessCoreResults() {
            foreach (var core in CoreInstance.Values) {
                core.IsFailed = new Site<bool>(false);
                core.IsResultValid = new Site<bool>(true);
                foreach (var iclInstanceName in core) {
                    if (IclInstance.TryGetValue(iclInstanceName, out var iclInstanceResult)) {
                        core.IsFailed |= iclInstanceResult.IsFailed;
                        core.IsResultValid &= iclInstanceResult.IsResultValid;
                    } else {
                        core.IsResultValid &= false;
                    }
                }
            }
        }

        /// <summary>
        /// Gets the list of failed core instances for each site.
        /// </summary>
        /// <returns>Per site value is a list of failed core instance names.</returns>
        public Site<List<string>> GetFailedCoreInstanceList() {
            Site<List<string>> failedCoresBySite = new();
            ForEachSite(site => {
                failedCoresBySite[site] = GetFailedCoreInstanceList(site);
            });
            return failedCoresBySite;
        }

        /// <summary>
        /// Gets the list of failed core instances for a specified site.
        /// </summary>
        /// <param name="site">The site number.</param>
        /// <returns>A list of failed core instance names for the specified site.</returns>
        public List<string> GetFailedCoreInstanceList(int site) {
            return CoreInstance.Values
                .Where(core => core.IsFailed[site])
                .Select(core => core.InstanceName)
                .ToList();
        }
        #endregion
    }
}
