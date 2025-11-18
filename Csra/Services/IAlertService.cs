using System;
using System.Runtime.CompilerServices;
using Teradyne.Igxl.Interfaces.Public;

namespace Csra.Interfaces {

    public interface IAlertService : IService {

        /// <summary>
        /// Reads or sets the output target for Log Alerts. Defaults to OutputWindow, which cannot be disabled.
        /// </summary>
        public AlertOutputTarget LogTarget { get; set; }

        /// <summary>
        /// Reads or sets the output target for Info Alerts. Defaults to OutputWindow, which cannot be disabled.
        /// </summary>
        public AlertOutputTarget InfoTarget { get; set; }

        /// <summary>
        /// Reads or sets the output target for Warning Alerts. Defaults to OutputWindow and Datalog, which cannot be disabled.
        /// </summary>
        public AlertOutputTarget WarningTarget { get; set; }

        /// <summary>
        /// Reads or sets the output target for Error Alerts. Defaults to OutputWindow and Datalog, which cannot be disabled.
        /// </summary>
        public AlertOutputTarget ErrorTarget { get; set; }

        /// <summary>
        /// Reads or sets the file path for the output file.
        /// </summary>
        public string OutputFile { get; set; }

        /// <summary>
        /// Reads or sets whether a time stamp is added to Info / Warning and Error Alerts.
        /// </summary>
        public bool TimeStamp { get; set; }

        /// <summary>
        /// Sends a Log Alert message to the selected output target(s).
        /// </summary>
        /// <param name="message">The message string to be logged.</param>
        /// <param name="color">Optional. The color to be used (OutputWindow only, ignored elsewhere).</param>
        /// <param name="bold">Optional. Whether bold font is used (OutputWindow only, ignored elsewhere).</param>
        public void Log(string message, ColorConstants color = ColorConstants.Black, bool bold = false);

        /// <summary>
        /// Sends a Log Alert message to the selected output target(s).
        /// </summary>
        /// <param name="message">The message string to be logged.</param>
        /// <param name="red">The red component of a RGB color.</param>
        /// <param name="green">The green component of a RGB color.</param>
        /// <param name="blue">The blue component of a RGB color.</param>
        /// <param name="bold">Optional. Whether bold font is used (OutputWindow only, ignored elsewhere).</param>
        public void Log(string message, byte red, byte green, byte blue, bool bold = false);

        /// <summary>
        /// Sends an Info Alert message to the selected output target(s). Use for positive / neutral information relevant to the user.
        /// </summary>
        /// <param name="info">The Info Alert message.</param>
        /// <param name="doNotSpecify">DO NOT SPECIFY - the name of the calling method is automatically inserted by the compiler.</param>
        public void Info(string info, [CallerMemberName] string doNotSpecify = "");

        /// <summary>
        /// Sends a Warning Alert message to the selected output target(s). Use for recoverable issues that may require attention.
        /// </summary>
        /// <param name="warning">The Warning Alert message.</param>
        /// <param name="doNotSpecify">DO NOT SPECIFY - the name of the calling method is automatically inserted by the compiler.</param>
        public void Warning(string warning, [CallerMemberName] string doNotSpecify = "");

        /// <summary>
        /// Sends an Error Alert message to the selected output target(s). At test program runtime, this raises an exception to the IG-XL error handler. If
        /// called during validation, it fails validation and flags the error appropriately. Use this for non-recoverable conditions that require immediate and
        /// safe termination. Note: The compiler does not recognize that this method never returns, so you may need extra checks to satisfy .NET Framework
        /// requirements.
        /// </summary>
        /// <param name="error">The Error Alert message.</param>
        /// <param name="validationArgumentIndex">Optional. The offending test instance argument index (one-based), if applicable when used in validation.</param>
        /// <param name="doNotSpecify">DO NOT SPECIFY - the name of the calling method is automatically inserted by the compiler.</param>
        public void Error(string error, int validationArgumentIndex = 0, [CallerMemberName] string doNotSpecify = "");


        /// <summary>
        /// Sends an Error Alert message to the selected output target(s). At test program runtime, this raises a user-selectable exception to the IG-XL error
        /// handler. If called during validation, it fails validation and flags the error appropriately. Use this for non-recoverable conditions that require
        /// immediate and safe termination. Note: The compiler does not recognize that this method never returns, so you may need extra checks to satisfy .NET
        /// Framework requirements.
        /// </summary>
        /// <typeparam name="TException">The type of the exception to be thrown.</typeparam>
        /// <param name="error">The Error Alert message.</param>
        /// <param name="validationArgumentIndex">Optional. The offending test instance argument index (one-based), if applicable when used in validation.</param>
        /// <param name="doNotSpecify">DO NOT SPECIFY - the name of the calling method is automatically inserted by the compiler.</param>
        public void Error<TException>(string error, int validationArgumentIndex = 0, [CallerMemberName] string doNotSpecify = "") where TException : Exception;
    }
}
