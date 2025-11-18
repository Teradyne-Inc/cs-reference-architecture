using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Csra.Interfaces;

#if PORTBRIDGE_ENABLED
using Teradyne.PortBridge;
#endif

namespace Csra {
#if PORTBRIDGE_ENABLED
    internal class PortbridgeTransactionConfig : ITransactionConfig {

        private static PortBridgeLanguage_Remote _portBridge = new PortBridgeLanguage_Remote("IG.NET");

        public bool Valid {
            get {
                // ToDo: Implement validation logic
                return false;
            }
        }

        public void LoadRegisterMap(string name, string registerMapPath, PortBridgeRegisterMapFormat format, string customParserPluginPath) {
            _portBridge.RegisterMaps.Load(name, registerMapPath, format, customParserPluginPath);
        }

        public bool LoadDefinition(string definitionName, string definitionPath, string definitionFormat, string configurationName,
            string definitionParserPath) {
            _portBridge.Tests.AddDefinition(definitionName, definitionPath, configurationName, true, definitionParserPath);
            return false;
        }

        public bool AddPin(string pin, string atePin = "", string type = "", string defaultState = "", string initState = "") {
            return false;
        }

        public bool SetCachePath(string cachePath) {
            return false;
        }

    }
#endif
}
