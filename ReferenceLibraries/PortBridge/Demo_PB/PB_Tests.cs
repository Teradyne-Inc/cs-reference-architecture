using System;
using System.Collections.Generic;
using System.Linq;
using Teradyne.Igxl.Interfaces.Public;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;
using PortBridgeUtils;
using Teradyne.PortBridge;
using Csra;
using static Csra.Api;

namespace Demo.PB {
    //Demo.PB
    [TestClass]
    public class PB_Tests : TestCodeBase {

        // Optional: centralize your paths/labels so they're consistent
        private const string _regPath = "PERIPH_A.SUBBLOCK_A.CFG0";
        private const string _blockName = "PERIPH_A";
        private const string _sub_blockName = "SUBBLOCK_A";
        private const string _registerName = "CFG0";
        private const string _fieldName = "SEL";

        /// <summary>
        /// Loads PortBridge + register map, and validates that hierarchical paths resolve.
        /// </summary>
        [TestMethod, Steppable, CustomValidation]
        public void InitialSetup() {

            // PortBridge Setup
            Initialize.PortBridgeSetup();
            PBLib.ResetRegisterAll();

            //TheExec.AddOutput("=== Register Map Debug Info ===");
            Services.Alert.Log("=== Register Map Debug Info ===");

            try {
                IPortBridgeRegisterMap registerMap = PBLib.PortBridge.RegisterMaps[PBLib.REGISTER_MAP];

                // Just confirm the map object exists                            
                Services.Alert.Log($"Register map object retrieved: {PBLib.REGISTER_MAP}");

                if (registerMap.Blocks != null) {
                    Services.Alert.Log($"Top-level Blocks: {registerMap.Blocks.Count}");
                } else {
                    Services.Alert.Log("Blocks is null (dim or array-style map)");
                }
                // DEBUG: Uncomment below for detailed structure debugging if needed
                /*
                foreach (var _blockName in registerMap.Blocks.List) {
                    IPortBridgeRegisterMapBlock block = registerMap.Blocks[_blockName];
                    TheExec.AddOutput($"Block: '{_blockName}'");
                    TheExec.AddOutput($"  Direct Registers: {block.Registers.Count}");
                    TheExec.AddOutput($"  SubBlocks (clusters): {block.SubBlocks.Count}");

                    // List subblocks (clusters)
                    foreach (string _sub_blockName in block.SubBlocks.List) {
                        IPortBridgeRegisterMapBlock subBlock = block.SubBlocks[_sub_blockName];
                        TheExec.AddOutput($"    SubBlock: '{_sub_blockName}'");
                        TheExec.AddOutput($"      Registers: {subBlock.Registers.Count}");

                        foreach (var regName in subBlock.Registers.List) {
                            IPortBridgeRegisterMapRegister reg = subBlock.Registers[regName];
                            TheExec.AddOutput($"      Register: '{regName}' @ 0x{reg.Address:X}");

                            // List fields
                            foreach (var _fieldName in reg.Fields.List) {
                                TheExec.AddOutput($"   Field: '{_fieldName}'");
                            }
                        }
                    }
                }
                */

                // Access CFG0 register directly to verify structure:
                IPortBridgeRegisterMapRegister globReg = registerMap.Blocks[_blockName].SubBlocks[_sub_blockName].Registers[_registerName];
                Services.Alert.Log($"{_registerName} Register found! Address: 0x{globReg.Address:X}");

                // Test PBLib hierarchical access
                if (PBLib.CheckLabel(_regPath)) {
                    Services.Alert.Log($"PBLib can access {_registerName} using hierarchical path '{_regPath}'");
                }

                Services.Alert.Log("Register map loaded successfully.");

            } catch (Exception ex) {
                Services.Alert.Log($"ERROR: {ex.Message}");

            }
        }

        /// <summary>
        /// Reads the GLOB register and field values, then validates them against the specified limits.
        /// </summary>
        /// <param name="regLimit">The expected register value to test against.</param>
        /// <param name="fldLimit">The expected field value to test against.</param>
        [TestMethod, Steppable, CustomValidation]
        public void ReadAndLogRegister(uint regLimit, uint fldLimit) {
            // Use hierarchical path for register access
            Site<int> fldValue = new Site<int>();
            Site<int> regValue = new Site<int>();

            regValue = PBLib.GetRegValue(_regPath);
            fldValue = PBLib.GetFldValue(_regPath, _fieldName);
            TheExec.Flow.TestLimit(regValue, unchecked((int)regLimit), unchecked((int)regLimit),
                         FormatStr: "%x", TName: $"Reg {_registerName}");
            TheExec.Flow.TestLimit(fldValue, unchecked((int)fldLimit), unchecked((int)fldLimit),
                         FormatStr: "%x", TName: $"Fld {_fieldName}");
        }

        /// <summary>
        /// Demonstrates Site&lt;T&gt; generic usage with PortBridge register operations.
        /// Tests reading and writing register and field values using per-site data.
        /// </summary>
        [TestMethod, Steppable, CustomValidation]
        public void Check_SiteGenerics() {

            // Notes to SiteGenerics
            // SiteGenerics to SiteLong
            // SL = SG.ToSiteLong()
            // SiteLong to SiteGenerics
            // SG = SL.ToSite<>(})

            // variable setup
            Site<int> trimValue = new Site<int>();
            Site<int> retValue;

            PBLib.ResetRegisterAll();
            retValue = PBLib.GetRegValue(_regPath);
            TheExec.Flow.TestLimit(retValue, 0x00000000, 0x00000000,
            FormatStr: "%x", TName: $"SG Default {_registerName}");

            ForEachSite(site => {
                trimValue[site] = (site + 1);
            });

            PBLib.SetFldValue(_regPath, _fieldName, trimValue);
            retValue = PBLib.GetRegValue(_regPath);
            TheExec.Flow.TestLimit(retValue, 0x00000001, 0x00000004,
            FormatStr: "%x", TName: $"SG SiteVal Reg {_registerName}");

            retValue = PBLib.GetFldValue(_regPath, _fieldName);
            TheExec.Flow.TestLimit(retValue, 1, 4,
            FormatStr: "%x", TName: $"SG SiteVal Fld {_fieldName}");

            PBLib.SetRegValue(_regPath, trimValue);
            retValue = PBLib.GetRegValue(_regPath);
            TheExec.Flow.TestLimit(retValue, 1, 4,
             FormatStr: "%x", TName: $"SG SiteVal Reg {_registerName}");
        }

        /// <summary>
        /// Verifies register write operations by reading back values and comparing.
        /// Uses Read-Modify-Write-Verify pattern to ensure hardware access is working.
        /// </summary>
        [TestMethod, Steppable, CustomValidation]
        public void VerifyRegisterReadWrite() {

            Services.Alert.Log("=== Starting Register Read/Write Verification ===");

            // Step 1: Reset all registers to known state
            PBLib.ResetRegisterAll();
            Services.Alert.Log("Registers reset to default state");


            // Step 2: Read initial value
            Site<int> initialValue = PBLib.GetRegValue(_regPath);
            ForEachSite(site => {
                Services.Alert.Log($"Site {site} - Initial Register Value: 0x{initialValue[site]:X}");
            });

            // Step 3: Write a known test pattern
            Site<int> testPattern = new Site<int>();
            ForEachSite(site => {
                testPattern[site] = 0xAA + site; // Different value per site
            });

            Services.Alert.Log($"Writing test pattern to {_regPath}...");
            PBLib.SetRegValue(_regPath, testPattern);

            // Step 4: Read back and verify
            Site<int> readbackValue = PBLib.GetRegValue(_regPath);

            bool allSitesMatch = true;
            ForEachSite(site => {
                bool matches = (readbackValue[site] == testPattern[site]);
                allSitesMatch &= matches;
                Services.Alert.Log($"Site {site}: Wrote=0x{testPattern[site]:X}, Read=0x{readbackValue[site]:X}, Match={matches}");
            });

            // Step 5: Test limit to pass/fail the verification - compare per site
            ForEachSite(site => {
                TheExec.Flow.TestLimit(readbackValue[site], testPattern[site], testPattern[site],
                    FormatStr: "%x", TName: $"Verify {_registerName} R/W Site{site}");
            });

            if (allSitesMatch) {
                Services.Alert.Log("Register Read/Write Verification PASSED");
            } else {
                Services.Alert.Log("Register Read/Write Verification FAILED");
            }

            // Step 6: Test field-level access
            Services.Alert.Log($"\n=== Testing Field-Level Access ===");

            Site<int> fieldTestValue = new Site<int>();
            ForEachSite(site => {
                fieldTestValue[site] = 0x0F + site;
            });

            PBLib.SetFldValue(_regPath, _fieldName, fieldTestValue);
            Site<int> fieldReadback = PBLib.GetFldValue(_regPath, _fieldName);

            ForEachSite(site => {
                bool fieldMatches = (fieldReadback[site] == fieldTestValue[site]);
               
                Services.Alert.Log($"Site {site} Field: Wrote=0x{fieldTestValue[site]:X}, Read=0x{fieldReadback[site]:X}, Match={fieldMatches}");
                TheExec.Flow.TestLimit(fieldReadback[site], fieldTestValue[site], fieldTestValue[site],
                              FormatStr: "%x", TName: $"Verify {_fieldName} Field R/W Site{site}");
            });

            // Step 7: Restore to initial state
            PBLib.ResetRegisterAll();
            Services.Alert.Log("Registers restored to default state");
        }
    }
}
