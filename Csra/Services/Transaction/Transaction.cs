using System;
using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;

namespace Csra.Services {

    /// <summary>
    /// Services.Transaction.- being fluent in devices' dialects
    /// </summary>
    [Serializable]
    public class Transaction : ITransactionService {

        private static ITransactionService _instance = null;
        private TransactionHandler _transactionHandler;
        private Dictionary<string, TransactionHandler> _handlerMap;
        private Dictionary<string, Pins.Pin> _pins;

        protected Transaction() {
            _handlerMap = new Dictionary<string, TransactionHandler>();
            _pins = new();
        }

        public static ITransactionService Instance => _instance ??= new Transaction();

        public Site<T> GetField<T>(string register, string field, string port = "") => _transactionHandler.GetField<T>(register, field, port);

        public void SetField<T>(string register, string field, T data, string port = "") =>
            _transactionHandler.SetField(register, field, data, port);

        public void SetFieldPerSite<T>(string register, string field, Site<T> data, string port = "") =>
            _transactionHandler.SetFieldPerSite(register, field, data, port);

        public Site<T> GetRegister<T>(string register, string port = "") => _transactionHandler.GetRegister<T>(register, port);

        public void SetRegister<T>(string register, T data, string port = "") => _transactionHandler.SetRegister(register, data, port);

        public void SetRegisterPerSite<T>(string register, Site<T> data, string port = "") =>
            _transactionHandler.SetRegisterPerSite(register, data, port);

        public void PullRegister(string register, string port = "") => _transactionHandler.PullRegister(register, port);

        public void PushRegister(string register, string port = "") => _transactionHandler.PushRegister(register, port);

        public Site<T> ReadRegister<T>(string register, string port = "") => _transactionHandler.ReadRegister<T>(register, port);

        public Site<bool> ExpectRegister<T>(string register, T data, string port = "") => _transactionHandler.ExpectRegister(register, data, port);

        public Site<bool> ExpectRegisterPerSite<T>(string register, Site<T> data, string port = "") =>
            _transactionHandler.ExpectRegisterPerSite(register, data, port);

        public void WriteRegister<T>(string register, T data, string port = "") => _transactionHandler.WriteRegister(register, data, port);

        public void WriteRegisterPerSite<T>(string register, Site<T> data, string port = "") =>
            _transactionHandler.WriteRegisterPerSite(register, data, port);

        public void Execute(string module, string port = "") => _transactionHandler.Execute(module, port);

        public List<Site<T>> ExecuteRead<T>(string module, int readCount, string port = "") =>
            _transactionHandler.ExecuteRead<T>(module, readCount, port);

        public void ReInitRegister(string register, string port = "") => _transactionHandler.ReInitRegister(register, port);

        // Verify below if still required when using setupobjects
        // TODO: Implement shadow register reset logic https://github.com/TER-SEMITEST-InnerSource/cs-reference-architecture/issues/1455

        public void ReInitAll() => _transactionHandler.ReInitAll();

        public void Reset() {
            _transactionHandler = null;
            _handlerMap.Clear();
            _pins.Clear();
        }

        public void Init(TransactionType transactionType) {
            switch (transactionType) {
                case TransactionType.PortBridge:
#if PORTBRIDGE_ENABLED
                    if (!_handlerMap.ContainsKey(transactionType.ToString()))
                        _handlerMap.Add(transactionType.ToString(), new PortbridgeTransactionHandler());
                    _transactionHandler = _handlerMap[transactionType.ToString()];
                    break;
#else
                    throw new NotSupportedException("PortBridge transaction type is not supported. Please enable PortBridge in the project settings.");
#endif
                case TransactionType.CsraGeneric:
                    if (!_handlerMap.ContainsKey(transactionType.ToString()))
                        _handlerMap.Add(transactionType.ToString(), new CsraTransactionHandler());
                    _transactionHandler = _handlerMap[transactionType.ToString()];
                    break;
                default:
                    throw new NotSupportedException($"Transaction type {transactionType} is not supported.");
            }
        }

        public bool ConfigureHandler(ITransactionConfig transactionConfig) {
            if (_transactionHandler == null) {
                throw new InvalidOperationException("Transaction handler is not initialized. Please call Initttt() before configuring the handler.");
            }
            return _transactionHandler.Configure(transactionConfig);
        }

        public void RemoveHandler(TransactionType transactionType) {
            if (_handlerMap.ContainsKey(transactionType.ToString())) {
                if (_transactionHandler == _handlerMap[transactionType.ToString()]) {
                    _transactionHandler = null;
                }
                _handlerMap.Remove(transactionType.ToString());
            }
        }

        internal void SetHandlerForTest(TransactionHandler handler) {
            _transactionHandler = handler;
        }
    }
}
