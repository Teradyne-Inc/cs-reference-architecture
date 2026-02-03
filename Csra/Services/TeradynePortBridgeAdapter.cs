using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;


#if PORTBRIDGE_ENABLED
using Teradyne.PortBridge;

namespace Csra.Services {
    internal sealed class TeradynePortBridgeAdapter : IPortBridgeAdapter {
        private readonly PortBridgeLanguage_Remote _portBridge;

        public TeradynePortBridgeAdapter(PortBridgeLanguage_Remote portBridge) {
            if (portBridge is null) {
                Api.Services.Alert.Error<ArgumentNullException>("PortBridge object is null");
            }
            _portBridge = portBridge;
        }
        public SiteLong GetField(string registerMapName, string register, string field) =>
       (SiteLong)_portBridge.RegisterMaps[registerMapName].Registers[register].Fields[field].Value.SiteVariable;

        public void SetField(string registerMapName, string register, string field, SiteLong value) =>
            _portBridge.RegisterMaps[registerMapName].Registers[register].Fields[field].Value.SiteVariable = value;
        public SiteLong GetRegister(string registerMapName, string register) =>
            (SiteLong)_portBridge.RegisterMaps[registerMapName].Registers[register].Value.SiteVariable;

        public void SetRegister(string registerMapName, string register, SiteLong value) =>
            _portBridge.RegisterMaps[registerMapName].Registers[register].Value.SiteVariable = value;   
    }
}
#endif
