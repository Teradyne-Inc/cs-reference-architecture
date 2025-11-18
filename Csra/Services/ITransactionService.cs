using System.Collections.Generic;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra.Interfaces {

    public interface ITransactionService : IService {

        /// <summary>
        /// Retrieves a field from the shadow register.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>The value of the field.</returns>
        Site<T> GetField<T>(string register, string field, string port = "");

        /// <summary>
        /// Sets a field in the shadow register (site-uniform).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="data">The data to be set.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void SetField<T>(string register, string field, T data, string port = "");

        /// <summary>
        /// Sets a field in the shadow register (site-specific).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="field">The name of the field.</param>
        /// <param name="data">The data to be set.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void SetFieldPerSite<T>(string register, string field, Site<T> data, string port = "");

        /// <summary>
        /// Retrieves a register from the shadow register.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>The content of the register.</returns>
        Site<T> GetRegister<T>(string register, string port = "");

        /// <summary>
        /// Sets a register in the shadow register (site-uniform).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be set.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void SetRegister<T>(string register, T data, string port = "");

        /// <summary>
        /// Sets a register in the shadow register (site-specific).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be set.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void SetRegisterPerSite<T>(string register, Site<T> data, string port = "");

        /// <summary>
        /// Pulls a register from the DUT to the shadow register.
        /// </summary>
        /// <param name="register">The name of the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void PullRegister(string register, string port = "");

        /// <summary>
        /// Pushes a register from the shadow register to the DUT.
        /// </summary>
        /// <param name="register">The name of the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void PushRegister(string register, string port = "");

        /// <summary>
        /// Reads a register from the DUT.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>The content of the register read from the DUT.</returns>
        Site<T> ReadRegister<T>(string register, string port = "");

        /// <summary>
        /// Reads a register from the DUT and checks if the content matches the expected data.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be compared to the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>True if the register content matches the data.</returns>
        Site<bool> ExpectRegister<T>(string register, T data, string port = "");

        /// <summary>
        /// Checks if the register matches the expected data.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be compared to the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>True if the register content matches the data.</returns>
        Site<bool> ExpectRegisterPerSite<T>(string register, Site<T> data, string port = "");

        /// <summary>
        /// Writes data to a register in the DUT (site-uniform).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be written to the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void WriteRegister<T>(string register, T data, string port = "");

        /// <summary>
        /// Writes data to a register in the DUT (site-specific).
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="register">The name of the register.</param>
        /// <param name="data">The data to be written to the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void WriteRegisterPerSite<T>(string register, Site<T> data, string port = "");

        /// <summary>
        /// Execute a transaction module, pushing data from the shadow register to the DUT (without reads).
        /// </summary>
        /// <param name="module">The name of the transaction module.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void Execute(string module, string port = "");

        /// <summary>
        /// Executes a transaction file, pushing data from the shadow register to the DUT and reading data from
        /// the DUT to the code.
        /// </summary>
        /// <typeparam name="T">The type defined for this port.</typeparam>
        /// <param name="module">The name of the module.</param>
        /// <param name="readCount">The number of results to read after execution.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        /// <returns>A list of values read from the DUT during execution.</returns>
        List<Site<T>> ExecuteRead<T>(string module, int readCount, string port = "");

        /// <summary>
        /// Sets a single shadow register to it's default / init value.
        /// </summary>
        /// <param name="register">The name of the register.</param>
        /// <param name="port">Optional. The name of the port to use for transaction, if empty default or first port is used.</param>
        void ReInitRegister(string register, string port = "");

        /// <summary>
        /// Initializes the transaction service for a specific transaction type. This must be called before any other transaction methods are used.
        /// Multiple transaction types can be initialized, but only one can be active at a time.
        /// To switch the active transaction type, call Init again with the desired type.
        /// </summary>
        /// <param name="transactionType"></param>
        void Init(TransactionType transactionType);

        /// <summary>
        /// Configures the handler for the current transaction type.
        /// </summary>
        /// <param name="config"></param>
        bool ConfigureHandler(ITransactionConfig config);

        /// <summary>
        /// Removes the handler for the specified transaction type.
        /// </summary>
        /// <param name="transactionType"></param>
        void RemoveHandler(TransactionType transactionType);
    }
}
