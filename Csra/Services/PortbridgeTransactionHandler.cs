using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;

#if PORTBRIDGE_ENABLED
using Teradyne.PortBridge;
#endif

namespace Csra.Services {

#if PORTBRIDGE_ENABLED
    internal class PortbridgeTransactionHandler : TransactionHandler {

        [DoNotSync]
        private readonly IPortBridgeAdapter _injectedPortBridge;

        [DoNotSync]
        private IPortBridgeAdapter _portBridge;

        private PortbridgeTransactionConfig _portBridgeConfig;
        // Store map name independently so UT can set it without needing PortbridgeTransactionConfig
        private string _registerMapName = string.Empty;

        public PortbridgeTransactionHandler() : base() { }

        internal PortbridgeTransactionHandler(IPortBridgeAdapter portBridgeAdapter) : base() {
            _injectedPortBridge = portBridgeAdapter;
        }

        internal override bool Configure(ITransactionConfig transactionConfig) {
            if (transactionConfig == null) {
                Api.Services.Alert.Error<ArgumentNullException>("Received TransactionConfig is null!");
            }
            if (transactionConfig.GetType() != typeof(PortbridgeTransactionConfig)) {
                Api.Services.Alert.Error<ArgumentException>(
                    $"Expected PortbridgeTransactionConfig but received {transactionConfig.GetType().Name}");
            }

            _portBridgeConfig = (PortbridgeTransactionConfig)transactionConfig;
            _registerMapName = _portBridgeConfig.RegisterMapName;

            _portBridge = _injectedPortBridge ?? new TeradynePortBridgeAdapter(_portBridgeConfig.PortBridge);

            // ToDo transactionConfig.Valid;
            return true;
        }
        // UT-only config path (no Teradyne types required)
        internal bool ConfigureForTest(string registerMapName) {
            _registerMapName = registerMapName ?? string.Empty;

            if (_injectedPortBridge is null) {
                throw new InvalidOperationException(
                    "ConfigureForTest requires constructing PortbridgeTransactionHandler with an injected IPortBridgeAdapter.");
            }

            _portBridge = _injectedPortBridge;
            return true;
        }

        internal override Site<T> GetField<T>(string register, string field, string port = "") {
            if (typeof(T) != typeof(int))
                Api.Services.Alert.Error<NotSupportedException>($"Portbridge only supports int but received {typeof(T).Name}");
            SiteLong fieldValueSL = new SiteLong();
            fieldValueSL[-1] = _portBridge.GetField(_registerMapName, register, field);
            return fieldValueSL.ToSite() as Site<T>;
        }

        internal override void SetField<T>(string register, string field, T data, string port = "") {
            Site<int> siteData = null;
            if (typeof(T) == typeof(Site<int>)) {
                siteData = data as Site<int>;
            } else if (typeof(T) == typeof(int)) {
                siteData = new Site<int>();
                siteData.Fill((int)(object)data);
            } else {
                Api.Services.Alert.Error<NotSupportedException>($"Portbridge only supports Site<int> or int but received {typeof(T).Name}");
            }
            _portBridge.SetField(_registerMapName, register, field, siteData.ToSiteLong());
        }

        internal override void SetFieldPerSite<T>(string register, string field, Site<T> data, string port = "") => throw new NotImplementedException();

        internal override Site<T> GetRegister<T>(string register, string port = "") {
            if (typeof(T) != typeof(int))
                Api.Services.Alert.Error<NotSupportedException>($"Portbridge only supports int but received {typeof(T).Name}");
            SiteLong regValueSL = new SiteLong();
            regValueSL[-1] = _portBridge.GetRegister(_registerMapName, register);
            return regValueSL.ToSite() as Site<T>;
        }

        internal override void SetRegister<T>(string register, T data, string port = "") {
            Site<int> siteData = new Site<int>();
            if (typeof(T) == typeof(Site<int>)) {
                siteData = data as Site<int>;
            } else if (typeof(T) == typeof(int)) {
                siteData.Fill((int)(object)data);
            } else {
                Api.Services.Alert.Error<NotSupportedException>($"Portbridge only supports Site<int> or int but received {typeof(T).Name}");
            }
            _portBridge.SetRegister(_registerMapName, register, siteData.ToSiteLong());
        }

        internal override void SetRegisterPerSite<T>(string register, Site<T> data, string port = "") => throw new NotImplementedException();

        internal override void PullRegister(string register, string port = "") => throw new NotImplementedException();

        internal override void PushRegister(string register, string port = "") => throw new NotImplementedException();

        internal override Site<T> ReadRegister<T>(string register, string port = "") => throw new NotImplementedException();

        internal override Site<bool> ExpectRegister<T>(string register, T data, string port = "") => throw new NotImplementedException();

        internal override Site<bool> ExpectRegisterPerSite<T>(string register, Site<T> data, string port = "") => throw new NotImplementedException();

        internal override void WriteRegister<T>(string register, T data, string port = "") => throw new NotImplementedException();

        internal override void WriteRegisterPerSite<T>(string register, Site<T> data, string port = "") => throw new NotImplementedException();

        internal override void Execute(string module, string port = "") => throw new NotImplementedException();

        internal override List<Site<T>> ExecuteRead<T>(string module, int readCount, string port = "") => throw new NotImplementedException();

        internal override void ReInitRegister(string register, string port = "") => throw new NotImplementedException();

        internal override void ReInitAll() => throw new NotImplementedException();
    }
#endif
}
