using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Teradyne.Igxl.Interfaces.Public;
using Teradyne.PortBridge;
using Teradyne.PortBridge.Utilities.CodeGeneration;
using static Teradyne.Igxl.Interfaces.Public.Constants.Global_Units;

namespace PortBridgeUtils {
    /// <summary>
    /// PortBridge Library (PBLib) provides a simplified C# API for register and field access operations.
    /// This utility class wraps Teradyne PortBridge functionality to enable hierarchical register map navigation,
    /// read/write operations on registers and fields, and seamless conversion between Site&lt;int&gt;, SiteLong, and SiteReg32 types.
    /// Supports both flat and hierarchical register paths (e.g., 'REGISTER' or 'BLOCK.SUBBLOCK.REGISTER').
    /// </summary>
    [TestClass]
    public partial class PBLib : TestCodeBase {

        public static IPortBridgeLanguage PortBridge = new PortBridgeLanguage_Remote("IG.NET");
        private static string _registerMap;
        /// <summary>
        /// Gets the register map name. Must be set via <see cref="Initialize"/> before accessing register map features.
        /// </summary>
        public static string REGISTER_MAP {
            get {
                if (_registerMap is null)
                    throw new InvalidOperationException("Register map name has not been specified. Call PBLib.Initialize(registerMapName) before using register map features.");
                return _registerMap;
            }
        }
        /// <summary>
        /// Initializes PBLib with the specified register map name.
        /// The register map name is optional and only required when using register map features.
        /// </summary>
        /// <param name="registerMapName">The name of the register map (e.g., "Demo"). Optional if not using register map features.</param>
        public static void Initialize(string registerMapName = null) {
            _registerMap = registerMapName;
        }


        /// <summary>
        /// Helper method to parse hierarchical register path
        /// Returns tuple of (_blockName, _sub_blockName, _registerName) or (null, null, _registerName) for flat access
        /// </summary>
        private static (string block, string subBlock, string register) ParseRegisterPath(string regName) {
            if (regName.Contains(".")) {
                string[] parts = regName.Split('.');
                if (parts.Length == 3) {
                    // Format: Block.SubBlock.Register
                    return (parts[0], parts[1], parts[2]);
                } else if (parts.Length == 2) {
                    // Format: Block.Register
                    return (parts[0], null, parts[1]);
                }
                throw new ArgumentException($"Invalid hierarchical path format: {regName}. Expected 'Block.Register' or 'Block.SubBlock.Register'");
            }
            return (null, null, regName);
        }

        /// <summary>
        /// Open PortBridge Debug Tool with specified module
        /// </summary>
        /// <param name="moduleName">Module Name</param>
        public static void OpenInDebugTool(string moduleName) {

            PortBridge.Tests[moduleName].OpenInDebugTool();
        }


        /// <summary>
        /// Verify is the label is part of register map
        /// </summary>
        /// <param name="regName"> regName</param>
        public static bool CheckLabel(string regName) {
            try {
                (string block, string subBlock, string register) = ParseRegisterPath(regName);
                IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

                if (block != null && subBlock != null) {
                    return registerMap.Blocks[block].SubBlocks[subBlock].Registers.Contains(register);
                } else if (block != null) {
                    return registerMap.Blocks[block].Registers.Contains(register);
                } else {
                    return registerMap.Registers.Contains(register);
                }
            } catch {
                return false;
            }
        }

        /// <summary>
        /// Verify is the field label is part of register map
        /// </summary>
        /// <param name="regName">Register name</param>
        /// <param name="fldName">Field name</param>
        /// <returns></returns>
        public static bool CheckFieldLabel(string regName, string fldName) {
            try {
                (string block, string subBlock, string register) = ParseRegisterPath(regName);
                IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

                if (block != null && subBlock != null) {
                    return registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields.Contains(fldName);
                } else if (block != null) {
                    return registerMap.Blocks[block].Registers[register].Fields.Contains(fldName);
                } else {
                    return registerMap.Registers[register].Fields.Contains(fldName);
                }
            } catch {
                return false;
            }
        }


        /// <summary>
        /// Restore all Registers to default values
        /// </summary>
        public static void ResetRegisterAll() {
            PortBridge.RegisterMaps.RestoreAllDefaultValues();
        }

        /// <summary>
        /// Restore specific Register to default value
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        public static void ResetRegister(string regName) {
            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].RestoreDefaultValue();
            } else if (block != null) {
                registerMap.Blocks[block].Registers[register].RestoreDefaultValue();
            } else {
                registerMap.Registers[register].RestoreDefaultValue();
            }
        }

        /// <summary>
        /// Set Register Value as SiteReg32 - just use for reference
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="regValue">Register Value</param>
        public static void SetSiteReg32(string regName, SiteReg32 regValue) {
            IPBSiteLong regValuePB;
            SiteLong regValueSL;

            regValueSL = regValue.ToSiteLong();
            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValuePB.SiteVariable = regValueSL;
        }

        /// <summary>
        /// Set Register Value as SiteGeneric
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="regValue">Register Value</param>
        public static void SetRegValue(string regName, Site<int> regValue) {
            IPBSiteLong regValuePB;
            SiteLong regValueSL;

            regValueSL = regValue.ToSiteLong();
            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValuePB.SiteVariable = regValueSL;
        }

        /// <summary>
        /// Set Register Value as SiteLong
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="regValue">Register Value</param>
        public static void SetRegValuePerSite(string regName, SiteLong regValue) {
            IPBSiteLong regValuePB;

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValuePB.SiteVariable = regValue;
        }
        /// <summary>
        /// Get Register Value as SiteReg32 - just use for reference
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <returns>Register Value as SiteGeneric</returns>
        public static SiteReg32 GetSiteReg32(string regName) {
            IPBSiteLong regValuePB;
            SiteLong regValueSL = new SiteLong();
            SiteReg32 regValue = new SiteReg32();
            Site<int> regValueSG;

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValueSL[-1] = regValuePB.SiteVariable;
            regValueSG = regValueSL.ToSite();
            ForEachSite(site => {
                regValue.Fill((uint)regValueSG[site], site);
            });
            return regValue;
        }

        /// <summary>
        /// Get Register Value as SiteGeneric
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <returns>Register Value as SiteGeneric</returns>
        public static Site<int> GetRegValue(string regName) {
            IPBSiteLong regValuePB;
            SiteLong regValueSL = new SiteLong();
            Site<int> regValueSG;

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValueSL[-1] = regValuePB.SiteVariable;
            regValueSG = regValueSL.ToSite();

            return regValueSG;
        }


        /// <summary>
        /// Get Register Value as SiteLong
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <returns>Register Value as SiteLong</returns>
        public static SiteLong GetRegValuePerSite(string regName) {
            IPBSiteLong regValuePB;
            SiteLong regValueSL = new SiteLong();

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                regValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Value;
            } else if (block != null) {
                regValuePB = registerMap.Blocks[block].Registers[register].Value;
            } else {
                regValuePB = registerMap.Registers[register].Value;
            }
            regValueSL[-1] = regValuePB.SiteVariable;
            return regValueSL;
        }


        /// <summary>
        /// Set Register Field Value as SiteGeneric
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="fldName">Field Name</param>
        /// <param name="fldValue">Field Value</param>
        public static void SetFldValue(string regName, string fldName, Site<int> fldValue) {
            IPBSiteLong fldValuePB;
            SiteLong fldValueSL;

            fldValueSL = fldValue.ToSiteLong();
            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                fldValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields[fldName].Value;
            } else if (block != null) {
                fldValuePB = registerMap.Blocks[block].Registers[register].Fields[fldName].Value;
            } else {
                fldValuePB = registerMap.Registers[register].Fields[fldName].Value;
            }
            fldValuePB.SiteVariable = fldValueSL;
        }

        /// <summary>
        /// Set Register Field Value as SiteLong
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="fldName">Field Name</param>
        /// <param name="fldValue">Field Value</param>
        public static void SetFldValuePerSite(string regName, string fldName, SiteLong fldValue) {
            IPBSiteLong fldValuePB;

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                fldValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields[fldName].Value;
            } else if (block != null) {
                fldValuePB = registerMap.Blocks[block].Registers[register].Fields[fldName].Value;
            } else {
                fldValuePB = registerMap.Registers[register].Fields[fldName].Value;
            }
            fldValuePB.SiteVariable = fldValue;

        }

        /// <summary>
        /// Retrun field mask
        /// </summary>
        /// <param name="regName">Register name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="fldName">Field name</param>
        /// <returns></returns>
        public static long GetFieldMask(string regName, string fldName) {
            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                return registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields[fldName].Mask;
            } else if (block != null) {
                return registerMap.Blocks[block].Registers[register].Fields[fldName].Mask;
            } else {
                return registerMap.Registers[register].Fields[fldName].Mask;
            }
        }

        /// <summary>
        /// Get Register Field Value as SiteGeneric
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="fldName">Field Name</param>
        /// <returns>Register Field Value as SiteGeneric</returns>
        public static Site<int> GetFldValue(string regName, string fldName) {
            IPBSiteLong fldValuePB;
            SiteLong fldValueSL = new SiteLong();
            Site<int> fldValueSG;

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                fldValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields[fldName].Value;
            } else if (block != null) {
                fldValuePB = registerMap.Blocks[block].Registers[register].Fields[fldName].Value;
            } else {
                fldValuePB = registerMap.Registers[register].Fields[fldName].Value;
            }
            fldValueSL[-1] = fldValuePB.SiteVariable;
            fldValueSG = fldValueSL.ToSite();
            return fldValueSG;
        }

        /// <summary>
        /// Get Register Field Value as SiteLong
        /// </summary>
        /// <param name="regName">Register Name (supports hierarchical paths like 'Block.SubBlock.Register')</param>
        /// <param name="fldName">Field Name</param>
        /// <returns>Register Field Value as SiteLong</returns>
        public static SiteLong GetFldValuePerSite(string regName, string fldName) {
            IPBSiteLong fldValuePB;
            SiteLong fldValueSL = new SiteLong();

            (string block, string subBlock, string register) = ParseRegisterPath(regName);
            IPortBridgeRegisterMap registerMap = PortBridge.RegisterMaps[REGISTER_MAP];

            if (block != null && subBlock != null) {
                fldValuePB = registerMap.Blocks[block].SubBlocks[subBlock].Registers[register].Fields[fldName].Value;
            } else if (block != null) {
                fldValuePB = registerMap.Blocks[block].Registers[register].Fields[fldName].Value;
            } else {
                fldValuePB = registerMap.Registers[register].Fields[fldName].Value;
            }
            fldValueSL[-1] = fldValuePB.SiteVariable;
            return fldValueSL;
        }


    }
}
