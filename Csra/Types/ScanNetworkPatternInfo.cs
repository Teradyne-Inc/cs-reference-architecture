using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;

namespace Csra {

    /// <summary>
    /// Class to store ScanNetwork information about a ScanNetwork pattern(set).
    /// </summary>
    [Serializable]
    public class ScanNetworkPatternInfo {

        #region Properties

        /// <summary>
        /// The ratio of JTAG(TCK) versus Scan Bus (Scan Shift Clock) in the ScanNetwork pattern.
        /// It is used to determine the length of the contribution bits in the ScanNetwork_setup pattern and length of sticky bits in the ScanNetwork_end
        /// pattern.
        /// </summary>
        private int? _tckRatio = null;
        /// <summary>
        /// The pin list is used for modifying the disable-contribution-bits in the ScanNetwork_setup pattern.
        /// In most cases this pin will be 'jtag_tdi'.
        /// </summary>
        private string[] _contribPins = null;
        /// <summary>
        /// The label is used for modifying the disable-contribution-bits in the ScanNetwork_setup pattern.
        /// </summary>
        private string _contribLabel = null;
        /// <summary>
        /// The vector offsets in the ScanNetwork_setup pattern, that contain disable-contribution-bits for patching during OCComp Diagnosis reburst.
        /// Each contribPin has an array of offsets, each offset indicates the start vector of a disable-contribution-bit.
        /// </summary>
        private Dictionary<string, int[]> _contribOffsets = [];
        /// <summary>
        /// The pin list that output the sticky bits in the ScanNetwork_end pattern.
        /// In most cases this pin will be 'jtag_tdo'.
        /// </summary>
        private string _stickyPin = null;
        /// <summary>
        /// The mapping table from absolute cycles of sticky-bits in the ScanNetwork_end pattern module to icl-instance names.
        /// It is used to determine the name of a failed icl-instance.
        /// </summary>
        private Dictionary<double, string> _stickyCycles = [];
        /// <summary>
        /// The <c>AllocationName</c> for <c>NonContiguousModify()</c>,
        /// which is used to set/clear the disable-contribution-bits for diagnosing OnChipCompare icl-instances.
        /// </summary>
        private string _disableContribPatternModifyAllocationName = null;
        /// <summary>
        /// The test results of the TesterCompare after completing a non-diagnosis burst(s).
        /// </summary>
        private IScanNetworkResults _testerCompareResults = null;
        /// <summary>
        /// Counter that records the number of rebursts that have been performed since latest counter reset.
        /// </summary>
        private int _reburstCount = 0;

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern(set) in IGXL.
        /// </summary>
        public string PatternSetName { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern file that contains the contribution bits.
        /// This pattern will be modified during OCComp Diagnosis reburst.
        /// </summary>
        public string SetupPatternName { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The name of the ScanNetwork map file(csv).
        /// This name is used for masking certain time slots on scan out pins by applying TesterCompare Masks.
        /// TesterCompare related methods are all under: <c>TheHdw.Digital.ScanNetwork[ScanNetworkMapping]</c>
        /// </summary>
        public string ScanNetworkMapping { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The dictionary that contains the information of every ICL instance that are tested in this pattern.<br/>
        /// The key is the name of the ICL instance and the value is the associated <see cref="SshIclInstanceInfo"/> object
        /// that contains all the configuration and attributes of that ICL instance.
        /// </summary>
        public Dictionary<string, IclInstanceInfo> IclInstance { get; } = [];

        /// <summary>
        /// [<see langword="readonly"/>] The dictionary that contains the names of all the cores that are tested in this pattern,
        /// and the names of icl instances under each core instance.<br/>
        /// The key of the dictionary is the name of the core instance
        /// and the value is the <see cref="List{String}"/> of icl instances that belong to the core.
        /// </summary>
        public Dictionary<string, List<string>> CoreInstance { get; } = [];

        /// <summary>
        /// [<see langword="readonly"/>] The dictionary that contains the pattern's capture-global-group mapping,
        /// which associates each global group ID with a list of icl instances.<br/>
        /// The key of the dictionary is the ID of the capture-global-group
        /// and the value is the <see cref="List{String}"/> of icl instances that belong to the capture-global-group.
        /// </summary>
        public Dictionary<string, List<string>> CaptureGlobalGroup { get; } = [];
        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        /// <param name="setupPatternCsvFileName">Specifies the Csv file that comes with the ScanNetwork_setup pattern.</param>
        /// <param name="endPatternCsvFileName">Specifies the Csv file that comes with the ScanNetwork_end pattern</param>
        public ScanNetworkPatternInfo(Pattern patternSetName, string setupPatternCsvFileName, string endPatternCsvFileName) {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        /// <param name="concatenatedPatternCsvFileName">Specifies the Csv file that comes with the concatenated(set+payload+end) ScanNetwork pattern.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName, string concatenatedPatternCsvFileName) {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName) {
            throw new NotImplementedException();
        }
        #endregion

        #region Methods

        /// <summary>
        /// Sets all disable_contribution_bits in the ScanNetwork_setup pattern, disabling all OnChipCompare icl-instances from contributing to the ScanNetwork
        /// bus output.
        /// </summary>
        public void SetAllDisableContributionBits() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Clears all disable_contribution_bits, resetting the state to its default.
        /// </summary>
        /// <remarks>This method affects the internal state by removing any flags that disable
        /// contributions. It should be used when a full reset of contribution settings is required.</remarks>
        public void ClearAllDisableContributionBits() {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns a <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.
        /// </summary>
        /// <returns>A <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.</returns>
        public ScanNetworkPatternResults GetScanNetworkPatternResults() {
            throw new NotImplementedException();
        }
        #endregion
    }
}
