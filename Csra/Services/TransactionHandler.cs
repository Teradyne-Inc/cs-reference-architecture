using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;
using Csra.Interfaces;
using System;

namespace Csra.Services {

    [Serializable]
    internal abstract class TransactionHandler {

        internal TransactionHandler() { }
         
        internal abstract bool Configure(ITransactionConfig transactionConfig);

        internal abstract Site<T> GetField<T>(string register, string field, string port = "");

        internal abstract void SetField<T>(string register, string field, T data, string port = "");

        internal abstract void SetFieldPerSite<T>(string register, string field, Site<T> data, string port = "");

        internal abstract Site<T> GetRegister<T>(string register, string port = "");

        internal abstract void SetRegister<T>(string register, T data, string port = "");

        internal abstract void SetRegisterPerSite<T>(string register, Site<T> data, string port = "");

        internal abstract void PullRegister(string register, string port = "");

        internal abstract void PushRegister(string register, string port = "");

        internal abstract Site<T> ReadRegister<T>(string register, string port = "");

        internal abstract Site<bool> ExpectRegister<T>(string register, T data, string port = "");

        internal abstract Site<bool> ExpectRegisterPerSite<T>(string register, Site<T> data, string port = "");

        internal abstract void WriteRegister<T>(string register, T data, string port = "");

        internal abstract void WriteRegisterPerSite<T>(string register, Site<T> data, string port = "");

        internal abstract void Execute(string module, string port = "");

        internal abstract List<Site<T>> ExecuteRead<T>(string module, int readCount, string port = "");

        internal abstract void ReInitRegister(string register, string port = "");

        internal abstract void ReInitAll();
    }
}
