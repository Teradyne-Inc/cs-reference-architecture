using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;
using static Teradyne.Igxl.Interfaces.Public.TestCodeBase;
using Teradyne.Igxl.Interfaces.Public;


#if PORTBRIDGE_ENABLED
using Teradyne.PortBridge;
#endif

namespace Csra {
#if PORTBRIDGE_ENABLED
    public class PortbridgeTransactionConfig : ITransactionConfig {

        [DoNotSync]
        private static PortBridgeLanguage_Remote _portBridge;
        [DoNotSync]
        private IPortBridgeTestConfiguration _baseConfig;
        private int configErrors = 0;
        private const string _baseConfigName = "SWD1";
        private string _baseConfigRegisterMapName = "";

        private enum ConfigErrorType {
            None = 0,
            InvalidRegisterMapFormat = 1,
            RegisterMapLoadFailed = 2,
            DefinitionLoadFailed = 4,
            PinAddFailed = 8,
            CachePathSetFailed = 16
        }

        public PortbridgeTransactionConfig(PortBridgeLanguage_Remote portBridge) {
            _portBridge = portBridge;
            _portBridge.RegisterMaps.UnloadAll();
            _baseConfig = _portBridge.Tests.Configurations[_baseConfigName];
            _baseConfig.Digital.Options["-pinmap_workbook"] = TheProgram.PathAndName;
        }

        public PortBridgeLanguage_Remote PortBridge {
            get {
                return _portBridge;
            }
        }

        public int ConfigErrors {
            get {
                return configErrors;
            }
        }

        public bool Valid {
            get {
                // ToDo: Check for complete configuration
                return configErrors == 0;
            }
        }

        public string RegisterMapName {
            get {
                return _baseConfigRegisterMapName;
            }
        }

        public void SetDefaultTimeset(string timesetName) {
            _baseConfig.Digital.TimeSets.SetDefaultTimeset(timesetName);
        }

        public void LoadRegisterMap(string name, string registerMapPath, PortBridgeRegisterMapFormat format, string customParserPluginPath) {
            _portBridge.RegisterMaps.Load(name, registerMapPath, format, customParserPluginPath);
            // Add the loaded register map to the test configuration automatically. ToDo: verify if 
            _baseConfig.RegisterMap.Add(name);
            _baseConfigRegisterMapName = name;
        }

        public bool LoadDefinition(string definitionName, string definitionPath, string definitionFormat, string configurationName,
            string definitionParserPath) {
            _portBridge.Tests.AddDefinition(definitionName, definitionPath, configurationName, true, definitionParserPath);
            return false;
        }

        public bool AddDigitalPin(string pin, string atePin = "", PortBridgeDigitalConnectionState state = PortBridgeDigitalConnectionState.Input_Output, string defaultState = "X", string initState = "") {
            _baseConfig.Digital.Pins.Add(pin, atePin, state, defaultState, initState);
            return true;
        }

        public bool AddAnalogPin(string pin, string atePin = "", string instrumentType = "") {
            _baseConfig.Instruments.Add(pin, atePin, instrumentType);
            return true;
        }

        public bool SetCachePath(string cachePath) {
            if (System.IO.Directory.Exists(cachePath)) {
                _baseConfig.Digital.Cache.Add(cachePath);
                return true;
            }
            configErrors |= (int)ConfigErrorType.CachePathSetFailed;
            return false;
        }

        public void SetSheet(string sheetType, string sheetName) {
            _baseConfig.Digital.Options[sheetType] = sheetName;
        }

        public void SetTestDefinition(string testDefinitionName, string definitionFilePath, bool defaultDefinition, string customParserAssemblyPath) {
            _portBridge.Tests.AddDefinition(testDefinitionName, definitionFilePath, _baseConfigName, defaultDefinition, customParserAssemblyPath);
        }

        public bool AddRegister(string registerName, long defaultValue, long registerAddress, int bitCount = 32) {
            if (_baseConfigRegisterMapName == string.Empty) {
                return false;
            }
            // ToDo: Add support for bit order parameter
            _portBridge.RegisterMaps[_baseConfigRegisterMapName].Registers.Add(registerName, defaultValue, registerAddress, bitCount,
                PortBridgeBitOrder.LSBFirst);
            return true;
        }

        public bool AddField(string registerName, string fieldName, long fieldMask, long defaultValue) {
            if (_baseConfigRegisterMapName == string.Empty) {
                return false;
            }
            _portBridge.RegisterMaps[_baseConfigRegisterMapName].Registers[registerName].Fields.Add(fieldName, fieldMask, defaultValue);
            return true;
        }

        public void LoadFilesFromDirectory(string directoryPath, string filter = null, string definitionName = "*", bool reloadIfAlreadyLoaded = true,
            bool removeCommentAndWhitespaceLine = true, string customParserForCommandComments = "Basic", bool readEvenWhenExpectIsNotSpecified = true,
            string includeFileAliasCSL = null, bool searchSubDirectories = false, string appendPrefix = "") {
            _portBridge.Tests.LoadFilesFromDirectory(directoryPath, filter, definitionName, reloadIfAlreadyLoaded, removeCommentAndWhitespaceLine,
                customParserForCommandComments, readEvenWhenExpectIsNotSpecified, includeFileAliasCSL, searchSubDirectories, appendPrefix);
        }
    }
#endif
}
