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

        static PortBridgeLanguage_Remote _portBridge = null;

        public PortbridgeTransactionHandler() : base() { }

        internal override bool Configure(ITransactionConfig transactionConfig) {
            if (transactionConfig.GetType() != typeof(PortbridgeTransactionConfig))
                throw new ArgumentException($"Expected PortbridgeTransactionConfig but received {transactionConfig.GetType().Name}", nameof(transactionConfig));
            // ToDo: Add actual configuration logic for PortBridge
            return transactionConfig.Valid;
        }

        internal override Site<T> GetField<T>(string register, string field, string port = "") => throw new NotImplementedException();

        internal override void SetField<T>(string register, string field, T data, string port = "") => throw new NotImplementedException();

        internal override void SetFieldPerSite<T>(string register, string field, Site<T> data, string port = "") => throw new NotImplementedException();

        internal override Site<T> GetRegister<T>(string register, string port = "") => throw new NotImplementedException();

        internal override void SetRegister<T>(string register, T data, string port = "") => throw new NotImplementedException();

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
    }
#endif
}
