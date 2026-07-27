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
        /// The name of the pattern module that contains the sticky bits for the ssn pattern.
        /// </summary>
        private string _setupPatternModuleName = null;
        /// <summary>
        /// The name of the pattern module that contains the sticky bits for the ssn pattern.
        /// </summary>
        private string _endPatternModuleName = null;

        private string[] _testerCompareInstances = [];
        private string[] _onChipCompareInstances = [];
        private string[] _representativeInstances = [];
        private Site<string[]> _testerCompareMaskedInstances = new Site<string[]>(site => []);
        private Site<string[]> _onChipCompareMaskedInstances = new Site<string[]>(site => []);

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern(set) in IGXL.
        /// </summary>
        public string PatternSetName { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern file that contains the contribution bits.
        /// This pattern will be modified during OCComp Diagnosis reburst.
        /// </summary>
        public string ScanNetworkSetupPatternModuleName { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The name of the ScanNetwork map file(csv).
        /// This name is used for masking certain time slots on scan out pins by applying TesterCompare Masks.
        /// TesterCompare related methods are all under: <c>TheHdw.Digital.ScanNetwork[ScanNetworkMapping]</c>
        /// </summary>
        public string ScanNetworkMapping { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The dictionary that contains the information of every ICL instance that are tested in this pattern.<br/>
        /// The key is the name of the ICL instance and the value is the associated <see cref="IclInstanceInfo"/> object.
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
        /// Gets the names of all available core instances.
        /// </summary>
        /// <remarks>The returned array contains the instance names for each core currently identified from the input files.</remarks>
        public string[] CoreInstances => CoreInstance.Keys.ToArray();

        /// <summary>
        /// Gets the collection of instance names currently registered in the ICL system.
        /// </summary>
        /// <remarks>The returned array contains the instance names for each icl currently identified from the input files.</remarks>
        public string[] IclInstances => IclInstance.Keys.ToArray();
        #endregion

        #region Constructors

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        /// <param name="setupPatternCsvFileName">Specifies the Csv file that comes with the ScanNetwork_setup pattern.</param>
        /// <param name="endPatternCsvFileName">Specifies the Csv file that comes with the ScanNetwork_end pattern</param>
        public ScanNetworkPatternInfo(Pattern patternSetName, string setupPatternCsvFileName, string endPatternCsvFileName) {
            PatternSetName = patternSetName.Value;

            // load the pattern(set). the ScanNetwork map file will also be loaded automatically.
            LoadPatternAndTags(PatternSetName);

            // Load the SSN_Setup csv file.
            // It's ok if no csv file is provided since it might be a TC only ssn pattern,
            // in which case we only need to parse the mapping file.
            if (!string.IsNullOrEmpty(setupPatternCsvFileName)) {
                LoadSsnCsv(setupPatternCsvFileName);
                _disableContribPatternModifyAllocationName = setupPatternCsvFileName;

                // Load the SSN_End csv files.
                // In case of concatenated ssn pattern file, the ssnEndCsvFileName can be the same as ssnSetupCsvFileName.
                // in that case, either provide the same csv file name again or leave it empty.
                if (!string.IsNullOrEmpty(endPatternCsvFileName) && endPatternCsvFileName != setupPatternCsvFileName) {
                    LoadSsnCsv(endPatternCsvFileName);
                }
            } else if (string.IsNullOrEmpty(ScanNetworkMapping)) {
                Api.Services.Alert.Error<ArgumentException>($"No csv file provided for pattern {PatternSetName}, and no ScanNetwork mapping found. " +
                    $"Make sure it's a valid ScanNetwork Pattern.");
            }

            LoadScanNetworkMapfile();

            ProcessCoreList();

            ProcessStickyBitsAndContributionBits();

            void LoadSsnCsv(string ssnCsvFileName) {

                // Load the SSN Setup csv file.
                var ssnCsvFile = new SsnCsvFile(ssnCsvFileName);

                // retrieve the none-instance specific attributes.
                _contribPins ??= string.IsNullOrEmpty(ssnCsvFile.ContribPin) ? null : ssnCsvFile.ContribPin.Split([','], StringSplitOptions.RemoveEmptyEntries); // will only be set once.
                _contribLabel ??= ssnCsvFile.ContribLabel;  // will only be set once. 
                _tckRatio ??= int.TryParse(ssnCsvFile.TckRatio, out var tckRatio) ? tckRatio : null;    // will only be set once. need to check if there is a conflict.
                _stickyPin ??= string.IsNullOrEmpty(ssnCsvFile.StickyPin) ? null : ssnCsvFile.StickyPin;// will only be set once.

                // retrieve instance specific attributes and create/update SshIclInstanceInfo objects accordingly.
                ssnCsvFile.SshInstances.ToList().ForEach(instance => {
                    // Check if the instance already exists in the SshIclInstance dictionary.
                    if (IclInstance.ContainsKey(instance.Value["Icl instance"])) {
                        // update existing instance.
                        IclInstance[instance.Value["Icl instance"]].UpdateInstanceInfo(
                            sshInstanceName: instance.Value["Ssh instance"],
                            sshIclInstanceName: instance.Value["Icl instance"],
                            coreInstanceName: instance.Value["Core instance"],
                            isOnChipCompare: instance.Value.TryGetValue("On chip compare", out string isOnChipCompare) ? (isOnChipCompare == "on") :
                            instance.Value.TryGetValue("Group ID", out string cgg) ? cgg != "0" : false,
                            tckRatio: _tckRatio,
                            contribPin: instance.Value.TryGetValue("Contrib pin", out string contribPin) ? contribPin : null,
                            contribOffset: instance.Value.TryGetValue("Contrib offset", out string contribOffset) ? int.Parse(contribOffset) : null,
                            stickyPin: instance.Value.TryGetValue("Sticky pin", out string stickyPin) ? stickyPin : null,
                            stickyLabel: instance.Value.TryGetValue("Sticky label", out string stickyLabel) ? stickyLabel : string.Empty,
                            stickyOffset: instance.Value.TryGetValue("Sticky offset", out string stickyOffset) ? int.Parse(stickyOffset) : null,
                            stickyCycle: instance.Value.TryGetValue("Sticky cycle", out string stickyCycle) ? double.Parse(stickyCycle) : null,
                            globalGroupID: instance.Value.TryGetValue("Group ID", out string globalGroupID) ? globalGroupID : string.Empty,
                            representativeSsh: instance.Value.TryGetValue("Representative ssh", out string representativeSsh) ? representativeSsh : string.Empty
                        );

                    } else
                        // create new instance.
                        IclInstance.Add(instance.Value["Icl instance"], new IclInstanceInfo(
                            sshInstanceName: instance.Value["Ssh instance"],
                            sshIclInstanceName: instance.Value["Icl instance"],
                            coreInstanceName: instance.Value["Core instance"],
                            isOnChipCompare: instance.Value.TryGetValue("On chip compare", out string isOnChipCompare) ? (isOnChipCompare == "on") :
                            instance.Value.TryGetValue("Group ID", out string cgg) ? cgg != "0" : false,
                            tckRatio: _tckRatio,
                            contribPin: instance.Value.TryGetValue("Contrib pin", out string contribPin) ? contribPin : null,
                            contribOffset: instance.Value.TryGetValue("Contrib offset", out string contribOffset) ? int.Parse(contribOffset) : null,
                            stickyPin: instance.Value.TryGetValue("Sticky pin", out string stickyPin) ? stickyPin : null,
                            stickyLabel: instance.Value.TryGetValue("Sticky label", out string stickyLabel) ? stickyLabel : string.Empty,
                            stickyOffset: instance.Value.TryGetValue("Sticky offset", out string stickyOffset) ? int.Parse(stickyOffset) : null,
                            stickyCycle: instance.Value.TryGetValue("Sticky cycle", out string stickyCycle) ? int.Parse(stickyCycle) : null,
                            globalGroupID: instance.Value.TryGetValue("Group ID", out string globalGroupID) ? globalGroupID : string.Empty,
                            representativeSsh: instance.Value.TryGetValue("Representative ssh", out string representativeSsh) ? representativeSsh : string.Empty
                        ));
                });

                // get the list of ssh-icl-instances that are on-chip-compare from the csv file, and store them in a separate list for easy access during pattern modification in diagnosis.
                _onChipCompareInstances = IclInstance.Values.Where(instance => instance.IsOnChipCompare).Select(instance => instance.IclInstanceName).ToArray();
            }

            void LoadPatternAndTags(string patternName) {

                // load the pattern(set). the ScanNetwork map file will also be loaded automatically.
                TheHdw.Patterns(patternName).Load();

                // Check if the pattern is a set or a file, and get the MappingFileName accordingly.
                DriverDigPatterns loadedPatterns = TheHdw.Digital.Patterns();
                string[] patternModules;
                try {

                    if (loadedPatterns.Sets.Contains(patternName)) {
                        // is pattern set
                        //IsPatternSet = true;
                        ScanNetworkMapping = loadedPatterns.Sets[patternName].ScanNetworkMappingFileName;
                        // if it is a set, then return all the modules in the pattern set.
                        // the first module in the set that contains contribution tags should be the ssn_setup pattern.
                        patternModules = GetModuleNamesFromPatSet(patternName);
                    } else if (loadedPatterns.Files.Contains(patternName)) {
                        // is single file
                        //IsPatternSet = false;
                        ScanNetworkMapping = loadedPatterns.Files[patternName].ScanNetworkMappingFileName;
                        // if it is NOT a set, then it must be a concatenated ssn pattern file.
                        // which means it contains the contribution bits that will be patched during OCComp Diagnosis reburst.
                        // here we also try to get the list of pattern modules in the file, and the first module that contains contribution tags should be
                        // the ssn_setup pattern.
                        patternModules = GetModuleNamesFromFile(patternName);
                    } else {
                        throw new ArgumentException($"ERROR: Pattern {patternName} not found in Loaded Pattern Sets or Files.");
                    }
                } catch (Exception ex) {
                    throw new ArgumentException($"ERROR: Error occurred in LoadPatternAndTags({patternName})", ex);
                }

                // Load vector Tags and get the ssn_setup module and ssn_end module based on the presence of contribution tags and sticky tags, respectively.
                ScanNetworkSetupPatternModuleName = GetSsnSetupPatternModuleNameAndTags(patternModules, out IVectorTagIdsCollection contribTags);
                if (contribTags.Count() > 0) {
                    TheExec.AddOutput($"ScanNetworkSetup pattern module found in Pattern [{patternName}],\n\t modue name = [{ScanNetworkSetupPatternModuleName}]");
                }
                _setupPatternModuleName = ScanNetworkSetupPatternModuleName;
                _endPatternModuleName = GetSsnEndPatternModuleNameAndTags(patternModules, out IVectorTagIdsCollection stickyTags);

                string allVectorTags = string.Join("\n", contribTags.Union(stickyTags).Select(tag => $"{tag.VectorNumber}:\t{tag.ElementAtOrDefault(0).Value}"));
                if (!string.IsNullOrEmpty(allVectorTags)) {
                    TheExec.AddOutput($"Vector Tags for ScanNetwork:\n{allVectorTags}");
                }

#if IGXL_11_09_93_uflx

                // Load module tags
                if (!string.IsNullOrEmpty(_setupPatternModuleName)) {
                    string[] contribModuleTags = TheHdw.Digital.Patterns().Modules[_setupPatternModuleName].Tags.ModuleTags
                    .SkipWhile(_ => !_.StartsWith("//SSN instances")).TakeWhile(_ => !_.StartsWith("//End_ssn_instance")).ToArray();
                    if (contribModuleTags.Length > 0) {
                        TheExec.AddOutput($"Module Tags for ScanNetwork in pattern module [{_setupPatternModuleName}]:");
                        TheExec.AddOutput($"{string.Join("\n", contribModuleTags)}");
                    }
                }

                if (!string.IsNullOrEmpty(_endPatternModuleName) && _endPatternModuleName != _setupPatternModuleName) {
                    string[] stickyModuleTags = TheHdw.Digital.Patterns().Modules[_endPatternModuleName].Tags.ModuleTags
                        .SkipWhile(_ => !_.StartsWith("//SSN instances")).TakeWhile(_ => !_.StartsWith("//End_ssn_instance")).ToArray();
                    if (stickyModuleTags.Length > 0) {
                        TheExec.AddOutput($"Module Tags for ScanNetwork in pattern module [{_endPatternModuleName}]:");
                        TheExec.AddOutput($"{string.Join("\n", stickyModuleTags)}");
                    }
                }
#endif
            }

            void LoadScanNetworkMapfile() {

                // Load the mapping csv file that contains Core List information for Tester Compare.
                // from v2025.8 onward, the ssh-icl-instance names in the mapping file can be suffixed with core-instance names.
                // e.g. ssh-icl-instance@core-instance
                if (!string.IsNullOrEmpty(ScanNetworkMapping)) {
                    string[] igxlInstances = TheHdw.Digital.ScanNetworks[ScanNetworkMapping].CoreNames;
                    List<string> representativeList = new List<string>();
                    foreach (string igxlInstanceName in igxlInstances) {
                        // For each core instance, create a new SshIclInstanceInfo object with the instance name.
                        var igxlInstance = new IclInstanceInfo(igxlInstanceName);
                        // update the existing ssh instance or add a new one.
                        if (IclInstance.TryGetValue(igxlInstance.IclInstanceName, out IclInstanceInfo existingInstance)) {
                            // found by the unique icl instance name, the igxlInstance must be in icl-name or icl@core-name format, either way it can update
                            // the existing instance with igxlInstance name for TesterCompare instance masking, and determine whether it's representative for
                            // Tester Compare.
                            existingInstance.UpdateIgxlInstanceInfo(igxlInstanceName);
                            if (existingInstance.IsOnChipCompare) {
                                representativeList.Add(igxlInstanceName);
                            }
                        } else {
                            // igxlInstance does not contain icl name, it must be core-instance name.
                            // find all icl instances that belong to this core instance and do some smart predictions:
                            var iclInstancesWithSameCore = IclInstance.Values.Where(instance => instance.CoreInstanceName == igxlInstance.CoreInstanceName);
                            if (iclInstancesWithSameCore.Count() == 1) {
                                // if this core instance contains only 1 icl instance, it is most likely that the igxlInstance represents this icl instance for
                                // Tester Compare, Update this sole icl instance with the igxlInstance name.
                                IclInstanceInfo soleInstance = IclInstance[iclInstancesWithSameCore.First().IclInstanceName];
                                soleInstance.UpdateIgxlInstanceInfo(igxlInstanceName);
                                // and add igxlInstance name to the representative instance list if it is OnChipCompare.
                                if (soleInstance.IsOnChipCompare) {
                                    representativeList.Add(igxlInstanceName);
                                }
                            } else if (iclInstancesWithSameCore.Count() > 1 && iclInstancesWithSameCore.Where(icl => !icl.IsOnChipCompare).Count() == 0) {
                                // if there are multiple icl instances belong to the same core instance but none of them is Tester Compare, we can safely add
                                // the igxlInstance as the representative instance for all these icl instances that are OnChipCompare, representative instances
                                // will all be masked in none-diagnosis pattern executions.
                                representativeList.Add(igxlInstanceName);
                            } else {
                                // Otherwise, we won't know which icl instances are represented by the igxlInstance when parsing the Tester Compare test result
                                // so, we will just keep the original ssh-icl-instances, and add this igxlInstance as a separate TesterCompare icl instance that
                                // will not be treated as OnChipCompare representatives.
                                IclInstance.Add(igxlInstance.IclInstanceName, igxlInstance);
                            }
                        }
                    }
                    _representativeInstances = [.. representativeList];
                    _testerCompareInstances = [.. igxlInstances.Except(representativeList)];
                }
            }

            void ProcessCoreList() {
                // Process the icl-to-core mapping and the capture global group mapping.
                foreach (IclInstanceInfo icl in IclInstance.Values) {
                    // Process the icl-to-Core Instance mapping
                    string coreName = icl.CoreInstanceName;
                    if (!CoreInstance.ContainsKey(coreName)) {
                        CoreInstance.Add(coreName, new List<string>());
                    }
                    if (!CoreInstance[coreName].Contains(icl.IclInstanceName))
                        CoreInstance[coreName].Add(icl.IclInstanceName);
                }
            }

            void ProcessStickyBitsAndContributionBits() {
                // generate dictionary for mapping sticky cycles to sticky pins & icl-instance names.
                _stickyCycles = IclInstance.Values
                    .Where(instance => instance.StickyCycle.HasValue)
                    .ToDictionary(instance => instance.StickyCycle.Value, instance => $"{instance.StickyPin}::{instance.IclInstanceName}");

                // generate NonContiguousModify AllocationName for patching contribution bits during OCComp Diagnosis reburst.
                // in case of TC only, we still want to go through the flow but just skip the OCComp part, so set contribPins to empty array instead of null.
                _contribPins ??= [];
                foreach (string contributionPin in _contribPins) {
                    int[] perPinContribOffsets = IclInstance.Values
                        .Where(instance => instance.ContribPin == contributionPin)
                        .Select(instance => Enumerable.Range(start: instance.ContribOffset.Value, count: (int)_tckRatio).ToArray())
                        .SelectMany(offsets => offsets)
                        .OrderBy(offset => offset)
                        .ToArray();
                    _contribOffsets.Add(contributionPin, perPinContribOffsets);
                    string perPinDisableContribPatternModifyAllocationName = $"{_disableContribPatternModifyAllocationName}__{contributionPin}";
                    if (!TheHdw.Digital.Pins(contributionPin).Patterns(ScanNetworkSetupPatternModuleName).NonContiguousModify.IsAllocated(perPinDisableContribPatternModifyAllocationName))
                        TheHdw.Digital.Pins(contributionPin).Patterns(ScanNetworkSetupPatternModuleName).NonContiguousModify
                            .AllocateVectorOffset(perPinDisableContribPatternModifyAllocationName, _contribLabel, ref perPinContribOffsets);
                }
            }

            string GetSsnSetupPatternModuleNameAndTags(string[] moduleList, out IVectorTagIdsCollection contribTags) {
                contribTags = null;
                foreach (string moduleName in moduleList) {
                    contribTags = TheHdw.Digital.Patterns().Modules[moduleName].Tags.GetTagIdsAll("disable_on_chip_compare_contribution");
                    if (contribTags != null && contribTags.Count() > 0) {
                        return moduleName;
                    }
                }
                return moduleList.FirstOrDefault();
            }

            string GetSsnEndPatternModuleNameAndTags(string[] moduleList, out IVectorTagIdsCollection stickyTags) {
                stickyTags = null;
                foreach (string moduleName in moduleList.Reverse()) {
                    stickyTags = TheHdw.Digital.Patterns().Modules[moduleName].Tags.GetTagIdsAll("sticky_status");
                    if (stickyTags != null && stickyTags.Count() > 0) {
                        return moduleName;
                    }
                }
                return moduleList.LastOrDefault();
            }

            string[] GetModuleNamesFromPatSet(string patternSetName) {
                return TheHdw.Digital.Patterns().Sets[patternSetName].Modules.List;
            }

            string[] GetModuleNamesFromFile(string patternSetName) {
                return TheHdw.Digital.Patterns().Files[patternSetName].Modules.List;
            }
        }

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        /// <param name="concatenatedPatternCsvFileName">Specifies the Csv file that comes with the concatenated(set+payload+end) ScanNetwork pattern.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName, string concatenatedPatternCsvFileName) : this(patternSetName, concatenatedPatternCsvFileName, "") {

        }

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName) : this(patternSetName, "", "") {

        }
        #endregion

        #region Methods

        /// <summary>
        /// Prepare the instrument for non-diagnosis burst: Mask representative ssh instances; Enable all contribution bits; Set CMEM to capture sticky bits.
        /// </summary>
        public void SetupNonDiagnosisBurst() {

            // setup cmem for non-diagnosis
            DriverDigCMEM cmem = TheHdw.Digital.CMEM;
            cmem.CentralFields = tlCMEMCaptureFields.ModCycle;
            cmem.SetCaptureConfig(-1, CmemCaptType.Fail, tlCMEMCaptureSource.PassFailData, true);

            // for TesterCompare:
            MaskListedInstances(_testerCompareMaskedInstances, increment: false, maskRepresentatives: true, debugWriteComment: false);
            // for OnChipCompare:
            DisableContributionOfListedInstances(_onChipCompareMaskedInstances, increment: false, debugWriteComment: false);
        }

        /// <summary>
        /// Execute the ScanNetwork pattern(set) in non-diagnosis mode with reburst until no more reburst is needed.
        /// </summary>
        public void ExecuteNonDiagnosisBurst() {

            TheHdw.Patterns(PatternSetName).ExecuteSet();
            TheHdw.Digital.Patgen.HaltWait();
            _reburstCount = 1;

            // no need to reburst if the pattern(set) does not contain atpg_scannetwork metadata, i.e. ScanNetworkMapping is null or empty.
            if (!string.IsNullOrEmpty(ScanNetworkMapping)) {
                IScanNetworkResults rsnr = TheHdw.Digital.Patgen.ReadScanNetworkResults();
                _testerCompareResults = rsnr;
                DriverDigScanNetwork scanNetworks = TheHdw.Digital.ScanNetworks[ScanNetworkMapping];
                DriverDigScanNetworkCoreMasks masks = scanNetworks.CoreMasks;
                // Utopia will never need reburst.
                while (rsnr.ReburstNeeded.Any(true) && masks.Count.ToSite().Min() < scanNetworks.CoreNames.Count()) {
                    // mask
                    masks.AddPerSite(rsnr.FailedCores);
                    masks.Apply();
                    // reburst
                    TheHdw.Patterns(PatternSetName).ExecuteSet();
                    TheHdw.Digital.Patgen.HaltWait();
                    _reburstCount++;
                    // get result
                    rsnr = TheHdw.Digital.Patgen.ReadScanNetworkResults();
                    _testerCompareResults.MergeWith(rsnr);
                }
                if (_reburstCount > 1) {
                    // if reburst occurred, need to restore the TesterCompare mask to the original state for later diagnosis flow.
                    MaskListedInstances(_testerCompareMaskedInstances, increment: false, maskRepresentatives: true, debugWriteComment: false);
                }
            }
        }

        /// <summary>
        /// Returns a <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.
        /// </summary>
        /// <returns>A <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.</returns>
        public ScanNetworkPatternResults GetScanNetworkPatternResults() {

            var results = new ScanNetworkPatternResults(this);

            // get TesterCompare results if applicable
            if (_testerCompareResults != null) {
                foreach (string igxlCoreName in _testerCompareInstances) {
                    foreach (var instance in IclInstance.Values.Where(instance => instance.IgxlInstanceName == igxlCoreName)) {
                        results.IclInstance[instance.IclInstanceName].IsResultValid = !_testerCompareResults.Core(igxlCoreName).Masked.ToSite();
                        results.IclInstance[instance.IclInstanceName].IsFailed = _testerCompareResults.Core(igxlCoreName).Failed.ToSite();
                    }
                }
            }

            // get OnChipCompare results if applicable
            if (!string.IsNullOrEmpty(_stickyPin)) {
                ICmemModuleCycleData cmemData = TheHdw.Digital.Pins(_stickyPin).CMEM.ModuleCycleData(true, false, -1);
                List<string> stickyPins = _stickyPin.Split([','], StringSplitOptions.None).ToList();
                _stickyCycles.Values.Select(pinToIcl => pinToIcl.Split(["::"], StringSplitOptions.None).LastOrDefault()).ToList()
                    .ForEach(instance => results.IclInstance[instance].IsFailed ??= new Site<bool>(false));
                ForEachSite(site => {
                    List<double> perSiteFailedCycles = ((double[])cmemData.CycleOffsets[site]).ToList();
                    Dictionary<string, byte[]> perSitePinCycleData = [];
                    stickyPins.ForEach(pin => {
                        perSitePinCycleData.Add(pin, (byte[])cmemData.CycleData.Pins[pin].get_Value(site));
                    });
                    _stickyCycles.Where(x => {
                        int index = perSiteFailedCycles.IndexOf(x.Key);
                        if (index != -1) {
                            string pinName = x.Value.Split(["::"], StringSplitOptions.None).FirstOrDefault();
                            return perSitePinCycleData[pinName][index] == 1;
                        } else return false;
                    }).ToList().ForEach(x => {
                        string failedInstanceName = x.Value.Split(["::"], StringSplitOptions.None).LastOrDefault();
                        results.IclInstance[failedInstanceName].IsFailed[site] = true;
                    });
                    _stickyCycles.Values.Select(pinToIcl => pinToIcl.Split(["::"], StringSplitOptions.None).LastOrDefault()).ToList()
                    .ForEach(instance => results.IclInstance[instance].IsResultValid[site] = IclInstance[instance].ModifyVectorData[site].Contains("0"));
                });
            }

            // get core level results
            results.ProcessCoreResults();
            return results;
        }

        /// <summary>
        /// Execute diagnosis rebursts for failed cores sequentially until all failed cores are diagnosed.
        /// </summary>
        /// <param name="nonDiagnosisResults"></param>
        public void ExecuteDiagnosisCoresSequential(ScanNetworkPatternResults nonDiagnosisResults) {

            Site<List<string>> failedCoreList = nonDiagnosisResults.GetFailedCoreInstanceList();
            while (failedCoreList.Any(list => list.Count > 0)) {
                Site<List<string>> coreToDiagnose = GetNextToBeTestedCoreFrom(failedCoreList);
                SetupDiagnosisForCore(coreToDiagnose, true);
                ExecuteDiagnosisReburst(true);
            }
        }

        /// <summary>
        /// Execute diagnosis rebursts for failed cores concurrently until all failed cores are diagnosed.
        /// </summary>
        /// <param name="nonDiagnosisResults"></param>
        public void ExecuteDiagnosisCoresConcurrent(ScanNetworkPatternResults nonDiagnosisResults) {

            Site<List<string>> failedCoreList = nonDiagnosisResults.GetFailedCoreInstanceList();
            while (failedCoreList.Any(list => list.Count > 0)) {
                Site<List<string>> coresToDiagnose = GetConcurrentTestableCoreListFrom(failedCoreList);
                SetupDiagnosisForCore(coresToDiagnose, true);
                ExecuteDiagnosisReburst(true);
            }
        }

        /// <summary>
        /// Prepare the instrument for diagnosis of specified cores: Mask non-relevant ssh-icl-instances; Set disable contribution bits for non-relevant OCComp icl-instances.
        /// </summary>
        /// <param name="coreList"></param>
        /// <param name="debugWriteComment"></param>
        public void SetupDiagnosisForCore(Site<List<string>> coreList, bool debugWriteComment = false) {

            // generate icl list that need to be enabled
            Site<string[]> iclList = new();
            ForEachSite(site => {
                iclList[site] = [.. IclInstance.Values.Where(icl => coreList[site].Contains(icl.CoreInstanceName)).Select(icl => icl.IclInstanceName)];
            });
            // debug print
            if (debugWriteComment) {
                ForEachSite(site => {
                    TheExec.Datalog.WriteComment($"[Site {site}] :: Setup Diagnosis for cores:\n\t{string.Join("\n\t", coreList[site])}\n"
                        + $"active ssh-icl instances:\n\t{string.Join("\n\t", iclList[site])}");
                });
            }

            // perform mask/disable for each icl
            MuteAllExcept(iclList, debugWriteComment);
        }

        /// <summary>
        /// Execute a diagnosis reburst: increment reburst count; execute pattern(set); write FTR &amp; STR.
        /// </summary>
        /// <param name="debugWriteComment"></param>
        public void ExecuteDiagnosisReburst(bool debugWriteComment = false) {

            // record reburst count.
            _reburstCount++;
            if (debugWriteComment) {
                List<string> siteNumbers = [];
                ForEachSite(site => {
                    siteNumbers.Add($"{site}");
                });
                TheExec.Datalog.WriteComment($"[Site {string.Join(",", siteNumbers)}] :: Diagnosis reburst #{_reburstCount}\n");
            }

            // execute pattern(set) and write FTR & STR.
            IPatternSetResult[] patternSetResult = TheHdw.Patterns(PatternSetName).ExecuteSet();
            foreach (IPatternSetResult patternModuleResult in patternSetResult) {
                TheExec.Flow.FunctionalTestLimit(patternModuleResult, PatternSetName);
            }
        }

        /// <summary>
        /// Sets all disable_contribution_bits in the ScanNetwork_setup pattern, disabling all OnChipCompare icl-instances from contributing to the ScanNetwork
        /// bus output.
        /// </summary>
        public void SetAllDisableContributionBits() {
            foreach (var icl in IclInstance.Values) {
                ForEachSite(site => icl.SetDisableContributionBit(site, '1'));
            }
        }

        /// <summary>
        /// Clears all disable_contribution_bits, resetting the state to its default.
        /// </summary>
        /// <remarks>This method affects the internal state by removing any flags that disable
        /// contributions. It should be used when a full reset of contribution settings is required.</remarks>
        public void ClearAllDisableContributionBits() {
            foreach (var icl in IclInstance.Values) {
                ForEachSite(site => icl.SetDisableContributionBit(site, '0'));
            }
        }

        [Obsolete("EnableAllContribution is deprecated. Use ClearAllDisableContributionBits instead.")]
        public void EnableAllContribution() {
            ClearAllDisableContributionBits();
        }

        /// <summary>
        /// Sets all disable_contribution_bits except for those icl-instances specified in the contributingSshList parameter.
        /// </summary>
        /// <param name="contributingSshList">(Per site)List of ssh instances that should remain 'contributing' for diagnosis.</param>
        /// <param name="debugWriteComment">For debug use</param>
        public void MuteAllExcept(Site<string[]> contributingSshList, bool debugWriteComment = false) {

#if true
            // masking TC icls NOT on the list
            if (!string.IsNullOrEmpty(ScanNetworkMapping)) {
                DriverDigScanNetwork scanNetworks = TheHdw.Digital.ScanNetworks[ScanNetworkMapping];
                DriverDigScanNetworkCoreMasks masks = scanNetworks.CoreMasks;
                Site<string[]> maskingCores = new(site => new string[0]);
                masks.RemoveAll();
                foreach (string igxlCore in scanNetworks.CoreNames) {
                    string iclName = igxlCore.Split('@').First();
                    if (!IclInstance[iclName].IsOnChipCompare) {
                        ForEachSite(site => {
                            if (!contributingSshList[site].Contains(iclName)) {
                                //maskingCores[site].Append(igxlCore);  // cannot use Append() directly, IGXL-123941
                                string[] tmpStringArray = maskingCores[site].Append(igxlCore).ToArray();
                                maskingCores[site] = tmpStringArray;
                            }
                        });
                    }
                }
                masks.AddPerSite(maskingCores.ToSiteVariant());
                masks.Apply();
            }

            // disable OCComp icls not on the list
            ForEachSite(site => {
                IclInstance.Values.Where(icl => icl.IsOnChipCompare).ToList().ForEach(icl => {
                    if (contributingSshList[site].Contains(icl.IclInstanceName)) {
                        icl.SetDisableContributionBit(site, '0');
                    } else {
                        icl.SetDisableContributionBit(site, '1');
                    }
                });
            });
            PatchingDisableContributionBits(debugWriteComment);
#else
            IgnoreListedInstances(contributingSshList, increment: false, maskRepresentatives: false, debugWriteComment: debugWriteComment);
#endif

        }

        /// <summary>
        /// Sets disable_contribution_bits or perform TesterCompare mask for those instances specified in the instanceList parameter.
        /// </summary>
        /// <param name="instanceList">(Per site)List of ssh instances that should be masked/disable_contribution for upcoming Pattern Burst.</param>
        /// <param name="debugWriteComment">For debug use</param>
        public void IgnoreListedInstances(Site<string[]> instanceList, bool increment, bool maskRepresentatives, bool debugWriteComment = false) {

            // get the corresponding igxl instances for Tester Compare masking, it is possible that some of the instances don't have Tester Compare mapping,
            // in that case they will just be ignored for masking but still have their disable-contribution-bit set if they are OnChipCompare instances.
            var testerCompareInstances = GetIgxlInstancesFrom(instanceList);
            MaskListedInstances(testerCompareInstances, increment: increment, maskRepresentatives: maskRepresentatives, debugWriteComment: debugWriteComment);
            // get OnChipCompare icl instances that need to be disabled for contribution, it is possible that a ssh instance is enlisted but not tested
            // in the current pattern, thus will be disregarded since there is no corresponding contribution bit or sticky bit for it in the pattern.
            var onChipCompareInstances = GetCommonInstances(instanceList, _onChipCompareInstances);
            DisableContributionOfListedInstances(onChipCompareInstances, increment: increment, debugWriteComment: debugWriteComment);

        }

        /// <summary>
        /// Sets disable_contribution_bits or perform TesterCompare mask for those failed instances in the test results, preparing for the next pattern burst.
        /// </summary>
        /// <param name="results">
        /// </param>
        /// <param name="increment">whether to keep the original ignored instances</param>
        /// <param name="debugWriteComment">For debug use</param>
        public void IgnoreFailedInstances(ScanNetworkPatternResults results, bool increment, bool maskRepresentatives, bool debugWriteComment = false) {
            Site<string[]> failedIclInstances = results.GetFailedIclInstances();
            IgnoreListedInstances(failedIclInstances, increment, maskRepresentatives, debugWriteComment);
        }

        /// <summary>
        /// perform TesterCompare mask for specified instances in the instanceList parameter.
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="increment">whether to keep the existing masked instances</param>
        /// <param name="debugWriteComment">For debug use</param>
        public void MaskListedInstances(Site<string[]> instanceList, bool increment, bool maskRepresentatives, bool debugWriteComment = false) {
            // masking TC icls on the list
            if (!string.IsNullOrEmpty(ScanNetworkMapping)) {
                DriverDigScanNetwork scanNetworks = TheHdw.Digital.ScanNetworks[ScanNetworkMapping];
                DriverDigScanNetworkCoreMasks masks = scanNetworks.CoreMasks;
                if (!increment) masks.RemoveAll();
                if (debugWriteComment) {
                    string loggedText = $"[TesterCompare per-site masked instances]\n";
                    ForEachSite(site => {
                        loggedText += $"[Site {site}] :: {string.Join(", ", instanceList[site])}\n";
                    });
                    TheExec.Datalog.WriteComment(loggedText);
                }
                if (maskRepresentatives) {
                    foreach (string representativeSsh in _representativeInstances) {
                        masks.Add(representativeSsh);
                    }
                }
                masks.AddPerSite(instanceList.ToSiteVariant());
                masks.Apply();
                if (increment) {
                    ForEachSite(site => {
                        _testerCompareMaskedInstances[site] = _testerCompareMaskedInstances[site].Union(instanceList[site]).Distinct().ToArray();
                    });
                } else {
                    _testerCompareMaskedInstances = instanceList;
                }
            }
        }

        /// <summary>
        /// Sets disable_contribution_bits for OnChipCompare icl-instances specified in the instanceList parameter.
        /// </summary>
        /// <param name="instanceList"></param>
        /// <param name="increment">whether to keep the existing masked instances</param>
        /// <param name="debugWriteComment">For debug use</param>
        public void DisableContributionOfListedInstances(Site<string[]> instanceList, bool increment, bool debugWriteComment = false) {
            // disable OCComp ssh-icl-instances on the list
            ForEachSite(site => {
                foreach (string icl in _onChipCompareInstances) {
                    if (instanceList[site].Contains(icl)) {
                        IclInstance[icl].SetDisableContributionBit(site, '1');
                    } else if (!increment) {
                        IclInstance[icl].SetDisableContributionBit(site, '0');
                    }
                }
            });
            PatchingDisableContributionBits(debugWriteComment);
            if (increment) {
                ForEachSite(site => {
                    _onChipCompareMaskedInstances[site] = _onChipCompareMaskedInstances[site].Union(instanceList[site]).Distinct().ToArray();
                });
            } else {
                _onChipCompareMaskedInstances = instanceList;
            }
        }
        #endregion

        #region Private/internal functions

        internal Site<T[]> GetCommonInstances<T>(Site<T[]> list1, T[] list2) {
            Site<T[]> commonItems = new();
            ForEachSite(site => {
                commonItems[site] = [.. list1[site].Intersect(list2)];
            });
            return commonItems;
        }
        internal Site<T[]> GetCommonInstances<T>(Site<List<T>> list1, T[] list2) {
            Site<T[]> commonItems = new();
            ForEachSite(site => {
                commonItems[site] = [.. list1[site].Intersect(list2)];
            });
            return commonItems;
        }

        internal Site<string[]> GetIgxlInstancesFrom(Site<string[]> instanceList) {
            Site<string[]> testerCompareInstances = new Site<string[]>(site => []);
            ForEachSite(site => {
                testerCompareInstances[site] = [..instanceList[site]
                    .Where(icl => IclInstance.TryGetValue(icl, out IclInstanceInfo instance)
                                  && !instance.IsOnChipCompare && !string.IsNullOrEmpty(instance.IgxlInstanceName))
                    .Select(icl => IclInstance[icl].IgxlInstanceName).Distinct()];
            });
            return testerCompareInstances;
        }
        internal Site<string[]> GetIgxlInstancesFrom(Site<List<string>> instanceList) {
            Site<string[]> testerCompareInstances = new Site<string[]>(site => []);
            ForEachSite(site => {
                testerCompareInstances[site] = [..instanceList[site]
                    .Where(icl => IclInstance.TryGetValue(icl, out IclInstanceInfo instance)
                                  && !instance.IsOnChipCompare && !string.IsNullOrEmpty(instance.IgxlInstanceName))
                    .Select(icl => IclInstance[icl].IgxlInstanceName).Distinct()];
            });
            return testerCompareInstances;
        }

        internal Site<List<string>> GetNextToBeTestedCoreFrom(Site<List<string>> waitingList) {
            Site<List<string>> resultList = new();
            ForEachSite(site => {
                resultList[site] = new List<string>();
                string x = waitingList[site].FirstOrDefault();
                if (!string.IsNullOrEmpty(x)) {
                    // .Add() and .Remove() used to work...
                    // workaround is either in above IGXL_10_60_01_uflx or this one:
                    {
                        // resultList[site].Add(x);    // List under Site<T> won't add/remove, IGXL-137489
                        var tmpList = resultList[site];
                        tmpList.Add(x);
                        resultList[site] = tmpList;
                        // waitingList[site]).Remove(x);
                        tmpList = waitingList[site];
                        tmpList.Remove(x);
                        waitingList[site] = tmpList;
                    }
                }
            });
            return resultList;
        }

        internal Site<List<string>> GetConcurrentTestableCoreListFrom(Site<List<string>> waitingList) {
            Site<List<string>> resultList = new();
            ForEachSite(site => {
#if true    //work-around for IGXL-137489
                List<string> selectedCoreList = [];
                List<string> remainingList = waitingList[site];
                foreach (var core in remainingList) {
                    if (!IsSharingGlobalGroupID(core, selectedCoreList)) {
                        selectedCoreList.Add(core);
                    }
                }
                resultList[site] = selectedCoreList;
                waitingList[site] = remainingList.Except(selectedCoreList).ToList();
#else       // until IGXL-137489 is resolved
                resultList[site] = [];
                foreach (var core in waitingList[site]) {
                    if (!IsSharingGlobalGroupID(core, resultList[site])) {
                        resultList[site].Add(core); // List under Site<T> won't add/remove, IGXL-137489
                    }
                }
                waitingList[site] = waitingList[site].Except(resultList[site]).ToList();
#endif
            });
            return resultList;

            bool IsSharingGlobalGroupID(string coreInstanceName, List<string> coreList) {
                foreach (var existingCore in coreList) {
                    if (IclInstance.Where(x => CoreInstance[coreInstanceName].Contains(x.Key))
                        .Select(x => x.Value.GlobalGroupID)
                        .Intersect(
                        IclInstance.Where(y => CoreInstance[existingCore].Contains(y.Key))
                        .Select(y => y.Value.GlobalGroupID)
                        ).Any()) { return true; }
                }
                return false;
            }
        }

        private void PatchingDisableContributionBits(bool debugWriteComment = false) {
            foreach (string contributionPin in _contribPins) {
                Site<string[]> perPinStringArray = new(site => [string.Empty]);
                ForEachSite(site => {
                    string[] tmpstrarray = [""];
                    // perPinStringArray[site][0] = "whatever string"; >> this doesn't work now. IGXL-123941
                    tmpstrarray[0] = new string(IclInstance.Values
                        .Where(icl => icl.ContribPin == contributionPin)
                        .OrderBy(icl => icl.ContribOffset)
                        .SelectMany(icl => icl.ModifyVectorData[site])
                        .ToArray()
                        );
                    perPinStringArray[site] = tmpstrarray;
                    if (debugWriteComment)
                        TheExec.Datalog.WriteComment($"Patching Pin:{contributionPin}[site {site}] with new data: {perPinStringArray[site][0]}");
                });
                SiteVariant perPinDisableContributionBits = perPinStringArray.ToSiteVariant();
                string allocationName = $"{_disableContribPatternModifyAllocationName}__{contributionPin}";
                TheHdw.Digital.Pins(contributionPin).Patterns(ScanNetworkSetupPatternModuleName).NonContiguousModify.ModifyVectorData(allocationName, perPinDisableContributionBits);
            }
        }
        #endregion
    }
}
