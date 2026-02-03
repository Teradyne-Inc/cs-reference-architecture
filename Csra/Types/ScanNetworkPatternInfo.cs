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

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern(set) in IGXL.
        /// </summary>
        public string PatternSetName { get; private set; }

        /// <summary>
        /// [<see langword="readonly"/>] The name of the pattern file that contains the contribution bits.
        /// This pattern will be modified during OCComp Diagnosis reburst.
        /// </summary>
        public string SetupPatternFileName { get; private set; }

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
            }

            // Load the SSN_End csv files.
            // In case of concatenated ssn pattern file, the ssnEndCsvFileName can be the same as ssnSetupCsvFileName.
            // in that case, either provide the same csv file name again or leave it empty.
            if (!string.IsNullOrEmpty(endPatternCsvFileName) && endPatternCsvFileName != setupPatternCsvFileName) {
                LoadSsnCsv(endPatternCsvFileName);
            }

            LoadScanNetworkMapfile();

            ProcessCoreList();

            ProcessStickyBitsAndContributionBits();

            void LoadSsnCsv(string ssnCsvFileName) {
                // Load the SSN Setup csv file.
                var ssnCsvFile = new SsnCsvFile(ssnCsvFileName);
                _contribPins ??= string.IsNullOrEmpty(ssnCsvFile.ContribPin) ? null : ssnCsvFile.ContribPin.Split([','], StringSplitOptions.RemoveEmptyEntries); // will only be set once.
                _contribLabel ??= ssnCsvFile.ContribLabel;  // will only be set once. 
                _tckRatio ??= int.TryParse(ssnCsvFile.TckRatio, out var tckRatio) ? tckRatio : null;    // will only be set once. need to check if there is a conflict.
                _stickyPin ??= string.IsNullOrEmpty(ssnCsvFile.StickyPin) ? null : ssnCsvFile.StickyPin;// will only be set once.

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


            }

            void LoadPatternAndTags(string patternSetName) {
                // load the pattern(set). the ScanNetwork map file will also be loaded automatically.
                TheHdw.Patterns(patternSetName).Load();

                // Check if the pattern is a set or a file, and get the MappingFileName accordingly.
                DriverDigPatterns loadedPatterns = TheHdw.Digital.Patterns();
                try {
                    if (loadedPatterns.Sets.Contains(patternSetName)) {
                        // is pattern set
                        //IsPatternSet = true;
                        ScanNetworkMapping = loadedPatterns.Sets[patternSetName].ScanNetworkMappingFileName;
                        // if it is a set, then the first module in the set should be the ssn_setup pattern.
                        SetupPatternFileName = GetFirstModuleFileName(patternSetName);
                    } else if (loadedPatterns.Files.Contains(patternSetName)) {
                        // is single file
                        //IsPatternSet = false;
                        ScanNetworkMapping = loadedPatterns.Files[patternSetName].ScanNetworkMappingFileName;
                        // if it is NOT a set, then it must be a concatenated ssn pattern file.
                        // which means it contains the contribution bits that will be patched during OCComp Diagnosis reburst.
                        SetupPatternFileName = patternSetName;
                    }
                } catch (Exception ex) {
                    throw new ArgumentException($"ERROR: Error occurred in LoadPatternAndTags({patternSetName})", ex);
                }
#if IGXL_99_99_92_uflx
                // Load vector Tags
                string[] patternModules = GetModuleNames(patternSetName);
                _setupPatternModuleName = patternModules.FirstOrDefault();
                _endPatternModuleName = patternModules.LastOrDefault();

                var contribTags = TheHdw.Digital.Patterns().Modules[_setupPatternModuleName].Tags.GetTagIdsAll("disable_on_chip_compare_contribution");
                var stickyTags = TheHdw.Digital.Patterns().Modules[_endPatternModuleName].Tags.GetTagIdsAll("sticky_status");
                string allVectorTags = string.Join("\n", contribTags.Union(stickyTags).Select(tag => $"{tag.VectorNumber}:\t{tag.ElementAtOrDefault(0).Value}"));
                if (!string.IsNullOrEmpty(allVectorTags)) {
                    TheExec.AddOutput($"Vector Tags found in pattern{patternSetName}:\n{allVectorTags}");
                }

                // Load module tags
                string[] contribModuleTags = TheHdw.Digital.Patterns().Modules[_setupPatternModuleName].Tags.ModuleTags
                    .SkipWhile(_ => !_.StartsWith("//SSN instances")).TakeWhile(_ => !_.StartsWith("//End_ssn_instance")).ToArray();
                if (contribModuleTags.Length > 0) {
                    TheExec.AddOutput($"Module Tags found in pattern{patternSetName}:");
                    TheExec.AddOutput($"{string.Join("\n", contribModuleTags)}");
                }
                if (_endPatternModuleName != _setupPatternModuleName) {
                    string[] stickyModuleTags = TheHdw.Digital.Patterns().Modules[_endPatternModuleName].Tags.ModuleTags
                        .SkipWhile(_ => !_.StartsWith("//SSN instances")).TakeWhile(_ => !_.StartsWith("//End_ssn_instance")).ToArray();
                    if (stickyModuleTags.Length > 0) {
                        TheExec.AddOutput($"Module Tags found in pattern{patternSetName}:");
                        TheExec.AddOutput($"{string.Join("\n", stickyModuleTags)}");
                    }
                }
#endif
            }

            void LoadScanNetworkMapfile() {
                // Load the mapping csv file that contains Core List information for Tester Compare.
                // from v2025.8 onward, the ssh-icl-instance names in the mapping file can be suffixed with core-instance names. e.g. ssh-icl-instance@core-instance
                TheHdw.Digital.ScanNetworks[ScanNetworkMapping].CoreNames.ToList().ForEach(sshIclWithCoreName => {
                    // For each core instance, create a new SshIclInstanceInfo object with the instance name.
                    var sshIclInstance = new IclInstanceInfo(sshIclWithCoreName);
                    // update the existing ssh instance or add a new one.
                    if (IclInstance.ContainsKey(sshIclInstance.IclInstanceName)) {
                        // updating existing instance.
                        IclInstance[sshIclInstance.IclInstanceName].UpdateInstanceInfo(
                            sshInstanceName: sshIclInstance.SshInstanceName,        // will NOT update name, only to check for conflict.
                            sshIclInstanceName: sshIclInstance.IclInstanceName,     // will NOT update name, only to check for conflict.
                            coreInstanceName: sshIclInstance.CoreInstanceName       // will NOT update name, only to check for conflict.
                        );
                    } else {
                        // Add the new SshIclInstanceInfo to the SshIclInstance dictionary.
                        IclInstance.Add(sshIclInstance.IclInstanceName, sshIclInstance);
                    }
                });
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

                // generate dictionary for mapping sticky cycles/pin to ssh-icl-instance name.
                _stickyCycles = IclInstance.Values
                    .Where(instance => instance.StickyCycle.HasValue)
                    .ToDictionary(instance => instance.StickyCycle.Value, instance => $"{instance.StickyPin}::{instance.IclInstanceName}");

                // generate NonContiguousModify AllocationName for patching contribution bits during OCComp Diagnosis reburst.
                foreach (string contributionPin in _contribPins) {
                    int[] perPinContribOffsets = IclInstance.Values
                        .Where(instance => instance.ContribPin == contributionPin)
                        .Select(instance => Enumerable.Range(start: instance.ContribOffset.Value, count: (int)_tckRatio).ToArray())
                        .SelectMany(offsets => offsets)
                        .OrderBy(offset => offset)
                        .ToArray();
                    _contribOffsets.Add(contributionPin, perPinContribOffsets);
                    string perPinDisableContribPatternModifyAllocationName = $"{_disableContribPatternModifyAllocationName}__{contributionPin}";
                    if (!TheHdw.Digital.Pins(contributionPin).Patterns(SetupPatternFileName).NonContiguousModify.IsAllocated(perPinDisableContribPatternModifyAllocationName))
                        TheHdw.Digital.Pins(contributionPin).Patterns(SetupPatternFileName).NonContiguousModify
                            .AllocateVectorOffset(perPinDisableContribPatternModifyAllocationName, _contribLabel, ref perPinContribOffsets);
                }
            }

            string GetFirstModuleFileName(string patternSetName) {
                IDigitalPatternSetElement firstElement = TheHdw.Digital.Patterns().Sets[patternSetName].Elements[0];
                if (firstElement.Type == tlPatternSetElementType.Set)
                    return GetFirstModuleFileName(firstElement.Set.Name);
                else if (firstElement.Type == tlPatternSetElementType.File)
                    return firstElement.File.Name;
                else
                    return firstElement.Name;
            }

            string[] GetModuleNames(string patternSetName) {
                return TheHdw.Digital.Patterns().Sets[patternSetName].Modules.List;
            }
        }

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        /// <param name="concatenatedPatternCsvFileName">Specifies the Csv file that comes with the concatenated(set+payload+end) ScanNetwork pattern.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName, string concatenatedPatternCsvFileName):this(patternSetName, concatenatedPatternCsvFileName,"") {
            
        }

        /// <summary>
        /// Construct a new <see cref="ScanNetworkPatternInfo"/> object and load the specified pattern(set).
        /// </summary>
        /// <param name="patternSetName">Specifies the ScanNetwork pattern(set) to load. This can be the name of a pattern file or a pattern set.</param>
        public ScanNetworkPatternInfo(Pattern patternSetName):this(patternSetName, "", "") {

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

            // mask all ssh-icl-instances that are both OCComp and have TC mapping(IsRepresentative):
            TheHdw.Digital.ScanNetworks.ClearAllMasks();
            DriverDigScanNetwork scanNetwork = TheHdw.Digital.ScanNetworks[ScanNetworkMapping];
            DriverDigScanNetworkCoreMasks masks = scanNetwork.CoreMasks;
            foreach (string igxlCoreName in scanNetwork.CoreNames) {
                string sshIclInstanceName = igxlCoreName.Split('@').FirstOrDefault();
                if (IclInstance.ContainsKey(sshIclInstanceName) &&
                    IclInstance[sshIclInstanceName].IsOnChipCompare) {
                    masks.Add(igxlCoreName);
                }
            }
            masks.Apply();

            // enable all contribution bits
            EnableAllContribution();
            PatchingDisableContributionBits(true);
        }

        /// <summary>
        /// Execute the ScanNetwork pattern(set) in non-diagnosis mode with reburst until no more reburst is needed.
        /// </summary>
        public void ExecuteNonDiagnosisBurst() {

            TheHdw.Patterns(PatternSetName).ExecuteSet();
            _reburstCount = 1;
            IScanNetworkResults rsnr = TheHdw.Digital.Patgen.ReadScanNetworkResults();
            _testerCompareResults = rsnr;
            DriverDigScanNetworkCoreMasks masks = TheHdw.Digital.ScanNetworks[ScanNetworkMapping].CoreMasks;
            while (rsnr.ReburstNeeded.Any(true)) {
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
        }

        /// <summary>
        /// Returns a <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.
        /// </summary>
        /// <returns>A <see cref="ScanNetworkPatternResults"/> object that contains the test results of the latest execution of this pattern.</returns>
        public ScanNetworkPatternResults GetScanNetworkPatternResults() {

            var results = new ScanNetworkPatternResults(this);
            // get Tester Compare results       
            foreach (string igxlCoreName in _testerCompareResults.CoreNames) {
                string sshIclInstanceName = igxlCoreName.Split('@').FirstOrDefault();
                string coreInstanceName = igxlCoreName.Split('@').LastOrDefault();
                if (!IclInstance[sshIclInstanceName].IsOnChipCompare) {
                    results.IclInstance[sshIclInstanceName].IsResultValid = !_testerCompareResults.Core(igxlCoreName).Masked.ToSite();
                    results.IclInstance[sshIclInstanceName].IsFailed = _testerCompareResults.Core(igxlCoreName).Failed.ToSite();
                }
            }
            // get On-Chip Compare results
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
            Site<List<string>> iclList = new();
            ForEachSite(site => {
                iclList[site] = IclInstance.Values.Where(icl => coreList[site].Contains(icl.CoreInstanceName)).Select(icl => icl.IclInstanceName).ToList();
            });
            // debug print
            if (debugWriteComment)
                ForEachSite(site => {
                    TheExec.Datalog.WriteComment($"[Site {site}] :: Setup Diagnosis for cores:\n\t{string.Join("\n\t", coreList[site])}\n"
                        + $"active ssh-icl instances:\n\t{string.Join("\n\t", iclList[site])}");
                });
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
        public void MuteAllExcept(Site<List<string>> contributingSshList, bool debugWriteComment = false) {
            // masking TC icls NOT on the list
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
        }
        #endregion

        #region Private/internal functions

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
                TheHdw.Digital.Pins(contributionPin).Patterns(SetupPatternFileName).NonContiguousModify.ModifyVectorData(allocationName, perPinDisableContributionBits);
            }
        }
        #endregion
    }
}
