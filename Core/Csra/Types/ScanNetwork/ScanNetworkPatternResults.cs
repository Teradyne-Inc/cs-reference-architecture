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
        public string TestConditions { get; set; } = string.Empty;
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

        /// <summary>
        /// Create a new instance of <see cref="ScanNetworkPatternResults"/> by copying another instance. This can be useful when you want to create a new
        /// instance to hold updated results while keeping the original results unchanged.
        /// </summary>
        /// <param name="source"></param>
        public ScanNetworkPatternResults(ScanNetworkPatternResults source) {
            foreach (var kvp in source.IclInstance) {
                IclInstance.Add(kvp.Key, new IclInstanceTestResult(kvp.Value));
            }
            foreach (var kvp in source.CoreInstance) {
                CoreInstance.Add(kvp.Key, new CoreInstanceTestResult(kvp.Value));
            }
            TestConditions = source.TestConditions;
        }

        /// <summary>
        /// Create a new instance of <see cref="ScanNetworkPatternResults"/> with default values.
        /// </summary>        
        public ScanNetworkPatternResults() {
            // Parameterless constructor for serialization or manual population of results
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
            return [..CoreInstance.Values
                .Where(core => core.IsFailed[site])
                .Select(core => core.InstanceName)];
        }

        /// <summary>
        /// Retrieves a mapping of sites to lists of ICL instance identifiers that belong to any failed cores on that site.
        /// </summary>
        /// <remarks>This method aggregates failed ICL instances across all sites. The returned mapping
        /// can be used to identify and process failed cores per site. The method does not modify any site or instance
        /// state.</remarks>
        /// <returns>A <see cref="Site{string[]}"/> containing, for each site, an array of ICL instance identifiers within any failed
        /// cores. If a site has no failed cores, its array will be empty.</returns>
        public Site<string[]> GetFailedIclInstances() {
            Site<string[]> allInstancesOfFailedCoresBySite = new();
            ForEachSite(site => {
                allInstancesOfFailedCoresBySite[site] = GetFailedIclInstances(site);
            });
            return allInstancesOfFailedCoresBySite;
        }

        /// <summary>
        /// Retrieves a list of all the ICL instance identifiers that belongs to failed cores for a specified site.
        /// </summary>
        /// <param name="site">The site number.</param>
        /// <returns>A list of ICL instance identifiers within failed cores for the specified site. If the site has no failed cores, the list will be empty.
        /// </returns>
        public string[] GetFailedIclInstances(int site) {
            return [..CoreInstance.Values
                .Where(core => core.IsFailed[site])
                .SelectMany(core => core)];
        }

        /// <summary>
        /// merges the results from another <see cref="ScanNetworkPatternResults"/> instance into the current instance.
        /// This is useful when you have multiple sets of results for the same pattern and want to combine them to get an overall result.
        /// The merging logic will combine the test results for each core and icl instance, ensuring that if any instance is failed in any of the sets,
        /// it will be marked as failed in the merged results. Similarly, the validity of results will also be combined accordingly.
        /// </summary>
        /// <param name="other"></param>
        public void MergeWith(ScanNetworkPatternResults other) {
            foreach (var kvp in other.IclInstance) {
                if (IclInstance.TryGetValue(kvp.Key, out var existingIclResult)) {
                    existingIclResult.MergeWith(kvp.Value);
                } else {
                    IclInstance[kvp.Key] = new IclInstanceTestResult(kvp.Value);
                }
            }
            foreach (var kvp in other.CoreInstance) {
                if (CoreInstance.TryGetValue(kvp.Key, out var existingCoreResult)) {
                    existingCoreResult.MergeWith(kvp.Value);
                } else {
                    CoreInstance[kvp.Key] = new CoreInstanceTestResult(kvp.Value);
                }
            }
        }
        #endregion
    }
}
