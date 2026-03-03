using Teradyne.Igxl.Interfaces.Public;

#if PORTBRIDGE_ENABLED
using Teradyne.PortBridge;

namespace Csra.Interfaces {
    internal interface IPortBridgeAdapter {
        
        SiteLong GetField(string registerMapName, string register, string field);
        void SetField(string registerMapName, string register, string field, SiteLong value);
        SiteLong GetRegister(string registerMapName, string register);
        void SetRegister(string registerMapName, string register, SiteLong value);
    }
}
#endif
